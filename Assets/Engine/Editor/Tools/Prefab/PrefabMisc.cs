using System.IO;
using UnityEditor;
using UnityEngine;

namespace CFEngine.Editor
{
    public class PrefabMisc : CommonToolTemplate
    {
        enum OpType
        {
            OpNone,
            OpReplace,
        }
        private string prefabDir = "";
        private int mask = 0;
        private OpType opType = OpType.OpNone;
        public override void OnInit()
        {
            base.OnInit();
        }

        public override void OnUninit()
        {
            base.OnUninit();
        }

        public override void DrawGUI(ref Rect rect)
        {
            EditorGUILayout.BeginHorizontal();
            prefabDir = EditorGUILayout.TextField("", prefabDir, GUILayout.MaxWidth(300));
            EditorGUI.BeginChangeCheck();
            DefaultAsset asset = null;
            asset = EditorGUILayout.ObjectField("", asset, typeof(DefaultAsset), false, GUILayout.MaxWidth(50)) as DefaultAsset;
            if (EditorGUI.EndChangeCheck())
            {
                prefabDir = AssetDatabase.GetAssetPath(asset);
            }
            EditorGUILayout.LabelField("Layers:", GUILayout.MaxWidth(100));
            mask = EditorGUILayout.MaskField("", mask, DefaultGameObjectLayer.layerMaskName, GUILayout.MaxWidth(300));

            if (GUILayout.Button("Replace", GUILayout.MaxWidth(80))) { opType = OpType.OpReplace; }
            EditorGUILayout.EndHorizontal();
        }

        public override void Update()
        {

            switch (opType)
            {
                case OpType.OpReplace:
                    ReplaceLayer();
                    break;
            }
            opType = OpType.OpNone;
        }
        private static void ReplaceRenderLayer(GameObject prefab, string path, System.Object context)
        {
            var pm = context as PrefabMisc;
            var renders = EditorCommon.GetRenderers(prefab);
            for (int i = 0; i < renders.Count; ++i)
            {
                var render = renders[i];
                render.renderingLayerMask = (uint)pm.mask;
            }

            PrefabUtility.SavePrefabAsset(prefab);
        }

        private void ReplaceLayer()
        {
            if (!string.IsNullOrEmpty(prefabDir))
            {
                CommonAssets.enumPrefab.cb = ReplaceRenderLayer;
                CommonAssets.EnumAsset<GameObject>(CommonAssets.enumPrefab, "ReplaceLayer", prefabDir, this);
            }
        }

    }

    public partial class BuildMisc : PreBuildPreProcess
    {
        public override string Name { get { return "Misc"; } }
        public override int Priority
        {
            get
            {
                return 0;
            }
        }
        public override void PreProcess()
        {
            if(Application.isBatchMode) return;
            base.PreProcess();
            PrefabConfigTool.RefreshPrefabFolder(EditorPrefabData.instance, new PrefabConvertContext());
            PrefabConfigTool.SavePrefabData(EditorPrefabData.instance);
            string des = "config/prefabconfig.bytes";
            CopyFile(des, des);
        }

    }
}