//#if UNITY_EDITOR
//using System;
//using UnityEditor;
//using UnityEditorInternal;
//using UnityEngine;

//using System.IO;
//using System.Runtime.Serialization;
//using System.Runtime.Serialization.Formatters.Binary;
//using System.Collections.Generic;

//using CFUtilPoolLib;
//using System.Reflection;
//using System.Text;
//using CFEngine;


//namespace XEditor
//{
//    [CustomEditor(typeof(XReactHoster))]
//    public class XReactHosterEditor : Editor
//    {

//        enum EntityEffectType
//        {
//            RimLight,
//            AddColor,
//            Fade,
//            Hide,
//        }

//        public static readonly float frame = (1.0f / 30.0f);

//        private XReactHoster _hoster = null;
//        private List<string> _xOptions = new List<string>();

//        public XReactHoster Hoster { get { return _hoster; } }

//        GUIContent colorTitle = new GUIContent("Color");

//        private GUIStyle _style = null;
//        private GUIStyle _labelstyle = null;
//        private int _option = 0;

//        private GUILayoutOption[] _line = new GUILayoutOption[] { GUILayout.Width(EditorGUIUtility.labelWidth), GUILayout.Height(1) };
//        protected GUIContent _content_add = new GUIContent("+");
//        protected GUIContent _content_remove = new GUIContent("-", "Remove Item.");

//        Vector4 shader_data = new Vector4();
//        Vector4 shader_data1 = new Vector4();


//        public void OnEnable()
//        {
//            hideFlags = HideFlags.HideAndDontSave;
//            if (_hoster == null) _hoster = target as XReactHoster;

//            //XInterfaceMgr.singleton.AttachInterface<IResourceHelp>(XCommon.singleton.XHash("XResourceHelper"), XResourceHelper.singleton);

//            _xOptions.Clear();
//            _xOptions.Add("Fx");
//            _xOptions.Add("Audio");
//            _xOptions.Add("ShaderEffect");
//            _xOptions.Add("BoneShake");
//        }

//        public override void OnInspectorGUI()
//        {
//            if (_labelstyle == null)
//            {
//                _labelstyle = new GUIStyle(EditorStyles.boldLabel);
//                _labelstyle.fontSize = 13;
//            }

//            /*****Base Settings*****/
//            GUILayout.Label("Basic Settings :", _labelstyle);

//            EditorGUILayout.LabelField("Data File", _hoster.ReactDataExtra.ScriptFile);
//            EditorGUILayout.LabelField("Data Path", _hoster.ReactDataExtra.ScriptPath);

//            EditorGUILayout.Space();

//            /*********/
//            GUILayout.Label("React Settings :", _labelstyle);

//            if (_hoster.ReactData.AnimationSpeedRatio == 0) _hoster.ReactData.AnimationSpeedRatio = 1;

//            EditorGUI.BeginChangeCheck();
//            AnimationClip clip = EditorGUILayout.ObjectField("Clip", _hoster.ReactDataExtra.ReactClip, typeof(AnimationClip), true) as AnimationClip;
//            if (EditorGUI.EndChangeCheck())
//            {
//                if (clip != null)
//                {
//                    _hoster.ReactDataExtra.ReactClip = clip;
//                    _hoster.ConfigData.ReactClip = AssetDatabase.GetAssetPath(clip);
//                    _hoster.ConfigData.ReactClipName = clip.name;
//                }
//                else
//                {
//                    _hoster.ReactDataExtra.ReactClip = null;
//                    _hoster.ConfigData.ReactClip = "";
//                    _hoster.ConfigData.ReactClipName = "";
//                }
//            }
            
//            EditorGUILayout.LabelField("Clip Name", _hoster.ConfigData.ReactClip);


//            EditorGUI.BeginChangeCheck();
//            if (_hoster.ConfigData.Lock)
//            {
//                EditorGUILayout.LabelField("Clip Length", (_hoster.ReactDataExtra.ReactClip_Frame * frame).ToString("F3") + "s" + "\t" + _hoster.ReactDataExtra.ReactClip_Frame.ToString("F1") + "(frame)");
//                _hoster.ReactData.AnimationSpeedRatio = EditorGUILayout.Slider("Clip Ratio", _hoster.ReactData.AnimationSpeedRatio, 0.5f, 2.0f);
//            }
//            else if (_hoster.ReactDataExtra.ReactClip != null)
//            {
//                EditorGUILayout.LabelField("Clip Length", _hoster.ReactDataExtra.ReactClip.length.ToString("F3") + "s" + "\t" + (_hoster.ReactDataExtra.ReactClip.length / frame).ToString("F1") + "(frame)");
//                _hoster.ReactDataExtra.ReactClip_Frame = (_hoster.ReactData.Time / frame);
//            }
//            EditorGUILayout.BeginHorizontal();
//            if (_hoster.ConfigData.Lock)
//            {
//                _hoster.ReactData.Time = _hoster.ReactDataExtra.ReactClip_Frame * frame;
//                EditorGUILayout.LabelField("React Time", _hoster.ReactData.Time.ToString("F3"));
//            }
//            else
//            {
//                _hoster.ReactData.AnimationSpeedRatio = 1;
//                _hoster.ReactData.Time = EditorGUILayout.FloatField("React Time", _hoster.ReactData.Time);
//                if (_hoster.ReactData.Time <= 0 && _hoster.ReactDataExtra.ReactClip != null)
//                    _hoster.ReactData.Time = _hoster.ReactDataExtra.ReactClip.length;
//            }                        

//            if (_hoster.ReactDataExtra.ReactClip != null)
//                _hoster.ReactDataExtra.ReactClip_Frame = (_hoster.ReactDataExtra.ReactClip.length * _hoster.ReactData.AnimationSpeedRatio / frame);
//            else
//            {
//                _hoster.ReactDataExtra.ReactClip_Frame = _hoster.ReactData.Time * _hoster.ReactData.AnimationSpeedRatio / frame;
//            }

//            GUILayout.Label("(s)");
//            EditorGUILayout.EndHorizontal();

//            _hoster.ConfigData.Lock = EditorGUILayout.Toggle("(Lock)", _hoster.ConfigData.Lock);


//            if (_hoster.ReactDataExtra.ReactClip != null && _hoster.ReactDataExtra.ReactClip.length > 0)
//            {
//                _hoster.ReactData.AnimTime = _hoster.ReactDataExtra.ReactClip.length;
//                _hoster.ReactData.FadeTime = EditorGUILayout.FloatField("Fade Time", _hoster.ReactData.FadeTime);
//            }
//            else
//            {
//                _hoster.ReactData.AnimTime = 0f;
//            }

//            if (EditorGUI.EndChangeCheck())
//            {
//                FoldOut();
//            }

//            /*****Timer*****/
//            GUILayout.Label("Timer :", _labelstyle);
//            EditorGUILayout.BeginHorizontal();

//            _option = EditorGUILayout.Popup("Create", _option, _xOptions.ToArray());
//            if (GUILayout.Button(_content_add, GUILayout.MaxWidth(30)))
//            {
//                switch (_option)
//                {
//                    case 0: Add<XFxData>(); break;
//                    case 1: Add<XAudioData>(); break;
//                    case 2: Add<XShaderEffData>();break;
//                    case 3: Add<XBoneShakeData>(); break;
//                }
//            }
//            EditorGUILayout.EndHorizontal();

//            EditorGUILayout.BeginHorizontal();
//            _hoster.ReactData.InputFreezed = EditorGUILayout.Toggle("InputFreezed", _hoster.ReactData.InputFreezed);
//            EditorGUILayout.EndHorizontal();

//            EditorGUILayout.BeginHorizontal();
//            _hoster.ReactData.StopAnimAtEnd = EditorGUILayout.Toggle("StopAnimAtEnd", _hoster.ReactData.StopAnimAtEnd);
//            EditorGUILayout.EndHorizontal();

//            //EditorGUILayout.BeginHorizontal();
//            //_hoster.ReactData.IsAdditive = EditorGUILayout.Toggle("IsAdditive", _hoster.ReactData.IsAdditive);
//            //EditorGUILayout.EndHorizontal();

//            //if (_hoster.ReactData.IsAdditive)
//            //{
//            //    EditorGUILayout.BeginHorizontal();
//            //    _hoster.ReactData.AdditiveWeight = EditorGUILayout.Slider("AdditiveWeight", _hoster.ReactData.AdditiveWeight, 0f, 1f);
//            //    EditorGUILayout.EndHorizontal();
//            //}

//            Hoster.ReactData.AnimLayerType = (EReactAnimLayerType)EditorGUILayout.EnumPopup("Anim Layer Type", Hoster.ReactData.AnimLayerType);

//            if (_hoster.ReactData.AnimLayerType != EReactAnimLayerType.None)
//            {
//                EditorGUILayout.BeginHorizontal();
//                _hoster.ReactData.LayerWeight = EditorGUILayout.Slider("LayerWeight", _hoster.ReactData.LayerWeight, 0f, 1f);
//                EditorGUILayout.EndHorizontal();

//                Hoster.ReactDataExtra.Mask = EditorGUILayout.ObjectField("AvatarMask", Hoster.ReactDataExtra.Mask, typeof(AvatarMask), true) as AvatarMask;

//                if (Hoster.ReactDataExtra.Mask != null)
//                {
//                    string path = AssetDatabase.GetAssetPath(Hoster.ReactDataExtra.Mask).Remove(0, 17);
//                    Hoster.ReactData.AvatarMask = path.Remove(path.LastIndexOf('.'));
//                    EditorGUILayout.LabelField("AvatarMask Path", Hoster.ReactData.AvatarMask);
//                }
//                else
//                {
//                    Hoster.ReactData.AvatarMask = "";
//                }
//            }

//            //float pose_at = Hoster.ReactDataExtra.ReactClip_Frame * Hoster.ReactDataExtra.PoseFrameRatio;
//            //EditorGUILayout.BeginHorizontal();
//            //pose_at = EditorGUILayout.FloatField("Pose Frame", pose_at);
//            //GUILayout.Label("(frame)");
//            //GUILayout.Label("", GUILayout.MaxWidth(30));
//            //EditorGUILayout.EndHorizontal();

//            //EditorGUILayout.BeginHorizontal();
//            //Hoster.ReactDataExtra.PoseFrameRatio = EditorGUILayout.Slider("Pose Ratio", Hoster.ReactDataExtra.PoseFrameRatio, 0, 1);
//            //GUILayout.Label("(0~1)", EditorStyles.miniLabel);
//            //EditorGUILayout.EndHorizontal();

//            Hoster.ReactData.PoseFrame = EditorGUILayout.IntField("Pose Frame", Hoster.ReactData.PoseFrame);


//            /*** Draw child ****/
//            EditorGUILayout.Space();
//            OnFxGUI();
//            EditorGUILayout.Space();
//            OnAudioGUI();
//            EditorGUILayout.Space();
//            OnShaderEffectGUI();
//            EditorGUILayout.Space();
//            OnBoneShakeGUI();

//            EditorGUILayout.Space();
//            EditorGUILayout.Space();
//            EditorGUILayout.BeginHorizontal();
//            GUILayout.FlexibleSpace();
//            if (GUILayout.Button("Apply", GUILayout.MaxWidth(150)))
//            {
//                SerializeData(GetDataFileWithPath());
//            }
//            EditorGUILayout.Space();
//            if (GUILayout.Button("Revert", GUILayout.MaxWidth(150))) DeserializeData(GetDataFileWithPath());

//            if (GUILayout.Button("Fold All", GUILayout.MaxWidth(150)))
//            {
//                FoldIn();
//            }
            

//            GUILayout.FlexibleSpace();
//            EditorGUILayout.EndHorizontal();
//            EditorGUILayout.Space();

//            if (GUI.changed)
//            {
//                EditorUtility.SetDirty(target);
//            }
//            XReactDataBuilder.singleton.Update(_hoster);
//        }

//        public void Add<T>()
//            where T : XBaseData, new()
//        {
//            T data = new T();
//            Type t = typeof(T);
//            if (t == typeof(XFxData)) { if (Hoster.ReactData.Fx == null) Hoster.ReactData.Fx = new List<XFxData>(); Hoster.ReactData.Fx.Add(data as XFxData); }
//            else if (t == typeof(XAudioData)) { if (Hoster.ReactData.Audio == null) Hoster.ReactData.Audio = new List<XAudioData>(); Hoster.ReactData.Audio.Add(data as XAudioData); }
//            else if (t == typeof(XShaderEffData))
//            {
//                if (Hoster.ReactData.ShaderEffData == null) Hoster.ReactData.ShaderEffData = new List<XShaderEffData>();
//                Hoster.ReactData.ShaderEffData.Add(data as XShaderEffData);
//            }
//            else if (t == typeof(XBoneShakeData))
//            {
//                if (Hoster.ReactData.BoneShakeData == null) Hoster.ReactData.BoneShakeData = new List<XBoneShakeData>();
//                Hoster.ReactData.BoneShakeData.Add(data as XBoneShakeData);
//            }


//            if (t == typeof(XFxData)) AddExtraEx<XFxDataExtra>();
//            else if (t == typeof(XAudioData)) AddExtraEx<XAudioDataExtra>();
//            else if (t == typeof(XBoneShakeData)) AddExtraEx<XBoneShakeExtra>();
//        }

//        public void AddExtraEx<T>()
//            where T : XBaseDataExtra, new()
//        {
//            T data = new T();
//            Hoster.ReactDataExtra.Add<T>(data);
//        }

//        private void FoldOut()
//        {
//            Hoster.EditorData.XFx_foldout = true;
//            Hoster.EditorData.XAudio_foldout = true;
//            Hoster.EditorData.XShaderEffect_foldout = true;
//        }

//        private void FoldIn()
//        {
//            Hoster.EditorData.XFx_foldout = false;
//            Hoster.EditorData.XAudio_foldout = false;
//            Hoster.EditorData.XShaderEffect_foldout = false;
//        }

//        #region Fx
//        void OnFxGUI()
//        {
//            if (_style == null) _style = new GUIStyle(GUI.skin.GetStyle("Label"));
//            _style.alignment = TextAnchor.UpperRight;

//            EditorGUILayout.BeginHorizontal();
//            Hoster.EditorData.XFx_foldout = EditorGUILayout.Foldout(Hoster.EditorData.XFx_foldout, "Fx");
//            GUILayout.FlexibleSpace();
//            int Count = Hoster.ReactData.Fx == null ? -1 : Hoster.ReactData.Fx.Count;
//            if (Count > 0) EditorGUILayout.LabelField("Total " + Count.ToString(), _style);
//            EditorGUILayout.EndHorizontal();

//            if (!Hoster.EditorData.XFx_foldout)
//            {
//                //nothing
//                return;
//            }

//            GUILayout.Box("", _line);

//            if (Hoster.ReactData.Fx == null) return;

//            for (int i = 0; i < Hoster.ReactData.Fx.Count; i++)
//            {
//                Hoster.ReactData.Fx[i].Index = i;
//                EditorGUILayout.BeginHorizontal();
//                Hoster.ReactData.Fx[i].Type = (SkillFxType)EditorGUILayout.EnumPopup("Type Based on", Hoster.ReactData.Fx[i].Type);
//                if (GUILayout.Button(_content_remove, GUILayout.MaxWidth(30)))
//                {
//                    Hoster.ReactData.Fx.RemoveAt(i);
//                    Hoster.ReactDataExtra.Fx.RemoveAt(i);
//                    EditorGUILayout.EndHorizontal();
//                    continue;
//                }
//                EditorGUILayout.EndHorizontal();

//                Hoster.ReactDataExtra.Fx[i].Fx = EditorGUILayout.ObjectField("Fx Object", Hoster.ReactDataExtra.Fx[i].Fx, typeof(GameObject), true) as GameObject;
//                if (null == Hoster.ReactDataExtra.Fx[i].Fx || !AssetDatabase.GetAssetPath(Hoster.ReactDataExtra.Fx[i].Fx).Contains("BundleRes/Effects/"))
//                {
//                    Hoster.ReactDataExtra.Fx[i].Fx = null;
//                }

//                if (null != Hoster.ReactDataExtra.Fx[i].Fx)
//                {
//                    string path = AssetDatabase.GetAssetPath(Hoster.ReactDataExtra.Fx[i].Fx).Remove(0, 17);
//                    Hoster.ReactData.Fx[i].Fx = path.Remove(path.LastIndexOf('.'));
//                    EditorGUILayout.LabelField("Fx Name", Hoster.ReactData.Fx[i].Fx);

//                    if (Hoster.ReactData.Fx[i].Type == SkillFxType.FirerBased)
//                    {
//                        if (!Hoster.ReactData.Fx[i].StickToGround)
//                        {
//                            if (Hoster.ReactData.Fx[i].Bone == null || Hoster.ReactData.Fx[i].Bone.Length == 0)
//                                Hoster.ReactDataExtra.Fx[i].BindTo = null;
//                            Hoster.ReactDataExtra.Fx[i].BindTo = EditorGUILayout.ObjectField("Bone", Hoster.ReactDataExtra.Fx[i].BindTo, typeof(GameObject), true) as GameObject;
//                            if (Hoster.ReactDataExtra.Fx[i].BindTo != null)
//                            {
//                                string name = "";
//                                Transform parent = Hoster.ReactDataExtra.Fx[i].BindTo.transform;
//                                while (parent.parent != null)
//                                {
//                                    name = name.Length == 0 ? parent.name : parent.name + "/" + name;
//                                    parent = parent.parent;
//                                }
//                                Hoster.ReactData.Fx[i].Bone = name.Length > 0 ? name : null;
//                            }
//                            else
//                                Hoster.ReactData.Fx[i].Bone = null;
//                        }
//                        else
//                            Hoster.ReactData.Fx[i].Bone = null;
//                    }
//                    else
//                        Hoster.ReactData.Fx[i].Bone = null;

//                    EditorGUILayout.Space();
//                    Vector3 vec = new Vector3(Hoster.ReactData.Fx[i].ScaleX, Hoster.ReactData.Fx[i].ScaleY, Hoster.ReactData.Fx[i].ScaleZ);
//                    vec = EditorGUILayout.Vector3Field("Scale", vec);
//                    Hoster.ReactData.Fx[i].ScaleX = vec.x;
//                    Hoster.ReactData.Fx[i].ScaleY = vec.y;
//                    Hoster.ReactData.Fx[i].ScaleZ = vec.z;

//                    if (Hoster.ReactData.Fx[i].Alone)
//                    {
//                        vec.Set(Hoster.ReactData.Fx[i].RotX, Hoster.ReactData.Fx[i].RotY, Hoster.ReactData.Fx[i].RotZ);
//                        vec = EditorGUILayout.Vector3Field("Rotation", vec);
//                        Hoster.ReactData.Fx[i].RotX = vec.x;
//                        Hoster.ReactData.Fx[i].RotY = vec.y;
//                        Hoster.ReactData.Fx[i].RotZ = vec.z;
//                        EditorGUILayout.Space();
//                    }

//                    if (Hoster.ReactData.Fx[i].Type == SkillFxType.TargetBased)
//                    {
//                        vec.Set(Hoster.ReactData.Fx[i].Target_OffsetX, Hoster.ReactData.Fx[i].Target_OffsetY, Hoster.ReactData.Fx[i].Target_OffsetZ);
//                        vec = EditorGUILayout.Vector3Field("Offset Target", vec);
//                        Hoster.ReactData.Fx[i].Target_OffsetX = vec.x;
//                        Hoster.ReactData.Fx[i].Target_OffsetY = vec.y;
//                        Hoster.ReactData.Fx[i].Target_OffsetZ = vec.z;

//                        vec.Set(Hoster.ReactData.Fx[i].OffsetX, Hoster.ReactData.Fx[i].OffsetY, Hoster.ReactData.Fx[i].OffsetZ);
//                        vec = EditorGUILayout.Vector3Field("Offset Firer when no Target", vec);
//                        Hoster.ReactData.Fx[i].OffsetX = vec.x;
//                        Hoster.ReactData.Fx[i].OffsetY = vec.y;
//                        Hoster.ReactData.Fx[i].OffsetZ = vec.z;
//                    }
//                    else
//                    {
//                        vec.Set(Hoster.ReactData.Fx[i].OffsetX, Hoster.ReactData.Fx[i].OffsetY, Hoster.ReactData.Fx[i].OffsetZ);
//                        vec = EditorGUILayout.Vector3Field("Offset", vec);
//                        Hoster.ReactData.Fx[i].OffsetX = vec.x;
//                        Hoster.ReactData.Fx[i].OffsetY = vec.y;
//                        Hoster.ReactData.Fx[i].OffsetZ = vec.z;
//                    }

//                    EditorGUILayout.Space();
//                    float fx_at = Hoster.ReactDataExtra.ReactClip_Frame * Hoster.ReactDataExtra.Fx[i].Ratio;
//                    EditorGUILayout.BeginHorizontal();
//                    fx_at = EditorGUILayout.FloatField("Play At", fx_at);
//                    GUILayout.Label("(frame)");
//                    GUILayout.Label("", GUILayout.MaxWidth(30));
//                    EditorGUILayout.EndHorizontal();

//                    Hoster.ReactDataExtra.Fx[i].Ratio = fx_at / Hoster.ReactDataExtra.ReactClip_Frame;
//                    if (Hoster.ReactDataExtra.Fx[i].Ratio > 1) Hoster.ReactDataExtra.Fx[i].Ratio = 1;

//                    EditorGUILayout.BeginHorizontal();
//                    Hoster.ReactDataExtra.Fx[i].Ratio = EditorGUILayout.Slider("Ratio", Hoster.ReactDataExtra.Fx[i].Ratio, 0, 1);
//                    GUILayout.Label("(0~1)", EditorStyles.miniLabel);
//                    EditorGUILayout.EndHorizontal();

//                    Hoster.ReactData.Fx[i].At = Hoster.ReactDataExtra.Fx[i].Ratio * Hoster.ReactDataExtra.ReactClip_Frame * frame;

//                    ////////////////////////////////
//                    float fx_end_at = Hoster.ReactDataExtra.ReactClip_Frame * Hoster.ReactDataExtra.Fx[i].End_Ratio;
//                    EditorGUILayout.BeginHorizontal();
//                    fx_end_at = EditorGUILayout.FloatField("End At", fx_end_at);
//                    GUILayout.Label("(frame)");
//                    GUILayout.Label("", GUILayout.MaxWidth(30));
//                    EditorGUILayout.EndHorizontal();

//                    Hoster.ReactDataExtra.Fx[i].End_Ratio = fx_end_at / Hoster.ReactDataExtra.ReactClip_Frame;
//                    if (Hoster.ReactDataExtra.Fx[i].End_Ratio > 1) Hoster.ReactDataExtra.Fx[i].End_Ratio = 1;

//                    EditorGUILayout.BeginHorizontal();
//                    Hoster.ReactDataExtra.Fx[i].End_Ratio = EditorGUILayout.Slider("End Ratio", Hoster.ReactDataExtra.Fx[i].End_Ratio, 0, 1);
//                    GUILayout.Label("(0~1)", EditorStyles.miniLabel);
//                    EditorGUILayout.EndHorizontal();

//                    Hoster.ReactData.Fx[i].End = Hoster.ReactDataExtra.Fx[i].End_Ratio * Hoster.ReactDataExtra.ReactClip_Frame * frame;
//                    if (Hoster.ReactData.Fx[i].End < Hoster.ReactData.Fx[i].At) Hoster.ReactData.Fx[i].At = Hoster.ReactData.Fx[i].End;
//                    EditorGUILayout.Space();

//                    Hoster.ReactData.Fx[i].StickToGround = EditorGUILayout.Toggle("Stick On Ground", Hoster.ReactData.Fx[i].StickToGround);
//                    /*
//                     * follow mode can not use for TargetBased
//                     * in that case the fx life-time will not in controlled.
//                     */
//                    if (Hoster.ReactData.Fx[i].Type == SkillFxType.FirerBased && !Hoster.ReactData.Fx[i].StickToGround)
//                        Hoster.ReactData.Fx[i].Follow = EditorGUILayout.Toggle("Follow", Hoster.ReactData.Fx[i].Follow);
//                    else
//                        Hoster.ReactData.Fx[i].Follow = false;
//                    if (!Hoster.ReactData.Fx[i].Follow &&
//                        !Hoster.ReactData.Fx[i].StickToGround &&
//                        Hoster.ReactData.Fx[i].Bone == null)
//                    {
//                        Hoster.ReactData.Fx[i].Alone = EditorGUILayout.Toggle("Alone", Hoster.ReactData.Fx[i].Alone);
//                    }
//                    else
//                        Hoster.ReactData.Fx[i].Alone = false;

//                    EditorGUILayout.Space();

//                    EditorGUILayout.BeginHorizontal();
//                    Hoster.ReactData.Fx[i].Destroy_Delay = EditorGUILayout.FloatField("Delay Destroy", Hoster.ReactData.Fx[i].Destroy_Delay);
//                    GUILayout.Label("(s)");
//                    EditorGUILayout.EndHorizontal();
//                    Hoster.ReactData.Fx[i].Shield = EditorGUILayout.Toggle("Shield", Hoster.ReactData.Fx[i].Shield);
//                }
//                else
//                {
//                    Hoster.ReactData.Fx[i].Fx = null;
//                }

//                if (i != Hoster.ReactData.Fx.Count - 1)
//                {
//                    GUILayout.Box("", _line);
//                    EditorGUILayout.Space();
//                }

//            }
//        }
//        #endregion

//        #region Audio
//        protected void OnAudioGUI()
//        {
//            if (_style == null) _style = new GUIStyle(GUI.skin.GetStyle("Label"));
//            _style.alignment = TextAnchor.UpperRight;

//            EditorGUILayout.BeginHorizontal();
//            Hoster.EditorData.XAudio_foldout = EditorGUILayout.Foldout(Hoster.EditorData.XAudio_foldout, "Audio");
//            GUILayout.FlexibleSpace();
//            int Count = Hoster.ReactData.Audio == null ? -1 : Hoster.ReactData.Audio.Count;
//            if (Count > 0) EditorGUILayout.LabelField("Total " + Count.ToString(), _style);
//            EditorGUILayout.EndHorizontal();

//            if (!Hoster.EditorData.XAudio_foldout)
//            {
//                //nothing
//                return;
//            }


//            if (Hoster.ReactData.Audio == null) return;

//            for (int i = 0; i < Hoster.ReactData.Audio.Count; i++)
//            {
//                Hoster.ReactData.Audio[i].Index = i;
//                EditorGUILayout.BeginHorizontal();

//                Hoster.ReactData.Audio[i].Clip = EditorGUILayout.TextField("Clip Name", Hoster.ReactData.Audio[i].Clip);

//                if (GUILayout.Button(_content_remove, GUILayout.MaxWidth(30)))
//                {
//                    Hoster.ReactData.Audio.RemoveAt(i);
//                    Hoster.ReactDataExtra.Audio.RemoveAt(i);
//                }
//                EditorGUILayout.EndHorizontal();

//                if (i < Hoster.ReactData.Audio.Count)
//                {
//                    float audio_at = Hoster.ReactDataExtra.ReactClip_Frame * Hoster.ReactDataExtra.Audio[i].Ratio;
//                    EditorGUILayout.BeginHorizontal();
//                    audio_at = EditorGUILayout.FloatField("Play At ", audio_at);
//                    GUILayout.Label("(frame)");
//                    GUILayout.Label("", GUILayout.MaxWidth(30));
//                    EditorGUILayout.EndHorizontal();

//                    Hoster.ReactDataExtra.Audio[i].Ratio = audio_at / Hoster.ReactDataExtra.ReactClip_Frame;
//                    if (Hoster.ReactDataExtra.Audio[i].Ratio > 1) Hoster.ReactDataExtra.Audio[i].Ratio = 1;

//                    EditorGUILayout.BeginHorizontal();
//                    Hoster.ReactDataExtra.Audio[i].Ratio = EditorGUILayout.Slider("Ratio", Hoster.ReactDataExtra.Audio[i].Ratio, 0, 1);
//                    GUILayout.Label("(0~1)", EditorStyles.miniLabel);
//                    EditorGUILayout.EndHorizontal();

//                    Hoster.ReactData.Audio[i].At = (Hoster.ReactDataExtra.Audio[i].Ratio * Hoster.ReactDataExtra.ReactClip_Frame) * frame;

//                    Hoster.ReactData.Audio[i].Channel = (AudioChannel)EditorGUILayout.EnumPopup("Channel", Hoster.ReactData.Audio[i].Channel);
//                }

//                if (i != Hoster.ReactData.Audio.Count - 1)
//                {
//                    GUILayout.Box("", _line);
//                    EditorGUILayout.Space();
//                }
//            }
//        }
//        #endregion

//        #region ShaderEffct

//        void OnShaderEffectGUI()
//        {
//            if (_style == null) _style = new GUIStyle(GUI.skin.GetStyle("Label"));
//            _style.alignment = TextAnchor.UpperRight;

//            EditorGUILayout.BeginHorizontal();
//            Hoster.EditorData.XShaderEffect_foldout = EditorGUILayout.Foldout(Hoster.EditorData.XShaderEffect_foldout, "ShaderEffect");
//            GUILayout.FlexibleSpace();
//            int Count = Hoster.ReactData.ShaderEffData == null ? -1 : Hoster.ReactData.ShaderEffData.Count;
//            if (Count > 0) EditorGUILayout.LabelField("Total " + Count.ToString(), _style);
//            EditorGUILayout.EndHorizontal();

//            if (!Hoster.EditorData.XShaderEffect_foldout)
//            {
//                //nothing
//                return;
//            }

//            if (Hoster.ReactData.ShaderEffData == null) return;

//            for (short i = 0; i < Hoster.ReactData.ShaderEffData.Count; i++)
//            {
//                var effect = Hoster.ReactData.ShaderEffData[i];
//                EditorGUILayout.BeginHorizontal();
//                EntityEffectType type = (EntityEffectType)effect.UniqueID;

//                EditorGUI.BeginChangeCheck();

//                effect.UniqueID = (short)((EntityEffectType)EditorGUILayout.EnumPopup("Effect Type", type));
//                if (GUILayout.Button(_content_remove, GUILayout.MaxWidth(30)))
//                {
//                    Hoster.ReactData.ShaderEffData.RemoveAt(i);
//                    EditorGUILayout.EndHorizontal();
//                    EditorGUI.EndChangeCheck();
//                    continue;
//                }
//                EditorGUILayout.EndHorizontal();

//                if (EditorGUI.EndChangeCheck())
//                {
//                    switch (type)
//                    {
//                        case EntityEffectType.RimLight:
//                            {
//                                shader_data = new Vector4(1, 1, 1, 0.5f);
//                                shader_data1 = new Vector4(0, 0, -1, -1);
//                            }
//                            break;
//                        case EntityEffectType.AddColor:
//                            {
//                                shader_data = new Vector4(1, 1, 1, 0.4f);
//                                shader_data1 = new Vector4(0, 0, -1, -1);
//                            }
//                            break;
//                        case EntityEffectType.Fade:
//                            {
//                                shader_data = new Vector4(0, 0, 0, 0.5f);
//                                shader_data1 = new Vector4(0, 0, -1, -1);
//                            }
//                            break;
//                        case EntityEffectType.Hide:
//                            {      
//                                shader_data = Vector4.zero;
//                                shader_data1 = new Vector4(0, 0, -1, -1);
//                            }
//                            break;
//                    }

//                }

//                EditorGUILayout.BeginHorizontal();
//                effect.At = EditorGUILayout.FloatField("Play At ", effect.At);
//                GUILayout.Label("(s)");
//                GUILayout.Label("", GUILayout.MaxWidth(30));
//                EditorGUILayout.EndHorizontal();


//                shader_data.x = effect.Data0;
//                shader_data.y = effect.Data1;
//                shader_data.z = effect.Data2;
//                shader_data.w = effect.Data3;

//                shader_data1.x = effect.FadeIn;
//                shader_data1.y = effect.FadeOut;
//                shader_data1.z = effect.LifeTime;
//                shader_data1.w = effect.Param;

//                switch (type)
//                {
//                    case EntityEffectType.RimLight:
//                        {
//                            var data = shader_data;
//                            var fadeInOut = shader_data1;

//                            Color color = data;
//                            color = EditorGUILayout.ColorField(colorTitle, color, false, false, true);
//                            data = new Vector4(color.r, color.g, color.b, data.w);
//                            float intensity = Mathf.Clamp(data.w - 0.5f, 0, 0.4f) * 2.5f; //0.5-0.9
//                            intensity = EditorGUILayout.Slider("Intensity", intensity, 0.0f, 1.0f);
//                            data.w = 0.5f + intensity * 0.4f;

//                            FadeInOutGUI(ref fadeInOut);

//                            //part?

//                            shader_data = data;
//                            shader_data1 = fadeInOut;
//                        }
//                        break;
//                    case EntityEffectType.AddColor:
//                        {
//                            var data = shader_data;
//                            var fadeInOut = shader_data1;

//                            Color color = data;
//                            color = EditorGUILayout.ColorField(colorTitle, color, false, false, true);
//                            data = new Vector4(color.r, color.g, color.b, 0.4f);

//                            FadeInOutGUI(ref fadeInOut);

//                            //part?

//                            shader_data = data;
//                            shader_data1 = fadeInOut;
//                        }
//                        break;
//                    case EntityEffectType.Fade:
//                        {
//                            var data = shader_data;
//                            var fadeInOut = shader_data1;

//                            float alpha = EditorGUILayout.Slider("FadeAlpha", data.w, 0.0f, 0.5f);
//                            data = new Vector4(1, 1, 1, alpha);

//                            FadeInOutGUI(ref fadeInOut);

//                            shader_data = data;
//                            shader_data1 = fadeInOut;
//                        }
//                        break;
//                    case EntityEffectType.Hide:
//                        {
//                            var data = shader_data;
//                            var fadeInOut = shader_data1;

//                            FadeInOutGUI(ref fadeInOut);

//                            shader_data = Vector4.zero;
//                            shader_data1 = fadeInOut;
//                        }
//                        break;
//                }

//                effect.Data0 = shader_data.x;
//                effect.Data1 = shader_data.y;
//                effect.Data2 = shader_data.z;
//                effect.Data3 = shader_data.w;
//                effect.FadeIn = shader_data1.x;
//                effect.FadeOut = shader_data1.y;
//                effect.LifeTime = shader_data1.z;
//                effect.Param = shader_data1.w;


//            }
//        }

//        public void FadeInOutGUI(ref Vector4 fadeInOut)
//        {
//            fadeInOut.x = EditorGUILayout.Slider("FadeIn", fadeInOut.x, 0.0f, 2);
//            fadeInOut.y = EditorGUILayout.Slider("FadeOut", fadeInOut.y, 0.0f, 2);
//            fadeInOut.z = EditorGUILayout.Slider("EffectTime", fadeInOut.z, -1.0f, 10.0f);
//        }

//        #endregion

//        #region BoneShake
//        void OnBoneShakeGUI()
//        {
//            if (_style == null) _style = new GUIStyle(GUI.skin.GetStyle("Label"));
//            _style.alignment = TextAnchor.UpperRight;

//            EditorGUILayout.BeginHorizontal();
//            Hoster.EditorData.XBoneShake_foldout = EditorGUILayout.Foldout(Hoster.EditorData.XBoneShake_foldout, "BoneShake");
//            GUILayout.FlexibleSpace();
//            int Count = Hoster.ReactData.BoneShakeData == null ? -1 : Hoster.ReactData.BoneShakeData.Count;
//            if (Count > 0) EditorGUILayout.LabelField("Total " + Count.ToString(), _style);
//            EditorGUILayout.EndHorizontal();

//            if (!Hoster.EditorData.XBoneShake_foldout)
//            {
//                //nothing
//                return;
//            }

//            if (Hoster.ReactData.BoneShakeData == null) return;

//            for (short i = 0; i < Hoster.ReactData.BoneShakeData.Count; i++)
//            {
//                var ShakeData = Hoster.ReactData.BoneShakeData[i];

//                EditorGUILayout.BeginHorizontal();

//                ShakeData.Bone = EditorGUILayout.TextField("BonePath", ShakeData.Bone);

//                if (GUILayout.Button(_content_remove, GUILayout.MaxWidth(30)))
//                {
//                    Hoster.ReactData.BoneShakeData.RemoveAt(i);
//                    Hoster.ReactDataExtra.BoneShake.RemoveAt(i);

//                    continue;
//                }
//                EditorGUILayout.EndHorizontal();

//                //bone

//                if (ShakeData.Bone == null || ShakeData.Bone.Length == 0)
//                    Hoster.ReactDataExtra.BoneShake[i].BindTo = null;
//                Hoster.ReactDataExtra.BoneShake[i].BindTo = EditorGUILayout.ObjectField("Bone", Hoster.ReactDataExtra.BoneShake[i].BindTo, typeof(GameObject), true) as GameObject;
//                if (Hoster.ReactDataExtra.BoneShake[i].BindTo != null)
//                {
//                    string name = "";
//                    Transform parent = Hoster.ReactDataExtra.BoneShake[i].BindTo.transform;
//                    while (parent.parent != null)
//                    {
//                        name = name.Length == 0 ? parent.name : parent.name + "/" + name;
//                        parent = parent.parent;
//                    }
//                    ShakeData.Bone = name.Length > 0 ? name : null;
//                }
//                else
//                    ShakeData.Bone = null;

//                ShakeData.LifeTime = EditorGUILayout.FloatField("Time", ShakeData.LifeTime);
//                ShakeData.Frequency = EditorGUILayout.FloatField("Frequency", ShakeData.Frequency);
//                EditorGUILayout.Space();
//                ShakeData.AmplitudeX = EditorGUILayout.FloatField("Amplitude X", ShakeData.AmplitudeX);
//                ShakeData.AmplitudeY = EditorGUILayout.FloatField("Amplitude Y", ShakeData.AmplitudeY);
//                ShakeData.AmplitudeZ = EditorGUILayout.FloatField("Amplitude Z", ShakeData.AmplitudeZ);

//                EditorGUILayout.BeginHorizontal();
//                ShakeData.At = EditorGUILayout.FloatField("Play At ", ShakeData.At);
//                GUILayout.Label("(s)");
//                GUILayout.Label("", GUILayout.MaxWidth(30));
//                EditorGUILayout.EndHorizontal();
//            }
//        }

//        #endregion

//        private string GetDataFileWithPath()
//        {
//            return _hoster.ReactDataExtra.ScriptPath + _hoster.ReactDataExtra.ScriptFile + ".txt";
//        }

//        private void SerializeData(string file)
//        {
//            _hoster.ReactData.Name = _hoster.ReactDataExtra.ScriptFile;

//            if (_hoster.ReactDataExtra.ReactClip != null)
//            {
//                string path = AssetDatabase.GetAssetPath(_hoster.ReactDataExtra.ReactClip).Remove(0, 17);
//                _hoster.ReactData.ClipName = path.Remove(path.LastIndexOf('.'));
//            }
//            else
//            {
//                _hoster.ReactData.ClipName = "";
//            }

//            StripData(_hoster.ReactData);

//            XDataIO<XReactData>.singleton.SerializeData(file, _hoster.ReactData);
//            XDataIO<XReactConfigData>.singleton.SerializeData(XEditorPath.GetCfgFromSkp(file), _hoster.ConfigData);

//            XReactDataBuilder.Time = File.GetLastWriteTime(file);
//        }

//        private void DeserializeData(string file)
//        {
//            _hoster.ConfigData = XDataIO<XReactConfigData>.singleton.DeserializeData(XEditorPath.GetCfgFromSkp(file));
//            _hoster.ReactData = XDataIO<XReactData>.singleton.DeserializeData(file);

//            XReactDataBuilder.singleton.HotBuildEx(_hoster, _hoster.ConfigData);
//        }

//        private void StripData(XReactData data)
//        {
//            if (string.IsNullOrEmpty(data.Name)) data.Name = null;
//            if (string.IsNullOrEmpty(data.ClipName)) data.ClipName = null;

//            ///////////////////////////////////////////////////
//            if (data.Audio != null && data.Audio.Count > 0)
//            {
//                foreach (XAudioData a in data.Audio)
//                {
//                    if (string.IsNullOrEmpty(a.Clip)) a.Clip = null;
//                }
//            }
//            else
//                data.Audio = null;
//            ///////////////////////////////////////////////////
//            if (data.Fx != null && data.Fx.Count > 0)
//            {
//                foreach (XFxData f in data.Fx)
//                {
//                    if (string.IsNullOrEmpty(f.Fx)) f.Fx = null;
//                    if (string.IsNullOrEmpty(f.Bone)) f.Bone = null;
//                    if (!f.Alone)
//                    {
//                        f.RotX = 0;
//                        f.RotY = 0;
//                        f.RotZ = 0;
//                    }
//                }
//            }
//            else
//                data.Fx = null;
//        }

        
//    }
//}
//#endif