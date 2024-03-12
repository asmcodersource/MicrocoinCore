using Microcoin.Blockchain.Block;
using Microcoin.Blockchain.Chain;
using Microcoin.JsonStreamParser;
using Newtonsoft.Json.Linq;

namespace Microcoin.ChainsIO
{
    public static class ChainStreaming
    {
        public static async Task<AbstractChain> ReadChainFromStream(Stream stream, CancellationToken cancellationToken)
        {
            var jsonStreamParser = new JsonStreamParser.JsonStreamParser<JObject>();
            var chainHeader = (await jsonStreamParser.ParseJsonObject(stream, cancellationToken)).ToObject<ChainHeader>();
            if (chainHeader is null)
                throw new Exception("Deserialized object has wrong type, must be 'Chain'");
            var blocksList = new List<Block>();
            var chain = new Chain();
            for (int i = 0; i < chainHeader.BlocksCount; i++)
            {
                var chainBlock = (await jsonStreamParser.ParseJsonObject(stream, cancellationToken)).ToObject<Block>();
                if (chainBlock is null)
                    throw new Exception("Deserialized object has wrong type, must be 'Block'");
                blocksList.Add(chainBlock);
            }
            chain.SetBlockList(blocksList);
            return chain;
        }

        public static async Task WriteChainToStream(Stream stream, AbstractChain chain, CancellationToken cancellationToken) 
        {
            await ChainSerialization.SerializeChainHeaderToStream(new ChainHeader(chain), stream, cancellationToken);
            var blocks = chain.GetBlocksList();
            foreach (var block in blocks)
                await ChainSerialization.SerilizeBlockToStream(block, stream, cancellationToken);
        }
    }
}
