using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Belt_calculation_Tg_bot.Handlers.Base;
using Belt_calculation_Tg_bot.Models;
using Belt_calculation_Tg_bot.Models.State;
using Telegram.Bot;
using Telegram.Bot.Types;
using Tg__bot.Helpers;

namespace Belt_calculation_Tg_bot.Handlers
{
    public class LogoutHandler : ICommandHandler
    {
        private readonly Dictionary<long, UserSession> _session;

        public LogoutHandler(Dictionary<long, UserSession> session)
        {
            _session = session;
        }

        public bool CanHandle(UserState state, string message)
        {
            return message == "/logout";
        }

        public async Task HandleAsync(ITelegramBotClient bot, long chatId, string message, UserState state, CancellationToken cancellationToken)
        {
            var lang = _session.TryGetValue(chatId, out var session)
                ? session.Language
                : "RU";

            async Task<string> SendTranslated(string original)
            {
                return await DeepL.Translate(original, lang);
            }

            if (_session.ContainsKey(chatId))
            {
                _session.Remove(chatId);
                await bot.SendMessage(chatId, await SendTranslated("Вы вышли из аккаунта."), cancellationToken: cancellationToken);
                var menu = KeyboardHelper.GetMenuForRole(UserRole.Guest);
                await bot.SendMessage(chatId, await SendTranslated("Меню обновлено"), replyMarkup: menu, cancellationToken: cancellationToken);
            }
            else
            {
                await bot.SendMessage(chatId, await SendTranslated("Вы не авторизованы."), cancellationToken: cancellationToken);
            }
        }

        public Task<bool> TryHandleCallbackQueryAsync(ITelegramBotClient bot, CallbackQuery callback, UserState userState, CancellationToken token)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<UserRole> AllowedRoles => new[] { UserRole.User, UserRole.Admin };
    }
}
