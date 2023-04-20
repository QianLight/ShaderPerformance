using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
namespace CFEngine.Editor
{
    [System.Serializable]
    public class AvatarConfig
    {
        public GameObject fbx;
        public AvatarMask avatarMask;
        public bool isHuman = true;
    }

    [System.Serializable]
    public class FbxConfig
    {
        public string fbxName;
        public GameObject fbx;
    }

    public class AvatarData : ScriptableObject
    {
        public List<AvatarConfig> avatarConfig = new List<AvatarConfig>();

        public List<FbxConfig> fbxConfig = new List<FbxConfig>();
    }
}