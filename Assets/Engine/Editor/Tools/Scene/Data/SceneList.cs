using System;
using System.Collections.Generic;


using UnityEngine;
using UnityEditor;
using UnityEngine.SceneManagement;
namespace CFEngine.Editor
{
    [Serializable]
    public class SceneInfo
    {
        public SceneAsset sceneAsset;
        public bool notBuild = false;
        [NonSerialized]
        public string ids = "";

        public bool needconvert=true;

    }
    public class SceneList : ScriptableObject
    {
        public List<SceneInfo> sceneList = new List<SceneInfo>();

    }
}