/*******************************************************************
* Copyright © 2017—2021 Perfect World.Co.Ltd. All rights reserved.
* author: 移动项目支持部 ZEUS FRAMEWORK TEAM
********************************************************************/
using UnityEngine;
using System;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;

namespace Zeus.Core
{
    public static partial class AwaiterExtensions
    {
        public static AsyncOperationAwaiter<T> GetAwaiter<T>(this T operation) where T : AsyncOperation
        {
            return AsyncOperationAwaiter<T>.GetAwaiter(operation);
        }

        public static TaskAwaiter GetAwaiter(this TimeSpan timeSpan)
        {
            return Task.Delay(timeSpan).GetAwaiter();
        }
    }
}