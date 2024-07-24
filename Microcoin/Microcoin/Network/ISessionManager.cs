namespace Microcoin.Microcoin.Network
{
    internal interface ISessionManager
    {
        Task<ISessionConnection> Connect(CommunicationEndPoint endPoint, string resource);
        ISessionListener CreateListener(CommunicationEndPoint endPoint);
        ISessionListener CreateListener();
    }
}
