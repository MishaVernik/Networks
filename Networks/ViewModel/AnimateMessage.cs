using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Networks
{
    public class AnimateMessage
    {
        public Int32 size { get; set; }
        public String info { get; set; }
        public bool isFinished { get; set; }

        public int sourceId { get; set; }
        public int targetId { get; set; }
        public String edgeId { get; set; }

        public Point currentPoint;
        public int currentPointId { get; set; }
        public int currentEdgeId { get; set; }
        public List<Point> pointPath { get; set; }
        public List<Int32> vertexPath { get; set; }
        public List<KeyValuePair<Int32, Point>> edgeBandwidth { get; set; }

        public int distance { get; set; }
        public AnimateMessage()
        {
            currentEdgeId = 0;
            currentPointId = 0;
            isFinished = false;
            vertexPath = new List<int>();
            pointPath = new List<Point>();
            edgeBandwidth = new List<KeyValuePair<int, Point>>();

        }
    }
}
