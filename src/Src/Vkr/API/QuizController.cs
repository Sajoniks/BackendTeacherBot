using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using LearnBotVrk.Telegram.Types;
using LearnBotVrk.Vkr.API.Client;
using LearnBotVrk.Vkr.Windows;

namespace LearnBotVrk.Vkr.API
{
    public class QuizController : ICourseQuizController
    {
        public class QuizTotals : ICourseQuizController.IQuizTotals
        {
            public int CorrectAnswers { get; }
            public int FailedAnswers { get; }
            public int TotalAnswers { get; }
            public Collection<string> FailedParagraphs { get; }

            public QuizTotals(int correctAnswers, int failedAnswers, int totalAnswers,
                Collection<string> failedParagraphs)
            {
                CorrectAnswers = correctAnswers;
                FailedAnswers = failedAnswers;
                TotalAnswers = totalAnswers;
                FailedParagraphs = failedParagraphs;
            }
        }

        public QuizController(CourseQuiz quiz)
        {
            _quiz = quiz;
            _failedParagraphs = new HashSet<string>();
            _questionIdx = 0;
            _failedNum = 0;
        }

        private HashSet<string> _failedParagraphs;
        private CourseQuiz _quiz;
        private int _questionIdx;
        private int _failedNum;

        public bool Completed => _quiz.Questions.Length == _questionIdx;
        public CourseQuiz Quiz => _quiz;
        public CourseQuiz.Question Question => _quiz.Questions[_questionIdx];

        public ICourseQuizController.IQuizTotals Totals => new QuizTotals(
            failedAnswers: _failedNum,
            correctAnswers: _quiz.Questions.Length - _failedNum,
            totalAnswers: _questionIdx,
            failedParagraphs: new Collection<string>(_failedParagraphs.ToList())
        );

        public void RegisterAnswer(string answer)
        {
            if (Question.CorrectOption != answer)
            {
                _failedParagraphs.Add(Question.ParagraphTitle);
                ++_failedNum;
            }

            ++_questionIdx;
        }

        public Task<bool> SavePollTotalsAsync(User user)
        {
            return TeachApi.Courses.MakeQuizProgression(user, _quiz);
        }
    }
}