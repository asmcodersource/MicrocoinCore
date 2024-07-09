using Microcoin.Microcoin.Blockchain.Block;
using Microcoin.Microcoin.Blockchain.Chain;
using Microcoin.Microcoin.ChainStorage;
using NodeNet.NodeNet.NetworkExplorer.Requests;
using System.Collections.Concurrent;

namespace Microcoin.Microcoin.ChainFetcher { 
    public record FetchRequest(Blockchain.Block.Block RequestedBlock, DateTime HandleAfterTime);
    public record HandlingFetchRequest(FetchRequest Request, FetchRequestHandler RequestHandler);
    public record FetchResult(HandlingFetchRequest HandlingFetchRequest, ChainContext? ChainContext);

/// <summary>
/// Part of blockchain realizations, responsible to loading long chains of blocks
/// Chain defined by two blocks on start and end of loading part
/// Loading allowen only by direct peer to peer connection
/// </summary>
public class ChainFetcher
    {
        public int MaxFetchQueueSize { get; set; } = 50;
        public int MaxHandlingConcurrentTask { get; set; } = 5;

        private HashSet<Blockchain.Block.Block> BlocksInFetchSystem = new HashSet<Blockchain.Block.Block>();
        private List<HandlingFetchRequest> HandlingRequests = new List<HandlingFetchRequest>();
        private Queue<FetchRequest> RequestQueue = new Queue<FetchRequest>();


        public bool RequestChainFetch(Microcoin.Blockchain.Block.Block block)
        {
            return RequestChainFetch(block, DateTime.UtcNow);
        }

        public bool RequestChainFetch(Microcoin.Blockchain.Block.Block block, DateTime handleTime)
        {
            // Lets assume this part of code as transaction
            // It prevent from adding more than 'MaxFetchQueueSize' requests
            lock (this)
            {
                if (RequestQueue.Count >= MaxFetchQueueSize)
                    return false;
                if (BlocksInFetchSystem.Contains(block))
                    return false;
                var newFetchRequest = new FetchRequest(block, handleTime);
                BlocksInFetchSystem.Add(block);
                RequestQueue.Enqueue(newFetchRequest);
                return true;
            }
        }

        public bool HandleNextRequest()
        {
            // Get first request, if it ready, then handle it, in other case return false
            FetchRequest? peekFetchRequest = null;
            lock (this)
            {
                if( RequestQueue.TryPeek(out peekFetchRequest) is not true )
                    return false;
                if (peekFetchRequest.HandleAfterTime < DateTime.UtcNow)
                    return false;
                RequestQueue.Dequeue();

                // Create task that will handle this request
                var newHandlingRequest = new HandlingFetchRequest(
                    peekFetchRequest,
                    new FetchRequestHandler(peekFetchRequest)
                );
                HandlingRequests.Add(newHandlingRequest);
                return true;
            }
        }
    }
}
