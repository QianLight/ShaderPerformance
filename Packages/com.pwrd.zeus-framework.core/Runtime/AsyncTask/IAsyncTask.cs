/*******************************************************************
* Copyright © 2017—2021 Perfect World.Co.Ltd. All rights reserved.
* author: 移动项目支持部 ZEUS FRAMEWORK TEAM
********************************************************************/
using System.Collections.Generic;

namespace Zeus
{
    public enum AsyncState
    {
        Before,
        Doing,
        After,
        Done
    }

    public interface IInternalMessage
    {
        int GetMessageId();
    }

    public class AsyncTaskMessage : IInternalMessage
    {
        //临时定义，标识异步在消息队列中的ID
        public const int AsyncMessageId = 100;

        public AsyncState State { get; set; }

        public IAsyncTask AsyncTask { get; set; }

        public int GetMessageId()
        {
            return AsyncMessageId;
        }
    }


    /// <summary>
    /// System message queue (Thread safe)
    /// </summary>
    public class SystemMessageQueue : Singleton<SystemMessageQueue>
    {
        private Queue<IInternalMessage> _messageQueue = new Queue<IInternalMessage>();

        /// Add a new message to the queue.
        public void Offer(IInternalMessage internalMessage)
        {
            lock (this)
            {
                _messageQueue.Enqueue(internalMessage);
            }
        }

        /// Poll message from the queue.
        public IInternalMessage Poll()
        {
            lock (this)
            {
                if (_messageQueue.Count == 0)
                {
                    return null;
                }
                return _messageQueue.Dequeue();
            }
        }

        public int GetCount()
        {
            return _messageQueue.Count;
        }
    }


    public interface IAsyncTask
    {
        /// Called by main thread.
        AsyncState BeforeAsyncTask();


        /// Call in a new thread.
        AsyncState DoAsyncTask();


        /// After job done, call in main thread.
        AsyncState AfterAsyncTask();
    }
}
