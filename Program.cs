using System.Threading;
using Belt_calculation_Tg_bot.Data;
using Belt_calculation_Tg_bot.Models;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using static Telegram.Bot.TelegramBotClient;

namespace Belt_calculation_Tg_bot


{
    internal class Program
    {
        //static async Task Main(string[] args)
        //{
        //    var botClient = new TelegramBotClient("<your_bot_token_here>");
        //    var cts = new CancellationTokenSource();
        //    var cancellationToken = cts.Token;

        //    var connectionString = "Host=localhost;Username=postgres;Password=your_password;Database=your_database";
        //    var database = new Database(connectionString);

        //    var updateHandler = new UpdateHandler(database);

        //    updateHandler.OnHandleUpdateStarted += (message) => Console.WriteLine($"Началась обработка сообщения '{message}'");
        //    updateHandler.OnHandleUpdateCompleted += (message) => Console.WriteLine($"Закончилась обработка сообщения '{message}'");

        //    var receiverOptions = new ReceiverOptions
        //    {
        //        AllowedUpdates = new[] { UpdateType.Message },
        //        DropPendingUpdates = true
        //    };

        //    var me = await botClient.GetMeAsync();
        //    Console.WriteLine($"{me.FirstName} запущен!");

        //    botClient.StartReceiving(updateHandler, receiverOptions, cancellationToken);

        //    try
        //    {
        //        while (true)
        //        {
        //            Console.WriteLine("Нажмите клавишу A для выхода...");
        //            var key = Console.ReadKey(true).Key;

        //            if (key == ConsoleKey.A)
        //            {
        //                Console.WriteLine("Завершение работы...");
        //                cts.Cancel();
        //                break;
        //            }
        //            else
        //            {
        //                var info = await botClient.GetMeAsync();
        //                Console.WriteLine($"Имя: {info.FirstName}, Username: @{info.Username}, ID: {info.Id}");
        //            }
        //        }
        //    }
        //    finally
        //    {
        //        updateHandler.OnHandleUpdateStarted -= (message) => Console.WriteLine($"Началась обработка сообщения '{message}'");
        //        updateHandler.OnHandleUpdateCompleted -= (message) => Console.WriteLine($"Закончилась обработка сообщения '{message}'");
        //    }
        //}
        static async Task Main(string[] args)
        {
            var botClient = new TelegramBotClient(BotConfig.TelegramToken);
            using var cts = new CancellationTokenSource();
            var cancellationToken = cts.Token;
            var receiverOptions = new ReceiverOptions
            {
                AllowedUpdates = Array.Empty<UpdateType>(), // <== получать все типы обновлений, включая CallbackQuery
                DropPendingUpdates = true
            };

            var database = new Database(BotConfig.connectionStringDB);
            var updateHandler = new UpdateHandler(database);

            updateHandler.OnHandleUpdateStarted += (message) => Console.WriteLine($"Началась обработка сообщения '{message}'");
            updateHandler.OnHandleUpdateCompleted += (message) => Console.WriteLine($"Закончилась обработка сообщения '{message}'");





            await botClient.DeleteWebhook(cancellationToken: cts.Token);
            botClient.StartReceiving(updateHandler, receiverOptions, cancellationToken);

            try
            {
                while (true)
                {
                    Console.WriteLine("Нажмите клавишу A для выхода...");
                    var key = Console.ReadKey(true).Key;

                    if (key == ConsoleKey.A)
                    {
                        Console.WriteLine("Завершение работы...");
                        cts.Cancel();
                        break;
                    }
                    else
                    {
                        var info = await botClient.GetMe();
                        Console.WriteLine($"Имя: {info.FirstName}, Username: @{info.Username}, ID: {info.Id}");
                    }
                }
            }
            finally
            {
                updateHandler.OnHandleUpdateStarted -= (message) => Console.WriteLine($"Началась обработка сообщения '{message}'");
                updateHandler.OnHandleUpdateCompleted -= (message) => Console.WriteLine($"Закончилась обработка сообщения '{message}'");
            }

            Console.ReadLine();
            cts.Cancel();
        }
    }
}
