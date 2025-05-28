using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Belt_calculation_Tg_bot.Handlers.Base;
using Belt_calculation_Tg_bot.Models;
using Telegram.Bot;

namespace Belt_calculation_Tg_bot.Handlers
{
    public class LogoutHandler : ICommandHandler
    {
        private readonly Dictionary<long, string> _sessionMap;

        public LogoutHandler(Dictionary<long, string> sessionMap)
        {
            _sessionMap = sessionMap;
        }

        public bool CanHandle(UserState state, string message)
        {
            return message == "/logout";
        }

        public async Task HandleAsync(ITelegramBotClient bot, long chatId, string message, UserState state, CancellationToken cancellationToken)
        {
            if (_sessionMap.ContainsKey(chatId))
            {
                _sessionMap.Remove(chatId);
                await bot.SendMessage(chatId, "Вы вышли из аккаунта.", cancellationToken: cancellationToken);
            }
            else
            {
                await bot.SendMessage(chatId, "Вы не авторизованы.", cancellationToken: cancellationToken);
            }
        }
    }
}
