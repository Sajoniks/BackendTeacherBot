using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using LearnBotVrk.botApi;
using LearnBotVrk.Telegram.BotAPI.Types;
using LearnBotVrk.Telegram.Types;
using Newtonsoft.Json;

namespace LearnBotVrk.Vkr.API
{
    public static class TeachApi
    {
        internal class TeachApiResponse<T>
        {
            [JsonProperty("data")] public T Data { get; set; }
            [JsonProperty("is_ok")] public bool IsOK { get; set; }
            [JsonProperty("response_code")] public int ResponseCode { get; set; }
        }

        public class TeachApiConfigurableRequest : VkrRequest
        {
            private static readonly string _basePath = Environment.GetEnvironmentVariable("api");
            public TeachApiConfigurableRequest(long user) : base(_basePath)
            {
                Append(user.ToString());
            }
        }

        private static async Task<T> GetResponseAndResult<T>(this VkrRequest request)
        {
            var response = await request.GetJsonAsync<TeachApiResponse<T>>();
            return response.IsOK ? response.Data : default;
        }

        private static async Task<T> PostResponseGetJson<T>(this VkrRequest request)
        {
            var response = await request.GetJsonAsync<TeachApiResponse<T>>(WebRequestMethods.Http.Post);
            return response.IsOK ? response.Data : default;
        }

        private static TeachApiConfigurableRequest GetTeachApiRequest(User user)
        {
            return new TeachApiConfigurableRequest(user.Id);
        }

        private static VkrRequest ConfigureCourse(this VkrRequest request) =>
            request
                .Append("courses").Append("{courseId}");

        private static VkrRequest ConfigureChapter(this VkrRequest request) =>
            request
                .ConfigureCourse()
                .Append("chapter").Append("{chapterId}");

        private static VkrRequest ConfigureParagraph(this VkrRequest request) =>
            request
                .ConfigureChapter()
                .Append("page").Append("{page}");

        private static VkrRequest WithCourseParameters(this VkrRequest req, string courseId) =>
            req
                .AppendParameters(new
                {
                    courseId
                });

        private static VkrRequest WithChapterParameters(this VkrRequest req, CourseChapter chapter) =>
            req
                .AppendParameters(new
                {
                    courseId = chapter.CourseId,
                    chapterId = chapter.Id
                });

        private static VkrRequest WithParagraphParameters(this VkrRequest req, CourseParagraph paragraph) =>
            req
                .AppendParameters(new
                {
                    courseId = paragraph.CourseId,
                    chapterId = paragraph.ChapterId,
                    page = paragraph.Id
                });

        private static VkrRequest WithQuizParameters(this VkrRequest req, CourseQuiz quiz) =>
            req
                .AppendParameters(new
                {
                    courseId = quiz.CourseId,
                    chapterId = quiz.ChapterId
                });

        private static TeachApiConfigurableRequest GetTeachApiRequest(long userId)
        {
            return new TeachApiConfigurableRequest(userId);
        }


        public static class Users
        {
            public static Task<bool> RegisterUser(Contact contact) =>
                GetTeachApiRequest(contact.UserId)
                    .Append("register")
                    .WithParameters(new
                    {
                        first_name = contact.FirstName,
                        last_name = contact.LastName,
                        phone_number = contact.PhoneNumber
                    }).PostResponseGetJson<bool>();

            public static Task<Collection<UserProfile.Progression>> GetProfile(User user) =>
                GetTeachApiRequest(user)
                    .Append("profile")
                    .GetResponseAndResult<Collection<UserProfile.Progression>>();

            public static Task<bool> IsRegistered(User user) =>
                GetTeachApiRequest(user).GetResponseAndResult<bool>();
        }


        public static class Courses
        {
            public static Task<Collection<Course>> GetCoursesAsync(User user) =>
                GetTeachApiRequest(user)
                    .Append("courses")
                    .GetResponseAndResult<Collection<Course>>();

            public static Task<Course> GetCourseAsync(User user, string courseId) =>
                GetTeachApiRequest(user)
                    .ConfigureCourse()
                    .WithCourseParameters(courseId)
                    .GetResponseAndResult<Course>();

            public static Task<string> GetParagraphTextAsync(User user, CourseParagraph paragraph) =>
                GetTeachApiRequest(user)
                    .ConfigureParagraph()
                    .WithParagraphParameters(paragraph)
                    .GetResponseAndResult<string>();

            public static Task<bool> MakeProgression(User user, CourseParagraph paragraph) =>
                GetTeachApiRequest(user)
                    .ConfigureChapter()
                    .WithParagraphParameters(paragraph)
                    .Append("complete")
                    .PostResponseGetJson<bool>();

            public static Task<bool> MakeQuizProgression(User user, CourseQuiz quiz) =>
                GetTeachApiRequest(user)
                    .ConfigureChapter()
                    .WithQuizParameters(quiz)
                    .Append("completeQuiz")
                    .PostResponseGetJson<bool>();

            public static Task<CourseQuiz> GetQuizAsync(User user, CourseChapter chapter) =>
                GetTeachApiRequest(user)
                    .ConfigureChapter()
                    .WithChapterParameters(chapter)
                    .Append("quiz")
                    .GetResponseAndResult<CourseQuiz>();
        }
    }
}