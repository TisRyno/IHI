using IHI.Server.Console;

namespace IHI.Server.Events
{
    public class ConsoleOutputEventArgs : IHIEventArgs
    {
        public ConsoleOutputLevel Level
        {
            get;
            private set;
        }

        public string Channel
        {
            get;
            private set;
        }

        public string Message
        {
            get;
            private set;
        }

        public ConsoleOutputEventArgs(ConsoleOutputLevel level, string channel, string message)
        {
            Level = level;
            Channel = channel;
            Message = message;
        }
    }
}