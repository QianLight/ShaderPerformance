using System;
using System.Collections;
using UnityEditor;
using UnityEngine;

using SFGroupEdior = CFEngine.Editor.CommonListEditor<CFEngine.ShaderFeatureData>;
using SFGroupContext = CFEngine.Editor.AssetListContext<CFEngine.ShaderFeatureData>;

using ShaderGUIEditor = CFEngine.Editor.CommonListEditor<CFEngine.ShaderGUIData>;
using ShaderGUIContext = CFEngine.Editor.AssetListContext<CFEngine.ShaderGUIData>;

namespace CFEngine.Editor
{

    public struct SFEditContext
    {
        public int sfDeleteIndex;
        public ListEditContext sfListContext;
        public int sfBundleDeleteIndex;
        public ListEditContext sfBundleListContext;
        public ShaderFeatureGroup sfGroup;
        public ShaderFeatureGroupRef sfGroupRef;
        public ShaderGUIConfigRef configShaderRef;
    }

    public class SFDrawContext
    {
        public enum OpShaderConfigType
        {
            None,
            OpSaveShaderConfigGroup,
            OpCreateShaderConfigGroup,
            OpCreateShaderGUIConfig,
            OpSaveShaderGUIConfig
        }
        private SFGroupContext sfGroupContext;
        private SFGroupEdior sfGroupEdior;
        private ShaderGUIContext shaderGUIContext;
        private ShaderGUIEditor shaderGUIEdior;

        private ShaderFeatureData sfDataRef;
        // private ToolsUtility.MouseClickContext clickContext;

        private ShaderConfig config;
        private SFEditContext sfContext;
        private OpShaderConfigType opType = OpShaderConfigType.None;

        public void InitShader (ShaderConfig sc)
        {
            config = sc;
            sfGroupContext.headGUI = ShaderHeadGUI;
            sfGroupContext.elementGUI = ShaderConfigGUI;
            sfGroupContext.needDelete = true;
            sfGroupContext.needAdd = true;

            sc.shaderFeature.name = "ConfigGroup";
            sfGroupEdior = new SFGroupEdior (sc.shaderFeature, ref sfGroupContext);

            shaderGUIContext.headGUI = ShaderSortGUI;
            shaderGUIContext.elementGUI = ShaderGUIGUI;
            shaderGUIContext.needDelete = true;
            shaderGUIContext.needAdd = true;

            sc.shaderGUI.name = "ShaderGUI";
            shaderGUIEdior = new ShaderGUIEditor (sc.shaderGUI, ref shaderGUIContext);
            sfDataRef = sc.shaderFeature;
            // clickContext.Reset ();

            sfContext.sfListContext.Reset ();
            sfContext.sfListContext.onCopy = OnCopySF;
            sfContext.sfListContext.canCopy = true;
            sfContext.sfListContext.canSwitch = true;
            sfContext.sfListContext.canInsert = true;

            sfContext.sfBundleListContext.Reset ();
            sfContext.sfBundleListContext.canSwitch = true;
            sfContext.sfBundleListContext.canInsert = true;
            RefreshSF (sfDataRef);
        }

        public void ShaderFeatureGUI (ShaderConfig sc, ref Rect rect)
        {
            if (sc.folder.FolderGroup ("ShaderFeature", "ShaderFeature", rect.width))
            {
                EditorGUILayout.LabelField ("Suffix:S=Select;R=Reset;P=Paste;SW=Switch;B=Before;A=After");
                // clickContext.BeginProcessEvent ();
                sfGroupEdior.Draw (config.folder, ref rect);
                shaderGUIEdior.Draw (config.folder, ref rect);
                // clickContext.EndProcessEvent ();
                EditorCommon.EndFolderGroup ();
            }
        }
        public void OnConfigUpdate ()
        {
            switch (opType)
            {
                case OpShaderConfigType.OpSaveShaderConfigGroup:
                    {
                        if (sfContext.sfGroup != null)
                        {
                            EditorCommon.SaveAsset (sfContext.sfGroup);
                            sfContext.sfGroup = null;
                        }
                    }
                    break;
                case OpShaderConfigType.OpCreateShaderConfigGroup:
                    {
                        if (sfContext.sfGroupRef != null)
                        {
                            string path = string.Format (
                                "Assets/Engine/Editor/EditorResources/ShaderConfig/ShaderFeatureGroup_{0}.asset",
                                sfContext.sfGroupRef.name);
                            var sfg = ScriptableObject.CreateInstance<ShaderFeatureGroup> ();;
                            EditorCommon.SaveAsset (path, sfg);
                            sfContext.sfGroupRef.sfg = sfg;
                            sfContext.sfGroupRef = null;
                        }
                    }
                    break;
                case OpShaderConfigType.OpCreateShaderGUIConfig:
                    {
                        if (sfContext.configShaderRef != null)
                        {
                            string shaderPath = AssetDatabase.GetAssetPath (sfContext.configShaderRef.shader);
                            shaderPath = shaderPath.Replace (".shader", "");
                            int index = shaderPath.LastIndexOf ("/");
                            if (index >= 0)
                            {
                                shaderPath = shaderPath.Substring (index + 1);
                            }
                            string path = string.Format (
                                "Assets/Engine/Editor/EditorResources/ShaderConfig/ShaderGUIConfig_{0}.asset",
                                shaderPath);
                            var configShader = ScriptableObject.CreateInstance<ShaderGUIConfig> ();
                            configShader.shader = sfContext.configShaderRef.shader;
                            EditorCommon.SaveAsset (path, configShader);
                            sfContext.configShaderRef.config = configShader;
                            sfContext.configShaderRef = null;
                        }
                    }
                    break;
                case OpShaderConfigType.OpSaveShaderGUIConfig:
                    {
                        if (sfContext.configShaderRef != null)
                        {
                            EditorCommon.SaveAsset (sfContext.configShaderRef.config);
                            sfContext.configShaderRef = null;
                        }
                    }
                    break;

            }
            opType = OpShaderConfigType.None;
        }
        #region ShaderFeature 

        private void RefreshSF (ShaderFeatureData data)
        {
            for (int i = 0; i < data.groupRefs.Count; ++i)
            {
                var groupRef = data.groupRefs[i];
                if (groupRef.sfg != null)
                {
                    for (int j = 0; j < groupRef.sfg.shaderFeatureBlocks.Count; ++j)
                    {
                        var block = groupRef.sfg.shaderFeatureBlocks[j];
                        for (int k = 0; k < block.shaderFeatures.Count; ++k)
                        {
                            var sf = block.shaderFeatures[k];
                            sf.key = string.Format ("{0}_{1}", groupRef.name, sf.name);
                        }
                    }
                }
            }
        }
        private void ShaderHeadGUI (ref Rect rect, ref SFGroupContext context, ShaderFeatureData data)
        {
            rect.width = 80;
            if (GUI.Button (rect, "ExportXML"))
            {
                data.Export ("Assets/Engine/Editor/EditorResources/ShaderConfig/ShaderConfig.xml");
            }
            rect.x += 85;
            if (GUI.Button (rect, "LoadXML"))
            {
                data.groupRefs.Clear ();
                data.Import ("Assets/Engine/Editor/EditorResources/ShaderConfig/ShaderConfig.xml");
            }
            rect.x += 85;
            if (GUI.Button (rect, "ResetSF"))
            {
                RefreshSF (data);
            }
        }

        private void OnCopySF (ref ListEditContext context)
        {
            ShaderFeature srcSF = context.srcList[context.selectIndex] as ShaderFeature;
            ShaderFeature targetSF = context.targetList[context.targetIndex] as ShaderFeature;
            targetSF.Copy (srcSF);
        }

        private void ShaderFeatureGUI (ref ListElementContext lec, ShaderFeature sf, IList sfLst, int index)
        {
            ToolsUtility.NewLine (ref lec, 35);
            ToolsUtility.TextField (ref lec, "Feature", 80, ref sf.name, 160, true);
            ToolsUtility.EnumPopup (ref lec, "", 80, ref sf.type, 100);
            string sfFolderPath = sf.GetHash ();
            bool show = ToolsUtility.SHButton (ref lec, config.folder, sfFolderPath);
            ToolsUtility.DeleteButton (ref sfContext.sfDeleteIndex, index, ref lec);
            sfContext.sfListContext.OnGUI (ref lec, sfLst, index);

            if (show)
            {
                ToolsUtility.NewLineWithOffset (ref lec);
                ToolsUtility.LineSpace (ref lec, 10);
                float h = lec.height;
                //
                if (lec.draw)
                {
                    Rect r = lec.rect;
                    r.x -= 10;
                    r.y -= 5;
                    ToolsUtility.DrawRect (r, 500, sf.height + 5);
                }
                bool hide = sf.HasFlag (ShaderFeature.Flag_Hide);
                ToolsUtility.Toggle (ref lec, "Hide", 80, ref hide, true);
                sf.SetFlag (ShaderFeature.Flag_Hide, hide);

                bool readOnly = sf.HasFlag (ShaderFeature.Flag_ReadOnly);
                ToolsUtility.Toggle (ref lec, "IsReadOnly", 80, ref readOnly);
                sf.SetFlag (ShaderFeature.Flag_ReadOnly, readOnly);

                ToolsUtility.NewLineWithOffset (ref lec);
                ToolsUtility.TextField (ref lec, "Property", 80, ref sf.propertyName, 150, true);
                ToolsUtility.EnumPopup (ref lec, "Type", 80, ref sf.type, 100);

                if (sf.type == ShaderPropertyType.Custom)
                {
                    for (int k = 0; k < sf.customProperty.Length; ++k)
                    {
                        var scp = sf.customProperty[k];

                        ToolsUtility.NewLine (ref lec, 35);

                        string desc = string.IsNullOrEmpty (scp.desc) || !scp.valid ? "value" + k.ToString () : scp.desc;
                        string descfolderPath = string.Format ("ShaderPropertyDesc_{0}_{1}", sfFolderPath, k);
                        bool scpfolder = ToolsUtility.Foldout (ref lec, config.folder, descfolderPath, desc, 50, true);
                        if (scpfolder)
                        {
                            ToolsUtility.NewLine (ref lec, 40);
                            float textWidth = 70;
                            ToolsUtility.Toggle (ref lec, "Valid", textWidth, ref scp.valid, true);
                            ToolsUtility.NewLineWithOffset (ref lec);
                            ToolsUtility.TextField (ref lec, "Desc", textWidth, ref scp.desc, 200, true);
                            ToolsUtility.NewLineWithOffset (ref lec);
                            ToolsUtility.IntSlider (ref lec, "Index", textWidth, ref scp.index, -1, 3, 200, true);
                            ToolsUtility.NewLineWithOffset (ref lec);
                            ToolsUtility.Toggle (ref lec, "IntValue", textWidth, ref scp.intValue, true);
                            ToolsUtility.NewLineWithOffset (ref lec);
                            ToolsUtility.Slider (ref lec, "Default", textWidth, ref scp.defaultValue, scp.min, scp.max, 200, true);
                            ToolsUtility.NewLineWithOffset (ref lec);
                            ToolsUtility.FloatField (ref lec, "Min", textWidth, ref scp.min, 200, true);
                            ToolsUtility.NewLineWithOffset (ref lec);
                            ToolsUtility.FloatField (ref lec, "Max", textWidth, ref scp.max, 200, true);
                            if (scp.max < scp.min)
                            {
                                scp.max = scp.min;
                            }

                            ToolsUtility.NewLineWithOffset (ref lec);
                            ToolsUtility.EnumPopup (ref lec, "ToggleType", textWidth, ref scp.toggleType, 200, true);
                            if (scp.toggleType == EShaderCustomValueToggle.Keyword)
                            {
                                ToolsUtility.NewLineWithOffset (ref lec);
                                ToolsUtility.TextField (ref lec, "ValueName", textWidth, ref scp.valueName, 200, true);
                            }
                            else if (scp.toggleType == EShaderCustomValueToggle.ValueToggle ||
                                scp.toggleType == EShaderCustomValueToggle.Toggle)
                            {
                                ToolsUtility.NewLineWithOffset (ref lec);
                                ToolsUtility.FloatField (ref lec, "Disable", textWidth, ref scp.disableValue, 200, true);
                                ToolsUtility.NewLineWithOffset (ref lec);
                                ToolsUtility.FloatField (ref lec, "Enable", textWidth, ref scp.enableValue, 200, true);

                                ToolsUtility.NewLineWithOffset (ref lec);
                                ToolsUtility.EnumPopup (ref lec, "CmpType", textWidth, ref scp.valueCmpType, 200, true);
                                ToolsUtility.NewLineWithOffset (ref lec);
                                ToolsUtility.FloatField (ref lec, "Threshold", textWidth, ref scp.thresholdValue, 200, true);
                            }
                        }
                        ToolsUtility.LineSpace (ref lec);
                    }
                }
                else if (sf.type == ShaderPropertyType.ValueToggle)
                {
                    var scp = sf.customProperty[0];
                    float textWidth = 70;
                    ToolsUtility.NewLine (ref lec, 35);
                    ToolsUtility.IntSlider (ref lec, "Index", textWidth, ref scp.index, -1, 3, 200, true);
                    ToolsUtility.NewLine (ref lec, 35);
                    ToolsUtility.FloatField (ref lec, "Disable", textWidth, ref scp.disableValue, 200, true);
                    ToolsUtility.NewLine (ref lec, 35);
                    ToolsUtility.FloatField (ref lec, "Enable", textWidth, ref scp.enableValue, 200, true);
                    ToolsUtility.NewLine (ref lec, 35);
                    Enum valueCmpType = scp.valueCmpType;
                    ToolsUtility.EnumPopup (ref lec, "CmpType", textWidth, ref valueCmpType, 200, true);
                    scp.valueCmpType = (EValueCmpType) valueCmpType;
                    ToolsUtility.NewLine (ref lec, 35);
                    ToolsUtility.FloatField (ref lec, "Threshold", textWidth, ref scp.thresholdValue, 200, true);

                }
                else if (sf.type == ShaderPropertyType.Color)
                {
                    ToolsUtility.NewLine (ref lec, 35);
                    bool showAlpha = sf.HasFlag (ShaderFeature.Flag_ShowAlpha);
                    ToolsUtility.Toggle (ref lec, "ShowAlpha", 80, ref showAlpha, true);
                    sf.SetFlag (ShaderFeature.Flag_ShowAlpha, showAlpha);
                    ToolsUtility.ColorField (ref lec, "DefaultColor", 80, ref sf.defaultColor, 160);
                }
                else if (sf.type == ShaderPropertyType.Tex)
                {
                    bool IsRamp = sf.HasFlag (ShaderFeature.Flag_IsRamp);
                    ToolsUtility.Toggle (ref lec, "IsRamp", 80, ref IsRamp);
                    sf.SetFlag (ShaderFeature.Flag_IsRamp, IsRamp);
                }
                ToolsUtility.NewLine (ref lec, 30);
                ToolsUtility.Toggle (ref lec, "isNor", 50, ref sf.dependencyPropertys.isNor, true);

                ToolsUtility.EnumPopup (ref lec, "DepType", 60, ref sf.dependencyPropertys.dependencyType, 50);
                if (ToolsUtility.Button (ref lec, "AddDep", 80))
                {
                    sf.dependencyPropertys.dependencyShaderProperty.Add ("");
                }

                int removeIndex = ToolsUtility.BeginDelete ();
                for (int k = 0; k < sf.dependencyPropertys.dependencyShaderProperty.Count; ++k)
                {
                    var dsp = sf.dependencyPropertys.dependencyShaderProperty[k];
                    ToolsUtility.NewLine (ref lec, 35);
                    ToolsUtility.TextField (ref lec, "Dep", 50, ref dsp, 100, true);
                    sf.dependencyPropertys.dependencyShaderProperty[k] = dsp;
                    ToolsUtility.DeleteButton (ref removeIndex, k, ref lec);
                }
                ToolsUtility.EndDelete (removeIndex, sf.dependencyPropertys.dependencyShaderProperty);
                h = lec.height + lec.lineHeight - h;
                if (sf.height != (int) h)
                {
                    sf.height = (int) h;
                }
                ToolsUtility.LineSpace (ref lec, 10);
            }

        }

        private void ShaderFeatureBlockGUI (ref ListElementContext lec, ShaderFeatureBlock sfBlock, IList blockLst, int index)
        {
            ToolsUtility.NewLine (ref lec, 25);
            string sfBlockPath = sfBlock.GetHash ();
            bool sfBlockFolder = ToolsUtility.Foldout (ref lec, config.folder, sfBlockPath,
                string.IsNullOrEmpty (sfBlock.bundleName) ? "empty" : sfBlock.bundleName, 100, true);

            ToolsUtility.DeleteButton (ref sfContext.sfBundleDeleteIndex, index, ref lec);
            sfContext.sfBundleListContext.OnGUI (ref lec, blockLst, index);
            if (sfBlockFolder)
            {
                ToolsUtility.NewLine (ref lec, 30);
                ToolsUtility.TextField (ref lec, "Bundle", 80, ref sfBlock.bundleName, 100, true);
                if (ToolsUtility.Button (ref lec, "Add", 80))
                {
                    sfBlock.shaderFeatures.Add (new ShaderFeature ());
                }
                sfContext.sfDeleteIndex = ToolsUtility.BeginDelete ();
                for (int i = 0; i < sfBlock.shaderFeatures.Count; ++i)
                {
                    var sf = sfBlock.shaderFeatures[i];
                    ShaderFeatureGUI (ref lec, sf, sfBlock.shaderFeatures, i);
                }
                ToolsUtility.EndDelete (sfContext.sfDeleteIndex, sfBlock.shaderFeatures);
            }

        }

        private void ShaderConfigGUI (ref ListElementContext lec, ref SFGroupContext context, ShaderFeatureData data, int i)
        {
            var groupRef = data.groupRefs[i];
            ToolsUtility.InitListContext (ref lec, context.defaultHeight);

            ToolsUtility.Label (ref lec, string.IsNullOrEmpty (groupRef.name) ? "empty" : groupRef.name, 100, true);

            string sgfolderPath = groupRef.GetHash ();
            if (ToolsUtility.SHButton (ref lec, config.folder, sgfolderPath))
            {
                //new line
                ToolsUtility.NewLine (ref lec, 20);
                ToolsUtility.TextField (ref lec, "", 0, ref groupRef.name, 150, true);
                ToolsUtility.ObjectField (ref lec, "", 0, ref groupRef.sfg, typeof (ShaderFeatureGroup), 250);
                var sfg = groupRef.sfg;
                if (sfg != null)
                {
                    if (ToolsUtility.Button (ref lec, "AddBlock", 100))
                    {
                        sfg.shaderFeatureBlocks.Add (new ShaderFeatureBlock ());
                    }
                    if (ToolsUtility.Button (ref lec, "Save", 100))
                    {
                        sfContext.sfGroup = sfg;
                        opType = OpShaderConfigType.OpSaveShaderConfigGroup;
                    }
                    sfContext.sfBundleDeleteIndex = ToolsUtility.BeginDelete ();
                    for (int j = 0; j < sfg.shaderFeatureBlocks.Count; ++j)
                    {
                        var sfBlock = sfg.shaderFeatureBlocks[j];
                        ShaderFeatureBlockGUI (ref lec, sfBlock, sfg.shaderFeatureBlocks, j);
                        if (j < sfg.shaderFeatureBlocks.Count - 1)
                        {
                            ToolsUtility.NewLine (ref lec, 10, 8);
                            ToolsUtility.DrawLine (ref lec, 600, 0, lec.lineHeight - 10);
                        }
                    }
                    ToolsUtility.EndDelete (sfContext.sfBundleDeleteIndex, sfg.shaderFeatureBlocks);
                }
                else if (!string.IsNullOrEmpty (groupRef.name))
                {
                    if (ToolsUtility.Button (ref lec, "Create", 100))
                    {
                        sfContext.sfGroupRef = groupRef;
                        opType = OpShaderConfigType.OpCreateShaderConfigGroup;
                    }
                }
            }
        }

        #endregion

        #region ShaderGUI
        private void RefreshShaderConfigGUI (ShaderGUIConfig guiConfig)
        {
            var sfData = config.shaderFeature;
            for (int j = 0; j < sfData.groupRefs.Count; ++j)
            {
                var groupRef = sfData.groupRefs[j];
                var sfg = groupRef.sfg;
                sfg.index = j;
            }
            guiConfig.Sort ();
        }
        private void ShaderSortGUI (ref Rect rect, ref ShaderGUIContext context, ShaderGUIData data)
        {
            rect.width = 80;
            if (GUI.Button (rect, "Sort"))
            {
                data.configRefs.Sort ((x, y) => x.config.name.CompareTo (y.config.name));
            }
        }
        private void ShaderGUIGUI (ref ListElementContext lec, ref ShaderGUIContext context, ShaderGUIData data, int i)
        {
            var configRef = data.configRefs[i];
            ToolsUtility.InitListContext (ref lec, context.defaultHeight);
            var guiConfig = configRef.config;
            ToolsUtility.ObjectField (ref lec, "", 0, ref configRef.config, typeof (ShaderGUIConfig), 450);
            if (guiConfig != null)
            {
                string configRefFolderPath = configRef.GetHash ();
                bool show = ToolsUtility.SHButton (ref lec, config.folder, configRefFolderPath);
                if (ToolsUtility.Button (ref lec, "Save", 100))
                {
                    sfContext.configShaderRef = configRef;
                    opType = OpShaderConfigType.OpSaveShaderGUIConfig;
                }
                if (show)
                {
                    ToolsUtility.NewLine (ref lec, 20);
                    ToolsUtility.ObjectField (ref lec, "", 0, ref guiConfig.shader, typeof (Shader), 250, true);

                    if (ToolsUtility.Button (ref lec, "Add", 80))
                    {
                        configRef.add = !configRef.add;
                        if (configRef.add)
                        {
                            configRef.sfState.Clear ();
                            var sfData = config.shaderFeature;
                            for (int j = 0; j < sfData.groupRefs.Count; ++j)
                            {
                                var groupRef = sfData.groupRefs[j];
                                var sfg = groupRef.sfg;
                                bool enable = guiConfig.HasShaderFeatureConfig (sfg);
                                configRef.sfState.Add (new ShaderFeatureState ()
                                {
                                    groupRef = groupRef,
                                        enable = enable
                                });
                            }
                        }
                        else
                        {
                            RefreshShaderConfigGUI (guiConfig);
                        }
                    }
                    if (ToolsUtility.Button (ref lec, "Refresh", 80))
                    {
                        RefreshShaderConfigGUI (guiConfig);
                    }
                    if (ToolsUtility.Button (ref lec, "Clear", 80))
                    {
                        for (int j = 0; j < guiConfig.shaderFeatures.Count; ++j)
                        {
                            var sfi = guiConfig.shaderFeatures[j];
                            sfi.shaderFeatures.Clear ();
                        }
                    }
                    ToolsUtility.EnumPopup (ref lec, "", 0, ref guiConfig.dummyMatType, 120);
                    ToolsUtility.EnumPopup (ref lec, "", 0, ref guiConfig.lsType, 120);
                    int lineCount = ToolsUtility.FixLineCount (ref lec);
                    if (configRef.add)
                    {
                        int configCount = configRef.sfState.Count;
                        int perLineCount = lineCount;
                        for (int ii = 0; ii < configCount; ++ii)
                        {
                            bool reset = ToolsUtility.FixPerLine (ref lec, lineCount, ref perLineCount);
                            var sfState = configRef.sfState[ii];
                            if (ToolsUtility.Toggle (ref lec, sfState.groupRef.name, 140, ref sfState.enable, reset))
                            {
                                if (sfState.enable)
                                {
                                    guiConfig.AddConfig (sfState.groupRef.sfg);
                                }
                                else
                                {
                                    guiConfig.RemoveConfig (sfState.groupRef.sfg);
                                }
                            }
                        }
                    }
                    var guiInstances = guiConfig.shaderFeatures;
                    for (int j = 0; j < guiInstances.Count; ++j)
                    {
                        var guiInstance = guiInstances[j];
                        ToolsUtility.NewLine (ref lec, 20);
                        ToolsUtility.Label (ref lec, guiInstance.sfg.groupName, 100, true);
                        for (int ii = 0; ii < guiInstance.sfg.shaderFeatureBlocks.Count; ++ii)
                        {
                            var block = guiInstance.sfg.shaderFeatureBlocks[ii];
                            ToolsUtility.NewLine (ref lec, 25);
                            ToolsUtility.Label (ref lec, block.bundleName, 100, true);
                            int selectState = 0;
                            if (ToolsUtility.Button (ref lec, "Select", 80))
                            {
                                selectState = 1;
                            }
                            if (ToolsUtility.Button (ref lec, "UnSelect", 80))
                            {
                                selectState = 2;
                            }
                            int perLineCount = lineCount;
                            for (int jj = 0; jj < block.shaderFeatures.Count; ++jj)
                            {
                                var sf = block.shaderFeatures[jj];
                                if (string.IsNullOrEmpty (sf.key))
                                {
                                    sf.key = string.Format ("{0}_{1}",
                                        guiInstance.sfg.groupName, sf.name);
                                }

                                bool reset = ToolsUtility.FixPerLine (ref lec, lineCount, ref perLineCount);
                                bool contain = guiInstance.shaderFeatures.Contains (sf.key);
                                bool isContain = contain;
                                ToolsUtility.Toggle (ref lec, sf.name, 140, ref contain, reset);
                                if (isContain != contain)
                                {
                                    if (contain)
                                    {
                                        guiInstance.shaderFeatures.Add (sf.key);
                                    }
                                    else
                                    {
                                        guiInstance.shaderFeatures.Remove (sf.key);
                                    }
                                }
                                if (selectState == 1)
                                {
                                    if (!contain)
                                    {
                                        guiInstance.shaderFeatures.Add (sf.key);
                                    }
                                }
                                else if (selectState == 2)
                                {
                                    guiInstance.shaderFeatures.Remove (sf.key);
                                }
                            }
                        }
                    }
                }
            }
            else
            {
                ToolsUtility.ObjectField (ref lec, "", 0, ref configRef.shader, typeof (Shader), 250);
                if (ToolsUtility.Button (ref lec, "Create", 100))
                {
                    if (configRef.shader != null)
                    {
                        sfContext.configShaderRef = configRef;
                        opType = OpShaderConfigType.OpCreateShaderGUIConfig;
                    }
                }
            }
        }
        #endregion

    }
    public partial class ShaderConfigTool : BaseConfigTool<ShaderConfig>
    {

        private SFDrawContext sfDrawContext;

        public override void OnInit ()
        {
            base.OnInit ();
            config = ShaderConfig.instance;
            sfDrawContext = new SFDrawContext ();

            sfDrawContext.InitShader (config);
        }
        protected override void OnConfigGui (ref Rect rect)
        {
            if (sfDrawContext != null)
            {
                sfDrawContext.ShaderFeatureGUI (config, ref rect);
            }
        }

        protected override void OnConfigUpdate ()
        {
            if (sfDrawContext != null)
            {
                sfDrawContext.OnConfigUpdate ();
            }
        }
    }
}