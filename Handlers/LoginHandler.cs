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
            if (state.State == AuthState.AwaitingLoginUsername)
            {
                var username = message;
                if (await _database.AuthenticateUserAsync(username))
                {
                    _session[chatId].Username = username;
                    await bot.SendMessage(chatId, $"Вход выполнен, {username}", cancellationToken: cancellationToken);
                }
                else
                {
                    await bot.SendMessage(chatId, "Логин не найден. Сначала зарегистрируйтесь: /register", cancellationToken: cancellationToken);
                }

                state.State = AuthState.None;
            }
            else // message == /login
            {
                state.State = AuthState.AwaitingLoginUsername;
                await bot.SendMessage(chatId, "Введите ваш логин:", cancellationToken: cancellationToken);
            }
        }

        public Task<bool> TryHandleCallbackQueryAsync(ITelegramBotClient bot, CallbackQuery callback, UserState userState, CancellationToken token)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<UserRole> AllowedRoles => new[] { UserRole.Guest };
    }
}
