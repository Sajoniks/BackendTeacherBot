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

        public class TelegramConfigurableRequest : Utility.HttpConfigurableRequest
        {
            private readonly string _token;
            
            public TelegramConfigurableRequest(IBot bot, string methodName) : base("https://api.telegram.org/", methodName)
            {
                _token = bot?.GetToken();
            }

            public static TelegramConfigurableRequest FromMethod(MethodBase methodBase, IBot bot)
            {
                var req = new TelegramConfigurableRequest(bot, Utility.ApiMethod.Resolve(methodBase.Name));
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

            protected override string BuildApiRequestUrl(StringBuilder sb)
            {
                // append token
                sb.Append($"bot{_token}").Append("/");
                // append method name
                sb.Append(MethodName);
                // append parameters
                AppendParameters(sb);

                return sb.ToString();
            }

            public override Task<T> GetResponseAndResult<T>()
            {
                var response = GetResponse<TelegramResponse<T>>();
                return Task.FromResult( response.IsOk ? response.Result : default );
            }
        }

        public static TelegramConfigurableRequest GetTelegramMethodRequest(this MethodBase methodBase, IBot bot)
        {
            return TelegramConfigurableRequest.FromMethod(methodBase, bot);
        }

        [Utility.ApiMethod("getUpdates")]
        public static Task<Update[]> PollAsync(this IBot bot
            , [Utility.ApiParam("offset")] long offset
        )
        {
            return MethodBase.GetCurrentMethod().GetTelegramMethodRequest(bot)
                .AddParam(offset)
                .GetResponseAndResult<Update[]>();
        }
        
        [Utility.ApiMethod("sendMessage")]
        public static Task<Message> SendMessageAsync(this IBot bot
            , [Utility.ApiParam("chat_id")]Chat chat
            , [Utility.ApiParam("text")] String text
            , [Utility.ApiParam("reply_markup")] IReplyMarkup replyMarkup = null
        )
        {
            return 
                MethodBase.GetCurrentMethod().GetTelegramMethodRequest(bot)
                .AddParam(chat.Id)
                .AddParam(text)
                .AddJsonParam(replyMarkup)
                .GetResponseAndResult<Message>();
        }

        [Utility.ApiMethod("sendPoll")]
        public static Task<Message> SendPollAsync(this IBot bot
            , [Utility.ApiParam("chat_id")] Chat chat
            , [Utility.ApiParam("question")] String question
            , [Utility.ApiParam("options")] string[] options
            , [Utility.ApiParam("is_anonymous")] bool? anonymous = null
            , [Utility.ApiParam("type")] Poll.Type? pollType = null
            , [Utility.ApiParam("allow_multiple_answers")] bool? allowMultiple = null
            , [Utility.ApiParam("correct_option_id")] int? correctOption = null
            , [Utility.ApiParam("explanation")] string explanation = null
            , [Utility.ApiParam("protect_content")] bool? protectContent = null
            , [Utility.ApiParam("open_period")] int? openPeriod = null
        )
        {
            return MethodBase.GetCurrentMethod().GetTelegramMethodRequest(bot)
                .AddParam(chat.Id)
                .AddParam(question)
                .AddJsonParam(options)
                .AddParam(anonymous)
                .AddParam(pollType)
                .AddParam(allowMultiple)
                .AddParam(correctOption)
                .AddParam(explanation)
                .AddParam(protectContent)
                .AddParam(openPeriod)
                .GetResponseAndResult<Message>();
        }

        [Utility.ApiMethod("stopPoll")]
        public static Task<Poll> StopPollAsync(this IBot bot
            , [Utility.ApiParam("chat_id")] Chat chat
            , [Utility.ApiParam("message_id")] long messageId
        )
        {
            return MethodBase.GetCurrentMethod().GetTelegramMethodRequest(bot)
                .AddParam(chat.Id)
                .AddParam(messageId)
                .GetResponseAndResult<Poll>();
        }

        [Utility.ApiMethod("deleteMessage")]
        public static Task<bool> DeleteMessageAsync(this IBot bot
            , [Utility.ApiParam("chat_id")] Chat chat
            , [Utility.ApiParam("message_id")] long messageId
        )
        {
            return MethodBase.GetCurrentMethod().GetTelegramMethodRequest(bot)
                .AddParam(chat.Id)
                .AddParam(messageId)
                .GetResponseAndResult<bool>();
        }

        [Utility.ApiMethod("editMessageText")]
        public static Task<Message> EditMessageTextAsync(this IBot bot
            , [Utility.ApiParam("chat_id")] Chat chat
            , [Utility.ApiParam("message_id")] long messageId
            , [Utility.ApiParam("text")] string messageText
            , [Utility.ApiParam("reply_markup")] IReplyMarkup replyMarkup = null
        )
        {
            return MethodBase.GetCurrentMethod().GetTelegramMethodRequest(bot)
                .AddParam(chat.Id)
                .AddParam(messageId)
                .AddParam(messageText)
                .AddJsonParam(replyMarkup)
                .GetResponseAndResult<Message>();
        }

        [Utility.ApiMethod("sendContact")]
        public static Task<Message> SendContactAsync(this IBot bot
            , [Utility.ApiParam("chat_id")] Chat chat
            , [Utility.ApiParam("phone_number")] String phoneNumber
            , [Utility.ApiParam("first_name")] String firstName
            , [Utility.ApiParam("last_name")] String lastName
            , [Utility.ApiParam("vcard")] String vCard
            , [Utility.ApiParam("disable_notification")] bool? silent = null
            , [Utility.ApiParam("protect_content")] bool? protect = null
            , [Utility.ApiParam("reply_to_message_id")] long? replyId = null
            , [Utility.ApiParam("allow_sending_without_reply")] bool? forceReply = null
            , [Utility.ApiParam("reply_markup")] IReplyMarkup replyMarkup = null
        )
        {
            return MethodBase.GetCurrentMethod().GetTelegramMethodRequest(bot)
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

        [Utility.ApiMethod("answerCallbackQuery")]
        public static Task<bool> AnswerCallbackQuery(this IBot bot
            , [Utility.ApiParam("callback_query_id")] string callbackQueryId
            , [Utility.ApiParam("text")] string? text = null
            , [Utility.ApiParam("show_alert")] bool? alert = null
            , [Utility.ApiParam("url")] string? openUrl = null
            , [Utility.ApiParam("cache_time")] int? cacheTime = null
        )
        {
            return MethodBase.GetCurrentMethod().GetTelegramMethodRequest(bot)
                .AddParam(callbackQueryId)
                .AddParam(text)
                .AddParam(alert)
                .AddParam(openUrl)
                .AddParam(cacheTime)
                .GetResponseAndResult<bool>();
        }
    }
}