/*******************************************************************
* Copyright © 2017—2021 Perfect World.Co.Ltd. All rights reserved.
* author: 移动项目支持部 ZEUS FRAMEWORK TEAM
********************************************************************/
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;
using System;

namespace Zeus.Core.FileSystem
{
    public class FileSystemSettingWindow : EditorWindow
    {
        FileSystemSettingConfig _localConfig;
        Vector2 vec = Vector2.zero;
        private void OnEnable()
        {
            Init();
        }

        private void Init()
        {
            _localConfig = FileSystemSetting.LoadLocalConfig();
            Zeus.Core.FileSystem.OuterPackage.Init();
        }

        [MenuItem("Zeus/FileSystem/FileSystemSettingWindow", false, 21)]
        public static void Open()
        {
            FileSystemSettingWindow window = GetWindow<FileSystemSettingWindow>();
            window.titleContent = new GUIContent("FileSystemSetting");
            window.Show();
        }

        private void OnGUI()
        {
            GUILayout.BeginVertical();

            _localConfig.isCombineFile = EditorGUILayout.Toggle("是否开启文件合并", _localConfig.isCombineFile);
            GUILayout.Space(10);
            vec = GUILayout.BeginScrollView(vec);
            GUILayout.Label("覆盖安装冗余文件处理");
            EditorGUILayout.HelpBox("当使用不同版本的安装包覆盖安装后,会对“Application.persistentDataPath/OuterPackage”文件夹下的文件进行清理，" +
                "在删除时，会对以下列表中OuterPackage子文件夹内的文件进行校验，如果在覆盖安装前后的两个版本间资源未发生变化，则其不会被删除;" +
                "其他OuterPackage文件夹下的文件则会直接删除。", MessageType.Info);
            for (int i = 0; i < _localConfig.RetainFileDirectoryList.Count; i++)
            {
                GUILayout.BeginHorizontal();
                EditorGUILayout.LabelField($"Path{i}:", GUILayout.Width(90));
                _localConfig.RetainFileDirectoryList[i] = EditorGUILayout.TextArea(_localConfig.RetainFileDirectoryList[i]);
                if (GUILayout.Button("修改", GUILayout.Width(50)))
                {
                    string oldPath = Application.dataPath.Replace("Assets", string.Empty) + OuterPackage.GetRealPath(_localConfig.RetainFileDirectoryList[i]);
                    if (!System.IO.Directory.Exists(oldPath))
                    {
                        System.IO.Directory.CreateDirectory(oldPath);
                    }
                    string newPath = EditorUtility.OpenFolderPanel("需要冗余处理的文件夹", oldPath, string.Empty);
                    string prePath = Application.dataPath.Replace("Assets", string.Empty) + OuterPackage.GetRealPath(string.Empty);
                    if (newPath.StartsWith(prePath))
                    {
                        _localConfig.RetainFileDirectoryList[i] = newPath.Replace(prePath, string.Empty);
                    }
                    else
                    {
                        EditorUtility.DisplayDialog("注意", "请选择或者手动输入运行时存在于OuterPackage文件夹内需要清理的文件夹", "确定");
                    }
                }
                GUILayout.Space(10);
                if (GUILayout.Button("-", GUILayout.Width(30)))
                {
                    _localConfig.RetainFileDirectoryList.RemoveAt(i);
                    i--;
                }
                GUILayout.EndHorizontal();
                GUILayout.Space(5);
            }
            if (GUILayout.Button("新增"))
            {
                _localConfig.RetainFileDirectoryList.Add("");
            }
            if (GUILayout.Button("清空"))
            {
                _localConfig.RetainFileDirectoryList.Clear();
            }
            GUILayout.Space(10);
            GUILayout.Label("第一次运行前需要拷贝到包外的文件设置,可以选择单独的文件，也可以将需要拷贝的文件写到Json文件里，添加Json文件，前者适合固定并且独立的文件，后者适合工具生成的内容");
            EditorGUILayout.HelpBox("以下列出的文件会被拷贝到包外", MessageType.Info);
            for (var i = _localConfig.InnerToOuterFileList.Count - 1; i >= 0; i--)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label(_localConfig.InnerToOuterFileList[i]);
                if (GUILayout.Button("-", GUILayout.Width(30)))
                {
                    _localConfig.InnerToOuterFileList.RemoveAt(i);
                }
                GUILayout.EndHorizontal();
            }
            if (GUILayout.Button("添加需要拷贝到包外的文件"))
            {
                var path = EditorUtility.OpenFilePanel("选择需要拷贝到的包外的文件", Application.streamingAssetsPath, "*");
                if (!string.IsNullOrEmpty(path))
                {
                    if (path.StartsWith(Application.streamingAssetsPath))
                    {
                        var relativePath = path.Substring(Application.streamingAssetsPath.Length + 1);
                        if (_localConfig.InnerToOuterFileList.Contains(relativePath))
                        {
                            EditorUtility.DisplayDialog("Error", $"要添加的文件已存在，请勿重复添加", "ok");
                        }
                        else
                        {
                            _localConfig.InnerToOuterFileList.Add(relativePath);
                        }
                    }
                    else
                    {
                        EditorUtility.DisplayDialog("Error", "拷贝到包外的文件需要在StreamingAssets下", "ok");
                    }
                }
            }
            EditorGUILayout.HelpBox("以下Json文件中所列的文件会被拷贝到包外,Json文件的内容为String Array eg. [\"file1\", \"file2\"]", MessageType.Info);
            for (var i = _localConfig.InnerToOuterJsonList.Count - 1; i >= 0; i--)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label(_localConfig.InnerToOuterJsonList[i]);
                if (GUILayout.Button("-", GUILayout.Width(30)))
                {
                    _localConfig.InnerToOuterJsonList.RemoveAt(i);
                }
                GUILayout.EndHorizontal();
            }
            if (GUILayout.Button("添加包含拷贝到包外文件列表的Json文件"))
            {
                var formatedParentPath = Path.Combine(Application.dataPath, "ZeusSetting").Replace("\\", "/");
                var path = EditorUtility.OpenFilePanel("选择需要拷贝到的包外的文件的配置", formatedParentPath, "json");
                if (!string.IsNullOrEmpty(path))
                {
                    if (path.StartsWith(formatedParentPath))
                    {
                        try
                        {
                            var jsonContent = File.ReadAllText(path);
                            var list = JsonUtilityExtension.FromJsonArray<List<string>>(jsonContent);
                            if(list.Count > 0)
                            {
                                var relativePath = path.Substring(formatedParentPath.Length + 1);
                                if(_localConfig.InnerToOuterJsonList.Contains(relativePath))
                                {
                                    EditorUtility.DisplayDialog("Error", $"要添加的配置已存在，请勿重复添加", "ok");
                                }
                                else
                                {
                                    _localConfig.InnerToOuterJsonList.Add(relativePath);
                                }
                            }
                            else
                            {
                                EditorUtility.DisplayDialog("Error", $"json内配置的路径为空，请检查json内容和格式", "ok");
                            }
                        }
                        catch (Exception e)
                        {
                            EditorUtility.DisplayDialog("Error", $"json配置解析失败:{e}", "ok");
                        }
                    }
                    else
                    {
                        EditorUtility.DisplayDialog("Error", "拷贝到包外的文件配置需要在ZeusSetting下", "ok");
                    }
                }
            }
            GUILayout.Space(10);
            if (GUILayout.Button("保存Config"))
            {
                FileSystemSetting.SaveLocalConfig(_localConfig);
            }

            GUILayout.EndScrollView();
            GUILayout.EndVertical();
        }
    }
}
