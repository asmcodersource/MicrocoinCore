using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microcoin.Microcoin.ChainStorage
{
    public class ChainHeader
    {
        public ChainIdentifier ChainIdentifier { get; protected set; }
        public List<ChainIdentifier> IdentifierLinks { get; protected set; }

        public void StoreToFile(string filePath)
        {

        }

        public static ChainHeader LoadFromFile(string filePath)
        {
            throw new NotImplementedException();
        }
    }
}
