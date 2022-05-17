using System;
using Newtonsoft.Json;

namespace LearnBotVrk.Telegram.Types
{
    public class User
    {
        [JsonProperty("id")] public long Id { get; set; }
        [JsonProperty("is_bot")] public bool IsBot { get; set; }
        [JsonProperty("first_name")] public String FirstName { get; set; }
        [JsonProperty("last_name")] public String LastName { get; set; }
        [JsonProperty("username")] public String Username { get; set; }
        [JsonProperty("language_code")] public String LanguageCode { get; set; }
    }
}