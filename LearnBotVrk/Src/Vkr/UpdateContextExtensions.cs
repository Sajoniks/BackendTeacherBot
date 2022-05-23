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
            var targetMessage = context.Update.Message ?? context.Update.CallbackQuery.Message;
            return context.Bot.SendMessageAsync(targetMessage.Chat, message, replyMarkup);
        }

        public static Task<Message> EditMessage(this UpdateContext context, Message message, string text, IReplyMarkup replyMarkup = null)
        {
            var targetMessage = message;
            return context.Bot.EditMessageTextAsync(targetMessage.Chat, targetMessage.Id, text, replyMarkup);
        }
    }
}