using System;
using System.Collections.Generic;
using System.Data;
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
            var lang = _session.TryGetValue(chatId, out var session)
                ? session.Language
                : "RU";

            async Task<string> SendTranslated(string original)
            {
                return await DeepL.Translate(original, lang);
            }
            if (state.State == AuthState.AwaitingRegisterUsername)
            {
                if (await _database.UserExistsAsync(message))
                {
                    await bot.SendMessage(chatId, await SendTranslated("Пользователь с таким логином уже существует. Попробуйте снова:") + "/register", cancellationToken: cancellationToken);
                }
                else
                {
                    var success = await _database.AddUserAsync(chatId, message);

                    if (!success)
                    {
                        await bot.SendMessage(chatId, await SendTranslated("Ошибка при регистрации. Повторите попытку позже."), cancellationToken: cancellationToken);
                        state.State = AuthState.None;
                        return;
                    }
                    _session[chatId].Username = message;
                    _session[chatId].Role = UserRole.User;
                    await bot.SendMessage(chatId, $"{await SendTranslated("Регистрация успешна! Добро пожаловать")}, {message}", cancellationToken: cancellationToken);
                    var menu = KeyboardHelper.GetMenuForRole(_session[chatId].Role);
                    await bot.SendMessage(chatId, await SendTranslated("Меню обновлено"), replyMarkup: menu, cancellationToken: cancellationToken);
                }
                state.State = AuthState.None;
            }
            else // message == /register
            {
                state.State = AuthState.AwaitingRegisterUsername;
                await bot.SendMessage(chatId, await SendTranslated("Введите логин для регистрации:"), cancellationToken: cancellationToken);
            }
        }

        public Task<bool> TryHandleCallbackQueryAsync(ITelegramBotClient bot, CallbackQuery callback, UserState userState, CancellationToken token)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<UserRole> AllowedRoles => new[] { UserRole.Guest };
    }
}
