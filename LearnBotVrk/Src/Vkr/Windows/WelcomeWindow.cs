using System.Threading.Tasks;
using LearnBotVrk.Vkr.API;

namespace LearnBotVrk.Vkr.Windows
{
    public class WelcomeWindow : BotWindow
    {
        public WelcomeWindow() : base("Привет! Я бот, который может помочь начать путь в Backend разработке!")
        {
            CreateOption(Option.TextOption("Регистрация"), HandleRegistrationRequest);
        }

        private async Task<Reply> HandleRegistrationRequest(UpdateContext ctx)
        {
            await StartWindow(new RegistrationWindow(), ctx);
            return Reply.Handled();
        }

        protected override void OnIntentResult(int resultCode)
        {
            FinishWindow(1);
        }
    }
}