namespace IHI.Server.Events
{
    public class IHIEventArgs
    {
        public bool IsCancelled
        {
            get;
            private set;
        }

        public IHIEventPriority Priority
        {
            get;
            internal set;
        }
        
        public string CancelReason
        {
            get;
            private set;
        }

        public bool Cancellable
        {
            get;
            protected set;
        }

        public IHIEventArgs(bool cancellable = true)
        {
            Cancellable = cancellable;
        }

        public virtual void Cancel(string reason)
        {
            if (Cancellable)
                throw new IHIEventException("Cannot cancel an uncancellable event.");

            IsCancelled = true;
            CancelReason = reason;
        }
    }
}