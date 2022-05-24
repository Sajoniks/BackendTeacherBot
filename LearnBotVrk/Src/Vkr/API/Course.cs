using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Runtime.Serialization;
using Newtonsoft.Json;

namespace LearnBotVrk.Vkr.API
{
    public class Course
    {
        [JsonProperty("id")] public string Id { get; set; }
        [JsonProperty("title")] public string Title { get; set; }
        [JsonProperty("chapters")] public Collection<CourseChapter> Chapters { get; set; }
    }
}