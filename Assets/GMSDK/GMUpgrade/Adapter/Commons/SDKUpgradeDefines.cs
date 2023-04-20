using System;
using UnityEngine;
using System.Collections.Generic;

namespace GMSDK
{
    public class UpgradeMethodName
    {
        public const string Init = "registerUpgrade";
        public const string CheckUpgrade = "checkUpgrade";
        public const string CheckUpgradeV2 = "checkUpgradeV2";
        public const string StartCustomUpgrade = "startCustomUpgrade";
        public const string StartCustomUpgradeV2 = "startCustomUpgradeV2";
        public const string CancelCustomUpgrade = "cancelCustomUpgrade";
        public const string QueryUpgradeInfoForOwnRule = "queryUpgradeInfoForOwnRule";
        public const string StartUpgradeForOwnRule = "startUpgradeForOwnRule";
        public const string ContinueExecution = "continueExecution";
        public const string CancelExecution = "cancelExecution";
        public const string RestartApp = "restartApp";
        public const string OverwriteInstallApk = "overwriteInstallApk";
    }

	public class CheckUpgradeResult : CallbackResult
    {
        public bool needUpgrade;
        public string channelId;
        public string url;
        public string backUrl;
        public int urlType;
        public string text;
        public string minVersion;
        public string maxVersion;
        public string targetVersion;
        public bool isForceUpgrade;
        public int forceUpdateType;
        public bool isPreload;
        public int netStatus;
    }

    public class DownloadUpdateResult : CallbackResult
    {
        public int downloadStatus;
        public long curBytes;
        public long totalBytes;
    }

    public class QueryUpgradeInfoResult : CallbackResult
    {
        public bool needUpgrade;
        public UpgradeInfoWrapper upgradeInfoWrapper;
    }

    public class UpgradeInfoWrapper
    {
        public PatchInfoWrapper patchInfoWrapper;
        public ApkInfoWrapper apkInfoWrapper;
        public int upgradeType;
        public int effectiveMode;
        public string customRule;
        public bool showPreDownloadNotifyDialog;
        public bool needUploadEntryInfo;
    }

    public class PatchInfoWrapper
    {
        public int patchType;
        public FileInfoWrapper fileInfoWrapper;
        public string packageName;
        public int baseApkVersionCode;
        public string baseApkVersionName;
        public string baseApkIdentity;
        public int newApkVersionCode;
        public string newApkVersionName;
        public string newApkIdentity;
    }

    public class ApkInfoWrapper
    {
        public FileInfoWrapper fileInfoWrapper;
        public string packageName;
        public int versionCode;
        public string versionName;
        public string apkIdentity;
    }

    public class FileInfoWrapper
    {
        public string md5;
        public string crc32;
        public string downloadUrl;
        public List<string> backupDownloadUrl;
        public long fileLength;
    }

    public class StartUpgradeInfoResult : CallbackResult
    {
        public int lifeCycle;
        public bool isWifi;
        public long downloadLengthInBytes;
        public int newApkVersionCode;
        public string apkPath;
        public string downloadedFilePath;
        public string newApkPath;
    }

    public class ContinueExecutionResult : CallbackResult
    {
        public int status;
        public int eventType;
        public float initialProgress;
        public float progress;
        public int endCode;
        public string endMessage;
    }
}