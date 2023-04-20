#if UNITY_EDITOR
using CFUtilPoolLib;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace VirtualSkill
{
    public class CameraShake
    {
        //public SkillCamera Hoster = null;

        //private float _interval_time = 0f;
        //private float _total_time = 0f;
        //private float _cur_time = 0f;
        //private float _frequency = 0f;
        //private float _amplitudeX = 0f;
        //private float _amplitudeY = 0f;
        //private float _amplitudeZ = 0f;
        //private bool _random = false;

        //private Vector3 _shake_delta;

        //public CameraShake(SkillCamera hoster)
        //{
        //    Hoster = hoster;
        //}

        //public void SetCameraShake(float lifeTime, float frequency, float amplitudeX, float amplitudeY, float amplitudeZ, bool random)
        //{
        //    _interval_time = 1;
        //    _total_time = lifeTime;
        //    _cur_time = 0;
        //    _frequency = frequency;
        //    _amplitudeX = amplitudeX;
        //    _amplitudeY = amplitudeY;
        //    _amplitudeZ = amplitudeZ;
        //    _random = random;

        //    if (!(SkillHoster.GetHoster.CFreeLook == null || SkillHoster.GetHoster.CFreeLook.Enable == false))
        //    {
        //        SkillHoster.GetHoster.CFreeLook.SetNoise(amplitudeX, frequency, lifeTime);
        //    }
        //}

        //public void PostUpdate(float deltaTime)
        //{
        //    if (Hoster == null) return;

        //    if (SkillHoster.GetHoster.CFreeLook == null || SkillHoster.GetHoster.CFreeLook.Enable == false)
        //    {
        //        if (_cur_time < _total_time)
        //        {
        //            _cur_time += (_total_time - _cur_time) > deltaTime ? deltaTime : (_total_time - _cur_time);
        //            _interval_time += deltaTime;
        //            if (_interval_time > 1.0f / _frequency)
        //            {
        //                _interval_time = 0;
        //                _shake_delta = Shake();
        //            }
        //            Hoster.CameraObject.transform.position += _shake_delta;
        //        }
        //    }
        //    else 
        //    {
        //        if (_cur_time < _total_time)
        //        {
        //            _cur_time += (_total_time - _cur_time) > deltaTime ? deltaTime : (_total_time - _cur_time);
        //            float scale = (1 - _cur_time / _total_time);
        //            SkillHoster.GetHoster.CFreeLook.SetNoise(_amplitudeX * scale, _frequency, 0);
        //        }
        //    }
        //}

        //private Vector3 Shake()
        //{
        //    float factor = Time.time;
        //    float factorX = (Mathf.Min(1, Mathf.PerlinNoise(factor * 10, factor * 10)) - 0.5f) * 2;
        //    factor += Mathf.PI;
        //    float factorY = (Mathf.Min(1, Mathf.PerlinNoise(factor * 10, factor * 10)) - 0.5f) * 2;
        //    factor += Mathf.PI;
        //    float factorZ = (Mathf.Min(1, Mathf.PerlinNoise(factor * 10, factor * 10)) - 0.5f) * 2;

        //    return new Vector3(_amplitudeX * factorX, _amplitudeY * factorY, _amplitudeZ * factorZ);
        //}

    }
}
#endif