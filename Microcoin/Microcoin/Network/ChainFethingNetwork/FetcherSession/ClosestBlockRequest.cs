using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NodeNet.NodeNetSession.Session;
using Microcoin.Microcoin.Blockchain.Block;
using Microcoin.Microcoin.Blockchain.Chain;
using System.Text.Json;
namespace Microcoin.Microcoin.Network.ChainFethingNetwork.FetcherSession
{
    public record RequestClosestMergeBlock(Block requestedBlock, Block lastKnownBlock);

    /// <summary>
    /// Tasks performed by instances of this class search for the nearest common point from which to begin loading an unknown part of the chain. 
    /// The block does not always have to be the last one for the requester, since the requester may have a long chain that it discards.
    /// </summary>
    public class ClosestBlockRequest
    {
        /// <summary>
        /// Creates a task that will communicate through the session to find the closest parent block for the requested chain
        /// </summary>  
        /// <returns>The block that will act as the parent for the loaded blockchain.</returns>
        public static async Task<Block> CreateRequestTask(FetcherSession fetcherSession, AbstractChain actualChain, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}
