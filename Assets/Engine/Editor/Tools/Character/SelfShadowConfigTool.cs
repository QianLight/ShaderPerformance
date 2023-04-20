using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

using SelfShadowListEdior = CFEngine.Editor.CommonListEditor<CFEngine.SelfShadowConfigs>;
using SelfShadowListContext = CFEngine.Editor.AssetListContext<CFEngine.SelfShadowConfigs>;

namespace CFEngine.Editor
{

    public partial class SelfShadowConfigTool : BaseConfigTool<SelfShadowEditorConfig>
    {
        private static Transform selfShadowLight;
        private SelfShadowListContext selfShadowContext;
        private SelfShadowListEdior selfShadowEditor;
        private MiscConfig miscConfig;
        static Material matSelf;
        public override void OnInit ()
        {
            base.OnInit ();
            config = SelfShadowEditorConfig.instance;
            InitSelfShadowLight ();

            selfShadowContext.name = "SelfShadows";
            selfShadowContext.elementGUI = SelfShadowConfigListGUI;
            selfShadowContext.needDelete = true;
            selfShadowContext.needAdd = true;
            config.configs.name = "SelfShadows";
            selfShadowEditor = new SelfShadowListEdior (config.configs, ref selfShadowContext);
            string path = string.Format ("{0}Config/MiscConfig.asset", LoadMgr.singleton.EngineResPath);
            miscConfig = AssetDatabase.LoadAssetAtPath<MiscConfig> (path);
            if (miscConfig == null)
            {
                miscConfig = ScriptableObject.CreateInstance<MiscConfig> ();
                miscConfig = CommonAssets.CreateAsset<MiscConfig> (path, ".asset", miscConfig);
                WorldSystem.miscConfig = miscConfig;
            }

        }
        private static void SetFlag (ref SelfShadowConfig config, uint f, bool add)
        {
            if (add)
            {
                config.flag |= f;
            }
            else
            {
                config.flag &= ~(f);
            }
        }
        private static void InitSelfShadowLight ()
        {
            GameObject go = GameObject.Find ("_SelfShadowLights");
            if (go == null)
            {
                go = new GameObject ("_SelfShadowLights");
            }
            selfShadowLight = go.transform;
        }

        private static Transform FindShadowLight (string name)
        {
            if (selfShadowLight == null)
            {
                InitSelfShadowLight ();
            }
            Transform t = null;
            if (selfShadowLight != null)
            {
                t = selfShadowLight.Find (name);
                if (t == null)
                {
                    GameObject go = new GameObject (name);
                    t = go.transform;
                    t.parent = selfShadowLight;
                }
                var shadowLight = t.GetComponent<SelfShadowLight> ();
                if (shadowLight == null)
                {
                    t.gameObject.AddComponent<SelfShadowLight> ();
                }
            }
            return t;
        }

        private void ESMGUI ()
        {
            if (miscConfig != null)
            {
                EditorGUILayout.BeginHorizontal ();
                miscConfig.param.x = EditorGUILayout.Slider ("Bias", miscConfig.param.x, 0, 0.1f);
                if (GUILayout.Button ("R", GUILayout.MaxWidth (20)))
                {
                    miscConfig.param.x = 0;
                }
                EditorGUILayout.EndHorizontal ();
                EditorGUILayout.BeginHorizontal ();
                miscConfig.param.y = EditorGUILayout.Slider ("BlurSampleDistance", miscConfig.param.y, 0, 0.2f);
                if (GUILayout.Button ("R", GUILayout.MaxWidth (20)))
                {
                    miscConfig.param.y = 0;
                }
                EditorGUILayout.EndHorizontal ();
                EditorGUILayout.BeginHorizontal ();
                miscConfig.param.z = EditorGUILayout.IntSlider ("BlurIteration", (int) miscConfig.param.z, 0, 2);
                if (GUILayout.Button ("R", GUILayout.MaxWidth (20)))
                {
                    miscConfig.param.z = 0;
                }
                EditorGUILayout.EndHorizontal ();
                EditorGUILayout.BeginHorizontal ();
                miscConfig.param.w = EditorGUILayout.Slider ("ESMExponent", miscConfig.param.w, 0, 100);
                if (GUILayout.Button ("R", GUILayout.MaxWidth (20)))
                {
                    miscConfig.param.w = 60;
                }
                EditorGUILayout.EndHorizontal ();
            }

        }

        public static void DrawSelfShadowConfig (string name, ref SelfShadowConfig config, ref Transform customLightTrans)
        {
            EditorGUILayout.BeginHorizontal ();
            EditorGUILayout.LabelField (name);
            EditorGUILayout.EndHorizontal ();

            EditorGUILayout.BeginHorizontal ();
            config.offset = EditorGUILayout.Vector3Field ("Offset", config.offset);
            if (GUILayout.Button ("R", GUILayout.MaxWidth (20)))
            {
                config.offset = Vector3.zero;
            }
            EditorGUILayout.EndHorizontal ();

            EditorGUILayout.BeginHorizontal ();
            config.scale = EditorGUILayout.Vector3Field ("Scale", config.scale);
            if (GUILayout.Button ("R", GUILayout.MaxWidth (20)))
            {
                config.scale = Vector3.one;
            }
            EditorGUILayout.EndHorizontal ();

            EditorGUILayout.BeginHorizontal ();
            bool blur = (config.flag & SelfShadowConfig.Flag_Blur) != 0;
            blur = EditorGUILayout.Toggle ("Blur", blur);
            SetFlag (ref config, SelfShadowConfig.Flag_Blur, blur);
            EditorGUILayout.EndHorizontal ();

            EditorGUILayout.BeginHorizontal ();
            bool staticDraw = (config.flag & SelfShadowConfig.Flag_StaticDraw) != 0;
            staticDraw = EditorGUILayout.Toggle ("StaticDraw", staticDraw);
            SetFlag (ref config, SelfShadowConfig.Flag_StaticDraw, staticDraw);
            EditorGUILayout.EndHorizontal ();

            EditorGUILayout.BeginHorizontal ();
            bool customLight = (config.flag & SelfShadowConfig.Flag_CustomLightDir) != 0;
            customLight = EditorGUILayout.Toggle ("CustomLight", customLight);
            SetFlag (ref config, SelfShadowConfig.Flag_CustomLightDir, customLight);
            EditorGUILayout.EndHorizontal ();

            if ((config.flag & SelfShadowConfig.Flag_CustomLightDir) != 0)
            {
                if (customLightTrans == null)
                {
                    customLightTrans = FindShadowLight ("_SelfShadow_" + name);
                }
                if (customLightTrans != null)
                {
                    EditorGUILayout.BeginHorizontal ();
                    EditorGUILayout.ObjectField (customLightTrans, typeof (Transform), true);
                    EditorGUILayout.EndHorizontal ();

                    EditorGUILayout.BeginHorizontal ();
                    config.lightDir = -customLightTrans.forward;
                    EditorGUILayout.Vector3Field ("ShadowDir", config.lightDir);
                    EditorGUILayout.EndHorizontal ();
                }
            }
        }
        private void SelfShadowConfigGUI (SelfShadowEditorInfo ssei)
        {
            DrawSelfShadowConfig (ssei.name, ref ssei.config, ref ssei.shadowLight);
        }

        private void SelfShadowConfigListGUI (ref ListElementContext lec, ref SelfShadowListContext context, SelfShadowConfigs data, int i)
        {
            var ssei = data.configs[i];
            bool writeBack = false;
            SelfShadowConfig selfShadowConfig = ssei.config;
            if (EngineContext.IsRunning &&
                WorldSystem.miscConfig != null &&
                WorldSystem.miscConfig.selfShadowConfig != null &&
                i < WorldSystem.miscConfig.selfShadowConfig.Length)
            {
                selfShadowConfig = WorldSystem.miscConfig.selfShadowConfig[i];
                writeBack = true;
            }

            ToolsUtility.InitListContext (ref lec, context.defaultHeight);

            ToolsUtility.Label (ref lec, string.Format ("{0}.{1}", i.ToString (), ssei.name), 160, true);

            string folderPath = ssei.GetHash ();
            bool sseiFolder = ToolsUtility.SHButton (ref lec, config.folder, folderPath);
            if (sseiFolder)
            {
                ToolsUtility.NewLineWithOffset (ref lec);
                ToolsUtility.TextField (ref lec, "ConfigName", 160, ref ssei.name, 160, true);

                ToolsUtility.NewLineWithOffset (ref lec);
                ToolsUtility.VectorField (ref lec, "Offset", 160, ref selfShadowConfig.offset, 300, true);
                if (ToolsUtility.Button (ref lec, "R", 20))
                {
                    selfShadowConfig.offset = Vector3.zero;
                }
                ToolsUtility.NewLineWithOffset (ref lec);
                ToolsUtility.VectorField (ref lec, "Scale", 160, ref selfShadowConfig.scale, 300, true);
                if (ToolsUtility.Button (ref lec, "R", 20))
                {
                    selfShadowConfig.scale = Vector3.one;
                }

                ToolsUtility.NewLineWithOffset (ref lec);
                bool blur = (selfShadowConfig.flag & SelfShadowConfig.Flag_Blur) != 0;
                ToolsUtility.Toggle (ref lec, "Blur", 160, ref blur, true);
                SetFlag (ref selfShadowConfig, SelfShadowConfig.Flag_Blur, blur);

                ToolsUtility.NewLineWithOffset (ref lec);
                bool staticDraw = (selfShadowConfig.flag & SelfShadowConfig.Flag_StaticDraw) != 0;
                ToolsUtility.Toggle (ref lec, "StaticDraw", 160, ref staticDraw, true);
                SetFlag (ref selfShadowConfig, SelfShadowConfig.Flag_StaticDraw, staticDraw);

                ToolsUtility.NewLineWithOffset (ref lec);
                bool customLight = (selfShadowConfig.flag & SelfShadowConfig.Flag_CustomLightDir) != 0;
                ToolsUtility.Toggle (ref lec, "CustomLight", 160, ref customLight, true);
                SetFlag (ref selfShadowConfig, SelfShadowConfig.Flag_CustomLightDir, customLight);

                if (customLight)
                {
                    if (ssei.shadowLight == null)
                    {
                        ssei.shadowLight = FindShadowLight ("_SelfShadow_" + ssei.name);
                    }
                    if (ssei.shadowLight != null)
                    {
                        ToolsUtility.NewLineWithOffset (ref lec);
                        Vector4 dir = -ssei.shadowLight.forward;
                        ToolsUtility.VectorField (ref lec, "ShadowDir", 160, ref dir, 300, true);
                    }
                }
                if (writeBack)
                {
                    WorldSystem.miscConfig.selfShadowConfig[i] = selfShadowConfig;
                }
                ssei.config = selfShadowConfig;
            }
        }

        protected override void OnConfigGui (ref Rect rect)
        {
            if (config.folder.FolderGroup ("SelfShadowFolder", "SelfShadow", 10000))
            {
                ESMGUI ();
                if (config.folder.Folder ("SelfShadowDefault", "Default"))
                {
                    SelfShadowConfigGUI (config.defaultConfig);
                    if (miscConfig != null)
                    {
                        miscConfig.defaultShadowConfig = config.defaultConfig.config;
                    }
                }
                selfShadowEditor.Draw (config.folder, ref rect);

                ShadowModify.drawSelfShadow = EditorGUILayout.Toggle ("DrawBox", ShadowModify.drawSelfShadow);
                EditorCommon.EndFolderGroup ();
            }
        }

        protected override void OnSave ()
        {
            if (miscConfig != null)
            {
                miscConfig.defaultShadowConfig = config.defaultConfig.config;
                if (config.configs.Count == 0)
                {
                    miscConfig.selfShadowConfig = null;
                }
                else
                {
                    miscConfig.selfShadowConfig = new SelfShadowConfig[config.configs.Count];
                    for (int i = 0; i < config.configs.Count; ++i)
                    {
                        var ssei = config.configs.configs[i];
                        miscConfig.selfShadowConfig[i] = ssei.config;
                    }
                }
                CommonAssets.SaveAsset (miscConfig);
            }
            // try
            // {
            //     string path = string.Format ("{0}/Config/EffectData.bytes", AssetsConfig.instance.ResourcePath);
            //     using (FileStream fs = new FileStream (path, FileMode.Create))
            //     {
            //         List<RenderEffect> effects = new List<RenderEffect> ();
            //         int[] effectOffset = new int[EffectData.EffectNum * 2];
            //         for (int i = 0; i < config.effectGroup.Length; ++i)
            //         {
            //             var group = config.effectGroup[i];
            //             effectOffset[i * 2] = effects.Count;
            //             if (group != null)
            //             {
            //                 for (int j = 0; j < group.effectList.Count; ++j)
            //                 {
            //                     var ere = group.effectList[j];
            //                     effects.Add (ere.effect);

            //                 }
            //                 effectOffset[i * 2 + 1] = group.effectList.Count;
            //             }
            //         }

            //         BinaryWriter bw = new BinaryWriter (fs);
            //         bw.Write ((short) effects.Count);
            //         for (int i = 0; i < effects.Count; ++i)
            //         {
            //             var re = effects[i];
            //             EditorCommon.WriteVector (bw, re.data);
            //             EditorCommon.WriteVector (bw, re.data1);
            //         }
            //         for (int i = 0; i < effectOffset.Length; ++i)
            //         {
            //             bw.Write ((short) effectOffset[i]);
            //         }
            //     }
            // }
            // catch (Exception ex)
            // {
            //     DebugLog.AddErrorLog (ex.StackTrace);
            // }

        }
        protected override void OnConfigUpdate ()
        {
            //RenderEffectSystem.UpdateEffect (Time.deltaTime);
        }
    }
}