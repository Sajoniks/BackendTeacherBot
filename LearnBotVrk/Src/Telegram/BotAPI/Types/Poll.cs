using System;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace LearnBotVrk.Telegram.BotAPI.Types
{
    public class Poll
    {
        public class PollOption
        {
            [JsonProperty("text")] public String text { get; set; }
            [JsonProperty("voter_count")] public int VotesCount { get; set; }
        }
        
        public enum Type
        {
            Quiz,
            Regular
        }
        
        [JsonProperty("id")] public long Id { get; set; }
        [JsonProperty("question")] public String Question { get; set; }
        [JsonProperty("options")] public PollOption[] Options { get; set; }
        [JsonProperty("total_voter_count")] public int TotalVoterCount { get; set; }
        [JsonProperty("is_closed")] public bool IsClosed { get; set; }
        [JsonProperty("is_anonymous")] public bool IsAnonymous { get; set; }
        [JsonProperty("type")] [JsonConverter(typeof(StringEnumConverter))] Type PollType { get; set; }
        [JsonProperty("allows_multiple_answers")] public bool AllowsMultipleAnswers { get; set; }
    }
}