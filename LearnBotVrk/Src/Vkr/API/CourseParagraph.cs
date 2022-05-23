using System;
using Newtonsoft.Json;

namespace LearnBotVrk.Vkr.API
{
    public class CourseParagraph
    {
        [JsonProperty("id")] public String Id { get; set; }
        [JsonProperty("title")] public String Title { get; set; }
        [JsonProperty("completed")] public bool IsCompleted { get; set; }
        [JsonIgnore] public CourseChapter Chapter { get; set; }
    }
}