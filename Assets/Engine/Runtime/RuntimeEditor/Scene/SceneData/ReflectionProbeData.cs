#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Rendering;

namespace CFEngine
{
    [System.Serializable]
    public class ReflectionProbeData
    {
        public ReflectionProbeMode mode;
        public int importance;
        public Texture cube;
        public Vector4 textureHDRDecodeValues;
        public ReflectionProbeClearFlags clearFlags;
        public Color backgroundColor;
        public Vector3 size;
        public int resolution;
        public float shadowDistance;
        public bool hdr;
        public float intensity;
        public float farClipPlane;
        public float nearClipPlane;
        public Vector3 center;
    }
}
#endif