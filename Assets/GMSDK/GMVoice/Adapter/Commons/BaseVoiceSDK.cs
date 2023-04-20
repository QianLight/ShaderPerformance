using System;
using System.Collections.Generic;
using UNBridgeLib;
using UNBridgeLib.LitJson;

namespace GMSDK
{

    public class BaseVoiceSDK
    {
        public BaseVoiceSDK()
        {
#if UNITY_ANDROID
            UNBridge.Call(VoiceMethodName.Init, null);
            UNBridge.Call(VoiceMethodName.InitASR, null);
#endif
        }
        
        #region 语音消息
        /// <summary>
        /// IM语音初始化配置
        /// </summary>
        /// <param name="savePath">语音文件缓存路径，安卓专属，IOS无需设置</param>
        /// <param name="playMode">语音播放模式，
        /// 0：默认模式(遮挡传感器时，使用听筒播放；取消遮挡传感器时，使用扬声器播放。)
        /// 1：外放模式(即一直使用扬声器播放)
        /// </param>
        /// <param name="expireTime">IM语音配置语音本地文件失效时间，秒为单位，默认30天。(30 * 24 * 60 * 60)</param>
        public void InitIMVoice(string savePath, int playMode, int expireTime = 30 * 24 * 60 * 60)
        {
            JsonData param = new JsonData();
            param["savePath"] = savePath;
            param["playMode"] = playMode;
            param["expireTime"] = expireTime;
            UNBridge.Call(VoiceMethodName.InitIM, param);
        }

        /// <summary>
        /// 开始录制IM语音
        /// </summary>
        /// <param name="callback">回调.</param>
        /// <param name="language">录制使用的语种.</param>
        /// <param name="region">录制所在的地区，用于精准匹配.</param>
        public void IMVoiceStartRecord(IIMVoiceRecordCallback callback, string language = null, string region = null)
        {
            IMListenStartRecordEvent(callback);
            IMListenFinishRecordEvent(callback);
            IMListenNotifyLeftSecondEvent(callback);
            IMListenNotifyVolumeEvent(callback);
            IMListenExceedLimitTimeEvent(callback);
            JsonData param = new JsonData();
            param["voiceLanguage"] = language;
            param["voiceRegion"] = region;
            UNBridge.Call(VoiceMethodName.IMStartRecord, param);
        }

        /// <summary>
        /// 结束录制IM语音
        /// </summary>
        public void IMVoiceStopRecord()
        {
            UNBridge.Call(VoiceMethodName.IMStopRecord, null);
        }

        /// <summary>
        /// 结束录制IM语音,不上传
        /// </summary>
        /// <param name="callback">回调.</param>
        public void IMVoiceStopRecordNotUpload(IIMVoiceRecordCallback callback)
        {
            VoiceCallbackHandler unCallback = new VoiceCallbackHandler();
            unCallback.iMVoiceRecordCallback = callback;
            unCallback.OnSuccess = new OnSuccessDelegate(unCallback.OnFinishRecordNotUploadCallback);
            UNBridge.Listen(VoiceResultName.IMFinishRecordNotUploadResult, unCallback);
            UNBridge.Call(VoiceMethodName.IMStopRecordNotUpload, null);
        }

        /// <summary>
        /// 上传语音文件
        /// </summary>
        /// <param name="uniqueId">路径.</param>
        /// <param name="callback">回调.</param>
        public void IMVoiceUploadVoiceByUniqueId(string uniqueId, IIMVoiceRecordCallback callback)
        {
            VoiceCallbackHandler unCallback = new VoiceCallbackHandler();
            unCallback.iMVoiceRecordCallback = callback;
            unCallback.OnSuccess = new OnSuccessDelegate(unCallback.OnUploadVoiceCallback);
            JsonData param = new JsonData();
            param["uniqueId"] = uniqueId;
            UNBridge.Call(VoiceMethodName.IMUploadVoice, param, unCallback);
        }

        /// <summary>
        /// 取消录制IM语音
        /// </summary>
        public void IMVoiceCancelRecord()
        {
            UNBridge.Call(VoiceMethodName.IMCancelRecord, null);
        }

        /// <summary>
        /// 播放单个语音文件
        /// </summary>
        /// <param name="voiceId">voiceId</param>
        public void IMVoiceStartPlay(string voiceId, IIMVoicePlayCallback callback)
        {
            IMListenStartPlayEvent(callback);
            IMListenFinishPlayEvent(callback);
            JsonData param = new JsonData();
            param["voiceId"] = voiceId;
            UNBridge.Call(VoiceMethodName.IMStartPlaySingleVoice, param);
        }

        /// <summary>
        /// 播放IM语音
        /// </summary>
        /// <param name="voiceIds">传入一组voiceId进行播放</param>
        public void IMVoiceStartPlay(List<string> voiceIds, IIMVoicePlayCallback callback)
        {
            IMListenStartPlayEvent(callback);
            IMListenFinishPlayEvent(callback);
            JsonData param = new JsonData();
            param["voiceIds"] = JsonMapper.ToJson(voiceIds);
            UNBridge.Call(VoiceMethodName.IMStartPlay, param);
        }

        /// <summary>
        /// 停止播放IM语音
        /// </summary>
        public void IMVoiceStopPlay()
        {
            UNBridge.Call(VoiceMethodName.IMStopPlay, null);
        }

        /// <summary>
        /// 释放IM语音资源
        /// </summary>
        public void IMVoiceRelease()
        {
            UNBridge.Call(VoiceMethodName.IMRelease, null);
            UNBridge.UnListen(VoiceResultName.IMStartPlayResult);
            UNBridge.UnListen(VoiceResultName.IMFinishPlayResult);
            UNBridge.UnListen(VoiceResultName.IMStartRecordResult);
            UNBridge.UnListen(VoiceResultName.IMFinishRecordResult);
            UNBridge.UnListen(VoiceResultName.IMNotifyLeftSecondResult);
            UNBridge.UnListen(VoiceResultName.IMNotifyVolumeResult);
            UNBridge.UnListen(VoiceResultName.IMExceedLimitTimeResult);
        }

        // 监听开始播放事件
        private void IMListenStartPlayEvent(IIMVoicePlayCallback callback)
        {
            VoiceCallbackHandler unCallBack = new VoiceCallbackHandler();
            unCallBack.iMVoicePlayCallback = callback;
            unCallBack.OnSuccess = new OnSuccessDelegate(unCallBack.OnStartPlayCallback);
            UNBridge.Listen(VoiceResultName.IMStartPlayResult, unCallBack);
        }

        // 监听IM语音播放完成事件
        private void IMListenFinishPlayEvent(IIMVoicePlayCallback callback)
        {
            VoiceCallbackHandler unCallBack = new VoiceCallbackHandler();
            unCallBack.iMVoicePlayCallback = callback;
            unCallBack.OnSuccess = new OnSuccessDelegate(unCallBack.OnFinishPlayCallback);
            UNBridge.Listen(VoiceResultName.IMFinishPlayResult, unCallBack);
        }

        // 监听开始录制IM语音事件
        private void IMListenStartRecordEvent(IIMVoiceRecordCallback callback)
        {
            VoiceCallbackHandler unCallBack = new VoiceCallbackHandler();
            unCallBack.iMVoiceRecordCallback = callback;
            unCallBack.OnSuccess = new OnSuccessDelegate(unCallBack.OnStartRecordCallback);
            UNBridge.Listen(VoiceResultName.IMStartRecordResult, unCallBack);
        }

        // 监听录制完成事件
        private void IMListenFinishRecordEvent(IIMVoiceRecordCallback callback)
        {
            VoiceCallbackHandler unCallBack = new VoiceCallbackHandler();
            unCallBack.iMVoiceRecordCallback = callback;
            unCallBack.OnSuccess = new OnSuccessDelegate(unCallBack.OnFinishRecordCallback);
            UNBridge.Listen(VoiceResultName.IMFinishRecordResult, unCallBack);
        }

        // 监听录制IM语音倒计时事件
        private void IMListenNotifyLeftSecondEvent(IIMVoiceRecordCallback callback)
        {
            VoiceCallbackHandler unCallBack = new VoiceCallbackHandler();
            unCallBack.iMVoiceRecordCallback = callback;
            unCallBack.OnSuccess = new OnSuccessDelegate(unCallBack.OnNotifyLeftSecondCallback);
            UNBridge.Listen(VoiceResultName.IMNotifyLeftSecondResult, unCallBack);
        }

        //监听语音音量大小
        private void IMListenNotifyVolumeEvent(IIMVoiceRecordCallback callback)
        {
            VoiceCallbackHandler unCallBack = new VoiceCallbackHandler();
            unCallBack.iMVoiceRecordCallback = callback;
            unCallBack.OnSuccess = new OnSuccessDelegate(unCallBack.OnNotifyVolumeCallback);
            UNBridge.Listen(VoiceResultName.IMNotifyVolumeResult, unCallBack);
        }

        // 监听录制IM语音超时事件
        private void IMListenExceedLimitTimeEvent(IIMVoiceRecordCallback callback)
        {
            VoiceCallbackHandler unCallBack = new VoiceCallbackHandler();
            unCallBack.iMVoiceRecordCallback = callback;
            unCallBack.OnSuccess = new OnSuccessDelegate(unCallBack.OnExceedLimitTimeCallback);
            UNBridge.Listen(VoiceResultName.IMExceedLimitTimeResult, unCallBack);
        }

        /// <summary>
        /// 翻译IM语音消息
        /// </summary>
        /// <param name="voiceId">语音消息Id</param>
        /// <param name="callback">回调</param>
        /// <param name="translateLanguage">目标语言</param>
        public void IMVoiceTranslate(string voiceId, Action<VoiceTranslateResult> callback, string translateLanguage = null)
        {
            VoiceCallbackHandler unCallBack = new VoiceCallbackHandler();
            unCallBack.voiceTranslateCallback = callback;
            unCallBack.OnSuccess = new OnSuccessDelegate(unCallBack.OnTranslateCallback);

            JsonData param = new JsonData();
            param["voiceId"] = voiceId;
            param["translateLanguage"] = translateLanguage;
            UNBridge.Call(VoiceMethodName.IMTranslate, param, unCallBack);
        }

        /// <summary>
        /// 翻译本地IM语音消息
        /// </summary>
        /// <param name="voicePath">语音消息路径</param>
        /// <param name="callback">回调</param>
        /// <param name="voiceLanguage">语音消息语言</param>
        /// <param name="translateLanguage">目标语言，传空使用SDK语言</param>
        /// <param name="region">录音语言区域，用于辅助录音语言的精准匹配，传空使用设备系统区域</param>
        public void IMVoiceTranslateLocal(string voicePath, Action<LocalVoiceTranslateResult> callback, string voiceLanguage, string translateLanguage = null, string region = null)
        {
            VoiceCallbackHandler unCallBack = new VoiceCallbackHandler();
            unCallBack.localVoiceTranslateCallback = callback;
            unCallBack.OnSuccess = new OnSuccessDelegate(unCallBack.OnLocalTranslateCallback);

            JsonData param = new JsonData();
            param["localVoicePath"] = voicePath;
            param["voiceLanguage"] = voiceLanguage;
            param["translateLanguage"] = translateLanguage;
            param["region"] = region;
            UNBridge.Call(VoiceMethodName.IMTranslateLocal, param, unCallBack);

        }
        // 监听根据voiceId获取语音消息本地路径完成事件
        private void IMListenFetchLocalPathEvent(Action<FetchLocalPathResult> callback)
        {
            VoiceCallbackHandler unCallBack = new VoiceCallbackHandler();
            unCallBack.fetchLocalPathCallback = callback;
            unCallBack.OnSuccess = new OnSuccessDelegate(unCallBack.OnFetchLocalPathCallback);
            UNBridge.Listen(VoiceResultName.IMFetchLocalPathResult, unCallBack);
        }
        
        /// <summary>
        /// 根据voiceId获取语音消息本地路径
        /// </summary>
        /// <param name="voiceId">语音消息Id</param>
        /// <param name="callback">回调</param>
        public void IMFetchLocalPath(string voiceId, Action<FetchLocalPathResult> callback)
        {
            IMListenFetchLocalPathEvent(callback);
            
            JsonData param = new JsonData();
            JsonData data = new JsonData();
            data["voiceId"] = voiceId;
            param["data"] = data;
            UNBridge.Call(VoiceMethodName.IMFetchLocalPath, param);
        }

        #endregion

        #region 语音转文字
        /// <summary>
        /// 初始化配置
        /// </summary>
        /// <param name="playMode">语音播放模式，
        /// 0：默认模式(遮挡传感器时，使用听筒播放；取消遮挡传感器时，使用扬声器播放。)
        /// 1：外放模式(即一直使用扬声器播放)
        /// </param>
        /// <param name="language">目标语，
        /// 0：中文（默认语言）
        /// 1：英文
        /// </param>
        public void InitASR(int playMode, IASRVoiceRecordCallback callback, int language = 0)
        {
            ASRListenInitEvent(callback);
            ASRListenEngineStartEvent(callback);
            ASRListenEngineStopEvent(callback);
            ASRListenEngineErrorEvent(callback);
            ASRListenStartRecordEvent(callback);
            ASRListenStopRecordEvent(callback);
            ASRListenCancelRecordEvent(callback);
            JsonData param = new JsonData();
            param["playMode"] = playMode;
            param["language"] = language;
            UNBridge.Call(VoiceMethodName.ASRInit, param);
        }

        /// <summary>
        /// 开始录制
        /// </summary>
        public void StartRecord(Action<ASRTranslateResult> callback)
        {
            ASRListenTranslateEvent(callback);
            UNBridge.Call(VoiceMethodName.ASRStartRecord, null);
        }

        /// <summary>
        /// 结束录制
        /// </summary>
        public void StopRecord()
        {
            UNBridge.Call(VoiceMethodName.ASRStopRecord, null);
        }

        /// <summary>
        /// 取消录制
        /// </summary>
        public void CancelRecord()
        {
            UNBridge.Call(VoiceMethodName.ASRCancelRecord, null);
        }

        /// <summary>
        /// 析构
        /// </summary>
        public void UnInit()
        {
            UNBridge.UnListen(VoiceResultName.IMStartPlayResult);
            UNBridge.UnListen(VoiceResultName.IMFinishPlayResult);
            UNBridge.UnListen(VoiceResultName.ASRInitResult);
            UNBridge.UnListen(VoiceResultName.ASRStartEngineResult);
            UNBridge.UnListen(VoiceResultName.ASRStopEngineResult);
            UNBridge.UnListen(VoiceResultName.ASRErrorEngineResult);
            UNBridge.UnListen(VoiceResultName.ASRStartRecordResult);
            UNBridge.UnListen(VoiceResultName.ASRStopRecordResult);
            UNBridge.UnListen(VoiceResultName.ASRCancelRecordResult);
            UNBridge.UnListen(VoiceResultName.ASRPartialTranslateResult);
            UNBridge.UnListen(VoiceResultName.ASRTranslateResult);
            UNBridge.UnListen(VoiceResultName.ASRFinalTranslateResult);
            UNBridge.UnListen(VoiceResultName.ASRVolumeLevelResult);
            UNBridge.Call(VoiceMethodName.ASRUnInit, null);
        }

        /// <summary>
        /// 播放语音
        /// </summary>
        public void StartPlayVoice(List<string> voiceIds, IIMVoicePlayCallback callback)
        {
            IMListenStartPlayEvent(callback);
            IMListenFinishPlayEvent(callback);
            JsonData param = new JsonData();
            param["voiceIds"] = JsonMapper.ToJson(voiceIds);
            UNBridge.Call(VoiceMethodName.ASRStartPlay, param);
        }

        /// <summary>
        /// 停止播放语音
        /// </summary>
        public void StopPlayVoice()
        {
            UNBridge.Call(VoiceMethodName.ASRStopPlay, null);
        }


        private void ASRListenInitEvent(IASRVoiceRecordCallback callback)
        {
            VoiceCallbackHandler unCallBack = new VoiceCallbackHandler();
            unCallBack.asrVoiceRecordCallback = callback;
            unCallBack.OnSuccess = new OnSuccessDelegate(unCallBack.OnASRInitCallback);
            UNBridge.Listen(VoiceResultName.ASRInitResult, unCallBack);
        }

        private void ASRListenEngineStartEvent(IASRVoiceRecordCallback callback)
        {
            VoiceCallbackHandler unCallBack = new VoiceCallbackHandler();
            unCallBack.asrVoiceRecordCallback = callback;
            unCallBack.OnSuccess = new OnSuccessDelegate(unCallBack.OnASREngineStartCallback);
            UNBridge.Listen(VoiceResultName.ASRStartEngineResult, unCallBack);
        }


        private void ASRListenEngineStopEvent(IASRVoiceRecordCallback callback)
        {
            VoiceCallbackHandler unCallBack = new VoiceCallbackHandler();
            unCallBack.asrVoiceRecordCallback = callback;
            unCallBack.OnSuccess = new OnSuccessDelegate(unCallBack.OnASREngineStopCallback);
            UNBridge.Listen(VoiceResultName.ASRStopEngineResult, unCallBack);
        }

        private void ASRListenStartRecordEvent(IASRVoiceRecordCallback callback)
        {
            VoiceCallbackHandler unCallBack = new VoiceCallbackHandler();
            unCallBack.asrVoiceRecordCallback = callback;
            unCallBack.OnSuccess = new OnSuccessDelegate(unCallBack.OnASRStartRecordCallback);
            UNBridge.Listen(VoiceResultName.ASRStartRecordResult, unCallBack);
        }

        private void ASRListenStopRecordEvent(IASRVoiceRecordCallback callback)
        {
            VoiceCallbackHandler unCallBack = new VoiceCallbackHandler();
            unCallBack.asrVoiceRecordCallback = callback;
            unCallBack.OnSuccess = new OnSuccessDelegate(unCallBack.OnASRStopRecordCallback);
            UNBridge.Listen(VoiceResultName.ASRStopRecordResult, unCallBack);
        }

        private void ASRListenCancelRecordEvent(IASRVoiceRecordCallback callback)
        {
            VoiceCallbackHandler unCallBack = new VoiceCallbackHandler();
            unCallBack.asrVoiceRecordCallback = callback;
            unCallBack.OnSuccess = new OnSuccessDelegate(unCallBack.OnASRCancelRecordCallback);
            UNBridge.Listen(VoiceResultName.ASRCancelRecordResult, unCallBack);
        }

        public void ASRListenEngineErrorEvent(IASRVoiceRecordCallback callback)
        {
            VoiceCallbackHandler unCallBack = new VoiceCallbackHandler();
            unCallBack.asrVoiceRecordCallback = callback;
            unCallBack.OnSuccess = new OnSuccessDelegate(unCallBack.OnASREngineErrorCallback);
            UNBridge.Listen(VoiceResultName.ASRErrorEngineResult, unCallBack);
        }

        public void ASRListenTranslateEvent(Action<ASRTranslateResult> callback)
        {
            VoiceCallbackHandler unCallBack = new VoiceCallbackHandler();
            unCallBack.asrTranslateCallback = callback;
            unCallBack.OnSuccess = new OnSuccessDelegate(unCallBack.OnASRTranslateCallback);
            UNBridge.Listen(VoiceResultName.ASRPartialTranslateResult, unCallBack);
            UNBridge.Listen(VoiceResultName.ASRTranslateResult, unCallBack);
            UNBridge.Listen(VoiceResultName.ASRFinalTranslateResult, unCallBack);
            UNBridge.Listen(VoiceResultName.ASRVolumeLevelResult, unCallBack);
        }

        #endregion
    }
}
