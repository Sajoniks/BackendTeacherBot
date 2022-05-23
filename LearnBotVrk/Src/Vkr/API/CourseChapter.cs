using System;
using System.Collections.Generic;
using YamlDotNet.Serialization;

namespace LearnBotVrk.Vkr.API
{
    public class CourseChapter 
    {
        [YamlIgnore]
        public int Id { get; set; }
        [YamlIgnore]
        public Course Course { get; set; }
        
        public String Title { get; set; }
        public Dictionary<String, CourseParagraph> Paragraphs { get; set; }
    }
}