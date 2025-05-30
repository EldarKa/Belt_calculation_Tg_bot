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
using static System.Collections.Specialized.BitVector32;

namespace Belt_calculation_Tg_bot.Handlers
{
    public class LangueHandler : ICommandHandler
    {
        private readonly Dictionary<long, UserSession> _session;

        public LangueHandler(Dictionary<long, UserSession> session)
        {
            _session = session;
        }

        public bool CanHandle(UserState state, string message)
        {
            return message == "/langue";
        }

        public async Task HandleAsync(ITelegramBotClient bot, long chatId, string message, UserState state, CancellationToken cancellationToken)
        {
            await KeyboardHelper.SendLanguageSelectionKeyboard(bot, chatId, cancellationToken);
        }

        public async Task<bool> TryHandleCallbackQueryAsync(ITelegramBotClient bot, CallbackQuery callback, UserState state, CancellationToken token)
        {
            var chatId = callback.Message.Chat.Id;
            var data = callback.Data;

            if (data != null && data.StartsWith("lang_"))
            {
                var langCode = data.Replace("lang_", "");
                _session[chatId].PreferredLanguage = langCode;

                await bot.AnswerCallbackQuery(callback.Id, cancellationToken: token);
                await bot.SendMessage(chatId, $"Выбран язык: {langCode.ToUpper()}", cancellationToken: token);
                return true;
            }

            return false;
        }

        public IEnumerable<UserRole> AllowedRoles => new[] { UserRole.User, UserRole.Admin, UserRole.Guest };
    }
}
