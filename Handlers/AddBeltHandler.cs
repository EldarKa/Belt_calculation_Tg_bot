using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Belt_calculation_Tg_bot.Data;
using Belt_calculation_Tg_bot.Handlers.Base;
using Belt_calculation_Tg_bot.Models;
using Belt_calculation_Tg_bot.Models.Enums;
using Belt_calculation_Tg_bot.Models.State;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace Belt_calculation_Tg_bot.Handlers
{
    public class AddBeltHandler : ICommandHandler
    {
        private readonly Database _database;

        public AddBeltHandler(Database database)
        {
            _database = database;
        }
        public bool CanHandle(UserState state, string message)
        {
            return message == "/add_Belt" || state.AddBeltState != AddBeltState.None;
        }

        public async Task HandleAsync(ITelegramBotClient bot, long chatId, string message, UserState state, CancellationToken cancellationToken)
        {
            switch (state.AddBeltState)
            {
                case AddBeltState.None:
                    state.TempBelt = new Belt();
                    state.AddBeltState = AddBeltState.AwaitingName;
                    await bot.SendMessage(chatId, "Введите название ремня:", cancellationToken: cancellationToken);
                    break;

                case AddBeltState.AwaitingName:
                    state.TempBelt.Name = message;
                    state.AddBeltState = AddBeltState.AwaitingWeight;
                    await bot.SendMessage(chatId, "Введите вес ремня (число):", cancellationToken: cancellationToken);
                    break;

                case AddBeltState.AwaitingWeight:
                    if (TryParseDouble(message, out double weight))
                    {
                        state.TempBelt.Weight = weight;
                        state.AddBeltState = AddBeltState.AwaitingK1;
                        await bot.SendMessage(chatId, "Введите коэффициент K1:", cancellationToken: cancellationToken);
                    }
                    else await InvalidNumber(bot, chatId, cancellationToken);
                    break;

                case AddBeltState.AwaitingK1:
                    if (TryParseDouble(message, out double k1))
                    {
                        state.TempBelt.K1 = k1;
                        state.AddBeltState = AddBeltState.AwaitingK2;
                        await bot.SendMessage(chatId, "Введите коэффициент K2:", cancellationToken: cancellationToken);
                    }
                    else await InvalidNumber(bot, chatId, cancellationToken);
                    break;

                case AddBeltState.AwaitingK2:
                    if (TryParseDouble(message, out double k2))
                    {
                        state.TempBelt.K2 = k2;
                        state.AddBeltState = AddBeltState.AwaitingK3;
                        await bot.SendMessage(chatId, "Введите коэффициент K3:", cancellationToken: cancellationToken);
                    }
                    else await InvalidNumber(bot, chatId, cancellationToken);
                    break;

                case AddBeltState.AwaitingK3:
                    if (TryParseDouble(message, out double k3))
                    {
                        state.TempBelt.K3 = k3;
                        state.AddBeltState = AddBeltState.AwaitingK4;
                        await bot.SendMessage(chatId, "Введите коэффициент K4:", cancellationToken: cancellationToken);
                    }
                    else await InvalidNumber(bot, chatId, cancellationToken);
                    break;

                case AddBeltState.AwaitingK4:
                    if (TryParseDouble(message, out double k4))
                    {
                        state.TempBelt.K4 = k4;
                        state.AddBeltState = AddBeltState.AwaitingL0;
                        await bot.SendMessage(chatId, "Введите длину ремня L0:", cancellationToken: cancellationToken);
                    }
                    else await InvalidNumber(bot, chatId, cancellationToken);
                    break;

                case AddBeltState.AwaitingL0:
                    if (TryParseDouble(message, out double l0))
                    {
                        state.TempBelt.L0 = l0;

                        await _database.AddBeltAsync(state.TempBelt);
                        await bot.SendMessage(chatId, $"Ремень \"{state.TempBelt.Name}\" добавлен в базу!", cancellationToken: cancellationToken);

                        state.TempBelt = new Belt();
                        state.AddBeltState = AddBeltState.None;
                    }
                    else await InvalidNumber(bot, chatId, cancellationToken);
                    break;
            }
        }

        public Task<bool> TryHandleCallbackQueryAsync(ITelegramBotClient bot, CallbackQuery callback, UserState userState, CancellationToken token)
        {
            return Task.FromResult(false); // В этом сценарии кнопки не нужны
        }

        private static bool TryParseDouble(string input, out double value)
            => double.TryParse(input.Replace(',', '.'), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out value);

        private static async Task InvalidNumber(ITelegramBotClient bot, long chatId, CancellationToken ct)
            => await bot.SendMessage(chatId, "Некорректное число. Попробуйте ещё раз.", cancellationToken: ct);

        public IEnumerable<UserRole> AllowedRoles => new[] { UserRole.Admin };
    }
}
