using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using PENet;
//using Newtonsoft.Json.Linq;
using System.Collections.Concurrent;
using LitJsonForSaveGame;

namespace Blueprint
{
    static class BlueprintConfig
    {
        private static BlueprintSettings bpSettings;
        public static readonly ConcurrentQueue<Action> RunOnMainThread = new ConcurrentQueue<Action>();

        [InitializeOnLoadMethod]
        static void InitializeOnLoadMethod()
        {
            ClientSession.OnReceiveMessage += ServerSession_OnReceiveMessage;
            EditorApplication.update += Update;
        }

        static void Update()
        {
            if (!RunOnMainThread.IsEmpty)
            {
                while (RunOnMainThread.TryDequeue(out var action))
                {
                    action?.Invoke();
                }
            }
        }

        private static void ServerSession_OnReceiveMessage(string obj)
        {
            RunOnMainThread.Enqueue(() =>
            {
                // Code here will be called in the main thread...
                string bpResourcePath = "BlueprintResourcePath";
                //JObject jobj = JObject.Parse(obj);

                //if (jobj[bpResourcePath] != null)
                JsonData data = JsonMapper.ToObject(obj);
                if(data.ContainsKey(bpResourcePath))
                {

                    var bpSettings = AssetDatabase.LoadMainAssetAtPath("Packages/com.pwrd.blueprint.util/Editor/BlueprintSettings.asset") as BlueprintSettings;
                    if (bpSettings == null)
                    {
                        bpSettings = ScriptableObject.CreateInstance<BlueprintSettings>();
                        AssetDatabase.CreateAsset(bpSettings, "Packages/com.pwrd.blueprint.util/Editor/BlueprintSettings.asset");
                        AssetDatabase.SaveAssets();
                    }
                    //string newPath = jobj[bpResourcePath].Value<string>();
                    string newPath = data[bpResourcePath].ToString();
                    string oldPath = bpSettings.BlueprintResourcePath;
                    bpSettings.BlueprintResourcePath = GetRelativePath(newPath);
                    if (oldPath != bpSettings.BlueprintResourcePath)
                    {
                        EditorUtility.SetDirty(bpSettings);
                        AssetDatabase.SaveAssets();
                    }
                }
            });
        }

        [SettingsProvider]
        public static SettingsProvider BlueprintSettingsProvider()
        {
            var provider = new SettingsProvider("Project/Blueprint", SettingsScope.Project)
            {
                label = "蓝图设置",
                activateHandler = (searchContext, rootElement) =>
                {
                    //bpSettings = Resources.Load<BlueprintSettings>("BlueprintSettings");
                    //if (bpSettings == null)
                    //{
                    //    bpSettings = ScriptableObject.CreateInstance<BlueprintSettings>();
                    //    AssetDatabase.CreateAsset(bpSettings, "Assets/Resources/BlueprintSettings.asset");
                    //    AssetDatabase.SaveAssets();
                    //}
                    bpSettings = AssetDatabase.LoadMainAssetAtPath("Packages/com.pwrd.blueprint.util/Editor/BlueprintSettings.asset") as BlueprintSettings;
                    if (bpSettings == null)
                    {
                        bpSettings = ScriptableObject.CreateInstance<BlueprintSettings>();
                        AssetDatabase.CreateAsset(bpSettings, "Packages/com.pwrd.blueprint.util/Editor/BlueprintSettings.asset");
                        AssetDatabase.SaveAssets();
                    }
                },
                guiHandler = (searchContext) =>
                {
                    if (bpSettings != null)
                    {
                        string oldPath = bpSettings.BlueprintResourcePath;
                        EditorGUILayout.BeginVertical();
                        EditorGUILayout.BeginHorizontal();
                        float originalLabelWidth = EditorGUIUtility.labelWidth;
                        float origianFiledWidth = EditorGUIUtility.fieldWidth;
                        EditorGUIUtility.labelWidth = 100;
                        bpSettings.BlueprintResourcePath = EditorGUILayout.TextField(new GUIContent("蓝图资产路径"), bpSettings.BlueprintResourcePath);
                        if (GUILayout.Button("浏览", GUILayout.MaxWidth(50)))
                        {
                            bool hasDefault = System.IO.Directory.Exists(bpSettings.GetBlueprintResourceFullPath());
                            string path = EditorUtility.OpenFolderPanel("选择蓝图资产目录", hasDefault ? System.IO.Path.GetDirectoryName(bpSettings.GetBlueprintResourceFullPath()) : string.Empty,
                                hasDefault ? System.IO.Path.GetFileName(bpSettings.GetBlueprintResourceFullPath()) : string.Empty);
                            if (path.Length != 0)
                            {
                                //计算path的相对路径
                                path = GetRelativePath(path);
                                bpSettings.BlueprintResourcePath = path;
                            }
                        }
                        EditorGUIUtility.labelWidth = originalLabelWidth;
                        EditorGUIUtility.fieldWidth = origianFiledWidth;
                        EditorGUILayout.EndHorizontal();
                        GUILayout.Label(bpSettings.GetBlueprintResourceFullPath(), EditorStyles.helpBox);
                        EditorGUILayout.EndVertical();
                        if (oldPath != bpSettings.BlueprintResourcePath)
                            EditorUtility.SetDirty(bpSettings);
                        AssetDatabase.SaveAssets();
                    }
                }
            };
            return provider;
        }

        private static string GetRelativePath(string path)
        {
            if (System.IO.Directory.GetCurrentDirectory()[0] == path[0])
            {
                Uri uirFile = new Uri(path);
                Uri uirCurrentDirectory = new Uri(System.IO.Directory.GetCurrentDirectory());
                path = "..\\" + uirCurrentDirectory.MakeRelativeUri(uirFile).ToString().Replace("/", "\\").Replace("%20", " ");
            }
            return path;
        }
    }
}
