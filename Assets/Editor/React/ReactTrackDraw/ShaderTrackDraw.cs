#if UNITY_EDITOR

using System;
using CFEngine;
using CFEngine.Editor;
using CFUtilPoolLib;
using UnityEditor;
using UnityEngine;

namespace XEditor
{

    public class ShaderTrackDraw : BaseTrackDraw
    {
        enum EntityEffectType
        {
            RimLight = 0,
            AddColor = 1,
            Fade = 2,
            Hide = 3,
            Dissolve = 4,
            Gear2 =5,
            Medusa = 6,
            DitherFade = 7,
            Disappear = 8,
            TexSwitch = 9,
            SfxDisplay = 10,
            Appear = 11,
            Scale = 12,
            Color = 13,
            RnederTarget = 14,
            MatAdd = 15,
            OutlineColorReplace = 16,
            OutlineColorMultiply = 17,
            ShaderKeyword = 18,
            HitShader = 19,
            HitFadeShader = 20
        }

        public int selectTrack = -1;
        public XShaderEffData selectData = null;

        // GUIContent colorTitle = new GUIContent ("Color");
        // RenderEffect re = new RenderEffect ();

        private EntityEffectType selectType = EntityEffectType.AddColor;
        private MatEffectData med;
        private MatEffectNodeInfo matEffectNode;
        public override string Name { get { return "Shader"; } }

        public override void Init (XReactMainWindow main)
        {
            base.Init (main);
            EffectConfig.instance.LoadEffectTemplate (null);
        }

        public void BuildData ()
        {
            if (selectData != null)
            {
                matEffectNode = EffectConfig.instance.FindNode (selectData.uniqueID);
                if (med == null)
                {
                    med = new MatEffectData ();
                }
                med.asset = null;
                med.x = selectData.x;
                med.y = selectData.y;
                med.z = selectData.z;
                med.w = selectData.w;
                med.partMask = selectData.partMask;
                med.param = selectData.param;
                string ext = ResObject.GetExt((byte)med.x);
                med.path = Window.GetPath(selectData.path, ext);
                EffectConfig.InitResData(med, selectData.path, ext);
            }

        }
        public override void OnDrawLeft ()
        {
            var ReactDataSet = Window.ReactDataSet;
            if (ReactDataSet.ReactData.ShaderEffData != null && ReactDataSet.ReactData.ShaderEffData.Count > 0)
            {
                for (int i = 0; i < ReactDataSet.ReactData.ShaderEffData.Count; ++i)
                {
                    var curData = ReactDataSet.ReactData.ShaderEffData[i];
                    curData.Index = i;

                    GUI.color = selectTrack == i ? Window.selectdColor : Window.trackColor;
                    GUILayout.BeginHorizontal ("box");
                    {
                        GUILayout.Space (30);

                        if (GUILayout.Button (((EntityEffectType) curData.uniqueID).ToString (), Window.trackStyle))
                        {
                            selectTrack = i;

                            selectData = curData;
                            BuildData ();
                        }

                        GUI.color = Window.trackDropColor;
                        if (GUILayout.Button (" Del ", Window.trackDropStyle))
                        {
                            selectTrack = -1;
                            selectData = null;

                            //del
                            ReactDataSet.ReactData.ShaderEffData.RemoveAt (i);
                        }
                    }
                    GUILayout.EndHorizontal ();
                }

                //auto select.
                if (selectTrack == -1 && ReactDataSet.ReactData.ShaderEffData.Count > 0)
                {
                    selectTrack = 0;
                    selectData = ReactDataSet.ReactData.ShaderEffData[0];
                    BuildData ();
                }
            }

            GUILayout.BeginHorizontal ();
            {
                GUILayout.FlexibleSpace ();
                selectType = (EntityEffectType) EditorGUILayout.EnumPopup (selectType, GUILayout.MaxWidth (100), GUILayout.Height (30));
                if (GUILayout.Button ("Add(+)", GUILayout.Width (60), GUILayout.Height (30)))
                {
                    Add ();
                }
                GUILayout.FlexibleSpace ();
            }
            GUILayout.EndHorizontal ();

            GUI.color = ReactCommon.InitColor;
        }

        public override void Add ()
        {
            var ReactDataSet = Window.ReactDataSet;
            string effectName = selectType.ToString ();
            var megNode = EffectConfig.instance.FindEffectGraph (effectName);
            if (megNode != null)
            {
                if (megNode.nodeList.Count > 0)
                {
                    var n = megNode.nodeList[0];
                    if (n.node != null)
                    {
                        var effect = new XShaderEffData ();
                        effect.uniqueID = n.uniqueID;
                        effect.partMask = 0xffffffff;
                        n.node.InitData (ref effect.x,
                            ref effect.y,
                            ref effect.z,
                            ref effect.w,
                            ref effect.path,
                            ref effect.param);
                        ReactDataSet.ReactData.ShaderEffData.Add (effect);
                    }
                }

            }
            else
            {
                DebugLog.AddErrorLog2 ("effect not exist:{0}", effectName);
            }

        }

        public override string GetCount ()
        {
            var ReactDataSet = Window.ReactDataSet;
            if (ReactDataSet == null) return "";
            if (ReactDataSet.ReactData.ShaderEffData != null && ReactDataSet.ReactData.ShaderEffData.Count > 0)
                return string.Format (" ({0})", ReactDataSet.ReactData.ShaderEffData.Count);
            return "";
        }

        public override void OnDrawRight ()
        {
            if (selectData == null) return;

            EditorGUILayout.BeginHorizontal ();
            EditorGUILayout.LabelField ("Play At", GUILayout.MaxWidth (100));
            selectData.At = EditorGUILayout.FloatField ("", selectData.At, GUILayout.MaxWidth (200));
            GUILayout.Label ("(s)", ReactCommon.NormalLabelStyle);
            GUILayout.Label ("", GUILayout.MaxWidth (30));
            EditorGUILayout.EndHorizontal ();

            EditorGUILayout.BeginHorizontal ();
            EditorGUILayout.LabelField ("Effect Time", GUILayout.MaxWidth (100));
            selectData.LifeTime = EditorGUILayout.FloatField ("", selectData.LifeTime, GUILayout.MaxWidth (200));
            GUILayout.Label ("(s)", ReactCommon.NormalLabelStyle);
            GUILayout.Label ("", GUILayout.MaxWidth (30));
            EditorGUILayout.EndHorizontal ();

            EditorGUILayout.BeginHorizontal ();
            EditorGUILayout.LabelField ("UniqueID", GUILayout.MaxWidth (100));
            EditorGUILayout.LabelField (selectData.uniqueID.ToString (), GUILayout.MaxWidth (100));
            EditorGUILayout.EndHorizontal ();

            EditorGUILayout.BeginHorizontal ();
            EditorGUILayout.LabelField ("Priority", GUILayout.MaxWidth (100));
            selectData.Priority = EditorGUILayout.IntField (selectData.Priority, GUILayout.MaxWidth (200));
            EditorGUILayout.EndHorizontal ();

            EditorGUILayout.BeginHorizontal ();
            EditorGUILayout.LabelField ("FadeIn", GUILayout.MaxWidth (100));
            selectData.FadeIn = EditorGUILayout.Slider (selectData.FadeIn, 0.0f, 2, GUILayout.MaxWidth (200));
            EditorGUILayout.EndHorizontal ();

            EditorGUILayout.BeginHorizontal ();
            EditorGUILayout.LabelField ("FadeOut", GUILayout.MaxWidth (100));
            selectData.FadeOut = EditorGUILayout.Slider (selectData.FadeOut, 0.0f, 2, GUILayout.MaxWidth (200));
            EditorGUILayout.EndHorizontal ();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Totaltime(FadeIn + Effect Time + FadeOut)", GUILayout.MaxWidth(250));
            EditorGUILayout.LabelField((selectData.FadeIn + selectData.LifeTime + selectData.FadeOut).ToString(), GUILayout.MaxWidth(100));
            EditorGUILayout.EndHorizontal();

            EditorGUI.BeginChangeCheck ();
            PartConfig.instance.OnPartGUI ("", ref med.partMask);
            if (EditorGUI.EndChangeCheck ())
            {
                selectData.partMask = med.partMask;
            } 
            if (matEffectNode != null)
            {
                EditorGUI.BeginChangeCheck ();
                matEffectNode.node.OnGUI (med);
                if (EditorGUI.EndChangeCheck ())
                {
                    selectData.path = med.asset != null ? med.asset.name.ToLower() : "";
                    selectData.x = med.x;
                    selectData.y = med.y;
                    selectData.z = med.z;
                    selectData.w = med.w;
                    selectData.param = med.param;
                    ResSave();
                }
            }

            EditorGUILayout.BeginHorizontal();
            selectData.UsingAttackScriptOffset = EditorGUILayout.Toggle("Using Attack Script Offset", selectData.UsingAttackScriptOffset);
            EditorGUILayout.EndHorizontal();

            // re = TurnXShaderEffData2RenderEffect (ref selectData);

            // EntityEffectType type = (EntityEffectType) selectData.UniqueID;
            // EditorGUI.BeginChangeCheck ();
            // selectData.UniqueID = (short) ((EntityEffectType) EditorGUILayout.EnumPopup ("Effect Type", type));

            // if (EditorGUI.EndChangeCheck ())
            // {
            //     re = EffectPreview.epList[(int) selectData.UniqueID].DefaultEffect ();

            // }

            // EffectPreview.epList[(int) type].OnEffectGUI (ref re);

            // TurnRenderEffect2XShaderEffData (ref selectData, ref re);
        }

        public override void OnSave()
        {
            base.OnSave();
            if (med != null)
            {
                if (!string.IsNullOrEmpty(med.path) &&
                    !string.IsNullOrEmpty(selectData.path))
                {

                    string ext = ResObject.GetExt((byte)med.x);
                    Window.resMap[selectData.path + ext] = med.path;
                }
            }
        }

        void ResSave()
        {
            if (!string.IsNullOrEmpty(med.path) &&
                      !string.IsNullOrEmpty(selectData.path))
            {

                string ext = ResObject.GetExt((byte)med.x);
                Window.resMap[selectData.path + ext] = med.path;
            }
        }
        // public static void TurnRenderEffect2XShaderEffData (ref XShaderEffData data, ref RenderEffect re)
        // {
        //     data.Data0 = re.data.x;
        //     data.Data1 = re.data.y;
        //     data.Data2 = re.data.z;
        //     data.Data3 = re.data.w;
        //     data.Data4 = re.data1.x;
        //     data.Data5 = re.data1.y;
        //     data.Data6 = re.data1.z;
        //     data.Data7 = re.data1.w;
        //     data.FadeIn = re.timeData.x;
        //     data.FadeOut = re.timeData.y;
        //     data.LifeTime = re.timeData.z;
        //     // data.Param = re.timeData.w;
        //     data.Priority = re.priority;
        //     data.PartFlag = re.partFlag;
        // }

        // public static RenderEffect TurnXShaderEffData2RenderEffect (ref XShaderEffData data)
        // {
        //     RenderEffect re = new RenderEffect ();
        //     re.data = new Vector4 (data.Data0, data.Data1, data.Data2, data.Data3);
        //     re.data1 = new Vector4 (data.Data4, data.Data5, data.Data6, data.Data7);
        //     // re.timeData = new Vector4 (data.FadeIn, data.FadeOut, data.LifeTime, data.Param);
        //     re.priority = data.Priority;
        //     re.partFlag = data.PartFlag;
        //     return re;
        // }
    }
}

#endif