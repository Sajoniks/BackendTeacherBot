using System;
using System.Collections.Generic;

namespace LearnBotVrk.Vkr
{
    public class WindowMediator
    {
        public class Action
        {
            public Action(string command, Window window)
            {
                this.Command = command;
                this.Window = window;    
            }
            
            public String Command { get; }
            public Window Window { get; }
        }

        private readonly Dictionary<string, Window> _bindings;
        private readonly Stack<Window> _windows;

        public WindowMediator()
        {
            _windows = new Stack<Window>();
            _bindings = new Dictionary<string, Window>();
        }

        public void Mediate(params Action[] actions)
        {
            foreach (var action in actions)
            {
                _bindings.Add(action.Command, action.Window);
            }
        }

        public void EnterWindow(Window window, Context context)
        {
            if (window != null)
            {
                _windows.Push(window);
                _windows.Peek().Enter(context);
            }
        }

        private class NewWindowResponse : Window.IActionResponse
        {
            private readonly WindowMediator _mediator;
            private readonly Window _window;

            public NewWindowResponse(WindowMediator mediator, Window window)
            {
                _mediator = mediator;
                _window = window;
            }
            
            public void Invoke()
            {
                _mediator.EnterWindow(_window, Context.Get());
            }
        }

        public Window.IActionResponse CreateWindowOpenResponse(Window window)
        {
            return new NewWindowResponse(this, window);
        }

        public void HandleCommand(string command, Context context)
        {
            EnterWindow(_bindings[command], context);
        }

        public void HandleUpdate(Context context)
        {
            _windows.Peek()?.HandleUpdate(context);
        }
    }
}