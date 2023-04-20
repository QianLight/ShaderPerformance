using System;
using System.Collections.Generic;
using Athena.MeshSimplify;
using com.pwrd.hlod.editor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;

namespace com.pwrd.hlod.editor
{
    [Serializable]
    public class HLODConfigSetting : ScriptableObject
    {
        public HlodMethod hlodMethod = HlodMethod.AthenaSimplify;
        public bool useVoxel = false;
        public ProxyMapType proxyMapType = ProxyMapType.LODGroup;
        public TextureChannel textureChannel = TextureChannel.Albedo;
        public RendererBakerSetting rendererBakerSetting;
        public ShaderBindConfig shaderBindConfig;
        public string targetParentNamePath;
        public List<string> rootNamePaths = new List<string>();
        public SceneSetting globalSetting = new SceneSetting();
    }
}