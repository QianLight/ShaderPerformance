/*******************************************************************
* Copyright © 2017—2021 Perfect World.Co.Ltd. All rights reserved.
* author: 移动项目支持部 ZEUS FRAMEWORK TEAM
********************************************************************/
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Reflection;
using System;
using UnityEditor;
using System.Linq;

namespace Zeus.Framework.Hotfix
{
    [Serializable]
    public class IncludeFolderItem
    {
        public string AbsolatePath;
        public string NameInPatch;
    }

    public class HotfixGeneratePatch
    {
        private List<string> supportedAssemblies
        {
            get
            {
                return new List<string>
                {
                    "Assembly-CSharp-Editor",
                };
            }
        }
        private const string patchExtension = ".luapatch";
        internal List<string> _listLuaPath;
        internal List<IncludeFolderItem> _otherIncludePath;
        List<string> m_beforeGenerateCallback;
        BuildTarget _buildTarget;
        bool _bLuaPatch;
        bool _useLuaEncryptor;
        string _tempDirectory;
        public string HotfixVersionDir
        {
            get
            {
                return Application.dataPath + "/ZeusSetting/BuildinSetting";
            }
        }

        public string HotfixVersionDirRelpath
        {
            get
            {
                return Application.dataPath;
            }
        }

 		public HotfixGeneratePatch(List<string> listLuaPath, List<IncludeFolderItem> otherIncludePath, List<string> beforeGenerateCallback)
        {
            _listLuaPath = SimplifyPathList(listLuaPath);
            var otherListToSimplify = new List<string>();
            for(var i = 0; i < otherIncludePath.Count; i++)
            {
                otherListToSimplify.Add(otherIncludePath[i].AbsolatePath);
            }
            var simplifiedOtherList = SimplifyPathList(otherListToSimplify);
            var items = from item in otherIncludePath where simplifiedOtherList.Contains(item.AbsolatePath) select item;
            _otherIncludePath = items.ToList();
            m_beforeGenerateCallback = beforeGenerateCallback;
        }

        //去掉重复路径以及子路径
        private List<string> SimplifyPathList(List<string> listLuaPath)
        {
            HashSet<string> set = new HashSet<string>();
            foreach (string path in listLuaPath)
            {
                set.Add(path);
            }
            listLuaPath = new List<string>(set);
            string[] listNeedRemove = PathUtil.GetChildPaths(listLuaPath);
            foreach (string pathNeedRemove in listNeedRemove)
            {
                listLuaPath.Remove(pathNeedRemove);
            }
            return listLuaPath;
        }

        private void GenerateLuaPatch()
        {
            if (_bLuaPatch)
            {
                CopyLuaPathtoStreamingAssets(Application.streamingAssetsPath);
                Encode(_buildTarget);
            }
        }

        private bool ProcessBeforeGenerate()
        {
            try
            {
                foreach(var callback in m_beforeGenerateCallback)
                {
                    foreach(var supportedAssembly in supportedAssemblies)
                    {
                        var assembly = Assembly.Load(supportedAssembly);
                        var lastDotIndex = callback.LastIndexOf(".");
                        var funName = callback.Substring(lastDotIndex + 1, callback.Length - lastDotIndex - 1);
                        var typeName = callback.Substring(0, lastDotIndex);
                        var type = assembly.GetType(typeName);
                        var method = type.GetMethod(funName);
                        method.Invoke(null, null);
                    }
                }
            }
            catch(Exception e)
            {
                EditorUtility.DisplayDialog("Error", $"预处理执行失败，请检查填入的回调方法是否存在并符合要求", "ok");
                return false;
            }
            return true;
        }

        public void Generate(string targetPath, string fileName, BuildTarget buildTarget, bool bLuaPatch, bool useLuaEncryptor)
        {
            if (!ProcessBeforeGenerate())
            {
                return;
            }
            _tempDirectory = Path.Combine(targetPath, "LuaPatchTemp");
            _buildTarget = buildTarget;
            _bLuaPatch = bLuaPatch;
            _useLuaEncryptor = useLuaEncryptor;
            fileName = fileName + patchExtension;
            bool success = true;
            try
            {
                GenerateLuaPatch();
                MoveFileToTargetDir();
                MoveVersionFileToTargetDir();
                Compress(targetPath, fileName);

            }
            catch (Exception e)
            {
                success = false;
                Debug.LogError(e);
                string filePath = targetPath.Replace("\\", "/") + "/" + fileName;
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                }
            }
            finally
            {
                Finish();
                EditorUtility.DisplayDialog("生成结果", success ? "生成成功，文件路径:"+ targetPath.Replace("\\", "/") + "/" + fileName : "生成失败，详细信息见console界面", "确定");
            }
        }

        private void Encode(BuildTarget buildTarget)
        {
            if (_useLuaEncryptor)
            {
                Type encryptorType = Assembly.Load("Zeus.Lua.Editor").GetType("Zeus.Framework.Lua.HotfixLuaPatch");
                if(null != encryptorType)
                {
                    object encryptor = System.Activator.CreateInstance(encryptorType);
                    MethodInfo method = encryptorType.GetMethod("Generate");
                    object[] parames = { buildTarget };
                    method.Invoke(encryptor, parames);
                }
                else
                {
                    Debug.LogError("no Type Zeus.Framework.Lua.HotfixLuaPatch in Zeus.Lua.Editor");
                }
            }
        }

        /// <summary>
        /// 拷贝版本信息
        /// </summary>
        private void MoveVersionFileToTargetDir()
        {
            Zeus.Core.FileUtil.CopyDirectory(HotfixVersionDir, Path.Combine(_tempDirectory, "ZeusSetting"));
        }

        private void MoveFileToTargetDir()
        {
            if (_bLuaPatch)
            {
                Zeus.Core.FileUtil.MoveDirectory(Path.Combine(Application.streamingAssetsPath, "Zeus/Lua"), Path.Combine(_tempDirectory, "Zeus"), true, false);
                Zeus.Core.FileUtil.MoveDirectory(Path.Combine(Application.streamingAssetsPath, "Tolua/Lua"), Path.Combine(_tempDirectory, "Tolua"), true, false);
                foreach (string p in _listLuaPath)
                {
                    string path = Path.Combine(Application.streamingAssetsPath, p);
                    if (Directory.Exists(path))
                    {
                        Zeus.Core.FileUtil.MoveDirectory(path, Path.Combine(_tempDirectory));
                    }
                    if (File.Exists(path))
                    {
                        Zeus.Core.FileUtil.EnsureFolder(Path.Combine(_tempDirectory, p));
                        File.Move(path, Path.Combine(_tempDirectory, p));
                    }
                }
            }
            foreach (var patchItem in _otherIncludePath)
            {
                if (File.Exists(patchItem.AbsolatePath))
                {
                    string desPath = Path.Combine(_tempDirectory, patchItem.NameInPatch);
                    if (File.Exists(desPath))
                    {
                        File.Delete(desPath);
                    }
                    File.Copy(patchItem.AbsolatePath, desPath, true);
                }
                else if (Directory.Exists(patchItem.AbsolatePath))
                {
                    if(!string.IsNullOrEmpty(patchItem.NameInPatch))
                    {
                        Zeus.Core.FileUtil.CopyDirectory(patchItem.AbsolatePath, _tempDirectory, true, false, patchItem.NameInPatch);
                    }
                    else
                    {
                        Zeus.Core.FileUtil.CopyFilesInDirectory(patchItem.AbsolatePath, _tempDirectory, true, false);
                    }
                }
            }

            Zeus.Core.FileUtil.DeleteEmptyDirectory(Application.streamingAssetsPath);
        }

        private void Finish()
        {
            if (Directory.Exists(Path.Combine(Application.streamingAssetsPath, "Zeus")))
            {
                Directory.Delete(Path.Combine(Application.streamingAssetsPath, "Zeus"), true);
            }
            if (Directory.Exists(Path.Combine(Application.streamingAssetsPath, "Tolua")))
            {
                Directory.Delete(Path.Combine(Application.streamingAssetsPath, "Tolua"), true);
            }
            foreach (string p in _listLuaPath)
            {
                string path = Path.Combine(Application.streamingAssetsPath, p);
                if (Directory.Exists(path))
                {
                    Directory.Delete(path, true);
                }
                else if (File.Exists(path))
                {
                    File.Delete(path);
                }
            }
            if (Directory.Exists(_tempDirectory))
            {
                Directory.Delete(_tempDirectory, true);
            }
            Debug.Log("Generate lua patch finished.");
        }

        private void CopyLuaPathtoStreamingAssets(string targetPath)
        {
            if (!Directory.Exists(targetPath))
            {
                Directory.CreateDirectory(targetPath);
            }
            CopyLuaFolder(PackageUtility.GetPackageFullPath("com.pwrd.zeus-framework.lua") + "/Runtime/Lua", targetPath + "/Zeus/Lua");
            CopyLuaFolder(PackageUtility.GetPackageFullPath("com.pwrd.zeus-framework.lua") + "/Tolua/Lua", targetPath + "/Tolua/Lua");
            foreach (string p in _listLuaPath)
            {
                if (string.IsNullOrEmpty(p))
                    continue;
                string path = (Application.dataPath + "/" + p).Replace("\\", "/");
                //如果是文件直接复制
                if (File.Exists(path))
                {
                    CopyFile(path, targetPath + "/ZeusSetting/BuildinSetting/" + Path.GetFileName(path));
                }
                else
                {
                    CopyLuaFolder(path, targetPath + path.Substring(path.LastIndexOf("/")));
                }
            }
        }

        private void CopyLuaFolder(string srcFolder, string targetFolderPath)
        {
            if (!Directory.Exists(srcFolder))
            {
                Debug.LogWarning(string.Format("The path \"{0}\" is not existed.", srcFolder));
                return;
            }
            string searchPattern = "*.lua";
            foreach (string file in Directory.GetFiles(srcFolder, searchPattern, SearchOption.AllDirectories))
            {
                string targetRelativePath = file.Replace("\\", "/").Replace(srcFolder, "");
                CopyFile(file, targetFolderPath + targetRelativePath);
            }
        }

        private void CopyFile(string file, string targetPath)
        {
            if (File.Exists(targetPath))
            {
                Debug.LogError(string.Format("{0} already exist.", targetPath));
                return;
            }
            string parentDirectory = Path.GetDirectoryName(targetPath);
            if (!Directory.Exists(parentDirectory))
            {
                Directory.CreateDirectory(parentDirectory);
            }
            File.Copy(file, targetPath);
        }

        private void Compress(string targetPath, string fileName)
        {
            if (string.IsNullOrEmpty(fileName))
            {
                fileName = "assets";
            }
            if (File.Exists(targetPath + "/" + fileName))
            {
                File.Delete(targetPath + "/" + fileName);
            }
            Zeus.Core.ZipUtil.ZipFileFromDirectory(_tempDirectory, targetPath + "/" + fileName);
        }
    }
}