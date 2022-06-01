using System;
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
            [JsonProperty("ok")] public bool IsOk { get; set; }
            [JsonProperty("description")] public string Description { get; set; }
            [JsonProperty("result")] public T Result { get; set; }

            public TelegramResponse()
            {
                IsOk = false;
                Description = null;
                Result = default;
            }
        }

        public class TelegramConfigurableRequest : VkrRequest
        {
            private readonly string _token;

            public TelegramConfigurableRequest(IBot bot, string methodName) : base("https://api.telegram.org/")
            {
                _token = bot.GetToken();
                Append($"bot{_token}").Append(methodName);
            }
        }

        private static async Task<T> GetResponseAndResult<T>(this VkrRequest request)
        {
            var response = await request.GetJsonAsync<TelegramResponse<T>>();
            return response.IsOk ? response.Result : default;
        }

        private static TelegramConfigurableRequest GetTelegramMethodRequest(IBot bot, string methodName)
        {
            return new TelegramConfigurableRequest(bot, methodName);
        }

        public static Task<Update[]> PollAsync(this IBot bot, long offset) =>
            GetTelegramMethodRequest(bot, "getUpdates")
                .WithParameters(new { offset })
                .GetResponseAndResult<Update[]>();

        public static Task<Message> SendMessageAsync(this IBot bot
            , Chat chat
            , String text
            , IReplyMarkup replyMarkup = null
        ) =>
            GetTelegramMethodRequest(bot, "sendMessage")
                .WithParameters(new
                {
                    chat_id = chat.Id,
                    text,
                    reply_markup = replyMarkup
                })
                .GetResponseAndResult<Message>();

        public static Task<Message> SendPollAsync(this IBot bot
            , Chat chat
            , String question
            , string[] options
            , bool? anonymous = null
            , Poll.Type? pollType = null
            , bool? allowMultiple = null
            , int? correctOption = null
            , string explanation = null
            , bool? protectContent = null
            , int? openPeriod = null
        ) =>
            GetTelegramMethodRequest(bot, "sendPoll")
                .WithParameters(new
                {
                    chat_id = chat.Id,
                    question,
                    options,
                    is_anonymous = anonymous,
                    type = pollType,
                    allow_multiple_answers = allowMultiple,
                    correct_option_id = correctOption,
                    explanation,
                    protect_content = protectContent,
                    open_period = openPeriod
                })
                .GetResponseAndResult<Message>();

        public static Task<Poll> StopPollAsync(this IBot bot
            , Chat chat
            , long messageId
        ) =>
            GetTelegramMethodRequest(bot, "sendPoll")
                .WithParameters(new
                {
                    chat_id = chat.Id,
                    message_id = messageId
                })
                .GetResponseAndResult<Poll>();

        public static Task<bool> DeleteMessageAsync(this IBot bot
            , Chat chat
            , long messageId
        ) =>
            GetTelegramMethodRequest(bot, "deleteMessage")
                .WithParameters(new
                {
                    chat_id = chat.Id,
                    message_id = messageId
                })
                .GetResponseAndResult<bool>();

        public static Task<Message> EditMessageTextAsync(this IBot bot
            , Chat chat
            , long messageId
            , string messageText
            , IReplyMarkup replyMarkup = null
        ) =>
            GetTelegramMethodRequest(bot, "editMessageText")
                .WithParameters(new
                {
                    chat_id = chat.Id,
                    message_id = messageId,
                    text = messageText,
                    reply_markup = replyMarkup
                })
                .GetResponseAndResult<Message>();

        public static Task<Message> SendContactAsync(this IBot bot
            , Chat chat
            , String phoneNumber
            , String firstName
            , String lastName
            , String vCard
            , bool? silent = null
            , bool? protect = null
            , long? replyId = null
            , bool? forceReply = null
            , IReplyMarkup replyMarkup = null
        ) =>
            GetTelegramMethodRequest(bot, "sendContact")
                .WithParameters(new
                {
                    chat_id = chat.Id,
                    phone_number = phoneNumber,
                    first_name = firstName,
                    last_name = lastName,
                    vcard = vCard,
                    disable_notification = silent,
                    protect_content = protect,
                    reply_to_message_id = replyId,
                    allow_sending_without_reply = forceReply,
                    reply_markup = replyMarkup
                })
                .GetResponseAndResult<Message>();

        public static Task<bool> AnswerCallbackQuery(this IBot bot
            , string callbackQueryId
            , string? text = null
            , bool? alert = null
            , string? openUrl = null
            , int? cacheTime = null
        ) =>
            GetTelegramMethodRequest(bot, "answerCallbackQuery")
                .WithParameters(new
                {
                    callback_query_id = callbackQueryId,
                    text,
                    show_alert = alert,
                    url = openUrl,
                    cache_time = cacheTime
                })
                .GetResponseAndResult<bool>();
    }
}