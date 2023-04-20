/*******************************************************************
* Copyright © 2017—2021 Perfect World.Co.Ltd. All rights reserved.
* author: 移动项目支持部 ZEUS FRAMEWORK TEAM
********************************************************************/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace Zeus.Framework.Asset
{

    public class SceneLoadOperation
    {
        private bool _isDone = false;
        private System.Object _param = null;
        private Action<SceneLoadOperation> _completed = null;
        private float _progress;
        public bool IsDone { get { return _isDone; } }

        public Action<SceneLoadOperation> Completed { get { return _completed; } }

        public System.Object Param { get { return _param; } set { _param = value; } }

        internal SceneLoadOperation(System.Object param)
        {
            _param = param;
        }

        internal void OnSceneLoadCallback(bool isDone, float progress, System.Object param)
        {
            _progress = progress;
            if (isDone)
            {
                _isDone = true;
                if (_completed != null)
                {
                    _completed(this);
                }
            }
        }
    }
}
