/*******************************************************************
* Copyright © 2017—2021 Perfect World.Co.Ltd. All rights reserved.
* author: 移动项目支持部 ZEUS FRAMEWORK TEAM
********************************************************************/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class PriorityQueue<T> : IEnumerable<T>
{
    private List<T> _content;
    private bool _isDirty;

    public T Dequeue()
    {
        if (_isDirty)
        {
            _content.Sort();
            _isDirty = false;
        }
        if (_content.Count != 0)
        {
            T item = _content[_content.Count - 1];
            _content.RemoveAt(_content.Count - 1);
            return item;
        }
        else
        {
            return default(T);
        }
    }

    public T Peek()
    {
        if (_isDirty)
        {
            _content.Sort();
            _isDirty = false;
        }
        if (_content.Count != 0)
        {
            T item = _content[_content.Count - 1];
            return item;
        }
        else
        {
            return default(T);
        }
    }

    public void Enqueue(T item)
    {
        _isDirty = true;
        _content.Add(item);
    }

    public int Count { get { return _content.Count; } }


    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
    {
        throw new NotImplementedException();
    }

    public IEnumerator<T> GetEnumerator()
    {
        foreach (T item in _content)
            yield return item;
    }

    public void Clear()
    {
        _isDirty = false;
        _content.Clear();
    }
    
}
