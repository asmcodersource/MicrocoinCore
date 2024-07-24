using Microcoin.Microcoin.Blockchain.Block;
using Microcoin.Microcoin.Blockchain.Chain;
using System.Text;
using System.Text.Json;

namespace Microcoin.Microcoin.ChainsIO
{
    public static class ChainSerialization
    {
        public static string SerializeChain(AbstractChain chain)
            => JsonSerializer.Serialize(chain);

        public static string SerializeBlock(Block block)
            => JsonSerializer.Serialize(block);

        public static string SerializeChainHeader(ChainHeader chainHeader)
            => JsonSerializer.Serialize(chainHeader);

        public static async Task SerilizeChainToStream(AbstractChain chain, StreamWriter stream, CancellationToken cancellationToken)
            => await stream.WriteAsync(new StringBuilder(SerializeChain(chain)), cancellationToken);

        public static async Task SerializeChainHeaderToStream(ChainHeader chainHeader, StreamWriter stream, CancellationToken cancellationToken)
            => await stream.WriteAsync(new StringBuilder(SerializeChainHeader(chainHeader)), cancellationToken);

        public static async Task SerilizeBlockToStream(Block block, StreamWriter stream, CancellationToken cancellationToken)
            => await stream.WriteAsync(new StringBuilder(SerializeBlock(block)), cancellationToken);
    }
}
