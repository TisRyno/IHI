#region Usings

using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;

#endregion

namespace IHI.Server.Network
{
    public abstract class OutgoingMessage
    {
        protected readonly IInternalOutgoingMessage InternalOutgoingMessage = new InternalOutgoingMessage();
        
        private HashSet<GameSocket> sentToGameSockets = null;

        public OutgoingMessage Send(IEnumerable<IMessageable> targets, bool sendOncePerConnection = false)
        {
            if (sendOncePerConnection)
                sentToGameSockets = sentToGameSockets ?? new HashSet<GameSocket>();

            foreach (IMessageable target in targets)
            {
                if (sendOncePerConnection)
                {
                    if (target is GameSocket)
                    {
                        if (sentToGameSockets.Contains(target as GameSocket))
                            continue;
                        sentToGameSockets.Add(target as GameSocket);
                    }
                }
                Send(target);
            }
            return this;
        }
        public OutgoingMessage Send(IMessageable target)
        {
            Compile();
            if (!InternalOutgoingMessage.IsCompiled)
                InternalOutgoingMessage.Compile();
            target.SendMessage(InternalOutgoingMessage);
            return this;
        }

        protected abstract void Compile();
    }
}