using Dijkstra.NET.Graph;
using Dijkstra.NET.ShortestPath;
using GraphSharp.Controls;
using Networks.Data;
using Networks.PopupWindows;
using Networks.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Networks
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        #region Private Members
        private struct PackageMessage
        {
            public String info;
            public Int32 size;
            public RoutingType messageType;

            public PackageMessage(string info, int size, RoutingType messageType)
            {
                this.info = info;
                this.size = size;
                this.messageType = messageType;

            }
        }
        private PackageMessage packageMessage;
        private RoutingType messageType;

        private MainWindowViewModel vm;
        private double RouterMenuHeight;
        private double RouterMenuWidth;
        private System.Windows.Threading.DispatcherTimer dispatcherTimer = new System.Windows.Threading.DispatcherTimer();

        private readonly BitmapImage bitmapSelectedLine;
        private readonly BitmapImage bitmapLine;

        private readonly BitmapImage bitmapSelectedMessage;
        private readonly BitmapImage bitmapMessage;

        private Point startPoint;
        private Point startStackPanelPoint;
        private OperationState operationState;
        private int currentSelectedRouter = 0;
        private String fromRouter = String.Empty;
        private String toRouter = String.Empty;
        private Int32 fromRouterId = 0;
        private Int32 toRouterId = 0;
        #endregion
        #region Constructor
        public MainWindow()
        {
            operationState = OperationState.None;
            bitmapSelectedLine = new BitmapImage(new Uri("pack://application:,,,/Images/selected_line.png"));
            bitmapLine = new BitmapImage(new Uri("pack://application:,,,/Images/line.png"));
            bitmapSelectedMessage = new BitmapImage(new Uri("pack://application:,,,/Images/message_yellow.png"));
            bitmapMessage = new BitmapImage(new Uri("pack://application:,,,/Images/message_gray.png"));

            dispatcherTimer.Tick += new EventHandler(dispatcherTimer_Tick);
            dispatcherTimer.Interval = new TimeSpan(0, 0, 0, 0, 100);


            vm = new MainWindowViewModel();
            this.DataContext = vm;
            this.SizeChanged += OnWindowSizeChanged;
            InitializeComponent();
        }
        #endregion
        #region Private Methods
        #region Draw Graphics


        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool GetCursorPos(ref Win32Point pt);

        [StructLayout(LayoutKind.Sequential)]
        internal struct Win32Point
        {
            public Int32 X;
            public Int32 Y;
        };
        public static Point GetMousePosition()
        {
            var w32Mouse = new Win32Point();
            GetCursorPos(ref w32Mouse);

            return new Point(w32Mouse.X, w32Mouse.Y);
        }

        private void RemoveLines()
        {
            List<Shape> shapesToRemove = new List<Shape>();
            foreach (var item in this.graphLayout.Children.OfType<Shape>())
            {
                shapesToRemove.Add((Shape)item);
            }

            foreach (var item in shapesToRemove)
            {
                this.graphLayout.Children.Remove(item);
            }

            List<Line> linesToRemove = new List<Line>();
            foreach (var item in this.graphLayout.Children.OfType<Line>())
            {
                linesToRemove.Add((Line)item);
            }

            foreach (var item in linesToRemove)
            {
                this.graphLayout.Children.Remove(item);
            }
        }
        private void CreateLine(Point endPoint)
        {
            Line redLine = new Line();
            redLine.X1 = startPoint.X;
            redLine.Y1 = startPoint.Y;
            redLine.X2 = endPoint.X;
            redLine.Y2 = endPoint.Y;

            // Create a red Brush  
            SolidColorBrush redBrush = new SolidColorBrush();
            redBrush.Color = Colors.Red;

            // Set Line's width and color  
            redLine.StrokeThickness = 4;
            redLine.Stroke = redBrush;

            // Add line to the Grid.  
            var delta = startPoint - startStackPanelPoint;
            //if (delta.X < 0) delta.X *= -1;
            //if (delta.X < 0) delta.Y *= -1;

            var shape = DrawLinkArrow(startPoint - delta, endPoint - delta);
            //var p = shape.TransformToAncestor(this.graphLayout).Transform(new Point(0, 0));
            //shape.Width = p.X;
            //shape.Height = p.Y;

            this.graphLayout.Children.Add(shape);
            // this.graphLayout.Children.Add(redLine);
        }
        private Shape DrawLinkArrow(Point p1, Point p2)
        {
            GeometryGroup lineGroup = new GeometryGroup();
            double theta = Math.Atan2((p2.Y - p1.Y), (p2.X - p1.X)) * 180 / Math.PI;

            PathGeometry pathGeometry = new PathGeometry();
            PathFigure pathFigure = new PathFigure();
            Point p = new Point(p1.X + ((p2.X - p1.X) / 1.005), p1.Y + ((p2.Y - p1.Y) / 1.005));
            pathFigure.StartPoint = p;

            Point lpoint = new Point(p.X + 4, p.Y + 8);
            Point rpoint = new Point(p.X - 4, p.Y + 8);
            LineSegment seg1 = new LineSegment();
            seg1.Point = lpoint;
            pathFigure.Segments.Add(seg1);

            LineSegment seg2 = new LineSegment();
            seg2.Point = rpoint;
            pathFigure.Segments.Add(seg2);

            LineSegment seg3 = new LineSegment();
            seg3.Point = p;
            pathFigure.Segments.Add(seg3);

            pathGeometry.Figures.Add(pathFigure);
            RotateTransform transform = new RotateTransform();
            transform.Angle = theta + 90;
            transform.CenterX = p.X;
            transform.CenterY = p.Y;
            pathGeometry.Transform = transform;
            lineGroup.Children.Add(pathGeometry);

            LineGeometry connectorGeometry = new LineGeometry();
            connectorGeometry.StartPoint = p1;
            connectorGeometry.EndPoint = p2;
            lineGroup.Children.Add(connectorGeometry);
            System.Windows.Shapes.Path path = new System.Windows.Shapes.Path();
            path.Data = lineGroup;
            path.StrokeThickness = 2;
            path.Stroke = path.Fill = Brushes.DarkGray;

            return path;
        }
        #endregion
        private void FindPath()
        {
            var graph = new Graph<int, string>();
            Int32 nodes = vm.Graph.VertexCount;
            for (int i = 0; i < nodes; i++)
            {
                graph.AddNode(i);
            }

            foreach (var edge in vm.Graph.Edges)
            {
                var source = edge.Source.idNumber + 1;
                var target = edge.Target.idNumber + 1;
                //   MessageBox.Show(source.ToString() + "->" + target.ToString());
                graph.Connect((uint)source, (uint)target, Convert.ToInt32(edge.Tag), ""); //First node has key equal 1
            }
            //DijkstrasAlgorithm.dijkstra(m, Convert.ToInt32(fromRouter.Replace("Router", "").Replace("Computer", "")));
            ShortestPathResult result = graph.Dijkstra((uint)fromRouterId + 1, (uint)toRouterId + 1); //result contains the shortest path                                        
            ShortestPathResult reverseResult = graph.Dijkstra((uint)toRouterId + 1,(uint)fromRouterId + 1); //result contains the shortest path                                        
            var path = string.Join(" -> ", result.GetPath());

            // ================= CREATE ANIMATED MESSAGE ====================================
            var reverseVertexPath = reverseResult.GetPath().ToList();
            var vertexPath = result.GetPath().ToList();
            AnimateMessage animateMessage = new AnimateMessage();
            AnimateMessage reverseAnimateMessage = new AnimateMessage();

            animateMessage.size = packageMessage.size;
            animateMessage.info = packageMessage.info;
            animateMessage.messageType = packageMessage.messageType;

            reverseAnimateMessage.size = packageMessage.size;
            reverseAnimateMessage.info = packageMessage.info;
            reverseAnimateMessage.messageType = packageMessage.messageType;

            var sourceVertex = vm.Graph.Vertices.Where(x => x.idNumber == fromRouterId).FirstOrDefault();
            var targetVertex = vm.Graph.Vertices.Where(x => x.idNumber == toRouterId).FirstOrDefault();

            Int32 totalCost = 0;
            Int32 reverseTotalCost = 0;

            animateMessage.sourceId = fromRouterId;
            animateMessage.targetId = toRouterId;
            animateMessage.targetID = targetVertex.ID;
            animateMessage.sourceID = sourceVertex.ID;

            reverseAnimateMessage.sourceId = toRouterId;
            reverseAnimateMessage.targetId = fromRouterId;
            animateMessage.targetID = sourceVertex.ID;
            animateMessage.sourceID = targetVertex.ID;

            animateMessage.vertexPath.Add(fromRouterId);
            animateMessage.vertexPath.Add(toRouterId);

            for (int vertexIndex = 1; vertexIndex < vertexPath.Count(); vertexIndex++)
            {

                sourceVertex = vm.Graph.Vertices.Where(x => x.idNumber == (int)vertexPath[vertexIndex - 1] - 1).FirstOrDefault();
                targetVertex = vm.Graph.Vertices.Where(x => x.idNumber == (int)vertexPath[vertexIndex] - 1).FirstOrDefault();

                animateMessage.vertexPath.Add((int)vertexPath[vertexIndex] - 1);

                PocEdge pocEdge;
                if (vm.Graph.TryGetEdge(sourceVertex, targetVertex, out pocEdge))
                {
                    foreach (var edgeControl in this.graphLayout.Children.OfType<EdgeControl>())
                    {
                        if (((PocEdge)edgeControl.DataContext).ID == pocEdge.ID)
                        {
                            if (edgeControl == null)
                                return;

                            var source = edgeControl.Source;
                            var p1 = new Point(GraphCanvas.GetX(source), GraphCanvas.GetY(source));
                            var target = edgeControl.Target;
                            var p2 = new Point(GraphCanvas.GetX(target), GraphCanvas.GetY(target));
                            animateMessage.edgeId = pocEdge.ID;
                            if (animateMessage.pointPath.Count == 0)
                            {
                                animateMessage.currentPoint = p1;
                                animateMessage.pointPath.Add(p1);
                            }

                            if (edgeControl.RoutePoints != null)
                            {
                                animateMessage.pointPath.AddRange(edgeControl.RoutePoints);
                            }
                            animateMessage.pointPath.Add(p2);
                            totalCost += Convert.ToInt32(pocEdge.Tag);
                            animateMessage.edgeBandwidth.Add(new KeyValuePair<int, Point>(pocEdge.Bandwidth, p2));
                            animateMessage.edgeLinkType.Add(new KeyValuePair<int, LinkType>(pocEdge.Bandwidth, pocEdge.linkType));
                        }
                    }
                }
            }
            for (int vertexIndex = 1; vertexIndex < reverseVertexPath.Count(); vertexIndex++)
            {

                sourceVertex = vm.Graph.Vertices.Where(x => x.idNumber == (int)reverseVertexPath[vertexIndex - 1] - 1).FirstOrDefault();
                targetVertex = vm.Graph.Vertices.Where(x => x.idNumber == (int)reverseVertexPath[vertexIndex] - 1).FirstOrDefault();

                reverseAnimateMessage.vertexPath.Add((int)reverseVertexPath[vertexIndex] - 1);

                PocEdge pocEdge;
                if (vm.Graph.TryGetEdge(sourceVertex, targetVertex, out pocEdge))
                {
                    foreach (var edgeControl in this.graphLayout.Children.OfType<EdgeControl>())
                    {
                        if (((PocEdge)edgeControl.DataContext).ID == pocEdge.ID)
                        {
                            if (edgeControl == null)
                                return;

                            var source = edgeControl.Source;
                            var p1 = new Point(GraphCanvas.GetX(source), GraphCanvas.GetY(source));
                            var target = edgeControl.Target;
                            var p2 = new Point(GraphCanvas.GetX(target), GraphCanvas.GetY(target));
                            reverseAnimateMessage.edgeId = pocEdge.ID;
                            if (reverseAnimateMessage.pointPath.Count == 0)
                            {
                                reverseAnimateMessage.currentPoint = p1;
                                reverseAnimateMessage.pointPath.Add(p1);
                            }

                            if (edgeControl.RoutePoints != null)
                            {
                                reverseAnimateMessage.pointPath.AddRange(edgeControl.RoutePoints);
                            }
                            reverseAnimateMessage.pointPath.Add(p2);
                            reverseTotalCost += Convert.ToInt32(pocEdge.Tag);
                            reverseAnimateMessage.edgeBandwidth.Add(new KeyValuePair<int, Point>(pocEdge.Bandwidth, p2));
                            reverseAnimateMessage.edgeLinkType.Add(new KeyValuePair<int, LinkType>(pocEdge.Bandwidth, pocEdge.linkType));
                        }
                    }
                }
            }
            animateMessage.totalCost = totalCost;
            reverseAnimateMessage.totalCost = reverseTotalCost;

            List<AnimateMessage> animateMessages = new List<AnimateMessage>();
            switch (packageMessage.messageType)
            {
                case RoutingType.Logical:

                    // System Connect Packages
                    animateMessage.packageType = PackageType.System;
                    animateMessages.Add(animateMessage.DeepClone());

                    reverseAnimateMessage.packageType = PackageType.System;
                    animateMessages.Add(reverseAnimateMessage.DeepClone());

                    // Info Packages
                    animateMessage.packageType = PackageType.Info;
                    animateMessages.Add(animateMessage.DeepClone());
                    reverseAnimateMessage.packageType = PackageType.Info;
                    animateMessages.Add(reverseAnimateMessage.DeepClone());
                    // System Disonnect Packages
                    animateMessage.packageType = PackageType.System;
                    animateMessages.Add(animateMessage.DeepClone());

                    reverseAnimateMessage.packageType = PackageType.System;
                    animateMessages.Add(reverseAnimateMessage.DeepClone());

                    messages.Add(animateMessages);
                    break;
                case RoutingType.Datagram:
                    animateMessage.packageType = PackageType.Info;
                    animateMessages.Add(animateMessage);
                    messages.Add(animateMessages);
                    break;
                case RoutingType.VirtualConnection:
                    reverseAnimateMessage = animateMessage.DeepClone();
                    reverseAnimateMessage.vertexPath.Reverse();
                    reverseAnimateMessage.edgeBandwidth.Reverse();
                    reverseAnimateMessage.edgeLinkType.Reverse();
                    reverseAnimateMessage.pointPath.Reverse();
                    if (reverseAnimateMessage.pointPath.Count > 0)
                    {
                        reverseAnimateMessage.currentPoint = reverseAnimateMessage.pointPath[0];
                    }
                    reverseAnimateMessage.sourceId = animateMessage.targetId;
                    reverseAnimateMessage.targetId = animateMessage.sourceId;

                    // System Connect Packages
                    animateMessage.packageType = PackageType.System;
                    animateMessages.Add(animateMessage.DeepClone());

                    reverseAnimateMessage.packageType = PackageType.System;
                    animateMessages.Add(reverseAnimateMessage.DeepClone());

                    // Info Packages
                    animateMessage.packageType = PackageType.Info;
                    animateMessages.Add(animateMessage.DeepClone());
                    reverseAnimateMessage.packageType = PackageType.Info;
                    animateMessages.Add(reverseAnimateMessage.DeepClone());
                    // System Disonnect Packages
                    animateMessage.packageType = PackageType.System;
                    animateMessages.Add(animateMessage.DeepClone());

                    reverseAnimateMessage.packageType = PackageType.System;
                    animateMessages.Add(reverseAnimateMessage.DeepClone());

                    messages.Add(animateMessages);
                    break;
                default:
                    break;
            }

            // =====================================================

            //MessageBox.Show(path);
            Console.WriteLine(path);
        }

        private void ResetDrawingMode()
        {
            RemoveLines();
            currentSelectedRouter = 0;

            fromRouter = String.Empty;
            toRouter = String.Empty;
        }
        #endregion
        #region Event Handlers
        private void ButtonAddNewRouter_Click(object sender, RoutedEventArgs routedEventArgs)
        {
            vm.AddNewVertex(Data.NetworkType.ROUTER);
        }

        private void ButtonRelayoutGraph(object sender, RoutedEventArgs e)
        {
            vm.ReLayoutGraph();
        }
        protected void OnWindowSizeChanged(object sender, SizeChangedEventArgs e)
        {
            RouterMenuHeight = e.NewSize.Height * 0.5;
            RouterMenuWidth = e.NewSize.Width * 0.3;
        }

        private void StackPanel_MouseDown(object sender, MouseButtonEventArgs e)
        {
            var f = ((StackPanel)sender).TransformToAncestor(this.graphLayout)
                        .Transform(new Point(0, 0));
            textBox_Copy.Text = ((Int32)f.X).ToString() + "," + ((Int32)f.Y).ToString();
            StackPanel stackPanel = (StackPanel)sender;
            switch (operationState)
            {
                case OperationState.LinkSelected:
                    {
                        startPoint = e.GetPosition(this.graphLayout);
                        startStackPanelPoint = f;
                        currentSelectedRouter++;
                        switch (currentSelectedRouter)
                        {
                            case 1:
                                fromRouter = ((PocVertex)stackPanel.DataContext).ID;
                                break;
                            case 2:
                                toRouter = ((PocVertex)stackPanel.DataContext).ID;
                                if (fromRouter != toRouter && toRouter != String.Empty && fromRouter != String.Empty)
                                {
                                    this.vm.AddNewEdge(fromRouter, toRouter);

                                    ResetDrawingMode();
                                }
                                currentSelectedRouter = 0;
                                break;
                        }

                        break;
                    }

                case OperationState.SendMessage:
                    currentSelectedRouter++;
                    switch (currentSelectedRouter)
                    {
                        case 1:
                            fromRouterId = ((PocVertex)stackPanel.DataContext).idNumber;
                            fromRouter = ((PocVertex)stackPanel.DataContext).ID;
                            break;
                        case 2:
                            toRouter = ((PocVertex)stackPanel.DataContext).ID;
                            toRouterId = ((PocVertex)stackPanel.DataContext).idNumber;
                            if (fromRouter != toRouter && toRouter != String.Empty && fromRouter != String.Empty)
                            {
                                FindPath();
                            }
                            currentSelectedRouter = 0;
                            break;
                    }
                    break;
                case OperationState.None:
                    this.RouterMenu.Visibility = Visibility.Visible;
                    this.RouterMenu.Width = RouterMenuWidth;
                    this.RouterMenu.Height = RouterMenuHeight;

                    ResetDrawingMode();
                    BuildRoutingTable(((PocVertex)stackPanel.DataContext));

                    break;
            }
        }

        private void BuildRoutingTable(PocVertex vertex)
        {
            this.routerDataGrid.Items.Clear();
            this.routerDataGrid.Columns.Clear();
            DataGridTextColumn c1 = new DataGridTextColumn();
            c1.Header = "From";
            c1.Binding = new Binding("sourceId");
            c1.Width = 110;
            this.routerDataGrid.Columns.Add(c1);
            DataGridTextColumn c2 = new DataGridTextColumn();
            c2.Header = "To";
            c2.Width = 110;
            c2.Binding = new Binding("targetId");
            this.routerDataGrid.Columns.Add(c2);
            DataGridTextColumn c3 = new DataGridTextColumn();
            c3.Header = "Cost";
            c3.Width = 110;
            c3.Binding = new Binding("weight");
            this.routerDataGrid.Columns.Add(c3);

            if (vm.Graph.TryGetOutEdges(vertex, out IEnumerable<PocEdge> edges))
            {
                foreach (PocEdge edge in edges)
                {
                    this.routerDataGrid.Items.Add(new RoutingTable() { sourceId = vertex.ID, targetId = edge.Target.ID, weight = edge.Tag.ToString() });
                }
            }
        }

        private void graphLayout_MouseDown(object sender, MouseButtonEventArgs e)
        {
            this.RouterMenu.Visibility = Visibility.Hidden;
        }

        private void ZoomControl_MouseDown(object sender, MouseButtonEventArgs e)
        {
            this.RouterMenu.Visibility = Visibility.Hidden;
        }

        private void ButtonAddNewLink_Click(object sender, RoutedEventArgs e)
        {
            switch (operationState)
            {
                case OperationState.None:
                    operationState = OperationState.LinkSelected;
                    this.AddLink.Source = bitmapSelectedLine;
                    break;
                default:
                    operationState = OperationState.None;
                    this.AddLink.Source = bitmapLine;
                    ResetDrawingMode();
                    break;
            }
        }

        private void zoomControl_MouseMove(object sender, MouseEventArgs e)
        {
            var f = e.GetPosition(this.graphLayout);
            textBox.Text = ((Int32)f.X).ToString() + "," + ((Int32)f.Y).ToString();

            if (operationState == OperationState.LinkSelected && currentSelectedRouter == 1)
            {
                RemoveLines();
                var endPoint = e.GetPosition(this.graphLayout);
                CreateLine(endPoint);
            }
        }


        private void ButtonAddNewComputer_Click(object sender, RoutedEventArgs e)
        {
            vm.AddNewVertex(Data.NetworkType.COMPUTER);
        }

        private void zoomControl_LayoutUpdated(object sender, EventArgs e)
        {

        }
        #region Message Animation

        private void DrawAnimtaedMessageLine(Point currentPoint, Point point, PackageType packageType)
        {
            Line line = new Line();
            Line line2 = new Line();
            Line line3 = new Line();
            Line line4 = new Line();            
            var vector = new Point(currentPoint.X - point.X, currentPoint.Y - point.Y);
            var length = Math.Sqrt(vector.X * vector.X + vector.Y * vector.Y);
            point = MovePointTowards(currentPoint, point, 10);

            line.X1 = currentPoint.X;
            line.X2 = point.X;
            line.Y1 = currentPoint.Y;
            line.Y2 = point.Y;

            line2.X1 = currentPoint.X;
            line2.X2 = currentPoint.X + length;
            line2.Y1 = currentPoint.Y;
            line2.Y2 = currentPoint.Y;

            line3.X1 = point.X;
            line3.X2 = point.X + length;
            line3.Y1 = point.Y;
            line3.Y2 = point.Y;

            line4.X1 = currentPoint.X + length;
            line4.X2 = point.X + length;
            line4.Y1 = currentPoint.Y;
            line4.Y2 = point.Y;


            SolidColorBrush redBrush = new SolidColorBrush();
            switch (packageType)
            {
                case PackageType.System:
                    redBrush.Color = Colors.Blue;
                    break;
                case PackageType.Info:
                    redBrush.Color = Colors.Red;
                    break;
                default:
                    break;
            }

            line.StrokeThickness = 4;
            line.Stroke = redBrush;

            line2.StrokeThickness = 4;
            line2.Stroke = redBrush;

            line3.StrokeThickness = 4;
            line3.Stroke = redBrush;

            line4.StrokeThickness = 4;
            line4.Stroke = redBrush;

            // ((PocEdge)((System.Windows.FrameworkElement)this.graphLayout.Children[0]).DataContext).ID
            this.graphLayout.Children.Add(line);
            this.graphLayout.Children.Add(line2);
            this.graphLayout.Children.Add(line3);
            this.graphLayout.Children.Add(line4);
        }
        List<List<AnimateMessage>> messages = new List<List<AnimateMessage>>();
        long ticksMade = 0;
        private void dispatcherTimer_Tick(object sender, EventArgs e)
        {
            // code goes here
            // 1. Get EdgeControl
            // 2. Get source and target
            // 3. Get route points
            // 4. Iterate     
            RemoveLines();
            for (int outerMessageIndex = 0; outerMessageIndex < messages.Count; outerMessageIndex++)
            {
                for (int message = 0; message < messages[outerMessageIndex].Count; message++)
                {
                    if (messages[outerMessageIndex][message].isFinished)
                    {
                        continue;
                    }
                    if (messages[outerMessageIndex][message].pointPath.Count == 0)
                    {
                        messages[outerMessageIndex][message].isFinished = true;
                        continue;
                    }
                    if (messages[outerMessageIndex][message].currentPointId + 1 >= messages[outerMessageIndex][message].pointPath.Count())
                    {
                        messages[outerMessageIndex][message].isFinished = true;
                        continue;
                    }
                    messages[outerMessageIndex][message].ticksMade++;
                    Int32 coefLink = 1;
                    switch (messages[outerMessageIndex][message].edgeLinkType[messages[outerMessageIndex][message].currentEdgeId].Value)
                    {
                        case LinkType.Normal:
                            coefLink = 1;
                            break;
                        case LinkType.Sattelite:
                            coefLink = 3;
                            break;
                        default:
                            break;
                    }
                    int v = (messages[outerMessageIndex][message].size / (messages[outerMessageIndex][message].edgeBandwidth[messages[outerMessageIndex][message].currentEdgeId].Key / coefLink));
                    Point point = MovePointTowards(
                        messages[outerMessageIndex][message].currentPoint,
                        messages[outerMessageIndex][message].pointPath[messages[outerMessageIndex][message].currentPointId + 1],
                        messages[outerMessageIndex][message].distance -
                        (v >= messages[outerMessageIndex][message].distance ? 9 : v));
                    DrawAnimtaedMessageLine(
                        currentPoint: messages[outerMessageIndex][message].currentPoint,
                        point: point,
                        packageType: messages[outerMessageIndex][message].packageType);
                    messages[outerMessageIndex][message].currentPoint = point;
                    if (Math.Abs(messages[outerMessageIndex][message].edgeBandwidth[messages[outerMessageIndex][message].currentEdgeId].Value.X
                        - messages[outerMessageIndex][message].pointPath[messages[outerMessageIndex][message].currentPointId].X) < 20 &&
                           Math.Abs(messages[outerMessageIndex][message].edgeBandwidth[messages[outerMessageIndex][message].currentEdgeId].Value.Y
                           - messages[outerMessageIndex][message].pointPath[messages[outerMessageIndex][message].currentPointId].Y) < 20)
                    {
                        if (messages[outerMessageIndex][message].currentEdgeId + 1 < messages[outerMessageIndex][message].edgeBandwidth.Count)
                        {
                            messages[outerMessageIndex][message].currentEdgeId++;
                        }
                    }
                    if (Math.Abs(messages[outerMessageIndex][message].currentPoint.X
                        - messages[outerMessageIndex][message].pointPath[messages[outerMessageIndex][message].currentPointId + 1].X) < 20 &&
                        Math.Abs(messages[outerMessageIndex][message].currentPoint.Y
                        - messages[outerMessageIndex][message].pointPath[messages[outerMessageIndex][message].currentPointId + 1].Y) < 20)
                    {
                        messages[outerMessageIndex][message].distance = 1;
                        if (messages[outerMessageIndex][message].currentPointId + 1 >= messages[outerMessageIndex][message].pointPath.Count)
                        {
                            messages[outerMessageIndex][message].isFinished = true;
                        }
                        else
                        {
                            messages[outerMessageIndex][message].currentPoint = messages[outerMessageIndex][message].pointPath[messages[outerMessageIndex][message].currentPointId + 1];
                            messages[outerMessageIndex][message].currentPointId++;
                        }
                    }
                    else
                    {
                        messages[outerMessageIndex][message].distance = 10;
                    }
                    break;
                }
            }
        }



        private void ButtonSendMessage_Click(object sender, RoutedEventArgs e)
        {
            switch (operationState)
            {
                case OperationState.None:
                    operationState = OperationState.SendMessage;
                    this.SendMessage.Source = bitmapSelectedMessage;
                    it = 10;
                    // Create package
                    CreatePackageWindow createPackageWindow = new CreatePackageWindow();
                    createPackageWindow.Show();
                    createPackageWindow.Closed += CreatePackageWindow_Closed;
                    //dispatcherTimer.Start();
                    break;
                case OperationState.SendMessage:
                    operationState = OperationState.None;
                    //dispatcherTimer.Stop();
                    this.SendMessage.Source = bitmapMessage;
                    break;
                default:
                    break;
            }
        }

        private void CreatePackageWindow_Closed(object sender, EventArgs e)
        {

            CreatePackageWindow createPackageWindow = (CreatePackageWindow)sender;
            if (createPackageWindow.IsSaved)
            {
                string richText = new TextRange(createPackageWindow.richTextBox.Document.ContentStart, createPackageWindow.richTextBox.Document.ContentEnd).Text;
                packageMessage = new PackageMessage(
                    richText,
                    Convert.ToInt32(createPackageWindow.txtSize.Text) * (Convert.ToInt32(createPackageWindow.txtQuantity.Text) > 0 ? Convert.ToInt32(createPackageWindow.txtQuantity.Text) : 1),
                    messageType);
            }
        }

        private Point MovePointTowards(Point a, Point b, int distance)
        {
            if (a == b)
            {
                return a;
            }
            var vector = new Point(b.X - a.X, b.Y - a.Y);
            var length = Math.Sqrt(vector.X * vector.X + vector.Y * vector.Y);
            var unitVector = new Point(vector.X / length, vector.Y / length);
            return new Point(a.X + unitVector.X * distance, a.Y + unitVector.Y * distance);
        }
        int it = 10;
        Point prevPoint = new Point(-1, -1);
        private void edgePath_MouseDown(object sender, MouseButtonEventArgs e)
        {
            EdgeSettingsWindow edgeSettings = new EdgeSettingsWindow();
            edgeSettings.Closed += EdgeSettings_Closed;
            // TODO: send init params to the edgeSettings
            // 1. weight
            // 2. error propbability
            // 3. Number
            // 4. Дуплекс/ полудуплекс
            edgeSettings.txtWeight.Text = ((PocEdge)((EdgeControl)sender).DataContext).Tag.ToString();
            edgeSettings.txtConnected.Text = ((PocEdge)((EdgeControl)sender).DataContext).ID;
            edgeSettings.txtBandwidth.Text = ((PocEdge)((EdgeControl)sender).DataContext).Bandwidth.ToString();
            edgeSettings.txtErrorProbability.Text = ((PocEdge)((EdgeControl)sender).DataContext).ErrorProbability.ToString();
            edgeSettings.txtSource.Text = ((PocEdge)((EdgeControl)sender).DataContext).Source.ID;
            edgeSettings.txtTarget.Text = ((PocEdge)((EdgeControl)sender).DataContext).Target.ID;
            if (((PocEdge)((EdgeControl)sender).DataContext).linkType == LinkType.Normal)
            {
                edgeSettings.rbtnNormalLink.IsChecked = true;
                edgeSettings.rbtnSatteliteLink.IsChecked = false;
            }
            else
            {
                edgeSettings.rbtnNormalLink.IsChecked = false;
                edgeSettings.rbtnSatteliteLink.IsChecked = true;
            }
            edgeSettings.Show();
        }

        private void EdgeSettings_Closed(object sender, EventArgs e)
        {
            PocEdge pocEdge = null;
            PocEdge newPocEdge = null;
            var edgeSettings = (EdgeSettingsWindow)sender;
            if (edgeSettings.IsSaved == true)
            {
                var edges = vm.Graph.Edges;
                foreach (var edge in edges)
                {
                    if (edge.ID == edgeSettings.txtConnected.Text)
                    {
                        if (Int32.TryParse(edgeSettings.txtWeight.Text, out int weight))
                        {
                            edge.Tag = weight.ToString();
                            edge.weight = weight;
                        }
                        if (Int32.TryParse(edgeSettings.txtBandwidth.Text, out int Bandwidth))
                        {
                            edge.Bandwidth = Bandwidth;
                        }
                        if (Int32.TryParse(edgeSettings.txtErrorProbability.Text, out int errorProb))
                        {
                            edge.ErrorProbability = errorProb;
                        }
                        if (edgeSettings.rbtnNormalLink.IsChecked == true)
                        {
                            edge.linkType = LinkType.Normal;
                        }
                        else
                        {
                            edge.linkType = LinkType.Sattelite;
                        }
                        pocEdge = edge;
                        newPocEdge = new PocEdge(edge.ID, edge.Source, edge.Target, edge.Tag);
                        newPocEdge.linkType = edge.linkType;
                        newPocEdge.Bandwidth = edge.Bandwidth;
                        newPocEdge.weight = edge.weight;
                        newPocEdge.Tag = edge.Tag;
                        newPocEdge.ErrorProbability = edge.ErrorProbability;
                        newPocEdge.ToolTip = edge.ToolTip;
                        vm.UpdateLayout();
                        break;
                    }

                }
            }
            if (pocEdge != null)
            {
                vm.Graph.RemoveEdge(pocEdge);
                vm.Graph.AddEdge(newPocEdge);
            }

        }
        #endregion
        #region Settings


        private void btnSettings_Click(object sender, RoutedEventArgs e)
        {
            ProgramSettingsWindow programSettingsWindow = new ProgramSettingsWindow();
            switch (messageType)
            {
                case RoutingType.Logical:
                    programSettingsWindow.rbtnLogicTCP.IsChecked = true;
                    programSettingsWindow.rbtndatagramUDP.IsChecked = false;
                    programSettingsWindow.rbtnVirtualConnection.IsChecked = false;
                    break;
                case RoutingType.Datagram:
                    programSettingsWindow.rbtnLogicTCP.IsChecked = false;
                    programSettingsWindow.rbtndatagramUDP.IsChecked = true;
                    programSettingsWindow.rbtnVirtualConnection.IsChecked = false;
                    break;
                case RoutingType.VirtualConnection:
                    programSettingsWindow.rbtnLogicTCP.IsChecked = false;
                    programSettingsWindow.rbtndatagramUDP.IsChecked = false;
                    programSettingsWindow.rbtnVirtualConnection.IsChecked = true;
                    break;
                default:
                    programSettingsWindow.rbtnLogicTCP.IsChecked = true;
                    programSettingsWindow.rbtndatagramUDP.IsChecked = false;
                    programSettingsWindow.rbtnVirtualConnection.IsChecked = false;
                    break;
            }
            programSettingsWindow.Show();
            programSettingsWindow.Closed += ProgramSettingsWindow_Closed;
        }

        private void ProgramSettingsWindow_Closed(object sender, EventArgs e)
        {
            if (((ProgramSettingsWindow)sender).rbtnLogicTCP.IsChecked == true)
            {
                messageType = RoutingType.Logical;
            }
            else if (((ProgramSettingsWindow)sender).rbtndatagramUDP.IsChecked == true)
            {
                messageType = RoutingType.Datagram;
            }
            else if (((ProgramSettingsWindow)sender).rbtnVirtualConnection.IsChecked == true)
            {
                messageType = RoutingType.VirtualConnection;
            }
        }
        #endregion
        #region Timer Start/Stop


        private void btnPauseTimer_Click(object sender, RoutedEventArgs e)
        {
            //Console.WriteLine("______________________________");
            //Console.WriteLine("TIME ELAPSED: {0}", dispatcherTimer.El);
            //Console.WriteLine("______________________________");
            dispatcherTimer.Stop();
        }

        private void btnStartTimer_Click(object sender, RoutedEventArgs e)
        {
            dispatcherTimer.Start();
        }

        private void Slider_DragCompleted(object sender, System.Windows.Controls.Primitives.DragCompletedEventArgs e)
        {
            dispatcherTimer.Interval = new TimeSpan(0, 0, 0, 0, (Int32)((Slider)sender).Value);
        }
        #endregion

        #endregion

        private void btnPackageLog_Click(object sender, RoutedEventArgs e)
        {
            ShowPackagesLogWindow showPackagesLogWindow = new ShowPackagesLogWindow(animateMessages: this.messages);
            showPackagesLogWindow.Show();
        }
    }
}
