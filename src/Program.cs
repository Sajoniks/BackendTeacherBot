using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;
using LearnBotVrk.Telegram.BotAPI;
using LearnBotVrk.Vkr;
using LearnBotVrk.Vkr.API;

namespace LearnBotVrk
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var token = Environment.GetEnvironmentVariable("token");
            Bot bot = new Bot(token);

            bot.StartPolling(new DefaultUpdateHandler(), CancellationToken.None);
        }
    }
}