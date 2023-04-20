/// <summary>
/// copy form UniRx
/// </summary>

using System;
using System.Collections.Generic;
using System.Text;

namespace Blueprint
{
    public interface ISubject<TSource, TResult> : IObserver<TSource>, IObservable<TResult>
    {
    }

    public interface ISubject<T> : ISubject<T, T>, IObserver<T>, IObservable<T>
    {
    }
}