/*******************************************************************
* Copyright © 2017—2021 Perfect World.Co.Ltd. All rights reserved.
* author: 移动项目支持部 ZEUS FRAMEWORK TEAM
********************************************************************/
using UnityEngine;
using System.Threading;

namespace Zeus.Core
{
    public static class AndroidJNIUtil
    {

        private static int _mainThreadID = int.MinValue;
        private static System.Collections.Concurrent.ConcurrentDictionary<int, int> thread2AttachCount = new System.Collections.Concurrent.ConcurrentDictionary<int, int>();

        static AndroidJNIUtil()
        {
#if !UNITY_EDITOR && UNITY_ANDROID
            Zeus.Core.ZeusCore.Instance.AddMainThreadTask(GetMainThreadID);
#endif
        }

        private static void GetMainThreadID()
        {
            _mainThreadID = Thread.CurrentThread.ManagedThreadId;
        }

        /// <summary>
        /// 注意::本函数必须在主线程调用！！！
        /// </summary>
        public static void InitMainThreadID()
        {
            if (_mainThreadID == int.MinValue)
            {
                _mainThreadID = Thread.CurrentThread.ManagedThreadId;
            }
        }

        public static void AttachCurrentThread()
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            if (_mainThreadID != int.MinValue && Thread.CurrentThread.ManagedThreadId != _mainThreadID)
            {
                int count;
                if (!thread2AttachCount.TryGetValue(Thread.CurrentThread.ManagedThreadId, out count) || count <= 0)
                {
                    thread2AttachCount[Thread.CurrentThread.ManagedThreadId] = 1;
                    AndroidJNI.AttachCurrentThread();
                }
                else
                {
                    count++;
                    thread2AttachCount[Thread.CurrentThread.ManagedThreadId] = count;
                }
            }
#endif
        }

        public static void DetachCurrentThread()
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            if (_mainThreadID != int.MinValue && Thread.CurrentThread.ManagedThreadId != _mainThreadID)
            {
                int count;
                if (thread2AttachCount.TryGetValue(Thread.CurrentThread.ManagedThreadId, out count))
                {
                    count--;
                    thread2AttachCount[Thread.CurrentThread.ManagedThreadId] = count;
                    if (count <= 0)
                    {
                        int v;
                        thread2AttachCount.TryRemove(Thread.CurrentThread.ManagedThreadId, out v);
                        AndroidJNI.DetachCurrentThread();
                    }
                }
            }
#endif
        }
    }

}