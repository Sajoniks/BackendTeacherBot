using System;
using System.Threading;
using System.Threading.Tasks;
using LearnBotVrk.botApi;
using LearnBotVrk.Telegram.BotAPI;
using LearnBotVrk.Telegram.Types;

namespace LearnBotVrk.Vkr
{
    public class DefaultUpdateHandler : IUpdateHandler
    {
        public async Task OnReceive(IBot bot, Update update, CancellationToken token)
        {
            if (update.Message.Text == "poll")
            {
                await bot.SendPollAsync(update.Message.Chat, "Играть идем?", new[] {"Да", "Нет"}, pollType: Poll.PollType.Quiz, correctOption: 0, explanation:"лох");
            }
        }

        public async Task OnException(IBot bot, Exception exception, CancellationToken token)
        {
            System.Console.WriteLine(exception.Message);
        }
    }
}