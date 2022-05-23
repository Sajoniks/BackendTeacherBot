using System;
using YamlDotNet.Serialization;

namespace LearnBotVrk.Vkr.API
{
    public class CourseParagraph
    {
        [YamlIgnore]
        public String Id { get; set; }
        [YamlIgnore]
        public CourseChapter Chapter { get; set; }
        public String Title { get; set; }
    }
}