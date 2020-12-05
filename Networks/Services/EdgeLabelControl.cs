using GraphSharp.Controls;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Networks.Services
{
    public class Phone
    {
        public string Title { get; set; }
        public string Company { get; set; }
        public int Price { get; set; }
    }
    public class EdgeLabelControl : ContentControl
    {
        public EdgeLabelControl()
        {
            MouseDoubleClick += EdgeLabelControl_MouseDoubleClick;
            LayoutUpdated += EdgeLabelControl_LayoutUpdated;
        }

        private void EdgeLabelControl_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (!IsLoaded)
                return;
            var edgeControl = GetEdgeControl(VisualParent);
            if (edgeControl == null)
                return;
            var source = edgeControl.Source;
            var p1 = new Point(GraphCanvas.GetX(source), GraphCanvas.GetY(source));
            var target = edgeControl.Target;
            var p2 = new Point(GraphCanvas.GetX(target), GraphCanvas.GetY(target));

            //p2 = BuildLine(p1, p2);

            double edgeLength;
            var routePoints = edgeControl.RoutePoints;
            if (routePoints == null)
                // the edge is a single segment (p1,p2)
                edgeLength = GetLabelDistance(GetDistanceBetweenPoints(p1, p2));
            else
            {
                // the edge has one or more segments
                // compute the total length of all the segments
                edgeLength = 0;
                for (int i = 0; i <= routePoints.Length; ++i)
                    if (i == 0)
                        edgeLength += GetDistanceBetweenPoints(p1, routePoints[0]);
                    else if (i == routePoints.Length)
                        edgeLength += GetDistanceBetweenPoints(routePoints[routePoints.Length - 1], p2);
                    else
                        edgeLength += GetDistanceBetweenPoints(routePoints[i - 1], routePoints[i]);
                // find the line segment where the half distance is located
                edgeLength = GetLabelDistance(edgeLength);
                Point newp1 = p1;
                Point newp2 = p2;
                for (int i = 0; i <= routePoints.Length; ++i)
                {
                    double lengthOfSegment;
                    if (i == 0)
                        lengthOfSegment = GetDistanceBetweenPoints(newp1 = p1, newp2 = routePoints[0]);
                    else if (i == routePoints.Length)
                        lengthOfSegment = GetDistanceBetweenPoints(newp1 = routePoints[routePoints.Length - 1], newp2 = p2);
                    else
                        lengthOfSegment = GetDistanceBetweenPoints(newp1 = routePoints[i - 1], newp2 = routePoints[i]);
                    if (lengthOfSegment >= edgeLength)
                        break;
                    edgeLength -= lengthOfSegment;
                }
                // redefine our edge points
                p1 = newp1;
                p2 = newp2;
            }
            // align the point so that it  passes through the center of the label content
            var p = p1;
            var desiredSize = DesiredSize;
            p.Offset(-desiredSize.Width / 2, -desiredSize.Height / 2);

            // move it "edgLength" on the segment
            var angleBetweenPoints = GetAngleBetweenPoints(p1, p2);
            p.Offset(edgeLength * Math.Cos(angleBetweenPoints), -edgeLength * Math.Sin(angleBetweenPoints));
            var rect = new Rect(p, desiredSize);
            //var r = new Rectangle(rect);
            //r.Fill = new SolidColorBrush(System.Windows.Media.Colors.AliceBlue);
            Arrange(rect);
        }

        private EdgeControl GetEdgeControl(DependencyObject parent)
        {
            while (parent != null)
                if (parent is EdgeControl)
                    return (EdgeControl)parent;
                else
                    parent = VisualTreeHelper.GetParent(parent);
            return null;
        }

        private static double GetAngleBetweenPoints(Point point1, Point point2)
        {
            return Math.Atan2(point1.Y - point2.Y, point2.X - point1.X);
        }

        private static double GetDistanceBetweenPoints(Point point1, Point point2)
        {
            return Math.Sqrt(Math.Pow(point2.X - point1.X, 2) + Math.Pow(point2.Y - point1.Y, 2));
        }

        private static double GetLabelDistance(double edgeLength)
        {
            return edgeLength / 2;  // set the label halfway the length of the edge
        }



        private void EdgeLabelControl_LayoutUpdated(object sender, EventArgs e)
        {
            if (!IsLoaded)
                return;
            var edgeControl = GetEdgeControl(VisualParent);
            if (edgeControl == null)
                return;
            var source = edgeControl.Source;
            var p1 = new Point(GraphCanvas.GetX(source), GraphCanvas.GetY(source));
            var target = edgeControl.Target;
            var p2 = new Point(GraphCanvas.GetX(target), GraphCanvas.GetY(target));
            BuildEdgeControl(edgeControl, ref p1, ref p2);
        }

        public void BuildEdgeControl(EdgeControl edgeControl, ref Point p1, ref Point p2)
        {
            double edgeLength;
            var routePoints = edgeControl.RoutePoints;
            if (routePoints == null)
                // the edge is a single segment (p1,p2)
                edgeLength = GetLabelDistance(GetDistanceBetweenPoints(p1, p2));
            else
            {
                // the edge has one or more segments
                // compute the total length of all the segments
                edgeLength = 0;
                for (int i = 0; i <= routePoints.Length; ++i)
                    if (i == 0)
                        edgeLength += GetDistanceBetweenPoints(p1, routePoints[0]);
                    else if (i == routePoints.Length)
                        edgeLength += GetDistanceBetweenPoints(routePoints[routePoints.Length - 1], p2);
                    else
                        edgeLength += GetDistanceBetweenPoints(routePoints[i - 1], routePoints[i]);
                // find the line segment where the half distance is located
                edgeLength = GetLabelDistance(edgeLength);
                Point newp1 = p1;
                Point newp2 = p2;
                for (int i = 0; i <= routePoints.Length; ++i)
                {
                    double lengthOfSegment;
                    if (i == 0)
                        lengthOfSegment = GetDistanceBetweenPoints(newp1 = p1, newp2 = routePoints[0]);
                    else if (i == routePoints.Length)
                        lengthOfSegment = GetDistanceBetweenPoints(newp1 = routePoints[routePoints.Length - 1], newp2 = p2);
                    else
                        lengthOfSegment = GetDistanceBetweenPoints(newp1 = routePoints[i - 1], newp2 = routePoints[i]);
                    if (lengthOfSegment >= edgeLength)
                        break;
                    edgeLength -= lengthOfSegment;
                }
                // redefine our edge points
                p1 = newp1;
                p2 = newp2;
            }
            // align the point so that it  passes through the center of the label content
            var p = p1;
            var desiredSize = DesiredSize;
            p.Offset(-desiredSize.Width / 2, -desiredSize.Height / 2);

            // move it "edgLength" on the segment
            var angleBetweenPoints = GetAngleBetweenPoints(p1, p2);
            p.Offset(edgeLength * Math.Cos(angleBetweenPoints), -edgeLength * Math.Sin(angleBetweenPoints));
            var rect = new Rect(p, desiredSize);
            //var r = new Rectangle(rect);
            //r.Fill = new SolidColorBrush(System.Windows.Media.Colors.AliceBlue);
            Arrange(rect);
        }
    }

}
