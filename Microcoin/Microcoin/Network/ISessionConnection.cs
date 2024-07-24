namespace Microcoin.Microcoin.Network
{
    public interface ISessionConnection
    {
        public CommunicationEndPoint EndPoint { get; }
        public Task<bool> SendMessageAsync(object message, CancellationToken cancellationToken);
        public bool SendMessage(object message);
        public Task<IMessage> ReceiveMessageAsync(CancellationToken cancellationToken);
        public IMessage ReceiveMessage();
    }
}
