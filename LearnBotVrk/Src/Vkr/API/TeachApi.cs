using System.Collections.Generic;
using LearnBotVrk.Telegram.BotAPI.Types;
using LearnBotVrk.Telegram.Types;

namespace LearnBotVrk.Vkr.API
{
    public static class TeachApi
    {
        private static readonly List<Course> CoursesList = new List<Course>()
        {
            new Course() { Id = "course1", Title = "Введение в Backend"},
            new Course() { Id = "course2", Title = "Микросервисы на платформе .NET"}
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
            public static IEnumerable<Course> GetCourses(User user)
            {
                return CoursesList;
            }
        }
    }
}