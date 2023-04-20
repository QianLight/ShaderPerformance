using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace CFEngine.Editor
{
    class ShaderKeyValue
    {
        public string shaderKey;
        public Vector4 value;
    }
    public class MiscConfigTool : CommonToolTemplate
    {
        enum OpType
        {
            OpNone,
            OpSaveConfig,
            OpRefreshMat,
        }
        private MiscConfig miscConfig;
        private OpType opType = OpType.OpNone;
        private Vector2 prefabsScroll = Vector2.zero;

        private string refreshDir;
        private List<Shader> refreshShader = new List<Shader> ();
        private List<ShaderKeyValue> matKey = new List<ShaderKeyValue> ();
        public override void OnInit ()
        {
            base.OnInit ();
            string path = string.Format ("{0}Config/MiscConfig.asset", LoadMgr.singleton.EngineResPath);
            miscConfig = AssetDatabase.LoadAssetAtPath<MiscConfig> (path);
            if (miscConfig == null)
            {
                miscConfig = ScriptableObject.CreateInstance<MiscConfig> ();
                miscConfig = CommonAssets.CreateAsset<MiscConfig> (path, ".asset", miscConfig);
                WorldSystem.miscConfig = miscConfig;
            }
        }

        public override void OnUninit ()
        {
            base.OnUninit ();
        }

        public override void DrawGUI (ref Rect rect)
        {
            if (miscConfig != null)
            {
                MiscConfigGUI ("0.Misc");
                MiscOperationGUI ("1.Operation");
            }
        }

        private void MiscConfigGUI (string info)
        {
            EditorCommon.BeginGroup (info);
            EditorGUILayout.BeginHorizontal ();
            if (GUILayout.Button ("Save", GUILayout.MaxWidth (80)))
            {
                opType = OpType.OpSaveConfig;
            }
            EditorGUILayout.EndHorizontal ();
            EditorGUILayout.BeginHorizontal ();
            EditorGUILayout.LabelField ("Physic", EditorStyles.boldLabel);
            EditorGUILayout.EndHorizontal ();
            EditorGUILayout.BeginHorizontal ();
            miscConfig.skinWidth = EditorGUILayout.Slider ("SkinWidth", miscConfig.skinWidth, 0.001f, 0.1f);
            EditorGUILayout.EndHorizontal ();

            EditorGUILayout.BeginHorizontal ();
            EditorGUILayout.LabelField ("Role Mat", EditorStyles.boldLabel);
            EditorGUILayout.EndHorizontal ();
            EditorGUILayout.BeginHorizontal ();
            miscConfig.lightFaceRampOffset = EditorGUILayout.Slider ("LightFaceRampOffset", miscConfig.lightFaceRampOffset, -1, 1);
            EditorGUILayout.EndHorizontal ();
            EditorGUILayout.BeginHorizontal ();
            miscConfig.darkFaceRampOffset = EditorGUILayout.Slider ("DarkFaceRampOffset", miscConfig.darkFaceRampOffset, -1, 1);
            EditorGUILayout.EndHorizontal ();
            EditorGUILayout.Space ();
            EditorGUILayout.BeginHorizontal ();
            miscConfig.rimParam.x = EditorGUILayout.Slider ("RimCutLocation", miscConfig.rimParam.x, -1, 1);
            EditorGUILayout.EndHorizontal ();
            EditorGUILayout.BeginHorizontal ();
            miscConfig.rimParam.y = EditorGUILayout.Slider ("RimCutSmoothness", miscConfig.rimParam.y, -1, 1);
            EditorGUILayout.EndHorizontal ();
            EditorGUILayout.BeginHorizontal ();
            miscConfig.rimParam.z = EditorGUILayout.Slider ("DarkRimCutLocation", miscConfig.rimParam.z, -1, 1);
            EditorGUILayout.EndHorizontal ();
            EditorGUILayout.BeginHorizontal ();
            miscConfig.rimParam.w = EditorGUILayout.Slider ("DarkRimCutSmoothness", miscConfig.rimParam.w, -1, 1);
            EditorGUILayout.EndHorizontal ();
            EditorGUILayout.Space ();
            EditorGUILayout.BeginHorizontal ();
            miscConfig.lightFadePram.x = EditorGUILayout.Slider ("RimNormalMapWeight", miscConfig.lightFadePram.x, 0, 1);
            EditorGUILayout.EndHorizontal ();
            EditorGUILayout.BeginHorizontal ();
            miscConfig.lightFadePram.y = EditorGUILayout.Slider ("OrientLight", miscConfig.lightFadePram.y, 0, 1);
            EditorGUILayout.EndHorizontal ();
            EditorGUILayout.BeginHorizontal ();
            miscConfig.lightFadePram.z = EditorGUILayout.Slider ("RimFadeBegin", miscConfig.lightFadePram.z, 0, 50);
            EditorGUILayout.EndHorizontal ();
            EditorGUILayout.BeginHorizontal ();
            miscConfig.lightFadePram.w = EditorGUILayout.Slider ("RimFadeEnd", miscConfig.lightFadePram.w, 0, 50);
            EditorGUILayout.EndHorizontal ();

            EditorGUILayout.BeginHorizontal ();
            EditorGUILayout.LabelField ("RT Blur", EditorStyles.boldLabel);

            EditorGUILayout.EndHorizontal ();
            EditorGUILayout.BeginHorizontal ();
            miscConfig.miscParam.x = EditorGUILayout.IntSlider ("DownSampleNum", miscConfig.miscParam.x, 1, 4);
            EditorGUILayout.EndHorizontal ();
            EditorGUILayout.BeginHorizontal ();
            miscConfig.miscParam.y = EditorGUILayout.IntSlider ("BlurSpreadSize", miscConfig.miscParam.y, 1, 8);
            EditorGUILayout.EndHorizontal ();
            EditorGUILayout.BeginHorizontal ();
            miscConfig.miscParam.z = EditorGUILayout.IntSlider ("BlurIterations", miscConfig.miscParam.z, 1, 8);
            EditorGUILayout.EndHorizontal ();

            EditorGUILayout.BeginHorizontal ();
            EditorGUILayout.LabelField ("Default Fade", EditorStyles.boldLabel);

            EditorGUILayout.EndHorizontal ();
            EditorGUILayout.BeginHorizontal ();
            miscConfig.miscParam.w = EditorGUILayout.IntField ("FadeEffectID", miscConfig.miscParam.w);
            EditorGUILayout.EndHorizontal ();

            EditorGUILayout.BeginHorizontal ();
            miscConfig.sceneFadeAlpha = EditorGUILayout.Slider ("SceneFadeAlpha", miscConfig.sceneFadeAlpha, 0, 1);
            EditorGUILayout.EndHorizontal ();

            EditorGUILayout.BeginHorizontal ();
            miscConfig.miscParam2.x = EditorGUILayout.Slider ("SHWeight", miscConfig.miscParam2.x, 0, 1);
            EditorGUILayout.EndHorizontal ();
            EditorGUILayout.BeginHorizontal ();
            miscConfig.miscParam2.y = EditorGUILayout.Slider ("BigEntityHeight", miscConfig.miscParam2.y, 0, 5);
            EditorGUILayout.EndHorizontal ();
            EditorCommon.EndGroup ();

        }
        private void MiscOperationGUI (string info)
        {
            EditorCommon.BeginGroup (info);
            EditorGUILayout.BeginHorizontal ();
            if (GUILayout.Button ("RefreshMat", GUILayout.MaxWidth (80)))
            {
                opType = OpType.OpRefreshMat;
            }
            EditorGUILayout.EndHorizontal ();

            EditorGUILayout.BeginHorizontal ();
            EditorGUILayout.LabelField ("Dir", GUILayout.MaxWidth (40));
            refreshDir = EditorGUILayout.TextField ("", refreshDir, GUILayout.MaxWidth (300));
            EditorGUI.BeginChangeCheck ();
            DefaultAsset asset = null;
            asset = EditorGUILayout.ObjectField ("", asset, typeof (DefaultAsset), false, GUILayout.MaxWidth (50)) as DefaultAsset;
            if (EditorGUI.EndChangeCheck ())
            {
                refreshDir = AssetDatabase.GetAssetPath (asset);
            }
            if (GUILayout.Button ("AddShader", GUILayout.MaxWidth (80))) { refreshShader.Add (null); }
            if (GUILayout.Button ("AddValue", GUILayout.MaxWidth (80))) { matKey.Add (new ShaderKeyValue ()); }
            EditorGUILayout.EndHorizontal ();

            EditorGUILayout.BeginHorizontal ();
            EditorGUILayout.BeginVertical ();
            int removeIndex = -1;
            for (int i = 0; i < refreshShader.Count; ++i)
            {
                EditorGUILayout.BeginHorizontal ();
                refreshShader[i] = EditorGUILayout.ObjectField ("", refreshShader[i], typeof (Shader), false, GUILayout.MaxWidth (100)) as Shader;
                if (GUILayout.Button ("Delete", GUILayout.MaxWidth (80))) { removeIndex = i; }
                EditorGUILayout.EndHorizontal ();
            }
            if (removeIndex >= 0)
            {
                refreshShader.RemoveAt (removeIndex);
            }
            EditorGUILayout.EndVertical ();
            EditorGUILayout.BeginVertical ();
            removeIndex = -1;
            for (int i = 0; i < matKey.Count; ++i)
            {
                EditorGUILayout.BeginHorizontal ();
                var mk = matKey[i];
                mk.shaderKey = EditorGUILayout.TextField ("", mk.shaderKey, GUILayout.MaxWidth (200));
                mk.value = EditorGUILayout.Vector4Field ("", mk.value, GUILayout.MaxWidth (200));
                if (GUILayout.Button ("Delete", GUILayout.MaxWidth (80))) { removeIndex = i; }
                EditorGUILayout.EndHorizontal ();
            }
            if (removeIndex >= 0)
            {
                matKey.RemoveAt (removeIndex);
            }
            EditorGUILayout.EndVertical ();
            EditorGUILayout.EndHorizontal ();

        }
        public override void Update ()
        {

            switch (opType)
            {
                case OpType.OpSaveConfig:
                    SaveConfig ();
                    break;
                case OpType.OpRefreshMat:
                    RefreshMat ();
                    break;

            }
            opType = OpType.OpNone;
        }

        private void SaveConfig ()
        {
            if (miscConfig != null)
            {
                CommonAssets.SaveAsset (miscConfig);
            }
        }

        private void RefreshMat ()
        {
            CommonAssets.enumMat.cb = (mat, path, context) =>
            {
                var mc = context as MiscConfigTool;
                var shader = mat.shader;
                if (mc.refreshShader.Contains (shader))
                {
                    for (int i = 0; i < mc.matKey.Count; ++i)
                    {
                        var mk = mc.matKey[i];
                        mat.SetVector (mk.shaderKey, mk.value);
                    }
                    DebugLog.AddEngineLog2 ("refresh mat:{0}", path);
                }
            };
            CommonAssets.EnumAsset<Material> (CommonAssets.enumMat, "RefreshMat", refreshDir, this, false, true);
        }
    }
}