using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using Blueprint.Actor.EventSystem;

namespace Blueprint.Actor
{
    [DisallowMultipleComponent]
    public class ActorEventUpdateSelected : ActorEventBase, IUpdateSelectedHandler
    {

        public void OnUpdateSelected(BaseEventData eventData)
        {
            SendEvent(ActorEventType.OnUpdateSelected, eventData);
        }

    }

}