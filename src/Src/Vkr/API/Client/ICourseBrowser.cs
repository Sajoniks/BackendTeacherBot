using System.Threading.Tasks;
using LearnBotVrk.Vkr.Windows;

namespace LearnBotVrk.Vkr.API.Client
{
    public interface ICourseBrowser
    {
        public Task UpdateCourseStatus();

        public bool SetChapter(int chapterId);
        public bool SetParagraph(string paragraphId);

        public Task<ICourseParagraphReader> CreateParagraphReaderAsync();

        public CourseParagraph Paragraph { get; }
        public CourseChapter Chapter { get; }
        public Course Course { get; }
        public ICourseParagraphReader Reader { get; }
    }
}