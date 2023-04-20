using System.Collections.Generic;
using GMSDK;

namespace GSDK
{
    /// <summary>
    /// 检测更新回调
    /// </summary>
    /// <param name="result"></param>
    ///  /// <para>
    ///    当前接口可能返回的错误码:
    ///             Success:成功
    ///             UpgradeNoNeedUpgrade, 无需更新
    ///             UpgradeRequestFailed, 请求失败
    /// <param name="upgradeInfo"></param>
    public delegate void CheckForceUpgradeDelegate(Result result, UpgradeInfo upgradeInfo);

#if UNITY_ANDROID    
    /// <summary>
    ///  自定义更新接口回调，仅Android
    /// </summary>
    /// <param name="result"></param>
    /// <param name="upradeDownloadInfo"></param>
    public delegate void DownloadUpdateDelegate(Result result, UpgradeDownloadInfo upradeDownloadInfo);

    /// <summary>
    /// 自定义规则查询信息接口回调
    /// </summary>
    /// <param name="result"></param>
    /// <param name="ownRuleUpgradeInfo">更新信息</param>
    public delegate void QueryUpgradeInfoDelegate(Result result, OwnRuleUpgradeInfo ownRuleUpgradeInfo);
    /// <summary>
    /// 自定义规则开始更新接口回调
    /// </summary>
    /// <param name="result"></param>
    /// <param name="startUpgradeInfo"></param>
    public delegate void StartUpgradeDelegate(Result result, StartUpgradeInfo startUpgradeInfo);
    /// <summary>
    /// 自定义规则继续更新流程接口回调
    /// </summary>
    /// <param name="result"></param>
    /// <param name="continueExecutionInfo"></param>
    public delegate void ContinueExecutionDelegate(Result result, ContinueExecutionInfo continueExecutionInfo);
#endif    
    /// <summary>
    /// 用于获取实例进行接口调用
    /// e.g.Upgrade.Service.MethodName();
    /// </summary>
    public static class Upgrade
    {
        public static IUpgradeService Service
        {
            get
            {
                return ServiceProvider.Instance.GetService(ServiceType.Upgrade) as IUpgradeService;
            }
        }
    }
    public interface IUpgradeService : IService
    {
        /// <summary>
        /// 检测更新
        /// </summary>
        /// <param name="checkForceUpgradeDelegate">检测结果回调</param>
        /// <param name="withUI">是否弹出UI，false仅返回检查信息</param>    
        void CheckForceUpgrade(CheckForceUpgradeDelegate checkForceUpgradeDelegate, bool withUI = true);
        /// <summary>
        /// 检测更新V2
        /// </summary>
        /// <param name="checkForceUpgradeDelegate">检测结果回调</param>
        /// <param name="withUI">是否弹出UI，false仅返回检查信息</param>
        void CheckForceUpgradeV2(CheckForceUpgradeDelegate checkForceUpgradeDelegate, bool withUI = true);
#if UNITY_ANDROID        
        /// <summary>
        /// 开始自定义更新，withUI为false的时候用户确认的时候调用
        /// </summary>
        /// <param name="callBack"></param>
        void StartCustomUpgrade(DownloadUpdateDelegate callBack);
        /// <summary>
        /// 开始自定义更新V2，withUI为false的时候用户确认的时候调用
        /// </summary>
        /// <param name="callBack"></param>
        void StartCustomUpgradeV2(DownloadUpdateDelegate callBack);
        /// <summary>
        /// 取消自定义更新
        /// </summary>
        void CancelCustomUpgrade();

        /// <summary>
        /// 自定义规则查询接口
        /// </summary>
        /// <param name="queryUpgradeInfoDelegate">结果回调</param>
        void QueryUpgradeInfoForOwnRule(QueryUpgradeInfoDelegate queryUpgradeInfoDelegate);
        /// <summary>
        /// 自定义规则开始更新接口
        /// </summary>
        /// <param name="ownRuleUpgradeInfo">更新信息</param>
        /// <param name="startUpgradeDelegate">结果回调</param>
        void StartUpgradeForOwnRule(OwnRuleUpgradeInfo ownRuleUpgradeInfo, StartUpgradeDelegate startUpgradeDelegate);

        /// <summary>
        /// 自定义规则继续更新流程
        /// </summary>
        /// <param name="ownRuleLifeCycle">当前所处的生命周期</param>
        /// <param name="continueExecutionDelegate">结果回调</param>
        void ContinueExecution(OwnRuleLifeCycle ownRuleLifeCycle, ContinueExecutionDelegate continueExecutionDelegate);
        /// <summary>
        /// 自定义规则取消更新流程
        /// </summary>
        void CancelExecution();
        /// <summary>
        /// 自定义规则重启App
        /// </summary>
        void RestartApp();
        /// <summary>
        /// 自定义规则触发覆盖安装
        /// </summary>
        /// <param name="apkPath">apk路径</param>
        void OverwriteInstallApk(string apkPath);
#endif
    }
    public enum UrlType
    {
        /// <summary>
        /// H5链接，用于跳转到应用商店
        /// </summary>
        H5Link = 1, 
        /// <summary>
        /// APK链接，用于下载APK
        /// </summary>
        APKLink = 2 
    }
    
    public enum UrlTypeV2
    {
        /// <summary>
        /// APK链接，用于下载APK
        /// </summary>
        ApkLink = 1, 
        /// <summary>
        /// scheme 跳转应用商店
        /// </summary>
        SchemeLink = 2,
        /// <summary>
        /// H5链接，用于跳转到应用商店
        /// </summary>
        H5Link = 3
    }
    
    public enum UpgradeType
    {
        /// <summary>
        /// 全版本更新,低于targetVersion都更新
        /// </summary>
        All = 0,
        /// <summary>
        /// 指定版本更新minVersion和maxVersion之间更新
        /// </summary>
        BetweenMinVersionAndMaxVersion = 1 
        
    }
    public enum NetStatus
    {
        /// <summary>
        /// 无网络
        /// </summary>
        NoNetwork = -1,
        /// <summary>
        /// Wifi状态
        /// </summary>
        Wifi = 1,
        /// <summary>
        /// 移动网络
        /// </summary>
        MobileNetwork = 2
    }
    public class UpgradeInfo
    {
        /// <summary>
        /// 是否需要更新
        /// </summary>
        public bool NeedUpgrade;
        /// <summary>
        /// 渠道ID
        /// </summary>
        public string ChannelId;
        /// <summary>
        /// 主地址
        /// </summary>
        public string Url;
        /// <summary>
        /// 备用地址（平台配置为APK地址类型时有效）
        /// </summary>
        public string BackupUrl;
        /// <summary>
        /// 地址类型
        /// </summary>
        public UrlType UrlType;
        /// <summary>
        /// V2的地址类型
        /// </summary>
        public UrlTypeV2 UrlTypeV2;
        /// <summary>
        /// 更新说明
        /// </summary>
        public string Text;
        /// <summary>
        /// 最小版本号（区间更新时有效）
        /// </summary>
        public string MinVersion;
        /// <summary>
        /// 最大版本号（区间更新时有效）
        /// </summary>
        public string MaxVersion;
        /// <summary>
        /// 目标版本（全版本更新时有效）
        /// </summary>
        public string TargetVersion;
        /// <summary>
        /// 是否强制更新
        /// </summary>
        public bool IsForceUpgrade;
        /// <summary>
        /// 更新类型
        /// </summary>
        public UpgradeType UpgradeType;
        /// <summary>
        /// 是否在预下载模式
        /// </summary>
        public bool IsPreload;
        /// <summary>
        /// 当前网络状态
        /// </summary>
        public NetStatus NetStatus;
    }

    public enum UpgradeDownloadStatus
    {
        /// <summary>
        /// 开始下载
        /// </summary>
        StartDonwload = 1,
        /// <summary>
        /// 下载中
        /// </summary>
        Downloading = 2,
        /// <summary>
        /// 下载成功
        /// </summary>
        DownloadSuccess = 3,
        /// <summary>
        /// 下载失败
        /// </summary>
        DownloadFailed = 4,
        /// <summary>
        /// 磁盘空间不足
        /// </summary>
        InsufficientSpace = 5,
        /// <summary>
        /// 开始差分合成
        /// </summary>
        StartPatch = 6,
        /// <summary>
        /// 差分合成中
        /// </summary>
        Patching = 7,
        /// <summary>
        /// 差分合成成功
        /// </summary>
        PatchSuccess = 8,
        /// <summary>
        /// 差分合成失败
        /// </summary>
        PatchFailed = 9,
        /// <summary>
        /// 开始热更安装
        /// </summary>
        StartHotInstall = 10,
        /// <summary>
        /// 热更安装中
        /// </summary>
        HotInstalling = 11,
        /// <summary>
        /// 热更安装成功
        /// </summary>
        HotInstallSuccess = 12,
        /// <summary>
        /// 热更安装失败
        /// </summary>
        HotInstallFailed = 13,
        /// <summary>
        /// 可以开始覆盖安装了
        /// </summary>
        OverwriteSuccess = 14,
        /// <summary>
        /// 其他可能出现的错误
        /// </summary>
        OtherFailed = 15
    }
#if UNITY_ANDROID  
    public class UpgradeDownloadInfo
    {
        /// <summary>
        /// 下载状态
        /// </summary>
        public UpgradeDownloadStatus Status;
        /// <summary>
        /// 下载完成字节
        /// </summary>
        public long CurBytes;
        /// <summary>
        /// 总字节
        /// </summary>
        public long TotalBytes;

    }
    
    public class OwnRuleUpgradeInfo
    {
        /// <summary>
        /// 是否需要更新
        /// </summary>
        public bool NeedUpgrade;
        /// <summary>
        /// Patch信息
        /// </summary>
        public OwnRulePatchInfo OwnRulePatchInfo;
        /// <summary>
        /// Apk信息
        /// </summary>
        public OwnRuleApkInfo OwnRuleApkInfo;
        /// <summary>
        /// 更新类型：覆盖安装 | 热更
        /// </summary>
        public OwnRuleUpgradeType UpgradeType;
        /// <summary>
        /// 生效类型：立即生效 | 暂不生效(稍后生效)
        /// </summary>
        public OwnRuleEffectiveMode EffectiveMode;
        /// <summary>
        /// 自定义规则
        /// </summary>
        public string CustomRule;
        /// <summary>
        /// 预下载是否需要弹窗提示
        /// </summary>
        public bool ShowPreDownloadNotifyDialog;
        /// <summary>
        /// 是否上传远程差分包
        /// </summary>
        public bool NeedUploadEntryInfo;
    }

    public class OwnRulePatchInfo
    {
        /// <summary>
        /// 补丁类型
        /// </summary>
        public OwnRulePatchType OwnRulePatchType;
        /// <summary>
        /// 文件信息
        /// </summary>
        public OwnRuleFileInfo OwnRuleFileInfo;
        /// <summary>
        /// 包名
        /// </summary>
        public string PackageName;
        /// <summary>
        /// 补丁对应老包版本号
        /// </summary>
        public int BaseApkVersionCode;
        /// <summary>
        /// 补丁对应老包版本名
        /// </summary>
        public string BaseApkVersionName;
        /// <summary>
        /// 补丁对应老包apk唯一标识
        /// </summary>
        public string BaseApkIdentity;
        /// <summary>
        /// 补丁对应新包版本号
        /// </summary>
        public int NewApkVersionCode;
        /// <summary>
        /// 补丁对应新包版本名
        /// </summary>
        public string NewApkVersionName;
        /// <summary>
        /// 补丁对应新包apk唯一标识
        /// </summary>
        public string NewApkIdentity;
    }

    public class OwnRuleApkInfo
    {
        /// <summary>
        /// 文件信息
        /// </summary>
        public OwnRuleFileInfo OwnRuleFileInfo;
        /// <summary>
        /// 包名
        /// </summary>
        public string PackageName;
        /// <summary>
        /// 版本号
        /// </summary>
        public int VersionCode;
        /// <summary>
        /// 版本名称
        /// </summary>
        public string VersionName;
        /// <summary>
        /// apk唯一标识
        /// </summary>
        public string ApkIdentity;
    }

    public class OwnRuleFileInfo
    {
        /// <summary>
        /// 文件md5
        /// </summary>
        public string Md5;
        /// <summary>
        /// 文件crc32
        /// </summary>
        public string Crc32;
        /// <summary>
        /// 文件下载地址
        /// </summary>
        public string DownloadUrl;
        /// <summary>
        /// 备用下载地址
        /// </summary>
        public List<string> BackupDownloadUrl;
        /// <summary>
        /// 文件长度
        /// </summary>
        public long FileLength;
    }

    public enum OwnRuleUpgradeType
    {
        /// <summary>
        /// 覆盖安装
        /// </summary>
        OverwriteInstall = 1,
        /// <summary>
        /// 热更
        /// </summary>
        HotUpdate = 2
    }

    public enum OwnRuleEffectiveMode
    {
        /// <summary>
        /// 稍后生效
        /// </summary>
        EffectiveLater = 1,
        /// <summary>
        /// 立即生效
        /// </summary>
        EffectiveImmediately = 2
    }

    public enum OwnRulePatchType
    {
        /// <summary>
        /// Hdiff差分包
        /// </summary>
        Hdiff = 2,
        /// <summary>
        /// 远程差分包
        /// </summary>
        RemoteDiff = 3
    }

    public class StartUpgradeInfo
    {
        /// <summary>
        /// 当前执行到的生命周期
        /// </summary>
        public OwnRuleLifeCycle LifeCycle;
        /// <summary>
        /// 是否在wifi环境，在 onPrepareToDownload 获取到
        /// </summary>
        public bool IsWifi;
        /// <summary>
        /// 下载大小，在 onPrepareToDownload 获取到
        /// </summary>
        public long DownloadLengthInBytes;
        /// <summary>
        /// 新包版本号，在 onPrepareToHotUpdateInstall 获取到
        /// </summary>
        public int NewApkVersionCode;
        /// <summary>
        /// 老包路径，在 onReadyToOverwriteInstall 获取到
        /// </summary>
        public string ApkPath;
        /// <summary>
        /// 下载文件路径，在 onDownloadFinished 获取到
        /// </summary>
        public string DownloadedFilePath;
        /// <summary>
        /// 新包路径，在 onSynthesizeFinished 获取到
        /// </summary>
        public string NewApkPath;
    }

    public enum OwnRuleLifeCycle
    {
        OnTaskStart = 1,
        OnTaskStop = 2,
        OnPrepareToDownload = 3,
        OnPrepareToSynthesize = 4,
        OnPrepareToHotUpdateInstall = 5,
        OnReadyToOverwriteInstall = 6,
        OnHotUpdateInstalled = 7,
        OnDownloadFinished = 8,
        OnSynthesizeFinished = 9
    }

    public class ContinueExecutionInfo
    {
        /// <summary>
        /// 更新状态
        /// </summary>
        public OwnRuleStatus Status;
        /// <summary>
        /// 事件类型
        /// </summary>
        public int EventType;
        /// <summary>
        /// 初始进度
        /// </summary>
        public float InitialProgress;
        /// <summary>
        /// 更新进度
        /// </summary>
        public float Progress;
        /// <summary>
        /// 结束状态码
        /// </summary>
        public int EndCode;
        /// <summary>
        /// 结束信息
        /// </summary>
        public string EndMessage;
    }

    public enum OwnRuleStatus
    {
        OnBegin = 1,
        OnProgress = 2,
        OnEnd = 3
    }
    
#endif

    public static partial class ErrorCode
    {
        /// <summary>
        /// 无需更新
        /// </summary>
        public const int UpgradeNoNeedUpgrade = -380002;
        /// <summary>
        /// 请求服务端接口失败
        /// </summary>
        public const int UpgradeRequestFailed = -380003;
    }


}