using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microcoin.Microcoin.ChainStorage
{
    public class ChainContext
    {
        public string BlocksFilePath { get; protected set; }
        public string HeaderFilePath { get; protected set; }
        public ChainHeader ChainHeader { get; protected set; }
        public ChainIdentifier ChainIdentifier { get; protected set; }


        public ChainContext(string blocksFilePath, string headerFilePath)
        {
            BlocksFilePath = blocksFilePath;
            HeaderFilePath = headerFilePath;
        }

        public void Fetch()
        {
            throw new NotImplementedException();
        }
    }
}
