using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;

namespace Belt_calculation_Tg_bot.Helpers
{
    public static class InputHelper
    {
        public static async Task<bool> HandleNumericInputAsync<T>(
            ITelegramBotClient bot,
            long chatId,
            string message,
            CancellationToken cancellationToken,
            string prompt,
            Action<T, double> applyValue,
            Func<T?>? getStateObject = null)
            where T : class
        {
            if (double.TryParse(message.Replace(',', '.'), out double value))
            {
                var target = getStateObject?.Invoke();
                if (target is not null)
                    applyValue(target, value);

                await bot.SendMessage(chatId, prompt, cancellationToken: cancellationToken);
                return true;
            }
            else
            {
                await bot.SendMessage(chatId, "Некорректное число. Пример: 12.34", cancellationToken: cancellationToken);
                return false;
            }
        }
    }
}
