using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LearnBotVrk.Telegram.BotAPI;
using LearnBotVrk.Telegram.BotAPI.Types.ReplyMarkup;
using LearnBotVrk.Telegram.Types;

namespace LearnBotVrk.Vkr.API
{
    public class CourseParagraphReader
    {
        private static readonly int MaxPageLength = 500;

        private int _curPage;
        private string[] _pages;

        public string CurrentPage => _pages[_curPage];

        public bool NextPage()
        {
            var nextPage = Math.Min(_curPage + 1, _pages.Length);
            if (_curPage != nextPage)
            {
                _curPage = nextPage;
                return true;
            }

            return false;
        }

        public bool PrevPage()
        {
            var prevPage = Math.Max(_curPage - 1, 0);
            if (_curPage != prevPage)
            {
                _curPage = prevPage;
                return true;
            }

            return false;
        }

        public IReplyMarkup CreateMarkup()
        {
            var buttons = new List<InlineKeyboardMarkup.Button>();
            var markupBuilder = new InlineKeyboardMarkup.Builder();

            if (_curPage - 1 >= 0)
            {
                buttons.Add(new InlineKeyboardMarkup.Button() { Text = "Назад ◀", CallbackData = "readerPrev" } );
            }
            
            if (_curPage + 1 <= _pages.Length - 1)
            {
                buttons.Add(new InlineKeyboardMarkup.Button() { Text = "Далее ▶", CallbackData = "readerNext" } );
            }

            return markupBuilder.Row(buttons.ToArray()).Build();
        }

        public static async Task<CourseParagraphReader> CreateReaderAsync(User user, CourseParagraph paragraph)
        {
            return new CourseParagraphReader(await TeachApi.Courses.GetParagraphTextAsync(user, paragraph));
        }
        
        private CourseParagraphReader(string content)
        {
            var size = content.Length;

            var indents = content.Split(new []{ "\n\n", "\r\n\r\n" }, StringSplitOptions.RemoveEmptyEntries);
            {
                List<string> tempPages = new List<string>();
                int currentSize = 0;
                StringBuilder sb = new StringBuilder();
                
                foreach (var indent in indents)
                {
                    currentSize += indent.Length;
                    sb.Append(indent);
                    
                    if (currentSize >= MaxPageLength)
                    {
                        tempPages.Add(sb.ToString());
                        sb = new StringBuilder();
                        currentSize = 0;
                    }
                }

                _pages = tempPages.ToArray();
                _curPage = 0;
            }
        }
    }
}