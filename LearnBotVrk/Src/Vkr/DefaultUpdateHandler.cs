using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using LearnBotVrk.botApi;
using LearnBotVrk.Telegram.BotAPI;
using LearnBotVrk.Telegram.BotAPI.Types;
using LearnBotVrk.Telegram.BotAPI.Types.ReplyMarkup;
using LearnBotVrk.Telegram.Types;

namespace LearnBotVrk.Vkr
{
    public class DefaultUpdateHandler : IUpdateHandler
    {
        private Window _welcomeWindow;
        private Window _registrationWindow;
        private Window _generalWindow;
        private WindowMediator _windowMediator;


        public DefaultUpdateHandler()
        {
            _windowMediator = new WindowMediator();

            _generalWindow = new Window.Builder("С чего начнем?")
                .CreateActionOption(Window.Option.TextOption("Доступные курсы"), context =>
                {
                    // gather courses for user...
                    return context.Bot.CreateBotMessageResponse("Доступные курсы:");
                })
                .CreateActionOption(Window.Option.TextOption("Профиль"), context => null)
                .Build();

            _registrationWindow = new Window.Builder("Отлично! Чтобы продолжить, нужно зарегистрироваться. Ничего страшного, этот номер будет использоваться только как идентификатор.")
                .CreateActionOption(Window.Option.ContactOption("Отправить номер"), context =>
                {
                    // register user...
                    
                    return new Window.MergedResponse(
                        context.Bot.CreateBotMessageResponse("Успешная регистрация"),
                        _windowMediator.CreateWindowOpenResponse(_generalWindow)
                    );
                })
                .Build();

            _welcomeWindow = new Window.Builder("Привет! Я бот, который может помочь начать путь в Backend разработке!")
                .CreateActionOption(Window.Option.TextOption("Регистрация"), context => _windowMediator.CreateWindowOpenResponse(_registrationWindow))
                .Build();


            _windowMediator.Mediate(new WindowMediator.Action("/start", _generalWindow));
        }


        public async Task OnReceive(IBot bot, Update update, CancellationToken token)
        {
            if (update.Type == Update.Types.Message)
            {
                var message = update.Message;
                var chat = message.Chat;
                var user = message.From;

                var ctx = Context.Get();
                ctx.Bot = bot;
                ctx.LastMessage = message;
                ctx.Chat = chat;
                ctx.From = user;
                ctx.Update = update;
                
                // Command
                if (message.Type == Message.Types.Text && message.Text.StartsWith("/"))
                {
                    await _windowMediator.HandleCommandAsync(message.Text, ctx);
                }
                else
                {
                    await _windowMediator.HandleUpdateAsync(ctx);
                }
            }
        }

        public async Task OnException(IBot bot, Exception exception, CancellationToken token)
        {
            Console.WriteLine(exception.Message);
        }
    }
}