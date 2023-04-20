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
    class SceneLoadCoroutineTask : SceneLoadTaskBase
    {
        public enum LoadStep
        {
            WaitingBundle,
            BundleReadly,
            Finish,
        };

        private  LoadStep _step;
        private AssetBundleLoadTask _bundleLoadTask;
        private string _sceneName;
        private LoadSceneMode _loadMode;
        private string _errorMsg = null;

        public SceneLoadCoroutineTask(string sceneName, LoadSceneMode loadMode, AssetBundleLoadTask bundleLoadTask)
        {
            _step = LoadStep.WaitingBundle;
            _sceneName = sceneName;
            _loadMode = loadMode;
            _bundleLoadTask = bundleLoadTask;
        }

        public override void UpdateLoadProgress()
        {
            if (_step == LoadStep.WaitingBundle)
            {
                if(_bundleLoadTask == null)
                {
                    _step = LoadStep.BundleReadly;
                }
                else
                {
                    float progress = _bundleLoadTask.LoadProgress() * 0.5f;
                    if (_bundleLoadTask.IsDone())
                    {
                        _step = LoadStep.BundleReadly;
                        if(_bundleLoadTask.GetAssetBundle() == null)
                        {
                            _errorMsg = string.Format("scene: {0} bundle load failed", _sceneName);
                        }
                    }
                }
            }
        }

        public bool IsBundleReadly()
        {
            return _step == LoadStep.BundleReadly;
        }

        public IEnumerator LoadSceneAsync()
        {
            _sceneName = AssetBundleUtils.RemoveSceneFileExtension(_sceneName);
            AsyncOperation operation = null;
            try
            {
                operation = SceneManager.LoadSceneAsync(_sceneName, _loadMode);
            }
            catch(Exception ex)
            {
                Debug.LogException(ex);
                _step = LoadStep.Finish;
                _errorMsg = ex.Message;
                yield break;
            }
            //operation.priority = (int)BundleAssetLoadTask.LoadPriority.High;
            yield return operation;
            _step = LoadStep.Finish;
        }

        public override bool IsDone()
        {
            return _step == LoadStep.Finish;
        }

        public override void ExecuteCallBack()
        {
            Debug.Assert(_step == LoadStep.Finish);
        }

        public string ErrorMsg { get { return _errorMsg; } }

        public void FinishTask()
        {
            _step = LoadStep.Finish;
        }
    }
}

