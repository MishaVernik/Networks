using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Networks.Data
{
    [Serializable]
    public enum RoutingType
    {
        Logical,
        Datagram,
        VirtualConnection
    }
}
