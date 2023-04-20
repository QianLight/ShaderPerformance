#if UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace CFEngine
{
    public enum EDummyMatType
    {
        SceneMat,
        DynamicMat,
        EffectMat,
    }
    public class EditorMaterialConfig : AssetBaseConifg<EditorMaterialConfig>
    {
        #region matShader
        [HideInInspector]
        public DummyMatData sceneMat = new DummyMatData ();
        [HideInInspector]
        public DummyMatData dynamicMat = new DummyMatData ();
        [HideInInspector]
        public DummyMatData effectMat = new DummyMatData ();
        #endregion       
        public List<string> ignoreBindParam = new List<string> ();
        public List<DefaultMatConfig> defaultMatConfig = new List<DefaultMatConfig> ();
    }

    [CustomEditor (typeof (EditorMaterialConfig))]
    public partial class EditorMaterialConfigEdit : UnityEditor.Editor
    {
        public override void OnInspectorGUI () { }

    }
}
#endif