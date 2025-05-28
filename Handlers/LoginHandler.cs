using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Belt_calculation_Tg_bot.Data;
using Belt_calculation_Tg_bot.Handlers.Base;
using Belt_calculation_Tg_bot.Models;
using Telegram.Bot;

namespace Belt_calculation_Tg_bot.Handlers
{
    public class LoginHandler : ICommandHandler
    {
        private readonly Database _database;
        private readonly Dictionary<long, string> _sessionMap;

        public LoginHandler(Database database, Dictionary<long, string> sessionMap)
        {
            _database = database;
            _sessionMap = sessionMap;
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
                    _sessionMap[chatId] = username;
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
    }
}
