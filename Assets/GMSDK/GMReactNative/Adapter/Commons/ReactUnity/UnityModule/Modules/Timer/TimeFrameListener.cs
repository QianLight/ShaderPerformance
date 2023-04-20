using System;
using System.Collections;
using System.Collections.Generic;

namespace GSDK.RNU
{
    public class TimeFrameListener: IFrameListener
    {
        private TimerModule tm;
        public TimeFrameListener(TimerModule tm)
        {
            this.tm = tm;
        }

        public void Do()
        {
            List<Timer> timers= tm.GetTimers();
            if (timers.Count <= 0)
            {
                return;
            }

            long nowTime = DateTime.Now.Ticks / 10000;

            ArrayList timersShouldFire = new ArrayList();
            foreach (Timer t in timers)
            {
                if (t.targetTime <= nowTime)
                {
                    timersShouldFire.Add(t.callbackID);
                    
                    if (t.repeat)
                    {
                        t.targetTime = nowTime + t.interval;
                    }
                }
            }
            timers.RemoveAll(t => t.targetTime <= nowTime);

            if (timersShouldFire.Count > 0)
            {
                tm.CallTimers(timersShouldFire);
            }
        }
    }
}