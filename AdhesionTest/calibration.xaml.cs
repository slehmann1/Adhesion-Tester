using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using AdhesionTest.Properties;

namespace AdhesionTest
{
    /// <summary>
    ///     Interaction logic for calibration.xaml
    /// </summary>
    public partial class calibration : Page
    {
        private readonly DAQ dataAcquirer;

        public calibration()
        {
            if (MainViewModel.dataAcquirer == null)
            {
                dataAcquirer = new DAQ(Settings.Default.normalChannelId, Settings.Default.shearChannelId);
                MainViewModel.setDataAcquirer(dataAcquirer);
            }
            InitializeComponent();

            shearGainBox.Text = Settings.Default.ShearGain.ToString();
            normalGainBox.Text = Settings.Default.NormalGain.ToString();
            shearOffsetBox.Text = Settings.Default.ShearOffset.ToString();
            normalOffsetBox.Text = Settings.Default.NormalOffset.ToString();

            MainViewModel.dataAcquirer.normalOffset = Settings.Default.NormalOffset;
            MainViewModel.dataAcquirer.shearOffset = Settings.Default.ShearOffset;
            MainViewModel.dataAcquirer.normalGain = Settings.Default.NormalGain;
            MainViewModel.dataAcquirer.shearGain = Settings.Default.ShearGain;

            MainViewModel.normalForceLabel = normalForceLabel;
            MainViewModel.shearForceLabel = shearForceLabel;
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

        private void continue_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.mainFrame.Navigate(new runSelection());
        }

        /// <summary>
        ///     Ensures that only numbers/decimals are input
        /// </summary>
        /// <param name="inputString"></param>
        /// <returns>Returns true if valid</returns>
        private bool sanitizeInput(string inputString)
        {
            if (!char.IsDigit(inputString, inputString.Length - 1) &&
                !(inputString.ElementAt(inputString.Length - 1) == '.') &&
                !(inputString.ElementAt(inputString.Length - 1) == '-'))
            {
                return false;
            }
            return true;
        }

        /// <summary>
        ///     Sets the normal gain, updating both the UI and the DAQ
        /// </summary>
        public void setNormalGain(double value)
        {
            normalGainBox.Text = value.ToString();
            Settings.Default.NormalGain = value;
            Settings.Default.Save();
            MainViewModel.dataAcquirer.normalGain = value;
        }

        /// <summary>
        ///     Sets the normal offset, updating both the UI and the DAQ
        /// </summary>
        public void setNormalOffset(double value)
        {
            normalOffsetBox.Text = value.ToString();
            Settings.Default.NormalOffset = value;
            Settings.Default.Save();
            MainViewModel.dataAcquirer.normalOffset = value;
        }

        //This is done in this manner so that hte graphs update in real time
        private void textBox_KeyUp(object sender, KeyEventArgs e)
        {
            double value = 0;
            if (sender.Equals(normalOffsetBox))
            {
                try
                {
                    value = Convert.ToDouble(normalOffsetBox.Text);
                }
                catch (FormatException)
                {
                    return;
                }

                Settings.Default.NormalOffset = value;
                MainViewModel.dataAcquirer.normalOffset = value;
            }
            else if (sender.Equals(shearOffsetBox))
            {
                try
                {
                    value = Convert.ToDouble(shearOffsetBox.Text);
                }
                catch (FormatException)
                {
                    return;
                }
                Settings.Default.ShearOffset = value;
                MainViewModel.dataAcquirer.shearOffset = value;
            }
            else if (sender.Equals(normalGainBox))
            {
                try
                {
                    value = Convert.ToDouble(normalGainBox.Text);
                }
                catch (FormatException)
                {
                    return;
                }
                Settings.Default.NormalGain = value;
                MainViewModel.dataAcquirer.normalGain = value;
            }
            else if (sender.Equals(shearGainBox))
            {
                try
                {
                    value = Convert.ToDouble(shearGainBox.Text);
                }
                catch (FormatException)
                {
                    return;
                }
                Settings.Default.ShearGain = value;
                MainViewModel.dataAcquirer.shearGain = value;
            }
            else
            {
                throw new Exception("Unaccounted for input box");
            }
            Settings.Default.Save();
        }

        private void autoCalibrate_Click(object sender, RoutedEventArgs e)
        {
            var popup = new autoCalibrateWindow(this);
            popup.ShowDialog();
        }
    }
}