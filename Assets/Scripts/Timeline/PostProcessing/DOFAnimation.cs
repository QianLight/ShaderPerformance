using System;
using UnityEngine.Rendering.Universal;

namespace UnityEngine
{
    [ExecuteInEditMode]
    public class DOFAnimation:MonoBehaviour
    {
        // public bool activeClip;
        public DOFParam OverrideParams;
        // private bool _isActive;
        private int _paramId;
        private void OnEnable()
        {
            // _paramId = URPDepthOfField.instance.AddParam(DOFParam.GetDefaultValue(), URPDOFSource.Timeline, 0);
        }

        private void Update()
        {
            // if (activeClip && !_isActive)
            // {
            //     _paramId = URPDepthOfField.instance.AddParam(DOFParam.GetDefaultValue(), URPDOFSource.Timeline, 0);
            //     _isActive = true;
            // }
            if (OverrideParams.active)
            {
                URPDepthOfField.instance.ModifyTimelineParam(OverrideParams);
                
            }

            // if (!activeClip && _isActive)
            // {
            //     URPDepthOfField.instance.RemoveParam(_paramId);
            //     _isActive = false;
            // }
        }

        private void OnDisable()
        {
            // URPDepthOfField.instance.RemoveParam(_paramId);
        }
        
    }
}