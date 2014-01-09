using System;
using System.Collections.Generic;
using IHI.Server.Plugins;
using IHI.Server.Useful;

namespace IHI.Server.Events
{
    public class EventManager
    {
        private readonly Dictionary<string, HashSet<IHIEventHandler>> _strongEventHandlers;
        private readonly Dictionary<string, WeakHashSet<IHIEventHandler>> _weakEventHandlers;

        #region Method: EventManager (Constructor)
        public EventManager()
        {
            _strongEventHandlers = new Dictionary<string, HashSet<IHIEventHandler>>();
            _weakEventHandlers = new Dictionary<string, WeakHashSet<IHIEventHandler>>();
        }
        #endregion


        #region Method: StrongBind
        public EventManager StrongBind(string eventName, IHIEventHandler handler)
        {
            if (!_strongEventHandlers.ContainsKey(eventName))
                _strongEventHandlers.Add(eventName, new HashSet<IHIEventHandler>());

            _strongEventHandlers[eventName].Add(handler);
            return this;
        }
        #endregion
        #region Method: StrongBind
        public EventManager StrongBind<T>(string eventName, IHIEventHandler<T> handler) where T : IHIEventArgs
        {
            return StrongBind(eventName, (eventArgs => handler.Invoke(eventArgs as T)));
        }
        #endregion


        #region Method: WeakBind
        public EventManager WeakBind(string eventName, IHIEventHandler handler)
        {
            if (!_weakEventHandlers.ContainsKey(eventName))
            {
                _weakEventHandlers.Add(eventName, new WeakHashSet<IHIEventHandler>());
            }
            _weakEventHandlers[eventName].Add(handler);
            return this;
        }
        #endregion
        #region Method: StrongBind
        public EventManager WeakBind<T>(string eventName, IHIEventHandler<T> handler) where T : IHIEventArgs
        {
            return WeakBind(eventName, (eventArgs => handler.Invoke(eventArgs as T)));
        }
        #endregion

        #region Method: GetEventHandlers
        private IEnumerable<IHIEventHandler> GetEventHandlers(string eventName)
        {
            if (_strongEventHandlers.ContainsKey(eventName))
            {
                foreach (IHIEventHandler handler in _strongEventHandlers[eventName])
                {
                    yield return handler;
                }
            }

            if (_weakEventHandlers.ContainsKey(eventName))
            {
                foreach (IHIEventHandler handler in _weakEventHandlers[eventName])
                {
                    yield return handler;
                }
            }
        }
        #endregion

        #region Method: InvokeHandlers
        private void InvokeHandlers(IEnumerable<IHIEventHandler> handlers, IHIEventArgs eventArgs)
        {
            foreach (IHIEventHandler handler in handlers)
            {
                SafeFire(handler, eventArgs);
            }
        }
        #endregion

        #region Method: Fire
        private void Fire(string eventName, IHIEventArgs eventArgs, Plugin plugin)
        {
            InvokeHandlers(GetEventHandlers(eventName), eventArgs);
        }
        #endregion
        #region Method: SafeFire
        private void SafeFire(IHIEventHandler handler, IHIEventArgs eventArgs)
        {
            try
            {
                handler.Invoke(eventArgs);
            }
            catch (Exception e)
            {
                string dumpPath = CoreManager.ServerCore.DumpException(e);
                CoreManager.ServerCore.ConsoleManager.Error("Event Handler", "An unhandled exception from an event handler has been swallowed!");
                CoreManager.ServerCore.ConsoleManager.Error("Event Handler", "    An exception dump has been saved to " + dumpPath);
            }
        }
        #endregion

        #region Method: NewEventFirer
        public EventFirer NewEventFirer(Plugin plugin)
        {
            return new EventFirer((eventName, eventArgs) => Fire(eventName, eventArgs, plugin));
        }
        #endregion
    }

    public enum IHIEventPriority
    {
        Before,
        After
    }
}