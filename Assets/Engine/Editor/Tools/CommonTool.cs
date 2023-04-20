using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
namespace CFEngine.Editor
{
    public enum ECommonType
    {
        None,
        Welcome,
        AssetsConfig,
        ShaderConfig,
        MaterialConfig,
        SVCConfig,
        QualitySetting,
        ResRedirect,
        ResScan,
        //AntiResScan,
        MatScan,
        Num,
    }

    public class CommonTool : ToolTemplate
    {
        private ECommonType tool = ECommonType.None;
        public CommonTool (EditorWindow editorWindow) : base (editorWindow) { }
        public override void OnEnable ()
        {
            base.OnEnable ();
            if (toolIcons == null)
            {
                toolIcons = new GUIContent[]
                {
                    new GUIContent ("Welcome"),
                    new GUIContent ("AssetsConfig"),
                    new GUIContent ("ShaderConfig"),
                    new GUIContent ("MaterialConfig"),
                    new GUIContent ("SVCConfig"),
                    new GUIContent ("QualitySetting"),
                    new GUIContent ("ResRedirect"),
                    new GUIContent ("ResScan"),
                    //new GUIContent("AntiResScan")
                    new GUIContent ("MatScan"),
                    
                };
            }
            tools.Clear ();
            for (ECommonType i = ECommonType.None; i < ECommonType.Num; ++i)
            {
                CommonToolTemplate stt = null;
                switch (i)
                {
                    case ECommonType.Welcome:
                        stt = ScriptableObject.CreateInstance<WelcomeTool> ();
                        break;
                    case ECommonType.AssetsConfig:
                        stt = ScriptableObject.CreateInstance<AssetsConfigTool> ();
                        break;
                    case ECommonType.ShaderConfig:
                        stt = ScriptableObject.CreateInstance<ShaderConfigTool> ();
                        break;
                    case ECommonType.MaterialConfig:
                        stt = ScriptableObject.CreateInstance<MaterialConfigTool> ();
                        break;
                    case ECommonType.SVCConfig:
                        stt = ScriptableObject.CreateInstance<ShaderVariantCollector>();
                        break;
                    case ECommonType.QualitySetting:
                        stt = ScriptableObject.CreateInstance<QualitySettingTool> ();
                        break;
                    case ECommonType.ResRedirect:
                        stt = ScriptableObject.CreateInstance<ResRedirectTool> ();
                        break;
                    case ECommonType.ResScan:
                        stt = ScriptableObject.CreateInstance<ResScanTool> ();
                        break;
                    case ECommonType.MatScan:
                        stt = ScriptableObject.CreateInstance<MatScanTool>();
                        break;

                    //case ECommonType.AntiResScan:
                    //    stt = ScriptableObject.CreateInstance<AntiResScanTool>();
                    //    break;
                }
                tools.Add (stt);
            }

            SetTool ((int) tool);

        }
    }
}