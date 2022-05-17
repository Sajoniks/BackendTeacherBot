using System.Runtime.Serialization;
using LearnBotVrk.Telegram.Types;
using Newtonsoft.Json;

namespace LearnBotVrk.Telegram.BotAPI.Types
{
    public class Update
    {
        public enum Type
        {
            Message,
            CallbackQuery,
            Poll
        }
        
        [JsonProperty("update_id")]
        public long Id { get; set; }

        public Type UpdateType { get; set; }
        
        [JsonProperty("message")]
        public Message Message { get; set; }

        [JsonProperty("poll")]
        public Poll Poll { get; set; }
        
        [JsonProperty("callback_query")]
        public CallbackQuery CallbackQuery { get; set; }


        [OnDeserialized]
        internal void OnDeserialized(StreamingContext context)
        {
            if (Message != null)
            {
                UpdateType = Type.Message;
            }
            else if (Poll != null)
            {
                UpdateType = Type.Poll;
            }
            else if (CallbackQuery != null)
            {
                UpdateType = Type.CallbackQuery;
            }
        }
    }
}