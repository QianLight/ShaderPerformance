/*******************************************************************
* Copyright © 2017—2021 Perfect World.Co.Ltd. All rights reserved.
* author: 移动项目支持部 ZEUS FRAMEWORK TEAM
********************************************************************/
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using UnityEditor;
using Zeus.Build;
using Zeus.Framework;


namespace Zeus.Core.FileSystem
{
    //[OnBeforePlay]
    public class EditorVFileSystem : IBeforeBuild, IAfterBuild
    {
        private const string BundleExtension = ".unity3d";
        private const string ManifestExtension = ".manifest";

        OtherFileList _OtherFileList;
        /// <summary>
        /// 统计 StreamingAssets 下所有非 AssetBundle 的文件，游戏在设备上第一次启动的
        /// 时候会将这些文件拷贝到读写目录(ZEUS_FIRSTCOPY)。
        /// </summary>
        public void OnBeforeBuild(BuildTarget target, string outputPath)
        {
            string abRoot = EditorAssetBundleUtils.GetPathRoot(target);
            GenOtherFileList(abRoot);
        }

        /// <summary>
        /// 删除OtherFileList。
        /// </summary>
        public void OnAfterBuild(BuildTarget target, string outputPath)
        {
            _OtherFileList = null;
        }


        private void GenOtherFileList(string abRoot)
        {
            _OtherFileList = new OtherFileList();
            _OtherFileList.Init();

            var dicInclude = GameBuildProcessor.DictionaryIncludeBuildPath;
            var dicExclude = GameBuildProcessor.DictionaryExcludeBuildPath;

            foreach (var pair in dicInclude)
            {
                string directory = PathUtil.FormatPathSeparator(pair.Key);
                string relativeRoot = PathUtil.FormatPathSeparator(pair.Value);
                _ProcessDir(new DirectoryInfo(directory), abRoot, dicExclude, relativeRoot);
            }

            _OtherFileList.SerializeToDisk();
        }
        private void _ProcessDir(DirectoryInfo dirInfo, string abRoot, Dictionary<string, bool> dicExcluce, string relativeRoot)
        {
            foreach (var fileInfo in dirInfo.GetFiles())
            {
                if (!_IsAssetBundle(fileInfo, abRoot) &&
                    !_IsAssetBundleManifest(fileInfo, abRoot) && fileInfo.Extension != ".meta" &&
                    !dicExcluce.ContainsKey(fileInfo.FullName))
                {
                    string fileFullPath = fileInfo.FullName;
                    string fileRelativePath = fileFullPath.Replace(dirInfo.FullName, "");
                    fileRelativePath = Path.Combine(relativeRoot, fileRelativePath);
                    _OtherFileList.m_ListFileName.Add(fileRelativePath);
                }
            }

            foreach (var subDir in dirInfo.GetDirectories())
            {
                if (!dicExcluce.ContainsKey(subDir.FullName))
                {
                    _ProcessDir(subDir, abRoot, dicExcluce, relativeRoot);
                }
            }
        }
        private bool _IsAssetBundle(FileInfo fileInfo, string abRoot)
        {
            string root = PathUtil.GetRelativePath(fileInfo.DirectoryName);
            return (root == abRoot || root == Path.Combine("Assets", abRoot)) && fileInfo.Extension == BundleExtension;
        }

        private bool _IsAssetBundleManifest(FileInfo fileInfo, string abRoot)
        {
            string root = PathUtil.GetRelativePath(fileInfo.DirectoryName);
            return (root == abRoot || root == Path.Combine("Assets", abRoot)) && fileInfo.Extension == ManifestExtension;
        }
    }

}
