using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using Blueprint.Actor.EventSystem;

namespace Blueprint.Actor
{
    [DisallowMultipleComponent]
    public class ActorEventDrop : ActorEventBase, IDropHandler
    {

        public void OnDrop(PointerEventData eventData)
        {
            SendEvent(ActorEventType.OnDrop, eventData);
        }

    }

}