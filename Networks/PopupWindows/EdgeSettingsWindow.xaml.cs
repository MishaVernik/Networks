using System.Windows;

namespace Networks.PopupWindows
{
    /// <summary>
    /// Interaction logic for EdgeSettingsWindow.xaml
    /// </summary>
    public partial class EdgeSettingsWindow : Window
    {
        public EdgeSettingsWindow()
        {
            InitializeComponent();
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void btnOK_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
