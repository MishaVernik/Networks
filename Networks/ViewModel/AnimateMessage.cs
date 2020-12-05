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
        public bool isFinished { get; set; }

        public int sourceId { get; set; }
        public int targetId { get; set; }
        public String edgeId { get; set; }

        public Point currentPoint;
        public int currentPointId { get; set; }
        public List<Point> pointPath { get; set; }
        public List<Int32> vertexPath { get; set; }


        public int distance { get; set; }
        public AnimateMessage()
        {
            currentPointId = 0;
            isFinished = false;
            vertexPath = new List<int>();
            pointPath = new List<Point>();

        }
    }
}
