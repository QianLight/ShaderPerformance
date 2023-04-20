using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using Blueprint.Actor.EventSystem;

namespace Blueprint.Actor
{
    [DisallowMultipleComponent]
    public class ActorEventScroll : ActorEventBase, IScrollHandler
    {

        public void OnScroll(PointerEventData eventData)
        {
            SendEvent(ActorEventType.OnScroll, eventData);
        }

    }

}