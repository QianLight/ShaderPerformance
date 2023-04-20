using System.Collections.Generic;

//ASRVoice不支持Android 5.0以下的系统
namespace GSDK.ASRVoice
{
    #region ASRVoice Delegate

    /// <summary>
    /// 初始化的回调。
    /// </summary>
    /// <param name="result">回调结果</param>
    /// <para>
    ///    当前接口可能返回的错误码:
    ///             Success:成功
    ///             VoiceASRInitializeFailed:初始化失败
    ///             VoiceASRCloudPlatformException:云平台异常
    ///             VoiceASRUnknownError:未知错误
    /// </para>
    public delegate void InitializeDelegate(Result result);
    
    /// <summary>
    /// 开始录制的回调。
    /// </summary>
    /// <param name="result">回调结果</param>
    /// <para>
    ///    当前接口可能返回的错误码:
    ///             Success:成功
    ///             VoiceASRUnInitialized:未初始化
    ///             VoiceASRStartRecordFailed:开始录制失败
    ///             VoiceASRJustGrant:用户刚通过授权，需要引导用户再次点击录制
    ///             VoiceASRMicrophoneNoPermission:用户没有授权麦克风
    ///             VoiceASRUnknownError:未知错误
    /// </para>
    public delegate void StartRecordDelegate(Result result);

    /// <summary>
    /// 结束录制的回调。
    /// </summary>
    /// <param name="result">回调结果</param>
    /// <para>
    ///    当前接口可能返回的错误码:
    ///             Success:成功
    ///             VoiceASRUnInitialized:未初始化
    ///             VoiceASRStopRecordFailed:结束录制失败
    ///             VoiceASRUnknownError:未知错误
    /// </para>
    public delegate void StopRecordDelegate(Result result);

    /// <summary>
    /// 取消录制的回调。
    /// </summary>
    /// <param name="result">回调结果</param>
    /// <para>
    ///    当前接口可能返回的错误码:
    ///             Success:成功
    ///             VoiceASRUnInitialized:未初始化
    ///             VoiceASRCancelRecordFailed:取消录制失败
    ///             VoiceASRUnknownError:未知错误
    /// </para>
    public delegate void CancelRecordDelegate(Result result);

    #endregion

    #region ASRVoice EventHandler
    
    /// <summary>
    /// 引擎出现错误的事件。
    /// </summary>
    /// <param name="result">事件的结果信息</param>
    /// <para>
    ///    当前接口可能返回的错误码:
    ///             VoiceASRMicrophoneOccupied:麦克风被占用
    ///             VoiceASREngineException:ASR引擎异常，会伴随着二级错误码
    ///             VoiceASRUnknownError:未知错误
    /// </para>
    public delegate void EngineErrorEventHandler(Result result);

    /// <summary>
    /// 开始播放语音消息的事件。
    /// </summary>
    /// <param name="result">事件的结果信息</param>
    /// <param name="playInfo">开始播放语音消息的信息</param>
    /// <para>
    ///    当前接口可能返回的错误码:
    ///             Success:成功
    ///             VoiceASRPlayFailed:播放失败
    ///             VoiceASRDownloadFailed:下载失败
    ///             VoiceASRUnknownError:未知错误
    /// </para>
    public delegate void StartPlayEventHandler(Result result, PlayInfo playInfo);

    /// <summary>
    /// 结束播放语音消息的事件。
    /// </summary>
    /// <param name="playInfo">结束播放语音消息的语音</param>
    public delegate void FinishPlayEventHandler(PlayInfo playInfo);

    /// <summary>
    /// 正在语音转文字的事件（ASR是边录制边语音转文字的模式，该事件在录制时会实时触发）。
    /// </summary>
    /// <param name="transcribingInfo">语音转文字中的信息（从录制开始当当前时刻的语音转文字结果）</param>
    public delegate void TranscribingEventHandler(TranscribingInfo transcribingInfo);

    /// <summary>
    /// 完成语音转文字的事件（整段语音的语音转文字，可能和部分语音转文字的结果有出入，完成录制后会触发该事件）。
    /// </summary>
    /// <param name="finishTranscribeInfo">完成语音转文字的信息</param>
    public delegate void FinishTranscribeEventHandler(FinishTranscribeInfo finishTranscribeInfo);

    /// <summary>
    /// 完成语音转文字并进行上传的事件（该事件在完成语音转文字后触发，如果上传成功会有语音消息ID和语音时长的信息）。
    /// </summary>
    /// <param name="result">事件的结果信息</param>
    /// <param name="finishTranscribeAndUploadInfo">完成语音转文字并进行上传后得到的信息</param>
    /// <para>
    ///    当前接口可能返回的错误码:
    ///             Success:成功
    ///             VoiceASRJsonException:Json解析异常
    ///             VoiceASRUploadFailed:上传失败
    ///             VoiceASRUnknownError:未知错误
    /// </para>
    public delegate void FinishTranscribeAndUploadEventHandler(Result result,
        FinishTranscribeAndUploadInfo finishTranscribeAndUploadInfo);

    /// <summary>
    /// 通知录音音量的事件（该事件在录制时，实时触发）。
    /// </summary>
    /// <param name="volume">录制音量，范围：[0-1.0]</param>
    public delegate void NotifyRecordVolumeEventHandler(double volume);

    #endregion

    public interface IASRVoice
    {
        #region Events

        /// <summary>
        /// 调用StartRecord之后，ASR引擎若运行出现错误则会触发该事件
        /// </summary>
        event EngineErrorEventHandler EngineErrorEvent; 

        /// <summary>
        /// 调用StartPlay之后，每条语音播放时都会触发该事件
        /// </summary>
        event StartPlayEventHandler StartPlayEvent; 

        /// <summary>
        /// 调用StopPlay或每条语音播放完毕后，会触发该事件
        /// </summary>
        event FinishPlayEventHandler FinishPlayEvent; 

        /// <summary>
        /// 调用StartRecord之后，会进行实时语音转文字，触发该事件
        /// </summary>
        event TranscribingEventHandler TranscribingEvent; 

        /// <summary>
        /// 调用StopRecord之后触发该事件，进行完整的语音转文字
        /// </summary>
        event FinishTranscribeEventHandler FinishTranscribeEvent; 

        /// <summary>
        /// 调用StopRecord之后触发该事件，进行完整的语音转文字并且上传
        /// </summary>
        event FinishTranscribeAndUploadEventHandler FinishTranscribeAndUploadEvent; 

        /// <summary>
        /// 调用StartRecord之后，实时触发该事件，通知当前的录制音量
        /// </summary>
        event NotifyRecordVolumeEventHandler NotifyRecordVolumeEvent; 

        #endregion

        #region Methods

        /// <summary>
        /// 初始化ASRVoice（使用ASRVoice必调，推荐在Start中调用）。
        /// </summary>
        /// <param name="playMode">播放模式</param>
        /// <param name="language">目标语言</param>>
        /// <param name="callback">初始化的回调</param>
        void Initialize(PlayMode playMode, InitializeDelegate callback, TargetLanguage language = TargetLanguage.Chinese);

        /// <summary>
        /// 释放ASRVoice资源（使用ASRVoice必调，推荐在OnDestroy中调用）。
        /// </summary>
        void Release();

        /// <summary>
        /// 开始录制语音消息。
        /// </summary>
        /// <param name="startRecordDelegate">开始录制的回调</param>
        void StartRecord(StartRecordDelegate startRecordDelegate);

        /// <summary>
        /// 停止录制语音消息。
        /// </summary>
        /// <param name="stopRecordDelegate">停止录制的回调</param>
        void StopRecord(StopRecordDelegate stopRecordDelegate);

        /// <summary>
        /// 取消录制语音消息。
        /// </summary>
        /// <param name="cancelRecordDelegate">取消录制的回调</param>
        void CancelRecord(CancelRecordDelegate cancelRecordDelegate);

        /// <summary>
        /// 开始播放语音消息。
        /// </summary>
        /// <param name="voiceIDs">语音消息列表</param>
        void StartPlay(List<string> voiceIDs);

        /// <summary>
        /// 停止播放语音消息。
        /// </summary>
        void StopPlay();

        #endregion
    }

    #region PublicDefinitions ASRVoice使用的所有定义

    /// <summary>
    /// 播放模式
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

    public enum TargetLanguage
    {
        /// <summary>
        /// 目标语言为中文（默认）
        /// </summary>
        Chinese,

        /// <summary>
        /// 目标语言为英文
        /// </summary>
        English,
    }

    /// <summary>
    /// 播放信息
    /// </summary>
    public struct PlayInfo
    {
        /// <summary>
        /// 语音消息ID
        /// </summary>
        public string VoiceID;
    }

    /// <summary>
    /// 语音转文字进行中的信息
    /// </summary>
    public struct TranscribingInfo
    {
        /// <summary>
        /// 部分语音转文字的结果（实时语音转文字，从录制开始到当前时刻的语音转文字结果）
        /// </summary>
        public string TranscribingText;
    }

    /// <summary>
    /// 完成语音转文字的信息
    /// </summary>
    public struct FinishTranscribeInfo
    {
        /// <summary>
        /// 完成语音转文字的结果（最终结果会进行调整，与实时语音转文字结果有差别）
        /// </summary>
        public string FinishTranscribeText;
    }

    /// <summary>
    /// 完成语音转文字并上传的信息
    /// </summary>
    public struct FinishTranscribeAndUploadInfo
    {
        /// <summary>
        /// 完成语音转文字的结果
        /// </summary>
        public string FinishTranscribeText;

        /// <summary>
        /// 上传成功后的语音ID（若失败则为null）
        /// </summary>
        public string VoiceID;

        /// <summary>
        /// 上传成功后的语音时长（若失败则为0）
        /// </summary>
        public long VoiceTime;
    }

    #endregion
}