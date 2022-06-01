using System;
using System.Collections.Generic;
using System.Linq;
using LearnBotVrk.Telegram.BotAPI.Types.ReplyMarkup;
using Newtonsoft.Json;

namespace LearnBotVrk.Telegram.BotAPI
{
    public class InlineKeyboardMarkup : IReplyMarkup
    {
        public class Button
        {
            [JsonProperty("text")] public String Text { get; set; }

            [JsonProperty("url", NullValueHandling = NullValueHandling.Ignore)]
            public String URL { get; set; }

            [JsonProperty("callback_data", NullValueHandling = NullValueHandling.Ignore)]
            public String CallbackData { get; set; }
        }

        public class Builder
        {
            public class BuilderRow
            {
                private Builder _builder;
                private List<Button> _buttons;

                internal BuilderRow(Builder builder)
                {
                    _builder = builder;
                    _buttons = new List<Button>();
                }

                public BuilderRow Add(Button button)
                {
                    _buttons.Add(button);
                    return this;
                }

                public Builder ToBuilder()
                {
                    _builder.Row(_buttons.ToArray());
                    _buttons.Clear();
                    return _builder;
                }
            }

            private InlineKeyboardMarkup _keyboardMarkup;

            public Builder()
            {
                _keyboardMarkup = new InlineKeyboardMarkup();
            }

            public Builder Row(Button[] buttons)
            {
                _keyboardMarkup._buttons.Add(buttons);
                return this;
            }

            public Builder Row(Button button)
            {
                return Row(new[] { button });
            }

            public BuilderRow Row()
            {
                return new BuilderRow(this);
            }

            public InlineKeyboardMarkup Build()
            {
                foreach (var row in _keyboardMarkup._buttons)
                {
                    foreach (var btn in row)
                    {
                        bool validation = false;
                        validation |= btn.CallbackData != null;
                        validation |= btn.URL != null;

                        if (!validation)
                        {
                            throw new Exception("Inline keyboard must have at least 1 optional field.");
                        }
                    }
                }

                var outMarkup = _keyboardMarkup;
                _keyboardMarkup = null;
                return outMarkup;
            }
        }

        [JsonProperty("inline_keyboard")] private List<Button[]> _buttons;

        private InlineKeyboardMarkup()
        {
            _buttons = new List<Button[]>();
        }
    }
}