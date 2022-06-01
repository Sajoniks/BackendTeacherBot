using System.Threading.Tasks;
using LearnBotVrk.Telegram.BotAPI;
using LearnBotVrk.Telegram.BotAPI.Types.ReplyMarkup;
using LearnBotVrk.Vkr.API;

namespace LearnBotVrk.Vkr.Windows
{
    public class RegistrationWindow : BotWindow
    {
        public RegistrationWindow() : base(
            "Отлично! Чтобы продолжить, нужно зарегистрироваться. Ничего страшного, этот номер будет использоваться только как идентификатор.")
        {
            CreateOption(Option.ContactOption("Отправить контакты"), OnContactReceived);
        }

        private async Task<Reply> OnContactReceived(UpdateContext arg)
        {
            var bot = arg.Bot;
            var message = arg.Update.Message;
            var chat = message.Chat;
            var contact = message.Contact;

            if (await TeachApi.Users.RegisterUser(contact))
            {
                await bot.SendMessageAsync(chat, "Успешная регистрация", ReplyKeyboardRemove.RemoveKeyboard());
                FinishWindow(1);
                return Reply.Handled();
            }

            await bot.SendMessageAsync(chat, "Не удалось зарегистрироваться", ReplyKeyboardRemove.RemoveKeyboard());
            FinishWindow(0);
            return Reply.Unhandled();
        }
    }
}