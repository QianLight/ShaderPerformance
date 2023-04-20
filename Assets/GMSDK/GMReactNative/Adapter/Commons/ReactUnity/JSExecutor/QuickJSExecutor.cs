using System;
using System.Collections;
using UnityEngine;

namespace GSDK.RNU
{
    public class QuickJSExecutor : JSExecutor
    {
        public QuickJSExecutor() {
            RNUNative.InitRuntime();
        }

        public void LoadApplicationScript(string uri)
        {
            int result = RNUNative.LoadApplicationScriptBytecode(uri);

            if (result == -1)
            {
                throw new CodeFileLoadException();
            }

            if (result == -2)
            {
                throw new CodeFileFormatException();
            }

            if (result == -3)
            {
                throw new CodeFileEvalException();
            }
        }

        public void CallFunction(string moduleName, string methodName, ArrayList args)
        {
            long start = DateTime.Now.Ticks;
            IntPtr argsInt = UnmanageJSDataOp.GeneJSArray(args);
            RNUNative.CallFunction(moduleName, methodName, argsInt);
            Util.Log("yk-duration CallFunction QuickJS: {0}", DateTime.Now.Ticks - start);

            // 每一次CallFunction 都有可能触发了 关闭引擎的操作。 需要在CallFunction调用结束之后，才能去回收相关的Quickjs 资源
            RNUMainCore.CloseIfNecessary();
        }

        public void InvokeCallback(int callID, ArrayList args)
        {
            long start = DateTime.Now.Ticks;
            IntPtr argsInt = UnmanageJSDataOp.GeneJSArray(args);
            RNUNative.InvokeCallback(callID, argsInt);
            
            Util.Log("yk-duration InvokeCallback QuickJS: {0}", DateTime.Now.Ticks - start);

            //每一次 InvokeCallback 都有可能触发了 关闭引擎的操作。 需要在InvokeCallback调用结束之后，去回收相关的Quickjs 资源
            RNUMainCore.CloseIfNecessary();
        }

        public void SetGlobalVariable(string propertyName, Hashtable obj)
        {

            IntPtr argsInt = UnmanageJSDataOp.GeneJSObject(obj);
            RNUNative.SetGlobalVariable(propertyName, argsInt);
        }

        public void Destroy() {
            RNUNative.FreeRuntime();
        }

    }
}
