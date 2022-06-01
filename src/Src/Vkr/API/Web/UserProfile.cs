using Newtonsoft.Json;

namespace LearnBotVrk.Vkr.API
{
    public class UserProfile
    {
        public class Progression
        {
            [JsonProperty("course_id")] public string CourseId { get; private set; }
            [JsonProperty("title")] public string Title { get; private set; }
            [JsonProperty("progress")] public float ProgressPercent { get; private set; }
            [JsonProperty("total_quiz_num")] public int TotalQuizes { get; private set; }
            [JsonProperty("completed_quiz_num")] public int DoneQuizes { get; private set; }
            [JsonProperty("total_pages_num")] public int TotalParagraphs { get; private set; }
            [JsonProperty("completed_pages_num")] public int DoneParagraphs { get; private set; }
        }
    }
}