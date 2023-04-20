namespace GSDK
{
    /// <summary>
    /// 用于获取实例进行接口调用
    /// e.g.Security.Service.MethodName();
    /// </summary>
    public static class Security
    {
        public static ISecurityService Service
        {
            get { return ServiceProvider.Instance.GetService(ServiceType.Security) as ISecurityService; }
        }
    }
    
    public delegate void FrameCaptureDelegate(Result result, FrameCaptureInfo frameCaptureInfo);
    
    /// <summary>
    /// 截帧工具信息
    /// </summary>
    public class FrameCaptureInfo
    {
        /// <summary>
        /// 是否处于截帧环境
        /// </summary>
        public bool Result;
        
        /// <summary>
        /// 截帧工具细节描述
        /// </summary>
        public string Detail;
        
        /// <summary>
        /// 截帧工具获取失败信息
        /// </summary>
        public string ErrorMsg;
        
        
    }

    public interface ISecurityService : IService
    {
        /// <summary>
        /// 在 登录成功/退出登录/切换登录 之后上报玩家信息，用于安全风控进行游戏安全检查与控制。
        ///
        /// 该接口必须在登录成功/退出登录/切换登录之后调用上报，否则安全检测不通过！
        /// </summary>
        /// <param name="accountType">玩家的类型</param>
        /// <param name="roleId">玩家角色ID</param>
        /// <param name="serverId">玩家区服ID</param>
        void ReportUserInfo(AccountType accountType, string roleId, string serverId);

        /// <summary>
        /// 获取数据
        /// </summary>
        byte[] ReadData();
        
        /// <summary>
        /// 上传数据
        /// </summary>
        /// <returns>
        /// 返回调用结果
        /// 可能涉及的错误码：
        ///     Success:成功
        ///     SecurityWritePacketError：包异常，不能被正确解析
        ///     SecurityMallocPacketError：GP存储服务端数据时本地存储malloc失败
        ///     SecurityInstancePacketError：未open初始化GamePacketChannel
        ///     SecurityDecodeBase64ErrorError：Base64解析失败
        /// </returns>
        Result WriteData(byte[] data);

        /// <summary>
        /// 调整安全客户端占用性能消耗等级
        /// </summary>
        /// <param name="priority">性能消耗等级</param>
        void SetPriority(PerformancePriority priority);
        
        void isFrameCapture(FrameCaptureDelegate callback);

        void isFrameCapture(FrameCaptureDelegate callback, long timeout);

#if UNITY_STANDALONE_WIN && !GMEnderOn
        bool IsGPDllExist();

        void DisableGP();
#endif
    }

    /// <summary>
    /// 性能消耗等级 1~5 = 最低~最高
    /// </summary>
    public enum PerformancePriority
    {
        Lowest = 1,
        BelowNormal,
        Normal,
        AboveNormal,
        Highest
    }
    
    public static partial class ErrorCode
    {
        /// <summary>
        /// 包异常，不能被正确解析
        /// </summary>
        public const int SecurityWritePacketError = -230001;
        
        /// <summary>
        /// GP存储服务端数据时本地存储malloc失败
        /// </summary>
        public const int SecurityMallocPacketError = -230002;
        
        /// <summary>
        /// 未open初始化GamePacketChannel
        /// </summary>
        public const int SecurityInstancePacketError = -230003;
        
        /// <summary>
        /// Base64解析失败
        /// </summary>
        public const int SecurityDecodeBase64ErrorError = -230004;
        
        /// <summary>
        /// Base64解析失败
        /// </summary>
        public const int SecuritySimulatorRecognitionError = -232001;
        
        /// <summary>
        /// Data为空
        /// </summary>
        public const int SecurityDataNullError = -239001;
        
        /// <summary>
        /// 未知错误
        /// </summary>
        public const int SecurityUnknownError = -239999;
    }
}