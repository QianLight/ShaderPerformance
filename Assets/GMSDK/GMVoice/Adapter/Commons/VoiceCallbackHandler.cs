using System;
using UNBridgeLib;
using UNBridgeLib.LitJson;
using GMSDK;
using UnityEngine;

namespace GMSDK
{
    public class VoiceCallbackHandler : BridgeCallBack
    {
        // 录制IM语音的回调
        public IIMVoiceRecordCallback iMVoiceRecordCallback;

        // 播放IM语音的回调
        public IIMVoicePlayCallback iMVoicePlayCallback;

        // 语音消息翻译的回调
        public Action<VoiceTranslateResult> voiceTranslateCallback;

        // 本地语音消息翻译的回调
        public Action<LocalVoiceTranslateResult> localVoiceTranslateCallback;
        
        // 根据voiceId获取语音文件本地路径的回调
        public Action<FetchLocalPathResult> fetchLocalPathCallback;

        // 语音转文本的回调
        public IASRVoiceRecordCallback asrVoiceRecordCallback;
        public Action<ASRTranslateResult> asrTranslateCallback;

        public void OnASRTranslateCallback(JsonData data)
        {
            LogUtils.D("OnASRTranslateCallback");
            ASRTranslateResult result = SdkUtil.ToObject<ASRTranslateResult>(data.ToJson());
            SdkUtil.InvokeAction<ASRTranslateResult>(asrTranslateCallback, result);
        }

        /// <summary>
        /// ASR初始化回调
        /// </summary>
        /// <param name="data"></param>
        public void OnASRInitCallback(JsonData jd)
        {
            LogUtils.D("ASR - OnASRInitCallback");
            CallbackResult result = SdkUtil.ToObject<CallbackResult>(jd.ToJson());
            if (asrVoiceRecordCallback != null)
            {
                SdkUtil.InvokeAction<CallbackResult>(asrVoiceRecordCallback.OnASRInit, result);
            }
        }

        /// <summary>
        /// ASR引擎启动回调
        /// </summary>
        /// <param name="data"></param>
        public void OnASREngineStartCallback(JsonData jd)
        {
            LogUtils.D("ASR - OnASREngineStartCallback");
            CallbackResult result = SdkUtil.ToObject<CallbackResult>(jd.ToJson());
            if (asrVoiceRecordCallback != null)
            {
                SdkUtil.InvokeAction<CallbackResult>(asrVoiceRecordCallback.OnEngineStart, result);
            }
        }

        /// <summary>
        /// ASR引擎关闭回调
        /// </summary>
        /// <param name="data"></param>
        public void OnASREngineStopCallback(JsonData jd)
        {
            LogUtils.D("ASR - OnASREngineStopCallback");
            CallbackResult result = SdkUtil.ToObject<CallbackResult>(jd.ToJson());
            if (asrVoiceRecordCallback != null)
            {
                SdkUtil.InvokeAction<CallbackResult>(asrVoiceRecordCallback.OnEngineStop, result);
            }
        }

        /// <summary>
        /// ASR引擎异常
        /// </summary>
        /// <param name="data"></param>
        public void OnASREngineErrorCallback(JsonData jd)
        {
            LogUtils.D("ASR - OnASREngineErrorCallback");
            CallbackResult result = SdkUtil.ToObject<CallbackResult>(jd.ToJson());
            if (asrVoiceRecordCallback != null)
            {
                SdkUtil.InvokeAction<CallbackResult>(asrVoiceRecordCallback.OnEngineError, result);
            }
        }

        /// <summary>
        /// ASR开始录制回调
        /// </summary>
        /// <param name="data"></param>
        public void OnASRStartRecordCallback(JsonData jd)
        {
            LogUtils.D("ASR - OnASRStartRecordCallback");
            CallbackResult result = SdkUtil.ToObject<CallbackResult>(jd.ToJson());
            if (asrVoiceRecordCallback != null)
            {
                SdkUtil.InvokeAction<CallbackResult>(asrVoiceRecordCallback.OnASRStartRecord, result);
            }
        }

        /// <summary>
        /// ASR停止录制回调
        /// </summary>
        /// <param name="data"></param>
        public void OnASRStopRecordCallback(JsonData jd)
        {
            LogUtils.D("ASR - OnASRStopRecordCallback");
            CallbackResult result = SdkUtil.ToObject<CallbackResult>(jd.ToJson());
            if (asrVoiceRecordCallback != null)
            {
                SdkUtil.InvokeAction<CallbackResult>(asrVoiceRecordCallback.OnASRStopRecord, result);
            }
        }

        /// <summary>
        /// ASR取消录制回调
        /// </summary>
        /// <param name="data"></param>
        public void OnASRCancelRecordCallback(JsonData jd)
        {
            LogUtils.D("ASR - OnASRCancelRecordCallback");
            CallbackResult result = SdkUtil.ToObject<CallbackResult>(jd.ToJson());
            if (asrVoiceRecordCallback != null)
            {
                SdkUtil.InvokeAction<CallbackResult>(asrVoiceRecordCallback.OnASRCancelRecord, result);
            }
        }


        public VoiceCallbackHandler()
        {
            this.OnFailed = new OnFailedDelegate(OnFailCallback);
            this.OnTimeout = new OnTimeoutDelegate(OnTimeoutCallback);
        }

        /// <summary>
        /// IM语音开始播放回调
        /// </summary>
        /// <param name="data"></param>
        public void OnStartPlayCallback(JsonData jd)
        {
            LogUtils.D("Voice - OnStartPlayCallback");
            StartPlayResult result = SdkUtil.ToObject<StartPlayResult>(jd.ToJson());
            if (iMVoicePlayCallback != null)
            {
                SdkUtil.InvokeAction<StartPlayResult>(iMVoicePlayCallback.OnStartPlay, result);
            }
        }

        /// <summary>
        /// IM语音结束播放回调
        /// </summary>
        /// <param name="data"></param>
        public void OnFinishPlayCallback(JsonData jd)
        {
            LogUtils.D("Voice - OnFinishPlayCallback");
            FinishPlayResult result = SdkUtil.ToObject<FinishPlayResult>(jd.ToJson());
            if (iMVoicePlayCallback != null)
            {
                SdkUtil.InvokeAction<FinishPlayResult>(iMVoicePlayCallback.OnFinishPlay, result);
            }
        }

        /// <summary>
        /// IM语音开始录制回调
        /// </summary>
        /// <param name="data"></param>
        public void OnStartRecordCallback(JsonData jd)
        {
            LogUtils.D("Voice - OnStartRecordCallback");
            CallbackResult result = SdkUtil.ToObject<CallbackResult>(jd.ToJson());
            if (iMVoiceRecordCallback != null)
            {
                SdkUtil.InvokeAction<CallbackResult>(iMVoiceRecordCallback.OnStartRecord, result);
            }
        }

        /// <summary>
        /// IM语音结束录制回调
        /// </summary>
        /// <param name="data"></param>
        public void OnFinishRecordCallback(JsonData jd)
        {
            LogUtils.D("Voice - OnFinishRecordCallback");
            FinishRecordResult result = SdkUtil.ToObject<FinishRecordResult>(jd.ToJson());
            if (iMVoiceRecordCallback != null)
            {
                SdkUtil.InvokeAction<FinishRecordResult>(iMVoiceRecordCallback.OnFinishRecord, result);
            }
        }

        /// <summary>
        /// IM语音结束录制不上传回调
        /// </summary>
        /// <param name="data"></param>
        public void OnFinishRecordNotUploadCallback(JsonData jd)
        {
            LogUtils.D("Voice - OnFinishRecordNotUploadCallback");
            FinishRecordNotUploadResult result = SdkUtil.ToObject<FinishRecordNotUploadResult>(jd.ToJson());
            if (iMVoiceRecordCallback != null)
            {
                SdkUtil.InvokeAction<FinishRecordNotUploadResult>(iMVoiceRecordCallback.OnFinishRecordNotUpload, result);
            }
        }

        /// <summary>
        /// IM语音上传的回调
        /// </summary>
        /// <param name="data"></param>
        public void OnUploadVoiceCallback(JsonData jd)
        {
            LogUtils.D("Voice - OnUploadVoiceCallback");
            UploadVoiceResult result = SdkUtil.ToObject<UploadVoiceResult>(jd.ToJson());
            if (iMVoiceRecordCallback != null)
            {
                SdkUtil.InvokeAction<UploadVoiceResult>(iMVoiceRecordCallback.OnUploadVoice, result);
            }
        }

        /// <summary>
        /// IM语音倒计时回调
        /// </summary>
        /// <param name="data"></param>
        public void OnNotifyLeftSecondCallback(JsonData jd)
        {
            LogUtils.D("Voice - OnNotifyLeftSecondCallback");
            NotifyLeftSecondResult result = SdkUtil.ToObject<NotifyLeftSecondResult>(jd.ToJson());
            if (iMVoiceRecordCallback != null)
            {
                SdkUtil.InvokeAction<NotifyLeftSecondResult>(iMVoiceRecordCallback.OnNotifyLeftSecond, result);
            }
        }

        /// <summary>
        /// IM语音音量回调
        /// </summary>
        /// <param name="data"></param>
        public void OnNotifyVolumeCallback(JsonData jd)
        {
            LogUtils.D("Voice - OnNotifyVolumeCallback");
            NotifyVolumeResult result = SdkUtil.ToObject<NotifyVolumeResult>(jd.ToJson());
            if (iMVoiceRecordCallback != null)
            {
                SdkUtil.InvokeAction<NotifyVolumeResult>(iMVoiceRecordCallback.OnNotifyVolume, result);
            }
        }

        /// <summary>
        /// IM语音超时回调
        /// </summary>
        /// <param name="data"></param>
        public void OnExceedLimitTimeCallback(JsonData jd)
        {
            LogUtils.D("Voice - OnNotifyLeftSecondCallback");
            CallbackResult result = SdkUtil.ToObject<CallbackResult>(jd.ToJson());
            if (iMVoiceRecordCallback != null)
            {
                SdkUtil.InvokeAction<CallbackResult>(iMVoiceRecordCallback.OnExceedLimitTime, result);
            }

        }

        /// <summary>
        /// IM语音翻译回调
        /// </summary>
        /// <param name="data"></param>
        public void OnTranslateCallback(JsonData jd)
        {
            LogUtils.D("Voice - OnTranslateCallback");
            VoiceTranslateResult result = SdkUtil.ToObject<VoiceTranslateResult>(jd.ToJson());
            SdkUtil.InvokeAction<VoiceTranslateResult>(voiceTranslateCallback, result);
        }


        /// <summary>
        /// 本地IM语音翻译回调
        /// </summary>
        /// <param name="data"></param>
        public void OnLocalTranslateCallback(JsonData jd)
        {
            LogUtils.D("Voice - OnLocalTranslateCallback");
            LocalVoiceTranslateResult result = SdkUtil.ToObject<LocalVoiceTranslateResult>(jd.ToJson());
            SdkUtil.InvokeAction<LocalVoiceTranslateResult>(localVoiceTranslateCallback, result);
        }

        /// <summary>
        /// 根据voiceID获取语音文件本地路径回调
        /// </summary>
        /// <param name="data"></param>
        public void OnFetchLocalPathCallback(JsonData jd)
        {
            LogUtils.D("Voice - OnFetchLocalPathCallback");
            FetchLocalPathResult result = SdkUtil.ToObject<FetchLocalPathResult>(jd.ToJson());
            SdkUtil.InvokeAction<FetchLocalPathResult>(fetchLocalPathCallback, result);
        }

        /// <summary>
        /// 失败统一回调，用于调试接口
        /// </summary>
        public void OnFailCallback(int code, string failMsg)
        {
            LogUtils.E("Voice - OnFailCallback");
            LogUtils.E("接口访问失败 " + code.ToString() + " " + failMsg);
        }
        /// <summary>
        /// 超时统一回调
        /// </summary>
        public void OnTimeoutCallback()
        {
            JsonData jd = new JsonData();
            jd["code"] = -321;
            jd["message"] = "Voice - request time out";
            if (this.OnSuccess != null)
            {
                this.OnSuccess(jd);
            }
        }
    }
}