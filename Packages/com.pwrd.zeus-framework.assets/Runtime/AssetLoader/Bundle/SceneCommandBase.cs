/*******************************************************************
* Copyright © 2017—2021 Perfect World.Co.Ltd. All rights reserved.
* author: 移动项目支持部 ZEUS FRAMEWORK TEAM
********************************************************************/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Zeus.Framework.Asset
{
    internal class SceneCommandBase
    {
        protected string _scenePath;
        protected string _sceneName;
        private CommandState _state;
        private CommandType _commandType;
        private int _frameId;

        public enum CommandState
        {
            Pending,
            Doing,
            Done,
        }

        public enum CommandType
        {
            AsyncLoad,
            CoroutineLoad,
            AsyncUnload,
            CoroutineUnload
        }


        public SceneCommandBase(string scenePath, CommandType type)
        {
            _scenePath = scenePath;
            int idx = scenePath.LastIndexOf('/');
            _sceneName = scenePath.Substring(idx + 1);
            _sceneName = AssetBundleUtils.RemoveSceneFileExtension(_sceneName);
            _state = CommandState.Pending;
            _commandType = type;
        }

        public string ScenePath { get { return _scenePath; } }
        public string SceneName { get { return _sceneName; } }

        public int FrameId { get { return _frameId; } set { _frameId = value; } }

        public virtual void BeginExecuteCommand()
        {
            AssetsLogger.Log("SceneCommand BeginExecuteCommand", _commandType, _scenePath);
            _state = CommandState.Doing;
        }

        public virtual void FinishCommand()
        {
            AssetsLogger.Log("SceneCommand FinishCommand", _commandType, _scenePath);
            _state = CommandState.Done;
        }

        public virtual void UpdateCommandProgress(float percent)
        {

        }

        public CommandState State { get { return _state; } }
        public CommandType CmdType { get { return _commandType; } }
    }
}

