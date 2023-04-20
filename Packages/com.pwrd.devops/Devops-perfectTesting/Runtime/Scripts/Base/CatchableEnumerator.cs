using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class CatchableEnumerator : IEnumerator
{
    private IEnumerator enumerator;

    private Action<Exception> exceptionCallback;

    private bool subExceptionCatched = false;

    private Exception exception;
    /// <summary>
    /// ctor
    /// </summary>
    /// <param name="enumerator">迭代器</param>
    /// <param name="exceptionCallback">异常Callback</param>
    public CatchableEnumerator(IEnumerator enumerator, Action<Exception> exceptionCallback = null)
    {
        this.enumerator = enumerator;
        this.exceptionCallback = exceptionCallback;
    }

    /// <summary>
    /// 绑定异常Callback
    /// </summary>
    /// <param name="exceptionCallback"></param>
    public void BindExceptionCallback(Action<Exception> exceptionCallback)
    {
        this.exceptionCallback += exceptionCallback;
    }

    public object Current 
    {
        get
        {
            if(enumerator.Current != null )
            {
                if(enumerator.Current is CatchableEnumerator caRator )
                {
                    caRator.BindExceptionCallback(SubExceptionCall);
                }
                else if(enumerator.Current is IEnumerator ieRator)
                {
                    return new CatchableEnumerator(ieRator, this.exceptionCallback);
                }
            }
            return enumerator.Current;
        }
    }

    public bool MoveNext()
    {
        if( subExceptionCatched )
        {
            exceptionCallback?.Invoke(exception);
            return false;
        }

        bool result = false;

        try
        {
            result = enumerator.MoveNext();
        }
        catch(Exception e)
        {
            var stacktrace = new System.Diagnostics.StackTrace(true);
            Debug.Log(stacktrace);
            result = false;
            exception = e;
            exceptionCallback?.Invoke(exception);
        }

        return result;
    }

    /// <summary>
    /// 子迭代器异常回调
    /// </summary>
    private void SubExceptionCall(Exception e)
    {
        subExceptionCatched = true;
    }

    public void Reset()
    {
        enumerator.Reset();
    }
}