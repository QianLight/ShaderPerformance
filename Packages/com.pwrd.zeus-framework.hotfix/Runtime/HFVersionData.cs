/*******************************************************************
* Copyright © 2017—2021 Perfect World.Co.Ltd. All rights reserved.
* author: 移动项目支持部 ZEUS FRAMEWORK TEAM
********************************************************************/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using Zeus.Core;
using System;
using Zeus.Core.FileSystem;

namespace Zeus.Framework.Hotfix
{
    public enum HotFixType
    {
        //不做更新
        None,
        //商店更新
        AppStore,
        //强制更新
        Force,
        //推荐更新，非强制
        Recommend,
        //商店推荐更新（可以跳过的商店更新）
        AppStoreRecommend,
    }

    [System.Serializable]
    public class HFControlData
    {
        //渠道ID
        public string Channel;
        //版本号
        public string Version;
        //更新数据
        [SerializeField]
        public HFPatchConfigData PatchConfigData;
        //商店推荐更新数据
        [SerializeField]
        public HFPatchConfigData AppRecommendPatchConfigData;
        //热更测试数据
        [SerializeField]
        public HFTestingData TestingData;
    }

    [System.Serializable]
    public class HFPatchConfigData
    {
        //更新类型
        public HotFixType Type;
        //热更开始版本号
        public string SourceVersion;
        //热更目标版本号
        public string TargetVersion;
        //定期开启时间,为空标识立即开始
        public string OpenTime;
        //商店URL
        public string AppStoreUrl;
        //包名对应商店地址，示例：[{\"appId\":\"com.wanmei.zhuxian\",\"url\":\"http://apple.com.cn\"},{\"appId\":\"com.wanmei.zhanshen\",\"url\":\"http://googleplay.com.cn\"}]
        public string SubChannelAppStoreUrl;
        //patch文件md5值
        public string PatchMd5;
        //patch文件下载路径，需要拼接URL获取最终地址
        public string PatchPath;
        //热更文件大小(字节)
        public long PatchSize;
        //热更文件解压大小
        public long PatchContentSize;

        //首次进入游戏的patch文件md5值
        public string FpPatchMd5;
        //首次进入游戏的patch文件下载路径，需要拼接URL获取最终地址
        public string FpPatchPath;
        //热更文件大小(字节)
        public long FpPatchSize;
        //热更文件解压大小
        public long FpPatchContentSize;

        public string[] GetUrls(string[] ServerUrls)
        {
            List<string> list = new List<string>();
            for (int i = 0; i < ServerUrls.Length; i++)
            {
                string url = ServerUrls[i];
                list.Add(Zeus.Core.UriUtil.CombineUri(url, GetPatchPath()));
            }
            return list.ToArray();
        }

        public string GetPatchSavePath()
        {
            string relativePath = string.Format("{0}/{1}/{2}_{3}{4}",
                HotfixService.HotFixTempFolder, SourceVersion, TargetVersion, GetPatchMd5(), Path.GetExtension(GetPatchPath()));
            return Zeus.Core.FileSystem.OuterPackage.GetRealPath(relativePath);
        }

        public string GetPatchUnzipPath()
        {
            return Zeus.Core.FileSystem.OuterPackage.GetRealPath(string.Empty);
        }

        public void DeletPatchFile()
        {
            string patchFile = GetPatchSavePath();
            string directoryPath = System.IO.Path.GetDirectoryName(patchFile);
            if (System.IO.Directory.Exists(directoryPath))
            {
                System.IO.Directory.Delete(directoryPath, true);
            }
        }

        [System.Serializable]
        private class AppID2AppStoreUrl
        {
            public string appId;
            public string url;
        }

        /// <summary>
        /// 当更新类型为商店更新，先在SubChannelAppStoreUrl内找商店地址，找不到再用AppStoreUrl
        /// </summary>
        /// <returns></returns>
        public string GetAppStoreUrl()
        {
            if (!string.IsNullOrEmpty(SubChannelAppStoreUrl))
            {
                AppID2AppStoreUrl[] array = null;
                try
                {
                    array = JsonUtilityExtension.FromJsonArray<AppID2AppStoreUrl[]>(SubChannelAppStoreUrl);
                }
                catch (System.Exception e)
                {
                    UnityEngine.Debug.LogError("[HotFix] GetAppStoreUrl Fail:" + e.ToString());
                }
                if (array != null && array.Length > 0)
                {
                    string appid = Application.identifier;
                    for (int i = 0; i < array.Length; i++)
                    {
                        if (!string.IsNullOrEmpty(array[i].appId) && appid.Equals(array[i].appId))
                        {
                            if (!string.IsNullOrEmpty(array[i].url))
                            {
                                return array[i].url;
                            }
                        }
                    }
                }
            }
            return AppStoreUrl;
        }

        private bool UseFpPatchData()
        {
            return !HotfixService.hasLaunchedGame && FpPatchSize > 0 && !string.IsNullOrEmpty(FpPatchPath);
        }

        //首次进入游戏的patch文件md5值
        public string GetPatchMd5()
        {
            if (!UseFpPatchData())
            {
                return PatchMd5;
            }
            else
            {
                return FpPatchMd5;
            }
        }
        //首次进入游戏的patch文件下载路径，需要拼接URL获取最终地址
        public string GetPatchPath()
        {
            if (!UseFpPatchData())
            {
                return PatchPath;
            }
            else
            {
                return FpPatchPath;
            }
        }
        //热更文件大小(字节)
        public long GetPatchSize()
        {
            if (!UseFpPatchData())
            {
                return PatchSize;
            }
            else
            {
                return FpPatchSize;
            }
        }
        //热更文件解压大小
        public long GetPatchContentSize()
        {
            if (!UseFpPatchData())
            {
                return PatchContentSize;
            }
            else
            {
                return FpPatchContentSize;
            }
        }
    }

    [System.Serializable]
    public class HFTestingData
    {
        public List<string> IDFAList;
        [SerializeField]
        public HFPatchConfigData PatchConfigData;
    }


    [System.Serializable]
    public class ResVersionData
    {
#if UNITY_EDITOR
        private static bool isOuterPackageInit = false;
        private static bool saveOuterPackageVersionInFile = true;
        private static bool encodeOuterPackageVersion = false;
        private static string innerVersion = string.Empty;
#else
        private static bool saveOuterPackageVersionInFile = false;
        private static bool encodeOuterPackageVersion = true;
        private static string innerVersion = string.Empty;
#endif

        public string ResVersion;
        public string BuildGUID;

        public static void InitHotfixLocalConfigSettingValue(HotfixLocalConfig config = null)
        {
            if (config == null)
            {
                config = HotfixLocalConfigHelper.LoadLocalConfig();
            }
            saveOuterPackageVersionInFile = config.saveOuterPackageVersionInFile;
            encodeOuterPackageVersion = config.encodeOuterPackageVersion;
            innerVersion = config.Version;
        }

        #region 继续上次未完的解压
        [System.Serializable]
        public class UnzipTask
        {
            public string ResVersion;
            public string PatchPath;//绝对路径
            public string UnzipPath;

            public UnzipTask(string resVersion, string patchPath, string unzipPath)
            {
                this.ResVersion = resVersion;
                this.PatchPath = patchPath;
                this.UnzipPath = unzipPath;
            }
        }
        public List<UnzipTask> UnfinishedUnzipTasklist = new List<UnzipTask>();

        public void RecordUnfinishedUnzipTask(string ResVersion, string PatchPath, string UnzipPath)
        {
            UnfinishedUnzipTasklist.Add(new UnzipTask(ResVersion, PatchPath, UnzipPath));
            Save();
        }

        public void FinishHotfix(UnzipTask task)
        {
            if (UnfinishedUnzipTasklist.Remove(task))
            {
                ResVersion = task.ResVersion;
                Save();
            }
        }

        public void FinishHotfix(string PatchPath)
        {
            FinishHotfix(UnfinishedUnzipTasklist.Find(t => t.PatchPath.Equals(PatchPath)));
        }
        #endregion

        #region save/load
        private static void CheckOuterPackageInit()
        {
#if UNITY_EDITOR
            if (!isOuterPackageInit)
            {
                typeof(OuterPackage).GetMethod("Init", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static).Invoke(null, null);
                isOuterPackageInit = true;
            }
#endif
        }

        /// <summary>
        /// 初始化版本数据，数据不存在则使用app内置版本
        /// 返回的结果有可能是null
        /// </summary>
        /// <returns></returns>
        public static ResVersionData Load()
        {
            ResVersionData resVersion = null;
            string json = null;
            string encryptJson = null;

            if (saveOuterPackageVersionInFile)
            {
                CheckOuterPackageInit();
                string rootFolder = OuterPackage.GetRealPath(HotfixService.HotFixTempFolder);
                string verFilePath = rootFolder + "/" + HotfixService.LocalVerDataFile;
                if (File.Exists(verFilePath))
                {
                    try
                    {
                        encryptJson = FileUtil.LoadFileText(verFilePath);
                    }
                    catch (Exception e)
                    {
                        encryptJson = null;
                        UnityEngine.Debug.LogError($"Load encryptJson exception:{e.ToString()}");
                    }
                }
                if (!string.IsNullOrEmpty(encryptJson))
                {
                    if (encodeOuterPackageVersion)
                    {
                        try
                        {
                            json = EncryptUtil.AesDecrypt(encryptJson, HotfixService.LocalVerDataFile);
                        }
                        catch (System.Exception e)
                        {
                            json = null;
                            FileUtil.DeleteFile(verFilePath);
                            UnityEngine.Debug.LogError($"encryptJson:{encryptJson} exception:{e.ToString()}");
                        }
                    }
                    else
                    {
                        json = encryptJson;
                    }
                }
            }
            else
            {
                encryptJson = PlayerPrefs.GetString(HotfixService.LocalVerDataKey, string.Empty);
                if (!string.IsNullOrEmpty(encryptJson))
                {
                    if (encodeOuterPackageVersion)
                    {
                        try
                        {
                            json = EncryptUtil.AesDecrypt(encryptJson, HotfixService.LocalVerDataKey);
                        }
                        catch (System.Exception e)
                        {
                            json = null;
                            PlayerPrefs.DeleteKey(HotfixService.LocalVerDataKey);
                            UnityEngine.Debug.LogError($"encryptJson:{encryptJson} exception:{e.ToString()}");
                        }
                    }
                    else
                    {
                        json = encryptJson;
                    }
                }
            }


            if (string.IsNullOrEmpty(json))
            {
                resVersion = InitData();
            }
            else
            {
#if UNITY_EDITOR
                UnityEngine.Debug.Log("Load ResVersionData:" + json);
#endif
                bool needInit = false;
                try
                {
                    resVersion = JsonUtility.FromJson<ResVersionData>(json);
                    needInit = !resVersion.BuildGUID.Equals(OuterPackage.QueryInnerPackageVersion());
                }
                catch (System.Exception e)
                {
                    needInit = true;
                }
                //如果buildGUID不等，说明覆盖安装了，需要从包内读取版本号数据
                if (needInit)
                {
                    resVersion = InitData();
                }
            }
            return resVersion;
        }

        private static ResVersionData InitData()
        {
            ResVersionData resVersion = new ResVersionData();
            resVersion.ResVersion = innerVersion;
            resVersion.BuildGUID = OuterPackage.QueryInnerPackageVersion();
            if (resVersion.Save())
            {
                return resVersion;
            }
            else
            {
                return null;
            }
        }

        public bool Save()
        {
            string json = JsonUtility.ToJson(this);
#if UNITY_EDITOR
            UnityEngine.Debug.Log("Save ResVersionData:" + json);
#endif
            if (saveOuterPackageVersionInFile)
            {
                CheckOuterPackageInit();
                string rootFolder = OuterPackage.GetRealPath(HotfixService.HotFixTempFolder);
                string verFilePath = rootFolder + "/" + HotfixService.LocalVerDataFile;
#if UNITY_EDITOR
                UnityEngine.Debug.Log("verFilePath:" + verFilePath);
#endif
                FileUtil.DeleteFile(verFilePath);

                var succeed = FileUtil.EnsureFolder(verFilePath);
                if(!succeed)
                {
                    Debug.LogErrorFormat("EnsureFolder {0} failed", verFilePath);
                    return false;
                }
                if (encodeOuterPackageVersion)
                {
                    FileUtil.SaveFileText(EncryptUtil.AesEncrypt(json, HotfixService.LocalVerDataFile), verFilePath);
                }
                else
                {
                    FileUtil.SaveFileText(json, verFilePath);
                }
            }
            else
            {
                if (encodeOuterPackageVersion)
                {
                    PlayerPrefs.SetString(HotfixService.LocalVerDataKey, EncryptUtil.AesEncrypt(json, HotfixService.LocalVerDataKey));
                }
                else
                {
                    PlayerPrefs.SetString(HotfixService.LocalVerDataKey, json);
                }
                PlayerPrefs.Save();
            }
            return true;
        }

        public static void Delete()
        {
#if UNITY_EDITOR
            UnityEngine.Debug.Log("Delete ResVersionData");
#endif
            if (saveOuterPackageVersionInFile)
            {
                CheckOuterPackageInit();
                string rootFolder = OuterPackage.GetRealPath(HotfixService.HotFixTempFolder);
                string verFilePath = rootFolder + "/" + HotfixService.LocalVerDataFile;
#if UNITY_EDITOR
                UnityEngine.Debug.Log("verFilePath:" + verFilePath);
#endif
                FileUtil.DeleteFile(verFilePath);
            }
            else
            {
                if (PlayerPrefs.HasKey(HotfixService.LocalVerDataKey))
                {
                    PlayerPrefs.DeleteKey(HotfixService.LocalVerDataKey);
                    PlayerPrefs.Save();
                }
            }

            HotfixService.ResetHasLaunchedGame();
        }
        #endregion
    }
}
