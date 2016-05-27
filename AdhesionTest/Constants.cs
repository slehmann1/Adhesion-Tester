namespace AdhesionTest
{
    public static class Constants
    {
        public static int DAQFREQ { get { return 100; } }//how frequently data is acquired from the probes/graphs are updated in ms.
        public static int NUMSAMPLES { get { return 1; } }//how many samples are averaged per datapoint
        public static string SHEARCHANNELNAME { get { return "shearChannel"; } }
        public static string NORMALCHANNELNAME { get { return "normalChannel"; } }
        public static string SHEARCHANNELID { get { return "dev1/ai2"; } }
        public static string NORMALCHANNELID { get { return "dev1/ai0"; } }
        
    }
}