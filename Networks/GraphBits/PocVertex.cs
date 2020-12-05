using Networks.Data;
using System;
using System.Diagnostics;

namespace Networks
{
    /// <summary>
    /// A simple identifiable vertex.
    /// </summary>
    [DebuggerDisplay("{ID}-{IsSatteliteRouter}")]
    [Serializable]
    public class PocVertex
    {
        public static int globalIndex = 0;
        public NetworkType networkType { get; set; }
        public string ID { get; private set; }
        public bool IsSatteliteRouter { get; private set; }
        public int idNumber { get; set; }
        public PocVertex(string id, NetworkType networkType, bool isSatteliteRouter)
        {
            this.networkType = networkType;
            ID = id;
            idNumber = globalIndex++;
            IsSatteliteRouter = isSatteliteRouter;
        }

        public override string ToString()
        {
            return string.Format("{0}-{1}", ID, IsSatteliteRouter);
        }
    }
}
