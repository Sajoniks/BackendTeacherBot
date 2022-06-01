using System;
using System.Threading;
using System.Threading.Tasks;
using LearnBotVrk.botApi;
using LearnBotVrk.Telegram.BotAPI.Types;
using LearnBotVrk.Telegram.Types;

namespace LearnBotVrk.Telegram.BotAPI
{
    public interface IUpdateHandler
    {
        Task OnReceive(IBot bot, Update update, CancellationToken token);
        Task OnException(IBot bot, Exception exception, CancellationToken token);
    }
}