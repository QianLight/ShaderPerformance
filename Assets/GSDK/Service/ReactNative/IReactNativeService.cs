using System;
using System.Collections.Generic;
using UnityEngine;

namespace GSDK
{
    /// <summary>
    /// 九尾模块。
    /// ReactNative.Service.XXX();
    /// </summary>
    public static class ReactNative
    {
        public static IReactNativeService Service
        {
            get { return ServiceProvider.Instance.GetService(ServiceType.ReactNative) as IReactNativeService; }
        }
    }

#if UNITY_ANDROID
    /// <summary>
    /// 完成查询 Debug 模式后的回调。
    /// </summary>
    /// <param name="result">包含有错误码的请求结果。</param>
    /// <param name="isEnabled">Debug 模式是否启用。</param>
    public delegate void ReactNativeIsDebugModeEnabledDelegate(Result result, bool isEnabled);
#endif
    
#if UNITY_IOS

    /// <summary>
    /// 设置横竖屏的接口回调
    /// </summary>
    /// <param name="result"></param>
    public delegate void ReactNativeSetOrientationDelegate(Result result);
#endif
    
    /// <summary>
    /// 初始化完成回调
    /// </summary>
    /// <param name="result"></param>
    public delegate void ReactNativeInitDelegate(Result result);
    
    /// <summary>
    /// 完成获取活动列表后的回调。
    /// </summary>
    /// <param name="result">包含有错误码的请求结果。</param>
    /// <param name="list">活动列表。</param>
    public delegate void ReactNativeFetchPagesDelegate(Result result, List<ReactNativePage> list);

    /// <summary>
    /// 打开页面后的回调。
    /// </summary>
    /// <param name="result">包含有错误码的请求结果。
    /// <para>
    /// 可能会出现的错误码：
    ///     ReactNativeCreateWindowFailed: 页面对应的 URL 不合法。对应通知中的 ReactNativeInvalidURLParameters 错误码。请检查 URL 格式是否正确。
    /// </para>
    /// </param>
    /// <param name="window">窗口，可调用相关函数打开/关闭/隐藏/显示。</param>
    public delegate void ReactNativeOpenPageDelegate(Result result, ReactNativeWindow window);


    /// <summary>
    /// 打开页面后的回调。
    /// </summary>
    /// <param name="result">包含有错误码的请求结果。
    /// <para>
    /// 可能会出现的错误码：
    ///     ReactNativeCreateWindowFailed: 页面对应的 URL 不合法。对应通知中的 ReactNativeInvalidURLParameters 错误码。请检查 URL 格式是否正确。
    /// </para>
    /// </param>
    /// <param name="window">关闭的页面</param>
    public delegate void ReactNativePageCloseDelegate(Result result, ReactNativeWindow window);

    /// <summary>
    /// 获取到多个红点信息后的回调。
    /// </summary>
    /// <param name="result">包含有错误码的请求结果。</param>
    /// <param name="badges">红点信息。字典的键为活动 ID，值为对应的红点信息。</param>
    public delegate void ReactNativeFetchBadgesDelegate(Result result, List<ReactNativeBadge> badges);

    /// <summary>
    /// 获取到单个红点信息后的回调。
    /// </summary>
    /// <param name="result">包含有错误码的请求结果。</param>
    /// <param name="badge">红点信息。字典的键为活动ID，值为对应的红点信息。
    /// <para>
    /// 可能会出现的错误码：
    ///     ReactNativeInvalidPageID: 当前页面的 ID 不正确。请检查页面的 ID 是否正确。
    /// </para>
    /// </param>
    public delegate void ReactNativeFetchBadgeDelegate(Result result, ReactNativeBadge badge);

    /// <summary>
    /// 服务器响应获取配置的值时的回调。
    /// </summary>
    /// <param name="result">包含有错误码的请求结果。</param>
    /// <param name="value">配置的值。</param>
    public delegate void ReactNativeFetchConfigValueDelegate(Result result, string value);

    /// <summary>
    /// 控制（关闭/隐藏/显示）一个页面时的回调。
    /// </summary>
    /// <param name="result">包含有是否成功的请求结果。
    /// <para>
    /// 可能会出现的错误码：
    ///     ReactNativeEventIsEmpty: 传递的事件名称为空，无法传递。请检查 @event 参数是否为 null 或 ""。
    ///     ReactNativeInvalidOperation: 当前页面为 Web 页面，操作无效。	检查当前页面的类型，确认为 ReactNative 后再试。
    ///     ReactNativeOperationFailed: 操作失败，可能原因有当前页面已经关闭、使用最新窗口模式，但所有窗口已经关闭或尚未打开任何窗口。
    /// </para>
    /// </param>
    public delegate void ReactNativeControlWindowDelegate(Result result);

    /// <summary>
    /// 向页面发送信息后的回调。
    /// </summary>
    /// <param name="result">包含有错误码的请求结果。
    /// <para>
    /// 可能会出现的错误码：
    ///     ReactNativeEventIsEmpty: 传递的事件名称为空，无法传递。请检查 @event 参数是否为 null 或 ""。
    ///     ReactNativeInvalidOperation: 当前页面为 Web 页面，操作无效。	检查当前页面的类型，确认为 ReactNative 后再试。
    ///     ReactNativeOperationFailed: 操作失败，可能原因有当前页面已经关闭、使用最新窗口模式，但所有窗口已经关闭或尚未打开任何窗口。
    /// </para>
    /// </param>
    public delegate void ReactNativeSendMessageDelegate(Result result);

    /// <summary>
    /// 从游戏本体（原生侧）接收到通知的事件。
    /// </summary>
    /// <param name="message">通知的内容。</param>
    public delegate void ReactNativeOnMessageReceivedEventHandler(ReactNativeMessage message);

    /// <summary>
    /// 从游戏本体（原生测）接收到异常的事件。
    /// </summary>
    /// <param name="result">可分析 result.Error 及 result.Message 来分析错误原因。
    /// <para>
    /// 可能会出现的错误码：
    ///     ReactNativeJSBundleNotFound: 对应的 JS Bundle 没有找到。Bundle 下载失败，或还没有完成 Bundle 的下载任务。
    ///         检查 moduleName 对应的文件夹是否存在于本地沙箱的 ReactNative 文件夹内；或提示用户稍后再试。
    ///     ReactNativeLoadJSBundleFailed: JS bundle 加载失败。联系接入人员解决。
    ///     ReactNativeUnsupportedDevice: 机型不支持。部分安卓手机不支持 ReactNative 框架，无法展示 ReactNative 页面。提示用户更换手机。
    ///     ReactNativeCreateWindowFailed: 窗口创建失败，可能原因是URL 存在问题。检查传入的 URL 是否符合规定。
    ///     ReactNativeJSRuntimeError: Javascript 运行时错误。页面上发生了 Javascript 错误。检查前端编写是否有问题。
    /// </para>
    /// </param>
    /// <param name="type">当前页面对应的场景类型。</param>
    /// <param name="inGameID">游戏内 ID，对应 OpenPage 时传入的 inGameID。</param>
    public delegate void ReactNativeOnErrorOccuredEventHandler(Result result, ReactNativeSceneType type,
        string inGameID);

    /// <summary>
    /// 打开自助验收页面回调
    /// </summary>
    /// <param name="result"></param>
    public delegate void ReactNativeOnShowTestPageEventHandler(Result result);
    
    public interface IReactNativeService : IService
    {
        /// <summary>接收到通知时的事件。用于九尾和游戏本体之间的通讯。请确保在初始化之前对本事件做了监听。</summary>
        event ReactNativeOnMessageReceivedEventHandler OnMessageReceived;

        /// <summary>接收到错误信息时的通知。此处错误信息仅限打开页面时及 ReactNative 的 Javascript 运行时错误。</summary>
        event ReactNativeOnErrorOccuredEventHandler OnErrorOccured;

#if UNITY_ANDROID
        /// <summary>
        /// 检测是否开启前端调试模式。仅支持 Android。若开启，可在页面"摇一摇"唤起调试页面。
        /// </summary>
        /// <param name="callback">包含有当前是否开启的回调。</param>
        void IsDebugModeEnabled(ReactNativeIsDebugModeEnabledDelegate callback);

        /// <summary>
        /// 设置是否开启前端调试膜。仅支持 Android。若开启，可在页面"摇一摇"唤起调试页面。
        /// </summary>
        /// <param name="isEnabled">是否启用。</param>
        void SetDebugMode(bool isEnabled);
#endif
        
#if UNITY_IOS
        /// <summary>
        /// 设置横竖屏
        /// </summary>
        /// <param name="type">横竖屏类型</param>
        /// <param name="callback">是否设置成功</param>
        void setOrientation(ReactNativeOrientationType type, ReactNativeSetOrientationDelegate callback);
#endif
        
        /// <summary>
        /// 初始化九尾模块。
        /// </summary>
        /// <param name="roleID">角色ID。</param>
        /// <param name="roleName">角色名称。</param>
        /// <param name="serverID">区服ID。</param>
        void Initialize(string roleID, string roleName, string serverID, ReactNativeInitDelegate callback);

        void SyncGecko(ReactNativeInitDelegate callback);

        /// <summary>
        /// 打开剪切板邀请页面。
        /// </summary>
        /// <param name="callback">包含有错误码的回调。</param>
        void OpenReferralPage(ReactNativeOpenPageDelegate callback);

        /// <summary>
        /// 获取指定场景类型的页面列表。
        /// </summary>
        /// <param name="scene">活动场景信息，包含场景名称及缓存策略，具体见 ReactNativeScene。</param>
        /// <param name="callback">包含有场景对应的页面列表的回调。</param>
        void FetchPages(ReactNativeScene scene, ReactNativeFetchPagesDelegate callback);

        /// <summary>
        /// 根据活动类型获取红点信息。
        /// </summary>
        /// <param name="type">场景类型，具体见 ReactNativeSceneType。</param>
        /// <param name="callback">包含有活动 ID 对应红点信息的回调。</param>
        void FetchBadges(ReactNativeSceneType type, ReactNativeFetchBadgesDelegate callback);

        /// <summary>通过配置的键来获取配置的值。</summary>
        /// <param name="key">想要查询的配置的值对应的配置的键。</param>
        /// <param name="callback">包含有对应配置值和请求状态的回调。</param>
        void FetchConfigValue(string key, ReactNativeFetchConfigValueDelegate callback);
        /// <summary>
        /// 获取打开的九尾窗口，倒序存储，最后一个为最上面的页面
        /// </summary>
        /// <returns>
        /// 打开的九尾窗口列表
        /// </returns>
        List<ReactNativeWindow> GetRNWindows();

        /// <summary>
        /// 关闭所有九尾窗口
        /// </summary>
        void CloseAllRNWindows();

        /// <summary>
        /// 打开自助验收页面
        /// </summary>
        void showTestPage(ReactNativeOnShowTestPageEventHandler callback);
        
        /*
      * 游戏设置父节点
      */
        void SetGameGoParent(string parentGoName);

        /*
         * 游戏的字体路径
         */
        void SetGameFont(string fontName, Font font);
        /*
         * 提供游戏的用户信息，如角色信息，区服信息等，提供更通用的设置接口
         */
        void SetGameData(Dictionary<string, object> gameData);
    }

    /// <summary>
    /// 九尾的场景设置。
    /// </summary>
    public struct ReactNativeScene
    {
        /// <summary>
        /// 场景类型。
        /// </summary>
        public readonly string Type;

        /// <summary>
        /// 是否从本地缓存加载。
        /// </summary>
        public readonly bool LoadFromCache;

        /// <summary>
        /// 预设场景：拍脸，从服务器端实时拉取。
        /// </summary>
        public static ReactNativeScene Home =
            new ReactNativeScene(ReactNativeInnerTools.ConvertSceneType(ReactNativeSceneType.Home), false);

        /// <summary>
        /// 预设场景：游戏大厅活动入口，默认从服务器实时读取。
        /// </summary>
        public static ReactNativeScene Lobby =
            new ReactNativeScene(ReactNativeInnerTools.ConvertSceneType(ReactNativeSceneType.Lobby), false);

        /// <summary>
        /// 预设场景：高光时刻，从服务端实时拉取。
        /// </summary>
        public static ReactNativeScene HighlightMoments =
            new ReactNativeScene(
                ReactNativeInnerTools.ConvertSceneType(ReactNativeSceneType.HighlightMoments), false);

        /// <summary>
        /// 自定义场景：支持提供任意参数。
        /// </summary>
        /// <param name="type">活动类型。</param>
        /// <param name="loadFromCache">是否从本地缓存加载。</param>
        /// <returns>缓存设置。</returns>
        internal static ReactNativeScene Custom(string type, bool loadFromCache)
        {
            return new ReactNativeScene(type, loadFromCache);
        }

        /// <summary>
        /// 构造器。
        /// </summary>
        /// <param name="type">活动类型。</param>
        /// <param name="loadFromCache">是否从本地缓存加载。</param>
        private ReactNativeScene(string type, bool loadFromCache)
        {
            Type = type;
            LoadFromCache = loadFromCache;
        }
    }


    /// <summary>
    /// 九尾的场景页面。
    /// </summary>
    public abstract class ReactNativePage
    {
        /// <summary>
        /// 活动ID。此 ID 为服务端随机生成，仅用于开启页面接口。
        /// </summary>
        public abstract ulong ID { get; }

        /// <summary>
        /// 活动URL。
        /// </summary>
        public abstract string URL { get; }

        /// <summary>
        /// 游戏内部ID。
        /// 此 ID 为开发者与服务端协商决定，可用于判断具体页面。
        /// </summary>
        public abstract string InGameID { get; }

        /// <summary>
        /// 自定义页面。
        /// </summary>
        /// <param name="url">页面对应的 URL。</param>
        /// <returns>ReactNativePage 页面。</returns>
        public static ReactNativePage Custom(ulong id, string url, string inGameID)
        {
            return new ReactNativePageImplementation(id, url, inGameID);
        }

        /// <summary>
        /// 打开当前页面。
        /// </summary>
        /// <param name="parameters">自定义传递给前端页面的参数。</param>
        /// <param name="callback">包含有打开 URL 是否成功的回调。</param>
        public abstract void Open(Dictionary<string, object> parameters, ReactNativeOpenPageDelegate callback);

        public abstract void Open(Dictionary<string, object> parameters, ReactNativeOpenPageDelegate callback, ReactNativePageCloseDelegate closeDelegate);

        /// <summary>
        /// 获取当前页面所对应的红点信息。
        /// </summary>
        /// <param name="callback">包含有对应红点信息的回调。</param>
        public abstract void FetchBadge(ReactNativeFetchBadgeDelegate callback);
    }

    /// <summary>
    /// 打开页面后，对应的窗口。
    /// </summary>
    public abstract class ReactNativeWindow
    {
        /// <summary>
        /// 窗口ID。
        /// </summary>
        public abstract string ID { get; }

        /// <summary>
        /// 窗口类型。
        /// </summary>
        public abstract ReactNativeWindowType Type { get; }

        /// <summary>
        /// 窗口是否显示
        /// </summary>
        public abstract bool Showing { get; }

        /// <summary>
        /// 活动url
        /// </summary>
        public abstract string Url { get; }

        /// <summary>
        /// 活动名称
        /// </summary>
        public abstract string InGameID { get; }

        /// <summary>
        /// 活动type
        /// </summary>
        public abstract ReactNativeSceneType SceneType { get; }

        /// <summary>
        /// 最后一个窗口。注意，返回内容会随窗口打开或关闭变化，并非固定一直指向当前状态下的最后一个窗口。
        /// </summary>
        public static ReactNativeWindow Last =
            new ReactNativeWindowImplementation("", ReactNativeWindowType.ReactNative);

        /// <summary>
        /// 根据 ID 自定义窗口。
        /// </summary>
        public static ReactNativeWindow Custom(string id)
        {
            return new ReactNativeWindowImplementation(id, ReactNativeWindowType.ReactNative);
        }

        /// <summary>
        /// 显示本窗口。
        /// </summary>
        public abstract void Show(ReactNativeControlWindowDelegate callback);

        /// <summary>
        /// 关闭窗口，关闭后不可再显示或隐藏。
        /// </summary>
        public abstract void Close(ReactNativeControlWindowDelegate callback);

        /// <summary>
        /// 隐藏本窗口。
        /// </summary>
        public abstract void Hide(ReactNativeControlWindowDelegate callback);

        /// <summary>
        /// 给窗体发送信息。若要接受从前端返回的数据，请监听 OnMessageReceived 事件。
        /// </summary>
        /// <param name="event">事件名称。</param>
        /// <param name="message">信息内容。</param>
        /// <param name="callback">包含是否发送成功的回调。</param>
        public abstract void Send(string @event, string message, ReactNativeSendMessageDelegate callback);
    }

    /// <summary>
    /// 场景类型。
    /// </summary>
    public enum ReactNativeSceneType
    {
        /// <summary>
        /// 未知场景，服务端没有配置。
        /// </summary>
        Unknown,

        /// <summary>
        /// 拍脸。
        /// </summary>
        Home,

        /// <summary>
        /// 游戏大厅活动。
        /// </summary>
        Lobby,

        /// <summary>
        /// 高光时刻。
        /// </summary>
        HighlightMoments,

        /// <summary>
        /// 邀请绑定。
        /// </summary>
        Referral
    }

    /// <summary>
    /// 红点信息。
    /// </summary>
    public class ReactNativeBadge
    {
        /// <summary>
        /// 活动ID。此 ID 为服务端随机生成，仅用于开启页面接口。
        /// </summary>
        public ulong ID;

        /// <summary>
        /// 游戏内部ID。
        /// 此 ID 为开发者与服务端协商决定，可用于判断具体页面。
        /// </summary>
        public string InGameID;

        /// <summary>
        /// 红点类型。
        /// </summary>
        public ReactNativeBadgeType Type;

        /// <summary>
        /// 具体数量，仅限 Type=Number 时有效。
        /// </summary>
        public int Count;

        /// <summary>
        /// 其他信息。可根据 CP 需求定制。
        /// </summary>
        public string Extra;
    }

    /// <summary>
    /// 通知类型。
    /// </summary>
    public enum ReactNativeBadgeType
    {
        /// <summary>
        /// 无。没有通知。
        /// </summary>
        None = 0,

        /// <summary>
        /// 红点。有通知，但不知道具体数量。
        /// </summary>
        Dot = 1,

        /// <summary>
        /// 具体数字。有通知，且知道具体数量，请参考 Count 字段。
        /// 注：目前此处数字更新不及时，可直接判断大于 0 显示红点即可。
        /// </summary>
        Number = 2,
    }

    /// <summary>
    /// 打开的页面的类型
    /// </summary>
    public enum ReactNativeWindowType
    {
        /// <summary>
        /// ReactNative 页面
        /// </summary>
        ReactNative,

        /// <summary>
        /// 网页页面
        /// </summary>
        Web,

        /// <summary>
        /// ReactUnity 页面
        /// </summary>
        ReactUnity,
    }

#if UNITY_IOS
    /// <summary>
    /// 九尾横竖屏类型
    /// </summary>
    public enum ReactNativeOrientationType
    {
        /// <summary>
        /// 竖屏
        /// </summary>
        Portrait = 2,
        /// <summary>
        /// 横屏
        /// </summary>
        Horizontal = 8,
    }
#endif

    /// <summary>
    /// 九尾模块通知内容。
    /// </summary>
    public class ReactNativeMessage
    {
        /// <summary>
        /// 当前通知的类型。
        /// </summary>
        public string Type;

        /// <summary>
        /// 通知附带的消息信息。
        /// </summary>
        public string Message;

        /// <summary>
        /// 通知附带的参数信息。
        /// </summary>
        public string Parameters;

        /// <summary>
        /// 原始的通知信息，为JSON字符串。解析失败时请参考本信息。
        /// </summary>
        public string Raw;
    }

    public static partial class ErrorCode
    {
        #region 初始化失败
        
        /// <summary>
        /// 初始化缺少必要参数
        /// </summary>
        public const int ReactNativeInitWithoutParams = -3200001;

        /// <summary>
        /// 不支持九尾引擎
        /// </summary>
        public const int ReactNativeNotSupport = -3200002;

        #endregion

        /// <summary>
        /// 获取结果失败
        /// </summary>
        public const int ReactNativeNetworkError = -3210001;
        
        #region 打开页面时的错误

        /// <summary>
        /// JS Bundle 没有找到。
        /// </summary>
        public const int ReactNativeJSBundleNotFound = -320100;

        /// <summary>
        /// JS Bundle 加载失败。
        /// </summary>
        public const int ReactNativeLoadJSBundleFailed = -320101;

        /// <summary>
        /// 机型不支持。
        /// </summary>
        public const int ReactNativeUnsupportedDevice = -320102;

        /// <summary>
        /// URL 参数错误。
        /// </summary>
        public const int ReactNativeInvalidURLParameters = -320103;

        /// <summary>
        /// 窗口创建失败，可能原因是：URL 不合法。
        /// </summary>
        public const int ReactNativeCreateWindowFailed = -320104;

        #endregion

        #region 前端错误

        /// <summary>
        /// JS 运行时异常。
        /// </summary>
        public const int ReactNativeJSRuntimeError = -320200;

        #endregion

        #region 发送消息错误

        /// <summary>
        /// 向页面发送的消息事件为空。
        /// </summary>
        public const int ReactNativeEventIsEmpty = -320300;

        /// <summary>
        /// 当前页面为 Web 页面，无法操作。
        /// </summary>
        public const int ReactNativeInvalidOperation = -320301;

        /// <summary>
        /// 页面操作失败。原因可能有：当前页面已经关闭、使用最新窗口模式，但所有窗口已经关闭。
        /// </summary>
        public const int ReactNativeOperationFailed = -320302;

        #endregion

        #region 获取红点问题

        /// <summary>
        /// 活动 ID 不合法。
        /// </summary>
        public const int ReactNativeInvalidPageID = -320400;

        #endregion



        #region 邀剪切板请绑定错误

        /// <summary>
        /// 邀请绑定失败。原因可能有：没有剪切板、没有匹配到正则、没有下发活动或活动打开失败。
        /// </summary>
        public const int ReactNativeOpenReferralPageFailed = 329804;

        #endregion


        /// <summary>
        /// 未知错误。
        /// </summary>
        public const int ReactNativeUnknownError = -329999;
    }
}