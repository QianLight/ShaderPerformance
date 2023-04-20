using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UsingTheirs.ShaderHotSwap
{

    [System.Serializable]
    public class SwapShadersReq
    {
        public List<RemoteShader> shaders;
        public string assetBundleBase64;
    }

}
