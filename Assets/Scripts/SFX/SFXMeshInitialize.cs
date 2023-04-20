using System;
using UnityEngine;

namespace CFEngine
{
    public class SFXMeshInitialize: MonoBehaviour
    {
        public ParticleSystemRenderer _psr;

        void Reset()
        {
            TryGetComponent(out ParticleSystemRenderer _psr);
            if (!_psr) DestroyImmediate(this);
        }

        private void Start()
        {
            if (!SystemInfo.supportsComputeShaders || SystemInfo.maxComputeBufferInputsVertex <= 0)
            {
                _psr.enableGPUInstancing = false;
            }
        }
    }
}