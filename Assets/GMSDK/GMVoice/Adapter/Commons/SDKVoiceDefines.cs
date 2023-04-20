using System.Collections.Generic;

namespace GMSDK
{
    public class VoiceMethodName
    {
        public const string Init = "registerVoice";
        public const string InitASR = "registerASR";

        public const string InitIM = "requestInitIM";
        public const string IMStartRecord = "requestStartRecord";
        public const string IMStopRecord = "requestStopRecord";
        public const string IMStopRecordNotUpload = "requestStopRecordWithoutUpload";
        public const string IMUploadVoice = "requestUploadVoiceFile";
        public const string IMCancelRecord = "requestCancelRecord";
        public const string IMStartPlay = "requestStartPlay";
        public const string IMStartPlaySingleVoice = "requestStartPlaySingleVoice";
        public const string IMStopPlay = "requestStopPlay";
        public const string IMRelease = "requestRelease";
        public const string IMTranslate = "requestTranslate";
		public const string IMFetchLocalPath = "requestFetchLocalPath";
        public const string IMTranslateLocal = "requestTranslateLocal";

        public const string ASRInit = "requestASRInit";
        public const string ASRStartRecord = "requestASRStartRecord";
        public const string ASRStopRecord = "requestASRStopRecord";
        public const string ASRCancelRecord = "requestASRCancelRecord";
        public const string ASRUnInit = "requestASRUnInit";
        public const string ASRStartPlay = "requestASRStartPlayVoice";
        public const string ASRStopPlay = "requestASRStopPlayVoice";
    }

    public class VoiceResultName
    {
        public const string IMStartPlayResult = "requestStartPlayResult";
        public const string IMFinishPlayResult = "requestFinishPlayResult";
        public const string IMStartRecordResult = "requestStartRecordResult";
        public const string IMFinishRecordResult = "requestFinishRecordResult";
        public const string IMFinishRecordNotUploadResult = "requestFinishRecordWithoutUploadResult";
        public const string IMNotifyLeftSecondResult = "requestNotifyLeftSecondResult";
        public const string IMNotifyVolumeResult = "requestNotifyVolumeResult";
        public const string IMExceedLimitTimeResult = "requestExceedLimitTimeResult";
		public const string IMFetchLocalPathResult = "requestFetchLocalPathResult";

        public const string ASRInitResult = "requestInitResult";
        public const string ASRStartEngineResult = "requestStartEngineResult";
        public const string ASRStopEngineResult = "requestStopEngineResult";
        public const string ASRErrorEngineResult = "requestEngineErrorResult";
        public const string ASRStartRecordResult = "requestStartRecordResult";
        public const string ASRStopRecordResult = "requestStopRecordResult";
        public const string ASRCancelRecordResult = "requestCancelRecordResult";
        public const string ASRPartialTranslateResult = "requestPartialTranslateResult";
        public const string ASRTranslateResult = "requestTranslateResult";
        public const string ASRFinalTranslateResult = "requestFinalTranslateResult";
        public const string ASRVolumeLevelResult = "requestVolumeLevelResult";

    }

    // 开始播放IM语音回调信息
    public class StartPlayResult : CallbackResult
    {
        // 语音Id
        public string voiceId;
    }
    // 结束播放IM语音回调信息
    public class FinishPlayResult : CallbackResult
    {
        public string voiceId;
    }

    // 结束录制IM语音回调信息
    public class FinishRecordResult : CallbackResult
    {
        public string voiceId;
        public long voiceDuration; //语音消息的时长，单位ms
    }

    // 结束录制IM语音不上传回调信息
    public class FinishRecordNotUploadResult : CallbackResult
    {
        public string uniqueId; //语音文件路径
        public long voiceDuration;
    }

    // 上传IM语音的回调
    public class UploadVoiceResult : CallbackResult
    {
        public string voiceId;
    }

    public class NotifyLeftSecondResult : CallbackResult
    {
        public int leftSecond;
    }

    //语音消息音量大小
    public class NotifyVolumeResult : CallbackResult
    {
        public double volume;
    }


    // 语音消息翻译结果
    public class VoiceTranslateResult : CallbackResult
    {
        public string voiceId; //语音消息Id
        public string voiceContent; //语音消息文字内容
    }

    // 本地语音消息翻译结果
    public class LocalVoiceTranslateResult : CallbackResult
    {
        public string voiceContent; //语音消息文字内容
    }

    // 通过voiceID获取IM语音本地路径回调信息
    public class FetchLocalPathResult : CallbackResult
    {
        public string voiceId;
		public string localPath;
    }

    // 语音转文字
    public class ASRTranslateResult : CallbackResult
    {
        public string partialResult; //识别过程的文本
        public string translateResult; //识别的文本
        public string voiceId; //语音消息Id
        public long voiceTime; //语音消息时长
        public double volume; //语音音量
    }

    // 播放语音消息的回调
    public interface IIMVoicePlayCallback
    {

        // 开始播放IM语音的回调
        void OnStartPlay(StartPlayResult result);

        // 结束播放IM语音的回调
        void OnFinishPlay(FinishPlayResult result);

    }
    // 语音消息的回调
    public interface IIMVoiceRecordCallback
    {

        // 开始录制IM语音的回调
        void OnStartRecord(CallbackResult result);

        // 结束录制IM语音的回调
        void OnFinishRecord(FinishRecordResult result);

        // 结束录制IM语音并不上传的回调
        void OnFinishRecordNotUpload(FinishRecordNotUploadResult result);

        // 上传IM语音的回调
        void OnUploadVoice(UploadVoiceResult result);

        // IM语音录制倒计时的回调
        void OnNotifyLeftSecond(NotifyLeftSecondResult result);

        //IM语音录制实时音量回调
        void OnNotifyVolume(NotifyVolumeResult result);

        // IM语音录制超时回调
        void OnExceedLimitTime(CallbackResult result);

    }

    //语音转文字的回调
    public interface IASRVoiceRecordCallback
    {
        void OnASRInit(CallbackResult result);
        void OnEngineStart(CallbackResult result);
        void OnEngineStop(CallbackResult result);
        void OnEngineError(CallbackResult result);
        void OnASRStartRecord(CallbackResult result);
        void OnASRStopRecord(CallbackResult result);
        void OnASRCancelRecord(CallbackResult result);
    }
}

