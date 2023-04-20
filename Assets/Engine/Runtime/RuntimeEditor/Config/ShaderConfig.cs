#if UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace CFEngine
{
    public class ShaderConfig : AssetBaseConifg<ShaderConfig>
    {

        #region matShader
        //[HideInInspector]
        public ShaderFeatureData shaderFeature = new ShaderFeatureData ();
        //[HideInInspector]
        public ShaderGUIData shaderGUI = new ShaderGUIData ();
        #endregion
    }
}
#endif