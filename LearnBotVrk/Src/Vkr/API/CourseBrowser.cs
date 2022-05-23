using System.Threading.Tasks;

namespace LearnBotVrk.Vkr.API
{
    public class CourseBrowser
    {
        private Course _course;

        private CourseChapter _chapter;
        private CourseParagraph _paragraph;
        private CourseParagraphReader _reader;
        
        private CourseBrowser(Course course)
        {
            _course = course;
            _reader = null;
            _chapter = null;
            _paragraph = null;
        }

        public bool SetChapter(int chapter)
        {
            _chapter = _course?.GetCourseChapter(chapter);
            return _chapter != null;
        }

        public bool SetParagraph(string paragraph)
        {
            if (_chapter != null && _chapter.Paragraphs.ContainsKey(paragraph))
            {
                _paragraph = _chapter.Paragraphs[paragraph];
                _reader = new CourseParagraphReader(_paragraph);
            }

            return _paragraph != null;
        }

        public CourseParagraphReader Reader => _reader;
        public CourseChapter Chapter => _chapter;
        public Course Course => _course;
        
        public static async Task<CourseBrowser> CreateCourseBrowserAsync(string id)
        {
            return new CourseBrowser(await TeachApi.Courses.GetCourseAsync(id));
        }
    }
}