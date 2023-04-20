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
    internal class EventBase
    {
        private string id;
        private Action<string, System.Object> actions;

        public EventBase(string eventId)
        {
            this.id = eventId;
        }

        public void Fire(System.Object param)
        {
            actions?.Invoke(id, param);
        }

        public void AddEventListener(Action<string, System.Object> action)
        {
            actions += action;
        }
        public void RemoveEventListener(Action<string, System.Object> action)
        {
            actions -= action;
        }
    }
}

