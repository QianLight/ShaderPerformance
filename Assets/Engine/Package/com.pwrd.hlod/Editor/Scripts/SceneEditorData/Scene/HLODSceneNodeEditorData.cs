using System;
using UnityEngine;

namespace com.pwrd.hlod.editor
{
    [ExecuteInEditMode]
    public class HLODSceneNodeEditorData : MonoBehaviour
    {
        /// <summary>
        /// 重载时记录有效
        /// </summary>
        public SceneNode sceneNode;

        private void Awake()
        {
            tag = "EditorOnly";
        }
    }
}