/*******************************************************************
* Copyright © 2017—2021 Perfect World.Co.Ltd. All rights reserved.
* author: 移动项目支持部 ZEUS FRAMEWORK TEAM
********************************************************************/
using UnityEngine;
using Zeus.Core;

namespace Zeus.Framework.Asset
{
    public sealed class AsyncAwaitOperationAwaiter<T> : Awaiter where T : AsyncAwaitOperation
    {
        private AsyncAwaitOperationAwaiter(T operation) : base(operation.GetHashCode())
        {
            m_Continuation = null;
            m_Result = operation;
            m_IsCompleted = operation.IsDone;
            operation.Completed += _OnComplete;
        }

        private readonly T m_Result;

        public T Result
        {
            get
            {
                return m_Result;
            }
        }

        public T GetResult()
        {
            return Result;
        }

        private void _OnComplete()
        {
            m_IsCompleted = true;
            m_Continuation?.Invoke();
        }

        public static AsyncAwaitOperationAwaiter<T> GetAwaiter(T operation)
        {
            if (operation == null)
                return null;

            AsyncAwaitOperationAwaiter<T> result;
            int key = operation.GetHashCode();

            //  如果已存在实例
            if (cache.TryGetValue(key, out var awaiter))
            {
                //  如果是该类型实例
                if (awaiter is AsyncAwaitOperationAwaiter<T>)
                {
                    result = awaiter as AsyncAwaitOperationAwaiter<T>;
                    //  如果是相同key值实例
                    if (key == result.m_Result.GetHashCode())
                    {
                        return result;
                    }
                    //  否则移除
                    cache.Remove(key);
                }
            }
            //  新建实例
            result = new AsyncAwaitOperationAwaiter<T>(operation);
            cache[key] = result;
            return result;
        }
    }
}
