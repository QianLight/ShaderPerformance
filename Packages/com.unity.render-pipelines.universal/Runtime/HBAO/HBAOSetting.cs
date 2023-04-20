using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace UnityEngine.Rendering.Universal
{
    [System.Serializable]
    public sealed class HBAOSetting
    {
        public bool Enable = false;
        [Range(0, 4)]
        public int DownSample = 1;
        [Range(3, 8)]
        public int RayMarchingDirectionCount = 4;
        [Range(1, 5)]
        public int RayMarchingStepCount = 2;
        [Range(0, 4)]
        public float RayMarchingRadius = 0.8f;
        [Range(64, 512)]
        public int MaxPixelRadius = 128;
        [Range(-1, 1.0f)]
        public float AngleBiasValue = 0.05f;
        [Range(0, 10)]
        public float Strength = 1;

        public Vector2 DistanceFalloff = new Vector2(50, 100);
        [Range(0, 2)]
        public float BlurRadius = 1.85f;
    }
}
