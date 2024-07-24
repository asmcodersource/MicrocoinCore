namespace Microcoin.Microcoin.Network
{
    public interface IBroadcastMessage
    {
        public string? Payload { get; set; }
        public string PayloadType { get; set; }
    }
}
