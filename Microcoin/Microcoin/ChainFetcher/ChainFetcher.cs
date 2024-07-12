using Microcoin.Microcoin.Blockchain.Block;
using Microcoin.Microcoin.Blockchain.Chain;
using Microcoin.Microcoin.ChainStorage;
using NodeNet.NodeNet.NetworkExplorer.Requests;
using System.Collections.Concurrent;
using NodeNet.NodeNet;

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
        public readonly Node CommunicationNode;
        // The circuit on the basis of which the loaded circuit will be created
        public AbstractChain? SourceChain { get; set; } 
        public int MaxFetchQueueSize { get; set; } = 50;
        public int MaxHandlingConcurrentTask { get; set; } = 5;

        private HashSet<Blockchain.Block.Block> BlocksInFetchSystem = new HashSet<Blockchain.Block.Block>();
        private List<HandlingFetchRequest> HandlingRequests = new List<HandlingFetchRequest>();
        private LinkedList<FetchRequest> RequestLinkedList = new LinkedList<FetchRequest>();

        public ChainFetcher(Node communcationNode)
        {
            CommunicationNode = communcationNode;
        }

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
                if (RequestLinkedList.Count >= MaxFetchQueueSize)
                    return false;
                if (BlocksInFetchSystem.Contains(block))
                    return false;
                var newFetchRequest = new FetchRequest(block, handleTime);
                BlocksInFetchSystem.Add(block);
                AddFetchRequestToList(newFetchRequest);
                return true;
            }
        }

        public bool HandleNextRequest()
        {
            // Get first request, if it ready, then handle it, in other case return false
            lock (this)
            {
                if (RequestLinkedList.Count == 0)
                    return false;
                FetchRequest peekFetchRequest = RequestLinkedList.First();
                if( peekFetchRequest.HandleAfterTime >= DateTime.UtcNow )
                    return false;
                RequestLinkedList.RemoveFirst();

                // Create task that will handle this request
                var newHandlingRequest = new HandlingFetchRequest(
                    peekFetchRequest,
                    new FetchRequestHandler(peekFetchRequest)
                );
                if (SourceChain is null)
                    throw new Exception("Source chain isn't initialized");
                newHandlingRequest.RequestHandler.StartHandling(CommunicationNode, SourceChain);
                HandlingRequests.Add(newHandlingRequest);
                return true;
            }
        }

        /// <summary>
        /// It is necessary to add a new request to the list, in the correct place, 
        /// which is determined by the time after which it should be processed (in ascending order).
        /// </summary>
        private void AddFetchRequestToList(FetchRequest fetchRequest)
        {
            if(RequestLinkedList.Count == 0)
            {
                RequestLinkedList.AddFirst(fetchRequest);
                return;
            }
            var enumerator = RequestLinkedList.GetEnumerator();
            FetchRequest? insertTargetRequest = null;
            while (enumerator.MoveNext())
            {
                if (enumerator.Current.HandleAfterTime >= fetchRequest.HandleAfterTime)
                {
                    insertTargetRequest = enumerator.Current;
                    break;
                }
            }
            if( insertTargetRequest is not null )
                RequestLinkedList.AddBefore(new LinkedListNode<FetchRequest>(insertTargetRequest), fetchRequest);
            else 
                RequestLinkedList.AddLast(fetchRequest);
        }
    }
}
