using System.Threading.Tasks;
using LearnBotVrk.Telegram.BotAPI.Types.ReplyMarkup;
using LearnBotVrk.Vkr.API;

namespace LearnBotVrk.Vkr.Windows
{
    public class RegistrationWindow : BotWindow
    {
        public RegistrationWindow() : base("Отлично! Чтобы продолжить, нужно зарегистрироваться. Ничего страшного, этот номер будет использоваться только как идентификатор.")
        {
            CreateOption(Option.ContactOption("Отправить контакты"), OnContactReceived);
        }

        private async Task<Reply> OnContactReceived(UpdateContext arg)
        {
            var message = arg.Update.Message;
            var contact = message.Contact;

            if (TeachApi.Users.RegisterUser(contact))
            {
                await arg.SendBotResponse("Успешная регистрация", ReplyKeyboardRemove.RemoveKeyboard());
                FinishWindow(1);
                return Reply.Handled();
            }

            await arg.SendBotResponse("Не удалось зарегистрироваться", ReplyKeyboardRemove.RemoveKeyboard());
            FinishWindow(0);
            return Reply.Unhandled();
        }
    }
}