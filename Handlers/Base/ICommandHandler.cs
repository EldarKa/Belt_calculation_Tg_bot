using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Belt_calculation_Tg_bot.Models;
using Belt_calculation_Tg_bot.Models.State;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace Belt_calculation_Tg_bot.Handlers.Base
{
    public interface ICommandHandler
    {
        bool CanHandle(UserState state, string message);
        Task HandleAsync(ITelegramBotClient bot, long chatId, string message, UserState userState, CancellationToken cancellationToken);

        Task<bool> TryHandleCallbackQueryAsync(
        ITelegramBotClient bot,
        CallbackQuery callback,
        UserState userState,
        CancellationToken token);

        IEnumerable<UserRole> AllowedRoles { get; }
    }
}
