using GMSDK;
namespace GSDK
{
    public class UpgradeService : IUpgradeService
    {
        public void CheckForceUpgrade(CheckForceUpgradeDelegate checkForceUpgradeDelegate, bool withUI = true)
        {
            GMUpgradeMgr.instance.SDK.CheckForceUpgrade((CheckUpgradeResult result) =>
            {
                if (checkForceUpgradeDelegate != null)
                {
                    checkForceUpgradeDelegate(InnerTools.ConvertToResult(result), UpgradeInnerTools.ConvertCheckUpgradeResult(result));
                }
            }, withUI);
        }

        public void CheckForceUpgradeV2(CheckForceUpgradeDelegate checkForceUpgradeDelegate, bool withUI = true)
        {
            GMUpgradeMgr.instance.SDK.CheckForceUpgradeV2((CheckUpgradeResult result) =>
            {
                if (checkForceUpgradeDelegate != null)
                {
                    checkForceUpgradeDelegate(InnerTools.ConvertToResult(result), UpgradeInnerTools.ConvertCheckUpgradeResult(result));
                }
            }, withUI);
        }
#if UNITY_ANDROID 
        public void StartCustomUpgrade(DownloadUpdateDelegate callBack)
        {
            GMUpgradeMgr.instance.SDK.StartCustomUpgrade((result =>
            {
                if (callBack != null)
                {
                    callBack(InnerTools.ConvertToResult(result), UpgradeInnerTools.ConvertToUpgradeDownloadInfo(result));
                }
            }));
        }

        public void StartCustomUpgradeV2(DownloadUpdateDelegate callBack)
        {
            GMUpgradeMgr.instance.SDK.StartCustomUpgradeV2((result =>
            {
                if (callBack != null)
                {
                    callBack(InnerTools.ConvertToResult(result), UpgradeInnerTools.ConvertToUpgradeDownloadInfo(result));
                }
            }));
        }

        public void CancelCustomUpgrade()
        {
            GMUpgradeMgr.instance.SDK.CancelCustomUpgrade();
        }

        public void QueryUpgradeInfoForOwnRule(QueryUpgradeInfoDelegate queryUpgradeInfoDelegate)
        {
            GMUpgradeMgr.instance.SDK.QueryUpgradeInfoForOwnRule((QueryUpgradeInfoResult result) =>
            {
                if (queryUpgradeInfoDelegate != null)
                {
                    queryUpgradeInfoDelegate(InnerTools.ConvertToResult(result), UpgradeInnerTools.ConvertQueryUpgradeInfoResult(result));
                }
            });
        }

        public void StartUpgradeForOwnRule(OwnRuleUpgradeInfo ownRuleUpgradeInfo, StartUpgradeDelegate startUpgradeDelegate)
        {
            GMUpgradeMgr.instance.SDK.StartUpgradeForOwnRule(ownRuleUpgradeInfo, (StartUpgradeInfoResult result) =>
            {
                if (startUpgradeDelegate != null)
                {
                    startUpgradeDelegate(InnerTools.ConvertToResult(result), UpgradeInnerTools.ConvertStartUpgradeInfoResult(result));
                }
            });
        }

        public void ContinueExecution(OwnRuleLifeCycle ownRuleLifeCycle, ContinueExecutionDelegate continueExecutionDelegate)
        {
            GMUpgradeMgr.instance.SDK.ContinueExecution(ownRuleLifeCycle, (ContinueExecutionResult result) =>
            {
                if (continueExecutionDelegate != null)
                {
                    continueExecutionDelegate(InnerTools.ConvertToResult(result), UpgradeInnerTools.ConvertContinueExecutionResult(result));
                }
            });
        }

        public void CancelExecution()
        {
            GMUpgradeMgr.instance.SDK.CancelExecution();
        }

        public void RestartApp()
        {
            GMUpgradeMgr.instance.SDK.RestartApp();
        }

        public void OverwriteInstallApk(string apkPath)
        {
            GMUpgradeMgr.instance.SDK.OverwriteInstallApk(apkPath);
        }
#endif        
    }
}