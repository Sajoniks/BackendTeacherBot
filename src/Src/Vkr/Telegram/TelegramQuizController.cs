using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LearnBotVrk.botApi;
using LearnBotVrk.Telegram.BotAPI;
using LearnBotVrk.Telegram.BotAPI.Types;
using LearnBotVrk.Telegram.Types;
using LearnBotVrk.Vkr.API;
using LearnBotVrk.Vkr.API.Client;
using LearnBotVrk.Vkr.Windows;

namespace LearnBotVrk.Vkr
{
    public class TelegramQuizController : ICourseQuizController
    {
        private ICourseQuizController _wrappee;
        private Message _pollMessage;

        public TelegramQuizController(ICourseQuizController controller)
        {
            _wrappee = controller;
        }

        public bool Completed => _wrappee.Completed;
        public CourseQuiz Quiz => _wrappee.Quiz;
        public CourseQuiz.Question Question => _wrappee.Question;
        public ICourseQuizController.IQuizTotals Totals => _wrappee.Totals;

        public void RegisterAnswer(string answer)
        {
            _wrappee.RegisterAnswer(answer);
        }

        public Task<bool> SavePollTotalsAsync(User user)
        {
            return _wrappee.SavePollTotalsAsync(user);
        }

        public async Task<Message> SendTelegramPollAsync(IBot bot, Chat chat)
        {
            if (_pollMessage != null)
            {
                await bot.DeleteMessageAsync(chat, _pollMessage.Id);
            }

            _pollMessage = await bot.SendPollAsync(chat
                , question: Question.Text
                , options: Question.Options
                , pollType: Poll.Type.Quiz
                , anonymous: true
                , allowMultiple: false
                , explanation: Question.CorrectOption
                , openPeriod: 25
                , correctOption: Question.CorrectOptionNum - 1
            );

            return _pollMessage;
        }

        public async Task<Message> SendPollTotalsAsync(IBot bot, User user, Chat chat)
        {
            Task<bool> saveTotalsTask = null;
            var totals = Totals;

            if (totals.FailedAnswers == 0)
            {
                saveTotalsTask = SavePollTotalsAsync(user);
            }


            StringBuilder sb = new StringBuilder("🎉 Тест пройден! 🎉")
                .AppendLine()
                .AppendLine()
                .AppendLine("💡 Результаты:")
                .AppendLine()
                .AppendLine($"Правильно отвечено: {totals.CorrectAnswers}/{totals.TotalAnswers}")
                .AppendLine();

            if (totals.FailedAnswers > 0)
            {
                sb.AppendLine("🧑‍🏫 Нужно повторить темы:");
                foreach (var paragraph in totals.FailedParagraphs)
                {
                    sb.AppendLine($"👉 {paragraph}");
                }
            }
            else
            {
                sb.AppendLine("Отличная работа!🔥 Продолжаем дальше.");
            }


            await bot.DeleteMessageAsync(chat, _pollMessage.Id);
            _pollMessage = null;

            if (saveTotalsTask != null)
            {
                await saveTotalsTask;
            }

            return await bot.SendMessageAsync(chat, sb.ToString());
        }
    }
}