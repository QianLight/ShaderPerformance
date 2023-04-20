

namespace GSDK
{

    /// <summary>
    /// 二维码扫描识别结果回调
    /// </summary>
    /// <param name="result"> 返回的错误码信息 </param>
    /// <para>
    ///     Success: 成功。
    ///     QRCodeNetworkError: 无网络时调用扫码接口
    ///     QRCodeVEAuthError: 鉴权失败(iOS独有错误码)
    /// </para>
    /// <param name="scanResult">扫描二维码识别返回的字符串，需自行处理后续逻辑</param>
    public delegate void ScanQRCodeDelegate(Result result, string scanResult);

    /// <summary>
    /// 用于获取实例进行接口调用
    /// e.g.QRCode.Service.MethodName();
    /// </summary>
    public static class QRCode
    {
        public static IQRCodeService Service
        {
            get{ return ServiceProvider.Instance.GetService(ServiceType.QRCode) as IQRCodeService;}
        }
    }

    public interface IQRCodeService : IService
    {
        /// <summary>
        /// 扫描识别二维码
        /// </summary>
        /// <param name="type">扫码功能类型</param>
        /// <para>
        /// 扫码功能类型：
        ///    1. 扫码返回识别结果。callback中返回扫码结果。回调中的scanResult为识别的字符串，可对该字符串自行处理。
        ///    2. 扫码授权PC登录。内部会实现扫码+授权逻辑，callback中会返回授权结果。回调中的scanResult为授权状态(0:授权;1:拒绝)
        /// </para>
        /// <param name="callback">扫码回调</param>
        /// <para>
        /// 当前接口可能返回的错误码：
        ///     Success: 成功。
        ///     QRCodeNetworkError: 无网络时调用扫码接口
        ///     QRCodeVEAuthError: 鉴权失败(iOS独有错误码)
        /// </para>
        void Scan(QRCodeType type, ScanQRCodeDelegate callback);
    }
    
    #region public defines

    public enum QRCodeType
    {
        /// <summary>
        /// 单纯扫码
        /// </summary>
        None,
        /// <summary>
        /// 扫码登录
        /// </summary>
        AuthLogin  
    }
    
    #endregion

    public static partial class ErrorCode
    {
        /// <summary>
        /// 无网络异常
        /// </summary>
        public static int QRCodeNetworkError = -340001;
        
        /// <summary>
        /// 鉴权失败 (iOS独有错误码)
        /// </summary>
        public static int QRCodeVEAuthError = -340002;

        //扫码授权PC登录
        public const int AccountInvalidQRCode = -103101; //无效二维码
        public const int AccountAuthFailed = -103102;  //授权失败
        public const int AccountGustNotAuth = -103103;  //游客禁止授权
    }
}