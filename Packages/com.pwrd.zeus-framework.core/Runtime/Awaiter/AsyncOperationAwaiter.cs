/*******************************************************************
* Copyright © 2017—2021 Perfect World.Co.Ltd. All rights reserved.
* author: 移动项目支持部 ZEUS FRAMEWORK TEAM
********************************************************************/
using UnityEngine;
namespace Zeus.Core
{
    public sealed class AsyncOperationAwaiter<T> : Awaiter where T : AsyncOperation
    {
        private AsyncOperationAwaiter(T operation) : base(operation.GetHashCode())
        {
            m_Continuation = null;
            m_Result = operation;
            m_IsCompleted = operation.isDone;
            operation.completed += _OnComplete;
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

        public AsyncOperationAwaiter<T> GetAwaiter()
        {
            return this;
        }

        private void _OnComplete(AsyncOperation operation)
        {
            m_IsCompleted = true;
            m_Continuation?.Invoke();
        }

        public static AsyncOperationAwaiter<T> GetAwaiter(T operation)
        {
            if (operation == null)
                return null;

            AsyncOperationAwaiter<T> result;
            int key = operation.GetHashCode();

            //  如果已存在实例
            if (cache.TryGetValue(key, out var awaiter))
            {
                //  如果是该类型实例
                if (awaiter is AsyncOperationAwaiter<T>)
                {
                    result = awaiter as AsyncOperationAwaiter<T>;
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
            result = new AsyncOperationAwaiter<T>(operation);
            cache[key] = result;
            return result;
        }
    }
}
