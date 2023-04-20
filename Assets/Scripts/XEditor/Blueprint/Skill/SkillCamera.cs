#if UNITY_EDITOR
using CFUtilPoolLib;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using CFEngine;

namespace VirtualSkill
{
    public class SkillCamera
    {
        public GameObject Hoster = null;

        public Camera UnityCamera = null;
        public GameObject CameraObject = null;
        public GameObject DummyObject = null;
        public Camera _dummyCamera = null;
        public Transform dummyCamera = null;
        public Animator Ator = null;

        public AnimatorOverrideController overrideController = new AnimatorOverrideController();

        private bool _inited = false;
        private float _base_offset;
        public float BaseOffset { get { return _base_offset; } }
        public float TargetOffset;
        private Vector3 _target_pos;
        private Quaternion _target_rot;

        public CameraMotion _motion;
        private CameraStretch _stretch;
        //private CameraShake _shake;
        private CameraLayerMask _layerMask;

        public bool AnchorBased = false;

        public float Speed
        {
            get { return Ator.speed; }
            set
            {
                Ator.speed = value;
            }
        }

        public SkillCamera(GameObject hoster)
        {
            Hoster = hoster;
            _motion = new CameraMotion(this);
            _stretch = new CameraStretch(this);
            //_shake = new CameraShake(this);
            _layerMask = new CameraLayerMask(this);
            Init();
        }

        public void Init()
        {
            CameraObject = GameObject.Find(@"Main Camera");
            UnityCamera = CameraObject.GetComponent<Camera>();
            

            if (null != CameraObject)
            {
                DummyObject = XResourceHelper.LoadEditorResourceAtBundleRes<GameObject>("Prefabs/Cinemachine/DummyCamera", true);
                DummyObject.name = "Dummy Camera";

                dummyCamera = DummyObject.transform.GetChild(0);
                _dummyCamera = dummyCamera.GetComponent<Camera>();
                Ator = DummyObject.GetComponent<Animator>();

                overrideController.runtimeAnimatorController = Ator.runtimeAnimatorController;
                Ator.runtimeAnimatorController = overrideController;


                OverrideAnimClip("Idle", "Animation/Main_Camera/Main_Camera_Idle");
            }
        }

        public void OverrideAnimClip(string motion, string clipname)
        {
            //get override clip
            AnimationClip animClip = LoadMgr.singleton.LoadAssetImmediate<AnimationClip>(clipname, ".anim");
            OverrideAnimClip(motion, animClip);
            LoadMgr.singleton.DestroyImmediate();
        }

        public void OverrideAnimClip(string motion, AnimationClip clip)
        {
            //override
            if (clip != null && overrideController[motion] != clip) overrideController[motion] = clip;
        }

        public void PlayCameraMotion(string motion, EcsData.XCameraMotionData data)
        {
            _motion.PlayCameraMotion(motion, data);
        }

        public void PlayCameraStretch(float target, float time, float factor)
        {
            _stretch.SetTargetOffset(target, time, factor);
        }

        public void SetSimpleFollowState(bool state)
        {
            if (SkillHoster.GetHoster.CFreeLook != null)
            {
                SkillHoster.GetHoster.CFreeLook.SimpleFollowState = state;
            }
        }

        public int PlayCameraShake(string path, float amplitudeGain, float frequencyGain, float time, float attackTime, float decayTime, Vector3 pos, float radius)
        {
            //_shake.SetCameraShake(lifeTime, frequency, amplitudeX, amplitudeY, amplitudeZ, random);
            if (SkillHoster.GetHoster.CFreeLook != null)
            {
                return SkillHoster.GetHoster.CFreeLook.SetImpulse(path, amplitudeGain, frequencyGain, time, attackTime, decayTime, pos, radius);
            }
            else if (SkillHoster.GetHoster.CSoloMix != null)
            {
                return SkillHoster.GetHoster.CSoloMix.SetImpulse(path, amplitudeGain, frequencyGain, time, attackTime, decayTime, pos, radius);
            }
            else if (SkillHoster.GetHoster.CMotion != null)
            {
                return SkillHoster.GetHoster.CMotion.SetImpulse(path, amplitudeGain, frequencyGain, time, attackTime, decayTime, pos, radius);
            }
            return -1;
        }

        public void StopCameraShake(int id)
        {
            if (SkillHoster.GetHoster.CFreeLook != null)
            {
                SkillHoster.GetHoster.CFreeLook.CancelImpulse(id);
            }
            else if (SkillHoster.GetHoster.CSoloMix != null)
            {
                SkillHoster.GetHoster.CSoloMix.CancelImpulse(id);
            }
            else if (SkillHoster.GetHoster.CMotion != null)
            {
                SkillHoster.GetHoster.CMotion.CancelImpulse(id);
            }
        }

        public void SetDamping(float time,AnimationCurve curve)
        {
            if (SkillHoster.GetHoster.CFreeLook != null)
                SkillHoster.GetHoster.CFreeLook.SetDamping(time,curve);
            else if (SkillHoster.GetHoster.CSoloMix != null)
                SkillHoster.GetHoster.CSoloMix.SetDamping(time,curve);
        }
        public void ResetDamping(float time)
        {
            if (SkillHoster.GetHoster.CFreeLook != null)
                SkillHoster.GetHoster.CFreeLook.ResetDamping(time);
            else if (SkillHoster.GetHoster.CSoloMix != null)
                SkillHoster.GetHoster.CSoloMix.ResetDamping(time);
        }
        public void SetFov(float fov, float fadeinTime, AnimationCurve fadeInCurve, float lastTime, float fadeoutTime, AnimationCurve fadeOutCurve, bool isMotion)
        {
            //_shake.SetCameraShake(lifeTime, frequency, amplitudeX, amplitudeY, amplitudeZ, random);
            if (!isMotion)
            {
                if (SkillHoster.GetHoster.CFreeLook != null)
                {
                    SkillHoster.GetHoster.CFreeLook.SetFov(fov, fadeinTime, fadeInCurve, lastTime, fadeoutTime, fadeOutCurve);
                }
                else if (SkillHoster.GetHoster.CSoloMix != null)
                {
                    SkillHoster.GetHoster.CSoloMix.SetFov(fov, fadeinTime, fadeInCurve, lastTime, fadeoutTime, fadeOutCurve);
                }
            }
            if (SkillHoster.GetHoster.CMotion != null)
            {
                SkillHoster.GetHoster.CMotion.SetFov(fov, fadeinTime, fadeInCurve, lastTime, fadeoutTime, fadeOutCurve);
            }
        }

        public void ResetFov(float time)
        {
            if (SkillHoster.GetHoster.CFreeLook != null)
            {
                SkillHoster.GetHoster.CFreeLook.ResetFov(time);
            }
            if (SkillHoster.GetHoster.CSoloMix != null)
            {
                SkillHoster.GetHoster.CSoloMix.ResetFov(time);
            }
            if (SkillHoster.GetHoster.CMotion != null)
            {
                SkillHoster.GetHoster.CMotion.ResetFov(time);
            }
        }

        public void SetMotionFov(float fov, float time)
        {
            if (SkillHoster.GetHoster.CMotion != null)
            {
                SkillHoster.GetHoster.CMotion.SetFov(fov, time, null, 0, 0.5f, null);
            }
        }

        public void ResetMotionFov(object o)
        {
            if (SkillHoster.GetHoster.CMotion != null)
            {
                SkillHoster.GetHoster.CMotion.ResetFov(0);
            }
        }

        public void StartRotate(Transform target, float[] targetAngles, float rotTime, float rotAcceleration)
        {
            if (SkillHoster.GetHoster.CFreeLook != null)
            {
                SkillHoster.GetHoster.CFreeLook.StartRotate(target, targetAngles, rotTime, rotAcceleration);
            }
        }

        public void PlayCameraLayerMask(float lifeTime,int mask)
        {
            _layerMask.SetCameraLayerMask(mask, lifeTime);
        }

        public void Reset(ulong id, bool _shake_dont_stop_at_end = false,float endblendTime=0.5f)
        {
            ResetFov(endblendTime);
            ResetDamping(0.5f);
            _motion.ResetStatus();
            _stretch.ResetTargetOffset();
        }

        public void PostUpdate(float deltaTime)
        {
            if ((SkillHoster.GetHoster.CFreeLook == null || SkillHoster.GetHoster.CFreeLook.Enable == false)&&(SkillHoster.GetHoster.CSoloMix == null || SkillHoster.GetHoster.CSoloMix.Enable == false))
            {
                MotionUpdate();
                _stretch.PostUpdate(deltaTime);

                SyncCamera();

            }
            _motion.PostUpdate(deltaTime);
            if (SkillHoster.GetHoster.CMotion != null)
            {
                Vector3 dummyCameraPos = dummyCamera.position - DummyObject.transform.position;
                Vector3 forward = Vector3.Cross(dummyCamera.forward, dummyCamera.up);
                Quaternion dummyCamera_quat = Quaternion.LookRotation(forward, dummyCamera.up);
                Vector3 _target_pos = (AnchorBased ? HosterRot : Quaternion.identity) * dummyCameraPos;
                Quaternion _target_rot = (AnchorBased ? HosterRot : Quaternion.identity) * dummyCamera_quat;
                if (_dummyCamera.fieldOfView > 1)
                {
                    _target_rot *= Quaternion.Euler(0, 90, 0);
                    SetMotionFov(_dummyCamera.fieldOfView, 0);
                }
                SkillHoster.GetHoster.CMotion.SetMotionCamera(HosterPos + DummyObject.transform.position + _target_pos, _target_rot);
            }

            _layerMask.PostUpdate(deltaTime);
            //_shake.PostUpdate(deltaTime);
        }

        private void MotionUpdate()
        {
            Vector3 dummyCameraPos = dummyCamera.position - DummyObject.transform.position;
            Vector3 forward = Vector3.Cross(dummyCamera.forward, dummyCamera.up);
            Quaternion dummyCamera_quat = Quaternion.LookRotation(forward, dummyCamera.up);
            if (!_inited)
            {
                _base_offset = dummyCameraPos.magnitude;
                TargetOffset = _base_offset;
            }
            _target_pos = (AnchorBased ? HosterRot : Quaternion.identity) * dummyCameraPos.normalized;
            _target_rot = (AnchorBased ? HosterRot : Quaternion.identity) * dummyCamera_quat;
        }

        public bool UseCache = false;
        private Vector3 _cache_pos;
        private Quaternion _cache_rot;
        public void CacheHoster()
        {
            _cache_pos = Hoster.transform.position;
            _cache_rot = Hoster.transform.rotation;
        }
        private Vector3 HosterPos { get { return UseCache ? _cache_pos : Hoster.transform.position; } }
        private Quaternion HosterRot { get { return UseCache ? _cache_rot : Hoster.transform.rotation; } }

        public void SyncCamera()
        {
            CameraObject.transform.position = HosterPos + DummyObject.transform.position + _target_pos * TargetOffset;
            CameraObject.transform.rotation = _target_rot;
        }
    }
}
#endif