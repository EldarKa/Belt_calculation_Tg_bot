using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Belt_calculation_Tg_bot.Data;
using Belt_calculation_Tg_bot.Handlers.Base;
using Belt_calculation_Tg_bot.Models;
using Belt_calculation_Tg_bot.Models.State;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace Belt_calculation_Tg_bot.Handlers
{
    public class CalculateHandler : ICommandHandler
    {
        private readonly Database _database;
        private readonly Dictionary<long, UserSession> _session;

        public CalculateHandler(Database database, Dictionary<long, UserSession> session)
        {
            _database = database;
            _session = session;
        }

        public bool CanHandle(UserState state, string message)
        {
            return message == "/calculate" || state.CalculateState != CalculateState.None;
        }

        public async Task HandleAsync(ITelegramBotClient bot, long chatId, string message, UserState userState, CancellationToken cancellationToken)
        {
            var lang = _session.TryGetValue(chatId, out var session)
                ? session.Language
                : "RU";

            async Task<string> SendTranslated(string original)
            {
                return await DeepL.Translate(original, lang);
            }

            if (userState.CalculateState == CalculateState.None)
            {
                var belts = await _database.GetAllBeltsAsync();
                var buttons = belts.Select(b => new[] {
                    InlineKeyboardButton.WithCallbackData(b.Name, $"belt:{b.Name}")
                }).ToList();
                await bot.SendMessage(
                    chatId: chatId,
                    text: await SendTranslated("Выберите ремень:"),
                    replyMarkup: new InlineKeyboardMarkup(buttons),
                    cancellationToken: cancellationToken
                );
                userState.CalculateState = CalculateState.AwaitingBeltChoice;
                return;
            }
            if (userState.CalculateState == CalculateState.AwaitingBeltChoice)
            {
                await bot.SendMessage(chatId, await SendTranslated("Сначала выберите ремень, нажав на кнопку выше."), cancellationToken: cancellationToken);
                return;
            }

            if (!_session.ContainsKey(chatId))
            {
                await bot.SendMessage(chatId, await SendTranslated("Произошла ошибка: расчёт не инициализирован. Введите /calculate заново."), cancellationToken: cancellationToken);
                return;
            }

            var sessionData = _session[chatId];
            var calcContour = sessionData.Calculation;
            switch (sessionData.State.CalculateState)
            {
                case CalculateState.AwaitingD1:
                    await HandleNumericInputAsync(bot, chatId, message, cancellationToken,
                        await SendTranslated("Введите диаметр ведомого шкива D2:"),
                        CalculateState.AwaitingD2,
                        (s, val) => s.Calculation.D1 = val);
                    break;

                case CalculateState.AwaitingD2:
                    await HandleNumericInputAsync(bot, chatId, message, cancellationToken,
                        await SendTranslated("Введите межосевое расстояние L:"),
                        CalculateState.AwaitingL,
                        (s, val) => s.Calculation.D2 = val);
                    break;

                case CalculateState.AwaitingL:
                    await HandleNumericInputAsync(bot, chatId, message, cancellationToken,
                        await SendTranslated("Введите передаваемую мощность P:"),
                        CalculateState.AwaitingP,
                        (s, val) => s.Calculation.L = val);
                    break;

                case CalculateState.AwaitingP:
                    await HandleNumericInputAsync(bot, chatId, message, cancellationToken,
                        await SendTranslated(await SendTranslated("Введите частоту вращения ведущего шкива:")),
                        CalculateState.AwaitingN,
                        (s, val) => s.Calculation.P = val);
                    break;

                case CalculateState.AwaitingN:
                    if (double.TryParse(message, out double n))
                    {
                        calcContour.N = n;

                        var b = calcContour.SelectedBelt;
                        double Lb = 2 * calcContour.L + (Math.PI / 2) * (calcContour.D1 + calcContour.D2) + Math.Pow(calcContour.D2 - calcContour.D1, 2) / (4 * calcContour.L);
                        double F1 = b.K1 * calcContour.P;
                        double F2 = b.K2 * calcContour.P;
                        double F0 = b.K3 * calcContour.P;
                        double Pnom = b.K4 * calcContour.N;

                        await bot.SendMessage(chatId,
                            $"{await SendTranslated("Результаты расчёта:")}\n" +
                            $"{await SendTranslated("Общая длина ремня Lb:")} {Lb:F2}\n" +
                            $"{await SendTranslated("Сила в ветви F1:")} {F1:F2}\n" +
                            $"{await SendTranslated("Сила в ветви F2:")} {F2:F2}\n" +
                            $"{await SendTranslated("Рекомендуемая сила натяжения F0:")} {F0:F2}\n" +
                            $"{await SendTranslated("Номинальная мощность:")} {Pnom:F2}",
                            cancellationToken: cancellationToken);

                        userState.CalculateState = CalculateState.None;
                        userState.SelectedBelt = null;
                    }
                    break;
            }
        }

        public async Task<bool> TryHandleCallbackQueryAsync(
            ITelegramBotClient bot,
            CallbackQuery callback,
            UserState userState,
            CancellationToken token)
        {
            if (callback.Data == null || !callback.Data.StartsWith("belt:"))
                return false;

            var beltName = callback.Data["belt:".Length..];
            var belt = await _database.GetBeltByNameAsync(beltName);
            if (belt == null)
            {
                await bot.AnswerCallbackQuery(callback.Id, cancellationToken: token);
                await bot.SendMessage(callback.Message!.Chat.Id, "Ремень не найден", cancellationToken: token);
                return true;
            }

            userState.SelectedBelt = belt;
            userState.CalculateState = CalculateState.AwaitingD1;

            // 👇 ОБЯЗАТЕЛЬНО: инициализируем CalculationSession
            _session[callback.Message.Chat.Id].Calculation = new CalculationContour
            {
                SelectedBelt = belt
            };

            await bot.AnswerCallbackQuery(callback.Id, cancellationToken: token);
            await bot.SendMessage(callback.Message.Chat.Id, "Введите диаметр ведущего шкива D1:", cancellationToken: token);
            return true;
        }

        private async Task<bool> HandleNumericInputAsync(
            ITelegramBotClient bot,
            long chatId,
            string message,
            CancellationToken cancellationToken,
            string prompt,
            CalculateState nextState,
            Action<UserSession, double> applyValue)
        {
            var lang = _session.TryGetValue(chatId, out var session)
            ? session.Language
            : "RU";
            if (double.TryParse(message, out double value))
            {
                applyValue(session, value);
                session.State.CalculateState = nextState;

                var translated = await DeepL.Translate(prompt, lang);
                await bot.SendMessage(chatId, translated, cancellationToken: cancellationToken);
                return true;
            }
            else
            {
                var error = await DeepL.Translate("Введите число. Пример: 123.45", lang);
                await bot.SendMessage(chatId, error, cancellationToken: cancellationToken);
                return false;
            }
        }

        public IEnumerable<UserRole> AllowedRoles => new[] { UserRole.User, UserRole.Admin };
    }
}
