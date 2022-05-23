using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LearnBotVrk.Telegram.BotAPI;
using LearnBotVrk.Telegram.BotAPI.Types;
using LearnBotVrk.Telegram.Types;
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

        private CourseBrowser _courseBrowser;
        private UpdateContext _currentContext;
        private WindowState _currentState;
        
        private CourseQuiz _activeQuiz;
        private int _quizQuestion;

        private Message _editMessage;

        public MainWindow() : base("С чего начнем?")
        {
            _currentState = WindowState.Idle;
            _activeQuiz = null;
            
            CreateDefaultLayout();

            CreateCommand("/", HandleGenericCommand);
        }

        private void CreateReaderLayout()
        {
            RemoveAllOptions();
            
            CreateOption(Option.TextOption("В оглавление 📃"), OpenCourseContents);
            CreateOption(Option.TextOption("Пройти тест 🚀"), BeginChapterQuiz);
        }

        private Task<Reply> OpenCourseContents(UpdateContext arg)
        {
            return Task.FromResult<Reply>(Reply.Handled());
        }

        private Task<Reply> BeginChapterQuiz(UpdateContext arg)
        {
            return Task.FromResult(Reply.Handled());
        }
        
        
        private void CreateDefaultLayout()
        {
            RemoveAllOptions();
            CreateOption(Option.TextOption("Доступные курсы"), ListAvailableCourses);
            CreateOption(Option.TextOption("Профиль"), PreviewProfile);
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
                _currentContext = ctx;
                await StartWindow(new WelcomeWindow(), ctx);
            }
        }

        protected override async void OnIntentResult(int resultCode)
        {
            if (resultCode == 1)
            {
                // registered
                await _currentContext.SendBotResponse("С чего начнем?", GenerateMarkup());
            }
        }

        protected override async Task<bool> HandleGenericUpdate(UpdateContext arg)
        {
            if (arg.Update.Type == Update.Types.CallbackQuery)
            {
                var bot = arg.Bot;
                var cb = arg.Update.CallbackQuery;
                var message = cb.Message;
                var user = cb.From;
                
                if (_currentState == WindowState.WaitingForChapter)
                {
                    int chapterId = int.Parse(cb.Data);
                    if (_courseBrowser.SetChapter(chapterId))
                    {
                        bool allCompleted = true;
                        // get paragraphs
                        var markupBuilder = new InlineKeyboardMarkup.Builder();
                        foreach (var par in _courseBrowser.Chapter.Paragraphs)
                        {
                            bool completed = await TeachApi.Courses.IsParagraphCompleted(user, par.Value);
                            allCompleted &= completed;
                            
                            markupBuilder.Row(new[]
                            {
                                new InlineKeyboardMarkup.Button()
                                    { Text = $"{(completed ? "✅" : "👉")} {par.Value.Title}", CallbackData = par.Key }
                            });
                        }

                        if (allCompleted)
                        {
                            markupBuilder.Row(new[]
                            {
                                new InlineKeyboardMarkup.Button() { CallbackData = "quiz", Text = "Пройти тест 🚀" }
                            });
                        }
                        
                        _editMessage = await bot.EditMessageTextAsync(_editMessage.Chat, _editMessage.Id,
                            _courseBrowser.Chapter.Title, markupBuilder.Build());
                        _currentState = WindowState.WaitingForParagraph;
                    }
                    else if (_editMessage != null)
                    {
                        await bot.EditMessageTextAsync(_editMessage.Chat, _editMessage.Id,
                            "Не удалось загрузить главу.");
                        _editMessage = null;
                        _currentState = WindowState.Idle;
                    }
                }
                else if (_currentState == WindowState.WaitingForParagraph || _currentState == WindowState.Reading)
                {
                    if (_currentState == WindowState.Reading)
                    {
                        var reader = _courseBrowser.Reader;
                        var chat = _editMessage.Chat;
                    
                        if (cb.Data == "readerPrev")
                        {
                            reader.PrevPage();
                            _editMessage = await bot.EditMessageTextAsync(chat, _editMessage.Id, reader.CurrentPage, reader.CreateMarkup());
                            return true;
                        }

                        if (cb.Data == "readerNext")
                        {
                            reader.NextPage();
                            _editMessage = await bot.EditMessageTextAsync(chat, _editMessage.Id, reader.CurrentPage, reader.CreateMarkup());
                            return true;
                        }
                        
                        await bot.DeleteMessageAsync(_editMessage.Chat, _editMessage.Id);
                        _editMessage = cb.Message;
                    }

                    if (cb.Data == "quiz")
                    {
                        Chat chat = _editMessage.Chat;
                        await bot.DeleteMessageAsync(_editMessage.Chat, _editMessage.Id);

                        _activeQuiz = _courseBrowser.Chapter.GetChapterQuiz();
                        if (_activeQuiz != null)
                        {
                            _currentState = WindowState.Quiz;
                            
                            _quizQuestion = 0;
                            _editMessage = await bot.CreateQuiz(chat, _activeQuiz.Questions[_quizQuestion]);
                            return true;
                        }
                    }
                    
                    string paragraphId = cb.Data;
                    if (_courseBrowser.SetParagraph(paragraphId))
                    {
                        var reader = _courseBrowser.Reader;
                        
                        try {await bot.AnswerCallbackQuery(cb.Id);} catch (Exception e) {}
                        
                        _editMessage = await bot.SendMessageAsync(_editMessage.Chat, reader.CurrentPage, reader.CreateMarkup());
                        _currentState = WindowState.Reading;
                    }
                    else
                    {
                        await bot.EditMessageTextAsync(_editMessage.Chat, _editMessage.Id,
                            "Не удалось загрузить параграф.");
                        _editMessage = null;
                        _currentState = WindowState.Idle;
                    }
                }
            }
            else if (arg.Update.Type == Update.Types.Poll)
            {
                var waitTask = Task.Delay(3000);
                
                var poll = arg.Update.Poll;
                var question = _activeQuiz.Questions[_quizQuestion];

                string selectedOption = null;
                {
                    var options = poll.Options;
                    var selected = options.FirstOrDefault(o => o.VotesCount > 0);
                    selectedOption = selected.Text;
                }
                
                _activeQuiz.RegisterAnswer(_activeQuiz.Questions[_quizQuestion], selectedOption);

                Chat chat = _editMessage.Chat;
                ++_quizQuestion;

                await waitTask;
                await arg.Bot.DeleteMessageAsync(chat, _editMessage.Id);

                if (_activeQuiz.Completed())
                {
                    var totals = _activeQuiz.GetQuizTotals();
                    var builder = new StringBuilder("🎉Тест пройден!🎉")
                        .AppendLine().AppendLine()
                        .AppendLine($"Правильно отвечено: {totals.CorrectAnswers} / {_activeQuiz.Questions.Count}");

                    if (totals.IncorrectAnswers > 0)
                    {
                        builder.AppendLine("📕 Темы которые неплохо повторить:").AppendLine();
                        foreach (var par in totals.FailedParagraphs)
                        {
                            builder.AppendLine($"👉 {_activeQuiz.Chapter.Paragraphs[par].Title}");
                        }
                    }
                    else
                    {
                        builder.AppendLine("🔥Отличная работа! Работаем дальше.");
                    }

                    await arg.Bot.SendMessageAsync(chat, builder.ToString());
                    _currentState = WindowState.Idle;
                }
                else
                {
                    _editMessage = await arg.Bot.CreateQuiz(chat, _activeQuiz.Questions[_quizQuestion]);
                }
            }

            return false;
        }

        private async Task<Reply> HandleGenericCommand(UpdateContext arg)
        {
            // by default we load course
            // /course1
            var courseBrowserTask = CourseBrowser.CreateCourseBrowserAsync(arg.Update.Message.Text);
            var responseMessage = await arg.SendBotResponse("Загрузка...");

            _courseBrowser = await courseBrowserTask;
            // Loaded
            if (_courseBrowser != null && _courseBrowser.Course != null)
            {
                // create keyboard
                var markupBuilder = new InlineKeyboardMarkup.Builder();
                foreach (var chap in _courseBrowser.Course.GetCourseChapters())
                {
                    bool completed = await TeachApi.Courses.IsChapterCompleted(arg.Update.Message.From, chap);
                    
                    markupBuilder.Row(new[]
                    {
                        new InlineKeyboardMarkup.Button()
                            { Text = $"{(completed ? "✅" : "👉")} Глава {chap.Id} - {chap.Title}", CallbackData = chap.Id.ToString() }
                    });
                }

                _editMessage = await arg.Bot.EditMessageTextAsync(responseMessage.Chat, responseMessage.Id,
                    _courseBrowser.Course.Title, markupBuilder.Build());
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

        private Task<Reply> PreviewProfile(UpdateContext arg)
        {
            return Task.FromResult(Reply.Handled());
        }

        private async Task<Reply> ListAvailableCourses(UpdateContext arg)
        {
            var coursesTask = TeachApi.Courses.GetCoursesAsync(arg.Update.Message.From);
            var message = await arg.SendBotResponse("Загрузка...");
            var courses = (await coursesTask).ToList();
            
            if (!courses.Any())
            {
                await arg.EditMessage(message, "Нет доступных курсов для Ваc.");
            }
            else
            {
                var builder = new StringBuilder("Доступные курсы: ").AppendLine().AppendLine();

                foreach (var c in courses)
                {
                    bool completed = await TeachApi.Courses.IsCourseCompleted(arg.Update.Message.From, c);
                    
                    builder.Append($"{(completed ? "✅": "👉")} \"{c.Title}\" - /{c.Id}");
                    builder.AppendLine().AppendLine();
                }

                await arg.EditMessage(message, builder.ToString());
            }
            return Reply.Handled();
        }
    }
}