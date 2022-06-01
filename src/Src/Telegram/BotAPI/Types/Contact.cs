using System;
using Newtonsoft.Json;

namespace LearnBotVrk.Telegram.BotAPI.Types
{
    public class Contact
    {
        [JsonProperty("phone_number")] public String PhoneNumber { get; set; }
        [JsonProperty("first_name")] public String FirstName { get; set; }
        [JsonProperty("last_name")] public String LastName { get; set; }
        [JsonProperty("user_id")] public long UserId { get; set; }
        [JsonProperty("vcard")] public String VCard { get; set; }
    }
}