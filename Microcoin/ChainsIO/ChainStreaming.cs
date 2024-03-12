using Microcoin.Blockchain.Block;
using Microcoin.Blockchain.Chain;
using Microcoin.JsonStreamParser;

namespace Microcoin.ChainsIO
{
    public class ChainStreaming
    {
        public async Task<AbstractChain> ReadChainFromStream(Stream stream, CancellationToken cancellationToken)
        {
            var jsonStreamParser = new JsonStreamParser.JsonStreamParser<object>();
            var chainHeader = await jsonStreamParser.ParseJsonObject(stream, cancellationToken) as AbstractChain;
            if (chainHeader is null)
                throw new Exception("Deserialized object has wrong type, must be 'Chain'");
            var blocksList = new List<Block>();
            for (int i = 0; i < chainHeader.BlocksCount; i++)
            {
                var chainBlock = await jsonStreamParser.ParseJsonObject(stream, cancellationToken) as Block;
                if (chainBlock is null)
                    throw new Exception("Deserialized object has wrong type, must be 'Block'");
                blocksList.Add(chainBlock);
            }
            chainHeader.SetBlockList(blocksList);
            return chainHeader;
        }

        public async Task WriteChainToStream(Stream stream, AbstractChain chain, CancellationToken cancellationToken) 
        {
            await ChainSerrialization.SerrilizeChainToStream(chain, stream, cancellationToken);
            var blocks = chain.GetBlocksList();
            foreach (var block in blocks)
                await ChainSerrialization.SerrilizeBlockToStream(block, stream, cancellationToken);
        }
    }
}
