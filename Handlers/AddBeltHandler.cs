using System;
using System.Collections.Concurrent;
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
        private readonly ConcurrentDictionary<long, UserSession> _session;

        public AddBeltHandler(Database database, ConcurrentDictionary<long, UserSession> session)
        {
            _database = database;
            _session = session;
        }
        public bool CanHandle(UserState state, string message)
        {
            return message == "/add_Belt" || state.AddBeltState != AddBeltState.None;
        }

        public async Task HandleAsync(ITelegramBotClient bot, long chatId, string message, UserState state, CancellationToken cancellationToken)
        {
            var lang = (_session.TryGetValue(chatId, out var session)) ? session.Language : "EN";
            async Task SendTranslated(string original)
            {
                var translated = await DeepL.Translate(original, lang);
                await bot.SendMessage(chatId, translated, cancellationToken: cancellationToken);
            }

            switch (state.AddBeltState)
            {
                case AddBeltState.None:
                    state.TempBelt = new Belt();
                    state.AddBeltState = AddBeltState.AwaitingName;
                    await SendTranslated("Введите название ремня:");
                    break;

                case AddBeltState.AwaitingName:
                    state.TempBelt.Name = message;
                    state.AddBeltState = AddBeltState.AwaitingWeight;
                    await SendTranslated("Введите вес ремня (Кг):");
                    break;

                case AddBeltState.AwaitingWeight:
                    if (TryParseDouble(message, out double weight))
                    {
                        state.TempBelt.Weight = weight;
                        state.AddBeltState = AddBeltState.AwaitingK1;
                        await SendTranslated("Введите коэффициент K1:");
                    }
                    else await InvalidNumber(bot, chatId, lang, cancellationToken);
                    break;

                case AddBeltState.AwaitingK1:
                    if (TryParseDouble(message, out double k1))
                    {
                        state.TempBelt.K1 = k1;
                        state.AddBeltState = AddBeltState.AwaitingK2;
                        await SendTranslated("Введите коэффициент K2:");
                    }
                    else await InvalidNumber(bot, chatId, lang, cancellationToken);
                    break;

                case AddBeltState.AwaitingK2:
                    if (TryParseDouble(message, out double k2))
                    {
                        state.TempBelt.K2 = k2;
                        state.AddBeltState = AddBeltState.AwaitingK3;
                        await SendTranslated("Введите коэффициент K3:");
                    }
                    else await InvalidNumber(bot, chatId, lang, cancellationToken); ;
                    break;

                case AddBeltState.AwaitingK3:
                    if (TryParseDouble(message, out double k3))
                    {
                        state.TempBelt.K3 = k3;
                        state.AddBeltState = AddBeltState.AwaitingK4;
                        await SendTranslated("Введите коэффициент K4:");
                    }
                    else await InvalidNumber(bot, chatId, lang, cancellationToken);
                    break;

                case AddBeltState.AwaitingK4:
                    if (TryParseDouble(message, out double k4))
                    {
                        state.TempBelt.K4 = k4;
                        state.AddBeltState = AddBeltState.AwaitingL0;
                        await SendTranslated("Введите длину ремня L0:");
                    }
                    else await InvalidNumber(bot, chatId, lang, cancellationToken);
                    break;

                case AddBeltState.AwaitingL0:
                    if (TryParseDouble(message, out double l0))
                    {
                        state.TempBelt.L0 = l0;

                        await _database.AddBeltAsync(state.TempBelt);
                        var confirmation = await DeepL.Translate($"Ремень \"{state.TempBelt.Name}\" добавлен в базу!", lang);
                        await bot.SendMessage(chatId, confirmation, cancellationToken: cancellationToken);

                        state.TempBelt = new Belt();
                        state.AddBeltState = AddBeltState.None;
                    }
                    else await InvalidNumber(bot, chatId, lang, cancellationToken);
                    break;
            }
        }

        public Task<bool> TryHandleCallbackQueryAsync(ITelegramBotClient bot, CallbackQuery callback, UserState userState, CancellationToken token)
        {
            return Task.FromResult(false); // В этом сценарии кнопки не нужны
        }

        private static bool TryParseDouble(string input, out double value)
            => double.TryParse(input.Replace(',', '.'), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out value);

        private static async Task InvalidNumber(ITelegramBotClient bot, long chatId, string lang, CancellationToken ct)
        {
            var text = await DeepL.Translate("Некорректное число. Попробуйте ещё раз.", lang);
            await bot.SendMessage(chatId, text, cancellationToken: ct);
        }



        public IEnumerable<UserRole> AllowedRoles => new[] { UserRole.Admin };
    }
}
