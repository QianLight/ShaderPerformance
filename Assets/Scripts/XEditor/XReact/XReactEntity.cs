#if UNITY_EDITOR
using CFEngine;
using CFUtilPoolLib;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace XEditor
{
    public class XReactEntity : MonoBehaviour
    {
        [HideInInspector]
        public XReactDataSet ReactDataSet = null;

        public BoneShake boneShake = null;

        [HideInInspector]
        public AnimatorOverrideController oVerrideController = null;
        private Animator _ator = null;
        public XEntityPresentation.RowData _present_data = null;

        //public XReactBoneShake BoneShakeCom = new XReactBoneShake ();
        public XReactPlable PlableCom = new XReactPlable ();

        private AnimationClip TestCurrentAnim;

        public enum DummyState { Idle, Fire }
        public DummyState _state = DummyState.Idle;
        private DummyState _pre_state = DummyState.Idle;
        public float _state_elapsed = 0;
        public float _state_loop = 0;

        private bool _execute = false;
        private float _delta = 0;
        private float _fire_time = 0;
        private float _action_framecount = 0;

        private GameObject _cameraObject = null;
        private Transform _cameraTransform = null;
        private UnityEngine.Camera _camera = null;

        //public float offsetDistance;
        //public float offsetHeight = 1.5f;
        //private Vector3 cam_offset;
        //private Vector3 lastPosition;
        private Transform swivel = null;
        float rotationAngle;
        public float rotationSpeed = 180f;

        public float TestSpeed = 1f;
        private float Pitch = 1f;

        public bool ShakeIgnoreBone = true;

        private bool _has_anim = false;
        private List<SFX> _fx = new List<SFX> ();

        private List<uint> _presentToken = new List<uint> ();

        AudioSource _audio_motion = null;
        AudioSource _audio_action = null;
        AudioSource _audio_skill = null;
        AudioSource _audio_behit = null;
        AudioSource _audio_batibehit = null;
        XFmod _emitter = null;

        EffectPreviewContext previewContext = null;

        private Rect _rect = new Rect (20, 20, 150, 40);
        private GUIStyle _style = new GUIStyle ();
        void OnGUI ()
        {
            GUI.Label (_rect, "Action Frame: " + _action_framecount, _style);
        }

        // Start is called before the first frame update
        void Start ()
        {
            //_style.normal.textColor = Color.black;
            _style.fontSize = 18;

            _state = DummyState.Idle;
            if (oVerrideController == null)
            {
                oVerrideController = new AnimatorOverrideController ();

                _ator = GetComponent<Animator> ();
                if (_ator == null)
                {
                    _ator = gameObject.AddComponent<Animator> ();
                    _ator.runtimeAnimatorController = AssetDatabase.LoadAssetAtPath<UnityEditor.Animations.AnimatorController> ("Assets/BundleRes/Controller/XAnimator.controller");
                }
                oVerrideController.runtimeAnimatorController = _ator.runtimeAnimatorController;
                _ator.runtimeAnimatorController = oVerrideController;
            }

            //> set camera
            _cameraObject = GameObject.Find (@"Main Camera");
            if (_cameraObject != null)
            {
                _camera = _cameraObject.GetComponent<UnityEngine.Camera> ();
                _cameraTransform = _cameraObject.transform;
                swivel = _cameraTransform.parent.parent;

                //offsetDistance = XGloabelConfLibrary.CameraEditorDefaultDis;
                //lastPosition = new Vector3(transform.position.x, transform.position.y + offsetHeight, transform.position.z - offsetDistance);
                //cam_offset = new Vector3(transform.position.x, transform.position.y + offsetHeight, transform.position.z - offsetDistance);
            }

            RebuildAniamtion ();
            Application.targetFrameRate = EnvironmentExtra.frameRate.Value;

            //BoneShakeCom.Init (transform);
            PlableCom.Init (_ator);
        }

        public void ChangeAnim (AnimationClip clip)
        {
            if (clip == null)
            {
                if (_present_data != null)
                    clip = XResourceHelper.LoadEditorResourceAtBundleRes<AnimationClip> ("Animation/" + _present_data.AnimLocation + _present_data.AttackIdle);
            }

            if (clip != null)
            {
                oVerrideController["Idle"] = clip;
                _state_loop = clip.length;

                TestCurrentAnim = clip;
            }
        }

        public void RebuildAniamtion ()
        {
            if (ReactDataSet == null) return;

            AnimationClip clip = XResourceHelper.LoadEditorResourceAtBundleRes<AnimationClip> (ReactDataSet.ReactData.ClipName);

            _present_data = XAnimationLibrary.AssociatedAnimations ((uint) ReactDataSet.ConfigData.Player);

            if (TestCurrentAnim == null)
                TestCurrentAnim = XResourceHelper.LoadEditorResourceAtBundleRes<AnimationClip> ("Animation/" + _present_data.AnimLocation + _present_data.AttackIdle);
            oVerrideController["Idle"] = TestCurrentAnim;

            if (TestCurrentAnim != null)
                _state_loop = TestCurrentAnim.length;
            else
            {
                Debug.LogError ("clip is null");
            }
        }

        private void Update ()
        {
            XTimerMgr.singleton.Update (Time.deltaTime);
            // EngineContext context = EngineContext.instance;
            // if (context != null)
            //     XFxMgr.singleton.PostUpdate (context);

            if (_pre_state != _state)
            {
                switch (_state)
                {
                    case DummyState.Idle:
                        {
                            if (TestCurrentAnim != null)
                                _state_loop = TestCurrentAnim.length;
                        }
                        break;

                }
                if (_pre_state != DummyState.Fire) _state_elapsed = 0;
            }
            _pre_state = _state;

            //fire skill
            if (Input.GetKeyDown (KeyCode.Space))
            {
                if (_state == DummyState.Fire)
                {
                    StopFire();
                }

                _action_framecount = 0;
                Fire ();
            }

            if (Input.GetKeyDown (KeyCode.J))
            {
                PlableCom.DPlay ();
            }

            //if (Input.GetKeyDown(KeyCode.LeftControl))
            {
                float w = Input.GetAxis ("Vertical");
                Pitch += w;
                Pitch = Mathf.Clamp01 (Pitch);
                PlableCom.SetMixInputWeight (Pitch);
            }

            if (_state != DummyState.Fire)
            {
                _action_framecount = 0;

                //fire skill
                //if (Input.GetKeyDown(KeyCode.Space))
                //{
                //    Fire();
                //}
                //else
                {
                    _state = DummyState.Idle;
                }
            }
            else
            {
                if (_execute)
                {
                    _delta += Time.deltaTime;
                    _action_framecount = _delta / XCommon.singleton.FrameStep;

                    if (_delta > ReactDataSet.ReactData.Time)
                    { 
                        StopFire ();
                    }
                }
            }
            if(PlableCom != null)
            {
                PlableCom.Update(Time.deltaTime);
            }

            UpdateCamera ();
            if (previewContext != null)
            {
                previewContext.Update ();
            }
            if(boneShake != null)
                boneShake.Update();
        }

        void LateUpdate ()
        {
            _state_elapsed += Time.deltaTime;
            if (_state_elapsed > _state_loop) _state_elapsed = 0;

            //if (BoneShakeCom != null)
            //    BoneShakeCom.LateUpdate (Time.deltaTime);

            LateUpdateCamera ();
            if (boneShake != null)
                boneShake.LateUpdate();
        }

        void UpdateCamera ()
        {
            float rotationDelta = Input.GetAxis ("Horizontal");
            if (rotationDelta != 0f)
            {
                AdjustRotation (rotationDelta);
            }
        }

        void LateUpdateCamera ()
        {

        }

        void AdjustRotation (float delta)
        {
            rotationAngle += delta * rotationSpeed * Time.deltaTime;
            if (rotationAngle < 0f)
            {
                rotationAngle += 360f;
            }
            else if (rotationAngle >= 360f)
            {
                rotationAngle -= 360f;
            }
            if(swivel != null)
                swivel.localRotation = Quaternion.Euler (0f, rotationAngle, 0f);
        }

        void Fire ()
        {
            _fx.Clear ();

            _state = DummyState.Fire;

            _has_anim = !string.IsNullOrEmpty (ReactDataSet.ReactData.ClipName);
            AvatarMask mask = null;
            if (!string.IsNullOrEmpty (ReactDataSet.ReactData.AvatarMask))
            {
                mask = XResourceHelper.LoadEditorResourceAtBundleRes<AvatarMask> (ReactDataSet.ReactData.AvatarMask);
            }
            if (_has_anim)
            {
                PlableCom.SetLayerAdditive (ReactDataSet.ReactData.AnimLayerType == EReactAnimLayerType.Additive);
                AnimationClip clip = XResourceHelper.LoadEditorResourceAtBundleRes<AnimationClip> (ReactDataSet.ReactData.ClipName);
                AnimationClip clip2 = string.IsNullOrEmpty (ReactDataSet.ReactData.ClipName2) ? null : XResourceHelper.LoadEditorResourceAtBundleRes<AnimationClip> (ReactDataSet.ReactData.ClipName2);
                PlableCom.Play (clip, clip2, mask);
                PlableCom.SetLayerWeight (ReactDataSet.ReactData.LayerWeight);
                PlableCom.SetSpeed (1f/ReactDataSet.ReactData.AnimationSpeedRatio);

                PlableCom.BeginCrossFade(ReactDataSet.ReactData.FadeTime, ReactDataSet.ReactData.FadeTimeOut, ReactDataSet.ReactData.Time, ReactDataSet.ReactData.LayerWeight);
                //AddedTimerToken(XTimerMgr.singleton.SetTimer((ReactDataSet.ReactData.Time - ReactDataSet.ReactData.FadeTimeOut) , Ani, ReactDataSet));
            }

            _delta = 0;

            FireEvents ();
        }

        void StopFire ()
        {

            if (_state != DummyState.Fire) return;

            _state = DummyState.Idle;

            PlableCom.StopReactAnim ();

            _execute = false;
            var sfxMgr = SFXMgr.singleton;
            for (int i = 0; i < _fx.Count; i++)
            {
                var sfx = _fx[i];

                sfxMgr.Destroy (ref sfx);
            }
            _fx.Clear ();

            if (ReactDataSet.ReactData.Audio != null)
            {
                foreach (XAudioData data in ReactDataSet.ReactData.Audio)
                {
                    AudioSource source = GetAudioSourceByChannel (data.Channel);
                    source.Stop ();
                }
            }

            foreach (uint token in _presentToken)
            {
                XTimerMgr.singleton.KillTimer (token);
            }

            _presentToken.Clear ();

            //BoneShakeCom.OnShakeEvent (0);

            Time.timeScale = 1;

            OnEndShader ();
        }

        private void FireEvents ()
        {
            _execute = true;

            _fire_time = Time.time;

            if (ReactDataSet.ReactData.Fx != null)
            {
                for (int i = 0; i < ReactDataSet.ReactData.Fx.Count; i++)
                {
                    XFxData data = ReactDataSet.ReactData.Fx[i];
                    AddedTimerToken(XTimerMgr.singleton.SetTimer(data.At, Fx, new List<object>() { data , i}));
                }
                //foreach (XFxData data in ReactDataSet.ReactData.Fx)
                //{
                //    AddedTimerToken (XTimerMgr.singleton.SetTimer (data.At, Fx, data));
                //}
            }

            if (ReactDataSet.ReactData.Audio != null)
            {
                foreach (XAudioData data in ReactDataSet.ReactData.Audio)
                {
                    AddedTimerToken (XTimerMgr.singleton.SetTimer (data.At, Audio, data));
                }
            }

            if (ReactDataSet.ReactData.BoneShakeData != null)
            {
                for (int i = 0; i < ReactDataSet.ReactData.BoneShakeData.Count; i++)
                {
                    XBoneShakeData data = ReactDataSet.ReactData.BoneShakeData[i];
                    XBoneShakeExtra dataE = ReactDataSet.ReactDataExtra.BoneShake[i];
                    AddedTimerToken(XTimerMgr.singleton.SetTimer(data.At, BoneShake, dataE));
                }
            }

            if (ReactDataSet.ReactData.ShaderEffData != null)
            {
                foreach (XShaderEffData data in ReactDataSet.ReactData.ShaderEffData)
                {
                    AddedTimerToken (XTimerMgr.singleton.SetTimer (data.At, PlayShader, data));
                }
            }
        }

        private void AddedTimerToken (uint token)
        {
            _presentToken.Add (token);
        }

        //private void Ani(object param)
        //{
        //    XReactDataSet reactDataSet = param as XReactDataSet;
        //    //PlableCom.BeginCrossFade(reactDataSet.ReactData.FadeTime, reactDataSet.ReactData.LayerWeight);
        //}
        private void Fx (object param)
        {
            List<object> ll = param as List<object>;
            XFxData data = ll[0] as XFxData;
            int index = (int)ll[1];

            if (data.Shield) return;

            Transform trans = transform;
            Vector3 offset = new Vector3 (data.OffsetX, data.OffsetY, data.OffsetZ);

            SFX fx = SFXMgr.singleton.Create (data.Fx);
            fx.InitDuration = data.Destroy_Delay;

            if (data.StickToGround)
            {
                switch (data.Type)
                {
                    case SkillFxType.FirerBased:
                        {

                        }
                        break;
                    case SkillFxType.TargetBased:
                        {
                            //if (_xData.NeedTarget && _target != null)
                            //{
                            //    trans = _target.transform;
                            //    offset = new Vector3(data.Target_OffsetX, data.Target_OffsetY, data.Target_OffsetZ);
                            //}
                        }
                        break;
                }

                Vector3 pos = trans.position + trans.rotation * offset;
                pos.y = 0;

                fx.SetPos (ref pos);
                var scale = new Vector3 (data.ScaleX, data.ScaleY, data.ScaleZ);
                fx.SetScale (ref scale);
                fx.Play ();
                // fx.Play(pos, Quaternion.identity, new Vector3(data.ScaleX, data.ScaleY, data.ScaleZ));
            }
            else if(data.BlowEnergy)
            {
                float hitDirection = ReactDataSet.ReactDataExtra.Fx[index].hitDirection;

                //XFx fx;
                Vector3 fxScale = new Vector3(data.ScaleX, data.ScaleY, data.ScaleZ);

                if (hitDirection > -360)
                {
                    hitDirection += 180;
                    var present = _present_data;

                    if (present.HugeMonsterColliders.Count == 0)
                    {
                        offset = XCommon.singleton.HorizontalRotateVetor3(
                            Vector3.forward * present.BoundRadius * present.Scale +
                            new Vector3(data.OffsetX, data.OffsetY, data.OffsetZ),
                            hitDirection, false);
                        offset = XCommon.singleton.HorizontalRotateVetor3(offset, -transform.rotation.eulerAngles.y, false);
                        Vector3 pos = transform.position;
                    }


                    fxScale.x *= present.HitFxScale[0] == 0 ? 1 : present.HitFxScale[0];
                    fxScale.y *= present.HitFxScale[1] == 0 ? 1 : present.HitFxScale[1];
                    fxScale.z *= present.HitFxScale[2] == 0 ? 1 : present.HitFxScale[2];
                }
                else
                {
                    offset = new Vector3(data.OffsetX, data.OffsetY, data.OffsetZ);
                }
                Vector3 HitOffset = ReactDataSet.ReactDataExtra.Fx[index].HitOffset;
                Vector3 eulerAngles = new Vector3(HitOffset.x,
                            HitOffset.y + hitDirection,
                            HitOffset.z);
                    fx.SetRot(ref eulerAngles, true);
                    fx.SetPos(ref offset);
                    fx.SetScale(ref fxScale);
                    fx.flag.SetFlag(SFX.Flag_Follow, data.Follow);
                    fx.InitDuration = ReactDataSet.ReactData.Time;
                    fx.FadeOut = data.Destroy_Delay;
                    fx.Play();
            }
            else
            {
                switch (data.Type)
                {
                    case SkillFxType.FirerBased:
                        {
                            // if (data.Bone != null && data.Bone.Length > 0)
                            // {
                            //     Transform attachPoint = trans.Find(data.Bone);
                            //     if (attachPoint != null)
                            //     {
                            //         trans = attachPoint;
                            //     }
                            //     else
                            //     {
                            //         int index = data.Bone.LastIndexOf("/");
                            //         if (index >= 0)
                            //         {
                            //             string bone = data.Bone.Substring(index + 1);
                            //             attachPoint = trans.Find(bone);
                            //             if (attachPoint != null)
                            //             {
                            //                 trans = attachPoint;
                            //             }
                            //         }
                            //     }
                            // }
                        }
                        break;
                    case SkillFxType.TargetBased:
                        {
                            //if (_current.NeedTarget && _target != null)
                            //{
                            //    trans = _target.transform;
                            //    offset = new Vector3(data.Target_OffsetX, data.Target_OffsetY, data.Target_OffsetZ);
                            //}
                        }
                        break;
                }

                if (!data.Alone)
                {
                    fx.SetParent (trans, data.Bone);
                    fx.SetPos (ref offset);
                    var scale = new Vector3 (data.ScaleX, data.ScaleY, data.ScaleZ);
                    fx.SetScale (ref scale);
                    fx.flag.SetFlag (SFX.Flag_Follow, data.Follow);
                    fx.Play ();
                    //fx.Play (trans, offset, new Vector3 (data.ScaleX, data.ScaleY, data.ScaleZ), 1, data.Follow);
                }
                else
                {
                    Vector3 pos = transform.position + transform.rotation * offset;
                    Quaternion rot = Quaternion.Euler (data.RotX, data.RotY, data.RotZ);
                    fx.SetPos (ref pos);
                    fx.SetRot (ref rot);
                    var scale = new Vector3 (data.ScaleX, data.ScaleY, data.ScaleZ);
                    fx.SetScale (ref scale);
                    fx.Play ();
                    // fx.Play (pos, rot, new Vector3 (data.ScaleX, data.ScaleY, data.ScaleZ));
                }
            }

            if (data.End > 0)
                AddedTimerToken (XTimerMgr.singleton.SetTimer (data.End - data.At, KillFx, fx));
            _fx.Add (fx);
        }

        private void KillFx (object o)
        {
            SFX fx = o as SFX;

            _fx.Remove (fx);

            SFXMgr.singleton.Destroy (ref fx);
        }

        private void Audio (object param)
        {
            XAudioData data = param as XAudioData;

            if (data.Channel == AudioChannel.Behit) return;

            if (_emitter == null)
                _emitter = gameObject.AddComponent<XFmod> ();

            _emitter.StartEvent ("event:/" + data.Clip, data.Channel);
        }

        private AudioSource GetAudioSourceByChannel (AudioChannel channel)
        {
            switch (channel)
            {
                case AudioChannel.Action:
                    {
                        if (_audio_action == null)
                            _audio_action = gameObject.AddComponent<AudioSource> ();

                        return _audio_action;
                    }
                case AudioChannel.Motion:
                    {
                        if (_audio_motion == null)
                            _audio_motion = gameObject.AddComponent<AudioSource> ();

                        return _audio_motion;
                    }
                case AudioChannel.Skill:
                    {
                        if (_audio_skill == null)
                            _audio_skill = gameObject.AddComponent<AudioSource> ();

                        return _audio_skill;
                    }
                case AudioChannel.Behit:
                    {
                        if (_audio_behit == null)
                            _audio_behit = gameObject.AddComponent<AudioSource> ();

                        return _audio_behit;
                    }
                case AudioChannel.BatiBehit:
                    {
                        if (_audio_batibehit == null)
                            _audio_batibehit = gameObject.AddComponent<AudioSource>();

                        return _audio_batibehit;
                    }
            }

            return _audio_action;
        }

        private void BoneShake (object param)
        {
            XBoneShakeExtra data = param as XBoneShakeExtra;
            if (boneShake != null)
            {
                BoneShakeHit hit = new BoneShakeHit();
                hit.direction = data.direction;
                hit.duration = data.duration;
                hit.intensity = data.intensity;
                hit.position = data.position;
                boneShake.Play(hit);
            }
            //BoneShakeCom.EnableIgnoreBone = ShakeIgnoreBone;
            //if (BoneShakeCom != null) 
            //    BoneShakeCom.OnShakeEvent (1, data);
        }

        private EffectPreviewContext GetPlayerPreviewContext ()
        {
            if (previewContext == null)
                previewContext = new EffectPreviewContext ();
            if (!previewContext.entityInit)
            {
                EffectConfig.InitEffect (previewContext, gameObject);
            }
            EffectConfig.PostInit (previewContext, "");
            return previewContext;
        }

        private void PlayShader (object o)
        {
            XShaderEffData data = o as XShaderEffData;
            EffectPreviewContext context = GetPlayerPreviewContext ();
            if (context != null)
            {
                var effectInst = RenderEffectSystem.CreateEffectInstance (
                    context,
                    data.Priority,
                    data.LifeTime,
                    data.FadeIn,
                    data.FadeOut,
                    true);
                if (effectInst != null)
                {
                    // RenderEffectSystem.PostCreateEffectInstance (effectInst);
                    RenderEffectSystem.AddEffect (effectInst, data.uniqueID,
                        data.path,
                        data.x, data.y, data.z, data.w, data.param, data.partMask);

                }
            }

        }

        void OnEndShader ()
        {
            //if (previewContext != null)
            //    RenderEffectMgr.EndEffect(previewContext.renderComponent, effectToken);
        }
    }
}
#endif