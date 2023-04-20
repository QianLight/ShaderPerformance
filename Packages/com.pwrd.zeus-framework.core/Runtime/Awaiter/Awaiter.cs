/*******************************************************************
* Copyright © 2017—2021 Perfect World.Co.Ltd. All rights reserved.
* author: 移动项目支持部 ZEUS FRAMEWORK TEAM
********************************************************************/
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Zeus.Core
{
    public abstract class Awaiter : INotifyCompletion
    {
        protected Awaiter(int hashCode)
        {
            cache[hashCode] = this;
        }

        protected Action m_Continuation;
        protected bool m_IsCompleted;
        public bool IsCompleted
        {
            get
            {
                return m_IsCompleted;
            }
        }

        void INotifyCompletion.OnCompleted(Action continuation)
        {
            if (m_IsCompleted)
                continuation?.Invoke();
            else
                m_Continuation += continuation;
        }

        protected static Dictionary<int, Awaiter> cache = new Dictionary<int, Awaiter>();

        public static void Dispose()
        {
            cache.Clear();
        }
    }
}