namespace Microcoin.Microcoin.Network
{
    public interface ISessionConnection
    {
        public ICommunicationEndPoint EndPoint { get; }
        public Task<bool> SendMessageAsync(object message, CancellationToken cancellationToken);
        public bool SendMessage(object message);
        public Task<ISessionMessage> ReceiveMessageAsync(CancellationToken cancellationToken);
        public ISessionMessage ReceiveMessage();
    }
}
