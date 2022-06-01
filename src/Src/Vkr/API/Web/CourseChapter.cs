using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.Serialization;
using Newtonsoft.Json;

namespace LearnBotVrk.Vkr.API
{
    public class CourseChapter
    {
        [JsonProperty("id")] public int Id { get; private set; }
        [JsonProperty("courseId")] public String CourseId { get; private set; }
        [JsonProperty("title")] public String Title { get; private set; }
        [JsonProperty("paragraphs")] public Collection<CourseParagraph> Paragraphs { get; private set; }
        [JsonProperty("completed")] public bool IsCompleted { get; private set; }

        public CourseParagraph GetParagraph(string id)
        {
            return Paragraphs.FirstOrDefault(p => p.Id == id);
        }
    }
}