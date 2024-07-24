namespace Microcoin.Microcoin.Network
{
    internal interface ISessionListener
    {
        Task StartListeningAsync(Func<ISessionConnection, Task> onConnection, string resource, CancellationToken cancellationToken);
    }
}
