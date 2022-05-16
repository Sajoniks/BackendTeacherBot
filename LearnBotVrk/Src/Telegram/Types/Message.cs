using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace LearnBotVrk.Telegram.Types
{
    public class Message
    {
        [JsonProperty("message_id")] private int MessageId { get; set; }
        [JsonProperty("text")] public string Text { get; set; }
        [JsonProperty("from")] public User From { get; set; }
        [JsonProperty("chat")] public Chat Chat { get; set; }
    }
}