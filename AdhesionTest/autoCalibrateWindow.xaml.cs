using System;
using System.Threading.Tasks;
using System.Windows;
using MahApps.Metro.Controls;

namespace AdhesionTest
{
    /// <summary>
    ///     This class defines the autocalibration function, which allows for the semi-automatic callibration of the normal
    ///     force probe.
    /// </summary>
    public partial class autoCalibrateWindow : MetroWindow
    {
        private const int WAITTIME = 5000;
            // How many ms to wait between steps, used to establish a baseline, this is the amount of time samples

        private const int EXTRAWAITTIME = 250; // How many ms to wait between steps, this time is unsampled
        private const string unweightedText = "Remove all weights and loads from the normal force probe";
        private const string weightedText = "Add a 1 gram weight to the normal probe";
        private const double targetValue = -9.80665; //Gravitational acceleration constant
        private readonly calibration caller;
        private bool unweightedComplete; //whether or not the unweighted operation is finished

        public autoCalibrateWindow(calibration caller)
        {
            this.caller = caller;
            InitializeComponent();
            mainText.Text = unweightedText;
        }

        

        //Ran when the button is pressed. Must be asyncronous to allow UI updating
        private async void nextStep(object sender, RoutedEventArgs e)
        {
            nextStepButton.IsEnabled = false; //prevent further clicking
            loadingIcon.Visibility = Visibility.Visible;

            var nextText = "Calibration complete";
            Console.WriteLine("PRE");
            await Task.Delay(WAITTIME + EXTRAWAITTIME);
            Console.WriteLine("WAITED");
            if (!unweightedComplete)
            {
                unweightedComplete = true;
                nextText = weightedText;
                caller.setNormalOffset(-baselineManager.getBaseline(WAITTIME) + MainViewModel.dataAcquirer.normalOffset);
            }
            else
            {
                nextText = weightedText;
                caller.setNormalGain(targetValue/ baselineManager.getBaseline(WAITTIME)*MainViewModel.dataAcquirer.normalGain);
                Close();
                return;
            }

            mainText.Text = nextText;
            nextStepButton.IsEnabled = true;
            loadingIcon.Visibility = Visibility.Hidden;
        }
    }
}