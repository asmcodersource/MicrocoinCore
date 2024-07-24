namespace Microcoin.Microcoin.Network
{
    public interface IMessage
    {
        public string Payload { get; set; }
        public string? PayloadType { get; set; }
    }
}
