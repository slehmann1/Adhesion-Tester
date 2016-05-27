using NationalInstruments;
using NationalInstruments.DAQmx;
using System.Windows.Threading;
using System;
using System.Collections.Generic;

namespace AdhesionTest 
{
    /// <summary>
    /// DAQ=Data acquisition. This class interfaces with the National instruments hub, to collect information from the force probes
    /// </summary>
    public class DAQ 
    {
        private AIChannel normalForceChannel, shearForceChannel;
        private Task analogInTask = new Task();
        private AnalogMultiChannelReader reader;

        public List<double> normalForces { get; private set; }
        public List<double> shearForces { get; private set; }

        public double normalOffset { private get; set; } = 0;
        public double shearOffset { private get; set; } = 0;

        public double normalGain { private get; set; } = 98.1;
        public double shearGain { private get; set; } = 98.1;


        /// <summary>
        /// The return type of the acquire data function
        /// </summary>
        struct acquiredData
        {
            public double normalForce, shearForce;
        }

        private int sampleCount = 0;
        private acquiredData[] colData = new acquiredData[Constants.NUMSAMPLES];

        /// <summary>
        /// Constructs a DAQ object.
        /// </summary>
        /// <param name="normalChannelId">The channel of the normal force probe</param>
        /// <param name="shearChannelId">The channel of the shear force probe</param>
        public DAQ(string normalChannelId, string shearChannelId)
        {
            //Initialize lists
            normalForces = new List<double>();
            shearForces = new List<double>();

            try
            {
                //Define channels
                normalForceChannel = analogInTask.AIChannels.CreateVoltageChannel(normalChannelId, Constants.NORMALCHANNELNAME, AITerminalConfiguration.Differential, 0, 5, AIVoltageUnits.Volts);
                shearForceChannel = analogInTask.AIChannels.CreateVoltageChannel(shearChannelId, Constants.SHEARCHANNELNAME, AITerminalConfiguration.Differential, 0, 5, AIVoltageUnits.Volts);

                //Define reader
                reader = new AnalogMultiChannelReader(analogInTask.Stream);



                DispatcherTimer dispatcherTimer = new DispatcherTimer { Interval = new TimeSpan(0, 0, 0, 0, (Constants.DAQFREQ / Constants.NUMSAMPLES)) };
                dispatcherTimer.Tick += dispatcherTimer_Tick;
                dispatcherTimer.Start();
            }
            catch (DaqException e)
            {
                Console.Write("DAQEXCEPTION: " + e.Message);

                normalForces.Add(0);
                shearForces.Add(0);
            }

        }

        private void dispatcherTimer_Tick(object sender, EventArgs e)
        {
            Dispatcher.CurrentDispatcher.Invoke(() =>
            {
                Console.WriteLine(sampleCount);
                colData[sampleCount] = acquireData();

                //adjust the data
                colData[sampleCount].normalForce *= normalGain;
                colData[sampleCount].normalForce += normalOffset;
                colData[sampleCount].shearForce *= shearGain;
                colData[sampleCount].shearForce += shearOffset;
                if ((sampleCount + 1) % Constants.NUMSAMPLES == 0)
                {
                    //Average the data
                    //This just does a simple linear average, which is what was done in the original labview program. This could be changed.
                    double shearSum = 0;
                    double normalSum = 0;

                    foreach (acquiredData datum in colData)
                    {
                        shearSum += datum.shearForce;
                        normalSum += datum.normalForce;
                    }

                    normalForces.Add(normalSum / Constants.NUMSAMPLES);
                    shearForces.Add(shearSum / Constants.NUMSAMPLES);

                    sampleCount = 0;
                    MainViewModel.updateGraph();
                }
                else
                {
                    sampleCount++;
                }
            });
        }

        /// <summary>
        /// Reads a single datapoint from the NI hub
        /// </summary>
        /// <returns>A struct of type acquiredData containing the data </returns>
        private acquiredData acquireData()
        {
            double[] data = reader.ReadSingleSample();
            acquiredData returnData = new acquiredData();
            returnData.normalForce = data[0];
            returnData.shearForce = data[1];
            return returnData;
        }


    }
}
