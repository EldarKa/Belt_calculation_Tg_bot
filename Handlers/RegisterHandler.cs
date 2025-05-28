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
    public class RegisterHandler : ICommandHandler
    {
        private readonly Database _database;
        private readonly Dictionary<long, string> _sessionMap;

        public RegisterHandler(Database database, Dictionary<long, string> sessionMap)
        {
            _database = database;
            _sessionMap = sessionMap;
        }

        public bool CanHandle(UserState state, string message)
        {
            return state.State == AuthState.AwaitingRegisterUsername || message == "/register";
        }

        public async Task HandleAsync(ITelegramBotClient bot, long chatId, string message, UserState state, CancellationToken cancellationToken)
        {
            if (state.State == AuthState.AwaitingRegisterUsername)
            {
                if (await _database.UserExistsAsync(message))
                {
                    await bot.SendMessage(chatId, "Пользователь с таким логином уже существует. Попробуйте снова: /register", cancellationToken: cancellationToken);
                }
                else
                {
                    await _database.AddUserAsync(chatId, message);
                    _sessionMap[chatId] = message;
                    await bot.SendMessage(chatId, $"Регистрация успешна! Добро пожаловать, {message}", cancellationToken: cancellationToken);
                }
                state.State = AuthState.None;
            }
            else // message == /register
            {
                state.State = AuthState.AwaitingRegisterUsername;
                await bot.SendMessage(chatId, "Введите логин для регистрации:", cancellationToken: cancellationToken);
            }
        }
    }
}
