namespace Microcoin.Microcoin.Network.MessageAcceptors
{
    /// <summary>
    /// Acceptor it is entity that handle any type of NodeNet messages in context of Microcoin
    /// It is entry point to begin handle transactions, blocks, or other messages of network
    /// </summary>
    public interface IAcceptor
    {
        public void Handle(IBroadcastMessage messageContext);
    }
}
