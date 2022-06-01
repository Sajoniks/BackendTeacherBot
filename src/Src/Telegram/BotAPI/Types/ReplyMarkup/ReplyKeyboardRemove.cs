using Newtonsoft.Json;

namespace LearnBotVrk.Telegram.BotAPI.Types.ReplyMarkup
{
    public class ReplyKeyboardRemove : IReplyMarkup
    {
        [JsonProperty("remove_keyboard")] private bool _remove;

        public ReplyKeyboardRemove(bool remove)
        {
            this._remove = remove;
        }

        public static ReplyKeyboardRemove RemoveKeyboard()
        {
            return new ReplyKeyboardRemove(true);
        }
    }
}