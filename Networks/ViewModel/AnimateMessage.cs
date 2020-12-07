using Networks.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Networks
{
    [Serializable]
    public class AnimateMessage
    {
        public long ticksMade { get; set; }
        public PackageType packageType { get; set; }
        public RoutingType messageType { get; set; }
        public Int32 size { get; set; }
        public String info { get; set; }
        public bool isFinished { get; set; }
        public int totalCost { get; set; }
        public String sourceID { get; set; }
        public String targetID { get; set; }

        public int sourceId { get; set; }
        public int targetId { get; set; }
        public String edgeId { get; set; }

        public Point currentPoint;
        public int currentPointId { get; set; }
        public int currentEdgeId { get; set; }        
        public List<Point> pointPath { get; set; }
        public List<Int32> vertexPath { get; set; }
        public List<KeyValuePair<Int32, Point>> edgeBandwidth { get; set; }
        public List<KeyValuePair<Int32, LinkType>> edgeLinkType { get; set; }

        public int distance { get; set; }
        public AnimateMessage()
        {
            packageType = PackageType.Info;
            messageType = RoutingType.Logical;
            currentEdgeId = 0;
            currentPointId = 0;
            isFinished = false;
            vertexPath = new List<int>();
            pointPath = new List<Point>();
            edgeLinkType = new List<KeyValuePair<int, LinkType>>();
            edgeBandwidth = new List<KeyValuePair<int, Point>>();

        }
    }
}
