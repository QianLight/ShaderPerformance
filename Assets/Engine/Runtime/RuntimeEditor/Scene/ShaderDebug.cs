#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace CFEngine
{
    public class ShaderDebugModel
    {
        private enum DebugOpType
        {
            None,
            OpRenderLevel,
            OpShader,
            OpTess
        }

        private enum RenderLevelType
        {
            VeryLow,
            Low,
            Medium,
            High//,
            //VeryHigh
        }

        private static string[] DebugOption = new[]
        {
            "Default",
            "Albedo",
            "Metallic",
            "Roughness",
            "Specular",
            "NormalWS",
            "NormalGrayscale",
            "EnvironmentReflection",
            "Lightmap",
            "ShadowMask",
        };

        private enum TessSwitch
        {
            TessOff,
            TessOn
        }

        private static RenderLevelType renderLevelType = RenderLevelType.High;
        private static int opSelect;
        private static DebugOpType currentOpType = DebugOpType.None;
        private static readonly int shaderDebugModel = Shader.PropertyToID("_ShaderDebugModel");
        private static TessSwitch tessSwitch = TessSwitch.TessOff;

        public static void DuringSceneGUI(SceneView sceneView)
        {
            Handles.BeginGUI();

            GUILayout.BeginArea(new Rect(0, 0, 230, 500));
            GUILayout.BeginHorizontal();

            EditorGUI.BeginChangeCheck();
            renderLevelType = (RenderLevelType) EditorGUILayout.EnumPopup(renderLevelType,GUILayout.Width(70));
            if (EditorGUI.EndChangeCheck())
            {
                currentOpType = DebugOpType.OpRenderLevel;
            }

            EditorGUI.BeginChangeCheck();
            tessSwitch = (TessSwitch)EditorGUILayout.EnumPopup(tessSwitch, GUILayout.Width(80));
            if (EditorGUI.EndChangeCheck())
            {
                currentOpType = DebugOpType.OpTess;
            }

            EditorGUI.BeginChangeCheck();
            opSelect = EditorGUILayout.Popup(opSelect, DebugOption);
            if (EditorGUI.EndChangeCheck())
            {
                currentOpType = DebugOpType.OpShader;
            }

            GUILayout.EndHorizontal();

            GUILayout.EndArea();

            Handles.EndGUI();

            UpdateOpHandler();
        }

        private static void UpdateOpHandler()
        {
            switch (currentOpType)
            {
                case DebugOpType.OpRenderLevel:
                    RenderLevelChange();
                    break;
                case DebugOpType.OpShader:
                    ShaderDebug();
                    break;
                case DebugOpType.OpTess:
                    SwitchTess();
                    break;
            }

            currentOpType = DebugOpType.None;
        }

        private static void RenderLevelChange()
        {
            Debug.Log("RenderLevelChange: " + renderLevelType.ToString());

            //Shader.DisableKeyword("_SHADER_LEVEL_VERY_HIGH");
            Shader.DisableKeyword("_SHADER_LEVEL_HIGH");
            Shader.DisableKeyword("_SHADER_LEVEL_MEDIUM");
            Shader.DisableKeyword("_SHADER_LEVEL_LOW");
            Shader.DisableKeyword("_SHADER_LEVEL_VERY_LOW");
            QualitySettings.shadowmaskMode = ShadowmaskMode.Shadowmask;
            switch (renderLevelType)
            {
                //case RenderLevelType.VeryHigh:
                //    QualitySettings.shadowmaskMode = ShadowmaskMode.DistanceShadowmask;
                //    Shader.EnableKeyword("_SHADER_LEVEL_VERY_HIGH");
                //    break;
                case RenderLevelType.High:
                    Shader.EnableKeyword("_SHADER_LEVEL_HIGH");
                    break;
                case RenderLevelType.Medium:
                    Shader.EnableKeyword("_SHADER_LEVEL_MEDIUM");
                    break;
                case RenderLevelType.Low:
                    Shader.EnableKeyword("_SHADER_LEVEL_LOW");
                    break;
                case RenderLevelType.VeryLow:
                    Shader.EnableKeyword("_SHADER_LEVEL_VERY_LOW");
                    break;
            }
        }

        private static void ShaderDebug()
        {
            if (opSelect == 0)
            {
                Shader.DisableKeyword("_SHADER_DEBUG");
            }
            else
            {
                Shader.EnableKeyword("_SHADER_DEBUG");
                Shader.SetGlobalFloat(shaderDebugModel, opSelect);
            }
        }

        private static void SwitchTess()
        {
            if (tessSwitch == TessSwitch.TessOff)
            {
                Shader.DisableKeyword("_TESSELLATION_ON");
            }
            else if (tessSwitch == TessSwitch.TessOn)
            {
                Shader.EnableKeyword("_TESSELLATION_ON");
            }
        }
    }
}
#endif