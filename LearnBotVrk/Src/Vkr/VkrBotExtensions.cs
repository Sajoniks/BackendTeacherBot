using System.Threading.Tasks;
using LearnBotVrk.botApi;
using LearnBotVrk.Telegram.BotAPI;
using LearnBotVrk.Telegram.BotAPI.Types;
using LearnBotVrk.Telegram.Types;
using LearnBotVrk.Vkr.API;

namespace LearnBotVrk.Vkr
{
    public static class VkrBotExtensions
    {
        public static Task<Message> CreateQuiz(this IBot bot, Chat chat, CourseQuiz.Question question, int openPeriod = 25)
        {
            return bot.SendPollAsync(
                chat
                , question: question.Text
                , options: question.OptionStrings
                , correctOption: question.CorrectOptionId - 1
                , pollType: Poll.Type.Quiz
                , anonymous: true
                , allowMultiple: false
                , openPeriod: openPeriod
                , explanation: question.CorrectOption
                , protectContent: true
            );
        }
    }
}