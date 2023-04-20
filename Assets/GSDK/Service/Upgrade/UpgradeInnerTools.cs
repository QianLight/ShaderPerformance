using GMSDK;

namespace GSDK
{
    public class UpgradeInnerTools
    {
        
        public static UpgradeInfo ConvertCheckUpgradeResult(CheckUpgradeResult result)
        {
            UpgradeInfo upgradeInfo = new UpgradeInfo();
            upgradeInfo.NeedUpgrade = result.needUpgrade;
            upgradeInfo.ChannelId = result.channelId;
            upgradeInfo.Url = result.url;
            upgradeInfo.BackupUrl = result.backUrl;
            upgradeInfo.UrlType = (UrlType)result.urlType;
            upgradeInfo.UrlTypeV2 = (UrlTypeV2)result.urlType;
            upgradeInfo.Text = result.text;
            upgradeInfo.MinVersion = result.minVersion;
            upgradeInfo.MaxVersion = result.maxVersion;
            upgradeInfo.TargetVersion = result.targetVersion;
            upgradeInfo.IsForceUpgrade = result.isForceUpgrade;
            upgradeInfo.UpgradeType = (UpgradeType)result.forceUpdateType;
            //v2新增的参数
            upgradeInfo.IsPreload = result.isPreload;
            upgradeInfo.NetStatus = (NetStatus)result.netStatus;
            return upgradeInfo;
        }
#if UNITY_ANDROID        
        public static UpgradeDownloadInfo ConvertToUpgradeDownloadInfo(DownloadUpdateResult result)
        {
            UpgradeDownloadInfo upgradeInfo = new UpgradeDownloadInfo();
            upgradeInfo.Status = (UpgradeDownloadStatus)result.downloadStatus;
            upgradeInfo.CurBytes = result.curBytes;
            upgradeInfo.TotalBytes = result.totalBytes;
            return upgradeInfo;
        }

        public static OwnRuleUpgradeInfo ConvertQueryUpgradeInfoResult(QueryUpgradeInfoResult result)
        {
            OwnRuleUpgradeInfo ownRuleUpgradeInfo = new OwnRuleUpgradeInfo();
            if (result != null)
            {
                ownRuleUpgradeInfo.NeedUpgrade = result.needUpgrade;
                if (result.upgradeInfoWrapper != null)
                {
                    ownRuleUpgradeInfo.UpgradeType = (OwnRuleUpgradeType)result.upgradeInfoWrapper.upgradeType;
                    ownRuleUpgradeInfo.EffectiveMode = (OwnRuleEffectiveMode)result.upgradeInfoWrapper.effectiveMode;
                    ownRuleUpgradeInfo.CustomRule = result.upgradeInfoWrapper.customRule;
                    ownRuleUpgradeInfo.ShowPreDownloadNotifyDialog = result.upgradeInfoWrapper.showPreDownloadNotifyDialog;
                    ownRuleUpgradeInfo.NeedUploadEntryInfo = result.upgradeInfoWrapper.needUploadEntryInfo;
                    if (result.upgradeInfoWrapper.patchInfoWrapper != null)
                    {
                        OwnRulePatchInfo ownRulePatchInfo = new OwnRulePatchInfo();
                        ownRulePatchInfo.OwnRulePatchType = (OwnRulePatchType)result.upgradeInfoWrapper.patchInfoWrapper.patchType;
                        if (result.upgradeInfoWrapper.patchInfoWrapper.fileInfoWrapper != null)
                        {
                            OwnRuleFileInfo patchOwnRuleFileInfo = new OwnRuleFileInfo();
                            patchOwnRuleFileInfo.Md5 = result.upgradeInfoWrapper.patchInfoWrapper.fileInfoWrapper.md5;
                            patchOwnRuleFileInfo.Crc32 = result.upgradeInfoWrapper.patchInfoWrapper.fileInfoWrapper.crc32;
                            patchOwnRuleFileInfo.DownloadUrl = result.upgradeInfoWrapper.patchInfoWrapper.fileInfoWrapper.downloadUrl;
                            patchOwnRuleFileInfo.BackupDownloadUrl = result.upgradeInfoWrapper.patchInfoWrapper.fileInfoWrapper.backupDownloadUrl;
                            patchOwnRuleFileInfo.FileLength = result.upgradeInfoWrapper.patchInfoWrapper.fileInfoWrapper.fileLength;
                            ownRulePatchInfo.OwnRuleFileInfo = patchOwnRuleFileInfo;
                        }
                        ownRulePatchInfo.PackageName = result.upgradeInfoWrapper.patchInfoWrapper.packageName;
                        ownRulePatchInfo.BaseApkVersionCode = result.upgradeInfoWrapper.patchInfoWrapper.baseApkVersionCode;
                        ownRulePatchInfo.BaseApkVersionName = result.upgradeInfoWrapper.patchInfoWrapper.baseApkVersionName;
                        ownRulePatchInfo.BaseApkIdentity = result.upgradeInfoWrapper.patchInfoWrapper.baseApkIdentity;
                        ownRulePatchInfo.NewApkVersionCode = result.upgradeInfoWrapper.patchInfoWrapper.newApkVersionCode;
                        ownRulePatchInfo.NewApkVersionName = result.upgradeInfoWrapper.patchInfoWrapper.newApkVersionName;
                        ownRulePatchInfo.NewApkIdentity = result.upgradeInfoWrapper.patchInfoWrapper.newApkIdentity;
                        ownRuleUpgradeInfo.OwnRulePatchInfo = ownRulePatchInfo;
                    }

                    if (result.upgradeInfoWrapper.apkInfoWrapper != null)
                    {
                        OwnRuleApkInfo ownRuleApkInfo = new OwnRuleApkInfo();
                        ownRuleApkInfo.PackageName = result.upgradeInfoWrapper.apkInfoWrapper.packageName;
                        ownRuleApkInfo.VersionCode = result.upgradeInfoWrapper.apkInfoWrapper.versionCode;
                        ownRuleApkInfo.VersionName = result.upgradeInfoWrapper.apkInfoWrapper.versionName;
                        ownRuleApkInfo.ApkIdentity = result.upgradeInfoWrapper.apkInfoWrapper.apkIdentity;
                        ownRuleUpgradeInfo.OwnRuleApkInfo = ownRuleApkInfo;
                        if (result.upgradeInfoWrapper.apkInfoWrapper.fileInfoWrapper != null)
                        {
                            OwnRuleFileInfo apkOwnRuleFileInfo = new OwnRuleFileInfo();
                            apkOwnRuleFileInfo.Md5 = result.upgradeInfoWrapper.apkInfoWrapper.fileInfoWrapper.md5;
                            apkOwnRuleFileInfo.Crc32 = result.upgradeInfoWrapper.apkInfoWrapper.fileInfoWrapper.crc32;
                            apkOwnRuleFileInfo.DownloadUrl = result.upgradeInfoWrapper.apkInfoWrapper.fileInfoWrapper.downloadUrl;
                            apkOwnRuleFileInfo.BackupDownloadUrl = result.upgradeInfoWrapper.apkInfoWrapper.fileInfoWrapper.backupDownloadUrl;
                            apkOwnRuleFileInfo.FileLength = result.upgradeInfoWrapper.apkInfoWrapper.fileInfoWrapper.fileLength;
                            ownRuleApkInfo.OwnRuleFileInfo = apkOwnRuleFileInfo;
                        }
                    }
                }
            }
            
            return ownRuleUpgradeInfo;
        }

        public static StartUpgradeInfo ConvertStartUpgradeInfoResult(StartUpgradeInfoResult startUpgradeInfoResult)
        {
            StartUpgradeInfo startUpgradeInfo = new StartUpgradeInfo();
            startUpgradeInfo.LifeCycle = (OwnRuleLifeCycle)startUpgradeInfoResult.lifeCycle;
            startUpgradeInfo.IsWifi = startUpgradeInfoResult.isWifi;
            startUpgradeInfo.DownloadLengthInBytes = startUpgradeInfoResult.downloadLengthInBytes;
            startUpgradeInfo.NewApkVersionCode = startUpgradeInfoResult.newApkVersionCode;
            startUpgradeInfo.ApkPath = startUpgradeInfoResult.apkPath;
            startUpgradeInfo.DownloadedFilePath = startUpgradeInfoResult.downloadedFilePath;
            startUpgradeInfo.NewApkPath = startUpgradeInfoResult.newApkPath;

            return startUpgradeInfo;
        }

        public static ContinueExecutionInfo ConvertContinueExecutionResult(ContinueExecutionResult continueExecutionResult)
        {
            ContinueExecutionInfo continueExecutionInfo = new ContinueExecutionInfo();
            continueExecutionInfo.Status = (OwnRuleStatus)continueExecutionResult.status;
            continueExecutionInfo.EventType = continueExecutionResult.eventType;
            continueExecutionInfo.InitialProgress = continueExecutionResult.initialProgress;
            continueExecutionInfo.Progress = continueExecutionResult.progress;
            continueExecutionInfo.EndCode = continueExecutionResult.endCode;
            continueExecutionInfo.EndMessage = continueExecutionResult.endMessage;

            return continueExecutionInfo;
        }
#endif
    }
}