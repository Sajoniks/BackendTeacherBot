using System;
using Newtonsoft.Json;

namespace LearnBotVrk.Telegram.Types
{
    public class Chat
    {
        [JsonProperty("id")] public int ChatId { get; set; }
        [JsonProperty("title")] public String Title { get; set; }
        [JsonProperty("username")] public String Username { get; set; }
        [JsonProperty("first_name")] public String FirstName { get; set; }
        [JsonProperty("last_name")] public String LastName { get; set; }
    }
}