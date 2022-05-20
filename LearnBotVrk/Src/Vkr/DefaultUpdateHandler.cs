using System;
using System.Threading;
using System.Threading.Tasks;
using LearnBotVrk.botApi;
using LearnBotVrk.Telegram.BotAPI;
using LearnBotVrk.Telegram.BotAPI.Types;
using LearnBotVrk.Vkr.API;
using LearnBotVrk.Vkr.Windows;

namespace LearnBotVrk.Vkr
{
    public class DefaultUpdateHandler : IUpdateHandler
    {
        public DefaultUpdateHandler()
        {
        }
        
        public async Task OnReceive(IBot bot, Update update, CancellationToken token)
        {
            if (update.Type == Update.Types.Message)
            {
                var message = update.Message;
                var text = message.Text;

                if (text != null && text.StartsWith("/"))
                {
                    if (text == "/start")
                    {
                        await BotWindow.Initialize(new MainWindow(), bot, update);
                    }
                    else
                        await BotWindow.GetCurrentWindow().HandleCommand(bot, text, update);
                }
                else
                {
                    await BotWindow.GetCurrentWindow().HandleUpdate(bot, update);
                }
            }
        }

        public async Task OnException(IBot bot, Exception exception, CancellationToken token)
        {
            Console.WriteLine(exception.Message);
        }
    }
}