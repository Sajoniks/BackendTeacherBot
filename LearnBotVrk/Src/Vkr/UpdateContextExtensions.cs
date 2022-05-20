using System.Threading.Tasks;
using LearnBotVrk.Telegram.BotAPI;
using LearnBotVrk.Telegram.BotAPI.Types;
using LearnBotVrk.Telegram.BotAPI.Types.ReplyMarkup;
using LearnBotVrk.Vkr.API;

namespace LearnBotVrk.Vkr
{
    public static class UpdateContextExtensions
    {
        public static Task<Message> SendBotResponse(this UpdateContext context, string message, IReplyMarkup replyMarkup = null)
        {
            return context.Bot.SendMessageAsync(context.Update.Message.Chat, message, replyMarkup);
        }
    }
}