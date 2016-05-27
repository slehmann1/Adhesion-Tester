using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;

using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using MahApps.Metro.Controls;
using OxyPlot;
using System.Windows.Threading;

namespace AdhesionTest
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MetroWindow
    {
        DAQ dataAcquirer;
        public MainWindow()
        {
            dataAcquirer = new DAQ(Constants.NORMALCHANNELID, Constants.SHEARCHANNELID);
            MainViewModel.dataAcquirer = dataAcquirer;
            InitializeComponent();
            DispatcherTimer dispatcherTimer = new DispatcherTimer { Interval = new TimeSpan(0, 0, 0, 0, Constants.DAQFREQ) };
            dispatcherTimer.Tick += dispatcherTimer_Tick;
            dispatcherTimer.Start();

        }
        private void dispatcherTimer_Tick(object sender, EventArgs e)
        {
            Dispatcher.CurrentDispatcher.Invoke(() =>
            {
                if (dataAcquirer.normalForces.Count != 0)
                {
                    normalForceLabel.Content = dataAcquirer.normalForces[dataAcquirer.normalForces.Count - 1].ToString("0.000");
                    shearForceLabel.Content = dataAcquirer.shearForces[dataAcquirer.shearForces.Count - 1].ToString("0.000");
                }
            });
        }

        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            MainViewModel.smoothData = true;
        }

        private void CheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            MainViewModel.smoothData = false;
        }

        private void TextBoxPreviewTextInput(object sender, TextCompositionEventArgs e)
        {

            if (!sanitizeInput(e.Text))
            {
                //Invalid input
                e.Handled = true;
            }
        }

        public void updateLabels(string normalLabel, string shearLabel)
        {
            normalForceLabel.Content = normalLabel;
            shearForceLabel.Content = shearLabel;
        }

        /// <summary>
        /// Ensures that only numbers/decimals are input
        /// </summary>
        /// <param name="inputString"></param>
        /// <returns>Returns true if valid</returns>
        private bool sanitizeInput(string inputString)
        {
            if (!char.IsDigit(inputString, inputString.Length - 1) && !(inputString.ElementAt(inputString.Length - 1) == '.') && !(inputString.ElementAt(inputString.Length - 1) == '-'))
            {
                return false;
            }
            return true;
        }

        private void textBox_KeyDown(object sender, KeyEventArgs e)
        {
            double value = 0;
            if (sender.Equals(normalOffsetBox))
            {
                try
                {
                    value = Convert.ToDouble(normalOffsetBox.Text);
                }
                catch (FormatException) { return; };
                MainViewModel.dataAcquirer.normalOffset = value;
            }
            else if (sender.Equals(shearOffsetBox))
            {
                try
                {
                    value = Convert.ToDouble(shearOffsetBox.Text);
                }
                catch (FormatException) { return; };
                MainViewModel.dataAcquirer.shearOffset = value;
            }
            else if (sender.Equals(normalGainBox))
            {
                try
                {
                    value = Convert.ToDouble(normalGainBox.Text);
                }
                catch (FormatException) { return; };
                MainViewModel.dataAcquirer.normalGain = value;
            }
            else if (sender.Equals(shearGainBox))
            {
                try
                {
                    value = Convert.ToDouble(shearGainBox.Text);
                }
                catch (FormatException) { return; };
                MainViewModel.dataAcquirer.shearGain = value;
            }
            else
            {
                throw new Exception("Unaccounted for input box");
            }

        }
    }
}



