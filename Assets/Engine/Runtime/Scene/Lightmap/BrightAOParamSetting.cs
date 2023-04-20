using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CFEngine
{
    [ExecuteInEditMode]
    public class BrightAOParamSetting : MonoBehaviour
    {
        public bool isUse = false;
        [Range(0, 1)] public float _AOThreshold = 0f;
        [Range(0, 1)] public float _InstensityOffset = 0f;
        [Range(3, 10)] public float _lightmapInstensity = 3f;

        //x: isUse
        //y: AOThreshold
        //z: InstensityOffset
        //w: LightmapInstensity
        private Vector4 _BrightAOParams;
        private static readonly int BrightAOParams = Shader.PropertyToID("_BrightAOParams");

        public void InitParam()
        {
            
        }
        
        // Update is called once per frame
        void Update()
        {
            _BrightAOParams.x = isUse ? 1f : 0f;
            _BrightAOParams.y = (50 - _AOThreshold * 50) / 100f;
            _BrightAOParams.z = (50 - _InstensityOffset * 50) / 100f;
            _BrightAOParams.w = _lightmapInstensity;
            Shader.SetGlobalVector(BrightAOParams, _BrightAOParams);
        }

        
    }
}