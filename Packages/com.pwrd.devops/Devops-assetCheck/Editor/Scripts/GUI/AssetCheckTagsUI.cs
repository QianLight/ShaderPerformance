using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace AssetCheck
{
    public class AssetCheckTagsUI : EditorWindow
    {
        static AssetCheckTags assetCheckTagsConfig;
        static bool bDirty = false;
        public static Action EventAssetCheckTagsChange;
        public static void ShowWindow()
        {
            AssetCheckTagsUI window = (AssetCheckTagsUI)EditorWindow.GetWindow(typeof(AssetCheckTagsUI));
            window.maxSize = new Vector2(500, 800);
            window.Show();
            window.Focus();

            assetCheckTagsConfig = AssetDatabase.LoadAssetAtPath<AssetCheckTags>($"{Defines.CheckPathConfigPath}/{Defines.CheckTagsConfigName}");
        }

        private void OnDestroy()
        {
            if(bDirty)
            {
#if UNITY_2020_3_OR_NEWER
                AssetDatabase.SaveAssetIfDirty(assetCheckTagsConfig);
#else
                AssetDatabase.SaveAssets();
#endif
                EventAssetCheckTagsChange?.Invoke();
            }
        }

        string editorTag = string.Empty;
        private void OnGUI()
        {
            GUILayout.BeginScrollView(Vector2.zero);
            GUILayout.BeginVertical();
            for (int i = 0; i < assetCheckTagsConfig.tags.Length; i++)
            {
                if (assetCheckTagsConfig.tags[i].Equals(string.Empty))
                    continue;
                GUILayout.BeginHorizontal();
                GUILayout.Label(assetCheckTagsConfig.tags[i]);
                if(GUILayout.Button(GUIDefines.ContentTrash))
                {
                    ArrayUtility.RemoveAt(ref assetCheckTagsConfig.tags, i);
                    EditorUtility.SetDirty(assetCheckTagsConfig);
                    bDirty = true;
                    break;
                }
                GUILayout.EndHorizontal();
            }
            editorTag = GUILayout.TextField(editorTag);
            if(GUILayout.Button("Ìí¼Ótag"))
            {
                if (ArrayUtility.Contains(assetCheckTagsConfig.tags, editorTag))
                    return;
                ArrayUtility.Add(ref assetCheckTagsConfig.tags, editorTag);
                editorTag = "";
                EditorUtility.SetDirty(assetCheckTagsConfig);
                bDirty = true;
            }
            GUILayout.EndVertical();
            GUILayout.EndScrollView();
        }
    }
}