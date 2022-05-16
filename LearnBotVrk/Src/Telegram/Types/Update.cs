﻿using Newtonsoft.Json;

namespace LearnBotVrk.Telegram.Types
{
    public class Update
    {
        [JsonProperty("update_id")]
        public int UpdateId { get; set; }
        
        [JsonProperty("message")]
        public Message Message { get; set; }
    }
}