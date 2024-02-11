using Microcoin.RSAEncryptions;

namespace NodeNet.Message
{
    internal interface IMessageSigner
    {
        void SetSignOptions(ISenderSignOptions options);
        void Sign(Message message);
    }
}
