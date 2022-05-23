using System;
using System.Threading;
using LearnBotVrk.Telegram.BotAPI;
using LearnBotVrk.Vkr;
using LearnBotVrk.Vkr.API;

namespace LearnBotVrk
{
    class Program
    {
        static void Main(string[] args)
        {
            var course = Resources.GetCourse("course1");
            var chap = course.GetCourseChapter(1);
            var quiz = chap.GetChapterQuiz();
            
            var token = Environment.GetEnvironmentVariable("token");
            Bot bot = new Bot(token);

            bot.StartPolling(new DefaultUpdateHandler(), CancellationToken.None);
        }
    }
}