using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AdhesionTest
{
    class baselineManager
    {
        const int numDeflectionDatum =3; //The number of datum required to be outside of the range before a deflection is recognised
        const int numReturnDatum = 10; //The number of datum required to be within the range before a return to baseline is recognised
        const double deflectionAmount = 0.1;
        private static bool waitingForDeflection = false;
        private static bool returningToBaseline = false;
        private static double baseline;
        const int baselineWaitTime = 500;
        private static int currSuccessiveDatum = 0;

        /// <summary>
        /// Sets the baseline to be used for deflections
        /// </summary>
        public static void establishDeflectionBaseline()
        {
            baseline = getBaseline(baselineWaitTime);
            Console.WriteLine("BASELINE: " + baseline);
            MainViewModel.dataAcquirer.dataAcquiredEvent += dataAcquired;
        }

        public static void waitForDeflection()
        {
            waitingForDeflection = true;

            while (true)
            {
                if (currSuccessiveDatum >= numDeflectionDatum)
                {
                    currSuccessiveDatum = 0;
                    waitingForDeflection = false;
                    return;
                }
                Thread.Sleep(1);
            }
        }

        public static void waitForBaselineReturn()
        {
            returningToBaseline = true;

            while (true)
            {
                Console.WriteLine(currSuccessiveDatum);
                if (currSuccessiveDatum >= numReturnDatum)
                {
                    currSuccessiveDatum = 0;
                    returningToBaseline = false;
                    return;
                }
                Thread.Sleep(1);
            }
        }

        /// <summary>
        ///     An event that fires when data has been acquired, checks whether or not the preload has been reached
        /// </summary>
        protected static void dataAcquired(object Sender, EventArgs e)
        {
            if (waitingForDeflection)
            {
                var force =
                MainViewModel.dataAcquirer.dataPoints[MainViewModel.dataAcquirer.dataPoints.Count - 1].normalForce;
                if (Math.Abs(force - baseline) > deflectionAmount)
                {
                    currSuccessiveDatum++;
                }
                else if (currSuccessiveDatum < numDeflectionDatum)//Don't reset it if it is beyond the limit
                {
                    currSuccessiveDatum = 0;
                }

            }
            else if (returningToBaseline)
            {
                var force =
                MainViewModel.dataAcquirer.dataPoints[MainViewModel.dataAcquirer.dataPoints.Count - 1].normalForce;
                if (Math.Abs(force - baseline) < deflectionAmount)
                {
                    currSuccessiveDatum++;
                }
                else if (currSuccessiveDatum < numReturnDatum)//Don't reset it if it is beyond the limit
                {
                    currSuccessiveDatum = 0;
                }
            }
        }

        /// <summary>
        ///     Does a simple average over the time history to return a baseline value
        /// </summary>
        public static double getBaseline(int waitTime)
        {
            try
            {
                double sum = 0;
                var numSamples = 0;
                var startTime =
                    MainViewModel.dataAcquirer.dataPoints[MainViewModel.dataAcquirer.dataPoints.Count - 1].time;
                for (var i = MainViewModel.dataAcquirer.dataPoints.Count - 1; i > 0; i--)
                {
                    //Break from loop if been through enough time history
                    if (startTime - MainViewModel.dataAcquirer.dataPoints[i].time > waitTime / 1000)
                    {
                        break;
                    }

                    sum += MainViewModel.dataAcquirer.dataPoints[i].normalForce;
                    numSamples += 1;
                }
                Console.WriteLine("BASELINE: " + sum / numSamples);
                return sum / numSamples;
            }
            catch
            {
                return -1; //if there are no samples
            }
        }
    }
}
