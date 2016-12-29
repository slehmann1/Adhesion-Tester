using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Windows;
using AdhesionTest.Properties;

namespace AdhesionTest
{
    public abstract class trialManager
    {
        public delegate void adhesionAddedEvent(object sender, adhesionAddedEventArgs e);

        //Definitions of the axes, they are defined this way for backwards compatibility
        protected const int ZAXIS = 1;
        protected const int XAXIS = 2;
        protected const int YAXIS = 3;
        private const double ACCELERATION = 5;
        protected static espManager esp;
        /// <summary>
        /// The amount of time allowed to return to a baseline
        /// </summary>
        protected const int baselineReturnTimeout = 30000;
        public List<double> adhesionValues = new List<double>();
        private double m_incomingVerticalAngle, m_OutgoingVerticalAngle;
        protected bool preloadReached;
        public List<double> preloadValues = new List<double>();
        protected int startPreloadIndex, endPreloadIndex;


        public trialManager()
        {
            try
            {
                if (!esp.initialized)
                {
                    esp = generateEspManager();
                }
            }
            catch (NullReferenceException)
            {
                esp = generateEspManager();
            }
            esp.changeUnits(1, espManager.unitOption.millimeter);
            esp.changeUnits(2, espManager.unitOption.millimeter);
            esp.changeUnits(3, espManager.unitOption.millimeter);

            esp.motorOn(XAXIS);
            esp.motorOn(YAXIS);
            esp.motorOn(ZAXIS);
            MainWindow.errorThrownEvent += errorThrown;
        }

        /// <summary>
        ///     Whether or not the default incoming and outgoing angles should be overriden, impacts writing to files. This is
        ///     useful for setting up
        ///     dynamic iterations of trials
        /// </summary>
        public bool overrideTrialDefaults { set; protected get; } = false;

        /// <summary>
        /// Whether or not to reverse the direction of the vertical axis for the trial. This allows the probe to be pressed up against a slide for viewing with a microscope
        /// </summary>
        public bool reverseDirection { protected get; set; }
        public double withdrawDistance { protected get; set; }
        public int numberOfTrials { get; set; }
        public bool collectFullData { get; set; }
        public bool accelerateMotion { get; set; }
        public double incomingVelocity { get; set; }
        public double outgoingVelocity { get; set; }
        public double acceleratedSpeed { protected get; set; }
        public double incomingVerticalAngle
        {
            get { return m_incomingVerticalAngle; }
            set { m_incomingVerticalAngle = value * Math.PI / 180; }
        } //from x-y plane

        public double outgoingVerticalAngle
        {
            get { return m_OutgoingVerticalAngle; }
            set { m_OutgoingVerticalAngle = value * Math.PI / 180; }
        } //from x-y plane
        public double preloadWaitTime { get; set; }
        public int totalCycles { get; protected set; }
        public int currentCycle { get; protected set; } = 1;
        public double dragVelocity { protected get; set; }

        public double desiredPreload { protected get; set; }
        public event adhesionAddedEvent adhesionAdded; //This event is used for interfacing with the UI

        public abstract void updateCycleCount();


        /// <summary>
        ///     An event that fires when an error has been thrown, attempt to log data
        /// </summary>
        protected virtual void errorThrown(object Sender, EventArgs e)
        {
            Console.WriteLine("WRITE");
            string output = "There was an error that occurred when executing the file. An error log has been generated. Data is below.";
            try
            {
                output += getFileHeader();
            }
            catch { }
            try
            {
                output = appendToHeader(output);
            }
            catch { }
            try
            {
                output += getFileBody();
            }
            catch { }
            try
            {
                output = appendFullData(output);
            }
            catch { }
            try
            {
                var specifier = getSpecifier(Settings.Default.filePath + Constants.FILENAME, Constants.FILEEXTENSION);
                Console.WriteLine("SPECIFIER:" + specifier);
                File.WriteAllText(Settings.Default.filePath + Constants.FILENAME + specifier + Constants.FILEEXTENSION,
                    output);
                Console.WriteLine("SAVED");
                Console.WriteLine(Settings.Default.filePath);
            }
            catch { }
            esp.abortMotion();
            esp.resetController();
        }

        /// <summary>
        ///     Begin a trial
        /// </summary>
        public virtual void runTrial()
        {
            Console.WriteLine("DRAG VELOCITY:" + dragVelocity);
            Console.WriteLine("Trial Began");
            preloadReached = false;
            MainViewModel.dataAcquirer.dataAcquiredEvent += dataAcquired;
        }

        /// <summary>
        ///     The trial has finished, cleanup and then show the trial complete page
        /// </summary>
        protected void trialComplete()
        {
            Application.Current.Dispatcher.Invoke(delegate { MainWindow.mainFrame.Navigate(new trialComplete()); });
        }

        /// <summary>
        ///     Indicates that a cycle is about to begin
        /// </summary>
        protected void beginCycle()
        {
            preloadReached = false;
            startPreloadIndex = MainViewModel.dataAcquirer.dataPoints.Count - 1;
        }

        /// <summary>
        ///     Indicates that a cycle has ended
        /// </summary>
        protected void endCycle()
        {
            endPreloadIndex = MainViewModel.dataAcquirer.dataPoints.Count - 1;
            preloadValues.Add(findMaximumNormalOverRange(startPreloadIndex, endPreloadIndex));
            adhesionValues.Add(findMinimumNormalOverRange(startPreloadIndex, endPreloadIndex));
            invokeAdhesionAddedEvent(this, new adhesionAddedEventArgs(adhesionValues, preloadValues));
            if (currentCycle + 1 < totalCycles) //Prevents overcounting of cycles
            {
                currentCycle++;
            }
        }

        /// <summary>
        ///     holds the current thread until the preload has been reached
        /// </summary>
        protected void waitForPreload()
        {
            while (true)
            {
                if (preloadReached)
                {
                    break;
                }
                Thread.Sleep(1);
            }
        }

        /// <summary>
        ///     Returns the minimum normal force over a range of indices in the DAQ's normal values
        /// </summary>
        protected double findMinimumNormalOverRange(int startIndex, int endIndex)
        {
            var minimum = MainViewModel.dataAcquirer.dataPoints[startIndex].normalForce;
            for (var i = startIndex + 1; i < endIndex; i++)
            {
                if (MainViewModel.dataAcquirer.dataPoints[i].normalForce < minimum)
                {
                    minimum = MainViewModel.dataAcquirer.dataPoints[i].normalForce;
                }
            }
            return minimum;
        }

        /// <summary>
        ///     Returns the maximum normal force over a range of indices in the DAQ's normal values
        /// </summary>
        protected double findMaximumNormalOverRange(int startIndex, int endIndex)
        {
            var maximum = MainViewModel.dataAcquirer.dataPoints[startIndex].normalForce;
            for (var i = startIndex + 1; i < endIndex; i++)
            {
                if (MainViewModel.dataAcquirer.dataPoints[i].normalForce > maximum)
                {
                    maximum = MainViewModel.dataAcquirer.dataPoints[i].normalForce;
                }
            }
            return maximum;
        }

        /// <summary>
        ///     An event that fires when data has been acquired, checks whether or not the preload has been reached
        /// </summary>
        protected virtual void dataAcquired(object Sender, EventArgs e)
        {
            var force =
                MainViewModel.dataAcquirer.dataPoints[MainViewModel.dataAcquirer.dataPoints.Count - 1].normalForce;
            if (force > desiredPreload)
            {
                preloadReached = true;
            }
        }

        /// <summary>
        /// Adds general information to a string for the file header
        /// </summary>
        /// <param name="output">the string to append to</param>
        /// <returns></returns>
        private string appendToHeader(string output)
        {
            output += "Trial finished at: " + DateTime.Now + Environment.NewLine;
            output += "Incoming velocity: " + incomingVelocity + Environment.NewLine;
            output += "Outgoing velocity: " + outgoingVelocity + Environment.NewLine;
            if (!overrideTrialDefaults)
            {
                output += "Incoming vertical angle (relative to the horizontal) " + incomingVerticalAngle * 180 / Math.PI +
                          Environment.NewLine;
                output += "Outgoing vertical angle (relative to the horizontal) " + outgoingVerticalAngle * 180 / Math.PI +
                          Environment.NewLine;
            }
            output += "Wait time at preload: " + preloadWaitTime + Environment.NewLine;
            output += "Drag velocity: " + dragVelocity + Environment.NewLine;
            return output;
        }
        /// <summary>
        /// Adds full data to the output string if needed
        /// </summary>
        /// <param name="output"></param>
        /// <returns></returns>
        private string appendFullData(string output)
        {
            if (collectFullData)
            {
                Console.WriteLine("Outputting full data");
                output += Environment.NewLine + "Full Data:" + Environment.NewLine;
                output += "Seconds,Shear Force,Normal Force" + Environment.NewLine;
                for (var i = 0; i < MainViewModel.dataAcquirer.dataPoints.Count; i++)
                {
                    output += MainViewModel.dataAcquirer.dataPoints[i].time + "," +
                              MainViewModel.dataAcquirer.dataPoints[i].normalForce + "," +
                              MainViewModel.dataAcquirer.dataPoints[i].shearForce + Environment.NewLine;
                }
            }
            return output;
        }
        /// <summary>
        ///     Saves the outpuit to the file
        /// </summary>
        protected void writeToFile()
        {
            Console.WriteLine("WRITE");
            var output = getFileHeader();
            output = appendToHeader(output);
            output += getFileBody();
            output = appendFullData(output);
            var specifier = getSpecifier(Settings.Default.filePath + Constants.FILENAME, Constants.FILEEXTENSION);
            Console.WriteLine("SPECIFIER:" + specifier);
            File.WriteAllText(Settings.Default.filePath + Constants.FILENAME + specifier + Constants.FILEEXTENSION,
                output);
            Console.WriteLine("SAVED");
            Console.WriteLine(Settings.Default.filePath);
        }

        /// <summary>
        ///     returns the specifier needed at the end of the file -1, -2 ...
        /// </summary>
        /// <param name="filePath"> the file path, including the file itself, but not the extension </param>
        /// <param name="fileExtension"> the file extension </param>
        /// <returns></returns>
        private string getSpecifier(string filePath, string fileExtension)
        {
            if (File.Exists(filePath + fileExtension))
            {
                Console.WriteLine("File exists already");
                var currentSpecifier = 1;
                while (File.Exists(filePath + "-" + currentSpecifier + fileExtension))
                {
                    currentSpecifier++;
                }
                return "-" + currentSpecifier;
            }
            return ""; //no specifier needed, don't return one
        }

        /// <summary>
        ///     Returns a string of text to go in the body of a saved file
        /// </summary>
        /// <returns></returns>
        protected abstract string getFileBody();

        /// <summary>
        ///     Returns a string of text to goin the the file header specific to a trial type when data is saved
        /// </summary>
        protected abstract string getFileHeader();

        //derived classes cannot invoke the event itself, do like so
        protected void invokeAdhesionAddedEvent(object Sender, adhesionAddedEventArgs e)
        {
            adhesionAdded(Sender, e);
        }

        private espManager generateEspManager()
        {
            esp = new espManager();
            esp.motorOn(1);
            esp.motorOn(2);
            esp.motorOn(3);
            esp.setAcceleration(XAXIS, ACCELERATION);
            esp.setAcceleration(YAXIS, ACCELERATION);
            esp.setAcceleration(ZAXIS, ACCELERATION);
            esp.setDeceleration(XAXIS, ACCELERATION);
            esp.setDeceleration(YAXIS, ACCELERATION);
            esp.setDeceleration(ZAXIS, ACCELERATION);
            return esp;
        }

        /// <summary>
        /// ends the current trial, stopping and turning off the motors   
        /// </summary>
        public void abortTrial()
        {
            esp.stopMotion(1);
            esp.stopMotion(2);
            esp.stopMotion(3);

            esp.motorOff(1);
            esp.motorOff(2);
            esp.motorOff(3);

            Thread t = new Thread(() => MessageBox.Show("Please wait, resetting the motor controller."));
            t.Start();
            esp.stopThreadUntilOperationsComplete();
            esp.resetController();
            esp.disable();
            esp.closeConnection();
            Thread.Sleep(20000); //Allows time for the motor controller to reset
            esp = generateEspManager();//Reopen the port, as it will have been closed

        }

        //Struct for storing 3d coordinates
        protected class vector3
        {
            public vector3(double x, double y, double z)
            {
                this.x = x;
                this.y = y;
                this.z = z;
            }

            public double x { get; set; }
            public double y { get; set; }
            public double z { get; set; }

            public static vector3 operator +(vector3 a, vector3 b)
            {
                return new vector3(a.x + b.x, a.y + b.y, a.z + b.z);
            }

            public static vector3 operator *(vector3 a, double b)
            {
                return new vector3(a.x * b, a.y * b, a.z * b);
            }
        }
    }

    /// <summary>
    ///     These eventargs are used for interfacing with the UI
    /// </summary>
    public class adhesionAddedEventArgs : EventArgs
    {
        public adhesionAddedEventArgs(List<double> adhesionValues, List<double> preloadValues)
        {
            this.adhesionValues = adhesionValues;
            this.preloadValues = preloadValues;
        }

        public List<double> adhesionValues { get; }

        public List<double> preloadValues { get; }
    }
}