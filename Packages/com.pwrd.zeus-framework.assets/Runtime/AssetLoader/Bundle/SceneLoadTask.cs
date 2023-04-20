/*******************************************************************
* Copyright © 2017—2020 Perfect World.Co.Ltd. All rights reserved.
* author: 移动项目支持部 ZEUS FRAMEWORK TEAM
********************************************************************/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.SceneManagement;

namespace Zeus.Framework.Asset
{
    class SceneLoadTask : SceneLoadTaskBase
    {
        public enum LoadStep
        {
            WaitingBundle,
            LoadingScene,
            Finish,
        };

        private Action<bool, float, object> _callback;
        private BundleRef _bundleRef;
        private LoadStep _step;
        private AssetBundleLoadTask _bundleLoadTask;
        private string _sceneName;
        private AsyncOperation _loadOperation;
        private LoadSceneMode _loadMode;
        private object _param;

        public SceneLoadTask(string sceneName, LoadSceneMode loadMode, object param, AssetBundleLoadTask bundleLoadTask, Action<bool, float, object> callback)
        {
            _step = LoadStep.WaitingBundle;
            _sceneName = sceneName;
            _loadMode = loadMode;
            _loadOperation = null;
            _param = param;
            _bundleLoadTask = bundleLoadTask;
            _callback = callback;
        }

        public override void UpdateLoadProgress()
        {
            if (_step == LoadStep.WaitingBundle)
            {
                float progress = _bundleLoadTask.LoadProgress() * 0.5f;
                //Debug.Log("UpdateLoadProgress w " + progress);
                _callback.Invoke(false, progress, _param);
                if (_bundleLoadTask.IsDone())
                {
                    _step = LoadStep.LoadingScene;
                    _sceneName = AssetBundleUtils.RemoveSceneFileExtension(_sceneName);
                    _loadOperation = SceneManager.LoadSceneAsync(_sceneName, _loadMode);
                }
            }
            else if (_step == LoadStep.LoadingScene)
            {
                if (_loadOperation.isDone)
                {
                    _step = LoadStep.Finish;
                }
                else
                {
                    float progress = 0.5f + _loadOperation.progress * 0.5f;
                    //Debug.Log("UpdateLoadProgress L " + progress);
                    _callback.Invoke(false, _bundleLoadTask.LoadProgress() * 0.5f, _param);
                }
            }
        }

        public override bool IsDone()
        {
            return _step == LoadStep.Finish;
        }

        public override void ExecuteCallBack()
        {
            Debug.Assert(_step == LoadStep.Finish);
            _callback.Invoke(true, 1.0f, _param);
        }
    }
}

