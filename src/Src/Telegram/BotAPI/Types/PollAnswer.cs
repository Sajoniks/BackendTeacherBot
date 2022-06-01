using LearnBotVrk.Telegram.Types;
using Newtonsoft.Json;

namespace LearnBotVrk.Telegram.BotAPI.Types
{
    public class PollAnswer
    {
        [JsonProperty("poll_id")] public string PollId { get; set; }
        [JsonProperty("user")] public User User { get; set; }
        [JsonProperty("option_ids")] public int[] OptionIds;
    }
}