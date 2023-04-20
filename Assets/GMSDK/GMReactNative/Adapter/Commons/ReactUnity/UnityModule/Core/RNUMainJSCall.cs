/*
 * @Author: hexiaonuo
 * @Date: 2021-10-15
 * @Description: Game init, about js call
 * @FilePath: ReactUnity/UnityModule/Core/RNUMainJSCall.cs
 */


using System;
using System.Collections;

namespace GSDK.RNU
{
    partial class RNUMainCore
    {
        private JSExecutor jsExecutor;

        private static string UnityEventEmitterModuleName = "RCTDeviceEventEmitter";
        private static string UnityEventEmitterMethodName = "emit";

        public static void CallJSFunction(string moduleName, string methodName, ArrayList args) {
            if (mainCoreInstance == null || mainCoreInstance.jsExecutor == null || mainCoreInstance.closeFlag)
            {
                Util.Log("page has closed! when invoke CallJSFunction {0} {1}", moduleName, methodName);
                return;
            }
            mainCoreInstance.jsExecutor.CallFunction(moduleName, methodName, args);
        }

        public static void InvokeJSCallback(int callID, ArrayList args)
        {
            if (mainCoreInstance == null || mainCoreInstance.jsExecutor == null || mainCoreInstance.closeFlag)
            {
                Util.Log("page has closed! when invoke InvokeJSCallback");
                return;
            }
            mainCoreInstance.jsExecutor.InvokeCallback(callID, args);
        }

        public static void CallNativeModule(int moduleId, int methodId, ArrayList args)
        {
            // 已经接收到关闭指令，不再执行任何实际操作
            if (mainCoreInstance == null || mainCoreInstance.closeFlag) {
                return;
            }

            try
            {
                ModuleManager mm = mainCoreInstance.moduleManager;
                if (methodId == -1 && moduleId == -1) {
                    // endOfBatch
                    mm.CallEndOfBatch();
                    return;
                }
                
                mm.CallNativeModule(moduleId, methodId, args);
            }
            catch (Exception e)
            {
                /*
                 * CallJSFunction/InvokeJSCallback 的执行一旦发生异常，将执行关闭引擎操作，以免错误和不一致对页面/游戏 产生影响，
                 *
                 * 这样同时也就要求内部的操作如果发生的是无关紧要的异常，需要自己捕获，如设置属性异常等， 以免在这里捕获导致引擎关闭
                 *
                 * 此外JS测执行的异常，不会在这里抛出， JS测的异常有自己的捕获逻辑
                 */
                if (mainCoreInstance.IsDebugMode())
                {
                    mainCoreInstance.debugPanel.ShowError("CallNativeModule moduleId: " + moduleId + " methodId:" + methodId + "\n" + e.StackTrace);
                    return;
                }
                Util.LogAndReport("CallNativeModule moduleId: {0} methodId: {1} {2}", moduleId, methodId, e.StackTrace);

                InfoAndErrorReporter.ReportNativeModuleInvokeException(moduleId, methodId, e);
                InfoAndErrorReporter.ReportMessageToUnity(InfoAndErrorReporter.RNUInnerError, "-1", "0");
                SetCloseFlag();
            }
        }


        public static void SendUnityEventToJs(string eventName, string jsonMessage)
        {
            if (mainCoreInstance == null || mainCoreInstance.closeFlag)
            {
                // 无page打开， 不发送消息
                return;
            }


            // 实际内部也是通过CallJSFunction调用来实现发消息
            CallJSFunction(UnityEventEmitterModuleName, UnityEventEmitterMethodName, new ArrayList()
            {
                eventName,
                jsonMessage
            });
        }
    }
}