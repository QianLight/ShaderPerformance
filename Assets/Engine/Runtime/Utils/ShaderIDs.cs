using UnityEngine;

namespace CFEngine
{
        // Pre-hashed shader ids - naming conventions are a bit off in this file as we use the same
        // fields names as in the shaders for ease of use... Would be nice to clean this up at some
        // point.
        public static class ShaderIDs
    {
        //common
        public static readonly int MainTex = Shader.PropertyToID("_MainTex");
        public static readonly int _MainTex_Half = Shader.PropertyToID("_MainTex_Half");
        public static readonly int UVTransform = Shader.PropertyToID("_UVTransform");

    }
}