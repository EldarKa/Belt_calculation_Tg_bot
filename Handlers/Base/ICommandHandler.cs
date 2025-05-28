using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Belt_calculation_Tg_bot.Models;
using Telegram.Bot;

namespace Belt_calculation_Tg_bot.Handlers.Base
{
    public interface ICommandHandler
    {
        bool CanHandle(UserState state, string message);
        Task HandleAsync(ITelegramBotClient bot, long chatId, string message, UserState userState, CancellationToken cancellationToken);
    }
}
