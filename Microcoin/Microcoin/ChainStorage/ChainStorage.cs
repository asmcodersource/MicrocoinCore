using Microcoin.Microcoin.Blockchain.Chain;

namespace Microcoin.Microcoin.ChainStorage
{
    public class ChainStorage
    {
        public string ChainHeaderExtension { get; set; } = "header";
        public string ChainChainExtension { get; set; } = "chain";
        public string WorkingDirectory { get; set; } = "./ChainsStorage";
        protected HashSet<ChainHeader> fetchedHeaders = new HashSet<ChainHeader>(); 
        protected Dictionary<ChainHeader, string> headersFiles = new Dictionary<ChainHeader, string>();
        protected Dictionary<ChainIdentifier, ChainHeader> chainsDictionary = new Dictionary<ChainIdentifier, ChainHeader>();

        /// <summary>
        /// To reduce the number of file accesses as well as the number of reading cycles, the system stores a copy of the header of existing chains in a hash set. 
        /// For synchronizationstate of temporary storage with actually stored chains, you need to perform this method.
        /// </summary>
        public void FetchChains()
        {
            fetchedHeaders.Clear();
            headersFiles.Clear();
            chainsDictionary.Clear();   
            var chainHeaderFiles = Directory.GetFiles(WorkingDirectory, $"*.{ChainHeaderExtension}");
            foreach( var chainHeaderFile in chainHeaderFiles)
            {
                var chainHeader = ChainHeader.LoadFromFile(chainHeaderFile);
                AddChainToFetched(chainHeaderFile, chainHeader);
            }
            Serilog.Log.Debug($"Microcoin | Chain storage fetched {fetchedHeaders.Count()} chains");
        }

        /// <summary>Suitable for restoring state after peer shutdown</summary>
        /// <returns>Tail chain object with linked pre-chains</returns>
        public ChainContext? LoadMostComprehensiveChain()
        {
            if( fetchedHeaders.Count() == 0 )
                return null;
            //  TODO: Find a more beautiful expression of the same thing using HashSet methods.
            ChainHeader mostComprehensiveChainHeader = fetchedHeaders.First();
            foreach (var chainHeader in fetchedHeaders)
                if (chainHeader.ChainIdentifier.ChainComplexity > mostComprehensiveChainHeader.ChainIdentifier.ChainComplexity)
                    mostComprehensiveChainHeader = chainHeader;
            ChainContext chainContext = new ChainContext(headersFiles[mostComprehensiveChainHeader]);
            chainContext.Fetch();
            Serilog.Log.Debug("Microcoin | Most comprehensive chain loaded from chain");
            return chainContext;
        }

        public bool IsChainExist(ChainIdentifier chainIdentifier)   
        {
            return chainsDictionary.ContainsKey(chainIdentifier);
        }

        public ChainContext? LoadChainByIdentifier(ChainIdentifier chainIdentifier)
        {
            if (chainsDictionary.ContainsKey(chainIdentifier) is not true)
                return null;

            var chainHeader = chainsDictionary[chainIdentifier];
            Serilog.Log.Debug("Microcoin | Chain loaded from chain");
            ChainContext chainContext = new ChainContext(headersFiles[chainHeader]);
            chainContext.Fetch();
            return chainContext;
        }

        public ChainHeader AddNewChainToStorage(AbstractChain chain)
        {
            var chainIdentifier = new ChainIdentifier(chain);
            if( chainsDictionary.ContainsKey(chainIdentifier))
                return chainsDictionary[chainIdentifier];
            // Since each subsequent part of the chain must refer to the previous one,
            // you need to recursively add all parts of the chain to the storage starting from the beginning;
            // those chains that are already present in the storage can be skipped.
            ChainHeader? previousChainHeader = null;
            if( chain.PreviousChain is not null)
                previousChainHeader = AddNewChainToStorage(chain.PreviousChain);
            var previousChainHeaderPath = previousChainHeader is null ? null : GetHeaderFileNameByIdentifier(previousChainHeader.ChainIdentifier);
            var headerFileName = GetHeaderFileNameByIdentifier(chainIdentifier);
            var storeChainHeader = new ChainHeader(
                chainIdentifier, 
                Path.Combine(WorkingDirectory, GetChainFileNameByIdentifier(chainIdentifier)),
                previousChainHeaderPath is null ? null: Path.Combine(WorkingDirectory, previousChainHeaderPath)
            );

            ChainContext chainContext = new ChainContext(Path.Combine(WorkingDirectory, headerFileName), chain, storeChainHeader);
            chainContext.Push();
            AddChainToFetched(Path.Combine(WorkingDirectory, headerFileName), storeChainHeader);
            return storeChainHeader;
        }

        public void RemoveChainFromStorage(ChainIdentifier chainIdentifier)
        {
            if (chainsDictionary.ContainsKey(chainIdentifier) is not true)
                throw new Exception("Trying to remove non-existing chain from storage");
            var chainHeader = chainsDictionary[chainIdentifier];
            var chainHeaderFilePath = headersFiles[chainHeader];
            var chainFilePath = chainHeader.ChainFilePath;
            File.Delete(chainFilePath);
            File.Delete(chainHeaderFilePath);
            chainsDictionary.Remove(chainIdentifier);
            headersFiles.Remove(chainHeader);
            fetchedHeaders.Remove(chainHeader);
            Serilog.Log.Debug("Microcoin | Chain removed from storage");
        }

        protected string GetHeaderFileNameByIdentifier(ChainIdentifier chainIdentifier)
        {
            return chainIdentifier.GetHashCode() + "." + ChainHeaderExtension;
        }

        protected string GetChainFileNameByIdentifier(ChainIdentifier chainIdentifier)
        {
            return chainIdentifier.GetHashCode() + "." + ChainChainExtension;
        }
        
        protected void AddChainToFetched(string chainHeaderFile, ChainHeader chainHeader)
        {
            headersFiles.Add(chainHeader, chainHeaderFile);
            fetchedHeaders.Add(chainHeader);
            chainsDictionary.Add(chainHeader.ChainIdentifier, chainHeader);
        }
    }
}
    