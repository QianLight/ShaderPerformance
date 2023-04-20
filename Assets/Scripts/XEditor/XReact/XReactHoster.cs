//#if UNITY_EDITOR
//using System;
//using System.Collections.Generic;
//using CFEngine;
//using CFUtilPoolLib;
//using FMODUnity;
//using UnityEditor;
//using UnityEditorInternal;
//using UnityEngine;

//namespace XEditor
//{
    

//    public class XReactHoster : MonoBehaviour
//    {
//        [SerializeField]
//        private XReactData _xData = null;
//        [SerializeField]
//        private XReactDataExtra _xDataExtra = null;
//        [SerializeField]
//        private XReactConfigData _xConfigData = null;
//        [SerializeField]
//        private XReactEditorData _xEditorData = null;

//        public XReactData ReactData
//        {
//            get
//            {
//                if (_xData == null) _xData = new XReactData ();
//                return _xData;
//            }
//            set
//            {
//                //for load data from file.
//                _xData = value;
//            }
//        }

//        public XReactDataExtra ReactDataExtra
//        {
//            get
//            {
//                if (_xDataExtra == null) _xDataExtra = new XReactDataExtra ();
//                return _xDataExtra;
//            }
//            set
//            {
//                _xDataExtra = value;
//            }
//        }

//        public XReactConfigData ConfigData
//        {
//            get
//            {
//                if (_xConfigData == null) _xConfigData = new XReactConfigData ();
//                return _xConfigData;
//            }
//            set
//            {
//                //for load data from file.
//                _xConfigData = value;
//            }
//        }

//        public XReactEditorData EditorData
//        {
//            get
//            {
//                if (_xEditorData == null) _xEditorData = new XReactEditorData ();
//                return _xEditorData;
//            }
//        }

//        [HideInInspector]
//        public AnimatorOverrideController oVerrideController = null;
//        private Animator _ator = null;

//        public XReactBoneShake BoneShakeCom = new XReactBoneShake();
//        public XReactPlable PlableCom = new XReactPlable();

//        private bool _execute = false;
//        private bool _anim_init = false;
//        private bool _has_anim = true;
//        //private bool _is_additvie = false;

//        public enum DummyState { Idle, Fire }
//        public DummyState _state = DummyState.Idle;
//        private DummyState _pre_state = DummyState.Idle;
//        public float _state_elapsed = 0;
//        public float _state_loop = 0;

//        private float _delta = 0;
//        private float _fire_time = 0;

//        public XEntityPresentation.RowData _present_data = null;

//        protected List<XFx> _fx = new List<XFx> ();

//        private List<uint> _presentToken = new List<uint> ();

//        AudioSource _audio_motion = null;
//        AudioSource _audio_action = null;
//        AudioSource _audio_skill = null;
//        AudioSource _audio_behit = null;
//        XFmod _emitter = null;

//        private GameObject _cameraObject = null;
//        private Transform _cameraTransform = null;
//        private UnityEngine.Camera _camera = null;

//        float rotateSpeed = 15f;
//        float rotate;
//        //float smoothing = 1f;
//        public float offsetDistance;
//        public float offsetHeight = 1.5f;
//        private Vector3 cam_offset;
//        private Vector3 lastPosition;

//        private float _action_framecount = 0;
//        private Rect _rect = new Rect (20, 20, 150, 40);

//        private GUIStyle _style = new GUIStyle ();

//        EffectPreviewContext previewContext = null;

//        void OnGUI ()
//        {
//            GUI.Label (_rect, "Action Frame: " + _action_framecount, _style);
//        }

//        void Start ()
//        {
//            _style.normal.textColor = Color.black;
//            _style.fontSize = 18;

//            _state = DummyState.Idle;
//            if (oVerrideController == null) BuildOverride ();

//            //> set camera
//            _cameraObject = GameObject.Find (@"Main Camera");
//            if (_cameraObject != null)
//            {
//                _camera = _cameraObject.GetComponent<UnityEngine.Camera> ();
//                _cameraTransform = _cameraObject.transform;

//                offsetDistance = XGloabelConfLibrary.CameraEditorDefaultDis;
//                lastPosition = new Vector3 (transform.position.x, transform.position.y + offsetHeight, transform.position.z - offsetDistance);
//                cam_offset = new Vector3 (transform.position.x, transform.position.y + offsetHeight, transform.position.z - offsetDistance);
//            }

//            RebuildAniamtion ();
//            Application.targetFrameRate = 60;

//            BoneShakeCom.Init(transform);
//            PlableCom.Init(_ator);

//            //_is_additvie = _xData.IsAdditive;
//        }

//        private void OnDisable()
//        {
//            OnEndShader();

//            if (_has_anim)
//            {
//                PlableCom.OnDisable();
//            }
//        }

//        void UpdateCamera ()
//        {

//            if (Input.GetKey (KeyCode.Q))
//            {
//                if (rotate == 0) rotate = -1;
//            }
//            else if (Input.GetKey (KeyCode.E))
//            {
//                if (rotate == 0) rotate = 1;
//            }
//            else
//            {
//                rotate = 0;
//            }

//            cam_offset = Quaternion.AngleAxis (rotate * rotateSpeed, Vector3.up) * cam_offset;
//            _cameraTransform.position = transform.position + cam_offset;
//            //_cameraTransform.position = new Vector3 (Mathf.Lerp (lastPosition.x, transform.position.x + cam_offset.x, smoothing * Time.deltaTime),
//            //    Mathf.Lerp (lastPosition.y, transform.position.y + cam_offset.y, smoothing * Time.deltaTime),
//            //    Mathf.Lerp (lastPosition.z, transform.position.z + cam_offset.z, smoothing * Time.deltaTime));

//            _cameraTransform.LookAt (transform.position);
//        }

//        void LateUpdateCamera ()
//        {
//            lastPosition = _cameraTransform.position;
//        }

//        private void BuildOverride ()
//        {
//            oVerrideController = new AnimatorOverrideController ();

//            _ator = GetComponent<Animator> ();
//            if (_ator == null)
//            {
//                _ator = gameObject.AddComponent<Animator> ();
//                _ator.runtimeAnimatorController = AssetDatabase.LoadAssetAtPath<UnityEditor.Animations.AnimatorController> ("Assets/BundleRes/Controller/XAnimator.controller");
//            }
//            oVerrideController.runtimeAnimatorController = _ator.runtimeAnimatorController;
//            _ator.runtimeAnimatorController = oVerrideController;

//        }

//        public void RebuildAniamtion ()
//        {
//            AnimationClip clip = XResourceHelper.LoadEditorResourceAtBundleRes<AnimationClip> (ReactData.ClipName);

//            if (oVerrideController == null) BuildOverride ();

//            _present_data = XAnimationLibrary.AssociatedAnimations ((uint) _xConfigData.Player);

//            oVerrideController["Idle"] = XResourceHelper.LoadEditorResourceAtBundleRes<AnimationClip> ("Animation/" + _present_data.AnimLocation + _present_data.AttackIdle);

//            _state_loop = oVerrideController["Idle"].length;
//        }

//        private void Update ()
//        {
//            XTimerMgr.singleton.Update (Time.deltaTime);
//            EngineContext context = EngineContext.instance;
//            if (context != null)
//                XFxMgr.singleton.PostUpdate (context);

//            if (_pre_state != _state)
//            {
//                switch (_state)
//                {
//                    case DummyState.Idle:
//                        {
//                            _state_loop = oVerrideController["Idle"].length;
//                        }
//                        break;
 
//                }
//                if (_pre_state != DummyState.Fire) _state_elapsed = 0;
//            }
//            _pre_state = _state;

//            if (_state != DummyState.Fire)
//            {
//                _action_framecount = 0;

//                //fire skill
//                if (Input.GetKeyDown (KeyCode.Space))
//                {
//                    //oVerrideController[_is_additvie ? "ReactAdditive" : "React"] = XResourceHelper.LoadEditorResourceAtBundleRes<AnimationClip> (_xData.ClipName);

//                    Fire ();
//                }
//                else
//                {
//                    _state = DummyState.Idle;
//                }
//            }
//            else
//            {

//                if (_execute)
//                {
//                    _delta += Time.deltaTime;
//                    _action_framecount = _delta / XCommon.singleton.FrameStep;

//                    if (_delta > _xData.Time)
//                    {
//                        StopFire ();
//                    }
//                }

//                if (_anim_init)
//                    Execute ();

//                _anim_init = false;
//            }

    

//            UpdateCamera ();
//        }

//        void LateUpdate ()
//        {
//            _state_elapsed += Time.deltaTime;
//            if (_state_elapsed > _state_loop) _state_elapsed = 0;

//            if (_has_anim)
//                _ator.speed = _xData == null ? 1 : 1 / _xData.AnimationSpeedRatio;
//            if (_ator.speed == 0)
//                _ator.speed = 1;

//            if (BoneShakeCom != null)
//                BoneShakeCom.LateUpdate(Time.deltaTime);

//            LateUpdateCamera ();


//            RenderEffectSystem.UpdateEffect(Time.deltaTime);
//        }

//        private void Execute ()
//        {
//            _execute = true;

//            _fire_time = Time.time;

//            if (_xData.Fx != null)
//            {
//                foreach (XFxData data in _xData.Fx)
//                {
//                    AddedTimerToken (XTimerMgr.singleton.SetTimer (data.At, Fx, data));
//                }
//            }

//            if (_xData.Audio != null)
//            {
//                foreach (XAudioData data in _xData.Audio)
//                {
//                    AddedTimerToken(XTimerMgr.singleton.SetTimer(data.At, Audio, data));
//                }
//            }

//            if (_xData.BoneShakeData != null)
//            {
//                foreach(XBoneShakeData data in _xData.BoneShakeData)
//                {
//                    AddedTimerToken(XTimerMgr.singleton.SetTimer(data.At, BoneShake, data));
//                }
//            }

//            if (_xData.ShaderEffData != null)
//            {
//                foreach (XShaderEffData data in _xData.ShaderEffData)
//                {
//                    AddedTimerToken(XTimerMgr.singleton.SetTimer(data.At, PlayShader, data));
//                }
//            }
//        }

//        private void AddedTimerToken (uint token)
//        {
//            _presentToken.Add (token);
//        }

//        private void Fire ()
//        {
//            _fx.Clear ();

//            _state = DummyState.Fire;

//            _has_anim = !string.IsNullOrEmpty(_xData.ClipName);
//            AvatarMask mask = null;
//            if (!string.IsNullOrEmpty(_xData.AvatarMask))
//            {
//                mask = XResourceHelper.LoadEditorResourceAtBundleRes<AvatarMask>(_xData.AvatarMask);
//            }
//            if (_has_anim)
//            {
//                PlableCom.SetLayerAdditive(_xData.AnimLayerType == EReactAnimLayerType.Additive);
//                PlableCom.Play(XResourceHelper.LoadEditorResourceAtBundleRes<AnimationClip>(_xData.ClipName), mask);
//                PlableCom.SetLayerWeight(_xData.LayerWeight);
//            }


//            _anim_init = true;
//            _delta = 0;
//        }

//        private void StopFire ()
//        {
//            if (_state != DummyState.Fire) return;

//            _state = DummyState.Idle;

//            PlableCom.StopReactAnim();

//            _execute = false;

//            for (int i = 0; i < _fx.Count; i++)
//                XFxMgr.singleton.DestroyFx (_fx[i], false);
//            _fx.Clear ();

//            if (_xData.Audio != null)
//            {
//                foreach (XAudioData data in _xData.Audio)
//                {
//                    AudioSource source = GetAudioSourceByChannel (data.Channel);
//                    source.Stop ();
//                }
//            }

//            foreach (uint token in _presentToken)
//            {
//                XTimerMgr.singleton.KillTimer (token);
//            }

//            _presentToken.Clear ();

//            BoneShakeCom.OnShakeEvent(0);

//            Time.timeScale = 1;

//            OnEndShader();
//        }

//        private void Fx (object param)
//        {

//            XFxData data = param as XFxData;

//            if (data.Shield) return;

//            Transform trans = transform;
//            Vector3 offset = new Vector3 (data.OffsetX, data.OffsetY, data.OffsetZ);

//            XFx fx = XFxMgr.singleton.CreateFx (data.Fx);
//            fx.DelayDestroy = data.Destroy_Delay;

//            if (data.StickToGround)
//            {
//                switch (data.Type)
//                {
//                    case SkillFxType.FirerBased:
//                        {

//                        }
//                        break;
//                    case SkillFxType.TargetBased:
//                        {
//                            //if (_xData.NeedTarget && _target != null)
//                            //{
//                            //    trans = _target.transform;
//                            //    offset = new Vector3(data.Target_OffsetX, data.Target_OffsetY, data.Target_OffsetZ);
//                            //}
//                        }
//                        break;
//                }

//                Vector3 pos = trans.position + trans.rotation * offset;
//                pos.y = 0;

//                fx.Play (pos, Quaternion.identity, new Vector3 (data.ScaleX, data.ScaleY, data.ScaleZ));
//            }
//            else
//            {
//                switch (data.Type)
//                {
//                    case SkillFxType.FirerBased:
//                        {
//                            if (data.Bone != null && data.Bone.Length > 0)
//                            {
//                                Transform attachPoint = trans.Find (data.Bone);
//                                if (attachPoint != null)
//                                {
//                                    trans = attachPoint;
//                                }
//                                else
//                                {
//                                    int index = data.Bone.LastIndexOf ("/");
//                                    if (index >= 0)
//                                    {
//                                        string bone = data.Bone.Substring (index + 1);
//                                        attachPoint = trans.Find (bone);
//                                        if (attachPoint != null)
//                                        {
//                                            trans = attachPoint;
//                                        }
//                                    }
//                                }
//                            }
//                        }
//                        break;
//                    case SkillFxType.TargetBased:
//                        {
//                            //if (_current.NeedTarget && _target != null)
//                            //{
//                            //    trans = _target.transform;
//                            //    offset = new Vector3(data.Target_OffsetX, data.Target_OffsetY, data.Target_OffsetZ);
//                            //}
//                        }
//                        break;
//                }

//                if (!data.Alone)
//                    fx.Play (trans, offset, new Vector3 (data.ScaleX, data.ScaleY, data.ScaleZ), 1, data.Follow);
//                else
//                {
//                    Vector3 pos = transform.position + transform.rotation * offset;
//                    Quaternion rot = Quaternion.Euler (data.RotX, data.RotY, data.RotZ);
//                    fx.Play (pos, rot, new Vector3 (data.ScaleX, data.ScaleY, data.ScaleZ));
//                }
//            }

//            if (data.End > 0)
//                AddedTimerToken (XTimerMgr.singleton.SetTimer (data.End - data.At, KillFx, fx));
//            _fx.Add (fx);
//        }

//        private void KillFx (object o)
//        {
//            XFx fx = o as XFx;

//            _fx.Remove (fx);

//            XFxMgr.singleton.DestroyFx (fx, false);
//        }

//        private void Audio (object param)
//        {
//            XAudioData data = param as XAudioData;

//            if (data.Channel == AudioChannel.Behit) return;

//            if (_emitter == null)
//                _emitter = gameObject.AddComponent<XFmod> ();

//            _emitter.StartEvent ("event:/" + data.Clip, data.Channel);
//        }


//        private AudioSource GetAudioSourceByChannel (AudioChannel channel)
//        {
//            switch (channel)
//            {
//                case AudioChannel.Action:
//                    {
//                        if (_audio_action == null)
//                            _audio_action = gameObject.AddComponent<AudioSource> ();

//                        return _audio_action;
//                    }
//                case AudioChannel.Motion:
//                    {
//                        if (_audio_motion == null)
//                            _audio_motion = gameObject.AddComponent<AudioSource> ();

//                        return _audio_motion;
//                    }
//                case AudioChannel.Skill:
//                    {
//                        if (_audio_skill == null)
//                            _audio_skill = gameObject.AddComponent<AudioSource> ();

//                        return _audio_skill;
//                    }
//                case AudioChannel.Behit:
//                    {
//                        if (_audio_behit == null)
//                            _audio_behit = gameObject.AddComponent<AudioSource> ();

//                        return _audio_behit;
//                    }
//            }

//            return _audio_action;
//        }

//        private void BoneShake(object param)
//        {
//            XBoneShakeData data = param as XBoneShakeData;

//            if (BoneShakeCom != null)
//                BoneShakeCom.OnShakeEvent(1, data);
//        }

//        private EffectPreviewContext GetPlayerPreviewContext()
//        {
//            if (previewContext == null)
//                previewContext = new EffectPreviewContext()
//                {
//                    ere = new EditorRenderEffect(),
//                    processByPart = true,
//                };
//            if (!previewContext.entityInit)
//            {
//                EffectConfig.InitEffect(previewContext, gameObject);
//            }
//            return previewContext;
//        }

//        private void PlayShader(object o)
//        {
//            XShaderEffData data = o as XShaderEffData;
//            EffectPreviewContext previewContext = GetPlayerPreviewContext();
//            if (previewContext != null)
//            {
//                if (data != null)
//                {
//                    previewContext.ere.effect.data = new Vector4(data.Data0, data.Data1, data.Data2, data.Data3);
//                    previewContext.ere.effect.data1 = new Vector4(data.FadeIn, data.FadeOut, data.LifeTime, data.Param);
//                    //RenderEffectSystem.InitEffect(previewContext.renderComponent, data.UniqueID, ref previewContext.ere.effect, true);
//                }
//            }

//        }

//        void OnEndShader()
//        {
//            //if (previewContext != null)
//            //    RenderEffectSystem.EndEffect(previewContext.renderComponent, -1);
//        }

//    }
//}
//#endif