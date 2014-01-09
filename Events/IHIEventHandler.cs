namespace IHI.Server.Events
{
    public delegate void IHIEventHandler<T>(T eventArgs) where T : IHIEventArgs;

    public delegate void IHIEventHandler(IHIEventArgs eventArgs);
}