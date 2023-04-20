/*******************************************************************
* Copyright © 2017—2021 Perfect World.Co.Ltd. All rights reserved.
* author: 移动项目支持部 ZEUS FRAMEWORK TEAM
********************************************************************/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace Zeus.Framework.Asset
{
    public class AsyncAwaitOperation
    {
        internal event Action Completed;
        protected bool _isDone = false;

        public bool IsDone
        {
            get
            {
                return _isDone;
            }
        }

        internal void InvokeComplete()
        {
            _isDone = true;
            if (Completed != null)
            {
                Completed();
            }
        }
    }
}
