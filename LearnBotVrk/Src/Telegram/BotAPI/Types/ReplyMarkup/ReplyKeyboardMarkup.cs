using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace LearnBotVrk.Telegram.BotAPI.Types.ReplyMarkup
{
    public class ReplyKeyboardMarkup : IReplyMarkup
    {
        public class Button
        {
            public Button(String text)
            {
                this.Text = text;
            }
            
            public class PollType
            {
                [JsonProperty("type")]
                public String Type { get; set; }

                public static readonly PollType Quiz = new PollType() {Type = Poll.Type.Quiz.ToString().ToLowerInvariant() };
                public static readonly PollType Regular = new PollType() { Type = Poll.Type.Regular.ToString().ToLowerInvariant() };
                public static readonly PollType Any = null;
            }
            
            [JsonProperty("text")] public String Text { get; set; }
            [JsonProperty("request_poll", NullValueHandling = NullValueHandling.Ignore)] public PollType PollRequestType { get; set; }
        }

        public class Builder
        {
            private ReplyKeyboardMarkup _markup;
            
            public bool ResizeKeyboard { get; set; }
            public bool OneTimeKeyboard { get; set; }
            public String Placeholder { get; set; }

            public Builder()
            {
                _markup = new ReplyKeyboardMarkup();
                OneTimeKeyboard = false;
                ResizeKeyboard = false;
            }

            public Builder Row(Button[] buttons)
            {
                _markup._buttons.Add(buttons);
                return this;
            }

            public ReplyKeyboardMarkup Build()
            {
                foreach (var row in _markup._buttons)
                {
                    foreach (var btn in row)
                    {
                        bool valid = true;
                        if (!valid)
                        {
                            throw new Exception("Invalid Keyboard markup: only one optional field is allowed");
                        }
                    }
                }

                _markup._placeholder = Placeholder;
                _markup._resizeKeyboard = ResizeKeyboard;
                _markup._oneTimeKeyboard = OneTimeKeyboard;
                
                var outMarkup = _markup;
                _markup = null;
                return outMarkup;
            }
        }

        private ReplyKeyboardMarkup()
        {
            _buttons = new List<Button[]>();
        }

        [JsonProperty("keyboard")] private List<Button[]> _buttons;
        [JsonProperty("resize_keyboard", NullValueHandling = NullValueHandling.Ignore)] private bool _resizeKeyboard;
        [JsonProperty("one_time_keyboard", NullValueHandling = NullValueHandling.Ignore)] private bool _oneTimeKeyboard;
        [JsonProperty("input_field_placeholder", NullValueHandling = NullValueHandling.Ignore)] private String _placeholder;
    }
}