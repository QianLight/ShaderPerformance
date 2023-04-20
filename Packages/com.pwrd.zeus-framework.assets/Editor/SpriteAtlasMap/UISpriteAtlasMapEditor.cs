/*******************************************************************
* Copyright © 2017—2020 Perfect World.Co.Ltd. All rights reserved.
* author: 移动项目支持部 ZEUS FRAMEWORK TEAM
********************************************************************/
using System.IO;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.U2D;
using Zeus.Framework.Asset;
using UnityEditor.U2D;
using ZeusFlatBuffers;

#if !RESOURCE_PROJECT
using Zeus.Core.FileSystem;
#endif

namespace Zeus.Framework.UI
{
    public class UISpriteAtlasMapEditor : EditorWindow
    {
        Dictionary<string, string> outLog = new Dictionary<string, string>();

        [MenuItem("Zeus/UI/UI Sprite Atlas Map", false, 21)]
        private static void Open()
        {
            GetWindow<UISpriteAtlasMapEditor>();
        }

        private void OnEnable()
        {
            titleContent.text = "UI Sprite Atlas Map";
        }

        private void OnGUI()
        {
            EditorGUILayout.LabelField("设置Atlas文件夹路径：");
            EditorGUILayout.BeginHorizontal();

            var atlasFolder = SpriteAtlasFolderSetting.AtlasFolderPath_Editor;
            EditorGUILayout.TextField(atlasFolder);

            if (GUILayout.Button("Browse", GUILayout.Width(80)))
            {
                string tempPath = EditorUtility.OpenFolderPanel("Choose Atlas Folder", Application.dataPath, "Atlas");
                tempPath = tempPath.Replace("\\", "/");
                atlasFolder = GetWorkSpacePath(tempPath);

                atlasFolder = atlasFolder.Replace("\\", "/");
                atlasFolder += "/";

                if (!atlasFolder.Contains("/Resources/"))
                    EditorUtility.DisplayDialog("error", "文件夹必须在Resources目录下!!!", "OK");
                else
                {
                    var lastIndex = atlasFolder.LastIndexOf("/Resources/");
                    var atlasFolder_underResources = atlasFolder.Remove(0,lastIndex + "/Resources/".Length);
                    if (atlasFolder_underResources == "")
                        atlasFolder_underResources = "/";
                    atlasFolder = atlasFolder.Remove(atlasFolder.Length-1, 1);
                    atlasFolder_underResources = atlasFolder_underResources.Remove(atlasFolder_underResources.Length-1, 1);
                    SpriteAtlasFolderSetting.Save(atlasFolder, atlasFolder_underResources);
                }
            }

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Update Sprite Atlas Map", GUILayout.Width(200)))
            {
#if UNITY_2018_1_OR_NEWER
                SpriteAtlasUtility.PackAllAtlases(BuildTarget.NoTarget, false);
#else
                throw new System.Exception("Can't pack all atlas when unity version is under 2018.1.");
#endif
                var atlasPaths = Directory.GetFiles(atlasFolder, "*.spriteatlas", SearchOption.AllDirectories);

                List<SpriteAtlasMap.MapItem> items = new List<SpriteAtlasMap.MapItem>();
                foreach (var item in atlasPaths)
                {
                    var atlas = AssetDatabase.LoadAssetAtPath<SpriteAtlas>(item);
                    var depends = AssetDatabase.GetDependencies(item);

                    var apath = item.Replace("\\", "/");
                    apath = apath.Replace(".spriteatlas", "");
                    var rindex = atlasFolder.LastIndexOf("Resources");
                    apath = apath.Remove(0, rindex + "Resources".Length + 1);

                    foreach (var de in depends)
                    {
                        var mt = AssetDatabase.GetMainAssetTypeAtPath(de);
                        if (mt == typeof(UnityEngine.Texture2D))
                        {
                            var mi = new SpriteAtlasMap.MapItem();
                            mi.atlasPath = apath;
                            mi.spritePath = de;
                            mi.spriteName = Path.GetFileNameWithoutExtension(de);
                            items.Add(mi);
                        }
                    }
                }

                outLog.Clear();
                bool shouldGen = true;

                Dictionary<string, SpriteAtlasMap.MapItem> records = new Dictionary<string, SpriteAtlasMap.MapItem>();

                foreach (var item in items)
                {
                    if (!records.ContainsKey(item.spriteName))
                    {
                        records.Add(item.spriteName, item);
                    }
                    else
                    {
                        if (!outLog.ContainsKey(item.spriteName))
                        {
                            outLog.Add(item.spriteName, records[item.spriteName].spritePath);
                        }
                        outLog[item.spriteName] += (" || " + item.spritePath);
                    }
                }

                if (shouldGen)
                {
                    SaveSpriteAtlasMap(items);
                }
            }

            EditorGUILayout.EndHorizontal();

            if (outLog.Keys.Count > 0)
            {
                DisplayDupicatedInfos();
            }
        }

        void DisplayDupicatedInfos()
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("请先保证所有的sprite不重名，再重新点击 Update Sprite Atlas Map, 以下是重名sprites：");
            EditorGUILayout.BeginVertical();
            foreach (var item in outLog.Keys)
            {
                EditorGUILayout.TextArea(string.Format("{0}  duplicated in paths: {1}", item, outLog[item]));
            }
            EditorGUILayout.EndVertical();
        }

        void SaveSpriteAtlasMap(List<SpriteAtlasMap.MapItem> sprite2AtlasList)
        {
            Dictionary<uint, List<SpriteAtlasMap.MapItem>> hash2Item = new Dictionary<uint, List<SpriteAtlasMap.MapItem>>();
            foreach (var s2a in sprite2AtlasList)
            {
                uint hash = AssetBundleUtils.GetHashCode(s2a.spriteName);
                List<SpriteAtlasMap.MapItem> sprites;
                if (!hash2Item.TryGetValue(hash, out sprites))
                {
                    sprites = new List<SpriteAtlasMap.MapItem>();
                    hash2Item.Add(hash, sprites);
                }
                sprites.Add(s2a);
            }

            var builder = new FlatBufferBuilder(1);

            List<Offset<SpriteHashMapAtlasFB>> h2A = new List<Offset<SpriteHashMapAtlasFB>>();
            List<Offset<SpriteNameMapAtlasFB>> n2A = new List<Offset<SpriteNameMapAtlasFB>>();

            foreach (var pair in hash2Item)
            {
                if (pair.Value.Count > 1)
                {
                    foreach (var item in pair.Value)
                    {
                        n2A.Add(SpriteNameMapAtlasFB.CreateSpriteNameMapAtlasFB(builder,
                            builder.CreateString(item.spriteName), builder.CreateString(item.atlasPath)));
                    }
                }
                else
                {
                    h2A.Add(SpriteHashMapAtlasFB.CreateSpriteHashMapAtlasFB(builder,
                        pair.Key, builder.CreateString(pair.Value[0].atlasPath)));
                }
            }
            Debug.Log("hash2Atlas Count: " + h2A.Count + " name2Atlas Count: " + n2A.Count);
            VectorOffset h2aVector = SpriteMapAtlasFB.CreateH2aVector(builder, h2A.ToArray());
            VectorOffset n2aVector = SpriteMapAtlasFB.CreateN2aVector(builder, n2A.ToArray());
            SpriteMapAtlasFB.StartSpriteMapAtlasFB(builder);
            SpriteMapAtlasFB.AddH2a(builder, h2aVector);
            SpriteMapAtlasFB.AddN2a(builder, n2aVector);
            var endOffset = SpriteMapAtlasFB.EndSpriteMapAtlasFB(builder);
            SpriteMapAtlasFB.FinishSpriteMapAtlasFBBuffer(builder, endOffset);
            string path = Path.Combine(SpriteAtlasFolderSetting.AtlasFolderPath_Editor, SpriteAtlasMap.MapFileName);
            File.WriteAllBytes(path, builder.SizedByteArray());
        }

        // 无论软链与否，都能根据选择的路径返回工程下的路径
        string GetWorkSpacePath(string tempPath)
        {
            var res = File.ReadAllText(tempPath + ".meta", System.Text.Encoding.UTF8);
            var guid = System.Text.RegularExpressions.Regex.Match(res, "guid: (.*?)[\r\n]{1}").Groups[1].Value;
            var realPath = AssetDatabase.GUIDToAssetPath(guid);
            return realPath;
        }
    }
}