namespace GSDK
{
    #region static service class

    public static class RealName
    {
        public static IRealNameService Service
        {
            get { return ServiceProvider.Instance.GetService(ServiceType.RealName) as IRealNameService; }
        }
    }

    #endregion

    #region delegates

    /// <summary>
    /// 获取实名状态结果回调
    /// </summary>
    /// <param name="result"></param>
    /// <param name="realNameState">实名状态</param>
    /// <para>
    /// 	可能返回的错误码：
    /// 		Success									成功
    /// 		AccountParameterError					检查吊起登录参数，检查是否初始化完成
    /// 		AccountTokenError						token异常、过期、不匹配
    /// 		AccountServerError						服务器异常
    /// 		AccountNetError							网络错误
    /// 		AccountGServerError						gsdk server 其他错误
    /// 		AccountGClientError						客户端其他错误
    /// 		AccountNotLoggedIn 						未登录错误
    /// </para>
    public delegate void FetchRealNameStateDelegate(Result result, RealNameState realNameState);

    /// <summary>
    /// 获取实名信息结果回调
    /// </summary>
    public delegate void FetchRealNameInfoDelegate(Result result, RealNameInfo realNameInfo);

    /// <summary>
    /// 实名认证结果回调
    /// </summary>
    /// <param name="code">实名认证结果错误码，code=0的时候实名认证成功</param>
    /// <param name="message">实名认证结果错误信息</param>
    public delegate void ComplianceRealNameAuthResultDelegate(int code, string message);

    #endregion

    #region IRealNameService

    public interface IRealNameService : IService
    {
        #region 接口

        /// <summary>
        /// 【无UI】查询实名状态
        /// </summary>
        /// <param name="fetchRealNameStateCallback"></param>
        void FetchRealNameState(FetchRealNameStateDelegate fetchRealNameStateCallback);

        /// <summary>
        /// 【无UI】查询实名信息
        /// </summary>
        void FetchRealNameInfo(FetchRealNameInfoDelegate fetchRealNameInfoCallback);

        /// <summary>
        /// 【有UI】启动实名认证
        /// </summary>
        /// <param name="realNameResultCallback">实名认证结果回调</param>
        void ComplianceRealNameAuth(ComplianceRealNameAuthResultDelegate realNameResultCallback);

        #endregion
    }

    #endregion

    public class RealNameState
    {
        /// <summary>
        /// 是否实名
        /// </summary>
        public bool IsVerified;
        /// <summary>
        /// 是否需要加验监护人
        /// </summary>
        public bool NeedParentVerify;
    }

    public class RealNameInfo
    {
        /// <summary>
        /// 是否实名
        /// </summary>
        public bool IsVerified;
        
        /// <summary>
        /// 年龄，仅外渠存在
        /// </summary>
        public int Age;
        /// <summary>
        /// 真实姓名，仅官渠存在
        /// </summary>
        public string RealName;

        /// <summary>
        /// 身份证号码，仅官渠存在
        /// </summary>
        public string IdentityNumber;
    }
}