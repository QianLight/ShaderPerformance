using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using Blueprint.Actor.EventSystem;

namespace Blueprint.Actor
{
    [DisallowMultipleComponent]
    public class ActorEventMove : ActorEventBase, IMoveHandler
    {

        public void OnMove(AxisEventData eventData)
        {
            SendEvent(ActorEventType.OnMove, eventData);
        }

    }

}