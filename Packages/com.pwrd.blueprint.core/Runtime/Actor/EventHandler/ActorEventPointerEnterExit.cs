using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using Blueprint.Actor.EventSystem;

namespace Blueprint.Actor
{
    [DisallowMultipleComponent]
    public class ActorEventPointerEnterExit : ActorEventBase, IPointerEnterHandler, IPointerExitHandler
    {

        public void OnPointerEnter(PointerEventData eventData)
        {
            SendEvent(ActorEventType.OnPointerEnter, eventData);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            SendEvent(ActorEventType.OnPointerExit, eventData);
        }
    }

}