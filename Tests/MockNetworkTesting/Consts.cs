using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tests.MockNetworkTesting
{
    internal class Consts
    {
        public static int TimeoutForCommunication {
            get {
                if (System.Diagnostics.Debugger.IsAttached)
                    return 60 * 1000;
                else
                    return 5 * 1000;
            }
        }

        public static int NetworkLevels {
            get
            {
                // It should be at least two
                return 4;
            }
        }
    }
}
