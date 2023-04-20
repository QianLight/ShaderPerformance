namespace GSDK
{
    #region Delegate

    /// <summary>
    /// 邀请码回调。
    /// </summary>
    /// <param name="result">回调的结果</param>
    /// <param name="invitationCode">邀请码</param>
    public delegate void DeepLinkInvitationCodeCallback(Result result, string invitationCode);

    /// <summary>
    /// 绑定裂变回调。
    /// </summary>
    /// <param name="result">绑定裂变的结果</param>
    /// <param name="extra">绑定成功的回调数据，该数据为自定义数据（目前只支持json格式），接入方如果需要使用可以和UG侧同学商定。</param>
    public delegate void DeepLinkFissionCallback(Result result, string extra);

    #endregion
    
    #region EventHandler
    
    /// <summary>
    /// 解析ZLink携带的数据，解析成功后的事件。
    /// 通过Universal Link或App Link拉起App、Scheme拉起App、剪切板读取数据，解析成功都会触发。
    /// </summary>
    /// <param name="zLinkInfo">ZLink信息</param>
    public delegate void DeepLinkParseZLinkEventHandler(DeepLinkZLinkInfo zLinkInfo);

    /// <summary>
    /// 归因成功的事件（只会触发一次）。
    /// iOS为自动归因，Android需要接入方手动调用DoAttribution接口。
    /// </summary>
    /// <param name="info">归因信息</param>
    public delegate void DeepLinkAttributionEventHandler(DeepLinkAttributionInfo info);

    #endregion

    /// <summary>
    /// 用于获取实例进行接口调用
    /// e.g.DeepLink.Service.MethodName();
    /// </summary>
    public static class DeepLink
    {
        public static IDeepLinkService Service
        {
            get { return ServiceProvider.Instance.GetService(ServiceType.DeepLink) as IDeepLinkService; }
        }
    }

    public interface IDeepLinkService : IService
    {
        #region Events

        /// <summary>
        /// 解析ZLink携带的数据，解析成功后的事件。
        /// 通过Universal Link或App Link拉起App、Scheme拉起App、剪切板读取数据，解析成功都会触发。
        /// </summary>
        event DeepLinkParseZLinkEventHandler ParseZLinkEvent;

        /// <summary>
        /// 归因成功的事件（该事件只会触发一次）。
        /// iOS为自动归因，Android需要接入方手动调用DoAttribution接口。
        /// </summary>
        event DeepLinkAttributionEventHandler AttributionEvent;

        #endregion
        
        #region Methods
        
        /// <summary>
        /// 初始化DeepLink服务。
        /// 需要在调用初始化前监听ParseZLinkEvent和AttributionEvent事件。
        /// </summary>
        void Initialize();
        
        /// <summary>
        /// 检查剪切板并解析ZLink数据。（该接口会识别剪切板）
        /// 如果数据解析成功，会触发ParseZLinkEvent事件。
        /// </summary>
        void CheckClipboard();

#if UNITY_ANDROID
        /// <summary>
        /// 进行归因。（该接口会识别剪切板）
        /// 如果归因成功，会触发AttributionEvent事件。
        /// </summary>
        void DoAttribution();
#endif
        
        /// <summary>
        /// 获取当前登录账号的邀请码。
        /// </summary>
        /// <param name="callback">邀请码回调</param>
        /// <para>
        ///     当前接口可能返回的错误码:
        ///         Success:成功
        ///         DeepLinkInvalidParameter:使用了非法参数
        ///         DeepLinkUnexpectedResponse:服务器给出了预期之外的回复
        ///         DeepLinkRequestError:网络请求错误
        ///         DeepLinkUnknownError:未知错误
        /// </para>
        void GetInvitationCode(DeepLinkInvitationCodeCallback callback);

        /// <summary>
        /// 拼接参数并生成携带邀请码的分享URL。
        /// </summary>
        /// <param name="url">分享裂变落地页URL</param>
        /// <param name="invitationCode">邀请码，可以通过GetInvitation获取</param>
        /// <param name="extraJson">分享者传递的自定义参数（目前只支持json格式），由接入方自定义。</param>
        /// <returns>拼接后的URL</returns>
        string GenerateShareURL(string url, string invitationCode, string extraJson = "");

        /// <summary>
        /// 上传邀请码并绑定裂变关系。（该接口在不传入邀请码时，会识别剪切板）
        /// 该接口由两种使用场景:
        /// 1.（手动传入invitationCode）由用户手动填写邀请码并绑定裂变关系。
        /// 2.（不传入invitationCode，通过剪切板自动识别）用户通过分享Url拉起App后，剪切板中会携带邀请码数据，此时调用该接口不需要传入参数。
        /// </summary>
        /// <param name="callback">绑定裂变的回调</param>
        /// <param name="invitationCode">邀请码</param>
        /// <para>
        ///     当前接口可能返回的错误码:
        ///         Success:成功
        ///         DeepLinkInvalidParameter:使用了非法参数
        ///         DeepLinkUnexpectedResponse:服务器给出了预期之外的回复
        ///         DeepLinkRequestError:网络请求错误
        ///         DeepLinkNoInvitationCode:剪切板中没有邀请码
        ///         DeepLinkUnknownError:未知错误
        /// </para>
        void BindFission(DeepLinkFissionCallback callback, string invitationCode = "");
        
#if UNITY_IOS
        /// <summary>
        /// 启动Apple Search Ads归因
        /// 请在GSDK初始化完成且成功后调用，并且保证ATT授权弹窗完成
        /// </summary>
        void StartASAAttribution();
        
        /// <summary>
        /// 返回Apple Search Ads是否归因
        /// </summary>
        bool IsASAAttributed();
#endif
        
        #endregion
    }

    #region Public Definitions DeepLink使用的所有数据结构

    /// <summary>
    /// ZLink信息。
    /// </summary>
    public struct DeepLinkZLinkInfo
    {
        /// <summary>
        /// Scheme字符串。
        /// </summary>
        public string Scheme;
        /// <summary>
        /// 数据来源。
        /// </summary>
        public DeepLinkSourceType SourceType;
    }

    /// <summary>
    /// 归因信息。
    /// </summary>
    public struct DeepLinkAttributionInfo
    {
        /// <summary>
        /// 归因成功后获得的数据。
        /// 该数据为自定义数据（目前只支持json格式），接入方如果需要使用可以和UG侧同学商定。
        /// </summary>
        public string Data;
        /// <summary>
        /// 数据来源。
        /// </summary>
        public DeepLinkSourceType SourceType;
    }

    /// <summary>
    /// 数据来源。
    /// </summary>
    public enum DeepLinkSourceType
    {
        /// <summary>
        /// 未知来源。
        /// </summary>
        Unknown = 1,
        /// <summary>
        /// 通过Universal Link或App Link拉起App时获取。
        /// </summary>
        UniversalOrAppLink = 2,
        /// <summary>
        /// 通过Scheme拉起App时获取。
        /// </summary>
        Scheme = 3,
        /// <summary>
        /// 通过剪切板读取。
        /// </summary>
        Clipboard = 4,
    }
    
    #endregion

    public static partial class ErrorCode
    {
        /// <summary>
        /// 使用了非法参数。
        /// </summary>
        public static int DeepLinkInvalidParameter = -260001;
        /// <summary>
        /// 服务器给出了预期之外的回复。
        /// </summary>
        public static int DeepLinkUnexpectedResponse = -260002;
        /// <summary>
        /// 网络请求错误。
        /// </summary>
        public static int DeepLinkRequestError = -260003;
        /// <summary>
        /// 剪切板中没有邀请码。
        /// </summary>
        public static int DeepLinkNoInvitationCode = -260004;
        /// <summary>
        /// 未知错误。
        /// </summary>
        public static int DeepLinkUnknownError = -269999;
    }
}
