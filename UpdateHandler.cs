using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using Belt_calculation_Tg_bot.Data;
using Belt_calculation_Tg_bot.Handlers;
using Belt_calculation_Tg_bot.Handlers.Base;
using Belt_calculation_Tg_bot.Models;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using Tg__bot.Helpers;
using static Telegram.Bot.TelegramBotClient;


namespace Belt_calculation_Tg_bot
{
    public delegate void MessageHandler(string message, string? username, long userId);

    public class UpdateHandler : IUpdateHandler
    {
        private readonly Database _database;
        private readonly Dictionary<long, string> _sessionMap = new(); 
        private readonly Dictionary<long, UserState> _userStates = new(); // telegramId -> state
        private readonly List<ICommandHandler> _handlers;

        public delegate void MessageHandler(string message);
        public event MessageHandler? OnHandleUpdateStarted;
        public event MessageHandler? OnHandleUpdateCompleted;

        public UpdateHandler(Database db)
        {
            _database = db;
            _handlers = new()
            {
                new RegisterHandler(db, _sessionMap),
                new LoginHandler(db, _sessionMap),
                new CalculateHandler(db),
                // Добавишь другие обработчики здесь
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

                if (!_userStates.ContainsKey(chatId))
                    _userStates[chatId] = new UserState();

                var state = _userStates[chatId];

                var handled = false;
                foreach (var handler in _handlers)
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
                    var isAuth = _sessionMap.TryGetValue(chatId, out var username);
                    var reply = isAuth ? $"Принято сообщение от {username}" : "Вы не авторизованы. Введите /login или /register";
                    await botClient.SendMessage(chatId: chatId, text: reply, cancellationToken: cancellationToken);
                }

                OnHandleUpdateCompleted?.Invoke(text);
            }
            else if (update.Type == UpdateType.CallbackQuery)
            {
                var callback = update.CallbackQuery!;
                var chatId = callback.Message!.Chat.Id;
                var data = callback.Data;

                if (!_userStates.ContainsKey(chatId))
                    _userStates[chatId] = new UserState();

                var state = _userStates[chatId];

                if (data != null && data.StartsWith("belt:"))
                {
                    var beltName = data.Substring("belt:".Length);
                    var handler = _handlers.OfType<CalculateHandler>().FirstOrDefault();
                    if (handler != null)
                    {
                        await handler.HandleBeltSelectionAsync(botClient, chatId, beltName, state, cancellationToken);
                        await botClient.MakeRequestAsync(
    new Telegram.Bot.Requests.AnswerCallbackQueryRequest(callback.Id),
    cancellationToken
);
                    }
                }
            }
        }


        public Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, HandleErrorSource source, CancellationToken cancellationToken)
        {
            Console.WriteLine($"Ошибка: {exception.Message}");
            return Task.CompletedTask;
        }
    }
}
