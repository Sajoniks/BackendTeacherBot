using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.Serialization;
using Newtonsoft.Json;

namespace LearnBotVrk.Vkr.API
{
    public class CourseChapter 
    {
        [JsonProperty("id")] public int Id { get; set; }
        [JsonIgnore] public Course Course { get; set; }
        [JsonProperty("title")] public String Title { get; set; }
        [JsonProperty("paragraphs")] public Collection<CourseParagraph> Paragraphs { get; set; }
    
        
        public CourseParagraph GetParagraph(string id)
        {
            return Paragraphs.FirstOrDefault(p => p.Id == id);
        }
    }
}