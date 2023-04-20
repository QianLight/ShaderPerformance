namespace GSDK.RNU
{
    public class Timer
    {
        public int callbackID;
        public bool repeat;
        public int interval; // 间隔
        public long targetTime;

        public Timer(int callbackID, bool repeat, int interval, long targetTime)
        {
            this.callbackID = callbackID;
            this.repeat = repeat;
            this.interval = interval;
            this.targetTime = targetTime;
        }

    }
}