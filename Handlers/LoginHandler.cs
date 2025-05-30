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
using Tg__bot.Helpers;

namespace Belt_calculation_Tg_bot.Handlers
{
    public class LoginHandler : ICommandHandler
    {
        private readonly Database _database;
        private readonly Dictionary<long, UserSession> _session;

        public LoginHandler(Database database, Dictionary<long, UserSession> session)
        {
            _database = database;
            _session = session;
        }

        public bool CanHandle(UserState state, string message)
        {
            return state.State == AuthState.AwaitingLoginUsername || message == "/login";
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
            
            if (state.State == AuthState.AwaitingLoginUsername)
            {
                var username = message;
                var (exists, role) = await _database.GetUserByUsernameAsync(username);

                if (exists && role.HasValue)
                {
                    _session[chatId].Username = username;
                    _session[chatId].Role = role.Value;
                    await bot.SendMessage(chatId, $"{await SendTranslated("Вход выполнен как")} {username} ({await SendTranslated("роль:")} {role})", cancellationToken: cancellationToken);
                    var menu = KeyboardHelper.GetMenuForRole(role.Value);
                    await bot.SendMessage(chatId, $"{ await SendTranslated("Меню обновлено")}", replyMarkup: menu, cancellationToken: cancellationToken);
                }
                else
                {
                    await bot.SendMessage(chatId, $"{await SendTranslated("Логин не найден. Сначала зарегистрируйтесь:")} /register", cancellationToken: cancellationToken);
                }

                state.State = AuthState.None;
            }
            else // message == /login
            {
                state.State = AuthState.AwaitingLoginUsername;
                await bot.SendMessage(chatId, $"{await SendTranslated("Введите ваш логин:")}", cancellationToken: cancellationToken);
            }
        }

        public Task<bool> TryHandleCallbackQueryAsync(ITelegramBotClient bot, CallbackQuery callback, UserState userState, CancellationToken token)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<UserRole> AllowedRoles => new[] { UserRole.Guest };

    }
}
