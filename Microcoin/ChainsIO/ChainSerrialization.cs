using Microcoin.Blockchain.Chain;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Microcoin.ChainsIO
{
    public static class ChainSerrialization
    {
        public static void SerializeChain(this AbstractChain chain, Stream output )
        {
            ImmutableChain immutableChain = new ImmutableChain(chain);
            
        }

        public static AbstractChain DeserializeChain(Stream input)
        {
            return null;
        }
    }
}
