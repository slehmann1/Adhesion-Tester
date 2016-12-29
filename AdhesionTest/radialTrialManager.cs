using System;
using System.Collections.Generic;
using System.Windows;

namespace AdhesionTest
{
    public class radialTrialManager : trialManager
    {
        private readonly List<double> pastAngles = new List<double>();

        private vector3 startPosition;

        public radialTrialIteration[] trials;
        public int numStrokes { set; private get; }
        public double dragDistance { set; private get; }

        /// <summary>
        ///     Whether or not the trial iterations should be performed in a randomized order. If false, they are done sequentially
        /// </summary>
        public bool randomizeOrder { protected get; set; } = true;

        /// <summary>
        ///     updates the number of cycles displayed in the UI
        /// </summary>
        public override void updateCycleCount()
        {
            if (overrideTrialDefaults)
            {
                totalCycles = numStrokes * numberOfTrials * trials.Length;
            }
            else
            {
                totalCycles = numStrokes * numberOfTrials;
            }
        }

        public override void runTrial()
        {
            try
            {
                base.runTrial();

                preloadReached = false;



                //begin a depth test
                var zVel = Math.Sin(incomingVerticalAngle) * incomingVelocity;
                //move down and wait
                esp.setVelocity(ZAXIS, zVel / 1000);
                if (!reverseDirection)
                {
                    esp.moveIndefinitely(ZAXIS, espManager.travelOption.positive);
                }else
                {
                    esp.moveIndefinitely(ZAXIS, espManager.travelOption.negative);
                }
                waitForPreload();
                esp.stopMotion(XAXIS);
                esp.stopMotion(YAXIS);
                esp.stopMotion(ZAXIS);
                esp.waitMS(Convert.ToInt32(preloadWaitTime * 1000));

                //move up
                esp.setVelocity(ZAXIS, zVel / 1000);
                if (!reverseDirection)
                {
                    esp.moveToRelativePosition(ZAXIS, -withdrawDistance / 1000);
                }else
                {
                    esp.moveToRelativePosition(ZAXIS, withdrawDistance / 1000);
                }
                esp.stopThreadUntilOperationsComplete();

                //save start positions
                startPosition = new vector3(esp.getCurrentPosition(XAXIS), esp.getCurrentPosition(YAXIS),
                    esp.getCurrentPosition(ZAXIS));
                Console.WriteLine("START POSITION: " + startPosition.x + startPosition.y + startPosition.z);
                esp.stopThreadUntilOperationsComplete();

                baselineManager.establishDeflectionBaseline();

                //generate the angles list
                var angles = new List<double>();
                double currentAngle = 0;
                var r = new Random();
                if (randomizeOrder)
                {
                    while (currentAngle < 2 * Math.PI - 0.01)//The -0.01 is to eliminate floating point errors
                    {
                        angles.Add(currentAngle);
                        currentAngle += 2 * Math.PI / numStrokes;
                    }
                }
                var originalAngles = new List<double>(angles); //create a backup to prevent need to regenerate
                Console.WriteLine("ORIGINAL ANGLES: " + originalAngles.Count);
                var currentIteration = new radialTrialIteration();
                for (var x = 0; x < numberOfTrials; x++)
                {
                    currentIteration.dragDistance = dragDistance;
                    currentIteration.incomingAngle = incomingVerticalAngle;
                    currentIteration.outgoingAngle = outgoingVerticalAngle;

                    if (overrideTrialDefaults)
                    {
                        for (var i = 0; i < trials.Length; i++)
                        {
                            currentAngle = 0;
                            angles = new List<double>(originalAngles); //reset the angles list
                            if (randomizeOrder)
                            {
                                while (angles.Count > 0)
                                {
                                    var currentIndex = r.Next(angles.Count);
                                    radialStroke(trials[i], angles[currentIndex]);
                                    pastAngles.Add(angles[currentIndex]);
                                    angles.RemoveAt(currentIndex);
                                }
                            }
                            else
                            {
                                while (currentAngle < 2 * Math.PI)
                                {
                                    radialStroke(trials[i], currentAngle);
                                    pastAngles.Add(currentAngle);
                                    currentAngle += 2 * Math.PI / numStrokes;
                                }
                            }
                        }
                    }
                    else
                    {
                        if (randomizeOrder)
                        {
                            while (angles.Count > 0)
                            {
                                var currentIndex = r.Next(angles.Count);
                                radialStroke(currentIteration, angles[currentIndex]);
                                pastAngles.Add(angles[currentIndex]);
                                angles.RemoveAt(currentIndex);
                            }
                        }
                        else
                        {
                            while (currentAngle < 2 * Math.PI)
                            {
                                radialStroke(currentIteration, currentAngle);
                                pastAngles.Add(currentAngle);
                                currentAngle += 2 * Math.PI / numStrokes;
                            }
                        }
                    }
                }
                writeToFile();
                trialComplete();
            }
            //Indicates that the espmanager was disabled for some reason partially through the trial
            catch (espManager.UninitializedException)
            {
                Console.WriteLine("Esp Manager Uninitialized");
            }
        }

        /// <summary>
        ///     performs a full stroke- an upwards and downwards motion
        /// </summary>
        /// <param name="iteration"></param>
        /// <param name="currentAngle">The desired angle to perform the stroke at in radians</param>
        private void radialStroke(radialTrialIteration iteration, double currentAngle)
        {
            beginCycle();
            //determine incoming velocities
            var xVel = Math.Abs(Math.Cos(iteration.incomingAngle) * Math.Cos(currentAngle) * incomingVelocity) / 1000;
            var yVel = Math.Abs(Math.Cos(iteration.incomingAngle) * Math.Sin(currentAngle) * incomingVelocity) / 1000;
            var zVel = Math.Sin(iteration.incomingAngle) * incomingVelocity / 1000;

            espManager.travelOption xDir, yDir;

            //Do some trig to determine directions, as the velocities cannot be negative
            if (currentAngle < Math.PI / 2)
            {
                xDir = espManager.travelOption.positive;
                yDir = espManager.travelOption.positive;
            }
            else if (currentAngle < Math.PI)
            {
                xDir = espManager.travelOption.negative;
                yDir = espManager.travelOption.positive;
            }
            else if (currentAngle < 3 * Math.PI / 2)
            {
                xDir = espManager.travelOption.negative;
                yDir = espManager.travelOption.negative;
            }
            else if (currentAngle < 2 * Math.PI)
            {
                xDir = espManager.travelOption.positive;
                yDir = espManager.travelOption.negative;
            }
            else
            {
                throw new Exception("ANGLE OUT OF BOUNDS");
            }

            if (accelerateMotion)
            {
                var watch = System.Diagnostics.Stopwatch.StartNew();
                Console.WriteLine("RUN ACCELERATION");
                var multiplier = acceleratedSpeed / zVel / 1000;
                esp.setVelocity(XAXIS, xVel * multiplier);
                esp.setVelocity(YAXIS, yVel * multiplier);
                esp.setVelocity(ZAXIS, zVel * multiplier);

                esp.moveIndefinitely(XAXIS, xDir);
                esp.moveIndefinitely(YAXIS, yDir);
                if (!reverseDirection)
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
            }else
            {
                Console.WriteLine("NO ACCELERATION DESIRED");
            }

            esp.setVelocity(XAXIS, xVel);
            esp.setVelocity(YAXIS, yVel);
            esp.setVelocity(ZAXIS, zVel);

            esp.moveIndefinitely(XAXIS, xDir);
            esp.moveIndefinitely(YAXIS, yDir);
            if (!reverseDirection)
            {
                esp.moveIndefinitely(ZAXIS, espManager.travelOption.positive);
            }else
            {
                esp.moveIndefinitely(ZAXIS, espManager.travelOption.negative);
            }
            waitForPreload();

            esp.stopMotion(XAXIS);
            esp.stopMotion(YAXIS);
            esp.stopMotion(ZAXIS);

            //wait at preload the desired amount of time
            esp.waitMS(Convert.ToInt32(preloadWaitTime * 1000));

            //Drag
            //Set the drag velocities
            xVel = Math.Abs(Math.Cos(currentAngle) * dragVelocity / 1000);
            yVel = Math.Abs(Math.Sin(currentAngle) * dragVelocity / 1000);

            esp.setVelocity(XAXIS, xVel);
            esp.setVelocity(YAXIS, yVel);

            esp.moveToRelativePosition(XAXIS, Math.Cos(currentAngle) * iteration.dragDistance / 1000);
            esp.moveToRelativePosition(YAXIS, Math.Sin(currentAngle) * iteration.dragDistance / 1000);

            esp.waitForMotionStop(XAXIS);
            esp.waitForMotionStop(YAXIS);

            //Move up
            //determine outgoing velocities
            xVel = Math.Abs(Math.Cos(iteration.outgoingAngle) * Math.Cos(currentAngle) * outgoingVelocity) / 1000;
            yVel = Math.Abs(Math.Cos(iteration.outgoingAngle) * Math.Sin(currentAngle) * outgoingVelocity) / 1000;
            zVel = Math.Sin(iteration.outgoingAngle) * outgoingVelocity / 1000;
            ascend(xVel, yVel, zVel, xDir, yDir);

            endCycle();
            //end preload early, reverting sometimes causes a large enought trigger to result in a false adhesion value

            //Set velocities to lateral motion
            esp.setVelocity(XAXIS, Constants.LATERALMOTIONSPEED);
            esp.setVelocity(YAXIS, Constants.LATERALMOTIONSPEED);
            esp.setVelocity(ZAXIS, Constants.LATERALMOTIONSPEED);

            //revert positions
            esp.moveToAbsolutePosition(XAXIS, startPosition.x);
            esp.moveToAbsolutePosition(YAXIS, startPosition.y);
            esp.moveToAbsolutePosition(ZAXIS, startPosition.z);

            esp.waitForMotionStop(XAXIS);
            esp.waitForMotionStop(YAXIS);
            esp.waitForMotionStop(ZAXIS);
        }

        /// <summary>
        /// Ascend after reaching a preload
        /// </summary>
        private void ascend(double xVel, double yVel, double zVel, espManager.travelOption xDir, espManager.travelOption yDir)
        {
            esp.setVelocity(XAXIS, xVel);
            esp.setVelocity(YAXIS, yVel);
            esp.setVelocity(ZAXIS, zVel);

            esp.moveIndefinitely(XAXIS, xDir);
            esp.moveIndefinitely(YAXIS, yDir);
            if (!reverseDirection)
            {
                esp.moveToRelativePosition(ZAXIS, -withdrawDistance / 1000);
            }else
            {
                esp.moveToRelativePosition(ZAXIS, withdrawDistance / 1000);
            }

            if (accelerateMotion)
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

           

            esp.waitForMotionStop(ZAXIS);
            esp.stopMotion(XAXIS);
            esp.stopMotion(YAXIS);

            esp.stopThreadUntilOperationsComplete();
        }

        protected override string getFileBody()
        {
            var s = "Angle,Preload,Adhesion" + Environment.NewLine;
            try
            {
                for (var i = 0; i < adhesionValues.Count; i++)
                {
                    //Add a new iteration header if it is needed
                    if (overrideTrialDefaults && i % numStrokes == 0 && (i / numStrokes) < trials.Length)
                    {
                        s += Environment.NewLine + "New Trial Iteration: " + Environment.NewLine;
                        Console.WriteLine(i);
                        s += "Drag Distance: " + trials[i / numStrokes].dragDistance + Environment.NewLine;
                        Console.WriteLine(i);
                        s += "Incoming Vertical Angle: " + trials[i / numStrokes].incomingAngle * 180 / Math.PI +
                             Environment.NewLine;
                        s += "Outgoing Vertical Angle: " + trials[i / numStrokes].outgoingAngle * 180 / Math.PI +
                             Environment.NewLine;
                        s += Environment.NewLine;
                    }
                    s += pastAngles[i] * 180 / Math.PI + "," + preloadValues[i] + "," + Math.Abs(adhesionValues[i]) + Environment.NewLine;
                }
            }
            catch (Exception e) { MessageBox.Show("ERROR: " + e.Message + Environment.NewLine + e.StackTrace); }
            return s;
        }

        protected override string getFileHeader()
        {
            var s = "Trial type: Radial Trial " + Environment.NewLine + Environment.NewLine;
            if (!overrideTrialDefaults)
            {
                s += "Drag distance " + dragDistance + Environment.NewLine;
            }
            s += "number of strokes " + numStrokes + Environment.NewLine;
            return s;
        }

        /// <summary>
        ///     Represents a full circular series of strokes
        /// </summary>
        public struct radialTrialIteration
        {
            /// <summary>
            ///     Angles are measured in radians
            /// </summary>
            public double incomingAngle, outgoingAngle;

            public double dragDistance;
        }
    }
}