using System.Collections.Concurrent;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using Belt_calculation_Tg_bot.Data;
using Belt_calculation_Tg_bot.Handlers;
using Belt_calculation_Tg_bot.Handlers;
using Belt_calculation_Tg_bot.Handlers.Base;
using Belt_calculation_Tg_bot.Models;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Polling;
using Telegram.Bot.Requests;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using Tg__bot.Helpers;
using static Telegram.Bot.TelegramBotClient;


namespace Belt_calculation_Tg_bot
{
    public class UpdateHandler : IUpdateHandler
    {
        private readonly Database _database;
        private readonly ConcurrentDictionary<long, UserSession> _session = new(); 
        private readonly List<ICommandHandler> _handlers;

        public delegate void MessageHandler(string message);
        public event MessageHandler? OnHandleUpdateStarted;
        public event MessageHandler? OnHandleUpdateCompleted;

        public UpdateHandler(Database db)
        {
            _database = db;
            _handlers = new()
            {
                new RegisterHandler(db, _session),
                new LoginHandler(db, _session),
                new CalculateHandler(db, _session),
                new AddBeltHandler(db, _session),
                new DeleteBeltHandler(db, _session),
                new LogoutHandler(_session),
                new LangueHandler(_session)
            };
        }

        public async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            if (update.Type == UpdateType.Message && update.Message?.Text != null)
            {
                var message = update.Message;
                var text = message.Text.Trim();
                var chatId = message.Chat.Id;

                OnHandleUpdateStarted?.Invoke(text);

                if (!_session.ContainsKey(chatId))
                    _session[chatId] = new UserSession();

                var state = _session[chatId].State;
                var role = _session[chatId].Role;
                var handled = false;

                var availableHandlers = _handlers
                    .Where(h => h.AllowedRoles.Contains(role))
                    .ToList();

                foreach (var handler in availableHandlers)
                {
                    if (handler.CanHandle(state, text))
                    {
                        await handler.HandleAsync(botClient, chatId, text, state, cancellationToken);
                        handled = true;
                        break;
                    }
                }

                if (!handled)
                {
                    var isAuth = _session.TryGetValue(chatId, out var userSession);
                    var lang = userSession?.Language;
                    string translated;

                    string reply;
                    if (isAuth && !string.IsNullOrEmpty(userSession?.Username))
                    {
                        translated = await DeepL.Translate("Принято сообщение от", lang);
                        reply = $"{translated} {userSession.Username}";
                    }
                    else
                    {
                        translated = await DeepL.Translate("Вы не авторизованы. Введите", lang);
                        reply = $"{translated} /login, /register";
                    }

                    var menu = KeyboardHelper.GetMenuForRole(role);

                    await botClient.SendMessage(chatId: chatId, text: reply, replyMarkup: menu, cancellationToken: cancellationToken);
                }

                OnHandleUpdateCompleted?.Invoke(text);
            }
            else if (update.Type == UpdateType.CallbackQuery)
            {
                var callback = update.CallbackQuery!;
                var chatId = callback.Message!.Chat.Id;
                var data = callback.Data;

                if (!_session.ContainsKey(chatId))
                    _session[chatId] = new UserSession();

                var state = _session[chatId].State;

                var langueHandler = _handlers.OfType<LangueHandler>().FirstOrDefault();
                if (langueHandler != null && await langueHandler.TryHandleCallbackQueryAsync(botClient, callback, state, cancellationToken))
                    return;

                if (data != null && data.StartsWith("belt:"))
                {
                    var calcHandler = _handlers.OfType<CalculateHandler>().FirstOrDefault();
                    if (calcHandler != null)
                    {
                        await calcHandler.TryHandleCallbackQueryAsync(botClient, callback, state, cancellationToken);
                        return;
                    }
                }

                await botClient.AnswerCallbackQuery(callback.Id, cancellationToken: cancellationToken);
            }
        }



        public Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, HandleErrorSource source, CancellationToken cancellationToken)
        {
            Console.WriteLine($"Error: {exception.Message}");
            return Task.CompletedTask;
        }
    }
}
