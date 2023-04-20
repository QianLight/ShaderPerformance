#if UNITY_EDITOR

using UnityEditor;
using UnityEngine;
using CFUtilPoolLib;

namespace XEditor {


    public class AnimTrackDraw : BaseTrackDraw
    {
        private GUIStyle _labelstyle = null;

        public override string Name { get { return "Anim"; } }

        public override void OnDrawLeft()
        {
            //GUI.color = Window.trackColor;
            //GUI.color = Color.white;
            GUILayout.BeginHorizontal("box");
            {
                GUILayout.Space(30);
                
                string name = Window.ReactDataSet.ReactData.ClipName != null ? Window.ReactDataSet.ReactData.ClipName : "null";
                if (GUILayout.Button(name, Window.trackStyle))
                {
                }

            }
            GUILayout.EndHorizontal();

            GUI.color = ReactCommon.InitColor;
        }

        public override void OnDrawRight()
        {
            var ReactData = Window.ReactDataSet.ReactData;
            var ReactDataExtra = Window.ReactDataSet.ReactDataExtra;
            var ConfigData = Window.ReactDataSet.ConfigData;

            if (_labelstyle == null)
            {
                _labelstyle = new GUIStyle(EditorStyles.boldLabel);
                _labelstyle.fontSize = 13;
            }

            EditorGUILayout.LabelField("Data Path", ReactDataExtra.ScriptPath);

            EditorGUILayout.Space();

            /*********/
            GUILayout.Label("React Settings :", _labelstyle);

            if (ReactData.AnimationSpeedRatio == 0) ReactData.AnimationSpeedRatio = 1;

            EditorGUI.BeginChangeCheck();
            AnimationClip clip = EditorGUILayout.ObjectField("Clip", ReactDataExtra.ReactClip, typeof(AnimationClip), true) as AnimationClip;
            EditorGUI.EndChangeCheck();
            if (clip != null)
            {
                ReactDataExtra.ReactClip = clip;
                ConfigData.ReactClip = AssetDatabase.GetAssetPath(clip);
                ConfigData.ReactClipName = clip.name;
            }
            else
            {
                ReactDataExtra.ReactClip = null;
                ConfigData.ReactClip = "";
                ConfigData.ReactClipName = "";
            }

            EditorGUILayout.LabelField("Clip Name", ConfigData.ReactClip);


            EditorGUI.BeginChangeCheck();
            if (ConfigData.Lock)
            {
                EditorGUILayout.LabelField("Clip Length", (ReactDataExtra.ReactClip_Frame * ReactCommon.FRAME_RATIO).ToString("F3") + "s" + "\t" + ReactDataExtra.ReactClip_Frame.ToString("F1") + "(frame)");
                ReactData.AnimationSpeedRatio = EditorGUILayout.Slider("Clip Ratio", ReactData.AnimationSpeedRatio, 0.5f, 2.0f);
            }
            else if (ReactDataExtra.ReactClip != null)
            {
                EditorGUILayout.LabelField("Clip Length", ReactDataExtra.ReactClip.length.ToString("F3") + "s" + "\t" + (ReactDataExtra.ReactClip.length / ReactCommon.FRAME_RATIO).ToString("F1") + "(frame)");
                ReactDataExtra.ReactClip_Frame = (ReactData.Time / ReactCommon.FRAME_RATIO);
            }
            EditorGUILayout.BeginHorizontal();
            if (ConfigData.Lock)
            {
                ReactData.Time = ReactDataExtra.ReactClip_Frame * ReactCommon.FRAME_RATIO;
                EditorGUILayout.LabelField("React Time", ReactData.Time.ToString("F3"));
            }
            else
            {
                ReactData.AnimationSpeedRatio = 1;
                ReactData.Time = EditorGUILayout.FloatField("React Time", ReactData.Time);
                if (ReactData.Time <= 0 && ReactDataExtra.ReactClip != null)
                    ReactData.Time = ReactDataExtra.ReactClip.length;
            }

            if (ReactDataExtra.ReactClip != null)
                ReactDataExtra.ReactClip_Frame = (ReactDataExtra.ReactClip.length * ReactData.AnimationSpeedRatio / ReactCommon.FRAME_RATIO);
            else
            {
                ReactDataExtra.ReactClip_Frame = ReactData.Time * ReactData.AnimationSpeedRatio / ReactCommon.FRAME_RATIO;
            }
           
            GUILayout.Label("(s)");
            EditorGUILayout.EndHorizontal();

            //draw second clip
            GUILayout.Space(20);

            EditorGUI.BeginChangeCheck();
            AnimationClip clip2 = EditorGUILayout.ObjectField("Clip2", ReactDataExtra.ReactClip2, typeof(AnimationClip), true) as AnimationClip;
            if (EditorGUI.EndChangeCheck())
            {
                if (clip2 != null)
                {
                    ReactDataExtra.ReactClip2 = clip2;
                    ConfigData.ReactClip2 = AssetDatabase.GetAssetPath(clip2);
                    ConfigData.ReactClipName2 = clip2.name;
                }
                else
                {
                    ReactDataExtra.ReactClip2 = null;
                    ConfigData.ReactClip2 = "";
                    ConfigData.ReactClipName2 = "";
                }
            }

            EditorGUILayout.LabelField("Clip2 Name", ConfigData.ReactClip2);

            ConfigData.Lock = EditorGUILayout.Toggle("(Lock)", ConfigData.Lock);


            ReactData.ClipNamePostFix = EditorGUILayout.TextField("ClipNamePostFix", ReactData.ClipNamePostFix);

            DrawLogicGUI();
        }

        void DrawLogicGUI()
        {
            var ReactData = Window.ReactDataSet.ReactData;
            var ReactDataExtra = Window.ReactDataSet.ReactDataExtra;
            var ConfigData = Window.ReactDataSet.ConfigData;

            GUILayout.Space(10);
            /*********/
            GUILayout.Label("Logic Settings :", _labelstyle);
            GUILayout.Space(5);

            if (ReactDataExtra.ReactClip != null && ReactDataExtra.ReactClip.length > 0)
            {
                ReactData.AnimTime = ReactDataExtra.ReactClip.length;
                ReactData.FadeTime = EditorGUILayout.FloatField("Fade Time In", ReactData.FadeTime);
                ReactData.FadeTimeOut = EditorGUILayout.FloatField("Fade Time Out", ReactData.FadeTimeOut);
            }
            else
            {
                ReactData.AnimTime = 0f;
            }

            GUILayout.BeginHorizontal();
            ReactData.InputFreezed = EditorGUILayout.Toggle("InputFreezed", ReactData.InputFreezed);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            ReactData.StopAnimAtEnd = EditorGUILayout.Toggle("StopAnimAtEnd", ReactData.StopAnimAtEnd);
            GUILayout.EndHorizontal();

            //float pose_at = ReactDataExtra.ReactClip_Frame * ReactDataExtra.PoseFrameRatio;
            //EditorGUILayout.BeginHorizontal();
            //pose_at = EditorGUILayout.FloatField("Pose Frame", pose_at);
            //GUILayout.Label("(frame)", ReactCommon.NormalLabelStyle);
            //GUILayout.Label("", GUILayout.MaxWidth(30));
            //EditorGUILayout.EndHorizontal();

            //EditorGUILayout.BeginHorizontal();
            //ReactDataExtra.PoseFrameRatio = EditorGUILayout.Slider("Pose Ratio", ReactDataExtra.PoseFrameRatio, 0, 1);
            //GUILayout.Label("(0~1)", EditorStyles.miniLabel);
            //EditorGUILayout.EndHorizontal();

            //ReactData.PoseFrame = (int)pose_at;

            ReactData.PoseFrame = EditorGUILayout.IntField("Pose Frame",ReactData.PoseFrame);

            /*********/
            GUILayout.Space(10);
            GUILayout.Label("SuperArmor Settings :", _labelstyle);
            GUILayout.Space(5);
            ReactData.AnimLayerType = (EReactAnimLayerType)EditorGUILayout.EnumPopup("Anim Layer Type", ReactData.AnimLayerType);

            //if (Window.IsHot)
            {
                if (ReactData.AnimLayerType != EReactAnimLayerType.None)
                {
                    GUILayout.BeginHorizontal();
                    ReactData.LayerWeight = EditorGUILayout.Slider("LayerWeight", ReactData.LayerWeight, 0f, 1f);
                    GUILayout.EndHorizontal();

                    ReactDataExtra.Mask = EditorGUILayout.ObjectField("AvatarMask", ReactDataExtra.Mask, typeof(AvatarMask), true) as AvatarMask;

                    if (ReactDataExtra.Mask != null)
                    {
                        string path = AssetDatabase.GetAssetPath(ReactDataExtra.Mask).Remove(0, 17);
                        ReactData.AvatarMask = path.Remove(path.LastIndexOf('.'));

                        GUILayout.BeginHorizontal();
                        {

                            GUILayout.Space(5);
                            GUILayout.Label("AvatarMask", ReactCommon.NormalLabelStyle);
                            GUILayout.FlexibleSpace();
                            GUILayout.Label(ReactData.AvatarMask, ReactCommon.NormalLabelStyle);
                            GUILayout.Space(5);
                        }
                        GUILayout.EndHorizontal();

                        GUILayout.Space(5);
                        GUILayout.BeginHorizontal();
                        {
                            GUILayout.Space(5);
                            if (GUILayout.Button("MaskEditor", GUILayout.Width(100)))
                            {
                                MskEditorClicked();
                            }
                        }
                        GUILayout.EndHorizontal();

                        
                    }
                    else
                    {
                        ReactData.AvatarMask = "";
                    }
                }
            }

            //AvatarMask mask = null;

            ///更改动作
            //GUILayout.BeginHorizontal();
            //{
            //    GUILayout.Space(15);
            //    if (GUILayout.Button("SetRefPose", GUILayout.Width(100)))
            //    {
            //        if (ReactDataExtra.ReactClip != null)
            //        {
            //            if (!ReactDataExtra.SettedAdditiveRefrencePose)
            //            {
            //                UnityEditor.AnimationUtility.SetAdditiveReferencePose(ReactDataExtra.ReactClip, ReactDataExtra.ReactClip, 1f);
            //                ReactDataExtra.SettedAdditiveRefrencePose = true;
            //            }
            //            else
            //            {
            //                UnityEditor.AnimationUtility.SetAdditiveReferencePose(ReactDataExtra.ReactClip, ReactDataExtra.ReactClip, 0f);
            //                ReactDataExtra.SettedAdditiveRefrencePose = false;
            //            }

            //        }
            //    }
            //    GUILayout.FlexibleSpace();

            //    GUILayout.Label("点击后记得上传修改过的动作", ReactCommon.NormalLabelStyle);
            //    GUILayout.Space(10);
            //}
            //GUILayout.EndHorizontal();

            GUILayout.Space(5);

            ReactData.ExtraScript = EditorGUILayout.TextField("Extra Script ", ReactData.ExtraScript);

            //GUILayout.BeginHorizontal();
            //{
            //    GUILayout.Space(5);
            //    if (GUILayout.Button("Extra Script", GUILayout.Width(100)))
            //    {
            //        AnimTest();
            //    }
            //    GUILayout.FlexibleSpace();
            //    testClip = EditorGUILayout.ObjectField("", testClip, typeof(AnimationClip), true) as AnimationClip;
            //}
            //GUILayout.EndHorizontal();


            GUILayout.Space(5);

            GUILayout.BeginHorizontal();
            {
                GUILayout.Space(5);
                if (GUILayout.Button("TestAnim", GUILayout.Width(100)))
                {
                    AnimTest();
                }
                GUILayout.FlexibleSpace();
                testClip = EditorGUILayout.ObjectField("", testClip, typeof(AnimationClip), true) as AnimationClip;
            }
            GUILayout.EndHorizontal();

        }

        XReactMaskEditor maskWid = null;

        void MskEditorClicked()
        {
            if (maskWid == null)
            {
                var ReactData = Window.ReactDataSet.ReactData;
                if (ReactData != null)
                {
                    maskWid = EditorWindow.GetWindow<XReactMaskEditor>(ReactData.AvatarMask + ".mask");
                    maskWid.position = new Rect(Window.Window.position.x + Window.Window.position.width, Window.Window.position.y, 500, Window.Window.position.height);
                    maskWid.wantsMouseMove = true;
                    maskWid.Init(Window.Window);
                    maskWid.Show();
                    maskWid.Repaint();
                }

            }
            else
            {
                maskWid.Close();
                maskWid = null;
            }
        }

        AnimationClip testClip = null;
        void AnimTest()
        {
            if (testClip != null)
            {
                var ReactHoster = GameObject.FindObjectOfType<XReactEntity>();
                if (ReactHoster != null)
                {
                    ReactHoster.ChangeAnim(testClip);
                }
            }
        }
    }
}

#endif