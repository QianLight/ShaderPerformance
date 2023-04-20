/*******************************************************************
* Copyright © 2017—2021 Perfect World.Co.Ltd. All rights reserved.
* author: 移动项目支持部 ZEUS FRAMEWORK TEAM
********************************************************************/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace Zeus.Core.Event
{
    public class EventMgr
    {
        static private EventMgr instance = null;
        private Dictionary<string, EventBase> _eventDict;
        private EventMgr()
        {
            _eventDict = new Dictionary<string, EventBase>();
        }

        static public EventMgr Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new EventMgr();
                }
                return instance;
            }
        }

        // Update is called once per frame
        public void Update()
        {

        }

        public void SendEvent(string eventId, System.Object param)
        {
            EventBase targetEvent = null;
            if(_eventDict.TryGetValue(eventId, out targetEvent))
            {
                targetEvent.Fire(param);
            }
        }

        public void ListenEvent(string eventId, Action<string, System.Object> action)
        {
            EventBase targetEvent = null;
            if (!_eventDict.TryGetValue(eventId, out targetEvent))
            {
                targetEvent = new EventBase(eventId);
                _eventDict.Add(eventId, targetEvent);
            }
            targetEvent.AddEventListener(action);
        }

        public void UnListenEvent(string eventId, Action<string, System.Object> action)
        {
            EventBase targetEvent = null;
            if (_eventDict.TryGetValue(eventId, out targetEvent))
            {
                targetEvent.RemoveEventListener(action);
            }
        }

        public void SendEventNextFrame(string eventId, System.Object param)
        {

        }
    }
}

