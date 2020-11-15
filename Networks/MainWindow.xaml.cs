using QuickGraph;
using System;
using System.Collections.Generic;
using System.Linq;
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
        private MainWindowViewModel vm;
        double RouterMenuHeight;
        double RouterMenuWidth;

        public MainWindow()
        {
            vm = new MainWindowViewModel();
            this.DataContext = vm;
            this.SizeChanged += OnWindowSizeChanged;
            InitializeComponent();
        }

        private void ButtonAddNewRouter_Click(object sender, RoutedEventArgs routedEventArgs)
        {
            vm.AddNewVertex();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
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
          
            this.RouterMenu.Visibility = Visibility.Visible;
            this.RouterMenu.Width = RouterMenuWidth;
            this.RouterMenu.Height = RouterMenuHeight;

        }

        private void graphLayout_MouseDown(object sender, MouseButtonEventArgs e)
        {
            this.RouterMenu.Visibility = Visibility.Hidden;
        }

        private void ZoomControl_MouseDown(object sender, MouseButtonEventArgs e)
        {
            this.RouterMenu.Visibility = Visibility.Hidden;
        }
    }
}
