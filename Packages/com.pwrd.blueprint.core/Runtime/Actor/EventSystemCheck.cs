using UnityEngine;
using System.Collections.Generic;

namespace Blueprint.Actor.EventSystem
{

    public static class EventSystemCheck
    {
        // 当监听此集合中的事件时，需要检测场景中是否有unity的eventsystems组件
        private static HashSet<ActorEventType> needCheckUnityEventSystemsSet = new HashSet<ActorEventType>()
        {
            ActorEventType.OnBeginDrag,                    
            ActorEventType.OnDrag,                        
            ActorEventType.OnEndDrag,                    

            ActorEventType.OnCancel,                     
            ActorEventType.OnSubmit,                     

            ActorEventType.OnSelect,                       
            ActorEventType.OnDeselect,                

            ActorEventType.OnDrop,                       
            ActorEventType.OnInitializePotentialDrag,     
            ActorEventType.OnMove,                       
            ActorEventType.OnScroll,                      
            ActorEventType.OnUpdateSelected,              

            ActorEventType.OnPointerClick,                
            ActorEventType.OnPointerDown,                 
            ActorEventType.OnPointerUp,                   
            ActorEventType.OnPointerEnter,               
            ActorEventType.OnPointerExit
        };

        private static bool inited = false;

        /// <summary>
        /// 检测是否需要创建eventsystem组件
        /// </summary>
        /// <param name="eventType"></param>
        public static void CheckEventSystem(int eventType)
        {
            if (!needCheckUnityEventSystemsSet.Contains((ActorEventType)eventType) || inited)
            {
                return ;
            }

            // 首先找场景中是否有eventsystem组件
            var eventSystem = GameObject.FindObjectOfType<UnityEngine.EventSystems.EventSystem>();

            if (eventSystem == null)
            {
                var obj = new GameObject("EventSystem");
                obj.AddComponent<UnityEngine.EventSystems.EventSystem>();
                obj.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
                MonoBehaviour.DontDestroyOnLoad(obj);
            }
            else 
            {
                MonoBehaviour.DontDestroyOnLoad(eventSystem.gameObject);
            }

            inited = true;
        }
    }

}