using System;

namespace AdhesionTest
{
    public static class Constants
    {
        public static int DAQFREQ
        {
            get { return 1; }
        } //how frequently data is acquired from the probes/graphs are updated in ms.

        public static int NUMSAMPLES
        {
            get { return 1; }
        } //how many samples are averaged per datapoint

        public static string SHEARCHANNELNAME
        {
            get { return "shearChannel"; }
        } 

        public static string NORMALCHANNELNAME
        {
            get { return "normalChannel"; }
        }
        
        public static Guid ESPGUID
        {
            get { return new Guid("{4d36e978-e325-11ce-bfc1-08002be10318}"); }
        } // the unique ID windows uses to recognise devices. This can be viewed in devices and printers

        public static uint ESPVENDORID
        {
            get { return 0x104D; }
        }

        public static uint ESPPRODUCTID
        {
            get { return 0x3001; }
        }

        public static int BAUDRATE
        {
            get { return 921600; }
        }

        public static int DATABITS
        {
            get { return 8; }
        }

        /// <summary>
        /// The time to wait between commands in ms, is used to prevent overloading of the device
        /// </summary>
        public static int INTERCOMMANDWAITTIME
        {
            get { return 25; }
        }

        /// <summary>
        /// The time to wait after data is recieved from the esp301 in ms
        /// </summary>
        public static int POSTDATAWAITTIME
        {
            get { return 25; }
        }

        public static double LATERALMOTIONSPEED
        {
            get { return 2.5; }
        }

        public static string FILENAME
        {
            get { return "\\adhesion_test"; }
        }

        public static string FILEEXTENSION
        {
            get { return ".csv"; }
        }
    }
}