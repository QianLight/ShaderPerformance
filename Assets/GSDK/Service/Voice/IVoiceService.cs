using GSDK.IMVoice;
using GSDK.ASRVoice;

namespace GSDK
{
    /// <summary>
    /// 用于获取实例进行接口调用
    /// e.g.Voice.Service.MethodName();
    /// </summary>
    public static class Voice
    {
        public static IVoiceService Service
        {
            get { return ServiceProvider.Instance.GetService(ServiceType.Voice) as IVoiceService; }
        }
    }

    /// <summary>
    /// Voice语音消息：提供录制、播放、语音转文字的能力（参考某手机通讯app的发送、播放、语音转文字）。
    /// 
    /// 目前Voice语音消息支持两种服务：IMVoice、ASRVoice。
    ///
    /// IMVoice与ASRVoice的区别：
    /// 使用方面：同时支持各语言转文字；
    /// 性能方面：ASRVoice性能更好；
    /// 兼容性方面：ASRVoice不支持Android 5.0以下的系统。
    /// </summary>
    public interface IVoiceService : IService
    {
        #region Methods

        IIMVoice IMVoice { get; }

        IASRVoice ASRVoice { get; }

        #endregion
    }

    /// <summary>
    /// IMVoice可能出现的错误码
    /// </summary>
    public static partial class ErrorCode
    {
        /// <summary>
        /// 用户没有授权（永久拒绝）
        /// </summary>
        public const int VoiceIMMicrophoneNoPermission = -110101;

        /// <summary>
        /// 用户没有授权（单次拒绝）
        /// </summary>
        public const int VoiceIMMicrophoneNoPermissionWithoutPromot = -112103;

        /// <summary>
        /// 用户刚通过授权，先调用StopRecord结束录制，再引导用户再次点击录制
        /// </summary>
        public const int VoiceIMUserJustGrant = -110102;

        /// <summary>
        /// 录制声音过短
        /// </summary>
        public const int VoiceIMRecordTooShort = -110103;

        /// <summary>
        /// 播放失败
        /// </summary>
        public const int VoiceIMPlayFailed = -110104;

        /// <summary>
        /// 下载失败
        /// </summary>
        public const int VoiceIMDownloadFailed = -110105;

        /// <summary>
        /// 通用：命中敏感信息风控
        /// </summary>
        public const int VoiceIMShark = -110106;

        /// <summary>
        /// 上传失败
        /// </summary>
        public const int VoiceIMUploadFailed = -110107;

        /// <summary>
        /// 系统内部错误
        /// </summary>
        public const int VoiceIMSystemInternalError = -111101;

        /// <summary>
        /// 录制失败
        /// </summary>
        public const int VoiceIMRecordFailed = -111102;

        /// <summary>
        /// SDK内部错误
        /// </summary>
        public const int VoiceIMSDKInternalError = -111103;

        /// <summary>
        /// VoiceID 无效
        /// </summary>
        public const int VoiceIMVoiceIDInvalid = -111104;

        /// <summary>
        /// 语音解密失败
        /// </summary>
        public const int VoiceIMDecryptFailed = -111105;

        /// <summary>
        /// 录制语音消息出错
        /// </summary>
        public const int VoiceIMRecordError = -112101;

        /// <summary>
        /// 未初始化语音消息模块 
        /// </summary>
        public const int VoiceIMVoiceUnInitialized = -112102;
        
        /// <summary>
        /// 网络错误
        /// </summary>
        public const int VoiceNetWorkError = -113001;
        
        /// <summary>
        /// 未知错误
        /// </summary>
        public const int VoiceUnknownError = -119999;
    }

    /// <summary>
    /// Vtranlate(在线)语音转文字可能出现的错误码
    /// </summary>
    public static partial class ErrorCode
    {
        /// <summary>
        /// 参数错误
        /// </summary>
        public const int VtranslateParamsError = -110201;

        /// <summary>
        /// Token错误
        /// </summary>
        public const int VtranslateTokenError = -110202;

        /// <summary>
        /// VoiceID 错误
        /// </summary>
        public const int VtranslateVoiceIDError = -110203;

        /// <summary>
        /// 翻译语言错误
        /// </summary>
        public const int VtranslateLanguageError = -110204;

        /// <summary>
        /// 服务器错误
        /// </summary>
        public const int VtranslateServerError = -110205;

        /// <summary>
        /// 语音转文字其他错误
        /// </summary>
        public const int VtranslateOtherError = 110206;

        /// <summary>
        /// 命中敏感信息风控
        /// </summary>
        public const int VtranslateShark = 110207;
    }

    /// <summary>
    /// ASRVoice（本地语音转文字）可能出现的错误码
    /// </summary>
    public static partial class ErrorCode
    {
        /// <summary>
        /// ASR初始化失败
        /// </summary>
        public const int VoiceASRInitializeFailed = -110001;

        /// <summary>
        /// 未初始化
        /// </summary>
        public const int VoiceASRUnInitialized = -110002;

        /// <summary>
        /// 麦克风被占用
        /// </summary>
        public const int VoiceASRMicrophoneOccupied = -110003;

        /// <summary>
        /// Json 解析异常
        /// </summary>
        public const int VoiceASRJsonException = -110004;

        /// <summary>
        /// ASR引擎异常
        /// </summary>
        public const int VoiceASREngineException = -110005;

        /// <summary>
        /// 开始录制失败
        /// </summary>
        public const int VoiceASRStartRecordFailed = -110006;

        /// <summary>
        /// 结束录制失败
        /// </summary>
        public const int VoiceASRStopRecordFailed = -110007;

        /// <summary>
        /// 取消录制失败
        /// </summary>
        public const int VoiceASRCancelRecordFailed = -110008;

        /// <summary>
        /// 云平台异常
        /// </summary>
        public const int VoiceASRCloudPlatformException = -110009;

        /// <summary>
        /// 用户刚通过授权，需要引导用户再次点击录制
        /// </summary>
        public const int VoiceASRJustGrant = -111001;

        /// <summary>
        /// 用户没有授权麦克风（永久拒绝）
        /// </summary>
        public const int VoiceASRMicrophoneNoPermission = -110010;

        /// <summary>
        /// 用户没有授权麦克风（单次拒绝）
        /// </summary>
        public const int VoiceASRMicrophoneNoPermissionWithoutPromot = -112011;
    }
}