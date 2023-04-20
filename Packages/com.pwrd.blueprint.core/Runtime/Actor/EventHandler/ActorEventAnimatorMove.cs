using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using Blueprint.Actor.EventSystem;

namespace Blueprint.Actor
{
    [DisallowMultipleComponent]
    public class ActorEventAnimatorMove : ActorEventBase
    {
        /// <summary>
        /// MonoBehaviour中定义该函数之后，挂载该组件的物体上如果有Animator组件，那么Animator组件的Apply RootMotion属性会受到影响
        /// </summary>
        void OnAnimatorMove()
        {
            SendEvent(ActorEventType.OnAnimatorMove);
        }

    }

}