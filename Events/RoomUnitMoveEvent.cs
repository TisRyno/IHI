using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IHI.Server.Network;
using IHI.Server.Rooms;

namespace IHI.Server.Events
{
    public class RoomUnitMoveEventArgs : IHIEventArgs
    {
        public IRoomUnit RoomUnit
        {
            get;
            private set;
        }

        public RoomPosition OldPosition
        {
            get;
            private set;
        }

        public RoomPosition NewPosition
        {
            get;
            private set;
        }

        public RoomUnitMoveEventArgs(IRoomUnit roomUnit, RoomPosition oldPosition, RoomPosition newPosition)
        {
            RoomUnit = roomUnit;
            OldPosition = oldPosition;
            NewPosition = newPosition;
        }
    }
}
