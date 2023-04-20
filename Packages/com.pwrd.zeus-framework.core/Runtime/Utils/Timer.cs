/*******************************************************************
* Copyright © 2017—2021 Perfect World.Co.Ltd. All rights reserved.
* author: 移动项目支持部 ZEUS FRAMEWORK TEAM
********************************************************************/
using UnityEngine;
using System.Collections.Generic;


namespace Zeus
{
    public class Timer
    {
        private float m_time = float.MaxValue;
        private float m_timeInterval;
        private bool m_releaseWhenStop = false;
        private bool m_loop = false;

        private float m_TriggleTime = 0;

        Timers.Callback m_funcCallback;
        // Function need call before trigger.
        Timers.CallbackWithTime m_funcCallbackWithTime;

        public Timer(Timers.Callback cb, bool loop, Timers.CallbackWithTime cbt, bool releaseAfterStop)
        {
            m_funcCallback = cb;
            m_loop = loop;
            m_funcCallbackWithTime = cbt;
            m_releaseWhenStop = releaseAfterStop;
        }

        public void Start(float triggleTime)
        {
            m_TriggleTime = triggleTime;
            m_timeInterval = triggleTime;
            m_time = Time.time + triggleTime;
        }

        public void Stop()
        {
            m_time = float.MaxValue;
            m_loop = false;
            m_releaseWhenStop = false;
        }

        public bool Update()
        {
            if (Time.time >= m_time)
            {
                // Determine the timer is loop or not.
                if (m_loop)
                    m_time = Time.time + m_TriggleTime;
                else
                    m_time = float.MaxValue;

                if (m_funcCallback != null)
                {
                    m_funcCallback();
                    return m_releaseWhenStop;
                }
            }
            else if (m_funcCallbackWithTime != null)
            {
                // Absolute remain time.
                float remainTime = Time.time - m_time;
                m_funcCallbackWithTime(remainTime);
            }

            return false;
        }
    }

    public class Timers : Singleton<Timers>
    {
        public delegate void Callback();
        public delegate void CallbackWithTime(float timeValue);

        public LinkedList<Timer> m_Timers = new LinkedList<Timer>();

        // Create a new timer and add to the collection.
        public Timer Create(Callback cb, bool loop, CallbackWithTime cbt, bool releaseAfterStop)
        {
            var t = new Timer(cb, loop, cbt, releaseAfterStop);

            m_Timers.AddLast(t);

            return t;
        }

        public void Destory(Timer timer)
        {
            m_Timers.Remove(timer);
            timer = null;
        }

        public void Update()
        {
            var timerNode = m_Timers.First;
            while (timerNode != null)
            {
                if (timerNode.Value.Update() == true)
                {
                    // Remove after trigger once.
                    var tmp = timerNode;
                    timerNode = timerNode.Next;
                    m_Timers.Remove(tmp);
                }
                else
                {
                    timerNode = timerNode.Next;
                }
            }
        }
    }
}
