namespace GSDK
{
    #region Delegate

    /// <summary>
    /// 初始化回调
    /// </summary>
    /// <param name="result">初始化</param>
    /// <para>
    ///     当前接口可能返回的错误码:
    ///         Success:成功
    ///         InitPermissionNotRequire:用户没有同意隐私政策的动态权限，引导用户去同意权限
    ///         InitTimout:初始化超时
    ///         InitNetworkError:初始化网络异常
    ///         Initializing:正在初始化中, 之前的初始化正在初始化中
    ///         InitIosError:iOS模块初始化失败
    ///         InitUnknownError:初始化未知错误
    /// </para>
    public delegate void InitializeDelegate(Result result);

    #endregion

    /// <summary>
    /// 用于获取实例进行接口调用
    /// e.g.GameSDK.Instance.Initialize();
    /// </summary>
    public static class GameSDK
    {
        public static readonly IGSDK Gsdk = new GSDKInner();

        public static IGSDK Instance
        {
            get { return Gsdk; }
        }
    }

    public interface IGSDK
    {
        #region Methods

        /// <summary>
        /// SDK 初始化
        ///
        /// 请务必在前置调用初始化接口，否则会影响 SDK 其他能力的正常使用！！
        /// </summary>
        /// <param name="callback">初始化回调，判断是否初始化成功</param>
        void Initialize(InitializeDelegate callback);

        #endregion
    }

    public partial class ErrorCode
    {
        /// <summary>
        /// 用户没有同意隐私政策的动态权限，引导用户去同意权限	
        /// </summary>
        public const int InitPermissionNotRequire = -901001;

        /// <summary>
        /// 初始化超时
        /// </summary>
        public const int InitTimout = -901002;

        /// <summary>
        /// 初始化网络异常
        /// </summary>
        public const int InitNetworkError = -901003;

        /// <summary>
        /// 正在初始化中, 之前的初始化正在初始化中
        /// </summary>
        public const int Initializing = -901004;

        /// <summary>
        /// iOS模块初始化失败
        /// </summary>
        public const int InitIosError = -901005;

        /// <summary>
        /// 初始化未知错误
        /// </summary>
        public const int InitUnknownError = -909999;
    }
}