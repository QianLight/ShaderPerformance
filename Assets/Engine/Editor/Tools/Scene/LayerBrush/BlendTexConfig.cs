using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace CFEngine.Editor
{

    [System.Serializable]
    public class BlendTexConfig : ScriptableObject
    {
        public Texture2D editTex;
        public Texture2D runtimeTex;
    }
}