using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using Blueprint.Actor.EventSystem;

namespace Blueprint.Actor
{
    [DisallowMultipleComponent]
    public class ActorEventInitializePotentialDrag : ActorEventBase, IInitializePotentialDragHandler
    {

        public void OnInitializePotentialDrag(PointerEventData eventData)
        {
            SendEvent(ActorEventType.OnInitializePotentialDrag, eventData);
        }

    }

}