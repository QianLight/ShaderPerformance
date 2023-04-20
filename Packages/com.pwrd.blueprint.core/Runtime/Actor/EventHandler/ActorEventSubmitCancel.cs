using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using Blueprint.Actor.EventSystem;

namespace Blueprint.Actor
{
    [DisallowMultipleComponent]
    public class ActorEventSubmitCancel : ActorEventBase, ISubmitHandler, ICancelHandler
    {

        public void OnSubmit(BaseEventData eventData)
        {
            SendEvent(ActorEventType.OnSubmit, eventData);
        }

        public void OnCancel(BaseEventData eventData)
        {
            SendEvent(ActorEventType.OnCancel, eventData);
        }

    }

}