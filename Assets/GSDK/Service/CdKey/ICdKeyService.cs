namespace GSDK
{
    /// <summary>
    /// 礼包码兑换的回调
    /// </summary>
    /// <param name="result">回调结果</param>
    /// <para>
    ///    当前接口可能返回的错误码:
    ///             Success:成功
    ///             CdKeyParamsError:未登录或参数错误
    ///             CdKeyIsUsed:CdKey已经被使用过了
    ///             CdKeyIsSameBatch:已经领取过同一批次的CdKey
    ///             CdKeyNotStarted:CdKey未开放领取
    ///             CdKeyAlreadyEnded:CdKey领取已经结束
    ///             CdKeyNotExist:CdKey不存在
    ///             CdKeyShark:命中风控
    ///             CdKeyNotIssued:CdKey尚未分发
    ///             CdKeyUseConditionFail:条件检验失败（实时人群条件检验不通过）
    /// 
    ///     出现以下错误码请联系中台同学:
    ///             CdKeyServerError:服务器内部错误
    ///             CdKeyValidateError:用户校验失败
    ///             CdKeyUseError:CdKey使用失败
    ///             CdKeyTemporaryError:临时错误
    ///             CdKeyUnknownError:未知错误
    /// </para>
    public delegate void CdKeyDelegate(Result result);

    /// <summary>
    /// 用于获取实例进行接口调用
    /// e.g.CdKey.Service.MethodName();
    /// </summary>
    public static class CdKey
    {
        public static ICdKeyService Service
        {
            get { return ServiceProvider.Instance.GetService(ServiceType.CdKey) as ICdKeyService; }
        }
    }
    
    public interface ICdKeyService : IService
    {
        #region Methods

        /// <summary>
        /// 礼包码兑换，礼包码需要在营销后台配置
        /// </summary>
        /// <param name="roleId">角色Id</param>
        /// <param name="serverId">区服Id</param>
        /// <param name="cdKey">礼包码</param>
        /// <param name="callback">回调</param>
        void RedeemGift(string roleId, int serverId, string cdKey, CdKeyDelegate callback);

        #endregion
    }

    public partial class ErrorCode
    {
        /// <summary>
        /// 未登录或参数错误
        /// </summary>
        public const int CdKeyParamsError =-400001;
        
        /// <summary>
        /// 服务器内部错误
        /// </summary>
        public const int CdKeyServerError = -400002;
        
        /// <summary>
        /// 用户校验失败
        /// </summary>
        public const int CdKeyValidateError = -400003;
        
        /// <summary>
        /// CdKey使用失败
        /// </summary>
        public const int CdKeyUseError = -400004;

        /// <summary>
        /// 临时错误
        /// </summary>
        public const int CdKeyTemporaryError = -400005;

        /// <summary>
        /// CdKey已经被使用过了
        /// </summary>
        public const int CdKeyIsUsed = -400006;

        /// <summary>
        /// 已经领取过同一批次的CdKey
        /// </summary>
        public const int CdKeyIsSameBatch = -400007;

        /// <summary>
        /// CdKey未开放领取
        /// </summary>
        public const int CdKeyNotStarted = -400008;

        /// <summary>
        /// CdKey领取已经结束
        /// </summary>
        public const int CdKeyAlreadyEnded = -400009;

        /// <summary>
        /// CdKey不存在
        /// </summary>
        public const int CdKeyNotExist = -400010;

        /// <summary>
        /// 命中风控
        /// </summary>
        public const int CdKeyShark = -400011;

        /// <summary>
        /// CDKEY尚未分发
        /// </summary>
        public const int CdKeyNotIssued = -400012;

        /// <summary>
        /// 条件检验失败（实时人群条件检验不通过）
        /// </summary>
        public const int CdKeyUseConditionFail = -400013;

        /// <summary>
        /// 网络错误
        /// </summary>
        public const int CdKeyNetWorkError = -403001; 
        /// <summary>
        /// 未知错误
        /// </summary>
        public const int CdKeyUnknownError = -409999;
    }
}