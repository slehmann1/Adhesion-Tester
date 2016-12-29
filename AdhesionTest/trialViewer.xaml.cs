using System;
using System.Linq;
using System.Threading;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using Timer = System.Timers.Timer;

namespace AdhesionTest
{
    /// <summary>
    ///     Interaction logic for trialViewer.xaml
    /// </summary>
    public partial class trialViewer : Page
    {
        private readonly DateTime startTime;
        private readonly trialManager trial;

        public trialViewer(trialManager trial)
        {
            InitializeComponent();
            MainViewModel.normalForceLabel = normalForceLabel;
            MainViewModel.shearForceLabel = shearForceLabel;
            trial.adhesionAdded += adhesionAddedEvent;
            this.trial = trial;
            trial.updateCycleCount();
            updateTrialStatus(trial);

            var myTimer = new Timer();
            myTimer.Elapsed += timer_Tick;
            myTimer.Interval = 1000;
            myTimer.Start();

            startTime = DateTime.Now;

            var trialThread = new Thread(trial.runTrial);
            trialThread.IsBackground = true;
            trialThread.Start();
        }

        private void timer_Tick(object sender, ElapsedEventArgs e)
        {
            //This is done with a dispatcher for thread safety
            Dispatcher.Invoke(() =>
            {
                var timeDiff = DateTime.Now - startTime;
                elapsedTimeText.Text = "Time elapsed: " + ((int) timeDiff.TotalMinutes).ToString("0") + " minutes and " +
                                       (timeDiff.TotalSeconds%60).ToString("0") + " seconds";
            });
        }

        /// <summary>
        ///     Updates the label handling the indication of which cycle the test is currently on
        /// </summary>
        private void updateTrialStatus(trialManager trial)
        {
            cycleIndicatorText.Text = "Testing Cycle " + (trial.adhesionValues.Count + 1) + " of " + trial.totalCycles;
        }

        private void adhesionAddedEvent(object sender, adhesionAddedEventArgs e)
        {
            //This is done with a dispatcher for thread safety
            Dispatcher.Invoke(() =>
            {
                averageAdhesionText.Text = "Average adhesion: " + e.adhesionValues.Average();
                maximumAdhesionText.Text = "Maximum adhesion: " + e.adhesionValues.Min();
                previousAdhesionText.Text = "Last adhesion: " + e.adhesionValues[e.adhesionValues.Count - 1];
                lastPreloadText.Text = "Last preload: " + e.preloadValues[e.preloadValues.Count - 1];
                updateTrialStatus(trial);
            });
        }

        private void stopTrialClick(object sender, RoutedEventArgs e)
        {
            trial.abortTrial();
            MainWindow.mainFrame.Navigate(new trialComplete()); //Move back to the start
        }
    }
}