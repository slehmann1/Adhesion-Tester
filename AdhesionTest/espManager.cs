using System;
using System.Collections;
using System.ComponentModel;
using System.IO.Ports;
using System.Threading;
using System.Windows;

namespace AdhesionTest
{
    /// <summary>
    /// This class interfaces with an esp301 motor controller. Documentation for this device can be found at: http://assets.newport.com/webDocuments-EN/images/ESP301_User_manual.PDF
    /// This class does not include all functions of the ESP301, but it includes the vast majority of the major ones, and demonstrates how to implement further functions
    /// Please note that this is not an official library, and is in no way afiliated with Newport.
    /// </summary>
    public class espManager
    {
        public enum travelOption { positive, negative }
        public enum unitOption { encoderCount, motorStep, millimeter, micrometer, inches, milliinches, microinches, degree, gradian, radian, milliradian, microradian }
        public bool initialized { get; private set; } = false;
        SerialPort comPort = new SerialPort();
        Queue recievedData = new Queue();
        string dataRecieved = "";
        bool dataHasBeenRecieved = false;

        /// <summary>
        /// Thrown when the espmanager is uninitialized
        /// </summary>
        public class UninitializedException : Exception
        {
            public UninitializedException()
            {
            }

            public UninitializedException(string message)
                : base(message)
            {
            }

            public UninitializedException(string message, Exception inner)
                : base(message, inner)
            {
            }
        }

        public espManager()
        {
            Console.WriteLine("RUN ESP");
            Application.Current.MainWindow.Closing += new CancelEventHandler(MainWindow_Closing);
            connectDevice();
        }
        /// <summary>
        /// An event that occurs when the window is closed, ensures that all motors are stopped
        /// </summary>
        void MainWindow_Closing(object sender, CancelEventArgs e)
        {
            Console.WriteLine("CLOSED");
            try
            {
                motorOff(1);
                motorOff(2);
                motorOff(3);
                initialized = false;
                closeConnection();
            }
            catch { }
        }
        public void closeConnection()
        {
            comPort.Dispose();
        }
        /// <summary>
        /// Stops motion of all actuators
        /// </summary>
        public void abortMotion()
        {
            writeSerialCommand("AB");
        }
        /// <summary>
        /// Sets the acceleration of an axis
        /// </summary>
        /// <param name="axisNum">the desired axis to target</param>
        /// <param name="acceleration">the desired acceleration value</param>
        public void setAcceleration(int axisNum, double acceleration)
        {
            writeSerialCommand(axisNum.ToString() + "AC" + acceleration.ToString());
        }
        /// <summary>
        /// Sets the emergency stop (e-stop) deceleration for an axis. An emergency stop is triggered on a front panel or interlock input,
        /// or by the reception of an abort command
        /// </summary>
        /// <param name="axisNum">the desired axis to target</param>
        /// <param name="acceleration">the desired acceleration value</param>
        public void setEmergencyStopDeceleration(int axisNum, double acceleration)
        {
            writeSerialCommand(axisNum.ToString() + "AE" + acceleration.ToString());
        }
        /// <summary>
        /// Sets the acceleration forward feed gain factor
        /// </summary>
        /// <param name="axisNum">the desired axis to target</param>
        /// <param name="accelerationGainFactor">the desired forward feed gain factor</param>
        public void setAccelerationFeedForwardGain(int axisNum, double accelerationGainFactor)
        {
            writeSerialCommand(axisNum.ToString() + "AF" + accelerationGainFactor.ToString());
        }
        /// <summary>
        /// Sets the deceleration of an axis
        /// </summary>
        /// <param name="axisNum">the desired axis to target</param>
        /// <param name="deceleration">the desired deceleration value</param>
        public void setDeceleration(int axisNum, double deceleration)
        {
            writeSerialCommand(axisNum.ToString() + "AG" + deceleration.ToString());
        }
        /// <summary>
        /// Aborts the program in process after the current command finishes executing
        /// </summary>
        public void abortProgram()
        {
            writeSerialCommand("AP");
        }
        /// <summary>
        /// Sets the maximum acceleration/deceleration of an axis of an axis
        /// </summary>
        /// <param name="axisNum">the desired axis to target</param>
        /// <param name="maxValue">the desired maximum acceleration/deceleration value</param>
        public void setMaxAcceleration(int axisNum, double maxValue)
        {
            writeSerialCommand(axisNum.ToString() + "AU" + maxValue.ToString());
        }
        /// <summary>
        /// See newport documentation
        /// </summary>
        /// <param name="axisNum">the desired axis to target</param>
        /// <param name="backlashComp">the desired backlash compensation value value</param>
        public void setBacklashCompensation(int axisNum, double backlashComp)
        {
            writeSerialCommand(axisNum.ToString() + "BA" + backlashComp.ToString());
        }
        /// <summary>
        /// See newport documentation
        /// </summary>
        /// <param name="bitNumber">the bit number used to trigger stored program execution</param>
        /// <param name="storedProgram">the name of stored program to be executed</param>
        public void assignDIOBits_executeStoredPrograms(int bitNumber, int storedProgram)
        {
            writeSerialCommand(bitNumber.ToString() + "BG" + storedProgram.ToString());
        }
        /// <summary>
        /// See newport documentation
        /// </summary>
        /// <param name="axisNumber">the desired axis to target</param>
        /// <param name="bitNumberForInhibition">the bit number for inhibiting motion</param>
        /// /// <param name="bitLevelForInhibition">the bit level when axis motion is inhibited</param>
        public void assignDIOBits_inhibitMotion(int axisNumber, int bitNumberForInhibition, int bitLevelForInhibition)
        {
            writeSerialCommand(axisNumber.ToString() + "BK" + bitLevelForInhibition.ToString() + "," + bitLevelForInhibition.ToString());
        }
        /// <summary>
        /// See newport documentation
        /// </summary>
        /// <param name="axisNumber">the desired axis to target</param>
        /// <param name="enable">if true, enable, if false, disable</param>
        public void enableDIOBits_inhibitMotion(int axisNumber, bool enable)
        {
            writeSerialCommand(axisNumber.ToString() + "BL" + Convert.ToInt32(enable).ToString());
        }
        /// <summary>
        /// See newport documentation
        /// </summary>
        /// <param name="axisNumber">the desired axis to target</param>
        /// <param name="bitNumber">bit number for notifying motion status</param>
        /// <param name="bitLevel">bit level when axis is not moving</param>
        public void assignDIOBits_notifyMotionStatus(int axisNumber, int bitNumber, int bitLevel)
        {
            writeSerialCommand(axisNumber.ToString() + "BM" + bitNumber.ToString() + "," + bitLevel.ToString());
        }
        /// <summary>
        /// See newport documentation
        /// </summary>
        /// <param name="axisNumber">the desired axis to target</param>
        /// <param name="enable">if true, enable, if false, disable</param>
        public void enableDIOBits_notifyMotionStatus(int axisNumber, bool enable)
        {
            writeSerialCommand(axisNumber.ToString() + "BN" + Convert.ToInt32(enable).ToString());
        }
        /// <summary>
        /// See newport documentation
        /// </summary>
        /// <param name="axisNumber">the desired axis to target</param>
        /// <param name="bitNumberNegative">bit number for jogging in negative direction</param>
        /// <param name="bitNumberPositive">bit number for jogging in positive direction</param>
        public void assignDIOBits_jogMode(int axisNumber, int bitNumberNegative, int bitNumberPositive)
        {
            writeSerialCommand(axisNumber.ToString() + "BM" + bitNumberNegative.ToString() + "," + bitNumberPositive.ToString());
        }
        /// <summary>
        /// See newport documentation
        /// </summary>
        /// <param name="axisNumber">the desired axis to target</param>
        /// <param name="enable">if true, enable, if false, disable</param>
        public void enableDIOBits_jogMode(int axisNumber, bool enable)
        {
            writeSerialCommand(axisNumber.ToString() + "BQ" + Convert.ToInt32(enable).ToString());
        }
        /// <summary>
        /// Sets the closed loop update interval for an axis. This command is only effective for stepper motors.
        /// </summary>
        /// <param name="axisNumber">the desired axis to target</param>
        /// <param name="interval">the time duration between position error corrections durign colosed loop stepper positioning</param>
        public void setClosedLoopUpdateInterval(int axisNumber, int interval)
        {
            writeSerialCommand(axisNumber.ToString() + "CL" + interval.ToString());
        }
        /// <summary>
        /// Sets the linear compensation, allowing for linear position errors due to stage innacuracies. Note: this command is only effective
        /// a home search or a define home command is performed upon the specified axis
        /// </summary>
        /// <param name="axisNumber">the desired axis to target</param>
        /// <param name="compensationValue">the desired linear compensation value</param>
        public void setLinearCompensation(int axisNumber, int compensationValue)
        {
            writeSerialCommand(axisNumber.ToString() + "CO" + compensationValue.ToString());
        }
        /// <summary>
        /// Sets the position deadband, allowing one to prevent limit cycling (see documentation). Note: this command is only effective during
        /// position regulation (stationary), not whilst in motion
        /// </summary>
        /// <param name="axisNumber">the desired axis to target</param>
        /// <param name="deadband">the desired deadband value. Examples of deadband include Led Zeppelin</param>
        public void setPositionDeadband(int axisNumber, int deadband)
        {
            writeSerialCommand(axisNumber.ToString() + "DB" + deadband.ToString());
        }
        /// <summary>
        /// This is not related to the DAQ class, and does not interface with force probes. See the newport documentation for info.
        /// </summary>
        /// <param name="dataAcquisitionMode">see documentation</param>
        /// <param name="axisToTrigger">the axis to trigger data acquisition</param>
        /// <param name="parameter3">newport has quality naming conventions</param>
        /// <param name="parameter4">10/10 love newport, newport is much better than oldport</param>
        /// <param name="sampleNumber">the number of data samples to be acquired</param>
        public void setupDataAcquisition(int dataAcquisitionMode, int axisToTrigger, int parameter3, int parameter4, int dataAcquisitionRate, int sampleNumber)
        {
            writeSerialCommand("Dc" + dataAcquisitionMode.ToString() + "," + axisToTrigger.ToString() + "," + parameter3.ToString() + "," + parameter4.ToString() + "," + dataAcquisitionRate.ToString() + "," + sampleNumber.ToString() + ",");
        }
        /// <summary>
        /// Returns true if the data acquisition is done. Returns false if it is not.
        /// </summary>
        /// <returns></returns>
        public bool getDataAcquisitionDoneStatus()
        {
            writeSerialCommand("DD");
            return Convert.ToBoolean(Convert.ToInt32(waitForData()));
        }
        /// <summary>
        /// Enable or disable the data acquisition request. Note:
        /// This command cannot be issued when an axis is being homed, moved to a travel limit or an index.
        /// </summary>
        /// <param name="enable">if true, enable the request, if false disable the request</param>
        public void enableDataAcquistion(bool enable)
        {
            writeSerialCommand("DE" + Convert.ToString(Convert.ToInt32(enable)));
        }

        /// <summary>
        /// Sets the desired position as the home position
        /// </summary>
        /// <param name="axisNum">the desired axis to target</param>
        /// <param name="position">the desired position value</param>
        public void defineHomePosition(int axisNum, double position)
        {
            writeSerialCommand(axisNum.ToString() + "DH" + position.ToString());
        }
        /// <summary>
        /// Gets the desired position
        /// </summary>
        /// <param name="axisNumber">the desired axis to target</param>
        public double getDesiredPosition(int axisNumber)
        {
            writeSerialCommand(axisNumber.ToString() + "DP?");
            return Convert.ToDouble(waitForData());
        }
        /// <summary>
        /// Gets the desired velocity
        /// </summary>
        /// <param name="axisNumber">the desired axis to target</param>
        public double getDesiredVelocity(int axisNumber)
        {
            writeSerialCommand(axisNumber.ToString() + "DV?");
            return Convert.ToDouble(waitForData());
        }
        /// <summary>
        /// Reads the stage model and serial number. The positioner must be ESP compatible.
        /// </summary>
        /// <param name="axisNumber"></param>
        /// <returns></returns>
        public string getStageModel_SerialNum(int axisNumber)
        {
            writeSerialCommand(axisNumber.ToString() + "ID?");
            return waitForData();
        }
        /// <summary>
        /// Sets the jerk rate (the rate of change in acceleration)
        /// </summary>
        /// <param name="axisNum">the desired axis to target</param>
        /// <param name="jerk">the desired jerk value</param>
        public void setJerk(int axisNum, double jerk)
        {
            writeSerialCommand(axisNum.ToString() + "JK" + jerk.ToString());
        }

        /// <summary>
        /// Locks the entirety of the buttons on the main panel of the ESP301 device
        /// </summary>
        public void lockKeyboard()
        {
            writeSerialCommand("LC2");
        }
        /// <summary>
        /// Unocks the entirety of the buttons on the main panel of the ESP301 device
        /// </summary>
        public void unlockKeyboard()
        {
            writeSerialCommand("LC0");
        }
        /// <summary>
        /// Locks the entirety of the buttons on the main panel of the ESP301 device except for the motor on/off button
        /// </summary>
        public void lockKeyboardSaveMotors()
        {
            writeSerialCommand("LC1");
        }
        /// <summary>
        /// returns true if the motion along the specified axis is complete, false if it is not
        /// </summary>
        /// <param name="axisNum">the desired axis to target</param>
        /// <returns></returns>
        public bool motionDone(int axisNum)
        {
            writeSerialCommand(axisNum.ToString() + "MD?");
            return Convert.ToBoolean(Convert.ToInt32(waitForData()));
        }
        /// <summary>
        /// Turns the desired motor off
        /// </summary>
        /// <param name="axisNum">the desired axis to target</param>
        public void motorOff(int axisNum)
        {
            writeSerialCommand(axisNum.ToString() + "MF");
        }
        /// <summary>
        /// Turns the desired motor on
        /// </summary>
        /// <param name="axisNum">the desired axis to target</param>
        public void motorOn(int axisNum)
        {
            writeSerialCommand(axisNum.ToString() + "MO");
        }
        /// <summary>
        /// Moves to the actuators limit
        /// </summary>
        /// <param name="axisNum">the desired axis to target</param>
        /// <param name="desiredDirection">an enum representing whether to move towards the positive or the negative axis</param>
        public void moveToAxisLimit(int axisNum, travelOption desiredDirection)
        {
            switch (desiredDirection)
            {
                case travelOption.positive:
                    writeSerialCommand(axisNum.ToString() + "MT+");
                    break;
                case travelOption.negative:
                    writeSerialCommand(axisNum.ToString() + "MT-");
                    break;

                default:
                    throw new Exception("Unnacounted for travel option");
            }
        }
        /// <summary>
        /// Move the desired axis indefinitely in the specified direction with the jerk, acceleration, and velocity that was defined earlier 
        /// </summary>
        /// <param name="axisNum">the desired axis to target</param>
        /// <param name="desiredDirection">an enum representing whether to move towards the positive or the negative axis</param>
        public void moveIndefinitely(int axisNum, travelOption desiredDirection)
        {
            switch (desiredDirection)
            {
                case travelOption.positive:
                    writeSerialCommand(axisNum.ToString() + "MV+");
                    break;
                case travelOption.negative:
                    writeSerialCommand(axisNum.ToString() + "MV-");
                    break;

                default:
                    throw new Exception("Unnacounted for travel option");
            }
        }
        /// <summary>
        /// Move the desired axis to the specified aboslute position, with the predefined jerk, velocity, and acceleration. Note: This absolute
        /// position is impacted by resetting the position of an axis via the menus on the ESP301 front panel
        /// </summary>
        /// <param name="axisNum">the desired axis to target</param>
        /// <param name="position">the desired absolute position to move to</param>
        public void moveToAbsolutePosition(int axisNum, double position)
        {
            writeSerialCommand(axisNum.ToString() + "PA" + position.ToString());
        }
        /// <summary>
        /// Move the desired axis the specified distance relative to the current position of the axis, with the predefined jerk, velocity, 
        /// and acceleration. Negative values are permitted.
        /// </summary>
        /// <param name="axisNum">the desired axis to target</param>
        /// <param name="relativePosition">the distance to travel relative to the current position</param>
        public void moveToRelativePosition(int axisNum, double relativePosition)
        {
            writeSerialCommand(axisNum.ToString() + "PR" + relativePosition.ToString());
        }
        /// <summary>
        /// Performs a hard reset of the controller.
        /// This command is used to perform a hardware reset of the controller. It performs
        /// the following preliminary tasks before resetting the controller: 
        /// 1. Stop all the axes that are in motion.The deceleration value specified
        /// using the command AG is used to stop the axes.
        /// 2. Wait for 500 ms to allow the axes to settle.
        /// 3. Disable all the axes by turning the power OFF.
        /// 4. Reset to the controller card.
        /// Once the command to reset the controller is detected by the DSP, the controller
        /// will stay in reset for a minimum of 200 ms.After the reset condition has
        /// occurred(i.e., after the 200 ms reset time), the controller firmware reboots the
        /// controller.At this point, all the parameters last saved to the non-volatile flash
        /// memory on the controller will be restored. Furthermore, the controller will detect
        /// any stages (ESP compatible or otherwise) and drivers connected to the controller.
        /// This process can take anywhere up to 20 seconds depending upon the controller
        /// configuration.
        /// </summary>
        public void resetController()
        {
            writeSerialCommand("RS");
        }
        /// <summary>
        /// Sets the software travel limit for the desired axis. Note: The negative limit cannot be greater than the right limit, and the
        /// limits must contain the home position
        /// </summary>
        /// <param name="axisNum">the desired axis to target</param>
        /// <param name="limit">the desired limit</param>
        /// <param name="desiredDirection">an enum representing whether to act on the positive or the negative axis limit</param>
        public void setTravelLimit(int axisNum, double limit, travelOption desiredDirection)
        {
            switch (desiredDirection)
            {
                case travelOption.positive:
                    writeSerialCommand(axisNum.ToString() + "SR" + limit.ToString());
                    break;
                case travelOption.negative:
                    writeSerialCommand(axisNum.ToString() + "SL" + limit.ToString());
                    break;

                default:
                    throw new Exception("Unnacounted for travel option");
            }
        }
        /// <summary>
        /// Changes the label displayed on the console for the units. Note: this does not change any of the parameters for the axis: velocity, 
        /// position, and so on these all MUST be converted and reset
        /// </summary>
        /// <param name="axisNum">the desired axis to target</param>
        /// <param name="desiredUnitOption">an enum representing the desired unit</param>
        public void changeUnits(int axisNum, unitOption desiredUnitOption)
        {

            switch (desiredUnitOption)
            {
                case unitOption.encoderCount:
                    writeSerialCommand(axisNum.ToString() + "SN" + 0);
                    break;
                case unitOption.motorStep:
                    writeSerialCommand(axisNum.ToString() + "SN" + 1);
                    break;

                case unitOption.millimeter:
                    writeSerialCommand(axisNum.ToString() + "SN" + 2);
                    break;
                case unitOption.micrometer:
                    writeSerialCommand(axisNum.ToString() + "SN" + 3);
                    break;
                case unitOption.inches:
                    writeSerialCommand(axisNum.ToString() + "SN" + 4);
                    break;
                case unitOption.milliinches:
                    writeSerialCommand(axisNum.ToString() + "SN" + 5);
                    break;
                case unitOption.microinches:
                    writeSerialCommand(axisNum.ToString() + "SN" + 6);
                    break;
                case unitOption.degree:
                    writeSerialCommand(axisNum.ToString() + "SN" + 7);
                    break;
                case unitOption.gradian:
                    writeSerialCommand(axisNum.ToString() + "SN" + 8);
                    break;
                case unitOption.radian:
                    writeSerialCommand(axisNum.ToString() + "SN" + 9);
                    break;
                case unitOption.milliradian:
                    writeSerialCommand(axisNum.ToString() + "SN" + 10);
                    break;
                case unitOption.microradian:
                    writeSerialCommand(axisNum.ToString() + "SN" + 11);
                    break;
                default:
                    throw new Exception("Unaccounted for unit option");
            }
        }
        /// <summary>
        /// stops the desired axis during a motion using the deceleration set prior
        /// </summary>
        /// <param name="axisNumber">the desired axis to target</param>
        public void stopMotion(int axisNumber)
        {
            writeSerialCommand(axisNumber.ToString() + "ST");
        }
        /// <summary>
        /// Gets the current actual position
        /// </summary>
        /// <param name="axisNumber">the desired axis to target</param>
        /// <returns></returns>
        public double getCurrentPosition(int axisNumber)
        {
            Console.WriteLine("WRITE");
            writeSerialCommand(axisNumber.ToString() + "TP");
            string s = waitForData();
            Console.WriteLine("DATA");
            return Convert.ToDouble(s);
        }

        /// <summary>
        /// Gets the current actual velocity
        /// </summary>
        /// <param name="axisNumber">the desired axis to target</param>
        /// <returns></returns>
        public double getCurrentVelocity(int axisNumber)
        {
            writeSerialCommand(axisNumber.ToString() + "TV");
            return Convert.ToDouble(waitForData());
        }

        /// <summary>
        /// Halts the current thread until the esp301 device is in an idle state.
        /// </summary>
        public void stopThreadUntilOperationsComplete()
        {
            while (true)
            {
                writeSerialCommand("TX");
                    if ((waitForData().ToCharArray()[0]) == 64)
                    {
                        break;
                    } 
            }
        }

        /// <summary>
        /// Sets the velocity of the desired axis. Note: the desired velocity cannot be negative
        /// </summary>
        /// <param name="axisNumber">the axis to target</param>
        /// <param name="velocity">the desired velocity</param>
        public void setVelocity(int axisNumber, double velocity)
        {
            writeSerialCommand(axisNumber.ToString() + "VA" + velocity.ToString());
        }
        /// <summary>
        /// Sets the maximum velocity of the desired axis
        /// </summary>
        /// <param name="axisNumber">the desired axis to target</param>
        /// <param name="maxVelocity">the maximum velocity</param>
        public void setMaximumVelocity(int axisNumber, double maxVelocity)
        {
            writeSerialCommand(axisNumber.ToString() + "VU" + maxVelocity.ToString());
        }
        /// <summary>
        /// Executes further commands only after the desired position is reached
        /// </summary>
        /// <param name="axisNumber">the desired axis to target</param>
        /// <param name="position">the desired position to reach</param>
        public void waitForPosition(int axisNumber, double position)
        {
            writeSerialCommand(axisNumber.ToString() + "WP" + position.ToString());
        }
        /// <summary>
        /// Executes further commands only after the motion of the current axis is halted. Operates on the ESP301 device. Not this application.
        /// </summary>
        /// <param name="axisNumber">the desired axis to target</param>
        public void waitForMotionStop(int axisNumber)
        {
            writeSerialCommand(axisNumber.ToString() + "WS");
        }
        /// <summary>
        /// Executes further commands only after the motion of the current axis is halted and then after waiting the desired delay in ms. Operates on the ESP301 device. Not this application.
        /// </summary>
        /// <param name="axisNumber">the desired axis to target</param>
        /// <param name="delay">the desired time to wait in ms</param>
        public void waitForMotionStop(int axisNumber, int delay)
        {
            writeSerialCommand(axisNumber.ToString() + "WS" + delay.ToString());
        }
        /// <summary>
        /// The controller waits the specified amount of time in ms before executing furthe commands. (Impacts all axes). Operates on the ESP301 device. Not this application.
        /// </summary>
        /// <param name="waitTime">the desired time to wait in ms</param>
        public void waitMS(int waitTime)
        {
            writeSerialCommand("WT" + waitTime.ToString());
        }

        private void errorRecievedHandler(object sender, SerialErrorReceivedEventArgs e)
        {
            Console.WriteLine("SERIAL ERROR: \n \n");
            Console.WriteLine(e.EventType);
        }

        private void dataRecievedHandler(object sender, SerialDataReceivedEventArgs e)
        {
            //    Console.WriteLine("DATARECIEVED");
            // Show all the incoming data in the port's buffer
            dataRecieved = comPort.ReadExisting();
            dataHasBeenRecieved = true;
            //   Console.WriteLine("ENDDATARECIEVED");
        }

        private string waitForData()
        {
            dataRecieved = "";

            while (!dataHasBeenRecieved)
            {
                if (initialized)
                {
                }
                else
                {
                    throw new UninitializedException();
                }
            }
            Thread.Sleep(10);
            //No data was collected
            if (dataRecieved.Equals(""))
            {
                dataHasBeenRecieved = false;
                waitForData();
            }

            comPort.DiscardInBuffer();
            dataHasBeenRecieved = false;
            return dataRecieved;
        }

        /// <summary>
        /// Marks the controller as uninitialized
        /// </summary>
        public void disable()
        {
            initialized = false;
        }

        /// <summary>
        /// Writes a command to the device
        /// </summary>
        /// <param name="command"></param>
        private void writeSerialCommand(string command)
        {
            if (initialized)
            {
                Thread.Sleep(Constants.INTERCOMMANDWAITTIME);
                command = command + "\r"; //add terminator (Arnold)
                byte[] message = System.Text.Encoding.UTF8.GetBytes(command);
                comPort.Write(message, 0, message.Length);
            }
            else
            {
                throw new UninitializedException();
            }
        }

        /// <summary>
        /// Handles the initialization of the ESP301 communication through sereal
        /// </summary>
        private void connectDevice()
        {
            try
            {
                comPort.ErrorReceived += new SerialErrorReceivedEventHandler(errorRecievedHandler);
                comPort.DataReceived += new SerialDataReceivedEventHandler(dataRecievedHandler);
                comPort.PortName = "COM" + Properties.Settings.Default.motorControllerPort.ToString();
                comPort.Handshake = Handshake.RequestToSendXOnXOff;
                comPort.BaudRate = Constants.BAUDRATE;
                comPort.Parity = Parity.None;
                comPort.DataBits = Constants.DATABITS;
                comPort.StopBits = StopBits.One;
                comPort.Open();
                initialized = true;
            }
            catch(Exception e)
            {
                MessageBox.Show("Failed to connect to the motor controller. Please check that it is connected properly and that all required drivers are installed. Once this is done, verify that the correct port setting is chosen.");
                MessageBox.Show(e.Message);
            }
        }

    }


}

