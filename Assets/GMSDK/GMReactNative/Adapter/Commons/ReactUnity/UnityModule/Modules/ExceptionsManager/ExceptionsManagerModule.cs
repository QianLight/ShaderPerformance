/*
 * @author yankang.nj
 * 处理JS测的异常，JS测发生异常的时候，会通过此bridge传递到这里
 *
 * JS测的callFunctionReturnFlushedQueue/invokeCallbackAndReturnFlushedQueue 调用 都是被__guard 保护的， 其内部函数调用异常的时候，
 * 都是会走到 ErrorUtils.reportFatalError(); 而这个函数默认是调用 ExceptionsManager.handleException(e, true)。 即对应这里的reportFatalException
 *
 * 其他React相关的一些异常 调用的是 ExceptionsManager.handleException(e, false); 即对应这里的reportSoftException
 *
 */

using System.Collections;
using System.Linq;
using UnityEngine;

namespace GSDK.RNU
{
    public class ExceptionsManagerModule : SimpleUnityModule
    {
        public static string NAME = "ExceptionsManager";

        private RNUMainCore context;

        public override string GetName()
        {
            return NAME;
        }

        public ExceptionsManagerModule(RNUMainCore rnuContext)
        {
            context = rnuContext;
        }

        /*
         * FatalException 统一需要关闭引擎。 以免活动页面异常 产生游戏的异常
         */
        [ReactMethod]
        public void reportFatalException(string message, string stacks, int exceptionId)
        {
            if (context.IsDebugMode()) 
            {
                // 开发模式不执行关闭
                return;
            }
            Util.LogAndReport("reportFatalException: {0}", message);

            InfoAndErrorReporter.ReportJSException(message, "");
            InfoAndErrorReporter.ReportMessageToUnity(InfoAndErrorReporter.JSEvalError, "-1", "0");
            RNUMainCore.SetCloseFlag();
        }

        [ReactMethod]
        public void reportSoftException(string message, string stacks, int exceptionId)
        {
            if (context.IsDebugMode())
            {
                // 开发模式不执行关闭
                return;
            }
            Util.LogAndReport("reportSoftException: {0}", message);
            //TODO
        }

        [ReactMethod]
        public void updateExceptionMessage(string title, ArrayList details, int exceptionId)
        {
            // no-op
        }

        [ReactMethod]
        public void dismissRedbox()
        {
            context.debugPanel.HideError();
        }
    }
}
