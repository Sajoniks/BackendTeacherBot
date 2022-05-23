using System;
using System.Collections.Generic;
using System.Linq;
using YamlDotNet.Serialization;

namespace LearnBotVrk.Vkr.API
{
    public class CourseQuiz
    {
        public CourseQuiz()
        {
            InvalidAnswers = new List<int>();
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
            
            [YamlIgnore]
            public string CorrectOption
            {
                get => OptionStrings[CorrectOptionId - 1]; 
            }

            [YamlIgnore] public CourseQuiz Quiz { get; set; }
        }

        [YamlMember(Alias = "quiz")]
        public List<Question> Questions { get; set;  }
        
        [YamlIgnore]
        public CourseChapter Chapter { get; set; }


        [YamlIgnore] private int CorrectAnswers { get; set; }
        [YamlIgnore] private List<int> InvalidAnswers { get; set; }

        public void RegisterAnswer(Question question, string answer)
        {
            if (question.CorrectOption == answer)
            {
                ++CorrectAnswers;
            }
            else
            {
                InvalidAnswers.Add(Questions.IndexOf(question));
            }
        }

        public bool Completed()
        {
            return CorrectAnswers + InvalidAnswers.Count == Questions.Count;
        }

        public class Totals
        {
            public int CorrectAnswers { get; set; }
            public int IncorrectAnswers { get; set; }
            
            public List<string> FailedParagraphs { get; set; }
        }

        public Totals GetQuizTotals()
        {
            return new Totals()
            {
                CorrectAnswers = this.CorrectAnswers,
                IncorrectAnswers = this.Questions.Count - this.CorrectAnswers,
                FailedParagraphs = InvalidAnswers.Select(s => Questions[s].ParagraphId).ToList()
            };
        }
    }
}