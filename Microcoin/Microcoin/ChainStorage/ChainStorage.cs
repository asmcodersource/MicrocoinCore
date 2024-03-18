using Microcoin.Microcoin.Blockchain.Chain;

namespace Microcoin.Microcoin.ChainStorage
{
    class ChainStorage
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
                headersFiles.Add(chainHeader, chainHeaderFile);
                fetchedHeaders.Add(chainHeader);
                chainsDictionary.Add(chainHeader.ChainIdentifier, chainHeader);
            }
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
            return chainContext;
        }

        public bool IsChainExist(ChainIdentifier chainIdentifier)   
        {
            return chainsDictionary.ContainsKey(chainIdentifier);
        }

        public ChainContext LoadChainByIdentifier(ChainIdentifier chainIdentifier)
        {
            var chainHeader = chainsDictionary[chainIdentifier];
            ChainContext chainContext = new ChainContext(headersFiles[chainHeader]);
            chainContext.Fetch();
            return chainContext;
        }

        public void AddNewChainToStorage(AbstractChain chain)
        {
            var chainIdentifier = new ChainIdentifier(chain);
            if (chainsDictionary.ContainsKey(chainIdentifier) is true)
                throw new Exception("Trying to add already exist chain to storage");
            // TODO: Implement method
            throw new NotImplementedException();
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
        }
    }
}
    