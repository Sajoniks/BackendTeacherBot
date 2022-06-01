using System;
using System.Threading;
using LearnBotVrk.botApi;

namespace LearnBotVrk.Telegram.BotAPI
{
    public class Bot : IBot
    {
        private string _token;
        private Thread _worker;
        private IUpdateHandler _handler;
        private CancellationToken _cancellation;
        private long _offset;

        public Bot(string token)
        {
            _token = token;
            _offset = 0;
        }

        private void Poll()
        {
            while (!_cancellation.IsCancellationRequested)
            {
                try
                {
                    var req = this.PollAsync(_offset).Result;
                    foreach (var update in req)
                    {
                        _handler?.OnReceive(this, update, _cancellation);
                        _offset = update.Id + 1;
                    }

                    Thread.Sleep(200);
                }
                catch (Exception e)
                {
                    _handler?.OnException(this, e, _cancellation);
                }
            }
        }

        public void StartPolling(IUpdateHandler handler, CancellationToken token)
        {
            _cancellation = token;
            _handler = handler;
            _worker = new Thread(Poll)
            {
                IsBackground = true
            };
            _worker.Start();
            _worker.Join();
        }

        public string GetToken()
        {
            return _token;
        }
    }
}