namespace Microcoin.Microcoin.Network
{
    public interface ISessionListener
    {
        Task StartListeningAsync(Func<ISessionConnection, Task> onConnection, string resource, CancellationToken cancellationToken);
    }
}
