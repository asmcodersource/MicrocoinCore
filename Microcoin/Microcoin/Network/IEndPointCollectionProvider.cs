namespace Microcoin.Microcoin.Network
{
    public interface IEndPointCollectionProvider
    {
        public IEnumerable<ICommunicationEndPoint> GetEndPoints();
    }
}
