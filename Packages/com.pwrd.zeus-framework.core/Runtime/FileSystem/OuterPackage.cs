/*******************************************************************
* Copyright © 2017—2020 Perfect World.Co.Ltd. All rights reserved.
* author: 移动项目支持部 ZEUS FRAMEWORK TEAM
********************************************************************/
using System;
using System.Collections;
using System.IO;
using UnityEngine;
using System.Collections.Generic;
using Zeus.Framework;
using System.Text;

namespace Zeus.Core.FileSystem
{
    public static class OuterPackage
    {

        private static string _ROOT;
        private static string _ROOT_BACKSLASH;
        private static byte[] _CopyCache = new byte[128 * ZeusConstant.KB];

        private const string CLEAR_OUTPACKAGE_FLAG = "zeus_ClearOutPackageFiles";
        private const string PRE_BUILD_GUID = "zeus_prebuildGUID";
        private const string COPY_FILE_ON_FIRST_LAUNCH_FLAG = "zeus_CopyFileFromInner2Outer";

        private static Func<List<string>> _forceDeleteFileFunc;

        private static Func<List<string>> _retainCacheFileFunc;

        private static Action _afterClearOuterPackageFunc;

        private static Func<List<string>> _copyFileInner2OuterFunc;

        private static Func<string> _innerPackageVersionQueryFunc;

        internal static void Init()
        {
#if UNITY_EDITOR
            _ROOT = "OuterPackage";
#else
            _ROOT = Application.persistentDataPath + "/OuterPackage";
#endif

            _ROOT_BACKSLASH = _ROOT + "/";
#if !UNITY_EDITOR || ZEUS_FIRSTCOPY
            Directory.CreateDirectory(_ROOT);
#endif
        }

        /// <summary>
        /// 通过虚拟路径获取真实路径，路径可读可写。
        /// </summary>
        public static string GetRealPath(string virtualPath)
        {
            return _ROOT_BACKSLASH + virtualPath;
        }

        internal static void CopyFromInternal(string vitualPath)
        {
            CopyFromInternal(vitualPath, _CopyCache);
        }

        internal static bool CopyFromInternal(string vitualPath, byte[] buffer)
        {
            try
            {
                if (InnerPackage.ExistsFile(vitualPath))
                {
                    Stream read = null;
                    Stream write = null;
                    int readBytes = 0;
                    using (read = InnerPackage.OpenReadStream(vitualPath))
                    {
                        string targetPath = GetRealPath(vitualPath);
                        string tempPath = targetPath + ".copyTemp";
                        string dirPath = Path.GetDirectoryName(tempPath);
                        if (!Directory.Exists(dirPath))
                        {
                            Directory.CreateDirectory(dirPath);
                        }
                        if (File.Exists(tempPath))
                        {
                            File.Delete(tempPath);
                        }
                        using (write = File.Create(tempPath))
                        {
                            while (true)
                            {
                                readBytes = read.Read(buffer, 0, buffer.Length);
                                if (readBytes <= 0)
                                {
                                    break;
                                }
                                write.Write(buffer, 0, readBytes);
                            }
                            write.Flush();
                        }
                        if (File.Exists(targetPath))
                        {
                            File.Delete(targetPath);
                        }
                        File.Move(tempPath, targetPath);
                    }
                }
                else
                {
                    Debug.LogError("Try Copy File(" + vitualPath + ") To OuterPackage,But It's Not Exist.");
                    return false;
                }
            }
            catch (Exception e)
            {
                Debug.LogError("CopyFromInternal: " + vitualPath + "," + e.ToString());
                return false;
            }
            return true;
        }

        /// <summary>
        /// 清空所有文件
        /// </summary>
        internal static void Clear()
        {
            if (Directory.Exists(_ROOT))
            {
                Directory.Delete(_ROOT, true);
            }
        }

        /// <summary>
        /// 返回覆盖安装或者清理包外文件的时候强制删除的文件，优先级最高，会直接删除。注：返回的文件路径要【绝对路径】！！！
        /// </summary>
        /// <param name="action"></param>
        public static void RegistDeleteRedundantForceDeleteFileFunc(Func<List<string>> action)
        {
            _forceDeleteFileFunc += action;
        }

        public static void UnregistDeleteRedundantForceDeleteFileFunc(Func<List<string>> action)
        {
            _forceDeleteFileFunc -= action;
        }

        public static void ClearDeleteRedundantForceDeleteFileFunc()
        {
            _forceDeleteFileFunc = null;
        }

        private static bool InvokeForceDelete()
        {
            bool rtn = true;
            try
            {
                if (_forceDeleteFileFunc != null)
                {
                    Delegate[] delegates = _forceDeleteFileFunc.GetInvocationList();
                    for (int i = 0; i < delegates.Length; i++)
                    {
                        List<string> filePathList = (List<string>)delegates[i].DynamicInvoke();
                        if (filePathList != null && filePathList.Count > 0)
                        {
                            for (int j = 0; j < filePathList.Count; j++)
                            {
                                try
                                {
                                    if (File.Exists(filePathList[j]))
                                    {
                                        File.Delete(filePathList[j]);
                                    }
                                }
                                catch (Exception ex)
                                {
                                    rtn = false;
                                    UnityEngine.Debug.LogError(ex);
                                }
                            }
                        }
                    }
                }
            }
            catch(Exception ex)
            {
                rtn = false;
                UnityEngine.Debug.LogError(ex);
            }
            return rtn;
        }

        /// <summary>
        /// 删除冗余文件的时候保留回调返回的文件，注：返回的文件路径要【绝对路径】！！！
        /// </summary>
        /// <param name="action"></param>
        public static void RegistDeleteRedundantFileFunc(Func<List<string>> action)
        {
            _retainCacheFileFunc += action;
        }

        public static void UnregistDeleteRedundantFileFunc(Func<List<string>> action)
        {
            _retainCacheFileFunc -= action;
        }

        public static void ClearDeleteRedundantFileFunc()
        {
            _retainCacheFileFunc = null;
        }

        /// <summary>
        /// 在清理OuterPackage文件夹的逻辑之后执行，可用来主动清理一些 ForceDeleteOutPackageFilesOnNextLaunch() 函数删除不到的东西，例如PlayerPrefs存储的信息
        /// </summary>
        /// <param name="action"></param>
        public static void RegistAfterClearOuterPackageFunc(Action action)
        {
            _afterClearOuterPackageFunc += action;
        }

        public static void UnregistAfterClearOuterPackageFunc(Action action)
        {
            _afterClearOuterPackageFunc -= action;
        }

        public static void ClearAfterClearOuterPackageFunc()
        {
            _afterClearOuterPackageFunc = null;
        }

        private static void InvokeAfterClearOuterPackage()
        {
            if (_afterClearOuterPackageFunc != null)
            {
                Delegate[] delegates = _afterClearOuterPackageFunc.GetInvocationList();
                for (int i = 0; i < delegates.Length; i++)
                {
                    try
                    {
                        delegates[i].DynamicInvoke();

                    }
                    catch (Exception e)
                    {
                        UnityEngine.Debug.LogError(e);
                    }
                }
            }
        }


        /// <summary>
        /// 注册安装包包内版本的查询回调，主要用于检测到覆盖安装后删除旧文件以及从包内拷贝文件到包外
        /// </summary>
        /// <param name="action"></param>
        public static void RegistInnerPackageVersionQueryFunc(Func<string> action)
        {
            _innerPackageVersionQueryFunc = action;
        }

        public static void ClearInnerPackageVersionQueryFunc()
        {
            _innerPackageVersionQueryFunc = null;
        }

        /// <summary>
        /// 查询安装包包内版本
        /// </summary>
        /// <returns></returns>
        public static string QueryInnerPackageVersion()
        {
#if !UNITY_EDITOR
            if (_innerPackageVersionQueryFunc != null)
            {
                return _innerPackageVersionQueryFunc();
            }
#endif
            return Application.buildGUID;
        }

        /// <summary>
        /// 删除处理OutPackage内容，
        /// 1.处理用户主动清空缓存文件
        /// 2.覆盖安装新版本时清理上一个版本的旧文件
        /// </summary>
        public static ZeusLaunchErrorFlag ProcessOutPackageContent()
        {
            ZeusLaunchErrorFlag flag = ZeusLaunchErrorFlag.None;
            if (PlayerPrefs.HasKey(PRE_BUILD_GUID))
            {
                if (PlayerPrefs.HasKey(CLEAR_OUTPACKAGE_FLAG))
                {
                    if(!InvokeForceDelete())
                    {
                        flag = flag | ZeusLaunchErrorFlag.CLEAR_OUTPACKAGE_ERROR;
                    }
                    if(!DeleteRedundantFile(null, null, null))
                    {
                        flag = flag | ZeusLaunchErrorFlag.CLEAR_OUTPACKAGE_ERROR;
                    }
                    InvokeAfterClearOuterPackage();
                    if(flag == ZeusLaunchErrorFlag.None)
                    {
                        PlayerPrefs.DeleteKey(CLEAR_OUTPACKAGE_FLAG);
                        PlayerPrefs.DeleteKey(COPY_FILE_ON_FIRST_LAUNCH_FLAG);
                        PlayerPrefs.Save();
                    }
                }
                else
                {
                    string record = PlayerPrefs.GetString(PRE_BUILD_GUID, string.Empty);
                    //检查是否是覆盖安装的情况，覆盖安装需要清理掉上个版本的文件
                    if (!QueryInnerPackageVersion().Equals(record))
                    {
                        if (!InvokeForceDelete())
                        {
                            flag = flag | ZeusLaunchErrorFlag.COVERLY_INSTALLATION_ERROR;
                        }
                        //获取需要保留的文件
                        List<string> retain = GetRetainList();
                        //包内数据（存储的是覆盖安装后的安装包所需文件的校验值）
                        RedundantFileCheckSumInfo inner = RedundantFileCheckSumInfo.LoadInnerInfosFromFB();
                        //包外数据（存储的是覆盖安装前的安装包所需文件的校验值）
                        RedundantFileCheckSumInfo outer = RedundantFileCheckSumInfo.LoadOuterInfosFromFB();
                        //清理包外临时数据（热更解压缩完成之前会存在临时数据）
                        ClearOuterTempFile(outer);
                        //清理除去保留的文件
                        if(!DeleteRedundantFile(new HashSet<string>(retain), inner, outer))
                        {
                            flag = flag | ZeusLaunchErrorFlag.COVERLY_INSTALLATION_ERROR;
                        }
                        PlayerPrefs.SetString(PRE_BUILD_GUID, QueryInnerPackageVersion());
                        PlayerPrefs.Save();
                    }
                }
            }
            else
            {
                PlayerPrefs.SetString(PRE_BUILD_GUID, QueryInnerPackageVersion());
                PlayerPrefs.Save();
            }
            return flag;
        }

        //获取需要保留的文件
        private static List<string> GetRetainList()
        {
            List<string> retain = new List<string>();
            //获取需要保留的文件
            if (_retainCacheFileFunc != null)
            {
                Delegate[] delegates = _retainCacheFileFunc.GetInvocationList();
                for (int i = 0; i < delegates.Length; i++)
                {
                    List<string> temp = (List<string>)delegates[i].DynamicInvoke();
                    if (temp == null || temp.Count == 0)
                    {
                        continue;
                    }
                    for (int j = 0; j < temp.Count; j++)
                    {
                        temp[j] = PathUtil.FormatPathSeparator(temp[j]);
                    }
                    retain.AddRange(temp);
                }
            }
            return retain;
        }

        //清理包外临时数据（热更解压缩完成之前会存在临时数据）
        private static void ClearOuterTempFile(RedundantFileCheckSumInfo outer)
        {
            //包外临时数据（热更解压缩完成之前会存在临时数据）
            RedundantFileCheckSumInfo tempCheckSumInfo = RedundantFileCheckSumInfo.LoadTempInfosFromFB();

            if (tempCheckSumInfo != null && outer != null)
            {
                foreach (var item in tempCheckSumInfo.InfoDic)
                {
                    string outerCheckSum;
                    if (outer.TryGetCheckSum(item.Key, out outerCheckSum))
                    {
                        if (outerCheckSum.Equals(item.Value))
                        {
                            continue;
                        }
                        try
                        {
                            string path = GetRealPath(item.Key);
                            if (File.Exists(path))
                            {
                                File.Delete(path);
                            }
                        }
                        catch (Exception ex)
                        {
                            Debug.LogError(ex);
                        }
                    }
                }
            }
        }

        public static bool DeleteRedundantFile(HashSet<string> retain, RedundantFileCheckSumInfo inner, RedundantFileCheckSumInfo outer)
        {
            bool rtn = true;
            Dictionary<string, string> retainDic = new Dictionary<string, string>();
            //删除冗余文件
            FileSystemSettingConfig config = FileSystemSetting.LoadLocalConfig();
            for (int i = 0; i < config.RetainFileDirectoryList.Count; i++)
            {
                string path = PathUtil.FormatPathSeparator(GetRealPath(config.RetainFileDirectoryList[i]));
                retainDic[path] = PathUtil.FormatPathSeparator(config.RetainFileDirectoryList[i]);
                //UnityEngine.Debug.LogError("Path:" + path + "======================" + config.RetainFileDirectoryList[i]);
            }
            try
            {
                string[] dirList = Directory.GetDirectories(_ROOT);
                for (int i = 0; i < dirList.Length; i++)
                {
                    if (!retainDic.ContainsKey(dirList[i]))
                    {
                        try
                        {
                            Directory.Delete(dirList[i], true);
                        }
                        catch (Exception e)
                        {
                            rtn = false;
                            Debug.LogError(e.ToString());
                        }
                    }
                    else
                    {
                        try
                        {
                            string[] files = Directory.GetFiles(dirList[i], "*", SearchOption.AllDirectories);
                            string runtimeFolder = retainDic[dirList[i]];
                            //UnityEngine.Debug.LogError("runtimeFolder:" + runtimeFolder);
                            for (int j = 0; j < files.Length; j++)
                            {
                                if (retain != null && retain.Contains(files[j]))
                                {
                                    continue;
                                }
                                if (inner != null && outer != null)
                                {
                                    //UnityEngine.Debug.LogError("files[j]:" + files[j]);
                                    string relativePath = files[j].Substring(files[j].IndexOf(runtimeFolder));
                                    relativePath = relativePath.Replace('\\', '/');
                                    //UnityEngine.Debug.LogError("relativePath:" + relativePath);
                                    string innerMD5;
                                    string outerMD5;
                                    if (inner.TryGetCheckSum(relativePath, out innerMD5))
                                    {
                                        if (outer.TryGetCheckSum(relativePath, out outerMD5))
                                        {
                                            if (innerMD5.Equals(outerMD5) && !InnerPackage.ExistsFile(relativePath))
                                            {
                                                continue;
                                            }
                                        }
                                    }
                                }
                                try
                                {
                                    File.Delete(files[j]);
                                }
                                catch (Exception e)
                                {
                                    rtn = false;
                                    Debug.LogError(e.ToString());
                                }
                            }
                        }
                        catch(Exception e)
                        {
                            rtn = false;
                            Debug.LogError(e.ToString());
                        }
                    }
                }
            }
            catch(Exception e)
            {
                rtn = false;
                Debug.LogError(e.ToString());
            }

            try
            {
                string[] fileList = Directory.GetFiles(_ROOT);
                for (int i = 0; i < fileList.Length; i++)
                {
                    if (retain != null && retain.Contains(fileList[i]))
                    {
                        continue;
                    }
                    if (inner != null && outer != null)
                    {
                        //UnityEngine.Debug.LogError("====files[j]:" + fileList[i]);
                        string relativePath = Path.GetFileName(fileList[i]);
                        //UnityEngine.Debug.LogError("====relativePath:" + relativePath);
                        string innerMD5;
                        string outerMD5;
                        if (inner.TryGetCheckSum(relativePath, out innerMD5))
                        {
                            if (outer.TryGetCheckSum(relativePath, out outerMD5))
                            {
                                if (innerMD5.Equals(outerMD5) && !InnerPackage.ExistsFile(relativePath))
                                {
                                    continue;
                                }
                            }
                        }
                    }
                    try
                    {
                        File.Delete(fileList[i]);
                    }
                    catch (Exception e)
                    {
                        rtn = false;
                        Debug.LogError(e.ToString());
                    }
                }
            }
            catch(Exception e)
            {
                rtn = false;
                Debug.LogError(e.ToString());
            }
            return rtn;
        }

        /// <summary>
        /// 强制下一次启动游戏的时候删除冗余文件
        /// </summary>
        public static void ForceDeleteOutPackageFilesOnNextLaunch()
        {
            PlayerPrefs.SetInt(CLEAR_OUTPACKAGE_FLAG, 1);
        }

        /// <summary>
        /// 注册从包内复制到包外的文件，注：文件路径填写【相对路径】！！！
        /// </summary>
        /// <param name="action"></param>
        public static void RegistCopyFileInner2OuterFunc(Func<List<string>> action)
        {
            _copyFileInner2OuterFunc += action;
        }

        public static void UnregistCopyFileInner2OuterFunc(Func<List<string>> action)
        {
            _copyFileInner2OuterFunc -= action;
        }

        public static void ClearCopyFileInner2OuterFunc()
        {
            _copyFileInner2OuterFunc = null;
        }


        ///modify by cmm
        private static WaitForEndOfFrame waitForEndOfFrame = new WaitForEndOfFrame();
        /// <summary>
        /// 从包内复制文件到包外
        /// </summary>
        public static IEnumerator CopyFileInner2Outer()
        {
            string record = PlayerPrefs.GetString(COPY_FILE_ON_FIRST_LAUNCH_FLAG, string.Empty);
            if (!QueryInnerPackageVersion().Equals(record))
            {
                bool suc = true;
                List<string> copyList = new List<string>() { RedundantFileCheckSumInfo.RedundantFileCheckSumInfoFB_RelativePath };//复制列表
                if (_copyFileInner2OuterFunc != null)
                {
                    Delegate[] delegates = _copyFileInner2OuterFunc.GetInvocationList();
                    for (int i = 0; i < delegates.Length; i++)
                    {
                        List<string> temp = (List<string>)delegates[i].DynamicInvoke();
                        if (temp == null || temp.Count == 0)
                        {
                            continue;
                        }
                        for (int j = 0; j < temp.Count; j++)
                        {
                            temp[j] = PathUtil.FormatPathSeparator(temp[j]);
                        }
                        copyList.AddRange(temp);
                    }
                }
                CollectListFromConfig(copyList);
                byte[] buffer = new byte[2048];
                int count = 100;
                foreach (var item in copyList)
                {
                    if (!CopyFromInternal(item, buffer))
                    {
                        suc = false;
                    }
                    XUpdater.XUpdater.singleton.CopyFileInner2OuterProcess(count * 1f / copyList.Count);
                    ++count;
                    if (count % 20 == 0)
                    {
                        yield return waitForEndOfFrame;
                    }
                }
                if (suc)
                {
                    PlayerPrefs.SetString(COPY_FILE_ON_FIRST_LAUNCH_FLAG, QueryInnerPackageVersion());
                }
            }
            Finish = true;
        }

        public static bool Finish = false;
        public static void CollectListFromConfig(List<string> copyList)
        {
            var config = FileSystemSetting.LoadLocalConfig();
            copyList.AddRange(config.InnerToOuterFileList);

            for (var i = 0; i < config.InnerToOuterJsonList.Count; i++)
            {
                var jsonPath = config.InnerToOuterJsonList[i];
                string jsonArr = VFileSystem.ReadAllText(VFileSystem.GetZeusSettingPath(jsonPath), Encoding.UTF8);
                List<string> list = JsonUtilityExtension.FromJsonArray<List<string>>(jsonArr);
                copyList.AddRange(list);
            }
        }
    }
}
