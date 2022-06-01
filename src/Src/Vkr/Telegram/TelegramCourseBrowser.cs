using System;
using System.Linq;
using System.Threading.Tasks;
using LearnBotVrk.botApi;
using LearnBotVrk.Telegram.BotAPI;
using LearnBotVrk.Telegram.BotAPI.Types;
using LearnBotVrk.Telegram.BotAPI.Types.ReplyMarkup;
using LearnBotVrk.Telegram.Types;
using LearnBotVrk.Vkr.API;
using LearnBotVrk.Vkr.API.Client;
using LearnBotVrk.Vkr.Windows;

namespace LearnBotVrk.Vkr
{
    public class TelegramCourseBrowser : ICourseBrowser
    {
        public static class Constants
        {
            public static readonly string Quiz = "courseQuiz";
            public static readonly string TableOfContents = "tableOfContents";
            public static readonly string NextPage = "nextPage";
            public static readonly string PrevPage = "prevPage";
        }

        private ICourseBrowser _wrappee;
        private Message _browserMessage;

        public Message BrowserMessage => _browserMessage;

        public async Task<bool> DeleteBrowserMessage(IBot bot, Chat chat)
        {
            try
            {
                return await bot.DeleteMessageAsync(chat, _browserMessage.Id);
            }
            catch (Exception e)
            {
                return false;
            }
        }

        public async Task<Message> SendReaderAsync(IBot bot, Chat chat)
        {
            var builderRow = new InlineKeyboardMarkup.Builder().Row();
            if (Reader.HasPrevious)
            {
                builderRow.Add(
                    new InlineKeyboardMarkup.Button()
                    {
                        Text = "⬅ Назад",
                        CallbackData = Constants.PrevPage
                    }
                );
            }

            builderRow.Add(
                new InlineKeyboardMarkup.Button()
                {
                    Text = $"{Reader.CurrentPage}/{Reader.PagesCount}",
                    CallbackData = "---"
                }
            );

            if (Reader.HasNext)
            {
                builderRow.Add(
                    new InlineKeyboardMarkup.Button()
                    {
                        Text = "Вперед ➡",
                        CallbackData = Constants.NextPage
                    }
                );
            }

            builderRow.ToBuilder().Row(
                new InlineKeyboardMarkup.Button()
                {
                    Text = "Завершить ☑",
                    CallbackData = Constants.TableOfContents
                }
            );

            return await bot.EditMessageTextAsync(chat, _browserMessage.Id, Reader.CurrentPageContent,
                builderRow.ToBuilder().Build());
        }

        public async Task<Message> SendTableOfContentsAsync(IBot bot, Chat chat)
        {
            var builder = new InlineKeyboardMarkup.Builder();
            foreach (var chapter in Course.Chapters)
            {
                bool completedChapter = chapter.IsCompleted;
                builder.Row(
                    new InlineKeyboardMarkup.Button()
                    {
                        Text = $"{(completedChapter ? "✅" : "👉")} Глава {chapter.Id} - {chapter.Title}",
                        CallbackData = chapter.Id.ToString()
                    }
                );
            }

            _browserMessage = await bot.EditMessageTextAsync(chat, _browserMessage.Id, Course.Title, builder.Build());
            return _browserMessage;
        }

        public async Task<Message> SendParagraphListAsync(IBot bot, Chat chat)
        {
            if (Chapter != null)
            {
                var builder = new InlineKeyboardMarkup.Builder();
                bool completedChapter = true;
                foreach (var paragraph in Chapter.Paragraphs)
                {
                    bool completedParagraph = paragraph.IsCompleted;
                    completedChapter &= completedParagraph;

                    builder.Row(
                        new InlineKeyboardMarkup.Button()
                        {
                            Text = $"{(completedParagraph ? "✅" : "👉")} {paragraph.Title}",
                            CallbackData = paragraph.Id
                        }
                    );
                }

                if (completedChapter)
                {
                    builder.Row(
                        new InlineKeyboardMarkup.Button()
                        {
                            Text = "Пройти тест 🚀",
                            CallbackData = Constants.Quiz
                        }
                    );
                }

                builder.Row(
                    new InlineKeyboardMarkup.Button()
                    {
                        Text = "В оглавление ⬅",
                        CallbackData = Constants.TableOfContents
                    }
                );

                _browserMessage =
                    await bot.EditMessageTextAsync(chat, _browserMessage.Id, $"Глава: \"{Chapter.Title}\"",
                        builder.Build());
            }

            return _browserMessage;
        }

        public TelegramCourseBrowser(ICourseBrowser browser, Message message)
        {
            _wrappee = browser;
            _browserMessage = message;
        }

        public Task UpdateCourseStatus()
        {
            return _wrappee.UpdateCourseStatus();
        }

        public bool SetChapter(int chapterId)
        {
            return _wrappee.SetChapter(chapterId);
        }

        public bool SetParagraph(string paragraphId)
        {
            return _wrappee.SetParagraph(paragraphId);
        }

        public Task<ICourseParagraphReader> CreateParagraphReaderAsync()
        {
            return _wrappee.CreateParagraphReaderAsync();
        }

        public CourseParagraph Paragraph => _wrappee.Paragraph;
        public CourseChapter Chapter => _wrappee.Chapter;
        public Course Course => _wrappee.Course;
        public ICourseParagraphReader Reader => _wrappee.Reader;
    }
}