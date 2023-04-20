using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UsingTheirs.ShaderHotSwap
{

    [System.Serializable]
    public class SwapShadersRes
    {
        public string error;
        public string log;
        public List<SwappedShader> shaders;
    }

    [System.Serializable]
    public class SwappedShader
    {
        public RemoteShader shader;
        public List<SwappedMaterial> materials;
    }

    [System.Serializable]
    public class SwappedMaterial
    {
        public RemoteMaterial material;
        public List<RemoteRenderer> renderers;
    }

    [System.Serializable]
    public class RemoteRendererList
    {
        public List<RemoteRenderer> renderers;
    }
}
