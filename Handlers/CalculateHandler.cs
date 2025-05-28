using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Belt_calculation_Tg_bot.Data;
using Belt_calculation_Tg_bot.Handlers.Base;
using Belt_calculation_Tg_bot.Models;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace Belt_calculation_Tg_bot.Handlers
{
    public class CalculateHandler : ICommandHandler
    {
        private readonly Database _database;
        private readonly Dictionary<long, CalculationSession> _sessions;

        public CalculateHandler(Database database)
        {
            _database = database;
        }

        public bool CanHandle(UserState state, string message)
        {
            return message == "/calculate" || state.CalculateState != CalculateState.None;
        }

        public async Task HandleAsync(ITelegramBotClient bot, long chatId, string message, UserState userState, CancellationToken cancellationToken)
        {
            if (userState.CalculateState == CalculateState.None)
            {
                var belts = await _database.GetAllBeltsAsync();
                var buttons = belts.Select(b => new[] {
                    InlineKeyboardButton.WithCallbackData(b.Name, $"belt:{b.Name}")
                }).ToList();
                await bot.SendMessage(
                    chatId: chatId,
                    text: "Выберите ремень:",
                    replyMarkup: new InlineKeyboardMarkup(buttons),
                    cancellationToken: cancellationToken
                );
                userState.CalculateState = CalculateState.AwaitingBeltChoice;
                return;
            }

            var sessionData = _sessions[chatId];
            switch (sessionData.State)
            {
                case CalculateState.AwaitingD1:
                    if (double.TryParse(message, out double d1))
                    {
                        sessionData.D1 = d1;
                        sessionData.State = CalculateState.AwaitingD2;
                        await bot.SendMessage(chatId, "Введите диаметр ведомого шкива D2:", cancellationToken: cancellationToken);
                    }
                    break;

                case CalculateState.AwaitingD2:
                    if (double.TryParse(message, out double d2))
                    {
                        sessionData.D2 = d2;
                        sessionData.State = CalculateState.AwaitingL;
                        await bot.SendMessage(chatId, "Введите межосевое расстояние L:", cancellationToken: cancellationToken);
                    }
                    break;

                case CalculateState.AwaitingL:
                    if (double.TryParse(message, out double l))
                    {
                        sessionData.L = l;
                        sessionData.State = CalculateState.AwaitingP;
                        await bot.SendMessage(chatId, "Введите передаваемую мощность P:", cancellationToken: cancellationToken);
                    }
                    break;

                case CalculateState.AwaitingP:
                    if (double.TryParse(message, out double p))
                    {
                        sessionData.P = p;
                        sessionData.State = CalculateState.AwaitingN;
                        await bot.SendMessage(chatId, "Введите частоту вращения ведущего шкива:", cancellationToken: cancellationToken);
                    }
                    break;

                case CalculateState.AwaitingN:
                    if (double.TryParse(message, out double n))
                    {
                        sessionData.N = n;

                        var b = sessionData.SelectedBelt;
                        double Lb = 2 * sessionData.L + (Math.PI / 2) * (sessionData.D1 + sessionData.D2) + Math.Pow(sessionData.D2 - sessionData.D1, 2) / (4 * sessionData.L);
                        double F1 = b.K1 * sessionData.P;
                        double F2 = b.K2 * sessionData.P;
                        double F0 = b.K3 * sessionData.P;
                        double Pnom = b.K4 * sessionData.N;

                        await bot.SendMessage(chatId,
                            $"Результаты расчёта:\n" +
                            $"Общая длина ремня Lb: {Lb:F2}\n" +
                            $"Сила в ветви F1: {F1:F2}\n" +
                            $"Сила в ветви F2: {F2:F2}\n" +
                            $"Рекомендуемая сила натяжения F0: {F0:F2}\n" +
                            $"Номинальная мощность: {Pnom:F2}",
                            cancellationToken: cancellationToken);

                        _sessions.Remove(chatId);
                    }
                    break;
            }
        }

        public async Task HandleBeltSelectionAsync(
            ITelegramBotClient bot,
            long chatId,
            string beltName,
            UserState userState,
            CancellationToken cancellationToken)
        {
            var belt = await _database.GetBeltByNameAsync(beltName);

            if (belt != null)
            {
                userState.SelectedBelt = belt;
                userState.CalculateState = CalculateState.AwaitingD1;

                await bot.SendMessage(
                    chatId: chatId,
                    text: "Введите диаметр ведущего шкива D1:",
                    cancellationToken: cancellationToken
                );
            }
            else
            {
                await bot.SendMessage(
                    chatId: chatId,
                    text: "Ремень не найден. Попробуйте снова с /calculate.",
                    cancellationToken: cancellationToken
                );
            }
        }
    }
}
