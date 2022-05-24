using System.Threading.Tasks;
using LearnBotVrk.Telegram.Types;
using LearnBotVrk.Vkr.Windows;

namespace LearnBotVrk.Vkr.API
{
    public class CourseBrowser : ICourseBrowser
    {
        private Course _course;
        private CourseChapter _chapter;
        private CourseParagraph _paragraph;
        private ICourseParagraphReader _reader;
        
        private User _user;
        
        private CourseBrowser(User user)
        {
            _user = user;
        }

        public async Task UpdateCourseStatus()
        {
            var newCourse = await TeachApi.Courses.GetCourseAsync(_user, _course.Id);

            if (_chapter != null)
            {
                var newChapter = newCourse.Chapters[_chapter.Id - 1];
                _chapter = newChapter;
                
                if (_paragraph != null)
                {
                    var newParagraph = newChapter.GetParagraph(_paragraph.Id);
                    _paragraph = newParagraph;

                    if (_reader != null)
                    {
                        _reader = await CreateParagraphReaderAsync();
                    }
                }
            }
            
            _course = newCourse;
        }

        public virtual bool SetChapter(int chapter)
        {
            _chapter = Course.Chapters[chapter - 1];
            return _chapter != null;
        }

        public virtual bool SetParagraph(string paragraph)
        {
            if (_chapter != null)
            {
                _paragraph = _chapter.GetParagraph(paragraph);
            }
            return _paragraph != null;
        }
        
        public async Task<ICourseParagraphReader> CreateParagraphReaderAsync()
        {
            if (_paragraph != null)
            {
                var content = await TeachApi.Courses.GetParagraphTextAsync(_user, _paragraph);
                _reader = new CourseParagraphReader(content);
                return _reader;
            }

            return null;
        }

        public CourseParagraph Paragraph => _paragraph;
        public ICourseParagraphReader Reader => _reader;
        public CourseChapter Chapter => _chapter;
        public Course Course => _course;
        
        public static async Task<CourseBrowser> CreateCourseBrowserAsync(string id, User user)
        {
            var browser = new CourseBrowser(user)
            {
                _course = await TeachApi.Courses.GetCourseAsync(user, id)
            };

            return browser;
        }
    }
}