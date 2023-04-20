/*
 * @author yankang.nj
 * timer 相关类。用来处理JS测的 setTimeout， setInterval， requestAnimationFrame， requestIdleCallback等
 *
 * 在调用应该触发的情况下：需要调用callTimers 来触发相关方法的执行。 如果是requestIdleCallback需要调用callIdleCallbacks
 */

using System;
using System.Collections;
using System.Collections.Generic;

namespace GSDK.RNU
{
    public class TimerModule : SimpleUnityModule
    {
        public static string NAME = "Timing";
        
        private List<Timer> timers = new List<Timer>();

        private bool sendIdleEvents = false; 

        public TimerModule()
        {
            IFrameListener fl = new TimeFrameListener(this);
            StaticCommonScript.AddFrameListener(fl);

            IFrameIdleListener fil = new TimeIdleFrameListener(this);
            StaticCommonScript.AddFrameIdleListener(fil);
        }

        public List<Timer> GetTimers()
        {
            return timers;
        }

        public bool HasIdleEvents()
        {
            return sendIdleEvents;
        }

        public override string GetName()
        {
            return NAME;
        }

        [ReactMethod]
        public void createTimer(int callbackID, int duration, double jsSchedulingTime, bool repeat) {
            Util.Log("createTimer {0} {1} {2} {3}", callbackID, duration, jsSchedulingTime, repeat);
            
            if (duration <= 0 && !repeat)
            {
                CallTimers(new ArrayList(){callbackID});
                return;
            }
            
            long deviceTime = DateTime.Now.Ticks / 10000;
            Util.Log("add Timer..");
            Timer t = new Timer(callbackID, repeat, duration, deviceTime + duration);
            timers.Add(t);
        }

        public void CallTimers(ArrayList timersShouldFire)
        {
            RNUMainCore.CallJSFunction("JSTimers", "callTimers", new ArrayList(){timersShouldFire});
        }

        public void CallIdleCallbacks(long absoluteFrameStartTime)
        {
            RNUMainCore.CallJSFunction("JSTimers", "callIdleCallbacks", new ArrayList(){absoluteFrameStartTime});
        }

        [ReactMethod]
        public void deleteTimer(int timerId)
        {
            timers.RemoveAll(t => t.callbackID == timerId);
        }

        [ReactMethod]
        public void setSendIdleEvents(bool sendIdleEvents) {
            Util.Log("setSendIdleEvents {0}", sendIdleEvents);
            this.sendIdleEvents = sendIdleEvents;
        }

    }
}