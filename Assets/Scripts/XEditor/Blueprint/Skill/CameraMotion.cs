#if UNITY_EDITOR
using CFUtilPoolLib;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using CFEngine;

namespace VirtualSkill
{
    public class CameraMotion
    {
        public SkillCamera Hoster = null;

        private string _pre_trigger = "ToIdle";
        private string _trigger = "ToIdle";
        private float _blend_time = 0;
        private float _force_blend_time = 0;
        private bool _use_camera_pos_at_end = false;

        private uint _reset_motion_fov_token = 0;

        public CameraMotion(SkillCamera hoster)
        {
            Hoster = hoster;
        }

        public float OverrideAnimClip(string motion, string clipname)
        {
            AnimationClip animClip = null;
            if (!SkillHoster.GetHoster.UsePhoneAnimLoad)
            {
                animClip = LoadMgr.singleton.LoadAssetImmediate<AnimationClip>(clipname, ".anim");
                LoadMgr.singleton.DestroyImmediate();
            }
            else
            {
                animClip = SkillHoster.GetHoster.LoadPhoneAnimClip(clipname);
            }
            //XResourceLoaderMgr.singleton.GetSharedResource<AnimationClip>(clipname, ".anim");
            Hoster.OverrideAnimClip(motion, animClip);
            return animClip.length;
        }

        public void PlayCameraMotion(string motion, EcsData.XCameraMotionData data)
        {
            ulong entityid = SkillHoster.GetHoster.GetEntityTarget(SkillHoster.PlayerIndex);
            string motionPath = 
                (data.UseAssistCameraAnim && entityid != 0 && !string.IsNullOrEmpty(SkillHoster.GetHoster.EntityDic[entityid].presentData.AssistCameraAnim)) ? 
                string.Format("Animation/Main_Camera/{0}", SkillHoster.GetHoster.EntityDic[entityid].presentData.AssistCameraAnim) : data.MotionPath;
            AnimTrigger("To" + motion, OverrideAnimClip(motion != "Effect" ? motion : "CameraEffect", motionPath));
            SkillHoster.GetHoster.CMotion.BlendTime = data.EnterBlendTime;
            SkillHoster.GetHoster.CMotion.FarClipPlane = data.FarClipPlane;
            SkillHoster.GetHoster.CMotion.Enable = true;
            _blend_time = data.BlendTime;
            _force_blend_time = data.ForceBlendTime;
            _use_camera_pos_at_end = data.UseCameraPosAtEnd;
            Hoster.AnchorBased = true;
            Hoster.UseCache = !data.FollowHoster;
            Hoster.CacheHoster();
            XTimerMgr.singleton.FireTimer(_reset_motion_fov_token);
        }

        private void AnimTrigger(string status, float reset = -1)
        {
            _trigger = status;
        }

        public void ResetStatus(bool force = true)
        {
            AnimTrigger("ToIdle");
            if (_pre_trigger != _trigger)
            {
                if (SkillHoster.GetHoster.CFreeLook != null)
                {
                    if (_use_camera_pos_at_end) SkillHoster.GetHoster.CFreeLook.SetInheritPosition(false);
                    else SkillHoster.GetHoster.CFreeLook.BackToFollow(0);
                }
                SkillHoster.GetHoster.CMotion.BlendTime = force ? _force_blend_time : _blend_time;
                SkillHoster.GetHoster.CMotion.Enable = false;
                Hoster.AnchorBased = false;
                Hoster.Ator.SetTrigger(_trigger);

                Hoster._dummyCamera.fieldOfView = 0.1f;
                XTimerMgr.singleton.KillTimer(_reset_motion_fov_token);
                _reset_motion_fov_token = XTimerMgr.singleton.SetTimer(_blend_time, Hoster.ResetMotionFov, null);
            }
            _pre_trigger = _trigger;
        }

        public void PostUpdate(float deltaTime)
        {
            if (_pre_trigger != _trigger)
            {
                if (_trigger == "ToIdle")
                {
                    if (_use_camera_pos_at_end) SkillHoster.GetHoster.CFreeLook.SetInheritPosition(false);
                    else SkillHoster.GetHoster.CFreeLook.BackToFollow(0);
                    SkillHoster.GetHoster.CMotion.BlendTime = _blend_time;
                    SkillHoster.GetHoster.CMotion.Enable = false;
                    Hoster.AnchorBased = false;

                    Hoster._dummyCamera.fieldOfView = 0.1f;
                    XTimerMgr.singleton.KillTimer(_reset_motion_fov_token);
                    _reset_motion_fov_token = XTimerMgr.singleton.SetTimer(_blend_time, Hoster.ResetMotionFov, null);
                }
                Hoster.Ator.SetTrigger(_trigger);
                Hoster.Ator.Update(0);
            }
            _pre_trigger = _trigger;
        }

    }
}
#endif