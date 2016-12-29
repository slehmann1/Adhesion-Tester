using System;
using System.Windows;

namespace AdhesionTest
{
    public class linearTrialManager : trialManager
    {
        private int currentIteration;
        private int cyclesPerTrial;
        private int iterationProgress;

        private double m_incomingLateralAngle, m_outgoingLateralAngle;
        private vector3 startPosition;

        public double incomingLateralAngle
        {
            get { return m_incomingLateralAngle; }
            set { m_incomingLateralAngle = value * Math.PI / 180; }
        }

        public double outgoingLateralAngle
        {
            get { return m_outgoingLateralAngle; }
            set { m_outgoingLateralAngle = value * Math.PI / 180; }
        }

        public linearTrialIteration[] trials { get; set; }
        public double yAxisShift { get; set; }

        public override void updateCycleCount()
        {
            foreach (var iteration in trials)
            {
                totalCycles += iteration.cycles;
            }
            cyclesPerTrial = totalCycles;
            totalCycles *= numberOfTrials;
        }

        protected override string getFileBody()
        {
            var s = "Preload,Adhesion,Drag Distance" + Environment.NewLine;
            try
            {
                for (var i = 0; i < adhesionValues.Count; i++)
                {
                    s += preloadValues[i] + "," + Math.Abs(adhesionValues[i]) + "," + getTrial(i).dragDistance + Environment.NewLine;
                }
            }
            catch (Exception e) { MessageBox.Show("ERROR: " + e.Message + Environment.NewLine + e.StackTrace); }
            return s;
        }

        private linearTrialIteration getTrial(int trialNumber)
        {
            trialNumber %= cyclesPerTrial;

            foreach (var iteration in trials)
            {
                trialNumber -= iteration.cycles;
                if (trialNumber < 0)
                {
                    return iteration;
                }
            }
            throw new Exception("No such trial");
        }

        protected override string getFileHeader()
        {
            var s = "Trial type: Linear Trial " + Environment.NewLine + Environment.NewLine;
            s += "Incoming lateral angle from the x axis: " + incomingLateralAngle + Environment.NewLine;
            s += "Outgoing lateral angle from the x axis: " + outgoingLateralAngle + Environment.NewLine;
            s += "Y axis shift: " + yAxisShift + Environment.NewLine;
            return s;
        }

        /// <summary>
        ///     Begin a trial
        /// </summary>
        public override void runTrial()
        {
            try
            {
                base.runTrial();
                if (accelerateMotion)
                {
                    baselineManager.establishDeflectionBaseline();
                }
                depthTest();

                esp.stopThreadUntilOperationsComplete();
                for (var x = 0; x < numberOfTrials; x++)
                {
                    currentIteration = 0;
                    foreach (var iteration in trials)
                    {
                        for (var i = 0; i < iteration.cycles; i++)
                        {
                            beginCycle();
                            descend();

                            waitForPreload();

                            //Stop and wait at preload
                            esp.stopMotion(XAXIS);
                            esp.stopMotion(YAXIS);
                            esp.stopMotion(ZAXIS);
                            esp.waitMS(Convert.ToInt32(preloadWaitTime * 1000));

                            drag();
                            esp.stopThreadUntilOperationsComplete();
                            ascend(false);
                            esp.stopThreadUntilOperationsComplete();

                            endCycle();
                            //end preload early, reverting sometimes causes a large enought trigger to result in a false adhesion value
                            revertPosition();
                            esp.stopThreadUntilOperationsComplete();
                            iterationProgress++;
                        }
                        if (currentIteration != trials.Length - 1)
                        {
                            currentIteration++;
                        }
                    }
                }
                writeToFile();
                trialComplete();
                //Indicates that the espmanager was disabled for some reason partially through the trial
            }
            catch (espManager.UninitializedException)
            {
                Console.WriteLine("Esp Manager Uninitialized");
            }
        }

        protected override void dataAcquired(object Sender, EventArgs e)
        {
            var force =
                MainViewModel.dataAcquirer.dataPoints[MainViewModel.dataAcquirer.dataPoints.Count - 1].normalForce;
            if (force > trials[currentIteration].preload)
            {
                preloadReached = true;
            }
        }


        //makes contact with the sample and then withdraws, allowing the saving of the start position, as one does not know how far the sample is
        private void depthTest()
        {
            descend();
            waitForPreload();
            //Stop and wait at preload
            esp.stopMotion(XAXIS);
            esp.stopMotion(YAXIS);
            esp.stopMotion(ZAXIS);
            esp.waitMS(Convert.ToInt32(preloadWaitTime));
            ascend(true);

            //Save the start positions
            startPosition = new vector3(esp.getCurrentPosition(XAXIS), esp.getCurrentPosition(YAXIS),
                esp.getCurrentPosition(ZAXIS));
        }


        //Drags in the direction of the xaxis with the desired velocity
        private void drag()
        {
            Console.WriteLine("DRAG VELOCITY: " + dragVelocity);
            esp.setVelocity(XAXIS, dragVelocity / 1000);
            esp.moveToRelativePosition(XAXIS, trials[currentIteration].dragDistance / 1000);
        }

        /// <summary>
        ///     begins the descent
        /// </summary>
        private void descend()
        {
            // determine velocities
            var xVel = incomingVelocity * Math.Cos(incomingVerticalAngle) * Math.Cos(incomingLateralAngle);
            var yVel = incomingVelocity * Math.Cos(incomingVerticalAngle) * Math.Sin(incomingLateralAngle);
            var zVel = incomingVelocity * Math.Sin(incomingVerticalAngle);
            if (accelerateMotion)
            {
                var watch = System.Diagnostics.Stopwatch.StartNew();
                Console.WriteLine("RUN ACCELERATION");
                var multiplier = acceleratedSpeed / zVel / 1000;
                esp.setVelocity(XAXIS, xVel * multiplier);
                esp.setVelocity(YAXIS, yVel * multiplier);
                esp.setVelocity(ZAXIS, zVel * multiplier);

                esp.moveIndefinitely(XAXIS, espManager.travelOption.negative);
                esp.moveIndefinitely(YAXIS, espManager.travelOption.positive);
                if(!reverseDirection)
                {
                    esp.moveIndefinitely(ZAXIS, espManager.travelOption.positive);
                }else
                {
                    esp.moveIndefinitely(ZAXIS, espManager.travelOption.negative);
                }
                

                baselineManager.waitForDeflection();
                watch.Stop();
                var elapsedMs = watch.ElapsedMilliseconds;
                Console.WriteLine("END ACCLERATION" + elapsedMs);
            }



            esp.setVelocity(XAXIS, xVel / 1000);
            esp.setVelocity(YAXIS, yVel / 1000);
            esp.setVelocity(ZAXIS, zVel / 1000);

            esp.moveIndefinitely(XAXIS, espManager.travelOption.negative);
            esp.moveIndefinitely(YAXIS, espManager.travelOption.positive);
            if (!reverseDirection)
            {
                esp.moveIndefinitely(ZAXIS, espManager.travelOption.positive);
            }else
            {
                esp.moveIndefinitely(ZAXIS, espManager.travelOption.negative);
            }
        }

        /// <summary>
        ///     Moves the probe to the beginning of the next trial
        /// </summary>
        private void revertPosition()
        {
            esp.setVelocity(XAXIS, Constants.LATERALMOTIONSPEED);
            esp.setVelocity(YAXIS, Constants.LATERALMOTIONSPEED);
            esp.setVelocity(ZAXIS, Constants.LATERALMOTIONSPEED);

            esp.moveToAbsolutePosition(XAXIS, startPosition.x);
            esp.moveToAbsolutePosition(YAXIS, startPosition.y + yAxisShift * currentCycle);
            esp.moveToAbsolutePosition(ZAXIS, startPosition.z);
        }

        /// <summary>
        ///     begins the ascent
        /// </summary>
        private void ascend(bool skipAcceleration)
        {
            // determine velocities
            var xVel = outgoingVelocity * Math.Cos(outgoingVerticalAngle) * Math.Cos(outgoingLateralAngle);
            var yVel = outgoingVelocity * Math.Cos(outgoingVerticalAngle) * Math.Sin(outgoingLateralAngle);
            var zVel = outgoingVelocity * Math.Sin(outgoingVerticalAngle);

            esp.setVelocity(XAXIS, xVel / 1000);
            esp.setVelocity(YAXIS, yVel / 1000);
            esp.setVelocity(ZAXIS, zVel / 1000);

            //begin movement
            esp.moveIndefinitely(XAXIS, espManager.travelOption.positive);
            esp.moveIndefinitely(YAXIS, espManager.travelOption.positive);

            if (!reverseDirection)
            {
                esp.moveToRelativePosition(ZAXIS, -(withdrawDistance / 1000));
            }
            else
            {
                esp.moveToRelativePosition(ZAXIS, (withdrawDistance / 1000));
            }
            


            if (accelerateMotion&&!skipAcceleration)
            {
                var watch = System.Diagnostics.Stopwatch.StartNew();
                IAsyncResult result;
                Action action = () =>
                {
                    baselineManager.waitForBaselineReturn();
                };

                result = action.BeginInvoke(null, null);

                if (!result.AsyncWaitHandle.WaitOne(baselineReturnTimeout))
                {
                    Console.WriteLine("FAILED TO RETURN TO BASELINE");
                    esp.waitForMotionStop(ZAXIS);
                    esp.stopMotion(XAXIS);
                    esp.stopMotion(YAXIS);

                    esp.stopThreadUntilOperationsComplete();
                    Console.WriteLine("Resetting Baseline");
                    //Reset the baseline in case it has reset
                    baselineManager.establishDeflectionBaseline();
                    return;
                }
                watch.Stop();
                Console.WriteLine("RETURNED TO BASELINE" + watch.ElapsedMilliseconds);
                /* ACCELERATE AT A HIGH SPEED UPWARDS
                var multiplier = acceleratedSpeed / zVel / 1000;
                esp.setVelocity(XAXIS, xVel * multiplier);
                esp.setVelocity(YAXIS, yVel * multiplier);
                esp.setVelocity(ZAXIS, zVel * multiplier);
                */

                //Stop axes
                esp.stopMotion(XAXIS);
                esp.stopMotion(YAXIS);
                esp.stopMotion(ZAXIS);
                esp.stopThreadUntilOperationsComplete();
                return;
            }

            //wait until the zaxis has stopped moving, and then stop all
            esp.waitForMotionStop(ZAXIS);
            esp.stopMotion(XAXIS);
            esp.stopMotion(YAXIS);
        }

        public struct linearTrialIteration
        {
            public int cycles;
            public double dragDistance;
            public double preload;
        }
    }
}