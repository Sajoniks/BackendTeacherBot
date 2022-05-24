using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using LearnBotVrk.Telegram.Types;
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

            public QuizTotals(int correctAnswers, int failedAnswers, int totalAnswers, Collection<string> failedParagraphs)
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
        }

        private HashSet<string> _failedParagraphs;
        private CourseQuiz _quiz;
        private int _questionIdx;

        public bool Completed => _quiz.Questions.Length == _questionIdx;
        public CourseQuiz Quiz => _quiz;
        public CourseQuiz.Question Question => _quiz.Questions[_questionIdx];

        public ICourseQuizController.IQuizTotals Totals => new QuizTotals(
            failedAnswers: _failedParagraphs.Count,
            correctAnswers: _quiz.Questions.Length - _failedParagraphs.Count,
            totalAnswers: _quiz.Questions.Length,
            failedParagraphs: new Collection<string>(_failedParagraphs.ToList())
        );

        public void RegisterAnswer(string answer)
        {
            if (Question.CorrectOption != answer)
            {
                _failedParagraphs.Add(Question.ParagraphTitle);
            }

            ++_questionIdx;
        }

        public Task<bool> SavePollTotalsAsync(User user)
        {
            return TeachApi.Courses.MakeQuizProgression(user, _quiz);
        }
    }
}