using GraphSharp.Controls;
using Networks.PopupWindows;
using Networks.Services;
using QuickGraph;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Networks
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        #region Private Members
        private MainWindowViewModel vm;
        private double RouterMenuHeight;
        private double RouterMenuWidth;
        private System.Windows.Threading.DispatcherTimer dispatcherTimer = new System.Windows.Threading.DispatcherTimer();
        private bool isAddNewLinkSelected;
        private bool isInProgressOfCreatingNewLink;
        private bool isSendMessageSelected;

        private readonly BitmapImage bitmapSelectedLine;
        private readonly BitmapImage bitmapLine;

        private readonly BitmapImage bitmapSelectedMessage;
        private readonly BitmapImage bitmapMessage;

        private Point startPoint;
        private Point startStackPanelPoint;

        private int currentSelectedRouter = 0;
        private String fromRouter = String.Empty;
        private String toRouter = String.Empty;
        #endregion
        #region Constructor
        public MainWindow()
        {
            isAddNewLinkSelected = false;
            isInProgressOfCreatingNewLink = false;
            isSendMessageSelected = false;
            bitmapSelectedLine = new BitmapImage(new Uri("pack://application:,,,/Images/selected_line.png"));
            bitmapLine = new BitmapImage(new Uri("pack://application:,,,/Images/line.png"));
            bitmapSelectedMessage = new BitmapImage(new Uri("pack://application:,,,/Images/message_yellow.png"));
            bitmapMessage = new BitmapImage(new Uri("pack://application:,,,/Images/message_gray.png"));

            dispatcherTimer.Tick += new EventHandler(dispatcherTimer_Tick);
            dispatcherTimer.Interval = new TimeSpan(0, 5, 0);


            vm = new MainWindowViewModel();
            this.DataContext = vm;
            this.SizeChanged += OnWindowSizeChanged;
            InitializeComponent();
        }
        #endregion
        #region Private Methods

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

            if (isAddNewLinkSelected)
            {
                StackPanel stackPanel = (StackPanel)sender;
                isInProgressOfCreatingNewLink = true;
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
            }
            else if (!isInProgressOfCreatingNewLink)
            {
                this.RouterMenu.Visibility = Visibility.Visible;
                this.RouterMenu.Width = RouterMenuWidth;
                this.RouterMenu.Height = RouterMenuHeight;

                ResetDrawingMode();
            }
        }

        private void ResetDrawingMode()
        {
            RemoveLines();
            currentSelectedRouter = 0;
            isInProgressOfCreatingNewLink = false;

            fromRouter = String.Empty;
            toRouter = String.Empty;
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
            isAddNewLinkSelected = !isAddNewLinkSelected;
            if (isAddNewLinkSelected)
            {
                this.AddLink.Source = bitmapSelectedLine;
            }
            else
            {
                this.AddLink.Source = bitmapLine;
                ResetDrawingMode();
            }
        }

        private void zoomControl_MouseMove(object sender, MouseEventArgs e)
        {
            var f = e.GetPosition(this.graphLayout);

            textBox.Text = ((Int32)f.X).ToString() + "," + ((Int32)f.Y).ToString();
            if (isAddNewLinkSelected && isInProgressOfCreatingNewLink)
            {
                RemoveLines();
                var endPoint = e.GetPosition(this.graphLayout);
                //var endPoint = e.GetPosition(this);
                //endPoint.X -= 85;
                //endPoint.Y -= 120;
                CreateLine(endPoint);
            }
        }
        #endregion

        private void ButtonAddNewComputer_Click(object sender, RoutedEventArgs e)
        {
            vm.AddNewVertex(Data.NetworkType.COMPUTER);
        }

        private void zoomControl_LayoutUpdated(object sender, EventArgs e)
        {

        }


        private void dispatcherTimer_Tick(object sender, EventArgs e)
        {
            // code goes here
        }

        private void ButtonSendMessage_Click(object sender, RoutedEventArgs e)
        {
            isSendMessageSelected = !isSendMessageSelected;
            if (isSendMessageSelected)
            {
                this.SendMessage.Source = bitmapSelectedMessage;
                dispatcherTimer.Start();
            }
            else
            {
                dispatcherTimer.Stop();
                this.SendMessage.Source = bitmapMessage;
            }
        }
        private Point BuildLine(Point p1, Point p2)
        {           
            if (p1.X <= p2.X)
            {
                if (p1.X > 0)
                {
                    for (double x = p1.X + it; x <= p2.X; x += it)
                    {
                        double y = (x - p1.X) * ((p2.Y - p1.Y) / (p2.X - p1.X)) + p1.Y;
                        prevPoint = new Point(x, y);
                        return prevPoint;
                    }
                }
                else
                {
                    for (double x = p1.X + it; x <= p2.X; x -= it)
                    {
                        double y = (x - p1.X) * ((p2.Y - p1.Y) / (p2.X - p1.X)) + p1.Y;
                        prevPoint = new Point(x, y);
                        return prevPoint;
                    }
                }
            }
            else
            {
                if (p1.X > 0)
                {
                    for (double x = p1.X + it; x >= p2.X; x -= it)
                    {
                        double y = (x - p1.X) * ((p2.Y - p1.Y) / (p2.X - p1.X)) + p1.Y;
                        prevPoint = new Point(x, y);
                        return prevPoint;
                    }
                }
                else
                {
                    for (double x = p1.X + it; x >= p2.X; x += it)
                    {
                        double y = (x - p1.X) * ((p2.Y - p1.Y) / (p2.X - p1.X)) + p1.Y;
                        prevPoint = new Point(x, y);
                        return prevPoint;
                    }
                }

            }
            return prevPoint;
        }
        int it = 10;
        Point prevPoint = new Point(-1,-1);
        private void edgePath_MouseDown(object sender, MouseButtonEventArgs e)
        {
            RemoveLines();
            it += 10;
            EdgeSettingsWindow edgeSettings = new EdgeSettingsWindow();
            EdgeControl edgeControl = (EdgeControl)sender;
            if (edgeControl == null)
                return;
            var source = edgeControl.Source;
           // MessageBox.Show(source.DataContext.ToString());
            var p1 = new Point(GraphCanvas.GetX(source), GraphCanvas.GetY(source));
            var target = edgeControl.Target;
            var p2 = new Point(GraphCanvas.GetX(target), GraphCanvas.GetY(target));
            //var rect = new Rect(p1.X, p1.Y, 500, 500);
            Rectangle rectangle = new Rectangle();
            rectangle.Width = 40;
            rectangle.Height = 20;
            rectangle.Fill = new SolidColorBrush(Colors.Red);
            if (prevPoint.X != -1 && prevPoint.Y != -1)
            {
                p1 = prevPoint;
            }
            p2 = BuildLine(p1, p2);
            Line line = new Line();
            
            line.X1 = p1.X;
            line.X2 = p2.X;
            line.Y1 = p1.Y;
            line.Y2 = p2.Y;
            SolidColorBrush redBrush = new SolidColorBrush();
            redBrush.Color = Colors.Red;

            // Set Line's width and color  
            line.StrokeThickness = 4;
            line.Stroke = redBrush;
            //rectangle.Arrange(new Rect(p1, p2));
            // ((PocEdge)((System.Windows.FrameworkElement)this.graphLayout.Children[0]).DataContext).ID
            this.graphLayout.Children.Add(line);
            //Arrange(rect);
           // edgeSettings.Show();
        }
    }
}
