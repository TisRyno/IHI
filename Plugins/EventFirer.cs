using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using IHI.Server.Useful;
using IHI.Server.Plugins;
using IHI.Server.Events;

namespace IHI.Server.Plugins
{
    public class EventFirer
    {
        private Action<string, IHIEventArgs> _fireDelegate;
        public EventFirer(Action<string, IHIEventArgs> fireDelegate)
        {
            _fireDelegate = fireDelegate;
        }

        #region Method: FireThreaded
        public EventFirer Fire(string eventName, IHIEventArgs eventArgs)
        {
            _fireDelegate(eventName, eventArgs);
            return this;
        }
        #endregion
    }
}