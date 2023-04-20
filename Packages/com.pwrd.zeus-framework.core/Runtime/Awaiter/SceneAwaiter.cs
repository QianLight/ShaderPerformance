/*******************************************************************
* Copyright © 2017—2021 Perfect World.Co.Ltd. All rights reserved.
* author: 移动项目支持部 ZEUS FRAMEWORK TEAM
********************************************************************/
using UnityEngine;
using UnityEngine.SceneManagement;
using System;
using System.Collections;
#if UNITY_EDITOR
using UnityEditor.SceneManagement;
#endif

namespace Zeus.Core
{
    public sealed class SceneAwaiter : Awaiter
    {
        public SceneAwaiter(string scenePath, LoadSceneMode mode) : base(scenePath.GetHashCode())
        {
            m_Continuation = null;
            m_ScenePath = scenePath;

            if (string.IsNullOrEmpty(scenePath) || !scenePath.EndsWith(".unity"))
            {
                m_IsCompleted = true;
            }
            else
            {
                var sceneName = System.IO.Path.GetFileNameWithoutExtension(scenePath);
                m_Result = SceneManager.GetSceneByName(sceneName);
                m_IsCompleted = m_Result.IsValid();

                if (!m_IsCompleted)
                {

#if UNITY_EDITOR
                    if (!Application.isPlaying)
                    {
                        EditorSceneManager.sceneOpened += _OnSceneOpenedCallback;
                        EditorSceneManager.OpenScene(scenePath, mode == LoadSceneMode.Additive ? OpenSceneMode.Additive : OpenSceneMode.Single);
                        return;
                    }
#endif
                    SceneManager.sceneLoaded += _OnSceneLoadedCallback;
                    SceneManager.LoadScene(scenePath, mode);
                }
            }
        }
        private string m_ScenePath;
        private Scene m_Result;

        public Scene Result
        {
            get
            {
                return m_Result;
            }
        }

        public Scene GetResult()
        {
            return Result;
        }

        public SceneAwaiter GetAwaiter()
        {
            return this;
        }

#if UNITY_EDITOR
        private void _OnSceneOpenedCallback(Scene scene, OpenSceneMode mode)
        {
            if (scene.path == m_ScenePath)
            {
                EditorSceneManager.sceneOpened -= _OnSceneOpenedCallback;
                m_Result = scene;
                m_IsCompleted = true;
                m_Continuation?.Invoke();
            }
        }
#endif
        private void _OnSceneLoadedCallback(Scene scene, LoadSceneMode mode)
        {
            if (scene.path == m_ScenePath)
            {
                SceneManager.sceneLoaded -= _OnSceneLoadedCallback;
                m_Result = scene;
                m_IsCompleted = true;
                m_Continuation?.Invoke();
            }
        }
    }

    public class SceneUnloadAsyncOperation : IEnumerator
    {
        public SceneUnloadAsyncOperation(Scene scene)
        {
            m_Scene = scene;
#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                EditorSceneManager.sceneClosed += _OnSceneClosedCallback;
                EditorSceneManager.CloseScene(scene, true);
                return;
            }
#endif
            SceneManager.sceneUnloaded += _OnSceneUnloadedCallback;
            SceneManager.UnloadSceneAsync(scene);
        }
        private Scene m_Scene;

        public bool isDone
        {
            get; private set;
        }

        public event Action<SceneUnloadAsyncOperation> completed;

        private void _OnSceneUnloadedCallback(Scene scene)
        {
            if (scene.path == m_Scene.path)
            {
                SceneManager.sceneUnloaded -= _OnSceneUnloadedCallback;
                isDone = true;
                completed?.Invoke(this);
            }
        }
#if UNITY_EDITOR
        private void _OnSceneClosedCallback(Scene scene)
        {
            if (scene.path == m_Scene.path)
            {
                EditorSceneManager.sceneClosed -= _OnSceneClosedCallback;
                isDone = true;
                completed?.Invoke(this);
            }
        }
#endif

        #region IEnumerator接口实现
        object IEnumerator.Current
        {
            get
            {
                return null;
            }
        }

        bool IEnumerator.MoveNext()
        {
            return !isDone;
        }

        void IEnumerator.Reset()
        {
        }
        #endregion
    }

    public static partial class SceneExtensions
    {
        public static SceneUnloadAsyncOperation Unload(this Scene scene)
        {
            return new SceneUnloadAsyncOperation(scene);
        }
    }
}