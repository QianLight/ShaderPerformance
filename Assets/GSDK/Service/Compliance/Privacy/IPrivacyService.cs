namespace GSDK
{
    /// <summary>
    /// 返回用户是否同意个人信息保护指引弹窗的结果
    /// </summary>
    /// <param name="agree">用户是否同意，true为同意</param>
    public delegate void PrivacyShownEventHandler(bool agree);

    /// <summary>
    /// 用于获取实例进行接口调用
    /// e.g.Privacy.Service.MethodName();
    /// </summary>
    public static class Privacy
    {
        public static IPrivacyService Service
        {
            get { return ServiceProvider.Instance.GetService(ServiceType.Privacy) as IPrivacyService; }
        }
    }

    /// <summary>
    /// 隐私合规（国内）
    ///
    /// 游戏需要在启动时弹出信息保护指引弹窗，明确告知玩家游戏所要的权限内容及用法，以及隐私政策和用户协议的正文披露。
    /// 注：Android需要在config勾选isNeedPrivacyProtection，则表示需要使用个人信息保护指引功能
    /// </summary>
    public interface IPrivacyService : IService
    {
        event PrivacyShownEventHandler PrivacyShownEvent;

        /// <summary>
        /// 检查是否已经同意过个人信息保护指引
        /// </summary>
        bool HasAgreePrivacy { get; }

        /// <summary>
        /// 展示个人信息保护指引弹窗
        /// 
        /// Event handler: PrivacyShownEvent
        /// </summary>
        /// <param name="content">支持修改正文内容，传入内容即替换征文内容</param>
        void ShowPrivacy(string content = null);
    }
}

