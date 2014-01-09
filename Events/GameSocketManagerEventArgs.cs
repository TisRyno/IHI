using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IHI.Server.Network;

namespace IHI.Server.Events
{
    public class GameSocketManagerEventArgs : IHIEventArgs
    {
        public GameSocketManager GameSocketManager
        {
            get;
            private set;
        }
        public string Name
        {
            get;
            private set;
        }

        public GameSocketManagerEventArgs(GameSocketManager gameSocketManager, string socketManagerName)
        {
            GameSocketManager = gameSocketManager;
            Name = socketManagerName;
        }
    }
}
