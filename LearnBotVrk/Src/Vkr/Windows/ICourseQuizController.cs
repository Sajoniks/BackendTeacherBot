using System.Collections.ObjectModel;
using System.Threading.Tasks;
using LearnBotVrk.Telegram.Types;
using LearnBotVrk.Vkr.API;

namespace LearnBotVrk.Vkr.Windows
{
    public interface ICourseQuizController
    {
        public interface IQuizTotals
        {
            public int CorrectAnswers { get; }
            public int FailedAnswers { get; }
            public int TotalAnswers { get; }
            
            public Collection<string> FailedParagraphs { get; }
        }
        
        public bool Completed { get; }
        public CourseQuiz Quiz { get; }
        public CourseQuiz.Question Question { get; }
        public IQuizTotals Totals { get; }

        public void RegisterAnswer(string answer);
        public Task<bool> SavePollTotalsAsync(User user);
    }
}