using System.Runtime.Serialization;
using LearnBotVrk.Telegram.Types;
using Newtonsoft.Json;

namespace LearnBotVrk.Telegram.BotAPI.Types
{
    public class Update
    {
        public enum Types
        {
            Message,
            CallbackQuery,
            Poll
        }
        
        [JsonProperty("update_id")]
        public long Id { get; set; }

        public Types Type { get; set; }
        
        [JsonProperty("message")]
        public Message Message { get; set; }

        [JsonProperty("poll")]
        public Poll Poll { get; set; }
        
        [JsonProperty("poll_answer")]
        public PollAnswer PollAnswer { get; set; }
        
        [JsonProperty("callback_query")]
        public CallbackQuery CallbackQuery { get; set; }


        [OnDeserialized]
        internal void OnDeserialized(StreamingContext context)
        {
            if (Message != null)
            {
                Type = Types.Message;
            }
            else if (Poll != null)
            {
                Type = Types.Poll;
            }
            else if (CallbackQuery != null)
            {
                Type = Types.CallbackQuery;
            }
        }
    }
}