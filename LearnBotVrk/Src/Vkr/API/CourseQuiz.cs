using System;
using System.Collections.Generic;
using System.Linq;

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

            public string Text { get; set; }
            

            public String ParagraphId { get; set; }

   
            public string[] OptionStrings { get; set; }
            

            public int CorrectOptionId { get; set; }
            
 
            public string CorrectOption
            {
                get => OptionStrings[CorrectOptionId - 1]; 
            }

            public CourseQuiz Quiz { get; set; }
        }


        public List<Question> Questions { get; set;  }
        
      
        public CourseChapter Chapter { get; set; }


       private int CorrectAnswers { get; set; }
      private List<int> InvalidAnswers { get; set; }

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