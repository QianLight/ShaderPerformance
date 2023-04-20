/*******************************************************************
* Copyright © 2017—2021 Perfect World.Co.Ltd. All rights reserved.
* author: 移动项目支持部 ZEUS FRAMEWORK TEAM
********************************************************************/
using System.Threading;
using System.Runtime.CompilerServices;
using UnityEngine;
using System;

namespace Zeus.Core
{
    public static class AsyncAwaitUtil
    {
        private static int _unityThreadId;
        public static bool IsInUnityThread
        {
            get
            {
                return Thread.CurrentThread.ManagedThreadId == _unityThreadId;
            }
        }
        /// <summary>
        /// 可以将后续的逻辑转移到unity的线程执行
        /// await AsyncUtility.UnitySyncContext;可以将后续的逻辑转移到unity的线程执行
        /// </summary>
        public static SynchronizationContext UnitySyncContext
        {
            get; private set;
        }
        /// <summary>
        /// 可以将后续的逻辑转移到某个子线程执行
        /// await AsyncUtility.BackgroundSyncContext;可以将后续的逻辑转移到某个子线程执行
        /// </summary>
        public static SynchronizationContext BackgroundSyncContext
        {
            get; private set;
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        public static void AsyncUtilityInitlize()
        {
            _unityThreadId = Thread.CurrentThread.ManagedThreadId;
            UnitySyncContext = SynchronizationContext.Current;
            BackgroundSyncContext = new SynchronizationContext();
#if UNITY_EDITOR
            UnityEngine.Debug.Log("AsyncUtility::AsyncUtilityInitlize unity thread id:" + Thread.CurrentThread.ManagedThreadId);
            UnityEngine.Debug.Log(UnitySyncContext.ToString());//UnityEngine.UnitySynchronizationContext
#endif
        }
    }

    public static class SynchronizationContextExtensions
    {
        public static SynchronizationContextAwaiter GetAwaiter(this SynchronizationContext scontext)
        {
            return new SynchronizationContextAwaiter(scontext);
        }
    }

    public struct SynchronizationContextAwaiter : INotifyCompletion
    {
        private static readonly SendOrPostCallback _postCallback = (state) =>
        {
            Action action = (Action)state;
            action();
        };
        private readonly SynchronizationContext _context;

        public SynchronizationContextAwaiter(SynchronizationContext context)
        {
            this._context = context;
        }

        public bool IsCompleted
        {
            get
            {
                return _context == SynchronizationContext.Current;
            }
        }

        void INotifyCompletion.OnCompleted(Action continuation)
        {
            _context.Post(_postCallback, continuation);
        }

        public void GetResult()
        {
        }
    }
}
