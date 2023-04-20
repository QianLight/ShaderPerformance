using System;
using System.Threading;

namespace UNBridgeLib
{
    public class TaskDispatch
    {
        private static int MainThreadId = Thread.CurrentThread.ManagedThreadId;
        private static SynchronizationContext MainThreadSynContext = SynchronizationContext.Current ?? new SynchronizationContext();


        public static void RunOnMainThread(Action action)
        {
            if (Thread.CurrentThread.ManagedThreadId == MainThreadId)
            {
                LogUtils.D("Origin Task run on main thread:" , MainThreadId.ToString());
                if (action != null)
                {
                    action.Invoke();
                }
            }
            else
            {
                LogUtils.D("Origin Task run on sub thread:" , Thread.CurrentThread.ManagedThreadId.ToString());
                MainThreadSynContext.Post(new SendOrPostCallback((s) =>
                {
                    if (action != null)
                    {
                        action.Invoke();
                    }
                }), null);
            }
        }
    }
}
