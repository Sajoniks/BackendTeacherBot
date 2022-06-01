using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LearnBotVrk.Telegram.BotAPI;
using LearnBotVrk.Telegram.BotAPI.Types.ReplyMarkup;
using LearnBotVrk.Telegram.Types;
using LearnBotVrk.Vkr.API.Client;
using LearnBotVrk.Vkr.Windows;

namespace LearnBotVrk.Vkr.API
{
    public class CourseParagraphReader : ICourseParagraphReader
    {
        private static readonly int MaxPageLength = 500;

        private int _curPage;
        private string[] _pages;

        public string CurrentPageContent => _pages[_curPage];
        public bool HasPrevious => _curPage > 0;
        public bool HasNext => _curPage < _pages.Length - 1;
        public int CurrentPage => _curPage + 1;
        public int PagesCount => _pages.Length;

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

        public CourseParagraphReader(string content)
        {
            var size = content.Length;

            var indents = content.Split(new[] { "\n\n", "\r\n\r\n" }, StringSplitOptions.RemoveEmptyEntries);
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