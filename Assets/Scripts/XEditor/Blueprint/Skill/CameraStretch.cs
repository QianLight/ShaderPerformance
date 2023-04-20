#if UNITY_EDITOR
using CFUtilPoolLib;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace VirtualSkill
{
    public enum StretchType
    {
        cameraStretch,
        cameraCamping,
    }

    public class CameraStretch
    {

        public SkillCamera Hoster = null;

        private float _total_time = 0f;
        private float _cur_time = 0f;
        private float _begin_offest = 0f;
        private float _target_offest = 0f;
        private float _factor = 1f;

        public bool isMotion;

        public CameraStretch(SkillCamera hoster)
        {
            Hoster = hoster;
        }

        public void SetTargetOffset(float target, float time, float factor)
        {
            if (Hoster == null) return;

            _total_time = time;
            _cur_time = 0f;
            _begin_offest = Hoster.TargetOffset;
            _target_offest = target * Hoster.BaseOffset;
            _factor = factor;
        }

        public void ResetTargetOffset()
        {
            if (Hoster == null) return;

            Hoster.TargetOffset = Hoster.BaseOffset;
            _total_time = 0f;
            _cur_time = 0f;
            _begin_offest = 0f;
            _target_offest = 0f;
            _factor = 1f;
        }

        public void PostUpdate(float deltaTime)
        {
            if (Hoster == null) return;

            if (_cur_time < _total_time)
            {
                _cur_time += (_total_time - _cur_time) > deltaTime ? deltaTime : (_total_time - _cur_time);
                Hoster.TargetOffset = _begin_offest + (_target_offest - _begin_offest) * Mathf.Pow((_cur_time / _total_time), _factor);
            }
        }

    }
}
#endif