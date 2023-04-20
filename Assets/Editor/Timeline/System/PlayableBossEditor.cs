using UnityEditor;
using UnityEngine;

namespace XEditor
{
    [CustomEditor(typeof(UIBossShowAsset))]
    public class PlayableBossEditor : Editor
    {
        Texture texture;
        UIBossShowAsset asset;

        private void OnEnable()
        {
            asset = target as UIBossShowAsset;
        }
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            EditorGUILayout.BeginVertical();
            string prefix = "Assets/BundleRes/";
            if (texture == null && !string.IsNullOrEmpty(asset.icon))
            {
                string pat = prefix + asset.icon + ".png";
                texture = AssetDatabase.LoadAssetAtPath<Texture>(pat);
            }
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("图标");
            EditorGUILayout.LabelField("sign");
            texture = EditorGUILayout.ObjectField(texture,
                typeof(Texture),
                true,
                GUILayout.Width(164),
                GUILayout.Height(160)) as Texture;

            string path = AssetDatabase.GetAssetPath(texture);
            path = path.Replace("Assets/BundleRes/", "");
            path = path.Replace(".png", "");
            asset.icon = path;
            EditorGUILayout.EndVertical();
        }

    }



}