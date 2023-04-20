/*******************************************************************
* Copyright © 2017—2021 Perfect World.Co.Ltd. All rights reserved.
* author: 移动项目支持部 ZEUS FRAMEWORK TEAM
********************************************************************/
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Zeus.Framework
{
    public abstract class Task
    {
        protected bool _isDone = false;
        public string Tag { get; set; }
        public virtual bool IsDone { get { return _isDone; } set { _isDone = value; } }
        public Request Request { get; set; }
    }

    public class TaskAsync : IEnumerator
    {
        public Task Task { get; set; }

        public TaskAsync(Task task)
        {
            Task = task;
        }

        public object Current
        {
            get { return null; }
        }


        public bool MoveNext()
        {
            if (Task == null)
                return true;

            return !Task.IsDone;
        }

        public void Reset()
        {
            
        }
    }

    public abstract class Request : IEnumerable<Task>
    {
        protected List<Task> _tasks;
        protected Dictionary<string, int> _tagIndexTable;
        public virtual bool IsDone
        {
            get { return ExcutedCount() >= TaskCount(); }
        }

        public  bool IsCancel { get; set; }

        public delegate void OnAllExcuted(Request req);
        public OnAllExcuted onAllExcuted { get; set; }

        public Request()
        {
            _tasks = new List<Task>();
            _tagIndexTable = new Dictionary<string, int>();
        }

        public virtual void AddTask(Task task)
        {
            if (_tagIndexTable.ContainsKey(task.Tag))
            {
                Debug.LogError("Same Tag already exist: " + task.Tag);
            }
            _tasks.Add(task);
            task.Request = this;
            _tagIndexTable[task.Tag] = _tasks.Count - 1;
        }

        public virtual Task GetTaskByTag(string tag)
        {
            int index;
            if (!_tagIndexTable.TryGetValue(tag, out index))
            {
                return null;
            }

            return _tasks[index];
        }

        public virtual void Cancle()
        {
            IsCancel = true;
        }

        public virtual int TaskCount()
        {
            return _tasks.Count;
        }

        public virtual int ExcutedCount()
        {
            int c = 0;
            foreach (var task in _tasks)
            {
                c += task.IsDone ? 1 : 0;
            }
            return c;
        }

        public virtual IEnumerator<Task> GetEnumerator()
        {
            return _tasks.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public virtual void CallAllExcuted()
        {
            if (onAllExcuted != null)
                onAllExcuted(this);
        }

        public virtual void Clear()
        {
            _tasks.Clear();
        }
    }


    //轮询Request 是否完成.
    public class RequestAsync : IEnumerator
    {

        public Request Req { get; set; }

        public RequestAsync(Request req)
        {
            Req = req;
        }

        public virtual object Current
        {
            get { return null; }
        }

        public virtual bool MoveNext()
        {
            if (Req == null)
                return true;

            return !Req.IsDone && !Req.IsCancel;
        }

        public virtual void Reset()
        {
            
        }
    }

}