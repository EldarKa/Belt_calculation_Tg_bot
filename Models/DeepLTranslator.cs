using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Belt_calculation_Tg_bot.Models.Base;

namespace Belt_calculation_Tg_bot.Models
{
    public static class DeepLTranslator
    {
        public static async Task<string> TranslateTextAsync(string text, string targetLang)
        {
            using var httpClient = new HttpClient();
            var request = new HttpRequestMessage(HttpMethod.Post, BotConfig.DeepLApiUrl);

            var content = new FormUrlEncodedContent(new[]
            {
            new KeyValuePair<string, string>("auth_key", BotConfig.DeepLApiKey),
            new KeyValuePair<string, string>("text", text),
            new KeyValuePair<string, string>("target_lang", targetLang.ToUpper())
        });

            request.Content = content;

            var response = await httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(json);
            return doc.RootElement
                .GetProperty("translations")[0]
                .GetProperty("text")
                .GetString()!;
        }
    }
}
