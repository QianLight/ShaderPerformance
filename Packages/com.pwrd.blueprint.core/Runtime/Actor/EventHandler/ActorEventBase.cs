using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using Blueprint.Actor.EventSystem;

namespace Blueprint.Actor
{
    public class ActorEventBase : MonoBehaviour
    {
        private List<ActorEventSystem> eventSystems = new List<ActorEventSystem>();

        public void AddEventSystem(ActorEventSystem eventSystem)
        {
            if (eventSystems.Contains(eventSystem))
            {
                return ;
            }

            eventSystems.Add(eventSystem);
        }

        protected void SendEvent(ActorEventType val)
        {
            foreach (ActorEventSystem s in eventSystems)
            {
                s.Send((int)val, this.gameObject);
            }
        }

        protected void SendEvent<T>(ActorEventType val, T param1)
        {
            foreach (ActorEventSystem s in eventSystems)
            {
                s.Send((int)val, this.gameObject, param1);
            }
        }

        protected void SendEvent<T>(ActorEventType val, T param1, T param2)
        {
            foreach (ActorEventSystem s in eventSystems)
            {
                s.Send((int)val, this.gameObject, param1, param2);
            }
        }
    }
}
