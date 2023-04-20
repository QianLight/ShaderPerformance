using System;
using System.Collections;
using System.Threading;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace com.pwrd.hlod.editor
{
    public static class UnityTaskUtils
    {
        private static int mainThreadId = -1;
        private static int m_taskIDCounter = 0;
        private static bool m_showLog = false;
        private static bool m_hasInit = false;

        private static bool IsMainThread()
        {
            Log(string.Format(" mainThreadId:{0} cur:{1}", mainThreadId, Thread.CurrentThread.ManagedThreadId));
            return false;
            //todo
            //这种方法会出现非主线程判断为主线程的情况;
            // return Thread.CurrentThread.ManagedThreadId == mainThreadId;
        }

        private static void InitMainThreadID()
        {
            if (m_hasInit)
            {
                return;
            }

            m_hasInit = true;
            EditorApplication.update += () =>
            {
                var threadId = Thread.CurrentThread.ManagedThreadId;
                if (mainThreadId != threadId)
                {
                    mainThreadId = threadId;
                }
            };
        }

        private static void Log(string info)
        {
            if (m_showLog)
            {
                HLODDebug.LogWarning("[UnityTaskUtils] " + info);
            }
        }

        /// <summary>
        /// 不能接受一步等待的action
        /// 即不能使用async关键字
        /// </summary>
        /// <param name="action"></param>
        /// <returns></returns>
        public static Task EditorMainThread(Action action, string name = "")
        {
            InitMainThreadID();
            if (IsMainThread())
            {
                return Task.Run(action);
            }

            return Task.Run(() =>
            {
                bool hasDone = false;
                int id = m_taskIDCounter;
                Log(string.Format(" name:{1}  taskid:{0} threadID{2} start", id, name,
                    Thread.CurrentThread.ManagedThreadId));

                m_taskIDCounter++;
                EditorApplication.CallbackFunction delayCall = () =>
                {
                    try
                    {
                        if (!hasDone && action != null)
                            action();
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                        throw;
                    }
                    finally
                    {
                        hasDone = true;
                        int theId = id;
                        Log(string.Format(" name:{1}  taskid:{0} threadID{2} run", id, name,
                            Thread.CurrentThread.ManagedThreadId));
                    }
                };
                EditorApplication.update += delayCall;

                while (!hasDone)
                {
                    Thread.Sleep(1);
                }

                Log(string.Format(" name:{1}  taskid:{0} threadID{2} end", id, name,
                    Thread.CurrentThread.ManagedThreadId));
                EditorApplication.update -= delayCall;
            });
        }

        /// <summary>
        /// 不能接受异步等待的函数
        /// 即不能使用async关键字
        /// </summary>
        /// <param name="func"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static Task<T> EditorMainThread<T>(Func<T> func, string name = "")
        {
            InitMainThreadID();

            if (IsMainThread())
            {
                return Task.Run(func);
            }

            return Task.Run(() =>
            {
                int id = m_taskIDCounter;
                Log(string.Format(" name:{1}  taskid:{0} start", id, name));
                m_taskIDCounter++;
                bool hasDone = false;
                T result = default(T);
                EditorApplication.CallbackFunction delayCall = () =>
                {
                    try
                    {
                        if (!hasDone && func != null)
                            result = func();
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                        throw;
                    }
                    finally
                    {
                        hasDone = true;
                        int theId = id;
                        Log(string.Format(" name:{1}  taskid:{0} run", id, name));
                    }
                };

                EditorApplication.update += delayCall;

                while (!hasDone)
                {
                    Thread.Sleep(1);
                }

                Log(string.Format(" name:{1}  taskid:{0} end", id, name));
                EditorApplication.update -= delayCall;

                return result;
            });
        }

        public static Task EndCameraRendering(this Camera cam, Action action)
        {
            return Task.Run(() =>
            {
                bool hasDone = false;
                Action<ScriptableRenderContext, Camera> delayCall = (context, camera) =>
                {
                    try
                    {
                        if (!hasDone && action != null && camera == cam)
                            action();
                    }
                    catch (Exception e)
                    {
                        Log(e + e.StackTrace);
                        throw;
                    }
                    finally
                    {
                        hasDone = true;
                    }
                };
                RenderPipelineManager.endCameraRendering += delayCall;

                while (!hasDone)
                {
                    Thread.Sleep(1);
                }

                RenderPipelineManager.endCameraRendering -= delayCall;
            });
        }

        public static Task<T> EndCameraRendering<T>(this Camera cam, Func<T> func)
        {
            return Task.Run(() =>
            {
                bool hasDone = false;
                T value = default(T);
                Action<ScriptableRenderContext, Camera> delayCall = (context, camera) =>
                {
                    try
                    {
                        if (!hasDone && func != null && camera == cam)
                            value = func();
                    }
                    catch (Exception e)
                    {
                        Log(e + e.StackTrace);
                        throw;
                    }
                    finally
                    {
                        hasDone = true;
                    }
                };
                RenderPipelineManager.endCameraRendering += delayCall;

                while (!hasDone)
                {
                    Thread.Sleep(1);
                }

                RenderPipelineManager.endCameraRendering -= delayCall;

                return value;
            });
        }


        public static void StartTask(IEnumerator itor)
        {
            EditorTaskProxy.Instance.StartTask(itor);
        }
    }
}