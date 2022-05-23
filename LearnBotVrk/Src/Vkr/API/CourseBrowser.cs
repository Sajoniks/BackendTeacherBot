using System.Threading.Tasks;
using LearnBotVrk.Telegram.Types;

namespace LearnBotVrk.Vkr.API
{
    public class CourseBrowser
    {
        private Course _course;

        private CourseChapter _chapter;
        private CourseParagraph _paragraph;
        private CourseParagraphReader _reader;
        private User _user;
        
        private CourseBrowser(User user, Course course)
        {
            _course = course;
            _reader = null;
            _chapter = null;
            _paragraph = null;
            _user = user;
        }

        public bool SetChapter(int chapter)
        {
            _chapter = Course.Chapters[chapter - 1];
            return _chapter != null;
        }

        public async Task<bool> SetParagraphAsync(string paragraph)
        {
            if (_chapter != null)
            {
                _paragraph = _chapter.GetParagraph(paragraph);
                if (_paragraph != null)
                {
                    _reader = await CourseParagraphReader.CreateReaderAsync(_user, _paragraph);
                }
            }
            return _paragraph != null;
        }

        public CourseParagraphReader Reader => _reader;
        public CourseChapter Chapter => _chapter;
        public Course Course => _course;
        
        public static async Task<CourseBrowser> CreateCourseBrowserAsync(string id, User user)
        {
            return new CourseBrowser(user, await TeachApi.Courses.GetCourseAsync(user, id.Substring(1)));
        }
    }
}