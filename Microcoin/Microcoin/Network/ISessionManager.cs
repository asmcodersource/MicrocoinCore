namespace Microcoin.Microcoin.Network
{
    public interface ISessionManager
    {
        Task<ISessionConnection> Connect(ICommunicationEndPoint endPoint, string resource);
        ISessionListener CreateListener();
    }
}
