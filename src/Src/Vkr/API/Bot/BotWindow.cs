using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LearnBotVrk.botApi;
using LearnBotVrk.Telegram.BotAPI;
using LearnBotVrk.Telegram.BotAPI.Types;
using LearnBotVrk.Telegram.BotAPI.Types.ReplyMarkup;

namespace LearnBotVrk.Vkr.API
{
    using CommandHandler = Func<UpdateContext, Task<Reply>>;
    using OptionHandler = Func<UpdateContext, Task<Reply>>;

    public class Reply
    {
        public static Reply Handled()
        {
            return new Reply(true);
        }

        public static Reply Unhandled()
        {
            return new Reply(false);
        }

        public bool IsHandled { get; }

        private Reply(bool handled)
        {
            IsHandled = handled;
        }
    }

    public class UpdateContext
    {
        internal UpdateContext(Update update, IBot bot)
        {
            this.Bot = bot;
            this.Update = update;
        }

        public Update Update { get; }
        public IBot Bot { get; }
    }

    public class BotWindow
    {
        protected class Option
        {
            public Message.Types SupportedMessageType { get; }
            public ReplyKeyboardMarkup.Button OptionButton { get; }

            private Option(ReplyKeyboardMarkup.Button button, Message.Types type)
            {
                OptionButton = button;
                SupportedMessageType = type;
            }

            public static Option TextOption(string text)
            {
                return new Option(ReplyKeyboardMarkup.Button.Default(text), Message.Types.Text);
            }

            public static Option ContactOption(string text)
            {
                return new Option(ReplyKeyboardMarkup.Button.WithContactRequest(text), Message.Types.Contact);
            }
        }

        protected Dictionary<String, Message.Types> ActionTypes { get; }

        private List<Tuple<Update.Types, OptionHandler, Func<UpdateContext, bool>>> GenericHandlers { get; }
        private List<Option> Options { get; }
        private List<OptionHandler> Actions { get; }
        private Dictionary<string, CommandHandler> Commands { get; }
        private Dictionary<String, int> ActionBindings { get; }

        private static readonly Stack<BotWindow> GlobalWindowStack = new Stack<BotWindow>();

        public static BotWindow GetCurrentWindow()
        {
            return GlobalWindowStack.Peek();
        }

        protected string EntryMessage { get; }


        protected IReplyMarkup GenerateMarkup(bool resizable = true, bool oneTime = false)
        {
            int btnsInRow = Actions.Count / 2 + 1;
            if (btnsInRow == 0) return null;

            var builder = new ReplyKeyboardMarkup.Builder()
            {
                OneTimeKeyboard = oneTime,
                ResizeKeyboard = resizable
            };

            ReplyKeyboardMarkup.Button[] btnsRow = new ReplyKeyboardMarkup.Button[btnsInRow];
            builder.Row(btnsRow);

            for (int i = 0; i < Actions.Count; ++i)
            {
                btnsRow[i % 2] = Options[i].OptionButton;
                if ((i + 1) % 2 == 0 && i + 1 != Actions.Count)
                {
                    builder.Row(btnsRow);
                    btnsRow = new ReplyKeyboardMarkup.Button[btnsInRow];
                }
            }

            return builder.Build();
        }

        public async Task StartWindow(BotWindow window, UpdateContext arg)
        {
            GlobalWindowStack.Push(window);
            await window.OnEnter(arg);
        }

        public static async Task Initialize(BotWindow window, IBot bot, Update initUpdate)
        {
            GlobalWindowStack.Push(window);
            await window.OnEnter(new UpdateContext(initUpdate, bot));
        }

        public void FinishWindow(int resultCode)
        {
            GlobalWindowStack.Pop();
            GlobalWindowStack.Peek()?.OnIntentResult(resultCode);
        }

        public BotWindow(string entryMessage)
        {
            EntryMessage = entryMessage;
            ActionTypes = new Dictionary<string, Message.Types>();
            Actions = new List<OptionHandler>();
            ActionBindings = new Dictionary<string, int>();
            Commands = new Dictionary<string, CommandHandler>();
            Options = new List<Option>();
            GenericHandlers = new List<Tuple<Update.Types, CommandHandler, Func<UpdateContext, bool>>>();
        }

        protected virtual Task<bool> HandleGenericUpdate(UpdateContext arg)
        {
            return Task.FromResult(false);
        }


        public async Task<bool> HandleUpdate(IBot bot, Update update)
        {
            var context = new UpdateContext(update, bot);

            var updateType = update.Type;
            if (updateType == Update.Types.Message)
            {
                var messageType = update.Message.Type;
                var messageText = update.Message.Text
                                  ?? ActionTypes
                                      .FirstOrDefault(p => p.Value == messageType)
                                      .Key;

                if (messageText != null && ActionTypes.ContainsKey(messageText) &&
                    ActionTypes[messageText] == messageType)
                {
                    var binding = ActionBindings[messageText];
                    var response = await Actions[binding].Invoke(context);
                    return response.IsHandled;
                }
            }
            else
            {
                var handler = GenericHandlers
                    .Where(l => l.Item1 == updateType)
                    .Where(l => l.Item3?.Invoke(context) ?? true);

                foreach (var tuple in handler)
                {
                    var response = await tuple.Item2.Invoke(context);
                    if (response.IsHandled)
                    {
                        return true;
                    }
                }
            }

            return await HandleGenericUpdate(context);
        }

        public async Task<bool> HandleCommand(IBot bot, string command, Update update)
        {
            var context = new UpdateContext(update, bot);

            CommandHandler handler = null;
            if (Commands.ContainsKey(command))
            {
                handler = Commands[command];
            }
            else if (Commands.ContainsKey("/"))
            {
                handler = Commands["/"];
            }

            if (handler != null)
            {
                var reply = await handler.Invoke(context);
                return reply.IsHandled;
            }

            return false;
        }

        protected virtual async Task OnEnter(UpdateContext ctx)
        {
            var bot = ctx.Bot;
            var message = ctx.Update.Message;
            var chat = message.Chat;

            await bot.SendMessageAsync(chat, EntryMessage, GenerateMarkup());
        }

        protected virtual void OnLeave()
        {
        }

        protected virtual void OnIntentResult(int resultCode)
        {
        }

        protected void CreateOption(Option option, OptionHandler action)
        {
            int last = Actions.Count;
            Actions.Add(action);
            Options.Add(option);
            ActionBindings.Add(option.OptionButton.Text, last);
            ActionTypes.Add(option.OptionButton.Text, option.SupportedMessageType);
        }

        protected void RemoveOption(string option)
        {
            var opt = Options.FirstOrDefault(o => o.OptionButton.Text == option);

            if (opt != null)
            {
                ActionBindings.Remove(opt.OptionButton.Text);
                ActionTypes.Remove(opt.OptionButton.Text);
                var idx = Options.IndexOf(opt);
                Options.RemoveAt(idx);
                Actions.RemoveAt(idx);

                for (int i = 0; i < Actions.Count(); ++i)
                {
                    var temp = Options[i];
                    ActionBindings[temp.OptionButton.Text] = i;
                }
            }
        }

        protected void RemoveAllOptions()
        {
            ActionBindings.Clear();
            Actions.Clear();
            ActionTypes.Clear();
            Options.Clear();
        }

        protected void CreateCommand(string command, CommandHandler action)
        {
            Commands.Add(command, action);
        }

        protected void CreateUpdateHandler(Update.Types updateType, OptionHandler handler,
            Func<UpdateContext, bool> filter = null)
        {
            GenericHandlers.Add(
                new Tuple<Update.Types, CommandHandler, Func<UpdateContext, bool>>(updateType, handler, filter));
        }
    }
}