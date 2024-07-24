namespace Microcoin.Microcoin.Network
{
    public interface IBroadcastTransceiver
    {
        public Task SendBroadcastMessageAsync(object message, string? type, int ttl, CancellationToken cancellationToken);
        public void SendBroadcastMessage(object message, string? type, int ttl);
        public Task<IBroadcastMessage> ReceiveBroadcastMessageAsync(CancellationToken cancellationToken);
        public IBroadcastMessage ReceiveBroadcastMessage();

        public event Action<IBroadcastMessage> OnMessageReceived;
    }
}
