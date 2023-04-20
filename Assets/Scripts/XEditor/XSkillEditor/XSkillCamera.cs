﻿#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using CFUtilPoolLib;
using UnityEngine;

namespace XEditor {
    public class XSkillCamera {
        public XSkillCamera (GameObject hoster) {
            _hoster = hoster;
        }

        private GameObject _hoster = null;
        private enum XCameraExStatus {
            Idle,
            Dash,
            Effect,
            UltraShow,
            UltraEnd
        }

        private bool _status_changed = false;

        private XCameraExStatus _status = XCameraExStatus.Idle;

        private GameObject _cameraObject = null;

        private GameObject _dummyObject = null;
        private Transform _dummyCamera = null;
        private Transform _cameraTransform = null;

        private Animator _ator = null;
        public AnimatorOverrideController _overrideController = new AnimatorOverrideController ();

        private string _trigger = null;

        private UnityEngine.Camera _camera = null;

        private float _elapsed = 0;

        private bool _damp = false;
        private float _damp_delta = 0;
        private Vector3 _damp_dir = Vector3.zero;

        private bool _follow_position = true;
        private bool _relative_to_idle = false;
        private bool _look_at = false;
        private bool _sync_begin = false;
        private CameraMotionSpace _effect_axis = CameraMotionSpace.World;

        private bool _root_pos_inited = false;
        private bool _idle_root_pos_inited = false;

        private Vector3 _root_pos = Vector3.zero;
        private Vector3 _idle_root_pos = Vector3.zero;

        private Vector3 _v_self_p = Vector3.zero;
        private Quaternion _q_self_r = Quaternion.identity;

        private Quaternion _idle_root_rotation = Quaternion.identity;
        private float _idle_root_rotation_y = 0;

        private Vector3 _last_dummyCamera_pos = Vector3.zero;
        private Vector3 _dummyCamera_pos = Vector3.zero;

        private readonly float _damp_factor = 1.0f;

        private XCameraMotionData _motion = new XCameraMotionData ();

        //distance set by arts in animation
        private float _basic_dis = 4.2f;
        //distance set by designer in table
        private float _default_dis = 4.2f;

        public float DefaultDis{get{return _default_dis;}set{_default_dis=value;}}

        public float CameraOffset = 0;

        public void InitDis (float dis) {
            DefaultDis = dis;
            TargetOffset = Offset = DefaultDis;
        }

        public float TargetOffset;
        public float Offset;

        //kill all timer when leave scene.
        private uint _token = 0;

        public UnityEngine.Camera UnityCamera {
            get { return _camera; }
        }

        public Transform CameraTrans {
            get { return _cameraTransform; }
        }

        public Animator CameraAnimator {
            get { return _ator; }
        }

        public Vector3 Position {
            get { return _cameraTransform.position; }
        }

        public Quaternion Rotaton {
            get { return _cameraTransform.rotation; }
        }

        public bool Initialize () {
            _cameraObject = GameObject.Find (@"Main Camera");

            if (null != _cameraObject) {
                _camera = _cameraObject.GetComponent<UnityEngine.Camera> ();
                _cameraTransform = _cameraObject.transform;

                //XResourceLoaderMgr.SafeDestroy (ref _dummyObject);

                //_dummyObject = XResourceHelper.LoadEditorResourceAtBundleRes<GameObject> ("Prefabs/Cinemachine/DummyCamera", true);
                //_dummyObject = XResourceLoaderMgr.singleton.CreateFromPrefab("Prefabs/DummyCamera") as GameObject;
                //_dummyObject.name = "Dummy Camera";

                //_dummyCamera = _dummyObject.transform.GetChild (0);
                //_ator = _dummyObject.GetComponent<Animator> ();

                //_overrideController.runtimeAnimatorController = _ator.runtimeAnimatorController;
                //_ator.runtimeAnimatorController = _overrideController;

                //_root_pos_inited = false;
                //_idle_root_pos_inited = false;

                //_status = XCameraExStatus.Idle;
                //_status_changed = false;

                //_idle_root_rotation_y = 0;

                //_overrideController["Idle"] = XResourceHelper.LoadEditorResourceAtBundleRes<AnimationClip> ("Animation/Main_Camera/Main_Camera_Idle");

                //TargetOffset = Offset = DefaultDis;
                // RenderingManager.instance.alwaysEnablePostproecss = true;
            }

            return true;
        }

        public void Damp () {
            _damp = true;
            _elapsed = 0;
        }

        public void YRotate (float addation) {
            if (addation != 0) {
                _idle_root_rotation_y += addation;

                _idle_root_rotation = Quaternion.Euler (0, _idle_root_rotation_y, 0);
                _root_pos = _idle_root_rotation * _dummyCamera.position;
            }
        }

        //public void OverrideAnimClip (string motion, string clipname) {
        //    //get override clip
        //    AnimationClip animClip = XResourceLoaderMgr.singleton.GetSharedResource<AnimationClip>(clipname, ".anim");
        //    OverrideAnimClip(motion, animClip);
        //}

        public void OverrideAnimClip (string motion, AnimationClip clip) {
            //override
            if (clip != null && _overrideController[motion] != clip) _overrideController[motion] = clip;
        }

        public void Effect (XCameraMotionData motion) {
            Effect (motion, "ToEffect");
        }

        public void UltraShow () {
            _trigger = "ToUltraShow";

            _motion.Follow_Position = false;
            _motion.Coordinate = CameraMotionSpace.Self;
            _motion.AutoSync_At_Begin = true;
            _motion.LookAt_Target = false;
        }

        public void UltraEnd () {
            _trigger = "ToUltraEnd";

            _motion.Follow_Position = false;
            _motion.Coordinate = CameraMotionSpace.World;
            _motion.AutoSync_At_Begin = true;
            _motion.LookAt_Target = false;
        }

        public void PostUpdate (float fDeltaT) {
            if (!_root_pos_inited) {
                _idle_root_rotation = Quaternion.Euler (0, _idle_root_rotation_y, 0);

                _root_pos = _idle_root_rotation * _dummyCamera.position;
                _root_pos_inited = true;

                if (!_idle_root_pos_inited) {
                    _idle_root_pos = _idle_root_rotation * _dummyCamera.position;
                    _idle_root_pos_inited = true;
                }

                _basic_dis = (_dummyCamera.position - _dummyObject.transform.position).magnitude;
            }

            InnerUpdateEx ();
            TriggerEffect ();

            if (_cur_time < _total_time) {
                _cur_time += (_total_time - _cur_time) > fDeltaT ? fDeltaT : (_total_time - _cur_time);
                TargetOffset = _begin_offest + (_target_offest - _begin_offest) * Mathf.Pow ((_cur_time / _total_time), _factor);
            }
        }

        private void AutoSync () {
            _q_self_r = _hoster.transform.rotation;
            _v_self_p = _hoster.transform.position;
        }

        private void InnerPosition () {
            Vector3 dummyCamera = _dummyCamera.position;
            Vector3 dummyObject = _dummyObject.transform.position;

            dummyCamera.y += CameraOffset;
            dummyObject.y += CameraOffset;


            if (_motion.MotionType == CameraMotionType.CameraBased) {
                Vector3 offset_dir = (dummyCamera - dummyObject);
                float offset_dis = offset_dir.magnitude;
                offset_dir.Normalize ();

                ////if (offset_dir.z > 0)
                ////{
                ////    offset_dis = -offset_dis;
                ////    offset_dir = -offset_dir;
                ////}
                float effect = (offset_dis - _basic_dis);
                float dis = TargetOffset + effect;

                if (dis <= 0) dis = 0.1f;
                dummyCamera = dummyObject + dis * offset_dir;
            }

            _dummyCamera_pos = _idle_root_rotation * (dummyCamera - dummyObject) + (dummyObject);

            if (_damp) {
                _damp_dir = (_dummyCamera_pos - _last_dummyCamera_pos);
                if (_elapsed == 0) _damp_delta = _damp_dir.magnitude;
                _damp_dir.Normalize ();

                if (_elapsed > _damp_factor) {
                    _elapsed = _damp_factor;
                    _damp = false;
                }

                _dummyCamera_pos = _dummyCamera_pos - _damp_dir * (_damp_delta * ((_damp_factor - _elapsed) / _damp_factor));
            }

            _last_dummyCamera_pos = _dummyCamera_pos;
        }

        private void InnerUpdateEx () {
            InnerPosition ();

            Quaternion q_self_r = _hoster.transform.rotation;
            Vector3 v_self_p = _hoster.transform.position;

            Vector3 forward = Vector3.Cross (_dummyCamera.forward, _dummyCamera.up);
            Quaternion _dummyCamera_quat = Quaternion.LookRotation (forward, _dummyCamera.up);
            Vector3 _dummyCamera_rot = _dummyCamera_quat.eulerAngles;

            if (_status_changed || _status == XCameraExStatus.Idle) _status_changed = false;

            Vector3 delta = (_dummyCamera_pos - _root_pos);
            Vector3 target_pos;

            {
                target_pos = (_sync_begin ? _q_self_r : Quaternion.identity) * (_relative_to_idle ? _idle_root_pos : _root_pos);
                delta = (_sync_begin ? _q_self_r : Quaternion.identity) * delta;

                if (!_look_at) _cameraTransform.rotation = (_sync_begin ? _q_self_r : Quaternion.identity) * _idle_root_rotation * _dummyCamera_quat;
            }

            target_pos += (_follow_position ? v_self_p : (_sync_begin ? _v_self_p : Vector3.zero));

            switch (_effect_axis) {
                case CameraMotionSpace.World:
                    {
                        target_pos += delta;
                    }
                    break;
                case CameraMotionSpace.Self:
                    {
                        target_pos += (_follow_position ? Quaternion.identity : q_self_r) * delta;
                    }
                    break;
            }

            _cameraTransform.position = target_pos;
            if (_look_at) _cameraTransform.LookAt (_hoster.transform.position + _dummyObject.transform.position);
        }

        public void EndEffect (object o) {
            if (_status == XCameraExStatus.Idle) return;

            _trigger = "ToIdle";

            _motion.Follow_Position = true;
            _motion.Coordinate = CameraMotionSpace.World;
            _motion.AutoSync_At_Begin = false;
            _motion.LookAt_Target = true;

            _motion.Motion = null;
        }

        public void Effect (XCameraMotionData motion, bool overrideclip) {
            //must be called from UPDATE pass
            //AnimationClip clip = XResourceLoaderMgr.singleton.GetSharedResource<AnimationClip> (motion.Motion3D, ".anim");

            //if (clip != null) {
            //    ResetTargetOffset ();

            //    _trigger = "ToEffect";
            //    if (overrideclip && _overrideController["CameraEffect"] != clip) _overrideController["CameraEffect"] = clip;

            //    _motion.LookAt_Target = motion.LookAt_Target;
            //    _motion.Follow_Position = true;
            //    _motion.Coordinate = CameraMotionSpace.World;

            //    switch (motion.Motion3DType) {
            //        case CameraMotionType.AnchorBased:
            //            {
            //                _motion.AutoSync_At_Begin = true;
            //                _motion.LookAt_Target = false;
            //            }
            //            break;
            //        case CameraMotionType.CameraBased:
            //            {
            //                _motion.AutoSync_At_Begin = false;
            //            }
            //            break;
            //    }

            //    _motion.Motion = motion.Motion3D;
            //}
        }

        public void Effect (XCameraMotionData motion, string trigger) {
            //must be called from UPDATE pass
            AnimationClip clip = XResourceHelper.LoadEditorResourceAtBundleRes<AnimationClip> (motion.Motion3D);

            if (clip != null) {
                _trigger = trigger;

                _motion.LookAt_Target = motion.LookAt_Target;
                _motion.Follow_Position = true;
                _motion.Coordinate = CameraMotionSpace.World;

                switch (motion.Motion3DType) {
                    case CameraMotionType.AnchorBased:
                        {
                            _motion.AutoSync_At_Begin = true;
                            _motion.LookAt_Target = false;
                        }
                        break;
                    case CameraMotionType.CameraBased:
                        {
                            _motion.AutoSync_At_Begin = false;
                        }
                        break;
                }

                _motion.Motion = motion.Motion3D;
            }
        }

        private void TriggerEffect () {
            if (_trigger != null && !_ator.IsInTransition (0)) {
                switch (_trigger) {
                    case "ToIdle":
                        {
                            _status = XCameraExStatus.Idle;
                            _idle_root_pos_inited = false;
                        }
                        break;
                    case "ToEffect":
                        _status = XCameraExStatus.Effect;
                        break;
                    case "ToDash":
                        _status = XCameraExStatus.Dash;
                        break;
                    case "ToUltraShow":
                        _status = XCameraExStatus.UltraShow;
                        break;
                    case "ToUltraEnd":
                        _status = XCameraExStatus.UltraEnd;
                        break;
                }

                XTimerMgr.singleton.KillTimer (_token);

                _follow_position = _motion.Follow_Position;
                _effect_axis = _motion.Coordinate;
                _sync_begin = _motion.AutoSync_At_Begin;
                _look_at = _motion.LookAt_Target;

                if (_sync_begin) AutoSync ();

                _ator.SetTrigger (_trigger);
                _root_pos_inited = false;

                _status_changed = true;
                _trigger = null;
            }
        }

        private float _total_time = 0f;
        private float _cur_time = 0f;
        private float _begin_offest = 0f;
        private float _target_offest = 0f;
        private float _factor = 1f;

        public void SetTargetOffset (float target, float time, float factor) {
            _total_time = time;
            _cur_time = 0f;
            _begin_offest = TargetOffset;
            _target_offest = target * DefaultDis;
            _factor = factor;
        }

        public void ResetTargetOffset () {
            TargetOffset = DefaultDis;
            _total_time = 0f;
            _cur_time = 0f;
            _begin_offest = 0f;
            _target_offest = 0f;
            _factor = 1f;
        }
    }
}
#endif