namespace IHI.Server.Events
{
    public class ConsoleInputEventArgs : IHIEventArgs
    {
        public string Message
        {
            get;
            private set;
        }

        public ConsoleInputEventArgs(string message)
        {
            Message = message;
        }
    }
}