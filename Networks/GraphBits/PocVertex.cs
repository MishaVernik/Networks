using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using Networks.Data;

namespace Networks
{
    /// <summary>
    /// A simple identifiable vertex.
    /// </summary>
    [DebuggerDisplay("{ID}-{IsSatteliteRouter}")]
    public class PocVertex
    {
        public NetworkType networkType { get; set; }
        public string ID { get; private set; }
        public bool IsSatteliteRouter { get; private set; }

        public PocVertex(string id, NetworkType networkType, bool isSatteliteRouter)
        {
            this.networkType = networkType;
            ID = id;
            IsSatteliteRouter = isSatteliteRouter;
        }

        public override string ToString()
        {
            return string.Format("{0}-{1}", ID, IsSatteliteRouter);
        }
    }
}
