using Microcoin.RSAEncryptions;

namespace Microcoin.Network.NodeNet.Message
{
    internal interface IMessageSigner
    {
        void SetSignOptions(ISenderSignOptions options);
        void Sign(Message message);
    }
}
