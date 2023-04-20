using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using Blueprint.Actor.EventSystem;

namespace Blueprint.Actor
{
    [DisallowMultipleComponent]
    public class ActorEventSelectDeselect : ActorEventBase, ISelectHandler, IDeselectHandler
    {

        public void OnSelect(BaseEventData eventData)
        {
            SendEvent(ActorEventType.OnSelect, eventData);
        }

        public void OnDeselect(BaseEventData eventData)
        {
            SendEvent(ActorEventType.OnDeselect, eventData);
        }

    }

}