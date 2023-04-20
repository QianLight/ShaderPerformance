#if UNITY_EDITOR
using CFUtilPoolLib;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace VirtualSkill
{
    public class CameraLayerMask
    {
        public SkillCamera Hoster = null;

        // private float _total_time = 0f;
        // private float _cur_time = 0f;

        // private int ignoreMask = 0;

        public CameraLayerMask(SkillCamera hoster)
        {
            Hoster = hoster;
        }

        public void SetCameraLayerMask(int mask, float LifeTime)
        {
            //ResetCameraLayerMask(ignoreMask);
            //GameObjectLayerHelper.HideLayerWithMask(mask, false);
            //GameObjectLayerHelper.Push();
            //ignoreMask = mask;
            //_total_time = LifeTime;
        }

        private void ResetCameraLayerMask(int mask)
        {
            //if (ignoreMask == 0) return;

            //// GameObjectLayerHelper.HideLayerWithMask(mask, true);
            //GameObjectLayerHelper.Pop();
            //ignoreMask = 0;
        }

        public void PostUpdate(float deltaTime)
        {
            //if (Hoster == null) return;

            //if (_cur_time < _total_time)
            //{
            //    _cur_time += (_total_time - _cur_time) > deltaTime ? deltaTime : (_total_time - _cur_time);
            //    ResetCameraLayerMask(ignoreMask);
            //}
        }

    }
}
#endif