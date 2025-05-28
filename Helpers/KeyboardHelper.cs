using Telegram.Bot;
using Telegram.Bot.Types.ReplyMarkups;

namespace Tg__bot.Helpers;

public static class KeyboardHelper
{
    public static async Task SendLanguageSelectionKeyboard(ITelegramBotClient botClient, long chatId, CancellationToken cancellationToken)
    {
        var keyboard = new InlineKeyboardMarkup(new[]
        {
            new[]
            {
                InlineKeyboardButton.WithCallbackData("🇬🇧 EN", "lang_en"),
                InlineKeyboardButton.WithCallbackData("🇷🇺 RU", "lang_ru")
            },
            new[]
            {
                InlineKeyboardButton.WithCallbackData("🇩🇪 DE", "lang_de"),
                InlineKeyboardButton.WithCallbackData("🇫🇷 FR", "lang_fr")
            }
        });

        await botClient.SendMessage(
            chatId: chatId,
            text: "Выберите язык перевода:",
            replyMarkup: keyboard,
            cancellationToken: cancellationToken
        );
    }
}