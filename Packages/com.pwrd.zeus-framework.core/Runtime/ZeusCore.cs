/*******************************************************************
* Copyright © 2017—2021 Perfect World.Co.Ltd. All rights reserved.
* author: 移动项目支持部 ZEUS FRAMEWORK TEAM
********************************************************************/
using UnityEngine;
using System;
using System.Collections.Generic;
using System.Collections;
using System.Diagnostics;
using System.Collections.Concurrent;

namespace Zeus.Core
{
    public class ZeusCore : MonoBehaviour
    {
        #region Static

        public static ZeusCore Instance
        {
            get
            {
                return _CreateInstance();
            }
        }

        internal static bool IsPlaying
        {
            get
            {
#if UNITY_EDITOR
                return s_IsPlaying;
#else
                return true;
#endif
            }
        }

        private static ZeusCore s_Instance;
#if UNITY_EDITOR
        private static bool s_IsPlaying = false;
#endif

        private static volatile bool s_IsApplicationQuit = false;

        [Conditional("ZEUS_LOG")]
        public static void Log(object msg)
        {
            UnityEngine.Debug.Log(string.Format("#Zeus# {0}", msg));
        }

        [Conditional("ZEUS_LOG")]
        public static void Log(string tag, object msg)
        {
            Log(string.Format("[{0}] {1}", tag, msg));
        }

        public static bool IsApplicationQuit { get { return s_IsApplicationQuit; } }

#if UNITY_EDITOR
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void _InitOnLoad()
        {
            _CreateInstance();

            s_IsPlaying = true;

            s_IsApplicationQuit = false;
        }
#endif

        private static ZeusCore _CreateInstance()
        {
            if (s_Instance == null)
            {
#if UNITY_EDITOR
                var go = GameObject.Find("ZeusCore");
                if (go != null)
                {
                    if(Application.isPlaying)
                    {
                        DontDestroyOnLoad(go);
                    }
                    s_Instance = go.GetComponent<ZeusCore>();
                }
                else
                {
                    go = new GameObject("ZeusCore");
                    if(Application.isPlaying)
                    {
                        DontDestroyOnLoad(go);
                    }
                    s_Instance = go.AddComponent<ZeusCore>();
                }
#else
                if (s_IsApplicationQuit)
                {
                    throw new Exception("Application Is Quit, Can Not CreateInstance");
                }
                GameObject go = new GameObject("ZeusCore");
                DontDestroyOnLoad(go);
                s_Instance = go.AddComponent<ZeusCore>();
#endif
            }
            return s_Instance;
        }

        #endregion

        private List<Action> _ListOnUpdate = new List<Action>();
        private List<Action> _ListOnLateUpdate = new List<Action>();
        private List<Action> _ListOnFixedUpdate = new List<Action>();
        private List<Action> _ListOnApplicationQuit = new List<Action>();
        private List<Action<bool>> _ListOnApplicationPause = new List<Action<bool>>();
        private List<Action<bool>> _ListOnApplicationFocus = new List<Action<bool>>();
        private ConcurrentQueue<Action> _MainThreadTaskQueue = new ConcurrentQueue<Action>();
        private ConcurrentQueue<Action1Data> _mainThreadAction1Queue = new ConcurrentQueue<Action1Data>();

        private class Action1Data
        {
            Action<object> action;
            object param;

            public Action1Data(Action<object> action, object param)
            {
                this.action = action;
                this.param = param;
            }

            public void ReSet(Action<object> action, object param)
            {
                this.action = action;
                this.param = param;
            }

            public void Execute()
            {
                action(param);
            }
        }

        private ZeusCore() { }

        public void RegisterUpdate(Action callback)
        {
            _RegisterEvent(_ListOnUpdate, callback);
        }

        public void UnRegisterUpdate(Action callback)
        {
            _UnRegisterEvent(_ListOnUpdate, callback);
        }

        public void RegisterLateUpdate(Action callback)
        {
            _RegisterEvent(_ListOnLateUpdate, callback);
        }

        public void UnRegisterLateUpdate(Action callback)
        {
            _UnRegisterEvent(_ListOnLateUpdate, callback);
        }

        public void RegisterFixedUpdate(Action callback)
        {
            _RegisterEvent(_ListOnFixedUpdate, callback);
        }

        public void UnRegisterFixedUpdate(Action callback)
        {
            _UnRegisterEvent(_ListOnFixedUpdate, callback);
        }

        public void RegisterOnApplicationQuit(Action callback)
        {
            _RegisterEvent(_ListOnApplicationQuit, callback);
        }

        public void UnRegisterOnApplicationQuit(Action callback)
        {
            _UnRegisterEvent(_ListOnApplicationQuit, callback);
        }

        public void RegisterOnApplicationPause(Action<bool> callback)
        {
            _RegisterEvent(_ListOnApplicationPause, callback);
        }

        public void UnRegisterOnApplicationPause(Action<bool> callback)
        {
            _UnRegisterEvent(_ListOnApplicationPause, callback);
        }

        public void RegisterOnApplicationFocus(Action<bool> callback)
        {
            _RegisterEvent(_ListOnApplicationFocus, callback);
        }
        public void UnRegisterOnApplicationFocus(Action<bool> callback)
        {
            _UnRegisterEvent(_ListOnApplicationFocus, callback);
        }

        public void AddMainThreadTask(Action action)
        {
            _MainThreadTaskQueue.Enqueue(action);
        }

        public void AddMainThreadTask(Action<object> action, object param)
        {
            _mainThreadAction1Queue.Enqueue(new Action1Data(action, param));
        }

        private void Update()
        {
            for (int i = 0; i < _ListOnUpdate.Count; ++i)
            {
                _ListOnUpdate[i]();
            }

            while (_MainThreadTaskQueue.Count > 0)
            {
                Action action;
                if (_MainThreadTaskQueue.TryDequeue(out action))
                {
                    try
                    {
                        action();
                    }
                    catch(Exception ex)
                    {
                        UnityEngine.Debug.LogException(ex);
                    }
                }
                else
                {
                    break;
                }
            }

            while (_mainThreadAction1Queue.Count > 0)
            {
                Action1Data actionData;
                if (_mainThreadAction1Queue.TryDequeue(out actionData))
                {
                    try
                    {
                        actionData.Execute();
                    }
                    catch (Exception ex)
                    {
                        UnityEngine.Debug.LogException(ex);
                    }
                }
                else
                {
                    break;
                }
            }
        }

        private void LateUpdate()
        {
            for (int i = 0; i < _ListOnLateUpdate.Count; ++i)
            {
                _ListOnLateUpdate[i]();
            }
        }

        private void FixedUpdate()
        {
            for (int i = 0; i < _ListOnFixedUpdate.Count; i++)
            {
                _ListOnFixedUpdate[i]();
            }
        }

        private void OnApplicationQuit()
        {
            try
            {
                for (int i = 0; i < _ListOnApplicationQuit.Count; ++i)
                {
                    _ListOnApplicationQuit[i]();
                }
            }
            catch (Exception e)
            {
                UnityEngine.Debug.LogException(e);
                throw e;
            }
            finally
            {
#if UNITY_EDITOR
                s_IsPlaying = false;
#endif
                s_IsApplicationQuit = true;

                s_Instance = null;
            }
        }

        private void OnApplicationFocus(bool hasFocus)
        {
            for (int i = 0; i < _ListOnApplicationFocus.Count; i++)
            {
                _ListOnApplicationFocus[i](hasFocus);
            }
        }

        private void OnApplicationPause(bool pause)
        {
            for (int i = 0; i < _ListOnApplicationPause.Count; i++)
            {
                _ListOnApplicationPause[i](pause);
            }
        }

        private void _RegisterEvent(IList list, Delegate dele)
        {
            for (int i = list.Count - 1; i >= 0; --i)
            {
                if ((Delegate)list[i] == dele)
                {
                    throw new RepeatedRegisterException();
                }
            }

            list.Add(dele);
        }

        private void _UnRegisterEvent(IList list, Delegate dele)
        {
            list.Remove(dele);
        }
    }

    public class RepeatedRegisterException : ZeusException { }
}
