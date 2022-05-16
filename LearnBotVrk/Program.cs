using System;
using System.Threading;
using LearnBotVrk.botApi;
using LearnBotVrk.Telegram.BotAPI;
using LearnBotVrk.Vkr;

namespace LearnBotVrk
{
    class Program
    {
        static void Main(string[] args)
        {
            var token = Environment.GetEnvironmentVariable("token");
            Bot bot = new Bot(token);
            
            bot.StartPolling(new DefaultUpdateHandler(), CancellationToken.None);
        }
    }
}