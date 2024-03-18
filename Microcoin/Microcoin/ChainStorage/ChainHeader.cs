using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Microcoin.Microcoin.ChainStorage
{
    public class ChainHeader
    {
        public ChainIdentifier ChainIdentifier { get; protected set; }
        public string ChainFilePath { get; protected set; }
        public string? PreviousChainHeaderPath { get; protected set; }

        public ChainHeader(ChainIdentifier chainIdentifier, string chainFilePath, string? previousChainHeaderPath = null)
        {
            ChainIdentifier = chainIdentifier;
            ChainFilePath = chainFilePath;
            PreviousChainHeaderPath = previousChainHeaderPath;
        }

        public void StoreToFile(string filePath)
        {
            var jsonThis = JsonSerializer.Serialize(this);
            var jsonBytes = Encoding.UTF8.GetBytes(jsonThis);
            using (var fileStream = File.OpenWrite(filePath))
                fileStream.Write(jsonBytes, 0, jsonBytes.Length);
        }

        public static ChainHeader LoadFromFile(string filePath)
        {
            ChainHeader chainHeader = null;
            using (var fileStream = File.OpenRead(filePath))
                 chainHeader = JsonSerializer.Deserialize<ChainHeader>(fileStream as Stream);
            if (chainHeader is null)
                throw new Exception("Loaded object is null");
            return chainHeader;
        }
    }
}
