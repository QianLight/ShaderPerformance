using System.Collections;

namespace GSDK.RNU
{
    public interface JSExecutor
    {
        /**
        * Load javascript into the js context
        * @param sourceURL url or file location from which script content was loaded
        */
         void LoadApplicationScript(string uri);

        /**
        * Execute javascript method within js context
        * @param methodName name of the method to be executed
        * @param jsonArgsArray json encoded array of arguments provided for the method call
        * @return json encoded value returned from the method call
        */
        void CallFunction(string moduleName, string methodName, ArrayList args);

        /**
         * 负责处理异步/callback的情况，当JS测调用一个异步的C#方法的时候， JS测会负责把方法的callback存储在一个结构里，并给出唯一callID。
         * C#测负责在异步方法完成之后，调用InvokeCallback 方法，（参数为callID）
         *
         * JS测在接收到InvokeCallback的响应之后， 从callID恢复出回调函数，并执行。
         *
         * 常见的异步函数有 网络，定时等
         */
        void InvokeCallback(int callID, ArrayList args);


        void SetGlobalVariable(string propertyName, Hashtable json);

        void Destroy();
    }
}