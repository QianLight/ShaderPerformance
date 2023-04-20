using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using Blueprint.Actor.EventSystem;

namespace Blueprint.Actor
{
    [DisallowMultipleComponent]
    public class ActorEventTrigger : MonoBehaviour
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

        void Start()
        {
            SendEvent(ActorEventType.Start);
        }


        private void OnCollisionEnter(Collision other)
        {
            SendEvent(ActorEventType.OnCollisionEnter, other);
        }

        private void OnCollisionStay(Collision other)
        {
            SendEvent(ActorEventType.OnCollisionStay, other);
        }

        private void OnCollisionExit(Collision other)
        {
            SendEvent(ActorEventType.OnCollisionExit, other);
        }

        private void OnCollisionEnter2D(Collision2D other)
        {
            SendEvent(ActorEventType.OnCollisionEnter2D, other);
        }

        private void OnCollisionStay2D(Collision2D other)
        {
            SendEvent(ActorEventType.OnCollisionStay2D, other);
        }

        private void OnCollisionExit2D(Collision2D other)
        {
            SendEvent(ActorEventType.OnCollisionExit2D, other);
        }

        private void OnTriggerEnter(Collider other)
        {
            SendEvent(ActorEventType.OnTriggerEnter, other);
        }

        private void OnTriggerStay(Collider other)
        {
            SendEvent(ActorEventType.OnTriggerStay, other);
        }

        private void OnTriggerExit(Collider other)
        {
            SendEvent(ActorEventType.OnTriggerExit, other);
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            SendEvent(ActorEventType.OnTriggerEnter2D, other);
        }

        private void OnTriggerStay2D(Collider2D other)
        {
            SendEvent(ActorEventType.OnTriggerStay2D, other);
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            SendEvent(ActorEventType.OnTriggerExit2D, other);
        }

        private void OnDisable()
        {
            SendEvent(ActorEventType.OnDisable);
        }

        private void OnEnable()
        {
            SendEvent(ActorEventType.OnEnable);
        }

        void OnDestroy()
        {
            SendEvent(ActorEventType.OnDestroy);
        }

        void OnAnimatorIK(int layerIndex)
        {
            SendEvent(ActorEventType.OnAnimatorIK, layerIndex);
        }

        void OnTransformChildrenChanged()
        {
            SendEvent(ActorEventType.OnTransformChildrenChanged);
        }

        void OnTransformParentChanged()
        {
            SendEvent(ActorEventType.OnTransformParentChanged);
        }

        void Reset()
        {
            SendEvent(ActorEventType.Reset);
        }

        void OnMouseDown()
        {
            SendEvent(ActorEventType.OnMouseDown);
        }

        void OnMouseDrag()
        {
            SendEvent(ActorEventType.OnMouseDrag);
        }

        void OnMouseEnter()
        {
            SendEvent(ActorEventType.OnMouseEnter);
        }

        void OnMouseExit()
        {
            SendEvent(ActorEventType.OnMouseExit);
        }

        void OnMouseOver()
        {
            SendEvent(ActorEventType.OnMouseOver);
        }

        void OnMouseUp()
        {
            SendEvent(ActorEventType.OnMouseUp);
        }

        void OnMouseUpAsButton()
        {
            SendEvent(ActorEventType.OnMouseUpAsButton);
        }

        void OnControllerColliderHit(ControllerColliderHit hit)
        {
            SendEvent(ActorEventType.OnControllerColliderHit, hit);
        }

        void OnJointBreak(float breakForce)
        {
            SendEvent(ActorEventType.OnJointBreak, breakForce);
        }

        void OnJointBreak2D(Joint2D brokenJoint)
        {
            SendEvent(ActorEventType.OnJointBreak2D, brokenJoint);
        }

        void OnParticleCollision(GameObject other)
        {
            SendEvent(ActorEventType.OnParticleCollision, other);
        }

        void OnParticleSystemStopped()
        {
            SendEvent(ActorEventType.OnParticleSystemStopped);
        }

        void OnParticleTrigger()
        {
            SendEvent(ActorEventType.OnParticleTrigger);
        }

        void OnParticleUpdateJobScheduled()
        {
            SendEvent(ActorEventType.OnParticleUpdateJobScheduled);
        }

        void OnPreCull()
        {
            SendEvent(ActorEventType.OnPreCull);
        }

        void OnPreRender()
        {
            SendEvent(ActorEventType.OnPreRender);
        }

        void OnPostRender()
        {
            SendEvent(ActorEventType.OnPostRender);
        }

        void OnRenderImage(RenderTexture src, RenderTexture dest)
        {
            SendEvent(ActorEventType.OnRenderImage, src, dest);
        }

        void OnRenderObject()
        {
            SendEvent(ActorEventType.OnRenderObject);
        }

        void OnWillRenderObject()
        {
            SendEvent(ActorEventType.OnWillRenderObject);
        }

        private void SendEvent(ActorEventType val)
        {
            foreach (ActorEventSystem s in eventSystems)
            {
                s.Send((int)val, this.gameObject);
            }
        }

        private void SendEvent<T>(ActorEventType val, T param1)
        {
            foreach (ActorEventSystem s in eventSystems)
            {
                s.Send((int)val, this.gameObject, param1);
            }
        }

        private void SendEvent<T>(ActorEventType val, T param1, T param2)
        {
            foreach (ActorEventSystem s in eventSystems)
            {
                s.Send((int)val, this.gameObject, param1, param2);
            }
        }

    }

}