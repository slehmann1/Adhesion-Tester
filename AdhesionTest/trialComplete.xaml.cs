using System.Windows;
using System.Windows.Controls;

namespace AdhesionTest
{
    /// <summary>
    ///     Interaction logic for trialComplete.xaml
    /// </summary>
    public partial class trialComplete : Page
    {
        public trialComplete()
        {
            InitializeComponent();
        }

        private void continue_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.mainFrame.Navigate(new calibration());
        }
    }
}