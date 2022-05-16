using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.Serialization;
using LearnBotVrk.Telegram.BotAPI;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;

namespace LearnBotVrk.Telegram.Types
{
    public class Poll
    {
        public enum PollType
        {
            Quiz,
            Regular
        }
    }
}