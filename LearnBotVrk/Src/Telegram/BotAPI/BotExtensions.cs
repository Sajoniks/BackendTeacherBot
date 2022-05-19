using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using LearnBotVrk.botApi;
using LearnBotVrk.Telegram.BotAPI.Types;
using LearnBotVrk.Telegram.BotAPI.Types.ReplyMarkup;
using LearnBotVrk.Telegram.Types;
using Newtonsoft.Json;

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
            private int _lastParamIdx;

            private RequestWrapper(IBot bot, string methodName)
            {
                _lastParamIdx = -1;
                _token = bot.GetToken();
                _methodName = methodName;
                _parms = new List<string>();
                _vals = new List<string>();
            }

            private void WriteParam(string value)
            {
                _vals[_lastParamIdx + 1] = value;
                ++_lastParamIdx;
            }

            public static RequestWrapper FromMethod(MethodBase methodBase, IBot bot)
            {
                var req = new RequestWrapper(bot, ApiMethod.Resolve(methodBase.Name));

                foreach (var param in methodBase.GetParameters())
                {
                    var name = ApiParam.Resolve(param);
                    if (name != null)
                    {
                        req._parms.Add(name);
                        req._vals.Add(null);
                    }
                }

                return req;
            }

            public RequestWrapper AddJsonParam<T>(T value)
            {
                WriteParam(value != null ? JsonConvert.SerializeObject(value) : null);
                return this;
            }

            public RequestWrapper AddParam(int? value)
            {
                WriteParam(value?.ToString());
                return this;
            }

            public RequestWrapper AddParam(long? value)
            {
                WriteParam(value?.ToString());
                return this;
            }
            
            public RequestWrapper AddParam(Enum enumerator)
            {
                WriteParam(enumerator?.ToString().ToLowerInvariant());
                return this;
            }

            public RequestWrapper AddParam(string value)
            {
                WriteParam(value);
                return this;
            }

            public RequestWrapper AddParam(bool? value)
            {
                WriteParam(value?.ToString());
                return this;
            }

            /// <summary>
            /// Build url and get response
            /// </summary>
            /// <returns>TelegramResponse object</returns>
            public TelegramResponse<T> GetResponse<T>()
            {
                for (int i = 0; i < _parms.Count;)
                {
                    if (_vals[i] == null)
                    {
                        _vals.RemoveAt(i);
                        _parms.RemoveAt(i);
                    }
                    else
                    {
                        ++i;
                    }
                }
                
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

                using var response = request.GetResponse();
                using var responseStream = response.GetResponseStream();
                
                if (responseStream == null) return null;
                
                string responseJson = new StreamReader(responseStream).ReadToEnd();
                
                return JsonConvert.DeserializeObject<TelegramResponse<T>>(responseJson);
            }

            public Task<T> GetResponseAndResult<T>(T fallback = default)
            {
                var response = GetResponse<T>();
                return Task.FromResult(response.IsOk ? response.Result : fallback);
            }
        }
        private class ApiMethod : Attribute
        {
            private String Name { get; }

            public ApiMethod(String name)
            {
                this.Name = name;
            }

            public static string Resolve(string caller)
            {
                MethodBase method = typeof(BotExtensions).GetMethod(caller);
                return method?.GetCustomAttribute<ApiMethod>()?.Name;
            }
        }
        private class ApiParam : Attribute
        {
            private String Name { get; }

            public ApiParam(String name)
            {
                this.Name = name;
            }

            public static string Resolve(ParameterInfo param)
            {
                return param?.GetCustomAttribute<ApiParam>()?.Name;
            }
        }

        private static RequestWrapper GetMethodApiRequest(this MethodBase methodBase, IBot bot)
        {
            return RequestWrapper.FromMethod(methodBase, bot);
        }

        [ApiMethod("getUpdates")]
        public static Task<Update[]> PollAsync(this IBot bot
            , [ApiParam("offset")] long offset
        )
        {
            return MethodBase.GetCurrentMethod().GetMethodApiRequest(bot)
                .AddParam(offset)
                .GetResponseAndResult<Update[]>();
        }
        
        [ApiMethod("sendMessage")]
        public static Task<Message> SendMessageAsync(this IBot bot
            , [ApiParam("chat_id")]Chat chat
            , [ApiParam("text")] String text
            , [ApiParam("reply_markup")] IReplyMarkup replyMarkup = null
        )
        {
            return 
                MethodBase.GetCurrentMethod().GetMethodApiRequest(bot)
                .AddParam(chat.Id)
                .AddParam(text)
                .AddJsonParam(replyMarkup)
                .GetResponseAndResult<Message>();
        }

        [ApiMethod("sendPoll")]
        public static Task<Message> SendPollAsync(this IBot bot
            , [ApiParam("chat_id")] Chat chat
            , [ApiParam("question")] String question
            , [ApiParam("options")] string[] options
            , [ApiParam("is_anonymous")] bool? anonymous = null
            , [ApiParam("type")] Poll.Type? pollType = null
            , [ApiParam("allow_multiple_answers")] bool? allowMultiple = null
            , [ApiParam("correct_option_id")] int? correctOption = null
            , [ApiParam("explanation")] string explanation = null
            , [ApiParam("protect_content")] bool? protectContent = null
        )
        {
            return MethodBase.GetCurrentMethod().GetMethodApiRequest(bot)
                .AddParam(chat.Id)
                .AddParam(question)
                .AddJsonParam(options)
                .AddParam(anonymous)
                .AddParam(pollType)
                .AddParam(allowMultiple)
                .AddParam(correctOption)
                .AddParam(explanation)
                .AddParam(protectContent)
                .GetResponseAndResult<Message>();
        }

        [ApiMethod("stopPoll")]
        public static Task<Poll> StopPollAsync(this IBot bot
            , [ApiParam("chat_id")] Chat chat
            , [ApiParam("message_id")] long messageId
        )
        {
            return MethodBase.GetCurrentMethod().GetMethodApiRequest(bot)
                .AddParam(chat.Id)
                .AddParam(messageId)
                .GetResponseAndResult<Poll>();
        }

        [ApiMethod("deleteMessage")]
        public static Task<bool> DeleteMessageAsync(this IBot bot
            , [ApiParam("chat_id")] Chat chat
            , [ApiParam("message_id")] long messageId
        )
        {
            return MethodBase.GetCurrentMethod().GetMethodApiRequest(bot)
                .AddParam(chat.Id)
                .AddParam(messageId)
                .GetResponseAndResult<bool>(false);
        }

        [ApiMethod("editMessageText")]
        public static Task<Message> EditMessageTextAsync(this IBot bot
            , [ApiParam("chat_id")] Chat chat
            , [ApiParam("message_id")] long messageId
            , [ApiParam("text")] string messageText
            , [ApiParam("reply_markup")] IReplyMarkup replyMarkup = null
        )
        {
            return MethodBase.GetCurrentMethod().GetMethodApiRequest(bot)
                .AddParam(chat.Id)
                .AddParam(messageId)
                .AddParam(messageText)
                .AddJsonParam(replyMarkup)
                .GetResponseAndResult<Message>();
        }

        [ApiMethod("sendContact")]
        public static Task<Message> SendContactAsync(this IBot bot
            , [ApiParam("chat_id")] Chat chat
            , [ApiParam("phone_number")] String phoneNumber
            , [ApiParam("first_name")] String firstName
            , [ApiParam("last_name")] String lastName
            , [ApiParam("vcard")] String vCard
            , [ApiParam("disable_notification")] bool? silent = null
            , [ApiParam("protect_content")] bool? protect = null
            , [ApiParam("reply_to_message_id")] long? replyId = null
            , [ApiParam("allow_sending_without_reply")] bool? forceReply = null
            , [ApiParam("reply_markup")] IReplyMarkup replyMarkup = null
        )
        {
            return MethodBase.GetCurrentMethod().GetMethodApiRequest(bot)
                .AddParam(chat.Id)
                .AddParam(phoneNumber)
                .AddParam(firstName)
                .AddParam(lastName)
                .AddParam(vCard)
                .AddParam(silent)
                .AddParam(protect)
                .AddParam(replyId)
                .AddParam(forceReply)
                .AddJsonParam(replyMarkup)
                .GetResponseAndResult<Message>();
        }
    }
}