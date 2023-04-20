using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace Blueprint.Actor
{

    using Blueprint.Actor.EventSystem;

    /// <summary>
    /// 蓝图actor基类
    /// </summary>
    public abstract class ActorBase : Blueprint.Logic.BP_Base
    {
        protected ActorEventSystem actorEventSystem = new ActorEventSystem();

        private BlueprintActor blueprintActor;

        private List<ActorInvoke> readyAddInvokeList;

        private List<ActorInvoke> invokeList;

        private List<ActorInvoke> readyRemoveInvokeList;

        private List<ActorInvoke> cancelInvokeList;

        [NotReflectAttribute]
        public Action OnActorGameObjectDestroyed;

        [NotReflectAttribute]
        public Transform transform => blueprintActor.transform;

        [NotReflectAttribute]
        public GameObject gameObject => blueprintActor.gameObject;
        
        [NotReflectAttribute]
        public string ActorPath = String.Empty;

        [NotReflectAttribute]
        public ActorBase()
        {

        }

        [NotReflectAttribute]
        public virtual void Awake() {}

        [NotReflectAttribute]
        public override void Start() {}

        [NotReflectAttribute]
        public virtual void Update() {}

        [NotReflectAttribute]
        public override void OnDestroy()
        {
            base.OnDestroy();
            OnActorGameObjectDestroyed?.Invoke();
        }

        [NotReflectAttribute]
        public virtual void FixedUpdate() {}

        [NotReflectAttribute]
        public virtual void LateUpdate() {}

        [NotReflectAttribute]
        public virtual void OnApplicationFocus(bool hasFocus) {}

        [NotReflectAttribute]
        public virtual void OnApplicationPause(bool pauseStatus) {}

        [NotReflectAttribute]
        public virtual void OnApplicationQuit() {}

        [NotReflectAttribute]
        public virtual void ResetVarValue() { }

        [NotReflectAttribute]
        public void InvokeUpdate(float time)
        {
            if (invokeList == null)
            {
                return ;
            }

            foreach (ActorInvoke invoke in invokeList)
            {
                invoke.time -= time;

                if (invoke.time <= 0)
                {
                    invoke.action.Invoke();

                    if (invoke.repeatRate == 0)
                    {
                        readyRemoveInvokeList.Add(invoke);
                    }
                    else
                    {
                        invoke.time = invoke.repeatRate;
                    }
                }
            }

            foreach (ActorInvoke invoke in readyRemoveInvokeList)
            {
                invokeList.Remove(invoke);
            }

            readyRemoveInvokeList.Clear();

            foreach (ActorInvoke invoke in cancelInvokeList)
            {
                invokeList.Remove(invoke);
            }

            cancelInvokeList.Clear();

            foreach (ActorInvoke invoke in readyAddInvokeList)
            {
                invokeList.Add(invoke);
            }

            readyAddInvokeList.Clear();
        }

        [NotReflectAttribute]
        public ActorEventSystem GetActorEventSystem()
        {
            return actorEventSystem;
        }

        [NotReflectAttribute]
        public void SetBlueprintActor(BlueprintActor actor)
        {
            blueprintActor = actor;
        }

        [NotReflectAttribute]
        public void SetParent(Transform transform, bool worldPositionStays = true)
        {
            if (blueprintActor != null)
            {
                blueprintActor.gameObject?.transform.SetParent(transform, worldPositionStays);
            }
        }

        [NotReflectAttribute]
        public void CancelInvoke(ActorInvoke invoke)
        {
            if (invokeList == null)
            {
                return ;
            }

            cancelInvokeList.Add(invoke);
        }

        [NotReflectAttribute]
        public void CancelAllInvoke()
        {
            if (invokeList == null)
            {
                return ;
            }

            cancelInvokeList.AddRange(invokeList);
        }

        [NotReflectAttribute]
        public void ClearAllInvokeList()
        {
            invokeList = new List<ActorInvoke>();
            cancelInvokeList = new List<ActorInvoke>();
            readyAddInvokeList = new List<ActorInvoke>();
            readyRemoveInvokeList = new List<ActorInvoke>();
        }
        
        protected ActorInvoke Invoke(Action action, float time)
        {
            if (action == null)
            {
                return null;
            }

            ActorInvoke ai = new ActorInvoke();
            ai.action = action;
            ai.time = time;
            AddInvoke(ai);
            return ai;
        }

        protected ActorInvoke InvokeRepeating(Action action, float time, float repeatRate)
        {
            if (action == null)
            {
                return null;
            }

            if (repeatRate < 0)
            {
                repeatRate = 0;
            }

            ActorInvoke ai = new ActorInvoke();
            ai.action = action;
            ai.time = time;
            ai.repeatRate = repeatRate;
            AddInvoke(ai);

            return ai;
        }

        protected void AddInvoke(ActorInvoke invoke)
        {
            if (invokeList == null)
            {
                invokeList = new List<ActorInvoke>();
                readyAddInvokeList = new List<ActorInvoke>();
                readyRemoveInvokeList = new List<ActorInvoke>();
                cancelInvokeList = new List<ActorInvoke>();
            }

            invoke.actor = this;
            readyAddInvokeList.Add(invoke);
        }

        protected GameObject GetGameObject(string uniqueName)
        {
            return blueprintActor.GetGameObject(uniqueName);
        }

        protected Transform GetTransform(string uniqueName)
        {
            GameObject obj = GetGameObject(uniqueName);

            if (obj != null)
            {
                return obj.transform;
            }

            return null;
        }

        protected T GetComponent<T>(string uniqueName) where T : Component
        {
            GameObject obj = GetGameObject(uniqueName);

            if (obj != null)
            {
                return obj.GetComponent<T>();
            }

            return default(T);
        }
    }

}