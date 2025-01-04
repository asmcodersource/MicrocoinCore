namespace Microcoin.Microcoin.Network
{
    public interface ISessionMessage
    {
        public string Payload { get; set; }
        public string? PayloadType { get; set; }
    }
}
