using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LearnBotVrk.Telegram.BotAPI;
using LearnBotVrk.Telegram.BotAPI.Types;
using LearnBotVrk.Vkr.API;

namespace LearnBotVrk.Vkr.Windows
{
    public class MainWindow : BotWindow
    {
        enum WindowState
        {
            Idle,
            Reading,
            WaitingForChapter,
            WaitingForParagraph,
            Quiz
        }

        private TelegramQuizController _quizController;
        private TelegramCourseBrowser _courseBrowser;
        private UpdateContext _currentContext;
        private WindowState _currentState;

        public MainWindow() : base("С чего начнем?")
        {
            _currentState = WindowState.Idle;

            CreateDefaultLayout();

            // Loads course
            CreateCommand("/", HandleGenericCommand);

            // Handle chapter selection when course browser is active
            CreateUpdateHandler(Update.Types.CallbackQuery, HandleChapterSelection,
                (ctx) => _currentState == WindowState.WaitingForChapter);
            // Handle paragraph selection when course browser is active
            CreateUpdateHandler(Update.Types.CallbackQuery, HandleParagraphSelection,
                (ctx) => _currentState == WindowState.WaitingForParagraph);
            // Handle reader states
            CreateUpdateHandler(Update.Types.CallbackQuery, HandleReaderStateChange,
                (ctx) => _currentState == WindowState.Reading);


            // Handle polls
            CreateUpdateHandler(Update.Types.Poll, HandlePollUpdate, (ctx) => _currentState == WindowState.Quiz);
        }

        private async Task<Reply> HandlePollUpdate(UpdateContext arg)
        {
            var bot = arg.Bot;
            var update = arg.Update;
            var poll = arg.Update.Poll;

            var answ = poll.Options.FirstOrDefault(opt => opt.VotesCount > 0)?.Text;
            _quizController.RegisterAnswer(answ);

            await Task.Delay(3000);
            if (_quizController.Completed)
            {
                await _quizController.SendPollTotalsAsync(bot, _currentContext.Update.Message.From,
                    _currentContext.Update.Message.Chat);
                _currentState = WindowState.Idle;
            }
            else
            {
                await _quizController.SendTelegramPollAsync(bot, _currentContext.Update.Message.Chat);
            }


            return Reply.Handled();
        }

        private async Task<Reply> HandleReaderStateChange(UpdateContext arg)
        {
            var bot = arg.Bot;
            var update = arg.Update;
            var cb = update.CallbackQuery;
            var chat = cb.Message.Chat;

            if (cb.Data == TelegramCourseBrowser.Constants.NextPage)
            {
                _courseBrowser.Reader.NextPage();
                await _courseBrowser.SendReaderAsync(bot, chat);

                return Reply.Handled();
            }

            if (cb.Data == TelegramCourseBrowser.Constants.PrevPage)
            {
                _courseBrowser.Reader.PrevPage();
                await _courseBrowser.SendReaderAsync(bot, chat);

                return Reply.Handled();
            }

            if (cb.Data == TelegramCourseBrowser.Constants.TableOfContents)
            {
                // read till end
                if (!_courseBrowser.Reader.HasNext)
                {
                    await TeachApi.Courses.MakeProgression(cb.From, _courseBrowser.Paragraph);
                }

                await _courseBrowser.UpdateCourseStatus();
                await _courseBrowser.SendParagraphListAsync(bot, chat);
                ;
                _currentState = WindowState.WaitingForParagraph;

                return Reply.Handled();
            }

            return Reply.Unhandled();
        }

        private async Task<Reply> HandleParagraphSelection(UpdateContext arg)
        {
            var bot = arg.Bot;
            var update = arg.Update;
            var cb = update.CallbackQuery;
            var user = cb.From;
            var chat = cb.Message.Chat;

            if (cb.Data == TelegramCourseBrowser.Constants.TableOfContents)
            {
                if (_courseBrowser.Chapter != null)
                {
                    await _courseBrowser.SendTableOfContentsAsync(arg.Bot, cb.Message.Chat);
                    _currentState = WindowState.WaitingForChapter;
                    return Reply.Handled();
                }
            }
            else if (cb.Data == TelegramCourseBrowser.Constants.Quiz)
            {
                await _courseBrowser.DeleteBrowserMessage(bot, cb.Message.Chat);

                var quiz = await TeachApi.Courses.GetQuizAsync(user, _courseBrowser.Chapter);
                _currentState = WindowState.Quiz;

                _quizController = new TelegramQuizController(
                    new QuizController(quiz)
                );

                await _quizController.SendTelegramPollAsync(bot, chat);

                return Reply.Handled();
            }
            else
            {
                var paragraphId = cb.Data;
                if (_courseBrowser.SetParagraph(paragraphId))
                {
                    var courseReaderTask = _courseBrowser.CreateParagraphReaderAsync();
                    await courseReaderTask;

                    if (_courseBrowser.Reader != null)
                    {
                        await _courseBrowser.SendReaderAsync(bot, cb.Message.Chat);
                        _currentState = WindowState.Reading;
                        await bot.AnswerCallbackQuery(cb.Id);
                        return Reply.Handled();
                    }
                }
            }

            return Reply.Unhandled();
        }

        private async Task<Reply> HandleChapterSelection(UpdateContext arg)
        {
            var bot = arg.Bot;
            var update = arg.Update;
            var cb = update.CallbackQuery;

            // goto table of contents
            try
            {
                int chapterId = int.Parse(cb.Data);
                if (_courseBrowser.SetChapter(chapterId))
                {
                    await _courseBrowser.SendParagraphListAsync(bot, cb.Message.Chat);
                    _currentState = WindowState.WaitingForParagraph;
                    return Reply.Handled();
                }
            }
            catch (FormatException e)
            {
                // do something
            }

            await bot.SendMessageAsync(cb.Message.Chat, "Не удалось загрузить главу.");
            return Reply.Unhandled();
        }

        private void CreateDefaultLayout()
        {
            RemoveAllOptions();
            CreateOption(Option.TextOption("Доступные курсы"), ListAvailableCourses);
            CreateOption(Option.TextOption("Профиль"), PreviewProfile);
        }

        protected override async Task OnEnter(UpdateContext ctx)
        {
            _currentContext = ctx;

            // check if user is registered.
            if (await TeachApi.Users.IsRegistered(ctx.Update.Message.From))
            {
                await base.OnEnter(ctx);
            }
            else
            {
                await StartWindow(new WelcomeWindow(), ctx);
            }
        }

        protected override async void OnIntentResult(int resultCode)
        {
            if (resultCode == 1)
            {
                // registered
                var bot = _currentContext.Bot;
                var chat = _currentContext.Update.Message.Chat;

                await bot.SendMessageAsync(chat, "С чего начнем?", GenerateMarkup());
            }
        }

        private async Task<Reply> HandleGenericCommand(UpdateContext arg)
        {
            var message = arg.Update.Message;
            var chat = message.Chat;
            var bot = arg.Bot;

            if (_courseBrowser != null)
            {
                await _courseBrowser.DeleteBrowserMessage(arg.Bot, chat);
                _courseBrowser = null;
            }

            // by default we load course
            var courseBrowserTask = CourseBrowser.CreateCourseBrowserAsync(message.Text.Substring(1), message.From);
            var responseTask = bot.SendMessageAsync(chat, "Загрузка...");


            var responseMessage = await responseTask;
            var courseBrowser = await courseBrowserTask;


            // Loaded
            if (courseBrowser?.Course != null)
            {
                _courseBrowser = new TelegramCourseBrowser(courseBrowser, responseMessage);
                await _courseBrowser.SendTableOfContentsAsync(arg.Bot, chat);
                _currentState = WindowState.WaitingForChapter;
            }
            else
            {
                await arg.Bot.EditMessageTextAsync(responseMessage.Chat, responseMessage.Id,
                    "Не удалось загрузить курс.");
                _currentState = WindowState.Idle;
            }

            return Reply.Handled();
        }

        private async Task<Reply> PreviewProfile(UpdateContext arg)
        {
            var bot = arg.Bot;
            var message = arg.Update.Message;
            var from = message.From;
            var chat = message.Chat;

            var profileTask = TeachApi.Users.GetProfile(from);
            var statusMessage = await bot.SendMessageAsync(chat, "Загрузка...");

            var data = await profileTask;

            var sb = new StringBuilder($"👤 Профиль пользователя @{from.Username}").AppendLine().AppendLine();
            foreach (var progression in data)
            {
                sb.AppendLine($"🔷 Курс \"{progression.Title}\":")
                    .AppendLine($"   ◻ Прогресс: {Math.Round(progression.ProgressPercent * 100)}%")
                    .AppendLine($"   ◻ Пройдено тем: {progression.DoneParagraphs}/{progression.TotalParagraphs}")
                    .AppendLine($"   ◻ Пройдено тестов: {progression.DoneQuizes}/{progression.TotalQuizes}")
                    .AppendLine();
            }

            await bot.EditMessageTextAsync(chat, statusMessage.Id, sb.ToString());

            return Reply.Handled();
        }

        private async Task<Reply> ListAvailableCourses(UpdateContext arg)
        {
            var bot = arg.Bot;
            var message = arg.Update.Message;
            var chat = message.Chat;
            var user = message.From;

            var coursesTask = TeachApi.Courses.GetCoursesAsync(arg.Update.Message.From);
            var statusMessage = await bot.SendMessageAsync(chat, "Загрузка...");
            var courses = (await coursesTask).ToList();

            if (!courses.Any())
            {
                await bot.EditMessageTextAsync(chat, message.Id, "Нет доступных курсов для Ваc.");
            }
            else
            {
                var builder = new StringBuilder("Доступные курсы: ").AppendLine().AppendLine();

                foreach (var c in courses)
                {
                    bool completed = false;

                    builder.Append($"{(completed ? "✅" : "👉")} \"{c.Title}\" - /{c.Id}");
                    builder.AppendLine().AppendLine();
                }

                await bot.EditMessageTextAsync(chat, statusMessage.Id, builder.ToString());
            }

            return Reply.Handled();
        }
    }
}