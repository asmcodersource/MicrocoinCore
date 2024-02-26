using Microcoin.RSAEncryptions;

namespace Microcoin.Network.NodeNet.Message
{
    public interface IMessageSigner
    {
        void SetSignOptions(ISenderSignOptions options);
        void Sign(Message message);
    }
}
