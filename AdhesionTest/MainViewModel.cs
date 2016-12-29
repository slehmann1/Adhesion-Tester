using System;
using System.Collections.Generic;
using System.Windows.Controls;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;

namespace AdhesionTest
{
    public class MainViewModel
    {
        private const string NORMTITLE = "Normal Force";
        private const string SHEARTITLE = "Shear Force";
        private const int TIMEHISTORY = 600; //The amount of time history that should be kept in the graph

        public MainViewModel()
        {
            smoothData = false;

            normalPlot = new PlotModel();
            normalPlot.Series.Add(new LineSeries());
            normalPlot.Title = NORMTITLE;
            normalPlot.Axes.Add(new LinearAxis {Position = AxisPosition.Bottom, Title = "Time Elapsed (seconds)"});
            normalPlot.Axes.Add(new LinearAxis {Position = AxisPosition.Left, Title = "Force (mN)"});
            normalPlot.DefaultColors = new List<OxyColor>
            {
                OxyColor.FromRgb(30, 144, 255)
            };

            shearPlot = new PlotModel();
            shearPlot.Series.Add(new LineSeries());
            shearPlot.DefaultColors = new List<OxyColor>
            {
                OxyColor.FromRgb(30, 144, 255)
            };
            shearPlot.Title = SHEARTITLE;
            shearPlot.Axes.Add(new LinearAxis {Position = AxisPosition.Bottom, Title = "Time Elapsed (seconds)"});
            shearPlot.Axes.Add(new LinearAxis {Position = AxisPosition.Left, Title = "Force (mN)"});

            timerStart = DateTime.Now;
        }

        private static DateTime timerStart { get; set; }

        public static PlotModel normalPlot { get; set; }
        public static PlotModel shearPlot { get; set; }

        public static Label normalForceLabel { get; set; }
        public static Label shearForceLabel { get; set; }

        public static bool smoothData { get; set; }
        public static DAQ dataAcquirer { get; private set; }

        public static void setDataAcquirer(DAQ value)
        {
            dataAcquirer = value;
            dataAcquirer.dataAcquiredEvent += updateGraph;
        }

        private static void updateGraph(object sender, EventArgs e)
        {
            if (dataAcquirer.dataPoints.Count > 0)
            {
                var time = (DateTime.Now - timerStart).TotalMilliseconds;
                time /= 1000;
                (normalPlot.Series[0] as LineSeries).Points.Add(new DataPoint(time,
                    dataAcquirer.dataPoints[dataAcquirer.dataPoints.Count - 1].normalForce));
                //If there are too many elements, remove the first
                if ((normalPlot.Series[0] as LineSeries).Points.Count*Constants.DAQFREQ > TIMEHISTORY)
                {
                    (normalPlot.Series[0] as LineSeries).Points.RemoveAt(0);
                }

                (normalPlot.Series[0] as LineSeries).Smooth = smoothData;
                normalPlot.InvalidatePlot(true);

                (shearPlot.Series[0] as LineSeries).Points.Add(new DataPoint(time,
                    dataAcquirer.dataPoints[dataAcquirer.dataPoints.Count - 1].shearForce));

                //If there are too many elements, remove the first
                if ((shearPlot.Series[0] as LineSeries).Points.Count*Constants.DAQFREQ > TIMEHISTORY)
                {
                    (shearPlot.Series[0] as LineSeries).Points.RemoveAt(0);
                }

                (shearPlot.Series[0] as LineSeries).Smooth = smoothData;
                shearPlot.InvalidatePlot(true);

                normalForceLabel.Content =
                    dataAcquirer.dataPoints[dataAcquirer.dataPoints.Count - 1].normalForce.ToString("0.000");
                shearForceLabel.Content =
                    dataAcquirer.dataPoints[dataAcquirer.dataPoints.Count - 1].shearForce.ToString("0.000");
            }
            else
            {
                Console.WriteLine("NO DATA POINTS");
            }
        }
    }
}