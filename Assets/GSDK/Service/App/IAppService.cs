using System;
using System.Collections.Generic;

namespace GSDK
{
    public enum DeviceRegistrationError
    {
        /// <summary>
        /// 设备注册在传入的超时限制到达时仍未成功
        /// </summary>
        Timeout = -902002,

        /// <summary>
        /// 设备注册内部错误，可进一步通过Result.ExtraCode Result.ExtraMessage确认细节原因
        /// </summary>
        DRError = -902003,

        /// <summary>
        /// 没有在配置面板打开设备注册优化，导致功能流程无法执行
        /// </summary>
        ConfigError = -902004
    }

    /// <summary>
    /// 申请Android权限回调
    /// </summary>
    /// <param name="result">回调结果</param>
    /// <param name="permissionInfo">权限信息</param>
    public delegate void RequestPermissionDelegate(Result result, PermissionInfo permissionInfo);

    /// <summary>
    /// FetchDeviceInfo 回调
    /// </summary>
    /// <param name="result">返回结果状态</param>
    /// <param name="did">设备ID</param>
    /// <param name="iid">安装ID</param>
    public delegate void RequestDeviceInfoCallback(Result result, string did, string iid);

    #region EventHandler

    /// <summary>
    /// Deeplink数据回调
    /// </summary>
    /// <param name="deeLinkData">Deeplink数据回调结果</param>
    public delegate void RequestDeepLinkURLEventHandler(string deeLinkUrl);

    /// <summary>
    /// 监听设备注册回调
    /// </summary>
    /// <param name="did">设备ID</param>
    /// <param name="iid">安装ID</param>
    public delegate void RequestDeviceInfoUpdateEventHandler(string did, string iid);

    #endregion

    /// <summary>
    /// 用于获取实例进行接口调用
    /// e.g.App.Service.MethodName();
    /// </summary>
    public static class App
    {
        public static IAppService Service
        {
            get { return ServiceProvider.Instance.GetService(ServiceType.App) as IAppService; }
        }
    }

    public interface IAppService : IService
    {
        #region Properties

        /// <summary>
        /// 初始化监听Deeplink事件。
        /// 需要在调用初始化前监听Deeplink Data事件。
        /// </summary>
        void Initialize();

        /// <summary>
        /// 业务方在初始化前，可以调用该方法，重新设置一些配置信息，目前支持server_region的配置（10--新加坡，20--美东）
        /// 必须在初始化前调用！
        /// </summary>
        /// <param name="extraParams">传入的字典参数value可以为任意类型</param>
        void updateConfiguration(Dictionary<String, Object> extraParams);

        /// <summary>
        /// Applog的设备id，用来唯一表示一台设备的id。在首次安装未完成设备注册的情况下可能为空
        /// </summary>
        string DeviceID { get; }

        /// <summary>
        /// 异步获取设备信息，含deviceid installid
        /// </summary>
        /// <param name="timeout">单位毫秒</param>
        /// <param name="callback"></param>
        void FetchDeviceInfo(int timeout, RequestDeviceInfoCallback callback);

        /// <summary>
        /// 注册监听设备注册信息更新事件
        /// 注意：在本地没有设备信息缓存的情况下，如果网络等外部环境的限制未消除（如断网等），设备注册一直没有成功则这个事件回调一直不会触发
        /// 注意：在本地有设备信息缓存的情况下，会直接使用设备信息回调
        /// </summary>
        /// <param name="handler"></param>
        void RegisterDeviceInfoUpdateHandler(RequestDeviceInfoUpdateEventHandler handler);

        /// <summary>
        /// Applog的安装id，用来唯一标识一次安装的id。一个设备对应对应一个App的安装，设备每安装一次全新的App（不同版本，不同渠道）都算作一次新的安装，
        /// 安装同一渠道的统一版本都算作同一词安装，一个设备可以对应多个App的安装，同时，一个设备也可以对应一个App的多次安装。
        /// </summary>
        string InstalledID { get; }

        /// <summary>
        /// 每个游戏配置的字节游戏中台服务的appid，用以访问各种服务。
        /// </summary>
        string AppID { get; }

        /// <summary>
        /// 每个游戏配置的字节游戏中台服务的DownloadSource字段，可以用于区分版属包
        /// </summary>
        string DownloadSource { get; }
        
        /// <summary>
        /// 一级渠道类型（自研、百度、小米、360）不同的渠道会有不同的账号和支付表现，例如百度渠道使用的是百度账号和百度支付，通过替换了账号和支付的渠道层实现逻辑
        /// </summary>
        string ChannelOp { get; }

        Dictionary<GSDKPermissionTypeKey,bool> RequestAllPermissionsStates();

        void changePermissionStates(GSDKPermissionTypeKey key, bool newState);

#if UNITY_STANDALONE_WIN && !GMEnderOn
        void OpenPCBrowser(string url);
#endif


        /// <summary>
        /// 二级渠道，仅代表下载渠道（taptap，应用宝）替换了config.json/info.plist配置文件中的一个字段，不影响表现和逻辑
        /// </summary>
        string Channel { get; }

        /// <summary>
        /// 当前Debug开关设置
        /// </summary>
        bool EnableDebugMode { set; get; }
        
        /// <summary>
        /// 当前RNDebug开关设置
        /// </summary>
        bool EnableRNDebugMode { set; get; }

        /// <summary>
        /// BOE环境下网络请求Header中特殊标识，通常不设置，如果接入的服务端有特殊要求，再传入即可，默认为"prod"，需要在EnableBOEMode前设置
        /// </summary>
        string BOEHeader { set; get; }

        /// <summary>
        /// 当前BOE设置开关
        /// </summary>
        bool EnableBOEMode { set; get; }


#if UNITY_STANDALONE_WIN && !GMEnderOn
        bool EnablePPEMode { set; get; }
#endif
        /// <summary>
        /// 当前沙盒设置开关
        /// </summary>
        bool EnableSandboxMode { set; get; }

        /// <summary>
        /// 当前GSDK Native SDK版本信息，该接口可以在初始化之后直接调用，无须等初始化结果
        /// </summary>
        string GSDKNativeVersion { get; }

#if UNITY_STANDALONE_WIN && !GMEnderOn
        /// <summary>
        /// 获取公参接口
        /// </summary>
        string CommonParam { get; }
#endif

        #endregion

        #region Events

        // <summary>
        /// deeplink数据回调事件。请【务必】在Initialize之前设置
        /// 通过Universal Link或App Link拉起App、Scheme拉起App会触发。
        /// </summary>
        event RequestDeepLinkURLEventHandler DeeplinkURLEvent;

        #endregion

        #region Methods


        /// <summary>
        /// 申请Android权限
        /// </summary>
        /// <param name="permissions">申请权限列表，如"android.permission.WRITE_EXTERNAL_STORAGE"</param>
        /// <param name="callback">完成后的回调</param>
        /// <param name="extraJsonData">设置游戏额外信息，用于用户权限授予行为埋点上报</param>
        void RequestPermission(List<string> permissions, RequestPermissionDelegate callback,
            string extraJsonData = null);


#if UNITY_IOS
        /// <summary>
        /// 用于触发NativeBridge层的空实现，在初始化时调用即可
        /// 只有iOS有
        /// </summary>
        /// <param name="triggerCallback">回调，啥也不传入也可以</param>
        void TriggerBridgeInit(Action<Result> triggerCallback);

        /// <summary>
        /// 清除本地缓存的DeviceId和InstallId
        /// </summary>
        /// <returns></returns>
        bool ClearDeviceIdAndInstallId();
#endif

        #endregion
    }

    public enum PermissionCode
    {
        /// <summary>
        /// 本次允许/运行期间允许/允许/始终允许
        /// </summary>
        AlwaysAllow = 0,
        /// <summary>
        /// 拒绝/禁止
        /// </summary>
        Deny = -1,
        /// <summary>
        /// 禁止后不再提示/拒绝且不再询问
        /// </summary>
        DenyWithoutPromp = -2
    }

    public enum GSDKPermissionTypeKey
    {
        GSDKPermissionTypeKeyCamera = 1,
        GSDKPermissionTypeKeyPhotoLibrary = 2,
        GSDKPermissionTypeKeyPhotoLibraryAdd = 3,
        GSDKPermissionTypeKeyMicrophone = 4,
        GSDKPermissionTypeKeyUserTracking = 5,
        GSDKPermissionTypeKeyLocation = 6,
        GSDKPermissionTypeKeyContacts = 7,
        GSDKPermissionTypeKeyPhone = 8,
        GSDKPermissionTypeKeySMS = 9,
        GSDKPermissionTypeKeyStorage = 10
    }


    public class PermissionInfo
    {
        /// <summary>
        /// 是否全部授权
        /// </summary>
        public bool IsAllGranted;
        /// <summary>
        /// 授权结果， key为申请的权限字符串，如"android.permission.WRITE_EXTERNAL_STORAGE"
        /// </summary>
        public Dictionary<string, PermissionCode> GrantResults;
    }


}