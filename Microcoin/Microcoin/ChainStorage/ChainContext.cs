using System;
using System.IO;
using Microcoin.Microcoin.Blockchain.Block;
using Microcoin.Microcoin.Blockchain.Chain;

namespace Microcoin.Microcoin.ChainStorage
{
    public class ChainContext
    {
        public string ChainFilePath { get; protected set; }
        public string HeaderFilePath { get; protected set; }
        public Chain? Chain { get; protected set; } = null;

        protected ChainHeader? chainHeader = null;


        public ChainContext(string blocksFilePath, string headerFilePath)
        {
            ChainFilePath = blocksFilePath;
            HeaderFilePath = headerFilePath;
        }

        public ChainHeader GetChainHeader()
        {
            if (chainHeader is null)
                throw new Exception("Chain context is not initialized");
            UpdateChainHeader();
            return chainHeader;
        }

        public void Fetch()
        {
            try
            {
                chainHeader = ChainHeader.LoadFromFile(HeaderFilePath);
                Chain = FetchChainFromFile(ChainFilePath);
            } finally
            {
                chainHeader = null;
                Chain = null;
            }
        }

        public void Push()
        {
            if (chainHeader is null || Chain is null)
                throw new Exception("Chain context is not initialized");

            UpdateChainHeader();
            chainHeader.StoreToFile(HeaderFilePath);
            PushChainToFile(ChainFilePath);
        }

        protected void PushChainToFile(string chainFilePath)
        {
            // Right know it is simple implementation, where file rewrites
            // TODO: Implement addition of missing blocks
            using (var fileStream = File.OpenWrite(chainFilePath))
                ChainsIO.ChainStreaming.WriteChainToStream(fileStream as Stream, Chain, CancellationToken.None).Wait();
        }

        protected Chain FetchChainFromFile(string chainFilePath)
        {
            using (var fileStream = File.OpenRead(chainFilePath))
                return ChainsIO.ChainStreaming.ReadChainFromStream(fileStream as Stream, chainHeader.ChainIdentifier.TailChainBlockCount, CancellationToken.None).Result as Chain;
        }

        /// <summary>
        /// Actions performed on a chain can change its parameters, such as the number of blocks and the complexity of the chain.
        /// Therefore, you should update the chain header before performing an action on it.
        /// </summary>
        protected void UpdateChainHeader()
        {
            if (Chain is null)
                throw new Exception("Chain context is not initialized");
            ChainIdentifier chainIdentifier = new ChainIdentifier(Chain);
            chainHeader = new ChainHeader(chainIdentifier, chainHeader.PreviousChainHeaderPath);
        }
    }
}
