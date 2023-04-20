using System;
using System.Collections.Generic;
using GMSDK;
using GSDK.IMVoice;
using GSDK.ASRVoice;

namespace GSDK
{
    public class VoiceService : IVoiceService
    {
        #region Variables
        
        private readonly IIMVoice _imVoice = new IMVoice.IMVoice();
        
        private readonly IASRVoice _asrVoice = new ASRVoice.ASRVoice();
        
        #endregion
    
        #region Properties

        public IIMVoice IMVoice
        {
            get { return _imVoice; }
        }
        
        public IASRVoice ASRVoice
        {
            get { return _asrVoice; }
        }
        
        #endregion
    }
}

namespace GSDK.IMVoice
{
    public class IMVoice : IIMVoice, IIMVoiceRecordCallback, IIMVoicePlayCallback
    {
        #region Events

        public event FinishRecordEventHandler FinishRecordEvent;
        public event StartPlayEventHandler StartPlayEvent;
        public event FinishPlayEventHandler FinishPlayEvent;
        public event CountDownEventHandler CountDownEvent;
        public event NotifyRecordVolumeEventHandler NotifyRecordVolume;
        public event RecordTimeExceedEventHandler RecordTimeExceedEvent;

        #endregion

        #region Variables

        // VoiceSDK实例
        private readonly BaseVoiceSDK _sdk;
        private StartRecordDelegate _startRecordCallback;
        private StopRecordNotUploadDelegate _stopRecordNotUploadCallback;
        private UploadVoiceByUniqueIdDelegate _uploadVoiceCallback;
        private TranscribeDelegate _transcribeCallback;
        private TranscribeLocalDelegate _transcribeLocalCallback;
        private FetchLocalPathDelegate _fetchLocalPathCallback;

        #endregion

        #region Methods

        public IMVoice()
        {
            _sdk = GMVoiceMgr.instance.SDK;
        }

        public void Initialize(PlayMode playMode, int expireTime, string savePath)
        {
            GSDKProfilerTools.BeginSample("Voice-Initialize");
            VoiceLog.LogInfo(VoiceTag.IMVoice,
                string.Format("Initialize, PlayMode:{0}, ExpireTime:{1}, SavePath:{2}", playMode, expireTime,
                    savePath));
            _sdk.InitIMVoice(savePath, VoiceInnerTools.Convert(playMode), expireTime);
            GSDKProfilerTools.EndSample();
        }

        public void Release()
        {
            GSDKProfilerTools.BeginSample("Voice-Release");
            VoiceLog.LogInfo(VoiceTag.IMVoice, "Release");
            _sdk.IMVoiceRelease();
            GSDKProfilerTools.EndSample();
        }

        public void StartRecord(StartRecordDelegate startRecordCallback, string language, string region)
        {
            GSDKProfilerTools.BeginSample("Voice-StartRecord");
            this._startRecordCallback = startRecordCallback;
            VoiceLog.LogInfo(VoiceTag.IMVoice, string.Format("StartRecord, Language:{0}, Region:{1}", language, region));
            _sdk.IMVoiceStartRecord(this, language, region);
            GSDKProfilerTools.EndSample();
        }

        public void StopRecord()
        {
            GSDKProfilerTools.BeginSample("Voice-StopRecord");
            VoiceLog.LogInfo(VoiceTag.IMVoice, "StopRecord");
            _sdk.IMVoiceStopRecord();
            GSDKProfilerTools.EndSample();
        }

        public void StopRecordNotUpload(StopRecordNotUploadDelegate stopRecordNotUploadCallback)
        {
            GSDKProfilerTools.BeginSample("Voice-StopRecordNotUpload");
            this._stopRecordNotUploadCallback = stopRecordNotUploadCallback;
            VoiceLog.LogInfo(VoiceTag.IMVoice, "StopRecordNotUpload");
            _sdk.IMVoiceStopRecordNotUpload(this);
            GSDKProfilerTools.EndSample();
        }

        public void UploadVoiceByUniqueId(string uniqueId, UploadVoiceByUniqueIdDelegate callback)
        {
            GSDKProfilerTools.BeginSample("Voice-UploadVoiceByUniqueId");
            this._uploadVoiceCallback = callback;
            VoiceLog.LogInfo(VoiceTag.IMVoice, "UploadVoice");
            _sdk.IMVoiceUploadVoiceByUniqueId(uniqueId, this);
            GSDKProfilerTools.EndSample();
        }


        public void CancelRecord()
        {
            GSDKProfilerTools.BeginSample("Voice-CancelRecord");
            VoiceLog.LogInfo(VoiceTag.IMVoice, "CancelRecord");
            _sdk.IMVoiceCancelRecord();
            GSDKProfilerTools.EndSample();
        }

        public void StartPlay(string voiceID)
        {
            GSDKProfilerTools.BeginSample("Voice-StartPlay");
            VoiceLog.LogInfo(VoiceTag.IMVoice, "PlaySingleVoice");
            _sdk.IMVoiceStartPlay(voiceID, this);
            GSDKProfilerTools.EndSample();
        }

        public void StartPlay(List<string> voiceIDs)
        {
            GSDKProfilerTools.BeginSample("Voice-StartPlayList");
            var voiceIdsString = voiceIDs == null ? "" : string.Join(",", voiceIDs.ToArray());
            VoiceLog.LogInfo(VoiceTag.IMVoice, string.Format("StartPlay, VoiceIds:[({0})]", voiceIdsString));
            _sdk.IMVoiceStartPlay(voiceIDs, this);
            GSDKProfilerTools.EndSample();
        }

        public void StopPlay()
        {
            GSDKProfilerTools.BeginSample("Voice-StopPlay");
            VoiceLog.LogInfo(VoiceTag.IMVoice, "StopPlay");
            _sdk.IMVoiceStopPlay();
            GSDKProfilerTools.EndSample();
        }

        public void Transcribe(string voiceID, TranscribeDelegate callback, string targetLanguage)
        {
            GSDKProfilerTools.BeginSample("Voice-Transcribe");
            _transcribeCallback = callback;
            VoiceLog.LogInfo(VoiceTag.IMVoice, string.Format("Transcribe ,voiceId:{0}, targetLanguage:{1}", voiceID, targetLanguage));
            _sdk.IMVoiceTranslate(voiceID, OnTranscribeResult, targetLanguage);
            GSDKProfilerTools.EndSample();
        }

        public void TranscribeLocal(string voicePath, TranscribeLocalDelegate callback, string voiceLanguage, string translateLanguage, string region)
        {
            GSDKProfilerTools.BeginSample("Voice-TranscribeLocal");
            _transcribeLocalCallback = callback;
            VoiceLog.LogInfo(VoiceTag.IMVoice, string.Format("TranscribeLocal ,voicePath:{0}, voiceLanguage:{1}, translateLanguage:{2}, region:{3}", voicePath, voiceLanguage, translateLanguage, region));
            _sdk.IMVoiceTranslateLocal(voicePath, OnTranscribeLocalResult, voiceLanguage, translateLanguage, region);
            GSDKProfilerTools.EndSample();
        }
        
        public void FetchLocalPath(string voiceID, FetchLocalPathDelegate callback)
        {
            GSDKProfilerTools.BeginSample("Voice-FetchLocalPath");
            _fetchLocalPathCallback = callback;
            VoiceLog.LogInfo(VoiceTag.IMVoice, string.Format("FetchLocalPath ,voiceId:{0}", voiceID));
            _sdk.IMFetchLocalPath(voiceID, OnFetchLocalPathResult);
            GSDKProfilerTools.EndSample();
        }

        #region IIMVoiceRecordCallback实现

        public void OnStartRecord(CallbackResult callbackResult)
        {
            if (_startRecordCallback != null)
            {
                try
                {
                    var result = VoiceInnerTools.ConvertVoiceIMRecordError(callbackResult);
                    VoiceLog.LogInfo(VoiceTag.IMVoice, "Perform StartRecordCallback", result);
                    _startRecordCallback(result);
                }
                catch (Exception e)
                {
                    VoiceLog.LogException(e);
                }
            }
            else
            {
                VoiceLog.LogWarning(VoiceTag.IMVoice, "StartRecordCallback is null");
            }
        }

        public void OnFinishRecord(FinishRecordResult finishRecordResult)
        {
            if (FinishRecordEvent != null)
            {
                try
                {
                    var result = VoiceInnerTools.ConvertVoiceIMRecordError(finishRecordResult);
                    var finishRecordInfo = VoiceInnerTools.Convert(finishRecordResult);
                    VoiceLog.LogInfo(VoiceTag.IMVoice, "Perform FinishRecordEvent", finishRecordInfo, result);
                    FinishRecordEvent(result, finishRecordInfo);
                }
                catch (Exception e)
                {
                    VoiceLog.LogException(e);
                }
            }
            else
            {
                VoiceLog.LogWarning(VoiceTag.IMVoice, "FinishRecordEvent is null");
            }
        }

        public void OnFinishRecordNotUpload(FinishRecordNotUploadResult finishRecordNotUploadResult)
        {
            if (_stopRecordNotUploadCallback != null)
            {
                try
                {
                    var result = VoiceInnerTools.ConvertVoiceIMRecordError(finishRecordNotUploadResult);
                    var finishRecordInfo = VoiceInnerTools.Convert(finishRecordNotUploadResult);
                    VoiceLog.LogInfo(VoiceTag.IMVoice, "Perform StopRecordNotUp", result);
                    _stopRecordNotUploadCallback(result, finishRecordInfo);
                }
                catch (Exception e)
                {
                    VoiceLog.LogException(e);
                }
            }
            else
            {
                VoiceLog.LogWarning(VoiceTag.IMVoice, "StopRecordNotUploadCallback is null");
            }

        }

        public void OnUploadVoice(UploadVoiceResult uploadResult)
        {
            if (_uploadVoiceCallback != null)
            {
                try
                {
                    var result = VoiceInnerTools.ConvertVoiceIMRecordError(uploadResult);
                    var uploadResultInfo = VoiceInnerTools.Convert(uploadResult);
                    VoiceLog.LogInfo(VoiceTag.IMVoice, "Perform UploadVoice", result);
                    _uploadVoiceCallback(result, uploadResultInfo);
                }
                catch (Exception e)
                {
                    VoiceLog.LogException(e);
                }
            }
            else
            {
                VoiceLog.LogWarning(VoiceTag.IMVoice, "UploadVoiceCallback is null");
            }
        }

        public void OnNotifyLeftSecond(NotifyLeftSecondResult notifyLeftSecondResult)
        {
            if (CountDownEvent != null)
            {
                try
                {
                    var leftSeconds = notifyLeftSecondResult.leftSecond;
                    VoiceLog.LogInfo(VoiceTag.IMVoice, "Perform CountDownEvent, leftSeconds:" + leftSeconds);
                    CountDownEvent(leftSeconds);
                }
                catch (Exception e)
                {
                    VoiceLog.LogException(e);
                }
            }
            else
            {
                VoiceLog.LogWarning(VoiceTag.IMVoice, "CountDownEvent is null");
            }
        }

        public void OnNotifyVolume(NotifyVolumeResult notifyVolumeResult)
        {
            if (NotifyRecordVolume != null)
            {
                try
                {
                    var volume = notifyVolumeResult.volume;
                    VoiceLog.LogInfo(VoiceTag.IMVoice, "Perform NotifyRecordVolume, volume:" + volume);
                    NotifyRecordVolume(volume);
                }
                catch (Exception e)
                {
                    VoiceLog.LogException(e);
                }
            }
            else
            {
                VoiceLog.LogWarning(VoiceTag.IMVoice, "NotifyRecordVolume is null");
            }
        }

        public void OnExceedLimitTime(CallbackResult callbackResult)
        {
            if (RecordTimeExceedEvent != null)
            {
                try
                {
                    VoiceLog.LogInfo(VoiceTag.IMVoice, "Perform RecordTimeExceedEvent");
                    RecordTimeExceedEvent();
                }
                catch (Exception e)
                {
                    VoiceLog.LogException(e);
                }
            }
            else
            {
                VoiceLog.LogWarning(VoiceTag.IMVoice, "RecordTimeExceedEvent is null");
            }
        }

        #endregion

        #region IIMVoicePlayCallback实现

        public void OnStartPlay(StartPlayResult startPlayResult)
        {
            if (StartPlayEvent != null)
            {
                try
                {
                    var result = VoiceInnerTools.ConvertVoiceIMPlayError(startPlayResult);
                    var playInfo = VoiceInnerTools.Convert(startPlayResult);
                    VoiceLog.LogInfo(VoiceTag.IMVoice, "Perform StartPlayEvent", playInfo, result);
                    StartPlayEvent(result, playInfo);
                }
                catch (Exception e)
                {
                    VoiceLog.LogException(e);
                }
            }
            else
            {
                VoiceLog.LogWarning(VoiceTag.IMVoice, "StartPlayEvent is null");
            }
        }

        public void OnFinishPlay(FinishPlayResult finishPlayResult)
        {
            if (FinishPlayEvent != null)
            {
                try
                {
                    var result = VoiceInnerTools.ConvertVoiceIMPlayError(finishPlayResult);
                    var finishPlayInfo = VoiceInnerTools.Convert(finishPlayResult);
                    VoiceLog.LogInfo(VoiceTag.IMVoice, "Perform FinishPlayEvent", finishPlayInfo, result);
                    FinishPlayEvent(finishPlayInfo);
                }
                catch (Exception e)
                {
                    VoiceLog.LogException(e);
                }
            }
            else
            {
                VoiceLog.LogWarning(VoiceTag.IMVoice, "FinishPlayEvent is null");
            }
        }

        #endregion

        private void OnTranscribeResult(VoiceTranslateResult voiceTranscribeResult)
        {
            if (_transcribeCallback != null)
            {
                try
                {
                    var result = VoiceInnerTools.ConvertVoiceIMTranscribeError(voiceTranscribeResult);
                    var transcribeInfo = VoiceInnerTools.Convert(voiceTranscribeResult);
                    VoiceLog.LogInfo(VoiceTag.IMVoice, "TranscribeCallback", transcribeInfo, result);
                    _transcribeCallback(result, transcribeInfo);
                }
                catch (Exception e)
                {
                    VoiceLog.LogException(e);
                }
            }
            else
            {
                VoiceLog.LogWarning(VoiceTag.IMVoice, "TranscribeCallback is null");
            }
        }

        private void OnTranscribeLocalResult(LocalVoiceTranslateResult localVoiceTranslateResult)
        {
            if (_transcribeLocalCallback != null)
            {
                try
                {
                    var result = VoiceInnerTools.ConvertVoiceIMTranscribeError(localVoiceTranslateResult);
                    var transcribeLocalInfo = VoiceInnerTools.Convert(localVoiceTranslateResult);
                    VoiceLog.LogInfo(VoiceTag.IMVoice, "TranscribeCallback", transcribeLocalInfo, result);
                    _transcribeLocalCallback(result, transcribeLocalInfo);
                }
                catch (Exception e)
                {
                    VoiceLog.LogException(e);
                }
            }
            else
            {
                VoiceLog.LogWarning(VoiceTag.IMVoice, "TranscribeLocalCallback is null");
            }
        }

        private void OnFetchLocalPathResult(FetchLocalPathResult fetchLocalPathResult)
        {
            if (_fetchLocalPathCallback != null)
            {
                try
                {
                    var result = VoiceInnerTools.ConvertVoiceIMPlayError(fetchLocalPathResult);
                    var voiceFileInfo = VoiceInnerTools.Convert(fetchLocalPathResult);
                    VoiceLog.LogInfo(VoiceTag.IMVoice, "FetchLocalPathCallbcak", voiceFileInfo, result);
                    _fetchLocalPathCallback(result, voiceFileInfo);
                }
                catch (Exception e)
                {
                    VoiceLog.LogException(e);
                }
            }
            else
            {
                VoiceLog.LogWarning(VoiceTag.IMVoice, message:"FetchLocalPathCallbcak is null");
            }
        }
        
        #endregion
    }
}

namespace GSDK.ASRVoice
{
    public class ASRVoice : IASRVoice, IASRVoiceRecordCallback, IIMVoicePlayCallback
    {
        #region Events
        
        public event EngineErrorEventHandler EngineErrorEvent;
        public event StartPlayEventHandler StartPlayEvent;
        public event FinishPlayEventHandler FinishPlayEvent;
        public event TranscribingEventHandler TranscribingEvent;
        public event FinishTranscribeEventHandler FinishTranscribeEvent;
        public event FinishTranscribeAndUploadEventHandler FinishTranscribeAndUploadEvent;
        public event NotifyRecordVolumeEventHandler NotifyRecordVolumeEvent;

        #endregion

        #region Variables

        // VoiceSDK实例
        private readonly BaseVoiceSDK _sdk;
        private InitializeDelegate _initializeDelegate;
        private StartRecordDelegate _startRecordDelegate;
        private StopRecordDelegate _stopRecordDelegate;
        private CancelRecordDelegate _cancelRecordDelegate;

        #endregion

        #region Methods

        public ASRVoice()
        {
            _sdk = GMVoiceMgr.instance.SDK;
        }
        
        public void Initialize(PlayMode playMode, InitializeDelegate initializeDelegate, TargetLanguage language = TargetLanguage.Chinese)
        {
            _initializeDelegate = initializeDelegate;
            VoiceLog.LogInfo(VoiceTag.ASRVoice, string.Format("Initialize, PlayMode:{0}, TargetLanguage:{1}", playMode, language));
            _sdk.InitASR(VoiceInnerTools.Convert(playMode), this, VoiceInnerTools.Convert(language));
        }

        public void Release()
        {
            VoiceLog.LogInfo(VoiceTag.ASRVoice, "Release");
            _sdk.UnInit();
        }

        public void StartRecord(StartRecordDelegate startRecordDelegate)
        {
            this._startRecordDelegate = startRecordDelegate;
            VoiceLog.LogInfo(VoiceTag.ASRVoice, "StartRecord");
            _sdk.StartRecord(OnTranscribeResult);
        }

        public void StopRecord(StopRecordDelegate stopRecordDelegate)
        {
            this._stopRecordDelegate = stopRecordDelegate;
            VoiceLog.LogInfo(VoiceTag.ASRVoice, "StopRecord");
            _sdk.StopRecord();
        }

        public void CancelRecord(CancelRecordDelegate cancelRecordDelegate)
        {
            this._cancelRecordDelegate = cancelRecordDelegate;
            VoiceLog.LogInfo(VoiceTag.ASRVoice, "CancelRecord");
            _sdk.CancelRecord();
        }

        public void StartPlay(List<string> voiceIDs)
        {
            var voiceIdsString = voiceIDs == null ? "" : string.Join(",", voiceIDs.ToArray());
            VoiceLog.LogInfo(VoiceTag.ASRVoice, string.Format("StartPlay, VoiceIds:[({0})]", voiceIdsString));
            _sdk.StartPlayVoice(voiceIDs, this);
        }

        public void StopPlay()
        {
            VoiceLog.LogInfo(VoiceTag.ASRVoice, "StopPlay");
            _sdk.StopPlayVoice();
        }

        #region IASRVoiceRecordCallback实现

        public void OnASRInit(CallbackResult callbackResult)
        {
            if (_initializeDelegate != null)
            {
                try
                {
                    var result = VoiceInnerTools.ConvertVoiceASRError(callbackResult);
                    VoiceLog.LogInfo(VoiceTag.ASRVoice, "Perform InitializeDelegate", result);
                    _initializeDelegate(result);
                }
                catch (Exception e)
                {
                    VoiceLog.LogException(e);
                }
            }
            else
            {
                VoiceLog.LogWarning(VoiceTag.ASRVoice, "InitializeDelegate is null");
            }
        }

        public void OnEngineStart(CallbackResult callbackResult) { }

        public void OnEngineStop(CallbackResult callbackResult) { }

        public void OnEngineError(CallbackResult callbackResult)
        {
            if (EngineErrorEvent != null)
            {
                try
                {
                    var result = VoiceInnerTools.ConvertVoiceASRError(callbackResult);
                    VoiceLog.LogInfo(VoiceTag.ASRVoice, "Perform EngineErrorEvent", result);
                    EngineErrorEvent(result);
                }
                catch (Exception e)
                {
                    VoiceLog.LogException(e);
                }
            }
            else
            {
                VoiceLog.LogWarning(VoiceTag.ASRVoice, "EngineErrorEvent is null");
            }
        }

        public void OnASRStartRecord(CallbackResult callbackResult)
        {
            if (_startRecordDelegate != null)
            {
                try
                {
                    var result = VoiceInnerTools.ConvertVoiceASRError(callbackResult);
                    VoiceLog.LogInfo(VoiceTag.ASRVoice, "Perform StartRecordDelegate", result);
                    _startRecordDelegate(result);
                }
                catch (Exception e)
                {
                    VoiceLog.LogException(e);
                }
            }
            else
            {
                VoiceLog.LogWarning(VoiceTag.ASRVoice, "StartRecordDelegate is null");
            }
        }

        public void OnASRStopRecord(CallbackResult callbackResult)
        {
            if (_stopRecordDelegate != null)
            {
                try
                {
                    var result = VoiceInnerTools.ConvertVoiceASRError(callbackResult);
                    VoiceLog.LogInfo(VoiceTag.ASRVoice, "Perform StopRecordDelegate", result);
                    _stopRecordDelegate(result);
                }
                catch (Exception e)
                {
                    VoiceLog.LogException(e);
                }
            }
            else
            {
                VoiceLog.LogWarning(VoiceTag.ASRVoice, "StopRecordDelegate is null");
            }
        }

        public void OnASRCancelRecord(CallbackResult callbackResult)
        {
            if (_cancelRecordDelegate != null)
            {
                try
                {
                    var result = VoiceInnerTools.ConvertVoiceASRError(callbackResult);
                    VoiceLog.LogInfo(VoiceTag.ASRVoice, "Perform CancelRecordDelegate", result);
                    _cancelRecordDelegate(result);
                }
                catch (Exception e)
                {
                    VoiceLog.LogException(e);
                }
            }
            else
            {
                VoiceLog.LogWarning(VoiceTag.ASRVoice, "CancelRecordDelegate is null");
            }
        }

        #endregion

        #region IIMVoicePlayCallback实现

        public void OnStartPlay(StartPlayResult startPlayResult)
        {
            if (StartPlayEvent != null)
            {
                try
                {
                    var result = VoiceInnerTools.ConvertVoiceASRError(startPlayResult);
                    var startPlayInfo = VoiceInnerTools.Convert(startPlayResult);
                    VoiceLog.LogInfo(VoiceTag.ASRVoice, "Perform StartPlayEvent", startPlayInfo, result);
                    StartPlayEvent(result, startPlayInfo);
                }
                catch (Exception e)
                {
                    VoiceLog.LogException(e);
                }
            }
            else
            {
                VoiceLog.LogWarning(VoiceTag.ASRVoice, "StartPlayEvent is null");
            }
        }

        public void OnFinishPlay(FinishPlayResult finishPlayResult)
        {
            if (FinishPlayEvent != null)
            {
                try
                {
                    var result = VoiceInnerTools.ConvertVoiceASRError(finishPlayResult);
                    var playInfo = VoiceInnerTools.Convert(finishPlayResult);
                    VoiceLog.LogInfo(VoiceTag.ASRVoice, "Perform FinishPlayEvent", playInfo, result);
                    FinishPlayEvent(playInfo);
                }
                catch (Exception e)
                {
                    VoiceLog.LogException(e);
                }
            }
            else
            {
                VoiceLog.LogWarning(VoiceTag.ASRVoice, "FinishPlayEvent is null");
            }
        }

        #endregion

        private void OnTranscribeResult(ASRTranslateResult asrTranscribeResult)
        {
            if (asrTranscribeResult.partialResult != null)
            {
                OnTranscribing(asrTranscribeResult);
            }
            else if (asrTranscribeResult.voiceId != null || !asrTranscribeResult.IsSuccess())
            {
                OnFinishTranscribeAndUpload(asrTranscribeResult);
            }
            else if (asrTranscribeResult.translateResult != null)
            {
                OnFinishTranscribe(asrTranscribeResult);
            }
            else
            {
                OnNotifyRecordVolume(asrTranscribeResult);
            }
        }

        private void OnTranscribing(ASRTranslateResult asrTranscribeResult)
        {
            if (TranscribingEvent != null)
            {
                try
                {
                    var result = VoiceInnerTools.ConvertVoiceASRError(asrTranscribeResult);
                    var transcribingInfo = VoiceInnerTools.ConvertToPartialTranscribeInfo(asrTranscribeResult);
                    VoiceLog.LogInfo(VoiceTag.ASRVoice, "Perform TranscribingEvent", transcribingInfo,result);
                    TranscribingEvent(transcribingInfo);
                }
                catch (Exception e)
                {
                    VoiceLog.LogException(e);
                }
            }
            else
            {
                VoiceLog.LogWarning(VoiceTag.ASRVoice, "TranscribingEvent is null");
            }
        }

        private void OnFinishTranscribe(ASRTranslateResult asrTranscribeResult)
        {
            if (FinishTranscribeEvent != null)
            {
                try
                {
                    var result = VoiceInnerTools.ConvertVoiceASRError(asrTranscribeResult);
                    var finishTranscribeInfo = VoiceInnerTools.ConvertToFinishTranscribeInfo(asrTranscribeResult);
                    VoiceLog.LogInfo(VoiceTag.ASRVoice, "Perform FinishTranscribeEvent", finishTranscribeInfo, result);
                    FinishTranscribeEvent(finishTranscribeInfo);
                }
                catch (Exception e)
                {
                    VoiceLog.LogException(e);
                }
            }
            else
            {
                VoiceLog.LogWarning(VoiceTag.ASRVoice, "FinishTranscribeEvent is null");
            }
        }

        private void OnFinishTranscribeAndUpload(ASRTranslateResult asrTranscribeResult)
        {
            if (FinishTranscribeAndUploadEvent != null)
            {
                try
                {
                    var result = VoiceInnerTools.ConvertVoiceASRError(asrTranscribeResult);
                    var finishTranscribeAndUploadInfo =
                        VoiceInnerTools.ConvertToFinishTranscribeAndUploadInfo(asrTranscribeResult);
                    VoiceLog.LogInfo(VoiceTag.ASRVoice, "Perform FinishTranscribeAndUploadEvent",
                        finishTranscribeAndUploadInfo, result);
                    FinishTranscribeAndUploadEvent(result, finishTranscribeAndUploadInfo);
                }
                catch (Exception e)
                {
                    VoiceLog.LogException(e);
                }
            }
            else
            {
                VoiceLog.LogWarning(VoiceTag.ASRVoice, "FinishTranscribeAndUploadEvent is null");
            }
        }

        private void OnNotifyRecordVolume(ASRTranslateResult asrTranscribeResult)
        {
            if (NotifyRecordVolumeEvent != null)
            {
                try
                {
                    var result = VoiceInnerTools.ConvertVoiceASRError(asrTranscribeResult);
                    var volume = asrTranscribeResult.volume;
                    VoiceLog.LogInfo(VoiceTag.ASRVoice, string.Format("Perform NotifyRecordVolumeEvent, volume:{0}", volume), result);
                    NotifyRecordVolumeEvent(volume);
                }
                catch (Exception e)
                {
                    VoiceLog.LogException(e);
                }
            }
            else
            {
                VoiceLog.LogWarning(VoiceTag.ASRVoice, "NotifyRecordVolumeEvent is null");
            }
        }

        #endregion
    }
}