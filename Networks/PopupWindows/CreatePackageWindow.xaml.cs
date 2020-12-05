using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Networks.PopupWindows
{
    /// <summary>
    /// Interaction logic for CreatePackageWindow.xaml
    /// </summary>
    public partial class CreatePackageWindow : Window
    {
        public bool IsSaved { get; private set; }

        private void NumberValidationTextBox(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
        }
        public CreatePackageWindow()
        {
            InitializeComponent();
        }

        private void txtSize_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            string richText = new TextRange(richTextBox.Document.ContentStart, richTextBox.Document.ContentEnd).Text;
            if (richText.Length > Convert.ToInt32(txtSize.Text))
            {
                MessageBox.Show("Data is bigger than size");
            }
            else
            {
                IsSaved = false;
                this.Close();
            }

        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            string richText = new TextRange(richTextBox.Document.ContentStart, richTextBox.Document.ContentEnd).Text;
            if (richText.Length > Convert.ToInt32(txtSize.Text))
            {
                MessageBox.Show("Data is bigger than size");
            }
            else
            {
                IsSaved = false;
                this.Close();
            }

        }

        private void richTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            //RichTextBox richTextBox = (RichTextBox)sender;
            //string richText = new TextRange(richTextBox.Document.ContentStart, richTextBox.Document.ContentEnd).Text;
            //if (richText.Length >= Convert.ToInt32(txtSize.Text){

            //}
        }
        private static Random random = new Random();
        public static string RandomString(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789!@#$%^&*()";
            return new string(Enumerable.Repeat(chars, length)
              .Select(s => s[random.Next(s.Length)]).ToArray());
        }
        private void checkBox_Checked(object sender, RoutedEventArgs e)
        {
            richTextBox.Document.Blocks.Clear();
            richTextBox.Document.Blocks.Add(new Paragraph(new Run(RandomString(Convert.ToInt32(txtSize.Text)))));
        }

        private void checkBox_Unchecked(object sender, RoutedEventArgs e)
        {

        }
    }
}
