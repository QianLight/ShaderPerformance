using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Blueprint.Actor.EventSystem
{
    /// <summary>
    /// 该枚举值在蓝图BlueprintActorEventNode类中也存在，并且相同
    /// </summary>
    public enum ActorEventType
    {
        Awake                           = 1,
        Start                           = 2,
        Update                          = 3,
        OnDestroy                       = 4,
        OnDisable                       = 5,
        OnEnable                        = 6,

        OnCollisionEnter                = 7,
        OnCollisionStay                 = 8,
        OnCollisionExit                 = 9,
        OnCollisionEnter2D              = 10,
        OnCollisionStay2D               = 11,
        OnCollisionExit2D               = 12,

        OnTriggerEnter                  = 13,
        OnTriggerStay                   = 14,
        OnTriggerExit                   = 15,

        OnTriggerEnter2D                = 16,
        OnTriggerStay2D                 = 17,
        OnTriggerExit2D                 = 18,

        OnAnimatorIK                    = 19,
        OnAnimatorMove                  = 20,
        OnTransformChildrenChanged      = 21,
        OnTransformParentChanged        = 22,

        Reset                           = 23,
        OnMouseDown                     = 24,
        OnMouseDrag                     = 25,
        OnMouseEnter                    = 26,
        OnMouseExit                     = 27,
        OnMouseOver                     = 28,
        OnMouseUp                       = 29,
        OnMouseUpAsButton               = 30,

        OnControllerColliderHit         = 31,
        OnJointBreak                    = 32,
        OnJointBreak2D                  = 33,
        OnParticleCollision             = 34,
        OnParticleSystemStopped         = 35,
        OnParticleTrigger               = 36,
        OnParticleUpdateJobScheduled    = 37,

        OnPreCull                       = 38,
        OnPreRender                     = 39,
        OnPostRender                    = 40,
        OnRenderImage                   = 41,
        OnRenderObject                  = 42,
        OnWillRenderObject              = 43,

        OnBeginDrag                     = 44,
        OnCancel                        = 45,
        OnDeselect                      = 46,
        OnDrag                          = 47,
        OnDrop                          = 48,
        OnEndDrag                       = 49,
        OnInitializePotentialDrag       = 50,
        OnMove                          = 51,
        OnPointerClick                  = 52,
        OnPointerDown                   = 53,
        OnPointerEnter                  = 54,
        OnPointerExit                   = 55,
        OnPointerUp                     = 56,
        OnScroll                        = 57,
        OnSelect                        = 58,
        OnSubmit                        = 59,
        OnUpdateSelected                = 60,
    }

    public class ActorEventSystem
    {
        public static Dictionary<ActorEventType, Type> EventTypeDic = new Dictionary<ActorEventType, Type>()
        {
            {ActorEventType.Awake,                          typeof(ActorEventTrigger)},
            {ActorEventType.Start,                          typeof(ActorEventTrigger)},
            {ActorEventType.Update,                         typeof(ActorEventTrigger)},
            {ActorEventType.OnDestroy,                      typeof(ActorEventTrigger)},
            {ActorEventType.OnDisable,                      typeof(ActorEventTrigger)},
            {ActorEventType.OnEnable,                       typeof(ActorEventTrigger)},

            {ActorEventType.OnCollisionEnter,               typeof(ActorEventTrigger)},
            {ActorEventType.OnCollisionStay,                typeof(ActorEventTrigger)},
            {ActorEventType.OnCollisionExit,                typeof(ActorEventTrigger)},
            {ActorEventType.OnCollisionEnter2D,             typeof(ActorEventTrigger)},
            {ActorEventType.OnCollisionStay2D,              typeof(ActorEventTrigger)},
            {ActorEventType.OnCollisionExit2D,              typeof(ActorEventTrigger)},

            {ActorEventType.OnTriggerEnter,                 typeof(ActorEventTrigger)},
            {ActorEventType.OnTriggerStay,                  typeof(ActorEventTrigger)},
            {ActorEventType.OnTriggerExit,                  typeof(ActorEventTrigger)},

            {ActorEventType.OnTriggerEnter2D,               typeof(ActorEventTrigger)},
            {ActorEventType.OnTriggerStay2D,                typeof(ActorEventTrigger)},
            {ActorEventType.OnTriggerExit2D,                typeof(ActorEventTrigger)},

            {ActorEventType.OnAnimatorIK,                   typeof(ActorEventTrigger)},
            {ActorEventType.OnAnimatorMove,                 typeof(ActorEventAnimatorMove)},
            {ActorEventType.OnTransformChildrenChanged,     typeof(ActorEventTrigger)},
            {ActorEventType.OnTransformParentChanged,       typeof(ActorEventTrigger)},

            {ActorEventType.Reset,                          typeof(ActorEventTrigger)},
            {ActorEventType.OnMouseDown,                    typeof(ActorEventTrigger)},
            {ActorEventType.OnMouseDrag,                    typeof(ActorEventTrigger)},
            {ActorEventType.OnMouseEnter,                   typeof(ActorEventTrigger)},
            {ActorEventType.OnMouseExit,                    typeof(ActorEventTrigger)},
            {ActorEventType.OnMouseOver,                    typeof(ActorEventTrigger)},
            {ActorEventType.OnMouseUp,                      typeof(ActorEventTrigger)},
            {ActorEventType.OnMouseUpAsButton,              typeof(ActorEventTrigger)},

            {ActorEventType.OnControllerColliderHit,        typeof(ActorEventTrigger)},
            {ActorEventType.OnJointBreak,                   typeof(ActorEventTrigger)},
            {ActorEventType.OnJointBreak2D,                 typeof(ActorEventTrigger)},
            {ActorEventType.OnParticleCollision,            typeof(ActorEventTrigger)},
            {ActorEventType.OnParticleSystemStopped,        typeof(ActorEventTrigger)},
            {ActorEventType.OnParticleTrigger,              typeof(ActorEventTrigger)},
            {ActorEventType.OnParticleUpdateJobScheduled,   typeof(ActorEventTrigger)},

            {ActorEventType.OnPreCull,                      typeof(ActorEventTrigger)},
            {ActorEventType.OnPreRender,                    typeof(ActorEventTrigger)},
            {ActorEventType.OnPostRender,                   typeof(ActorEventTrigger)},
            {ActorEventType.OnRenderImage,                  typeof(ActorEventTrigger)},
            {ActorEventType.OnRenderObject,                 typeof(ActorEventTrigger)},
            {ActorEventType.OnWillRenderObject,             typeof(ActorEventTrigger)},

            {ActorEventType.OnBeginDrag,                    typeof(ActorEventDrag)},
            {ActorEventType.OnDrag,                         typeof(ActorEventDrag)},
            {ActorEventType.OnEndDrag,                      typeof(ActorEventDrag)},

            {ActorEventType.OnCancel,                       typeof(ActorEventSubmitCancel)},
            {ActorEventType.OnSubmit,                       typeof(ActorEventSubmitCancel)},

            {ActorEventType.OnSelect,                       typeof(ActorEventSelectDeselect)},
            {ActorEventType.OnDeselect,                     typeof(ActorEventSelectDeselect)},

            {ActorEventType.OnDrop,                         typeof(ActorEventDrop)},
            {ActorEventType.OnInitializePotentialDrag,      typeof(ActorEventInitializePotentialDrag)},
            {ActorEventType.OnMove,                         typeof(ActorEventMove)},
            {ActorEventType.OnScroll,                       typeof(ActorEventScroll)},
            {ActorEventType.OnUpdateSelected,               typeof(ActorEventUpdateSelected)},

            {ActorEventType.OnPointerClick,                 typeof(ActorEventPointerClick)},
            {ActorEventType.OnPointerDown,                  typeof(ActorEventPointerUpDown)},
            {ActorEventType.OnPointerUp,                    typeof(ActorEventPointerUpDown)},
            {ActorEventType.OnPointerEnter,                 typeof(ActorEventPointerEnterExit)},
            {ActorEventType.OnPointerExit,                  typeof(ActorEventPointerEnterExit)},
        };

        private Dictionary<GameObject, Dictionary<int, Delegate>> eventParamDic;

        public ActorEventSystem()
        {
            eventParamDic = new Dictionary<GameObject, Dictionary<int, Delegate>>();
        }

        public void Sent<T>(ActorEventType eventType, GameObject obj, T param1, T param2)
        {
            Send<T>((int)eventType, obj, param1, param2);
        }

        /// <summary>
        /// 发送具有两个参数的事件
        /// </summary>
        /// <param name="eventType"></param>
        /// <param name="obj"></param>
        /// <param name="param1"></param>
        /// <typeparam name="T"></typeparam>
        public void Send<T>(int eventType, GameObject obj, T param1, T param2)
        {
            if (obj == null)
            {
                return ;
            }

            Dictionary<int, Delegate> dic = GetDic(obj);
            Action<T, T> action = GetAction(dic, eventType) as Action<T, T>;

            action?.Invoke(param1, param2);
        }

        public void Sent<T>(ActorEventType eventType, GameObject obj, T param1)
        {
            Send<T>((int)eventType, obj, param1);
        }

        /// <summary>
        /// 发送具有一个参数的事件
        /// </summary>
        /// <param name="eventType"></param>
        /// <param name="obj"></param>
        /// <param name="param1"></param>
        /// <typeparam name="T"></typeparam>
        public void Send<T>(int eventType, GameObject obj, T param1)
        {
            if (obj == null)
            {
                return ;
            }

            Dictionary<int, Delegate> dic = GetDic(obj);
            Action<T> action = GetAction(dic, eventType) as Action<T>;

            action?.Invoke(param1);
        }

        public void Sent(ActorEventType eventType, GameObject obj)
        {
            Send((int)eventType, obj);
        }

        /// <summary>
        /// 发送没有参数的事件
        /// </summary>
        /// <param name="eventType"></param>
        /// <param name="obj"></param>
        public void Send(int eventType, GameObject obj)
        {
            if (obj == null)
            {
                return;
            }

            Dictionary<int, Delegate> dic = GetDic(obj);
            Action action = GetAction(dic, eventType) as Action;

            action?.Invoke();
        }

        public void Send(ActorEventType eventType)
        {
            Send((int)eventType);
        }

        /// <summary>
        /// 给所有物体发送无参事件
        /// </summary>
        /// <param name="eventType"></param>
        public void Send(int eventType)
        {
            foreach (var dic in eventParamDic)
            {
                Action action = GetAction(dic.Value, eventType) as Action;
                action?.Invoke();
            }
        }

        public Action ListenEvent(ActorEventType eventType, GameObject obj, Action callBack)
        {
            return ListenEvent((int)eventType, obj, callBack);
        }

        /// <summary>
        /// 监听没有参数的事件
        /// </summary>
        /// <param name="eventType"></param>
        /// <param name="obj"></param>
        /// <param name="callBack"></param>
        /// <returns></returns>
        public Action ListenEvent(int eventType, GameObject obj, Action callBack)
        {
            if (obj == null || callBack == null)
            {
                return null;
            }

            EventSystemCheck.CheckEventSystem(eventType);

            Dictionary<int, Delegate> dic = GetDic(obj);
            Delegate dele = GetAction(dic, eventType);
            Action action = null;

            AddEventTrigger(eventType, obj);

            if (dele == null)
            {
                action = delegate {};
                dic.Add(eventType, action);
            }
            else
            {
                action = dele as Action;
            }

            action += callBack;
            // 绑定之后需要重新赋值，具体原因可参考如下连接
            // https://stackoverflow.com/questions/26396843/delegates-act-as-value-types
            dic[eventType] = action;
            return callBack;
        }

        public Action<T> ListenEvent<T>(ActorEventType eventType, GameObject obj, Action<T> callBack)
        {
            return ListenEvent<T>((int)eventType, obj, callBack);
        }

        /// <summary>
        /// 监听具有一个参数的事件
        /// </summary>
        /// <param name="eventType"></param>
        /// <param name="obj"></param>
        /// <param name="callBack"></param>
        /// <returns></returns>
        public Action<T> ListenEvent<T>(int eventType, GameObject obj, Action<T> callBack)
        {
            if (obj == null || callBack == null)
            {
                return null;
            }

            EventSystemCheck.CheckEventSystem(eventType);

            Dictionary<int, Delegate> dic = GetDic(obj);
            Delegate dele = GetAction(dic, eventType);
            Action<T> action = null;

            AddEventTrigger(eventType, obj);

            if (dele == null)
            {
                action = delegate {};
                dic.Add(eventType, action);
            }
            else
            {
                action = dele as Action<T>;
            }

            action += callBack;
            // 绑定之后需要重新赋值，具体原因可参考如下连接
            // https://stackoverflow.com/questions/26396843/delegates-act-as-value-types
            dic[eventType] = action;

            return callBack;
        }

        public Action<T, T> ListenEvent<T>(ActorEventType eventType, GameObject obj, Action<T, T> callBack)
        {
            return ListenEvent<T>((int)eventType, obj, callBack);
        }

        /// <summary>
        /// 监听具有两个参数的事件
        /// </summary>
        /// <param name="eventType"></param>
        /// <param name="obj"></param>
        /// <param name="callBack"></param>
        /// <returns></returns>
        public Action<T, T> ListenEvent<T>(int eventType, GameObject obj, Action<T, T> callBack)
        {
            if (obj == null || callBack == null)
            {
                return null;
            }

            EventSystemCheck.CheckEventSystem(eventType);

            Dictionary<int, Delegate> dic = GetDic(obj);
            Delegate dele = GetAction(dic, eventType);
            Action<T, T> action = null;

            if (dele == null)
            {
                action = delegate {};
                dic.Add(eventType, action);
            }
            else
            {
                action = dele as Action<T, T>;
            }

            action += callBack;
            // 绑定之后需要重新赋值，具体原因可参考如下连接
            // https://stackoverflow.com/questions/26396843/delegates-act-as-value-types
            dic[eventType] = action;
            return callBack;
        }

        /// <summary>
        /// 获取代理字典
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        private Dictionary<int, Delegate> GetDic(GameObject obj)
        {
            Dictionary<int, Delegate> dic = null;

            if (eventParamDic != null && !eventParamDic.TryGetValue(obj, out dic))
            {
                dic = new Dictionary<int, Delegate>();
                eventParamDic.Add(obj, dic);
            }

            return dic;
        }

        private Delegate GetAction(Dictionary<int, Delegate> dic, int eventType)
        {
            if (dic == null)
            {
                return null;
            }

            Delegate dele = null;
            dic.TryGetValue(eventType, out dele);
            return dele;
        }

        private void AddEventTrigger(int eventType, GameObject obj)
        {
            EventTypeDic.TryGetValue((ActorEventType)eventType, out var triggerType);

            if (triggerType == null)
            {
                Debug.Log("找不到指定的actor事件类型: " + eventType.ToString());
                return ;
            }

            Component comp = obj.GetOrAddComponent(triggerType);

            if (comp is ActorEventTrigger)
            {
                ActorEventTrigger trigger = comp as ActorEventTrigger;
                trigger.AddEventSystem(this);
            }
            else
            {
                ActorEventBase trigger = comp as ActorEventBase;
                trigger.AddEventSystem(this);
            }
        }
    }
}