using LearnBotVrk.botApi;
using LearnBotVrk.Telegram.BotAPI.Types;
using LearnBotVrk.Telegram.Types;

namespace LearnBotVrk.Vkr.Bot
{
    public class Context
    {
        private static Context _context = null;

        public static Context Get()
        {
            if (_context == null)
            {
                _context = new Context();
            }

            return _context;
        }

        public Chat Chat { get; set; }
        public Message LastReceivedMessage { get; set; }
        public Message LastSentMessage { get; set; }
        public IBot Bot { get; set; }

        public User From { get; set; }
        public Update Update { get; set; }
    }
}