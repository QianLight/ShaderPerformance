using System.Collections.Generic;

namespace GSDK.IMVoice
{
    #region IMVoice Delegate

    /// <summary>
    /// 开始录制的回调(录制音频时间上限60s，当时间到达60s时会自动完成录制)。
    /// </summary>
    /// <param name="result">回调结果</param>
    /// <para>
    ///    当前接口可能返回的错误码:
    ///             Success:成功
    ///             VoiceIMMicrophoneNoPermission:用户没有授权（永久拒绝）
    ///             VoiceIMMicrophoneNoPermissionWithoutPromot:用户没有授权(单次拒绝)
    ///             VoiceIMSystemInternalError:系统内部错误
    ///             VoiceIMUserJustGrant:用户刚通过授权，先调用StopRecord结束录制，再引导用户再次点击录制
    ///             VoiceUnknownError:录制未知错误
    /// </para>
    public delegate void StartRecordDelegate(Result result);


    /// <summary>
    /// 停止录制不自动上传的回调
    /// </summary>
    /// <param name="result">回调结果</param>
    /// <para>
    ///             Success:成功
    ///             VoiceIMUploadFailed:上传语音失败
    ///             VoiceIMRecordTooShort:录制声音过短
    ///             VoiceIMSDKInternalError:SDK内部错误
    ///             VoiceUnknownError:录制未知错误
    /// </para>
    /// <param name="finishRecordNotUploadInfo">录音文件信息，包括路径和时长</param>
    public delegate void StopRecordNotUploadDelegate(Result result, FinishRecordNotUploadInfo finishRecordNotUploadInfo);

    /// <summary>
    /// 上传语音文件的回调
    /// </summary>
    /// <param name="result">回调结果</param>
    /// <para>
    ///             Success:成功
    ///             VoiceIMUploadFailed:上传语音失败
    /// </para>
    /// <param name="playInfo">上传结果</param>
    public delegate void UploadVoiceByUniqueIdDelegate(Result result, PlayInfo playInfo);

    /// <summary>
    /// 语音转文字的回调。
    /// </summary>
    /// <param name="result">回调结果</param>
    /// <para>
    ///    当前接口可能返回的错误码:
    ///             Success:成功
    ///             VtranslateParamsError:参数错误
    ///             VtranslateTokenError:token错误
    ///             VtranslateVoiceIDError:VoiceID 错误
    ///             VtranslateLanguageError:语言错误
    ///             VtranslateServerError:服务器错误
    ///             VoiceNetWorkError:网络错误
    ///             VtranslateShark:命中敏感信息风控
    ///             VtranslateOtherError:语音转文字其他错误
    /// </para>
    /// <param name="transcribeInfo">语音转文字结果</param>
    public delegate void TranscribeDelegate(Result result, TranscribeInfo transcribeInfo);

    /// <summary>
    /// 语音转文字的回调。
    /// </summary>
    /// <param name="result">回调结果</param>
    /// <para>
    ///    当前接口可能返回的错误码:
    ///             Success:成功
    ///             VtranslateParamsError:参数错误
    ///             VtranslateTokenError:token错误
    ///             VtranslateLanguageError:语言错误
    ///             VtranslateServerError:服务器错误
    ///             VoiceNetWorkError:网络错误
    ///             VtranslateShark:命中敏感信息风控
    ///             VtranslateOtherError:语音转文字其他错误
    /// </para>
    /// <param name="transcribeLocalInfo">语音转文字结果</param>
    public delegate void TranscribeLocalDelegate(Result result, TranscribeLocalInfo transcribeLocalInfo);

    /// <summary>
    /// 获取到语音消息本地路径结果的回调。
    /// </summary>
    /// <param name="result">回调的结果</param>
    /// <para>
    ///    当前接口可能返回的错误码:
    ///             Success:成功
    ///             VoiceIMVoiceIDInvalid:VoiceID 无效
    ///             VoiceIMDownloadFailed:语音下载失败
    ///             VoiceIMShark:命中敏感信息风控
    ///             VoiceUnknownError:未知错误
    /// </para>
    /// <param name="fileInfo">语音消息文件信息</param>
    public delegate void FetchLocalPathDelegate(Result result, VoiceFileInfo fileInfo);

    #endregion

    #region IMVoice EventHandler

    /// <summary>
    /// 完成录制的事件。
    /// </summary>
    /// <param name="result">事件的结果信息</param>
    /// <para>
    ///    当前接口可能返回的错误码:
    ///             Success:成功
    ///             VoiceIMUploadError:上传语音失败
    ///             VoiceIMRecordTooShort:录制声音过短
    ///             VoiceIMSdkInternalError:SDK内部错误
    ///             VoiceIMRecordUnknownError:录制未知错误
    /// </para>
    /// <param name="finishRecordInfo">完成录制后返回的信息</param>
    public delegate void FinishRecordEventHandler(Result result, FinishRecordInfo finishRecordInfo);

    /// <summary>
    /// 开始播放的事件。
    /// </summary>
    /// <param name="result">事件的结果信息</param>
    /// <para>
    ///    当前接口可能返回的错误码:
    ///             Success:成功
    ///             VoiceIMVoiceIDInvalid:VoiceID 无效
    ///             VoiceIMDecryptFailed:语音解密失败
    ///             VoiceIMDownloadFailed:语音下载失败
    ///             VoiceIMShark:命中敏感信息风控
    ///             VoiceIMPlayVoiceUnknownError:播放未知错误
    /// </para>
    /// <param name="playInfo">开始播放的信息</param>
    public delegate void StartPlayEventHandler(Result result, PlayInfo playInfo);

    /// <summary>
    /// 完成播放的事件。
    /// </summary>
    /// <param name="playInfo">结束播放的信息</param>
    public delegate void FinishPlayEventHandler(PlayInfo playInfo);

    /// <summary>
    /// 倒计时的事件（开始录制后，当剩余时间只有10s时，每秒触发一次事件）。
    /// </summary>
    /// <param name="leftSeconds">剩余的秒数，单位s</param>
    public delegate void CountDownEventHandler(int leftSeconds);

    /// <summary>
    /// 通知当前的录制音量的事件(每100ms通知一次，录制音量是指传入麦克风的声音大小)。
    /// </summary>
    /// <param name="volume">录制音量，范围：[0,1.0]</param>
    public delegate void NotifyRecordVolumeEventHandler(double volume);

    /// <summary>
    /// 录制时间超出的事件（录制音频上限60s）。
    /// </summary>
    public delegate void RecordTimeExceedEventHandler();

    #endregion

    public interface IIMVoice
    {
        #region Events

        /// <summary>
        /// 完成录制后触发该事件
        /// </summary>
        event FinishRecordEventHandler FinishRecordEvent;
        
        /// <summary>
        /// 开始播放后，每条语音播放时都会触发该事件
        /// </summary>
        event StartPlayEventHandler StartPlayEvent;
        
        /// <summary>
        /// 完成播放后触发该事件
        /// </summary>
        event FinishPlayEventHandler FinishPlayEvent;
        
        /// <summary>
        /// 录制时间还剩10s时，每秒触发一次该时间
        /// </summary>
        event CountDownEventHandler CountDownEvent;
        
        /// <summary>
        /// 录制时实时触发该事件
        /// </summary>
        event NotifyRecordVolumeEventHandler NotifyRecordVolume;
        
        /// <summary>
        /// 录制时间超出时触发该事件
        /// </summary>
        event RecordTimeExceedEventHandler RecordTimeExceedEvent;

        #endregion

        #region Methods

        /// <summary>
        /// 初始化IMVoice（使用IMVoice必调，推荐在Start中调用）。
        /// </summary>
        /// <param name="playMode">播放模式，有默认模式和扬声器模式</param>
        /// <param name="expireTime">本地语音文件失效时间，单位为秒(s)，默认30天</param>
        /// <param name="savePath">语音文件缓存路径，安卓需要设置，IOS无需设置（如果不设值会使用默认的内部存储路径）</param>
        void Initialize(PlayMode playMode, int expireTime = 30 * 24 * 60 * 60,
            string savePath = "");

        /// <summary>
        /// 释放IMVoice资源（使用IMVoice必调，推荐在OnDestroy中调用）。
        /// </summary>
        void Release();

        /// <summary>
        /// 开始录制语音消息。
        /// </summary>
        /// <param name="startRecordCallback">开始录制的回调</param>
        /// <param name="language">录制使用的语种</param>
        /// <param name="language">录制所在的地区，用于精准匹配</param>
        void StartRecord(StartRecordDelegate startRecordCallback, string language = null, string region = null);

        /// <summary>
        /// 结束录制语音消息并上传。
        /// </summary>
        void StopRecord();

        /// <summary>
        /// 结束录制语音消息不上传。
        /// </summary>
        /// <param name="stopRecordNotUploadCallback">结束录制语音并不上传回调</param>
        void StopRecordNotUpload(StopRecordNotUploadDelegate stopRecordNotUploadCallback);


        /// <summary>
        /// 上传语音消息
        /// </summary>
        /// <param name="uniqueId">文件路径</param>
        /// <param name="callback">上传语音文件的回调</param>
        void UploadVoiceByUniqueId(string uniqueId, UploadVoiceByUniqueIdDelegate callback);

        /// <summary>
        /// 取消录制语音消息。
        /// </summary>
        void CancelRecord();

        /// <summary>
        /// 播放单个语音消息
        /// </summary>
        /// <param name="voiceID">语音ID</param>
        void StartPlay(string voiceID);

        /// <summary>
        /// 播放语音消息。
        /// </summary>
        /// <param name="voiceIDs">语音消息列表</param>
        void StartPlay(List<string> voiceIDs);

        /// <summary>
        /// 停止播放语音消息。
        /// </summary>
        void StopPlay();

        /// <summary>
        /// 语音消息转文字。
        /// </summary>
        /// <param name="voiceID">语音消息ID</param>
        /// <param name="callback">语音消息转文字的回调</param>
        /// <param name="targetLanguage">目标语言</param>
        void Transcribe(string voiceID, TranscribeDelegate callback, string targetLanguage = null);

        /// <summary>
        /// 本地语音消息转文字。
        /// </summary>
        /// <param name="voicePath">语音消息路径</param>
        /// <param name="callback">本地语音消息转文字的回调</param>
        /// <param name="voiceLanguage">语音消息语言</param>
        /// <param name="translateLanguage">目标语言，传空使用SDK语言</param>
        /// <param name="region">录音语言区域，用于辅助录音语言的精准匹配，传空使用设备系统区域</param>
        void TranscribeLocal(string voicePath, TranscribeLocalDelegate callback, string voiceLanguage, string translateLanguage = null, string region = null);

        /// <summary>
        /// 根据voiceId获取语音文件本地路径。
        /// 如果本地不存在该voiceId对应的语音消息文件，会首先去服务端拉取对应文件
        /// </summary>
        /// <param name="voiceID">语音消息ID</param>
        /// <param name="callback">获取消息路径的回调</param>
        void FetchLocalPath(string voiceID, FetchLocalPathDelegate callback);

        #endregion
    }

    #region PublicDefinitions IMVoice使用的所有定义

    /// <summary>
    /// 语音消息播放模式
    /// </summary>
    public enum PlayMode
    {
        /// <summary>
        /// 默认模式：遮挡传感器时，使用听筒播放；取消遮挡传感器时，使用扬声器播放
        /// </summary>
        Default,

        /// <summary>
        /// 扬声器模式：始终使用扬声器播放
        /// </summary>
        Speaker,
    }

    /// <summary>
    /// 完成录制信息
    /// </summary>
    public struct FinishRecordInfo
    {
        /// <summary>
        /// 语音消息ID
        /// </summary>
        public string VoiceID;

        /// <summary>
        /// 语音消息的时长，单位ms
        /// </summary>
        public long VoiceDuration;
    }

    /// <summary>
    /// 完成录制不上传信息
    /// </summary>
    public struct FinishRecordNotUploadInfo
    {
        /// <summary>
        /// 文件路径
        /// </summary>
        public string UniqueId;

        /// <summary>
        /// 语音消息的时长，单位ms
        /// </summary>
        public long VoiceDuration;
    }

    /// <summary>
    /// 语音播放信息
    /// </summary>
    public struct PlayInfo
    {
        /// <summary>
        /// 语音消息ID
        /// </summary>
        public string VoiceID;
    }

    /// <summary>
    /// 语音转文字的结果
    /// </summary>
    public struct TranscribeInfo
    {
        /// <summary>
        /// 语音消息ID
        /// </summary>
        public string VoiceID;

        /// <summary>
        /// 语音消息文字内容
        /// </summary>
        public string VoiceContent;
    }

    /// <summary>
    /// 本地语音转文字的结果
    /// </summary>
    public struct TranscribeLocalInfo
    {
        /// <summary>
        /// 语音消息文字内容
        /// </summary>
        public string VoiceContent;
    }

    /// <summary>
    /// 语音消息文件信息
    /// </summary>
    public struct VoiceFileInfo
    {
        /// <summary>
        /// 语音消息ID
        /// </summary>
        public string VoiceID;

        /// <summary>
        /// 语音消息的本地路径
        /// </summary>
        public string VoiceLocalFilePath;
    }
    
    #endregion
}
