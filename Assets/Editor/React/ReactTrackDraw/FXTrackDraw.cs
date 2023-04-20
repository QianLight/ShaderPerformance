#if UNITY_EDITOR

using UnityEditor;
using UnityEngine;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using System.IO;
using System.Xml.Serialization;
using CFUtilPoolLib;
using System;

namespace XEditor {
    

    public class FXTrackDraw : BaseTrackDraw
    {
        public int selectTrack = -1;
        public XFxData selectFxData = null;
        public XFxDataExtra selectFxDataExtra = null;

        private float effectTime = 0;

        public override string Name { get { return "Fx"; } }

        public override void OnDrawLeft()
        {
            var ReactDataSet = Window.ReactDataSet;
            if (ReactDataSet.ReactData.Fx != null && ReactDataSet.ReactData.Fx.Count > 0)
            {
                for (int i = 0; i < ReactDataSet.ReactData.Fx.Count; ++i)
                {
                    var fxData = ReactDataSet.ReactData.Fx[i];

                    GUI.color = selectTrack == i ? Window.selectdColor : Window.trackColor;
                    GUILayout.BeginHorizontal("box");
                    {
                        GUILayout.Space(30);
                        if (GUILayout.Button(fxData.Fx, Window.trackStyle))
                        {
                            selectTrack = i;

                            selectFxData = fxData;
                            selectFxDataExtra = ReactDataSet.ReactDataExtra.Fx[i];
                        }

                        GUI.color = Window.trackDropColor;
                        if (GUILayout.Button(" Del ", Window.trackDropStyle))
                        {
                            selectTrack = -1;
                            fxData = null;
                            selectFxDataExtra = null;

                            //del
                            ReactDataSet.ReactData.Fx.RemoveAt(i);
                            ReactDataSet.ReactDataExtra.Fx.RemoveAt(i);
                        }
                    }
                    GUILayout.EndHorizontal();
                }

                //auto select.
                if (selectTrack == -1 && ReactDataSet.ReactData.Fx.Count > 0)
                {
                    selectTrack = 0;
                    selectFxData = ReactDataSet.ReactData.Fx[0];
                    selectFxDataExtra = ReactDataSet.ReactDataExtra.Fx[0];
                }
            }

            GUILayout.BeginHorizontal();
            {
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("Add(+)", GUILayout.Width(60), GUILayout.Height(30)))
                {
                    Add();
                }
                GUILayout.FlexibleSpace();
            }
            GUILayout.EndHorizontal();

            GUI.color = ReactCommon.InitColor;
        }

        public override void Add()
        {
            var ReactDataSet = Window.ReactDataSet;
            ReactDataSet.ReactData.Fx.Add(new XFxData());
            ReactDataSet.ReactDataExtra.Fx.Add(new XFxDataExtra());
        }

        public override string GetCount()
        {
            var ReactDataSet = Window.ReactDataSet;
            if (ReactDataSet == null) return "";
            if (ReactDataSet.ReactData.Fx != null && ReactDataSet.ReactData.Fx.Count > 0)
                return string.Format(" ({0})", ReactDataSet.ReactData.Fx.Count);
            return "";
        }

        public override void OnDrawRight()
        {
            if (selectFxData == null || selectFxDataExtra == null) return;

            GUILayout.BeginHorizontal();
            {
                GUILayout.Label("Type Based on");
                GUILayout.FlexibleSpace();
                //selectFxData.Type = (SkillFxType)EditorGUILayout.EnumPopup(selectFxData.Type);
                GUILayout.Label(selectFxData.Type.ToString());
                GUILayout.Space(20);
            }
            GUILayout.EndHorizontal();

            GUILayout.Space(10);
            GUILayout.BeginHorizontal();
            {
                GUILayout.Label("Fx Object");
                GUILayout.FlexibleSpace();
                selectFxDataExtra.Fx = EditorGUILayout.ObjectField(selectFxDataExtra.Fx, typeof(GameObject), true) as GameObject;
            }
            GUILayout.EndHorizontal();
            if (null == selectFxDataExtra.Fx || !AssetDatabase.GetAssetPath(selectFxDataExtra.Fx).Contains("BundleRes/Effects/"))
            {
                selectFxDataExtra.Fx = null;
            }

            if (null != selectFxDataExtra.Fx)
            {
                string path = AssetDatabase.GetAssetPath(selectFxDataExtra.Fx).Remove(0, 17);
                selectFxData.Fx = path.Remove(path.LastIndexOf('.'));

                GUILayout.Space(10);
                GUILayout.BeginHorizontal();
                {
                    GUILayout.Label("Fx Name");
                    GUILayout.FlexibleSpace();
                    GUILayout.Label(selectFxData.Fx);
                }
                GUILayout.EndHorizontal();

                if (Window.IsHot)
                {
                    ///// Bone ///
                    if (selectFxData.Type == SkillFxType.FirerBased)
                    {
                        if (!selectFxData.StickToGround)
                        {
                            if (selectFxData.Bone == null || selectFxData.Bone.Length == 0)
                                selectFxDataExtra.BindTo = null;

                            GUILayout.Space(10);
                            GUILayout.BeginHorizontal();
                            {
                                GUILayout.Label("Bone");
                                GUILayout.FlexibleSpace();
                                selectFxDataExtra.BindTo = EditorGUILayout.ObjectField(selectFxDataExtra.BindTo, typeof(GameObject), true) as GameObject;
                            }
                            GUILayout.EndHorizontal();

                            if (selectFxDataExtra.BindTo != null)
                            {
                                string name = "";
                                Transform parent = selectFxDataExtra.BindTo.transform;
                                while (parent.parent != null)
                                {
                                    name = name.Length == 0 ? parent.name : parent.name + "/" + name;
                                    parent = parent.parent;
                                }
                                selectFxData.Bone = name.Length > 0 ? name : null;
                            }
                            else
                                selectFxData.Bone = null;
                        }
                        else
                            selectFxData.Bone = null;
                    }
                    else
                        selectFxData.Bone = null;
                }
                else
                {
                    GUILayout.Space(10);
                    GUILayout.BeginHorizontal();
                    {
                        GUILayout.Label("Bone");
                        GUILayout.FlexibleSpace();
                        GUILayout.Label("-- Wait Hot Load --", Window.hintLabelStyle);
                    }
                    GUILayout.EndHorizontal();
                }

                GUILayout.Space(10);
                selectFxData.BlowEnergy = EditorGUILayout.Toggle("BlowEnergy", selectFxData.BlowEnergy);
                if(selectFxData.BlowEnergy)
                {
                    Vector3 veco = new Vector3(selectFxDataExtra.HitOffset.x, selectFxDataExtra.HitOffset.y, selectFxDataExtra.HitOffset.z);
                    veco = EditorGUILayout.Vector3Field("HitOffset", veco);
                    selectFxDataExtra.HitOffset.x = veco.x;
                    selectFxDataExtra.HitOffset.y = veco.y;
                    selectFxDataExtra.HitOffset.z = veco.z;

                    selectFxDataExtra.hitDirection = EditorGUILayout.FloatField("hitDirection", selectFxDataExtra.hitDirection);
                }

                GUILayout.Space(10);
                Vector3 vec = new Vector3(selectFxData.ScaleX, selectFxData.ScaleY, selectFxData.ScaleZ);
                vec = EditorGUILayout.Vector3Field("Scale", vec);
                selectFxData.ScaleX = vec.x;
                selectFxData.ScaleY = vec.y;
                selectFxData.ScaleZ = vec.z;

                if (selectFxData.Alone)
                {
                    GUILayout.Space(10);
                    vec.Set(selectFxData.RotX, selectFxData.RotY, selectFxData.RotZ);
                    vec = EditorGUILayout.Vector3Field("Rotation", vec);
                    selectFxData.RotX = vec.x;
                    selectFxData.RotY = vec.y;
                    selectFxData.RotZ = vec.z;
                }

                if (selectFxData.Type == SkillFxType.TargetBased)
                {
                    GUILayout.Space(10);
                    vec.Set(selectFxData.Target_OffsetX, selectFxData.Target_OffsetY, selectFxData.Target_OffsetZ);
                    vec = EditorGUILayout.Vector3Field("Offset Target", vec);
                    selectFxData.Target_OffsetX = vec.x;
                    selectFxData.Target_OffsetY = vec.y;
                    selectFxData.Target_OffsetZ = vec.z;

                    GUILayout.Space(10);
                    vec.Set(selectFxData.OffsetX, selectFxData.OffsetY, selectFxData.OffsetZ);
                    vec = EditorGUILayout.Vector3Field("Offset Firer when no Target", vec);
                    selectFxData.OffsetX = vec.x;
                    selectFxData.OffsetY = vec.y;
                    selectFxData.OffsetZ = vec.z;
                }
                else
                {
                    GUILayout.Space(10);
                    vec.Set(selectFxData.OffsetX, selectFxData.OffsetY, selectFxData.OffsetZ);
                    vec = EditorGUILayout.Vector3Field("Offset", vec);
                    selectFxData.OffsetX = vec.x;
                    selectFxData.OffsetY = vec.y;
                    selectFxData.OffsetZ = vec.z;
                }


                var ReactDataExtra = Window.ReactDataSet.ReactDataExtra;

                EditorGUILayout.Space();
                float fx_at = ReactDataExtra.ReactClip_Frame * selectFxDataExtra.Ratio;
                EditorGUILayout.BeginHorizontal();
                fx_at = EditorGUILayout.FloatField("Play At", fx_at);
                GUILayout.Label("(frame)");
                GUILayout.Label("", GUILayout.MaxWidth(30));
                EditorGUILayout.EndHorizontal();

                selectFxDataExtra.Ratio = ReactDataExtra.ReactClip_Frame == 0 ? 0 : (fx_at / ReactDataExtra.ReactClip_Frame);
                if (selectFxDataExtra.Ratio > 1) selectFxDataExtra.Ratio = 1;
                EditorGUILayout.BeginHorizontal();
                selectFxDataExtra.Ratio = EditorGUILayout.Slider("Ratio", selectFxDataExtra.Ratio, 0, 1);
                GUILayout.Label("(0~1)", EditorStyles.miniLabel);
                EditorGUILayout.EndHorizontal();

                selectFxData.At = selectFxDataExtra.Ratio * ReactDataExtra.ReactClip_Frame * ReactCommon.FRAME_RATIO;

                ////////////////////////////////
                EditorGUILayout.BeginHorizontal();
                effectTime = EditorGUILayout.FloatField("EffectTime", effectTime);
                GUILayout.Label("", GUILayout.MaxWidth(30));
                EditorGUILayout.EndHorizontal();
                effectTime = effectTime <= 0 ? 0 : effectTime;
                selectFxDataExtra.End_Ratio = ReactDataExtra.ReactClip_Frame == 0 ? 0 : ((fx_at + effectTime) / ReactDataExtra.ReactClip_Frame);
                if (selectFxDataExtra.End_Ratio > 1) selectFxDataExtra.End_Ratio = 1;


                EditorGUILayout.BeginHorizontal();
                float end_fx_at = ReactDataExtra.ReactClip_Frame * selectFxDataExtra.End_Ratio;
                EditorGUILayout.LabelField("End At", end_fx_at.ToString());
                EditorGUILayout.EndHorizontal();

                //EditorGUILayout.BeginHorizontal();
                //selectFxDataExtra.End_Ratio = EditorGUILayout.Slider("End Ratio", selectFxDataExtra.End_Ratio, 0, 1);
                //GUILayout.Label("(0~1)", EditorStyles.miniLabel);
                //EditorGUILayout.EndHorizontal();

                selectFxData.End = selectFxDataExtra.End_Ratio * ReactDataExtra.ReactClip_Frame * ReactCommon.FRAME_RATIO;
                if (selectFxData.End < selectFxData.At) selectFxData.At = selectFxData.End;
                EditorGUILayout.Space();

                selectFxData.StickToGround = EditorGUILayout.Toggle("Stick On Ground", selectFxData.StickToGround);
                /*
                 * follow mode can not use for TargetBased
                 * in that case the fx life-time will not in controlled.
                 */
                if (selectFxData.Type == SkillFxType.FirerBased && !selectFxData.StickToGround)
                    selectFxData.Follow = EditorGUILayout.Toggle("Follow", selectFxData.Follow);
                else
                    selectFxData.Follow = false;
                if (!selectFxData.Follow &&
                    !selectFxData.StickToGround &&
                    selectFxData.Bone == null)
                {
                    selectFxData.Alone = EditorGUILayout.Toggle("Alone", selectFxData.Alone);
                }
                else
                    selectFxData.Alone = false;

                EditorGUILayout.Space();

                EditorGUILayout.BeginHorizontal();
                selectFxData.Destroy_Delay = EditorGUILayout.FloatField("Delay Destroy", selectFxData.Destroy_Delay);
                GUILayout.Label("(s)", ReactCommon.NormalLabelStyle);
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                selectFxData.Shield = EditorGUILayout.Toggle("Shield", selectFxData.Shield);
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                selectFxData.UsingAttackScriptOffset = EditorGUILayout.Toggle("Using Attack Script Offset", selectFxData.UsingAttackScriptOffset);
                EditorGUILayout.EndHorizontal();
            }
            else
            {
                selectFxDataExtra.Fx = null;
            }
        }
    }
}

#endif