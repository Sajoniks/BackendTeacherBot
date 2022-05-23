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
            private string _phoneNumber;
            private List<string> _formats;
            public TeachApiConfigurableRequest(string phoneNumber, string methodName) : base("https://localhost:7079/api/", methodName)
            {
                _phoneNumber = phoneNumber;
                _formats = new List<string>();
            }

            public override Task<T> GetResponseAndResult<T>(string methodType = "GET")
            {
                var response = GetResponse<TeachApiResponse<T>>(methodType);
                return Task.FromResult(response.IsOK ? response.Data : default);
            }

            protected override string BuildApiRequestUrl(StringBuilder sb)
            {
                if (_phoneNumber != null)
                {
                    sb.Append(_phoneNumber).Append('/');
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
            
            public static TeachApiConfigurableRequest FromMethod(MethodBase methodBase, string phoneNumber)
            {
                var req = new TeachApiConfigurableRequest(phoneNumber, Utility.ApiMethod.Resolve(methodBase));

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

        public static TeachApiConfigurableRequest GetTeachApiRequest(this MethodBase methodBase, string phoneNumber)
        {
            return TeachApiConfigurableRequest.FromMethod(methodBase, phoneNumber);
        }
        
        private static readonly TimeSpan FakeDelay = TimeSpan.FromMilliseconds(100);
       
        
        public static class Users
        {
            [Utility.ApiMethod("users/register")]
            public static Task<bool> RegisterUser(Contact contact)
            {
                return MethodBase.GetCurrentMethod().GetTeachApiRequest(null)
                    .PushParameters("first_name", "last_name", "phone_number", "user_id")
                    .AddParam(contact.FirstName)
                    .AddParam(contact.LastName)
                    .AddParam(contact.PhoneNumber)
                    .AddParam(contact.UserId)
                    .GetResponseAndResult<bool>("POST");
            }

            [Utility.ApiMethod("users/{user_id}")]
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
            public static Task<Collection<Course>> GetCoursesAsync(string id, User user)
            {
                return MethodBase.GetCurrentMethod().GetTeachApiRequest(null)
                    .PushParameters("user_id")
                    .AddParam(user.Id)
                    .GetResponseAndResult<Collection<Course>>();
            }

            [Utility.ApiMethod("courses/{user_id}/{course_id}")]
            public static Task<Course> GetCourseAsync(User user, string courseId)
            {
                return MethodBase.GetCurrentMethod().GetTeachApiRequest(null)
                    .PushParameters("user_id", "course_id")
                    .AddParam(user.Id)
                    .AddParam(courseId)
                    .GetResponseAndResult<Course>();
            }

            [Utility.ApiMethod("courses/{user_id}/{course_id}/chapter/{chapter_id}/page/{par_id}")]
            public static Task<string> GetParagraphTextAsync(User user, CourseParagraph paragraph)
            {
                return MethodBase.GetCurrentMethod().GetTeachApiRequest(null)
                    .PushParameters("user_id", "course_id", "chapter_id", "par_id")
                    .AddParam(user.Id)
                    .AddParam(paragraph.Chapter.Course.Id)
                    .AddParam(paragraph.Chapter.Id)
                    .AddParam(paragraph.Id)
                    .GetResponseAndResult<string>();
            }
        }
    }
}