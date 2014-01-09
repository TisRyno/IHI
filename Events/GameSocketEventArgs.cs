using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IHI.Server.Network;

namespace IHI.Server.Events
{
    public class GameSocketEventArgs : IHIEventArgs
    {
        public GameSocket Socket
        {
            get;
            private set;
        }

        public GameSocketEventArgs(GameSocket socket, bool cancellable = true)
        {
            Socket = socket;
            Cancellable = cancellable;
        }
    }
}
