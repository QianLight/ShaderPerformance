/*******************************************************************
* Copyright © 2017—2021 Perfect World.Co.Ltd. All rights reserved.
* author: 移动项目支持部 ZEUS FRAMEWORK TEAM
********************************************************************/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;


namespace Zeus.Framework
{

    public interface IExcutingQueue<R,T> where R:Request where T:Task
    {
        void AddRequest(R req);
        IEnumerator Excute();
        void SetTaskExcutor(Action<T> excutor);
        void SetTaskAsyncExcutor(Action<T> excutor);
        void SetRequestExcuted(Action<R> onExcuted);
        void ExcuteTask(T task);
        void ExcuteTaskAsync(T task);
        void OnRequestExcuted(R req);
    }

    public abstract class AbsExcutingQueue<R,T> : IExcutingQueue<R,T> where R : Request where T : Task
    {
        protected Queue<R> _excutingQueue;

        protected AbsExcutingQueue()
        {
            _excutingQueue = new Queue<R>();
        }

        public Queue<R> GetQueue()
        {
            return _excutingQueue;
        }

        public void AddRequest(R req)
        {
            _excutingQueue.Enqueue(req);
        }

        public abstract IEnumerator Excute();

        private Action<T> _excutor;
        private Action<T> _asyncExcutor;
        private Action<R> _onExcuted;

        public void ExcuteTask(T task)
        {
            this._excutor(task);
        }

        public void ExcuteTaskAsync(T task)
        {
            this._asyncExcutor(task);
        }

        public void OnRequestExcuted(R req)
        {
            this._onExcuted(req);
        }

        public void SetRequestExcuted(Action<R> onExcuted)
        {
            this._onExcuted = onExcuted;
        }

        public void SetTaskAsyncExcutor(Action<T> excutor)
        {
            this._asyncExcutor = excutor;
        }

        public void SetTaskExcutor(Action<T> excutor)
        {
            this._excutor = excutor;
        }
    }

    //同步执行队列,每个任务都是同步执行
    public class ExcutingQueue<R,T> : AbsExcutingQueue<R,T> where R : Request where T : Task
    {
        public override IEnumerator Excute()
        {
            while (_excutingQueue.Count > 0)
            {
                R req = _excutingQueue.Dequeue();
                if (req.IsDone || req.IsCancel)
                {
                    continue;
                }

                foreach(T task in req)
                {
                    ExcuteTask(task);
                }
                OnRequestExcuted(req);
                yield return null;
            }
        }
    }

    //异步执行,整个Request的所有任务一次性提交,全部任务执行完毕，再执行下一个Request
    public class AsyncExcutingQueue<R, T> : AbsExcutingQueue<R, T> where R : Request where T : Task
    {
        public override IEnumerator Excute()
        {
            while(_excutingQueue.Count > 0)
            {
                R req = _excutingQueue.Dequeue();
                if(req.IsDone || req.IsCancel)
                {
                    continue;
                }

                foreach(T task in req)
                {
                    ExcuteTaskAsync(task);
                }
                yield return new RequestAsync(req);
                OnRequestExcuted(req);
            }
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="R"></typeparam>
    /// <typeparam name="T"></typeparam>
    public class AsyncExcutingQueueSeq<R, T> : AbsExcutingQueue<R, T> where R : Request where T : Task
    {
        public override IEnumerator Excute()
        {
            while(_excutingQueue.Count>0)
            {
                R req = _excutingQueue.Dequeue();
                if(req.IsDone || req.IsCancel)
                {
                    continue;
                }

                foreach(T task in req)
                {
                    ExcuteTaskAsync(task);
                    yield return new TaskAsync(task);
                    if (req.IsCancel)
                        break;
                }

                OnRequestExcuted(req);
            }
        }
    }
}



