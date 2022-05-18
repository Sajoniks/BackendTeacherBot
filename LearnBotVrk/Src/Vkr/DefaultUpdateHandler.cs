using System;
using System.Threading;
using System.Threading.Tasks;
using LearnBotVrk.botApi;
using LearnBotVrk.Telegram.BotAPI;
using LearnBotVrk.Telegram.BotAPI.Types;

namespace LearnBotVrk.Vkr
{
    public class DefaultUpdateHandler : IUpdateHandler
    {
        public DefaultUpdateHandler()
        {
        }
        
        public async Task OnReceive(IBot bot, Update update, CancellationToken token)
        {
            
        }

        public async Task OnException(IBot bot, Exception exception, CancellationToken token)
        {
            Console.WriteLine(exception.Message);
        }
    }
}