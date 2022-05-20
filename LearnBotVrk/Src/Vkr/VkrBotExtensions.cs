using System;
using System.Threading.Tasks;
using LearnBotVrk.botApi;
using LearnBotVrk.Telegram.BotAPI;
using LearnBotVrk.Telegram.BotAPI.Types.ReplyMarkup;
using LearnBotVrk.Telegram.Types;

namespace LearnBotVrk.Vkr
{
    public static class VkrBotExtensions
    {
        private class BotMessageResponse : Window.IActionResponse
        {
            private string _text;
            private IReplyMarkup _markup;

            public BotMessageResponse(string text, IReplyMarkup markup)
            {
                _text = text;
                _markup = markup;
            }

            public async void Invoke()
            {
                var ctx = Context.Get();
                ctx.LastSentMessage = await ctx.Bot.SendMessageAsync(ctx.Chat, _text, _markup);
            }
        }

        public static Window.IActionResponse CreateBotMessageResponse(this IBot bot, String text, IReplyMarkup replyMarkup = null)
        {
            return new BotMessageResponse(text, replyMarkup);
        }
    }
}