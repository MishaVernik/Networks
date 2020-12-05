using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Networks.Data
{
    public class RoutingTable
    {
        public String sourceId { get; set; }
        public String targetId { get; set; }
        public String weight { get; set; }
    }
}
