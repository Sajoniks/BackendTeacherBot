using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using LearnBotVrk.botApi;
using LearnBotVrk.Telegram.BotAPI.Types;
using LearnBotVrk.Telegram.Types;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace LearnBotVrk.Telegram.BotAPI
{
    public static class BotExtensions
    {
        internal class TelegramResponse<T>
        {
            [JsonProperty("ok")]
            public bool IsOk { get; set; }
            [JsonProperty("description")]
            public string Description { get; set; }
            [JsonProperty("result")]
            public T Result { get; set; }

            public TelegramResponse()
            {
                IsOk = false;
                Description = null;
                Result = default;
            }
        }
        internal class RequestWrapper
        {
            private readonly String _token;
            private readonly String _methodName;
            
            private static String _baseUrl = "https://api.telegram.org/";

            private readonly List<String> _parms;
            private readonly List<String> _vals;
            
            public RequestWrapper(IBot bot, string methodName)
            {
                _token = bot.GetToken();
                _methodName = methodName;
                _parms = new List<string>();
                _vals = new List<string>();
            }

            public RequestWrapper AddJsonParam<T>(string param, T value)
            {
                if (!_parms.Contains(param) && value != null)
                {
                    _parms.Add(param);
                    _vals.Add(JsonConvert.SerializeObject(value));
                }

                return this;
            }

            private void AddParamImpl(string param, string value)
            {
                if (!_parms.Contains(param) && value != null)
                {
                    _parms.Add(param);
                    _vals.Add(value.ToString());
                }
            }

            public RequestWrapper AddParam(string param, int? value)
            {
                AddParamImpl(param, value.ToString());
                return this;
            }

            public RequestWrapper AddParam(string param, long? value)
            {
                AddParamImpl(param, value.ToString());
                return this;
            }
            
            public RequestWrapper AddParam(string param, Enum enumerator)
            {
                AddParamImpl(param, enumerator.ToString().ToLowerInvariant());
                return this;
            }

            public RequestWrapper AddParam(string param, string value)
            {
                AddParamImpl(param, value);
                return this;
            }

            public RequestWrapper AddParam(string param, bool? value)
            {
                AddParamImpl(param, value.ToString());
                return this;
            }

            /// <summary>
            /// Build url and get response
            /// </summary>
            /// <returns>TelegramResponse object</returns>
            public TelegramResponse<T> GetResponse<T>()
            {
                StringBuilder builder = new StringBuilder(_baseUrl);
                builder.Append($"bot{_token}").Append('/');
                builder.Append(_methodName);
                if (_parms.Count > 0)
                {
                    builder.Append("?");
                    
                    for (int i = 0; i < _parms.Count; ++i)
                    {
                        builder.AppendFormat("{0}={1}", _parms[i], _vals[i]);
                        if (i + 1 < _parms.Count)
                        {
                            builder.Append('&');
                        }
                    }
                }

                string apiString = builder.ToString();
                var request = WebRequest.CreateHttp(apiString);

                TelegramResponse<T> result = null;
                using (var response = request.GetResponse())
                {
                    using (var responseStream = response.GetResponseStream())
                    {
                        if (responseStream == null) return null;
                        
                        string responseJson = new StreamReader(responseStream).ReadToEnd();
                        result = JsonConvert.DeserializeObject<TelegramResponse<T>>(responseJson);
                    }
                }

                return result;
            }

            public Task<T> GetResponseAndResult<T>(T fallback = default)
            {
                var response = GetResponse<T>();
                if (response.IsOk)
                {
                    return Task.FromResult(response.Result);
                }

                return Task.FromResult<T>(fallback);
            }
        }

        private class MethodName : Attribute
        {
            private String Name { get; }

            public MethodName(String name)
            {
                this.Name = name;
            }

            public static string GetMethodName(string caller)
            {
                MethodBase method = typeof(BotExtensions).GetMethod(caller);
                return method?.GetCustomAttribute<MethodName>()?.Name;
            }
        }
        
        [MethodName("sendMessage")]
        public static Task<Message> SendMessageAsync(this IBot bot
            , Chat chat
            , String text
            , IReplyMarkup replyMarkup = null
        )
        {
            RequestWrapper req = new RequestWrapper(bot, MethodName.GetMethodName(nameof(SendMessageAsync)));
            req.AddParam("chat_id", chat.Id)
                .AddParam("text", text)
                .AddJsonParam("reply_markup", replyMarkup);

            return req.GetResponseAndResult<Message>();
        }

        [MethodName("sendPoll")]
        public static Task<Message> SendPollAsync(this IBot bot
            , Chat chat, String question
            , string[] options
            , bool? anonymous = null
            , Poll.Type? pollType = null
            , bool? allowMultiple = null
            , int? correctOption = null
            , string explanation = null
            , bool? protectContent = null
        )
        {
            RequestWrapper req = new RequestWrapper(bot, MethodName.GetMethodName(nameof(SendPollAsync)));
            req
                .AddParam("chat_id", chat.Id)
                .AddParam("question", question)
                .AddJsonParam("options", options)
                .AddParam("is_anonymous", anonymous)
                .AddParam("type", pollType)
                .AddParam("allow_multiple_answers", allowMultiple)
                .AddParam("correct_option_id", correctOption)
                .AddParam("explanation", explanation)
                .AddParam("protect_content", protectContent);

            return req.GetResponseAndResult<Message>();
        }

        [MethodName("stopPoll")]
        public static Task<Poll> StopPollAsync(this IBot bot, Chat chat, long messageId)
        {
            RequestWrapper req = new RequestWrapper(bot, MethodName.GetMethodName(nameof(EditMessageTextAsync)));
            req
                .AddParam("chat_id", chat.Id)
                .AddParam("message_id", messageId);

            return req.GetResponseAndResult<Poll>();
        }

        [MethodName("deleteMessage")]
        public static Task<bool> DeleteMessageAsync(this IBot bot, Chat chat, long messageId)
        {
            RequestWrapper req = new RequestWrapper(bot, MethodName.GetMethodName(nameof(EditMessageTextAsync)));
            req
                .AddParam("chat_id", chat.Id)
                .AddParam("message_id", messageId);
            
            return req.GetResponseAndResult<bool>(false);
        }

        [MethodName("editMessageText")]
        public static Task<Message> EditMessageTextAsync(this IBot bot
            , Chat chat
            , long messageId
            , string messageText
            ,
            IReplyMarkup replyMarkup = null
        )
        {
            RequestWrapper req = new RequestWrapper(bot, MethodName.GetMethodName(nameof(EditMessageTextAsync)));
            req
                .AddParam("chat_id", chat.Id)
                .AddParam("message_id", messageId)
                .AddParam("text", messageText)
                .AddJsonParam("reply_markup", replyMarkup);

            return req.GetResponseAndResult<Message>();
        }
    }
}