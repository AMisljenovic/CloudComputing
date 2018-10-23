using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Contracts
{
    // states of containers
    public enum State
    {
        HOT = 0,
        STANDBY = 1,
        FAIL = 2,
        STARTED = 3,
        INITIALIZING = 4
    }
}
