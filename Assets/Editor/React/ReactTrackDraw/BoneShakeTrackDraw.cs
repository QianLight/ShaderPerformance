#if UNITY_EDITOR

using UnityEditor;
using UnityEngine;
using CFUtilPoolLib;
using CFClient;
using System.Collections.Generic;

namespace XEditor {


    public class BoneShakeTrackDraw : BaseTrackDraw
    {
        public int selectTrack = -1;
        public XBoneShakeData selectData = null;
        public XBoneShakeExtra selectDataExtra = null;
        private bool fold = true;

        private bool testBoneShake = false;

        PresentConfigTable.RowData data = null;

        public override string Name { get { return "Bone"; } }

        public override void OnDrawLeft()
        {
            var ReactDataSet = Window.ReactDataSet;
            if (ReactDataSet.ReactData.BoneShakeData != null && ReactDataSet.ReactData.BoneShakeData.Count > 0)
            {
                for (int i = 0; i < ReactDataSet.ReactData.BoneShakeData.Count; ++i)
                {
                    var curData = ReactDataSet.ReactData.BoneShakeData[i];
                    curData.Index = i;

                    GUI.color = selectTrack == i ? Window.selectdColor : Window.trackColor;
                    GUILayout.BeginHorizontal("box");
                    {
                        GUILayout.Space(30);

                        string bone = curData.Bone == null ? "" : curData.Bone;
                        if (GUILayout.Button(bone, Window.trackStyle))
                        {
                            selectTrack = i;

                            selectData = curData;
                            selectDataExtra = ReactDataSet.ReactDataExtra.BoneShake[i];
                        }

                        GUI.color = Window.trackDropColor;
                        if (GUILayout.Button(" Del ", Window.trackDropStyle))
                        {
                            selectTrack = -1;
                            selectData = null;
                            selectDataExtra = null;

                            //del
                            ReactDataSet.ReactData.BoneShakeData.RemoveAt(i);
                            ReactDataSet.ReactDataExtra.BoneShake.RemoveAt(i);
                        }
                    }
                    GUILayout.EndHorizontal();
                }

                //auto select.
                if (selectTrack == -1 && ReactDataSet.ReactData.BoneShakeData.Count > 0)
                {
                    selectTrack = 0;
                    selectData = ReactDataSet.ReactData.BoneShakeData[0];
                    selectDataExtra = ReactDataSet.ReactDataExtra.BoneShake[0];
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
            ReactDataSet.ReactData.BoneShakeData.Add(new XBoneShakeData());
            ReactDataSet.ReactDataExtra.BoneShake.Add(new XBoneShakeExtra());
        }

        public override string GetCount()
        {
            var ReactDataSet = Window.ReactDataSet;
            if (ReactDataSet == null) return "";
            if (ReactDataSet.ReactData.BoneShakeData != null && ReactDataSet.ReactData.BoneShakeData.Count > 0)
                return string.Format(" ({0})", ReactDataSet.ReactData.BoneShakeData.Count);
            return "";
        }

        public override void OnDrawRight()
        {

            if (selectData == null || selectDataExtra == null) return;

            var ShakeData = selectData;

            GUILayout.BeginHorizontal();
            {
                GUILayout.Space(5);
                GUILayout.Label("BonePath", ReactCommon.NormalLabelStyle);
                GUILayout.FlexibleSpace();
                GUILayout.Label(ShakeData.Bone, ReactCommon.NormalLabelStyle);
                GUILayout.Space(5);
            }
            GUILayout.EndHorizontal();

            GUILayout.Space(5);
            if (Window.IsHot)
            {
                if (ShakeData.Bone == null || ShakeData.Bone.Length == 0)
                    selectDataExtra.BindTo = null;
                selectDataExtra.BindTo = EditorGUILayout.ObjectField("Bone", selectDataExtra.BindTo, typeof(GameObject), true) as GameObject;
                if (selectDataExtra.BindTo != null)
                {
                    string name = "";
                    Transform parent = selectDataExtra.BindTo.transform;
                    while (parent.parent != null)
                    {
                        name = name.Length == 0 ? parent.name : parent.name + "/" + name;
                        parent = parent.parent;
                    }
                    ShakeData.Bone = name.Length > 0 ? name : null;
                }
                else
                    ShakeData.Bone = null;
            }
            else
            {
                GUILayout.BeginHorizontal();
                {
                    GUILayout.Space(5);
                    GUILayout.Label("Bone", ReactCommon.NormalLabelStyle);
                    GUILayout.FlexibleSpace();
                    GUILayout.Label("-- Wait Hot Load --", Window.hintLabelStyle);
                    GUILayout.Space(5);
                }
                GUILayout.EndHorizontal();
            }

            GUILayout.Space(5);

            GUILayout.BeginHorizontal();
            {
                if (selectDataExtra.BindTo == null)
                {
                    ShakeData.BoneName = EditorGUILayout.TextField("BoneName:", ShakeData.BoneName);

                    if (GUILayout.Button("verify"))
                    {
                        XEntityPresentation.RowData xEntity = XAnimationLibrary.AssociatedAnimations((uint)Window.ReactDataSet.ConfigData.Player);
                        data = XReactLibrary.PresentConfig.GetPresentConfig(xEntity.Prefab, true, ShakeData.BoneName, xEntity.BoneType);

                        //if (data != null)
                        //{
                        //    ShakeData.Bone = data.Value;
                        //}
                    }
                    GUILayout.Label(data != null ? "存在" : "不存在");
                }

            }
            GUILayout.EndHorizontal();
  

            ShakeData.LifeTime = EditorGUILayout.FloatField("Time", ShakeData.LifeTime);
            ShakeData.Frequency = EditorGUILayout.FloatField("Frequency", ShakeData.Frequency);
            //ShakeData.Redius = EditorGUILayout.FloatField("Redius", ShakeData.Redius);

            testBoneShake = EditorGUILayout.Toggle("testBoneShake", testBoneShake);
            if(testBoneShake)
            {
                selectDataExtra.direction = EditorGUILayout.Vector3Field("direction", selectDataExtra.direction);
                selectDataExtra.position = EditorGUILayout.Vector3Field("position", selectDataExtra.position);
                selectDataExtra.duration = EditorGUILayout.FloatField("duration", selectDataExtra.duration);
                selectDataExtra.intensity = EditorGUILayout.FloatField("intensity", selectDataExtra.intensity);
                if (GUILayout.Button("DrawBone"))
                {
                    if(selectDataExtra.BindTo)
                    {
                        BoneSpreadDataNeedParam param = new BoneSpreadDataNeedParam(selectDataExtra.BindTo.transform, 
                            ShakeData.boneChainIntensity, ShakeData.boneChainIntensityDown);
                        string aa = "";
                        XReactDataHostBuilder.boneShake?.DrawBoneChainGizmos(param, out aa);
                    }
                    else
                    {
                        Transform bone = XReactDataHostBuilder.singleton.GetBone(Window.ReactDataSet, ShakeData.BoneName,
                            XReactDataHostBuilder.hoster.transform);
                        BoneSpreadDataNeedParam param = new BoneSpreadDataNeedParam(bone, ShakeData.boneChainIntensity, ShakeData.boneChainIntensityDown);

                        string aa = "";
                        XReactDataHostBuilder.boneShake?.DrawBoneChainGizmos(param, out aa);
                    }
                }
            }

            EditorGUILayout.Space();

            ShakeData.AmplitudeX = EditorGUILayout.FloatField("Amplitude X", ShakeData.AmplitudeX);
            ShakeData.AmplitudeY = EditorGUILayout.FloatField("Amplitude Y", ShakeData.AmplitudeY);
            ShakeData.AmplitudeZ = EditorGUILayout.FloatField("Amplitude Z", ShakeData.AmplitudeZ);


            //EditorGUILayout.Space();
            //ShakeData.FadeIn = EditorGUILayout.FloatField("FadeIn", ShakeData.FadeIn);
            //ShakeData.FadeOut = EditorGUILayout.FloatField("FadeOut", ShakeData.FadeOut);

            EditorGUILayout.CurveField("timeFadeIntensity", ShakeData.timeFadeIntensity);
            EditorGUILayout.CurveField("boneChainIntensity", ShakeData.boneChainIntensity);
            EditorGUILayout.CurveField("timeFadeIntensityDown", ShakeData.timeFadeIntensityDown);
            EditorGUILayout.CurveField("boneChainIntensityDown", ShakeData.boneChainIntensityDown);

            ShakeData.randomIntensity = EditorGUILayout.FloatField("randomIntensity", ShakeData.randomIntensity);
            ShakeData.randomPercent = EditorGUILayout.FloatField("randomPercent", ShakeData.randomPercent);

            EditorGUILayout.BeginHorizontal();
            ShakeData.At = EditorGUILayout.FloatField("Play At ", ShakeData.At);
            GUILayout.Label("(s)", ReactCommon.NormalLabelStyle);
            GUILayout.Label("", GUILayout.MaxWidth(30));
            EditorGUILayout.EndHorizontal();

            GUILayout.Space(10);
            fold = EditorGUILayout.Foldout(fold, "Ignore Bones");
            if (fold)
            {
                ///. ignore bones
                GUILayout.Space(5);
                //GUILayout.Label("Ignore Bones", ReactCommon.NormalLabelStyle);
                {
                    if ((ShakeData.IgnoreBones == null || ShakeData.IgnoreBones.Count == 0 && selectDataExtra.IgnoreBones != null))
                        selectDataExtra.IgnoreBones.Clear();

                    if (ShakeData.IgnoreBones != null)
                    {
                        for (int t = 0; t < ShakeData.IgnoreBones.Count; ++t)
                        {
                            if (Window.IsHot)
                            {

                                if (selectDataExtra.IgnoreBones.Count != ShakeData.IgnoreBones.Count)
                                {
                                    XDebug.singleton.AddErrorLog("err: " + ShakeData.IgnoreBones.Count.ToString() + "  " + selectDataExtra.IgnoreBones.Count.ToString()); ;
                                    ShakeData.IgnoreBones.Clear();
                                    selectDataExtra.IgnoreBones.Clear();
                                }

                                selectDataExtra.IgnoreBones[t] = EditorGUILayout.ObjectField("Bone", selectDataExtra.IgnoreBones[t], typeof(GameObject), true) as GameObject;

                                if (selectDataExtra.IgnoreBones[t] != null)
                                {
                                    string name = "";
                                    Transform parent = selectDataExtra.IgnoreBones[t].transform;
                                    while (parent.parent != null)
                                    {
                                        name = name.Length == 0 ? parent.name : parent.name + "/" + name;
                                        parent = parent.parent;
                                    }
                                    ShakeData.IgnoreBones[t] = name.Length > 0 ? name : null;
                                }
                            }

                            GUILayout.BeginHorizontal();
                            {
                                GUILayout.Space(10);

                                GUILayout.Label(ShakeData.IgnoreBones[t]);
                                GUILayout.Space(5);

                                if (GUILayout.Button("Del(-)"))
                                {
                                    DelIgnoreBone(t);
                                }

                            }
                            GUILayout.EndHorizontal();

                            GUILayout.Space(15);
                        }
                    }
                }



                GUILayout.BeginHorizontal();
                {
                    GUILayout.FlexibleSpace();
                    if (GUILayout.Button("Add(+)", GUILayout.Width(60), GUILayout.Height(30)))
                    {
                        AddIgnoreBone();
                    }
                    GUILayout.FlexibleSpace();
                }
                GUILayout.EndHorizontal();
            }

            
        }

        void AddIgnoreBone()
        {
            if (selectData.IgnoreBones == null)
            {
                selectData.IgnoreBones = new System.Collections.Generic.List<string>();
            }
            selectData.IgnoreBones.Add("");
            selectDataExtra.IgnoreBones.Add(null);
        }

        void DelIgnoreBone(int index)
        {
            selectData.IgnoreBones.RemoveAt(index);
            selectDataExtra.IgnoreBones.RemoveAt(index);
        }
    }
}

#endif