using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using LearnBotVrk.Telegram.BotAPI.Types;
using LearnBotVrk.Telegram.Types;

namespace LearnBotVrk.Vkr.API
{
    public static class TeachApi
    {
        public class TeachApiConfigurableRequest : Utility.HttpConfigurableRequest
        {
            
        }
        
        private static readonly TimeSpan FakeDelay = TimeSpan.FromMilliseconds(100);
        
        private static readonly List<Course> CoursesList = new List<Course>()
        {
            Resources.GetCourse("course1")
        };
        
        public static class Users
        {
            public static bool RegisterUser(Contact contact)
            {
                // mock...
                return true;
            }

            public static bool IsRegistered(User user)
            {
                return false;
            }
        }

        public static class Courses
        {
            public static async Task<Course> GetCourseAsync(string id)
            {
                await Task.Delay(FakeDelay);
                return CoursesList.FirstOrDefault((c) => c.Id == id.Substring(1));
            }

            public static async Task<bool> IsCourseCompleted(User user, Course course)
            {
                await Task.Delay(FakeDelay);
                return false;
            }
            
            public static async Task<bool> IsChapterCompleted(User user, CourseChapter chapter)
            {
                await Task.Delay(FakeDelay);
                return false;
            }

            public static async Task<bool> IsParagraphCompleted(User user, CourseParagraph paragraph)
            {
                await Task.Delay(FakeDelay);
                return true;
            }
            
            public static async Task<IEnumerable<Course>> GetCoursesAsync(User user)
            {
                await Task.Delay(FakeDelay);
                return CoursesList;
            }
        }
    }
}