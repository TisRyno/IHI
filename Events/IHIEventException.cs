using System;

namespace IHI.Server.Events
{
    public class IHIEventException : Exception
    {
        public IHIEventException(string message) : base(message)
        {
        }
    }
}