using System;


namespace GSDK.RNU
{
    public class TimeIdleFrameListener: IFrameIdleListener
    {
        private static readonly DateTime Jan1St1970 = new DateTime (1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        private int DEFAULT_FRAME_DURATION = 5;
        public static long Millis { get { return (long)((DateTime.UtcNow - Jan1St1970).TotalMilliseconds); } }

        
        private TimerModule tm;
        public TimeIdleFrameListener(TimerModule tm)
        {
            this.tm = tm;
        }

        public void Do()
        {
            if (tm.HasIdleEvents())
            {
                tm.CallIdleCallbacks(TimeIdleFrameListener.Millis - DEFAULT_FRAME_DURATION);
            }
        }
    }
}