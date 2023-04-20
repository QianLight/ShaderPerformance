using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using Blueprint.Actor.EventSystem;

namespace Blueprint.Actor
{
    [DisallowMultipleComponent]
    public class ActorEventDrag : ActorEventBase, IBeginDragHandler, IDragHandler, IEndDragHandler
    {

        public void OnBeginDrag(PointerEventData eventData)
        {
            SendEvent(ActorEventType.OnBeginDrag, eventData);
        }

        public void OnDrag(PointerEventData eventData)
        {
            SendEvent(ActorEventType.OnDrag, eventData);
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            SendEvent(ActorEventType.OnEndDrag, eventData);
        }

    }

}