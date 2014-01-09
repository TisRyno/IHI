using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IHI.Server.Habbos;
using IHI.Server.Network;
using IHI.Server.Rooms;

namespace IHI.Server.Events
{
    public class HabboEventArgs : IHIEventArgs
    {
        public Habbo Habbo
        {
            get;
            private set;
        }

        public HabboEventArgs(Habbo habbo)
        {
            Habbo = habbo;
        }
    }
}
