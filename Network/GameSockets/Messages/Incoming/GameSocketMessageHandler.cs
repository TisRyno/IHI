#region Usings

using System;
using IHI.Server.Habbos;

#endregion

namespace IHI.Server.Network
{
    public delegate void GameSocketMessageHandler(Habbo sender, IncomingMessage message);

    public class GameSocketMessageHandlers
    {
        public GameSocketMessageHandler HighPriority;
        public GameSocketMessageHandler LowPriority;
        public GameSocketMessageHandler DefaultAction;
        public GameSocketMessageHandler Watcher;

        public GameSocketMessageHandlers Invoke(Habbo sender, IncomingMessage message)
        {
            if (HighPriority != null)
                SafeInvoke(HighPriority, sender, message);

            if (message.Cancelled)
                return this;

            if (LowPriority != null)
                SafeInvoke(LowPriority, sender, message);

            if (message.Cancelled)
                return this;

            if (DefaultAction != null)
                SafeInvoke(DefaultAction, sender, message);

            if (Watcher != null)
                SafeInvoke(Watcher, sender, message);

            return this;
        }

        private void SafeInvoke(GameSocketMessageHandler handler, Habbo sender, IncomingMessage message)
        {
            try
            {
                handler(sender, message);
            }
            catch (Exception e)
            {
                sender.Socket.Disconnect("Unhandled Exception in packet handler");
                string dumpPath = CoreManager.ServerCore.DumpException(e);
                CoreManager.ServerCore.ConsoleManager.Error("Event Handler", "An unhandled exception from a packet handler has been caught and the socket has been closed!");
                CoreManager.ServerCore.ConsoleManager.Error("Event Handler", "    An exception dump has been saved to " + dumpPath);
            }
        }
    }
}