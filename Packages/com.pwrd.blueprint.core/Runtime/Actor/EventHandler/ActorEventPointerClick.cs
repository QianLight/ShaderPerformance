using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using Blueprint.Actor.EventSystem;

namespace Blueprint.Actor
{
    [DisallowMultipleComponent]
    public class ActorEventPointerClick : ActorEventBase, IPointerClickHandler
    {

        public void OnPointerClick(PointerEventData eventData)
        {
            SendEvent(ActorEventType.OnPointerClick, eventData);
        }

    }

}