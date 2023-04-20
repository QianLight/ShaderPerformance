using System;

namespace GSDK.RNU
{

    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public class ReactMethod : Attribute
    {
        //TODO 可根据方法的最后一个参数 判断出是否是Promise
        public bool isPromise = false;
        public ReactMethod(bool isPromise)
        {
            this.isPromise = isPromise;
        }

        public ReactMethod()
        {
            
        }
    }
}