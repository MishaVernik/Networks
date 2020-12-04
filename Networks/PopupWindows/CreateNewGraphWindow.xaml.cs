using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;

namespace Networks.PopupWindows
{
    /// <summary>
    /// Interaction logic for CreateNewGraphWindow.xaml
    /// </summary>
    public partial class CreateNewGraphWindow : Window
    {
        public CreateNewGraphWindow()
        {
            InitializeComponent();
        }

        private void NumberValidationTextBox(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
        }

        private void ButtonCreateGraph_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
