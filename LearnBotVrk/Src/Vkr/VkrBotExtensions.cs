using System;
using System.Threading.Tasks;
using LearnBotVrk.botApi;
using LearnBotVrk.Telegram.BotAPI;
using LearnBotVrk.Telegram.Types;

namespace LearnBotVrk.Vkr
{
    public static class VkrBotExtensions
    {
        private class BotMessageResponse : Window.IActionResponse
        {
            private string _text;

            public BotMessageResponse(string text)
            {
                _text = text;
            }

            public void Invoke()
            {
                var ctx = Context.Get();
                ctx.Bot.SendMessageAsync(ctx.Chat, _text);
            }
        }

        public static Window.IActionResponse CreateBotMessageResponse(this IBot bot, String text)
        {
            return new BotMessageResponse(text);
        }
    }
}