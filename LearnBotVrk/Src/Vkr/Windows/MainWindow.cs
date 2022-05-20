using System.Threading.Tasks;
using LearnBotVrk.Vkr.API;

namespace LearnBotVrk.Vkr.Windows
{
    public class MainWindow : BotWindow
    {
        private UpdateContext currentContext;
        public MainWindow() : base("С чего начнем?")
        {
            CreateOption(Option.TextOption("Доступные курсы"), ListAvailableCourses);
            CreateOption(Option.TextOption("Профиль"), PreviewProfile);
            
            CreateCommand("/", HandleGenericCommand);
            CreateCommand("/test", HandleTestCommand);
        }

        protected override async Task OnEnter(UpdateContext ctx)
        {
            // check if user is registered.
            if (TeachApi.Users.IsRegistered(ctx.Update.Message.From))
            {
                await base.OnEnter(ctx);
            }
            else
            {
                currentContext = ctx;
                await StartWindow(new WelcomeWindow(), ctx);
            }
        }

        protected override async void OnIntentResult(int resultCode)
        {
            if (resultCode == 1)
            {
                // registered
                await currentContext.SendBotResponse("С чего начнем?", GenerateMarkup());
            }
        }

        private async Task<Reply> HandleTestCommand(UpdateContext arg)
        {
            await arg.SendBotResponse("Test completed.");
            return Reply.Handled();
        }

        private Task<Reply> HandleGenericCommand(UpdateContext arg)
        {
            return Task.FromResult(Reply.Handled());
        }

        private Task<Reply> PreviewProfile(UpdateContext arg)
        {
            return Task.FromResult(Reply.Handled());
        }

        private Task<Reply> ListAvailableCourses(UpdateContext arg)
        {
            return Task.FromResult(Reply.Handled());
        }
    }
}