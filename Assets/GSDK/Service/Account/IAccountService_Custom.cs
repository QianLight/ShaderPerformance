namespace GSDK.CustomAccount
{
    /// <summary>
    /// 用于获取实例进行接口调用
    /// e.g.Account.Service.MethodName();
    /// </summary>
    public static class Account
    {
        public static IAccountCustomService Service
        {
            get
            {
                return ServiceProvider.Instance.GetService(ServiceType.Account,"Custom") as IAccountCustomService;
            }
        }
    }
    
    public interface IAccountCustomService:IAccountService
    {
        /// <summary>
        /// 登录事件
        /// </summary>
        event AccountLoginEventHandler LoginEvent;
        
        /// <summary>
        /// 登录（无UI）
        /// </summary>
        /// <param name="token">登录令牌，通过与服务端的交互获取</param>
        void LoginWithoutUI(string token);
    }
}

namespace GSDK
{
    public partial class ErrorCode
    {
        /// <summary>
        /// 网络错误
        /// </summary>
        public const int AccountCustomNetworkError = -443001;

        /// <summary>
        /// 未知错误
        /// </summary>
        public const int AccountCustomUnknownError = -449999;
    }
}