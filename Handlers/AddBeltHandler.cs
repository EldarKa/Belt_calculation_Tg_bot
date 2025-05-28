using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Belt_calculation_Tg_bot.Handlers.Base;
using Belt_calculation_Tg_bot.Models;
using Telegram.Bot;

namespace Belt_calculation_Tg_bot.Handlers
{
    public class AddBeltHandler : ICommandHandler
    {
        public bool CanHandle(UserState state, string message)
        {
            return message == "/add_Belt";
        }

        public async Task HandleAsync(ITelegramBotClient bot, long chatId, string message, UserState state, CancellationToken cancellationToken)
        {
            await bot.SendMessage(chatId, "Добавление ремня в базу ещё не реализовано.", cancellationToken: cancellationToken);
        }
    }
}
