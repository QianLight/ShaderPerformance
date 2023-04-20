using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;

namespace GSDK
{
    [CustomEditor(typeof(DownloadSettings))]
    public class DownloadInspector : Editor
    {
        private readonly string[] _version = {VersionCode.UnityVersion};

        private int _versionIndex;
        private int _regionIndex;

        private DownloadModel _model;

        private GameObject _gameObject;

        private DownloadSettings _settings;

        public void OnEnable()
        {
            _settings = (DownloadSettings) target;
            _model = _settings.Model;
            _model.RefreshRegionModel();
        }

        public override void OnInspectorGUI()
        {
            #region Version

            EditorGUILayout.Space();
            EditorGUILayout.Space();

            EditorGUILayout.HelpBox("1) 下载GSDK的版本", MessageType.None);
            _versionIndex = EditorGUILayout.Popup(_versionIndex, _version);
            _model.GSDKVersion = _version[_versionIndex];

            #endregion

            #region Plugins

            EditorGUILayout.Space();
            EditorGUILayout.Space();

            EditorGUILayout.HelpBox("2) 选择需要添加的组件", MessageType.None);

            var keys = new List<string>(_model.PluginMap.Keys);
            foreach (var entry in keys)
            {
                var pContent = new GUIContent(entry);
                _model.PluginMap[entry] = EditorGUILayout.Toggle(pContent, _model.PluginMap[entry]);
            }

            EditorGUILayout.Space();

            if (GUILayout.Button("全选"))
            {
                foreach (var key in keys)
                {
                    _model.PluginMap[key] = true;
                }
            }

            if (GUILayout.Button("清空"))
            {
                foreach (var key in keys)
                {
                    _model.PluginMap[key] = false;
                }
            }

            #endregion

            #region DownloadButton

            EditorGUILayout.Space();
            EditorGUILayout.Space();

            if (GUILayout.Button("开始下载"))
            {
                StartDownloadPlugins();
            }

            #endregion

            #region Delete3xCode

            EditorGUILayout.Space();
            EditorGUILayout.Space();
            EditorGUILayout.HelpBox("(可选) 如果不需要 Unity 3.x 代码，可用此按钮删除", MessageType.None);
            if (GUILayout.Button("删除 Unity 3.x 相关代码"))
            {
                DeleteUnity3xCode();
            }

            #endregion
        }

        private void StartDownloadPlugins()
        {
            var plugins = GetSelectPlugins();
            GLog.LogDebug("Download Tool Start Download Plugins ...");

            if (plugins.Length == 0)
            {
                GLog.LogInfo("Please choose a plugin to download.");
                return;
            }

            Directory.CreateDirectory(DownloadConstants.CurrentDownloadPath);

            foreach (var pluginName in plugins)
            {
                _gameObject = new GameObject();
                var task = _gameObject.AddComponent<DownloadTaskForEditor>();
                var filename = DownloadConstants.CurrentDownloadPath + pluginName + "_" + _model.GSDKVersion +
                               "_" + DownloadConstants.Region + DownloadConstants.FileType;

                // 下载组件是否存在
                if (!new FileInfo(filename).Exists)
                {
                    task.Download(pluginName, _model.GSDKVersion);
                }
                else
                {
                    AssetDatabase.ImportPackage(filename, false);
                }

                DestroyImmediate(_gameObject);
                GLog.LogDebug(pluginName + "Download Complete.");
            }
        }

        // 获取被勾选的组件
        private string[] GetSelectPlugins()
        {
            var list = new ArrayList();
            foreach (var entry in _model.PluginMap.Where(entry => entry.Value))
            {
                list.Add(entry.Key);
            }

            return (string[]) list.ToArray(typeof(string));
        }

        string CombinePaths(string[] paths)
        {
            string ret = "";
            foreach (var path in paths)
            {
                ret = Path.Combine(ret, path);
            }
            return ret;
        }
        
        private void DeleteUnity3xCode()
        {
            GLog.LogInfo("Start deleting Unity 3.x Code");
            string[] GSDKPath = new string[]
            {
                Directory.GetCurrentDirectory(),
                "Assets",
                "GSDK"
            };
            string unity3XCodeDir = CombinePaths(GSDKPath);
            
            GLog.LogInfo("Current Dir:" + unity3XCodeDir);

            List<string> filesShouldBeKeptRelativePath = new List<string>()
            {
                @"Common",
                @"Editor",
                Path.Combine(@"Service", @"DeepLink"),
                Path.Combine(@"Service", @"Download"),
                Path.Combine(@"Service", @"Service.cs"),
                @"GSDK.cs",
                @"GSDKPublicDefine.cs",
            };
            
            List<string> filesShouldBeKept = new List<string>();

            foreach (var filename in filesShouldBeKeptRelativePath)
            {
                string file = Path.Combine(unity3XCodeDir, filename);
                filesShouldBeKept.Add(file);
                GLog.LogDebug("adding " + file);
            }

            ReserveSubFileOrDirectoryAndDeleteOthers(filesShouldBeKept.ToArray(), unity3XCodeDir);
            GLog.LogInfo("Unity 3.x Code Deletion finished");
        }

        void ReserveSubFileOrDirectoryAndDeleteOthers(string[] subReserve, string rootPath)
        {
            string[] files = Directory.GetFiles(rootPath);
            foreach (var file in files)
            {
                if (!subReserve.Contains(file))
                {
                    GLog.LogInfo("deleting: " + file);
                    File.Delete(file);
                }
            }
            
            string[] directories = Directory.GetDirectories(rootPath);
            foreach (var directory in directories)
            {
                bool isParentDirectory = false;
                bool isReserveDirectory = false;
                foreach (var filename in subReserve)
                {
                    // 是需要保留的目录
                    if (filename.Equals(directory))
                    {
                        isReserveDirectory = true;
                        break;
                    }
                    // 是需要保留的母目录
                    if (filename.Contains(directory))
                    {
                        isParentDirectory = true;
                        break;
                    }
                }

                if (isReserveDirectory)
                {
                    continue;
                }
                if (isParentDirectory)
                {
                    ReserveSubFileOrDirectoryAndDeleteOthers(subReserve, directory);
                    if (Directory.GetFiles(directory).Length + Directory.GetDirectories(directory).Length == 0)
                    {
                        // 如果是空文件夹则删除
                        Directory.Delete(directory);
                    }
                }
                else
                {
                    Directory.Delete(directory, true);
                    GLog.LogInfo("deleting: " + directory);
                }
            }
        }
    }

    // 下载任务
    public class DownloadTaskForEditor : MonoBehaviour
    {
        public void Download(string plugin, string version)
        {
            StartCoroutine(DownloadPlugin(plugin, version));
        }

        private IEnumerator DownloadPlugin(string plugin, string version)
        {
            var url = DownloadConstants.UrlPath + plugin + "_" + version + "_" + DownloadConstants.Region +
                      DownloadConstants.FileType;
            // GMIM_2.11.0.0_domestic.unitypackage
            var fileName = plugin + "_" + version + "_" + DownloadConstants.Region + DownloadConstants.FileType;

            GLog.LogInfo("download url is " + url);

            var uwr = new UnityWebRequest(url, UnityWebRequest.kHttpVerbGET);
            var path = Path.Combine(DownloadConstants.CurrentDownloadPath, fileName);

            uwr.downloadHandler = new DownloadHandlerFile(path);
            uwr.SendWebRequest();

            if (uwr.isNetworkError || uwr.isHttpError)
            {
                GLog.LogError("Download " + plugin + "error. Please try again. Error : " + uwr.error);
                DeleteErrorFile(path);
            }
            else
            {
                GLog.LogDebug("Start Download " + plugin);

                var isError = false;
                while (!uwr.isDone)
                {
                    isError = uwr.isNetworkError || uwr.isHttpError;
                    if (isError)
                    {
                        GLog.LogError("Download " + plugin + "error. Please try again. Error : " + uwr.error);
                        DeleteErrorFile(path);
                        break;
                    }

                    // 更新下载进度
                    EditorUtility.DisplayProgressBar("Download plugins bar", plugin + " downloading...",
                        progress: uwr.downloadProgress);
                }

                EditorUtility.ClearProgressBar();

                if (!isError)
                {
                    GLog.LogDebug("filename is " + fileName);
                    AssetDatabase.ImportPackage(
                        DownloadConstants.CurrentDownloadPath + fileName, false);
                }

                yield return 1;
            }

            DestroyImmediate(gameObject);
        }

        private void DeleteErrorFile(string path)
        {
            File.Delete(path);
        }
    }
}