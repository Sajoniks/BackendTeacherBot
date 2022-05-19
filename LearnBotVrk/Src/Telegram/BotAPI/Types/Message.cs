using System;
using System.Runtime.Serialization;
using LearnBotVrk.Telegram.Types;
using Newtonsoft.Json;

namespace LearnBotVrk.Telegram.BotAPI.Types
{
    public class Message
    {
        public enum Types
        {
            Text,
            Animation,
            Audio,
            Document,
            Photo,
            Sticker,
            Video,
            VideoNote,
            Voice,
            Contact,
            Dice,
            Game,
            Poll,
            Venue,
            Location
        }

        [OnDeserialized]
        internal void OnDeserialization(StreamingContext context)
        {
            foreach (var rawType in Enum.GetValues(typeof(Types)))
            {
                Types foundType = (Types) rawType;
                switch (foundType)
                {
                    case Types.Animation:
                        break;
                    case Types.Audio:
                        break;
                    case Types.Document:
                        break;
                    case Types.Photo:
                        break;
                    case Types.Sticker:
                        break;
                    case Types.Video:
                        break;
                    case Types.VideoNote:
                        break;
                    case Types.Voice:
                        break;
                    case Types.Contact:
                        if (Contact != null) { 
                            Type = foundType;
                            return;
                        }
                        break;
                    case Types.Dice:
                        break;
                    case Types.Game:
                        break;
                    case Types.Poll:
                        if (Poll != null)
                        {
                            Type = foundType;
                            return;
                        }
                        break;
                    case Types.Venue:
                        break;
                    case Types.Location:
                        break;
                }
            }
        }

        public Types Type { get; set; }
        
        [JsonProperty("message_id")] public long Id { get; set; }
        [JsonProperty("text")] public string Text { get; set; }
        [JsonProperty("from")] public User From { get; set; }
        [JsonProperty("chat")] public Chat Chat { get; set; }
        
        [JsonProperty("poll")] public Poll Poll { get; set; }
        [JsonProperty("contact")] public Contact Contact { get; set; }
    }
}