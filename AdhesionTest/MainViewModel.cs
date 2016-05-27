namespace AdhesionTest
{
    using OxyPlot;
    using OxyPlot.Series;
    using System;
    using System.Windows.Threading;
    public class MainViewModel
    {
        private const String NORMTITLE = "Normal Force";
        private const String SHEARTITLE = "Shear Force";
        private const int TIMEHISTORY = 60000;//The amount of time history that should be kept in the graph
        private static DateTime timerStart { get; set; }

        public static PlotModel normalPlot { get; set; }
        public static PlotModel shearPlot { get; set; }

        public static bool smoothData { get; set; }
        public static DAQ dataAcquirer { get; set; }

        public MainViewModel()
        {
            smoothData = false;

            normalPlot = new PlotModel();
            normalPlot.Series.Add(new LineSeries());
            normalPlot.Title = NORMTITLE;

            shearPlot = new PlotModel();
            shearPlot.Series.Add(new LineSeries());
            shearPlot.Title = SHEARTITLE;

            timerStart = DateTime.Now;
        }

        public static void updateGraph()
        {

            double time = (DateTime.Now - timerStart).TotalMilliseconds;
            time /= 1000;

            (normalPlot.Series[0] as LineSeries).Points.Add(new DataPoint(time, dataAcquirer.normalForces[dataAcquirer.normalForces.Count - 1]));

            //If there are too many elements, remove the first
            if ((normalPlot.Series[0] as LineSeries).Points.Count * Constants.DAQFREQ > TIMEHISTORY)
            {
                (normalPlot.Series[0] as LineSeries).Points.RemoveAt(0);
            }

                (normalPlot.Series[0] as LineSeries).Smooth = smoothData;
            normalPlot.InvalidatePlot(true);
            Console.WriteLine("GRAPH " + (normalPlot.Series[0] as LineSeries).Points.Count);

            (shearPlot.Series[0] as LineSeries).Points.Add(new DataPoint(time, dataAcquirer.shearForces[dataAcquirer.shearForces.Count - 1]));

            //If there are too many elements, remove the first
            if ((shearPlot.Series[0] as LineSeries).Points.Count * Constants.DAQFREQ > TIMEHISTORY)
            {
                (shearPlot.Series[0] as LineSeries).Points.RemoveAt(0);
            }

                (shearPlot.Series[0] as LineSeries).Smooth = smoothData;
            shearPlot.InvalidatePlot(true);


        }
    }
}