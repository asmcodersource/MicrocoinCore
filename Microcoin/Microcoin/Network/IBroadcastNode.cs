using Microcoin.Microcoin.Network;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MicrocoinCore.Microcoin.Network
{
    public interface IBroadcastNode: IBroadcastTransceiver, IEndPointCollectionProvider {}
}
