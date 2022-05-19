using System;
using System.Collections.Generic;
using System.Linq;
using LearnBotVrk.botApi;
using LearnBotVrk.Telegram.BotAPI;
using LearnBotVrk.Telegram.BotAPI.Types;
using LearnBotVrk.Telegram.BotAPI.Types.ReplyMarkup;
using LearnBotVrk.Telegram.Types;

namespace LearnBotVrk.Vkr
{
    public class Window
    {
        public interface IActionResponse
        {
            void Invoke();
        }
        
        public class MergedResponse : IActionResponse
        {
            private List<IActionResponse> _wrappee;

            public MergedResponse(params IActionResponse[] response)
            {
                this._wrappee = new List<IActionResponse>();
                this._wrappee.AddRange(response);
            }
            public void Invoke()
            {
                foreach (var response in _wrappee)
                {
                    response.Invoke();
                }
            }
        }

        public class Builder
        {
            private Window _window;

            public Builder(string enterMessage)
            {
                _window = new Window(enterMessage);
            }

            public Builder CreateActionOption(Option option, Func<Context, IActionResponse> action)
            {
                int last = _window._actions.Count;
                _window._actions.Add(action);
                _window._options.Add(option);
                _window._actionBinding.Add(option.Button.Text, last);
                _window._actionType.Add(option.Button.Text, option.SupportedUpdateType);
                return this;
            }

            public Builder OnEnter(Action<Context> action)
            {
                _window._onEnter = action;
                return this;
            }
            

            public Window Build()
            {
                return _window;
            }
        }
        public class Option
        {
            private Message.Types _supportedUpdateType;
            private ReplyKeyboardMarkup.Button _button;
            public ReplyKeyboardMarkup.Button Button => _button;
            public Message.Types SupportedUpdateType => _supportedUpdateType;

            private Option(ReplyKeyboardMarkup.Button button, Message.Types supportedUpdateType)
            {
                _button = button;
                _supportedUpdateType = supportedUpdateType;
            }
            public static Option TextOption(String text)
            {
                return new Option(ReplyKeyboardMarkup.Button.Default(text), Message.Types.Text);
            }
            public static Option ContactOption(String text)
            {
                return new Option(ReplyKeyboardMarkup.Button.WithContactRequest(text), Message.Types.Contact);
            }
        }

        private Dictionary<String, Message.Types> _actionType;
        private Dictionary<String, int> _actionBinding;
        private List<Func<Context, IActionResponse>>_actions;
        private Action<Context> _onEnter;
        private List<Option> _options;
        private IReplyMarkup _activeMarkup;
        private string _enterMessage;

        private Window(string enterMessage)
        {
            _actions = new List<Func<Context, IActionResponse>>();
            _options = new List<Option>();
            _actionType = new Dictionary<string, Message.Types>();
            _actionBinding = new Dictionary<string, int>();
            _enterMessage = enterMessage;
        }

        private IReplyMarkup GenerateMarkup()
        {
            int btnsInRow = _actions.Count / 2 + 1;
            if (btnsInRow == 0) return null;
            
            var builder = new ReplyKeyboardMarkup.Builder();
            
            ReplyKeyboardMarkup.Button[] btnsRow = new ReplyKeyboardMarkup.Button[btnsInRow];
            builder.Row(btnsRow);
            
            for (int i = 0; i < _actions.Count; ++i)
            {
                btnsRow[i % 2] = _options[i].Button;
                if ((i + 1) % 2 == 0 && i + 1 != _actions.Count)
                {
                    builder.Row(btnsRow);
                    btnsRow = new ReplyKeyboardMarkup.Button[btnsInRow];
                }
            }

            _activeMarkup = builder.Build();
            return _activeMarkup;
        }

        public void Enter(Context context)
        {
            _onEnter?.Invoke(context);
            context.Bot.SendMessageAsync(context.Chat, _enterMessage, GenerateMarkup());
        }

        public bool HandleUpdate(Context context)
        {
            var type = context.LastMessage.Type;
            var text = context.LastMessage.Text 
                       ?? _actionType
                           .FirstOrDefault(p => p.Value == type)
                           .Key;
            
            if (text != null && _actionType[text] == type)
            {
                var binding = _actionBinding[text];
                var response = _actions[binding].Invoke(context);
                response?.Invoke();
                return true;
            }

            return false;
        }
    }
}