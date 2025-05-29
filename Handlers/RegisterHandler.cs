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
    public class RegisterHandler : ICommandHandler
    {
        private readonly Database _database;
        private readonly Dictionary<long, UserSession> _session;

        public RegisterHandler(Database database, Dictionary<long, UserSession> sessionMap)
        {
            _database = database;
            _session = sessionMap;
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
                    _session[chatId].Username = message;
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

        public Task<bool> TryHandleCallbackQueryAsync(ITelegramBotClient bot, CallbackQuery callback, UserState userState, CancellationToken token)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<UserRole> AllowedRoles => new[] { UserRole.Guest };
    }
}
