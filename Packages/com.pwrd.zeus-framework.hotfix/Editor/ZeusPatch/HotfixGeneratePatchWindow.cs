/*******************************************************************
* Copyright © 2017—2021 Perfect World.Co.Ltd. All rights reserved.
* author: 移动项目支持部 ZEUS FRAMEWORK TEAM
********************************************************************/
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using Zeus.Framework.Hotfix;
using UnityEngine;
using Zeus.Core;
using System.Reflection;

namespace Zeus.Framework.Lua
{
    public class HotfixGeneratePatchWindow : EditorWindow
    {

        HotfixPatchSetting hotfixPatchSetting;
        private HotfixLocalConfig m_hotfixLocalConfig;
        List<string> m_beforeGenerateToRemove = new List<string>();

        private const string TARGETPATH_STORE_KEY = "LuaPatchTargetPath";
        string _outputPath;

        List<string> _luaPathList;

        private void OnEnable()
        {
            LoadSetting();
            InitList();
        }

        [MenuItem("Zeus/Hotfix/Hotfix Patch", false, 2)]
        public static void Open()
        {
            HotfixGeneratePatchWindow luaPatchWindow = GetWindow<HotfixGeneratePatchWindow>("GenerateLuaPatch", typeof(HotfixGeneratePatchWindow));
            luaPatchWindow.Show();
        }

        private void OnGUI()
        {
            GUILayout.BeginHorizontal();
            _outputPath = EditorGUILayout.TextField("OutputPath", _outputPath);

            if (GUILayout.Button("Browse", EditorStyles.miniButton, GUILayout.Width(60)))
            {
                string temp = EditorUtility.OpenFolderPanel("Output Path", _outputPath, string.Empty);
                if (!string.IsNullOrEmpty(temp))
                {
                    _outputPath = temp;
                    PlayerPrefs.SetString(TARGETPATH_STORE_KEY, temp);
                }
            }
            GUILayout.EndHorizontal();
            EditorGUILayout.Space();
            EditorGUILayout.Space();

            bool bLuaPatch = EditorGUILayout.Toggle("Use Lua Patch", hotfixPatchSetting.bLuaPatch);
            if (bLuaPatch != hotfixPatchSetting.bLuaPatch)
            {
                hotfixPatchSetting.bLuaPatch = bLuaPatch;
                InitList();
            }
            if (hotfixPatchSetting.bLuaPatch)
            {
                hotfixPatchSetting.useLuaEncryptor = EditorGUILayout.Toggle("是否Lua加密", hotfixPatchSetting.useLuaEncryptor);
                EditorGUILayout.HelpBox("如果开启Use Lua Patch，请确保已经将Zeus框架内Lua模块添加到当前工程", MessageType.Warning);
            }
            EditorGUILayout.Space();
            m_hotfixLocalConfig.Version = EditorGUILayout.TextField("Version(补丁版本)：", m_hotfixLocalConfig.Version);
            EditorGUILayout.Space();

            EditorGUILayout.LabelField("包含的文件夹");
            EditorGUILayout.HelpBox("在patch里的路径是指打出来的patch中的相对路径，如果为空，则直接放到根目录下。具体表现，可以打个patch查看下内容，更直观一些", MessageType.Info);

            bool SelectIncludeFolder(int index)
            {
                string temp;
                if (string.IsNullOrEmpty(hotfixPatchSetting.includeFolder[index].AbsolatePath))
                {
                    temp = EditorUtility.OpenFolderPanel("LuaPatch Include Folder", "", "");
                }
                else
                {
                    temp = EditorUtility.OpenFolderPanel("LuaPatch Include Folder", hotfixPatchSetting.includeFolder[index].AbsolatePath, "");
                }
                bool valid = !string.IsNullOrEmpty(temp);
                if (valid)
                {
                    //foreach (var patchPathItem in hotfixPatchSetting.includeFolder)
                    for(var i = 0; i < hotfixPatchSetting.includeFolder.Count; i++)
                    {
                        if(i == index)
                        {
                            continue;
                        }
                        var patchPathItem = hotfixPatchSetting.includeFolder[i];
                        if (null == patchPathItem)
                        {
                            continue;
                        }
                        if(string.IsNullOrEmpty(patchPathItem.AbsolatePath))
                        {
                            continue;
                        }
                        if (patchPathItem.AbsolatePath == temp)
                        {
                            valid = false;
                            EditorUtility.DisplayDialog("Error", "相同的路径已存在", "ok");
                            break;
                        }
                        if (patchPathItem.AbsolatePath.Contains(temp))
                        {
                            valid = false;
                            EditorUtility.DisplayDialog("Error", $"选择的路径包含了已有的路径:\n选择:\n{temp}\n已有:\n{patchPathItem.AbsolatePath}", "ok");
                            break;
                        }
                        if (temp.Contains(patchPathItem.AbsolatePath))
                        {
                            valid = false;
                            EditorUtility.DisplayDialog("Error", $"选择的路径已经被包含:\n选择:\n{temp}\n已有:\n{patchPathItem.AbsolatePath}", "ok");
                            break;
                        }
                    }
                }
                if (valid)
                {
                    hotfixPatchSetting.includeFolder[index].AbsolatePath = temp;
                    hotfixPatchSetting.includeFolder[index].NameInPatch = Path.GetFileName(temp);
                    return true;
                }
                return false;
            }

            for (int i = 0; i < hotfixPatchSetting.includeFolder.Count; ++i)
            {
                if (hotfixPatchSetting.includeFolder[i] == null)
                {
                    continue;
                }
                GUILayout.BeginHorizontal();
                hotfixPatchSetting.includeFolder[i].NameInPatch = EditorGUILayout.TextField("在patch里的路径", hotfixPatchSetting.includeFolder[i].NameInPatch);
                GUILayout.Label("绝对路径:" + hotfixPatchSetting.includeFolder[i].AbsolatePath);
                if (GUILayout.Button("…", GUILayout.Width(20)))
                {
                    SelectIncludeFolder(i);
                }
                if (GUILayout.Button("-", GUILayout.Width(20)))
                {
                    hotfixPatchSetting.includeFolder[i] = null;
                }
                GUILayout.EndHorizontal();
            }

            if (GUILayout.Button("+", GUILayout.Width(20)))
            {
                hotfixPatchSetting.includeFolder.Add(new IncludeFolderItem());
                var lastIndex = hotfixPatchSetting.includeFolder.Count - 1;
                var added = SelectIncludeFolder(lastIndex);
                if(!added)
                {
                    hotfixPatchSetting.includeFolder.RemoveAt(lastIndex);
                }
            }

            EditorGUILayout.Space();
            EditorGUILayout.Space();

            if(hotfixPatchSetting.m_beforeGenerate.Count > 0)
            {
                EditorGUILayout.HelpBox("注意：这里填的方法一定要是在Editor下定义的不含参数的的静态方法", MessageType.Warning);
            }
            for(var i = 0; i < hotfixPatchSetting.m_beforeGenerate.Count; i++)
            {
                GUILayout.BeginHorizontal();
                hotfixPatchSetting.m_beforeGenerate[i] = GUILayout.TextField(hotfixPatchSetting.m_beforeGenerate[i]);
                if (GUILayout.Button("-", GUILayout.Width(20)))
                {
                    m_beforeGenerateToRemove.Add(hotfixPatchSetting.m_beforeGenerate[i]);
                }
                GUILayout.EndHorizontal();
            }
            hotfixPatchSetting.m_beforeGenerate.RemoveAll(item => m_beforeGenerateToRemove.Contains(item));
            m_beforeGenerateToRemove.Clear();
            if (GUILayout.Button("添加打Patch前的预处理"))
            {
                hotfixPatchSetting.m_beforeGenerate.Add("");
            }

            EditorGUILayout.Space();

            hotfixPatchSetting.buildTarget = (BuildTarget)EditorGUILayout.EnumPopup("Platform: ", hotfixPatchSetting.buildTarget);

            if (GUILayout.Button("Save"))
            {
                SaveSetting();
            }
            EditorGUILayout.Separator();
            if(GUILayout.Button("Generate Patch"))
            {

                //生成patch之前先保存下，以使在patch中的版本信息是这里设的信息
                HotfixLocalConfigHelper.SaveLocalConfig(m_hotfixLocalConfig);
                _outputPath = _outputPath.Replace("\\", "/");

                string fileName = m_hotfixLocalConfig.Version;

                new HotfixGeneratePatch(_luaPathList, hotfixPatchSetting.includeFolder, hotfixPatchSetting.m_beforeGenerate).Generate(_outputPath, fileName, hotfixPatchSetting.buildTarget, hotfixPatchSetting.bLuaPatch, hotfixPatchSetting.useLuaEncryptor);
            }
        }

        private void OnDestroy()
        {
            PlayerPrefs.SetString(TARGETPATH_STORE_KEY, _outputPath);
        }

        private void InitList()
        {
            if (hotfixPatchSetting.bLuaPatch)
            {
                System.Type encryptorType = Assembly.Load("Zeus.Lua.Editor").GetType("Zeus.Framework.Lua.HotfixLuaPatch");
                object encryptor = System.Activator.CreateInstance(encryptorType);
                MethodInfo method = encryptorType.GetMethod("GetLualist");
                object[] parames = null;
                _luaPathList = (List<string>)method.Invoke(encryptor, parames);
            }
            else
            {
                _luaPathList = new List<string>();
            }
        }

        private void LoadSetting()
        {
            m_hotfixLocalConfig = HotfixLocalConfigHelper.LoadLocalConfig();
            hotfixPatchSetting = HotfixPatchSetting.LoadSetting();
            _outputPath = PlayerPrefs.GetString(TARGETPATH_STORE_KEY, "");
        }

        private void SaveSetting()
        {
            HotfixPatchSetting.SaveSetting(hotfixPatchSetting);
            HotfixLocalConfigHelper.SaveLocalConfig(m_hotfixLocalConfig);
        }
    }
}
