

using System.Collections;

namespace GSDK.RNU
{
    public class HMRClient
    {

        public static void setup(string ip, int port, bool enable)
        {
            
            RNUMainCore.CallJSFunction("HMRClient", "setup", new ArrayList()
            {
                "android",
                "index.bundle", 
                ip, 
                port,
                enable
            });
        }

        public static void disable()
        {
            RNUMainCore.CallJSFunction("HMRClient", "disable", new ArrayList());
        }

        public static void enable()
        {
            RNUMainCore.CallJSFunction("HMRClient", "enable", new ArrayList());
        }
    }
}