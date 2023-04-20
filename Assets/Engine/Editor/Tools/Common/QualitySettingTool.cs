using System.IO;
using UnityEditor;
using UnityEngine;
namespace CFEngine.Editor
{
    public partial class QualitySettingTool : BaseConfigTool<QualitySettingConfig>
    {
        enum MSAACount
        {
            None = 1,
            MSAA2x = 2,
            MSAA4X = 4
        }
        private QualitySettingData qsd = null;
        private StatisticsThreathold st = null;
        private string[] qualityName = new string[] { "Ultra", "High", "Medium", "Low", "VeryLow" };

        private static readonly SavedBool radialBlurFolder = new SavedBool("QualitySettingTool.radialBlurFolder", false);
        private static readonly SavedBool dynamicBoneFolder = new SavedBool("QualitySettingTool.dynamicBoneFolder", false);
        private static readonly SavedVector2 scrollViewPos = new SavedVector2("QualitySettingTool.scrollViewPos");

        public override void OnInit ()
        {
            base.OnInit ();
            config = QualitySettingConfig.instance;
            string path = string.Format ("{0}Config/QualitySettings.asset", LoadMgr.singleton.EngineResPath);
            if (File.Exists (path))
                qsd = AssetDatabase.LoadAssetAtPath<QualitySettingData> (path);
            if (qsd == null)
            {
                qsd = ScriptableObject.CreateInstance<QualitySettingData> ();
                qsd = CommonAssets.CreateAsset<QualitySettingData> (path, ".asset", qsd);
                // WorldSystem.miscConfig = qsd;
            }

            path = string.Format("{0}Profile/ProfileThreathold.asset", LoadMgr.singleton.EngineResPath);
            if (File.Exists(path))
                st = AssetDatabase.LoadAssetAtPath<StatisticsThreathold>(path);
            if (st == null)
            {
                st = ScriptableObject.CreateInstance<StatisticsThreathold>();
                st = CommonAssets.CreateAsset<StatisticsThreathold>(path, ".asset", st);
            }
        }

        protected override void OnConfigGui(ref Rect rect)
        {
            if (config.folder.Folder("QualitySetting", "QualitySetting"))
            {
                if (qsd != null)
                {
                    for (int i = 0; i < qsd.settings.Length; ++i)
                    {
                        var qs = qsd.settings[i];
                        EditorGUI.indentLevel++;
                        QualitySettingGUI(qs, qualityName[i]);
                        EditorGUI.indentLevel--;
                    }
                }
            }

            if (config.folder.Folder("SceneProfile", "SceneProfile"))
            {
                StatisticsThreatholdGUI();
            }
        }

        private void QualitySettingFlagGUI (QualitySet qs, string name, uint flag)
        {
            EditorGUILayout.BeginHorizontal ();
            bool enable = qs.flag.HasFlag (flag);
            enable = EditorGUILayout.Toggle (name, enable, GUILayout.MaxWidth (300));
            qs.flag.SetFlag (flag, enable);
            EditorGUILayout.EndHorizontal ();
        }

        private void QualitySettingGUI (QualitySet qs, string name)
        {
            if (config.folder.Folder (name, name))
            {
                EditorGUI.indentLevel++;

                EditorGUILayout.BeginHorizontal ();
                qs.mainRTFormat = (RenderTextureFormat) EditorGUILayout.EnumPopup ("RTFormat", qs.mainRTFormat, GUILayout.MaxWidth (300));
                EditorGUILayout.EndHorizontal ();

                EditorGUILayout.BeginHorizontal ();
                //qs.rtScale = EditorGUILayout.Slider ("rtScale", qs.rtScale, 0.1f, 1, GUILayout.MaxWidth (300));
                //EditorGUILayout.EndHorizontal ();
                qs.renderQuality = (QualityLevel)EditorGUILayout.EnumPopup("renderQuality", qs.renderQuality, GUILayout.MaxWidth(300));
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.BeginHorizontal ();
                //MSAACount msaa = (MSAACount) qs.msaaCount;
                //msaa = (MSAACount) EditorGUILayout.EnumPopup ("MSAA", msaa, GUILayout.MaxWidth (300));
                //qs.msaaCount = (int) msaa;
                qs.antialiasinglevel = (QualityLevel)EditorGUILayout.EnumPopup("antialiasinglevel", qs.antialiasinglevel, GUILayout.MaxWidth(300));
                EditorGUILayout.EndHorizontal ();

                EditorGUILayout.BeginHorizontal();
                qs.lodLevel = (ShaderLod)EditorGUILayout.EnumPopup("ShaderLod", qs.lodLevel, GUILayout.MaxWidth(300));
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal ();
                qs.bloomQuality = (QualityLevel) EditorGUILayout.EnumPopup ("BloomQuality", qs.bloomQuality, GUILayout.MaxWidth (300));
                EditorGUILayout.EndHorizontal ();

                EditorGUILayout.BeginHorizontal();
                qs.dofQuality = (QualityLevel)EditorGUILayout.EnumPopup("DofQuality", qs.dofQuality, GUILayout.MaxWidth(300));
                EditorGUILayout.EndHorizontal();

                radialBlurFolder.Value = EditorGUILayout.Foldout(radialBlurFolder.Value, "RadialBlur");
                if (radialBlurFolder.Value)
                {
                    using (new EditorGUI.IndentLevelScope())
                    {
                        PPRadialBlurQuality radialBlur = qs.radialBlurQuality;
                        radialBlur.enable = EditorGUILayout.Toggle("Enable", radialBlur.enable, GUILayout.MaxWidth(300));
                        radialBlur.downSample = EditorGUILayout.IntSlider("Down Sample", radialBlur.downSample, 0, 2, GUILayout.MaxWidth(300));
                        radialBlur.cullDistance = EditorGUILayout.FloatField("Cull Distance", radialBlur.cullDistance, GUILayout.MaxWidth(300));
                        radialBlur.fadeOutCurve = EditorGUILayout.CurveField("Fade Out Curve", radialBlur.fadeOutCurve, GUILayout.MaxWidth(300));
                        radialBlur.times = (PPRadialBlurTimes)EditorGUILayout.EnumPopup("BlurTimes", radialBlur.times, GUILayout.MaxWidth(300));
                        if (radialBlur.times > PPRadialBlurTimes.One)
                        {
                            radialBlur.lodSize = EditorGUILayout.FloatField("LOD Size", radialBlur.lodSize, GUILayout.MaxWidth(300));
                        }
                    }
                }

                dynamicBoneFolder.Value = EditorGUILayout.Foldout(dynamicBoneFolder.Value, "DynamicBone");
                if (dynamicBoneFolder.Value)
                {
                    using (new EditorGUI.IndentLevelScope())
                    {
                        DynamicBoneQuality dynamicBone = qs.dynamicBoneQuality;
                        dynamicBone.m_UpdateRate = EditorGUILayout.Slider("刷新频率", dynamicBone.m_UpdateRate, 30, 60, GUILayout.MaxWidth(300));
                        dynamicBone.m_UpdateMode = (DynamicBoneQuality.UpdateMode)EditorGUILayout.EnumPopup("刷新方式", dynamicBone.m_UpdateMode, GUILayout.MaxWidth(300));
                        dynamicBone.m_DistantDisable = EditorGUILayout.Toggle("距离剔除", dynamicBone.m_DistantDisable, GUILayout.MaxWidth(300));
                        if (dynamicBone.m_DistantDisable)
                        {
                            dynamicBone.m_DistanceToObject = EditorGUILayout.FloatField("剔除距离", dynamicBone.m_DistanceToObject, GUILayout.MaxWidth(300));
                        }
                        dynamicBone.m_MovementThreshold = EditorGUILayout.FloatField("最大位移", dynamicBone.m_MovementThreshold, GUILayout.MaxWidth(300));
                    }
                }

                QualitySettingFlagGUI(qs, "EnablePostprocess", QualitySet.Flag_EnablePP);
                QualitySettingFlagGUI (qs, "EnableMRT", QualitySet.Flag_EnableMRT);
                QualitySettingFlagGUI (qs, "EnableShadow", QualitySet.Flag_EnableShadow);
                QualitySettingFlagGUI (qs, "EnableSoftShadow", QualitySet.Flag_EnableSoftShadow);
                QualitySettingFlagGUI (qs, "EnableDynamicLight", QualitySet.Flag_EnableDynamicLight);
                QualitySettingFlagGUI (qs, "EnableDOF", QualitySet.Flag_EnableDOF);
                QualitySettingFlagGUI (qs, "EnableDistortion", QualitySet.Flag_EnableDistortion);
                QualitySettingFlagGUI (qs, "EnableReflection", QualitySet.Flag_EnableReflection);
                QualitySettingFlagGUI (qs, "EnableUIPP", QualitySet.Flag_EnableUIPP);
                QualitySettingFlagGUI (qs, "EnableFXAA", QualitySet.Flag_EnableFXAA);
                EditorGUI.indentLevel--;
            }
        }

        private void StatisticsThreatholdGUI(SceneThreathold sceneThrea, bool isDefault,int index,ref int deleteIndex)
        {
            string sceneName = isDefault ? "default" : (string.IsNullOrEmpty(sceneThrea.sceneName) ? "empty" : sceneThrea.sceneName);

            bool folder = sceneName == "empty" ||
                config.folder.Folder(string.Format("scenethreath_{0}", sceneName), sceneName);
            if (!isDefault)
            {
                ToolsUtility.DeleteButton(ref deleteIndex, index, true);
            }
            if (folder)
            {
                EditorGUILayout.BeginHorizontal();
                if (isDefault)
                {
                    EditorGUILayout.LabelField("default");
                }
                else
                {
                    sceneThrea.sceneName = EditorGUILayout.TextField("scene name", sceneThrea.sceneName);
                }

                EditorGUILayout.EndHorizontal();
                EditorGUI.indentLevel++;
                EditorGUILayout.BeginHorizontal();
                sceneThrea.record.fps = EditorGUILayout.FloatField("fps", sceneThrea.record.fps);
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                sceneThrea.record.sceneDrawCalls = EditorGUILayout.IntField("sceneDrawCalls", sceneThrea.record.sceneDrawCalls);
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                sceneThrea.record.sceneTriangle = EditorGUILayout.IntField("sceneTriangle", sceneThrea.record.sceneTriangle);
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                sceneThrea.record.skinDrawCalls = EditorGUILayout.IntField("skinDrawCalls", sceneThrea.record.skinDrawCalls);
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                sceneThrea.record.skinTriangle = EditorGUILayout.IntField("skinTriangle", sceneThrea.record.skinTriangle);
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                sceneThrea.record.dynamicShadow = EditorGUILayout.IntField("dynamicShadow", sceneThrea.record.dynamicShadow);
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                sceneThrea.record.fxCount = EditorGUILayout.IntField("fxCount", sceneThrea.record.fxCount);
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                sceneThrea.record.psCount = EditorGUILayout.IntField("psCount", sceneThrea.record.psCount);
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                sceneThrea.record.meshCount = EditorGUILayout.IntField("meshCount", sceneThrea.record.meshCount);
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                int size = (int)sceneThrea.record.meshMemory / 1024;
                sceneThrea.record.meshMemory = EditorGUILayout.IntField("meshMemory(kb)", size) * 1024;
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                sceneThrea.record.texCount = EditorGUILayout.IntField("texCount", sceneThrea.record.texCount);
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                size = (int)sceneThrea.record.texMemory / 1024;
                sceneThrea.record.texMemory = EditorGUILayout.IntField("texMemory(kb)", size) * 1024;
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                sceneThrea.record.resCount = EditorGUILayout.IntField("resCount", sceneThrea.record.resCount);
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                size = (int)sceneThrea.record.resMemory / 1024;
                sceneThrea.record.resMemory = EditorGUILayout.IntField("resMemory(kb)", size) * 1024;
                EditorGUILayout.EndHorizontal();

                EditorGUI.indentLevel--;
            }

        }

        private void StatisticsThreatholdGUI()
        {
            int defaultDelete = 0;
            StatisticsThreatholdGUI(st.defaultThreadhold, true,0,ref defaultDelete);

            EditorGUILayout.BeginHorizontal();
            if(GUILayout.Button("Add",GUILayout.MaxWidth(100)))
            {
                st.sceneThreathold.Add(new SceneThreathold());
            }
            EditorGUILayout.EndHorizontal();
            int deleteIndex = ToolsUtility.BeginDelete();
            for (int i = 0; i < st.sceneThreathold.Count; ++i)
            {
                StatisticsThreatholdGUI(st.sceneThreathold[i], false, i, ref deleteIndex);
            }
            ToolsUtility.EndDelete(deleteIndex, st.sceneThreathold);
        }
        protected override void OnSave ()
        {
            if (qsd != null)
            {
                EditorCommon.SaveAsset (qsd);
            }

            if (st != null)
            {
                EditorCommon.SaveAsset(st);
            }

        }
    }
}