using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
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
        
        public class TeachApiConfigurableRequest : Utility.HttpConfigurableRequest
        {
            private List<string> _formats;
            private User _user;
            public TeachApiConfigurableRequest(User user, string methodName) : base("https://localhost:7079/api/", methodName)
            {
                _user = user;
                _formats = new List<string>();
            }

            public override Task<T> GetResponseAndResult<T>(string methodType = "GET")
            {
                var response = GetResponse<TeachApiResponse<T>>(methodType);
                return Task.FromResult(response.IsOK ? response.Data : default);
            }

            protected override string BuildApiRequestUrl(StringBuilder sb)
            {
                if (_user != null)
                {
                    sb.Append(_user.Id).Append('/');
                }

                var formatTemplate = MethodName;
                {
                    int idx = 0;
                    foreach (var format in _formats)
                    {
                        formatTemplate = formatTemplate.Replace(format, idx.ToString());
                        ++idx;
                    }
                }

                var parms = _formats.Select(s =>
                {
                    int idx = Parameters.IndexOf(s);
                    return Values[idx];
                }).ToArray<object>();
                
                var formatMethodName = String.Format(formatTemplate, parms);

                foreach (var t in _formats)
                {
                    if (Parameters.Contains(t))
                    {
                        int idx = Parameters.IndexOf(t);
                        Parameters[idx] = null;
                        Values[idx] = null;
                    }
                }

                sb.Append(formatMethodName);
                
                AppendParameters(sb);
                return sb.ToString();
            }

            public TeachApiConfigurableRequest PushParameters(params string[] parms)
            {
                Parameters.AddRange(parms);
                foreach (var s in parms)
                {
                    Values.Add(null);
                }

                return this;
            }

            protected override bool ServerCertificateValidationCallback(object sender, X509Certificate certificate,
                X509Chain chain,
                SslPolicyErrors sslpolicyerrors) => true;
            
            public static TeachApiConfigurableRequest FromMethod(MethodBase methodBase, User user)
            {
                var req = new TeachApiConfigurableRequest(user, Utility.ApiMethod.Resolve(methodBase));

                {
                    bool open = false;
                    int start = 0;
                    int end = 0;
                    for(int i = 0; i < req.MethodName.Length; ++i)
                    {
                        var ch = req.MethodName[i];
                        
                        if (ch == '{')
                        {
                            start = i;
                            open = true;
                        }
                        else if (ch == '}' && open)
                        {
                            open = false;
                            end = i;
                            req._formats.Add(req.MethodName.Substring(start + 1, (end - start - 1)));
                        }
                    }
                }

                foreach (var param in methodBase.GetParameters())
                {
                    var name = Utility.ApiParam.Resolve(param);
                    if (name != null)
                    {
                        req.Parameters.Add(name);
                        req.Values.Add(null);
                    }
                }

                return req;
            }
        }

        public static TeachApiConfigurableRequest GetTeachApiRequest(this MethodBase methodBase, User user)
        {
            return TeachApiConfigurableRequest.FromMethod(methodBase, user);
        }
        
        private static readonly TimeSpan FakeDelay = TimeSpan.FromMilliseconds(100);
       
        
        public static class Users
        {
            [Utility.ApiMethod("{user_id}/register")]
            public static Task<bool> RegisterUser(Contact contact)
            {
                return MethodBase.GetCurrentMethod().GetTeachApiRequest(null)
                    .PushParameters("user_id", "first_name", "last_name", "phone_number")
                    .AddParam(contact.UserId)
                    .AddParam(contact.FirstName)
                    .AddParam(contact.LastName)
                    .AddParam(contact.PhoneNumber)
                    .GetResponseAndResult<bool>("POST");
            }

            [Utility.ApiMethod("{user_id}/profile")]
            public static Task<Collection<UserProfile.Progression>> GetProfile(User user)
            {
                return MethodBase.GetCurrentMethod().GetTeachApiRequest(null)
                    .PushParameters("user_id")
                    .AddParam(user.Id)
                    .GetResponseAndResult<Collection<UserProfile.Progression>>();
            }

            [Utility.ApiMethod("{user_id}")]
            public static Task<bool> IsRegistered(User user)
            {
                return MethodBase.GetCurrentMethod().GetTeachApiRequest(null)
                    .PushParameters("user_id")
                    .AddParam(user.Id)
                    .GetResponseAndResult<bool>();
            }
        }

        public static class Courses
        {
            [Utility.ApiMethod("{user_id}/courses")]
            public static Task<Collection<Course>> GetCoursesAsync(User user)
            {
                return MethodBase.GetCurrentMethod().GetTeachApiRequest(null)
                    .PushParameters("user_id")
                    .AddParam(user.Id)
                    .GetResponseAndResult<Collection<Course>>();
            }

            [Utility.ApiMethod("{user_id}/courses/{course_id}")]
            public static Task<Course> GetCourseAsync(User user, string courseId)
            {
                return MethodBase.GetCurrentMethod().GetTeachApiRequest(null)
                    .PushParameters("user_id", "course_id")
                    .AddParam(user.Id)
                    .AddParam(courseId)
                    .GetResponseAndResult<Course>();
            }

            [Utility.ApiMethod("{user_id}/courses/{course_id}/chapter/{chapter_id}/page/{par_id}")]
            public static Task<string> GetParagraphTextAsync(User user, CourseParagraph paragraph)
            {
                return MethodBase.GetCurrentMethod().GetTeachApiRequest(null)
                    .PushParameters("user_id", "course_id", "chapter_id", "par_id")
                    .AddParam(user.Id)
                    .AddParam(paragraph.CourseId)
                    .AddParam(paragraph.ChapterId)
                    .AddParam(paragraph.Id)
                    .GetResponseAndResult<string>();
            }

            [Utility.ApiMethod("{user_id}/courses/{course_id}/chapter/{chapter_id}/complete")]
            public static Task<bool> MakeProgression(User user, CourseParagraph paragraph)
            {
                return MethodBase.GetCurrentMethod().GetTeachApiRequest(null)
                    .PushParameters("user_id", "course_id", "chapter_id", "paragraph")
                    .AddParam(user.Id)
                    .AddParam(paragraph.CourseId)
                    .AddParam(paragraph.ChapterId)
                    .AddParam(paragraph.Id)
                    .GetResponseAndResult<bool>("POST");
            }

            [Utility.ApiMethod("{user_id}/courses/{course_id}/chapter/{chapter_id}/completeQuiz")]
            public static Task<bool> MakeQuizProgression(User user, CourseQuiz quiz)
            {
                return MethodBase.GetCurrentMethod().GetTeachApiRequest(null)
                    .PushParameters("user_id", "course_id", "chapter_id")
                    .AddParam(user.Id)
                    .AddParam(quiz.CourseId)
                    .AddParam(quiz.ChapterId)
                    .GetResponseAndResult<bool>("POST");
            }

            [Utility.ApiMethod("{user_id}/courses/{course_id}/chapter/{chapter_id}/quiz")]
            public static Task<CourseQuiz> GetQuizAsync(User user, CourseChapter chapter)
            {
                return MethodBase.GetCurrentMethod().GetTeachApiRequest(null)
                    .PushParameters("user_id", "course_id", "chapter_id")
                    .AddParam(user.Id)
                    .AddParam(chapter.CourseId)
                    .AddParam(chapter.Id)
                    .GetResponseAndResult<CourseQuiz>();
            }
        }
    }
}