using System;
using System.Collections.Generic;
using System.Threading;
using System.Windows;
using System.Windows.Threading;
using NationalInstruments.DAQmx;

namespace AdhesionTest
{
    /// <summary>
    ///     DAQ=Data acquisition. This class interfaces with the National instruments hub, to collect information from the
    ///     force probes
    /// </summary>
    public class DAQ
    {
        public delegate void dataAcquired(object sender, EventArgs e);

        private readonly Task analogInTask = new Task();


        private readonly acquiredData[] colData = new acquiredData[Constants.NUMSAMPLES];
        private AIChannel normalForceChannel, shearForceChannel;
        private readonly AnalogMultiChannelReader reader;
        private readonly DateTime start;

        /// <summary>
        ///     Constructs a DAQ object.
        /// </summary>
        /// <param name="normalChannelId">The channel of the normal force probe</param>
        /// <param name="shearChannelId">The channel of the shear force probe</param>
        public DAQ(string normalChannelId, string shearChannelId)
        {
            Console.WriteLine("RUN INIT");
            //Initialize lists
            dataPoints = new List<dataPoint>();
            start = DateTime.Now;
            try
            {
                //Define channels
                normalForceChannel = analogInTask.AIChannels.CreateVoltageChannel(normalChannelId,
                    Constants.NORMALCHANNELNAME, AITerminalConfiguration.Differential, 0, 5, AIVoltageUnits.Volts);
                shearForceChannel = analogInTask.AIChannels.CreateVoltageChannel(shearChannelId,
                    Constants.SHEARCHANNELNAME, AITerminalConfiguration.Differential, 0, 5, AIVoltageUnits.Volts);

                //Define reader
                reader = new AnalogMultiChannelReader(analogInTask.Stream);

                var dataCollection = new Thread(beginTrial);
                dataCollection.IsBackground = true;
                dataCollection.Start();

                beginTrial();
            }
            catch (DaqException e)
            {
                Console.Write("DAQEXCEPTION: " + e.Message);
                MessageBox.Show(
                    "Failed to connect to force probes. Please check that they are connected properly and that all required drivers are installed.");
                var data = new dataPoint();
                data.time = -1;
                data.normalForce = 0;
                data.shearForce = 0;
                dataPoints.Add(data);
            }
        }

        public List<dataPoint> dataPoints { get; }

        public double normalOffset { get; set; } = 0;
        public double shearOffset { private get; set; } = 0;

        public double normalGain { get; set; } = 98.1;
        public double shearGain { private get; set; } = 98.1;
        public event dataAcquired dataAcquiredEvent; //This event is used for interfacing with the UI

        private void beginTrial()
        {
            var dispatcherTimer = new DispatcherTimer {Interval = new TimeSpan(0, 0, 0, 0, Constants.DAQFREQ)};
            dispatcherTimer.Tick += dispatcherTimer_Tick;
            dispatcherTimer.Start();
        }

        private void dispatcherTimer_Tick(object sender, EventArgs e)
        {
            Dispatcher.CurrentDispatcher.Invoke(() =>
            {
                for (var sampleCount = 0; sampleCount < Constants.NUMSAMPLES; sampleCount++)
                {
                    colData[sampleCount] = acquireData();
                    //adjust the data
                    colData[sampleCount].normalForce *= normalGain;
                    colData[sampleCount].normalForce += normalOffset;
                    colData[sampleCount].shearForce *= shearGain;
                    colData[sampleCount].shearForce += shearOffset;
                }
                //Average the data
                //This just does a simple linear average, which is what was done in the original labview program. This could be changed.
                double shearSum = 0;
                double normalSum = 0;

                foreach (var datum in colData)
                {
                    shearSum += datum.shearForce;
                    normalSum += datum.normalForce;
                }

                var data = new dataPoint();
                data.normalForce = normalSum/Constants.NUMSAMPLES;
                data.shearForce = shearSum/Constants.NUMSAMPLES;
                data.time = (DateTime.Now - start).TotalSeconds;
                dataPoints.Add(data);

                dataAcquiredEvent(this, new EventArgs());
            });
        }

        /// <summary>
        ///     Reads a single datapoint from the NI hub
        /// </summary>
        /// <returns>A struct of type acquiredData containing the data </returns>
        private acquiredData acquireData()
        {
            try
            {
                var data = reader.ReadSingleSample();

                var returnData = new acquiredData();
                returnData.normalForce = data[0];
                returnData.shearForce = data[1];
                return returnData;
            }
            catch
            {
                var returnData = new acquiredData();
                returnData.normalForce = 0;
                returnData.shearForce = 0;
                return returnData;
            }
        }

        public struct dataPoint
        {
            public double time { get; set; }
            public double normalForce { get; set; }
            public double shearForce { get; set; }
        }

        /// <summary>
        ///     The return type of the acquire data function
        /// </summary>
        private struct acquiredData
        {
            public double normalForce, shearForce;
        }
    }
}