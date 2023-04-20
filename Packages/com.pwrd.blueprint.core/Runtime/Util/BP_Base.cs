using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Blueprint.Logic
{
    [ClassPlatform(3)]
    public abstract class BP_Base
    {
        [NotReflectAttribute]
        public virtual void Start()
        {

        }

        [NotReflectAttribute]
        public virtual void OnDestroy()
        {
            DelayControl.Instance.RemoveAllDelay(this);
        }
    }
}

