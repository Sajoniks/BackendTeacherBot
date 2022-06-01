using System;
using Newtonsoft.Json;

namespace LearnBotVrk.Vkr.API
{
    public class CourseParagraph
    {
        [JsonProperty("id")] public String Id { get; private set; }
        [JsonProperty("courseId")] public String CourseId { get; private set; }
        [JsonProperty("chapterId")] public String ChapterId { get; private set; }
        [JsonProperty("title")] public String Title { get; private set; }
        [JsonProperty("completed")] public bool IsCompleted { get; private set; }
    }
}