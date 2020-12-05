using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;

namespace Networks.PopupWindows
{
    /// <summary>
    /// Interaction logic for EdgeSettingsWindow.xaml
    /// </summary>
    public partial class EdgeSettingsWindow : Window
    {
        public bool IsSaved { get; set; }
        public EdgeSettingsWindow()
        {
            InitializeComponent();
        }
        private void NumberValidationTextBox(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
        }
        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            IsSaved = false;
            this.Close();
        }

        private void btnOK_Click(object sender, RoutedEventArgs e)
        {
            IsSaved = true;
            this.Close();
        }
    }
}
