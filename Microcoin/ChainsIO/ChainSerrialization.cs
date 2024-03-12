using Microcoin.Blockchain.Chain;
using Microcoin.Blockchain.Block;
using Newtonsoft.Json;
using System.Text;

namespace Microcoin.ChainsIO
{
    public static class ChainSerrialization
    {
        public static String SerializeChain( AbstractChain chain )
            => JsonConvert.SerializeObject(chain);

        public static String SerializeBlock( Block block)
            => JsonConvert.SerializeObject(block);

        public static async Task SerrilizeChainToStream(AbstractChain chain, Stream stream, CancellationToken cancellationToken)
        {
            StreamWriter streamWriter = new StreamWriter(stream);
            await streamWriter.WriteAsync(new StringBuilder(SerializeChain(chain)), cancellationToken);
        }

        public static async Task SerrilizeBlockToStream(Block block, Stream stream, CancellationToken cancellationToken)
        {
            StreamWriter streamWriter = new StreamWriter(stream);
            await streamWriter.WriteAsync(new StringBuilder(SerializeBlock(block)), cancellationToken);
        }
    }
}
