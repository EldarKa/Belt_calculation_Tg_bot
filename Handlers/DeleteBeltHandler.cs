using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Belt_calculation_Tg_bot.Data;
using Belt_calculation_Tg_bot.Handlers.Base;
using Belt_calculation_Tg_bot.Models;
using Belt_calculation_Tg_bot.Models.State;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace Belt_calculation_Tg_bot.Handlers
{
    public class DeleteBeltHandler : ICommandHandler
    {
        private readonly Database _database;

        public DeleteBeltHandler(Database database)
        {
            _database = database;
        }

        public bool CanHandle(UserState state, string message)
        {
            return message == "/delete_Belt" || state.DeleteBeltState == DeleteBeltState.AwaitingId;
        }

        public async Task HandleAsync(ITelegramBotClient bot, long chatId, string message, UserState state, CancellationToken cancellationToken)
        {
            if (state.DeleteBeltState != DeleteBeltState.AwaitingId)
            {
                var belts = await _database.GetAllBeltsAsync();
                if (!belts.Any())
                {
                    await bot.SendMessage(chatId, "Список ремней пуст.", cancellationToken: cancellationToken);
                    return;
                }

                var list = string.Join("\n", belts.Select(b => $"{b.Id}: {b.Name}"));
                await bot.SendMessage(chatId, $"Список ремней:\n{list}\nВведите ID ремня для удаления:", cancellationToken: cancellationToken);

                state.DeleteBeltState = DeleteBeltState.AwaitingId;
                return;
            }

            if (int.TryParse(message, out int beltId))
            {
                var success = await _database.DeleteBeltAsync(beltId);
                if (success)
                {
                    await bot.SendMessage(chatId, $"✅ Ремень с ID {beltId} успешно удалён.", cancellationToken: cancellationToken);
                }
                else
                {
                    await bot.SendMessage(chatId, $"⚠️ Ремень с ID {beltId} не найден.", cancellationToken: cancellationToken);
                }
                state.DeleteBeltState = DeleteBeltState.None;
            }
            else
            {
                await bot.SendMessage(chatId, "❌ Пожалуйста, введите корректный числовой ID ремня.", cancellationToken: cancellationToken);
            }
        }

        public Task<bool> TryHandleCallbackQueryAsync(ITelegramBotClient bot, CallbackQuery callback, UserState userState, CancellationToken token)
        {
            return Task.FromResult(false);
        }

        public IEnumerable<UserRole> AllowedRoles => new[] { UserRole.Admin };
    }
}
