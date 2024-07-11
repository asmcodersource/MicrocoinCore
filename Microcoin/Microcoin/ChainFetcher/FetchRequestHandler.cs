using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microcoin.Microcoin.ChainFetcher
{
    public class FetchRequestHandler
    {
        public readonly FetchRequest Request;

        public FetchRequestHandler(FetchRequest fetchRequest) 
        {
            Request = fetchRequest;
        }

        public async Task StartHandling()
        {
            throw new NotImplementedException();
        }
    }
}
