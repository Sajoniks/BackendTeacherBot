using YamlDotNet.Serialization;

namespace LearnBotServer.API
{
    public class CourseQuiz
    {
        public CourseQuiz()
        {
      
        }
        
        public class Question
        {
            [YamlMember(Alias = "text")]
            public string Text { get; set; }
            [YamlMember(Alias = "page")]
            public String ParagraphId { get; set; }
            [YamlMember(Alias = "opts")]
            public string[] OptionStrings { get; set; }
            [YamlMember(Alias = "correct")]
            public int CorrectOptionId { get; set; }
            [YamlIgnore] public CourseQuiz Quiz { get; set; }
        }

        [YamlMember(Alias = "quiz")] public List<Question> Questions { get; set;  }
        [YamlIgnore] public CourseChapter Chapter { get; set; }
    }
}