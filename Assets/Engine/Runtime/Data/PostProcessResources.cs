using System;
using UnityEngine;

namespace CFEngine
{
    public sealed class PostProcessResources : ScriptableObject
    {
        [Serializable]
        public sealed class Shaders
        {
            public Shader bloom;
            public Shader godRay;
            public Shader depthOfField;
            public Shader scatter;
            public Shader fog;
            public Shader preEffects; 
            public Shader uber;
            public Shader rtBlur;
            public Shader blurMesh;
            public Shader lutBuilderHdr;
            public Shader radialBlur;
            public Shader copy;
            public ComputeShader lightCulling;
        }

        // public Texture2D fogNoise;
        public Texture3D fogNoise3d;
        public Texture2D distortion0;
        public Texture2D distortion1;

        // public Texture2D lutCurve;
        // public Texture2D preIntegrateSSS;
        public Shaders shaders;
        public SavedScatterInfo defaultScatterInfo;

#if UNITY_EDITOR
        public delegate void ChangeHandler();
        public ChangeHandler changeHandler;

        void OnValidate()
        {
            //shaders.lut2DBaker = Shader.Find("Hidden/PostProcessing/Lut2DBaker");
            if (changeHandler != null)
                changeHandler();
        }

#endif
    }
}
