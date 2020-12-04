using Dijkstra.NET.Graph;
using Dijkstra.NET.ShortestPath;
using GraphSharp.Controls;
using Networks.PopupWindows;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
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
        private enum OperationState
        {
            None,
            LinkSelected,
            SendMessage
        }

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
        OperationState operationState;
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
            var path = string.Join(" -> ", result.GetPath());

            // ================= CREATE ANIMATED MESSAGE ====================================
            var vertexPath = result.GetPath().ToList();
            AnimateMessage animateMessage = new AnimateMessage();

            var sourceVertex = vm.Graph.Vertices.Where(x => x.idNumber == fromRouterId).FirstOrDefault();
            var targetVertex = vm.Graph.Vertices.Where(x => x.idNumber == toRouterId).FirstOrDefault();

            animateMessage.sourceId = fromRouterId;
            animateMessage.targetId = toRouterId;

            animateMessage.vertexPath.Add(fromRouterId);
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
                            // MessageBox.Show(source.DataContext.ToString());
                            var p1 = new Point(GraphCanvas.GetX(source), GraphCanvas.GetY(source));
                            var target = edgeControl.Target;
                            var p2 = new Point(GraphCanvas.GetX(target), GraphCanvas.GetY(target));
                            animateMessage.edgeId = pocEdge.ID;
                            if (animateMessage.pointPath.Count == 0)
                            {
                                animateMessage.currentPoint = p1;
                                //animateMessage.currentPointId++;
                                animateMessage.pointPath.Add(p1);
                            }

                            if (edgeControl.RoutePoints != null)
                            {
                                animateMessage.pointPath.AddRange(edgeControl.RoutePoints);
                            }

                            animateMessage.pointPath.Add(p2);


                        }
                    }
                }
            }

            messages.Add(animateMessage);
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
                    break;
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
        #endregion
        #region Message Animation

        private class AnimateMessage
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
        private void DrawAnimtaedMessageLine(Point currentPoint, Point point)
        {
            RemoveLines();
            Line line = new Line();
            line.X1 = currentPoint.X;
            line.X2 = point.X;
            line.Y1 = currentPoint.Y;
            line.Y2 = point.Y;
            SolidColorBrush redBrush = new SolidColorBrush();
            redBrush.Color = Colors.Red;
            line.StrokeThickness = 4;
            line.Stroke = redBrush;

            // ((PocEdge)((System.Windows.FrameworkElement)this.graphLayout.Children[0]).DataContext).ID
            this.graphLayout.Children.Add(line);
        }
        List<AnimateMessage> messages = new List<AnimateMessage>();
        private void dispatcherTimer_Tick(object sender, EventArgs e)
        {
            // code goes here
            // 1. Get EdgeControl
            // 2. Get source and target
            // 3. Get route points
            // 4. Iterate                                  
            for (int message = 0; message < messages.Count; message++)
            {
                if (messages[message].isFinished)
                {
                    continue;
                }
                if (messages[message].currentPointId + 1 >= messages[message].pointPath.Count())
                {
                    messages[message].isFinished = true;
                    continue;
                }
                Point point = MovePointTowards(messages[message].currentPoint, messages[message].pointPath[messages[message].currentPointId + 1], messages[message].distance);
                DrawAnimtaedMessageLine(currentPoint: messages[message].currentPoint,
                    point: point);
                messages[message].currentPoint = point;

                if (Math.Abs(messages[message].currentPoint.X - messages[message].pointPath[messages[message].currentPointId + 1].X) < 20 &&
                    Math.Abs(messages[message].currentPoint.Y - messages[message].pointPath[messages[message].currentPointId + 1].Y) < 20)
                {
                    messages[message].distance = 1;
                    if (messages[message].currentPointId + 1 >= messages[message].pointPath.Count)
                    {
                        messages[message].isFinished = true;
                    }
                    else
                    {
                        messages[message].currentPoint = messages[message].pointPath[messages[message].currentPointId + 1];
                        messages[message].currentPointId++;
                    }
                }
                else
                {
                    messages[message].distance = 10;
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
                    dispatcherTimer.Start();
                    break;
                case OperationState.SendMessage:
                    operationState = OperationState.None;
                    dispatcherTimer.Stop();
                    this.SendMessage.Source = bitmapMessage;
                    break;
                default:
                    break;
            }
        }

        private Point MovePointTowards(Point a, Point b, int distance)
        {
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
            // TODO: send init params to the edgeSettings
            // 1. weight
            // 2. error propbability
            // 3. Number
            // 4. Дуплекс/ полудуплекс
            edgeSettings.Show();
        }
        #endregion
    }
}
