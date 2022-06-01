using System;
using LearnBotVrk.Telegram.Types;
using Newtonsoft.Json;

namespace LearnBotVrk.Telegram.BotAPI.Types
{
    public class CallbackQuery
    {
        [JsonProperty("id")] public string Id { get; set; }
        [JsonProperty("from")] public User From { get; set; }
        [JsonProperty("message")] public Message Message { get; set; }
        [JsonProperty("inline_message_id")] public String InlineMessageId { get; set; }
        [JsonProperty("data")] public String Data { get; set; }
    }
}