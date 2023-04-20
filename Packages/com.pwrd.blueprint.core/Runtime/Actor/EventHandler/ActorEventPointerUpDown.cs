using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using Blueprint.Actor.EventSystem;

namespace Blueprint.Actor
{
    [DisallowMultipleComponent]
    public class ActorEventPointerUpDown : ActorEventBase, IPointerUpHandler, IPointerDownHandler
    {

        public void OnPointerUp(PointerEventData eventData)
        {
            SendEvent(ActorEventType.OnPointerUp, eventData);
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            SendEvent(ActorEventType.OnPointerDown, eventData);
        }

    }

}