using System.Collections.Generic;
using CFUtilPoolLib.GSDK;
using GSDK;

/// <summary>
/// GSDK接口——非实时语音接口
/// </summary>
public partial class GSDKSystem
{
    private CFUtilPoolLib.GSDK.StartRecordDelegate m_startRecordCallback;
    private CFUtilPoolLib.GSDK.FinishRecordEventHandler m_finishRecordCallback;
    private CFUtilPoolLib.GSDK.FinishPlayEventHandler m_finishPlayCallback;
    private CFUtilPoolLib.GSDK.StartPlayEventHandler m_startPlayCallback;
    private CFUtilPoolLib.GSDK.TranscribeDelegate m_transcribeCallback;
    private CFUtilPoolLib.GSDK.FetchLocalPathDelegate m_fetchLocalPathCallback;

    #region 非实时语音模块
    public void IMInitialize(CFUtilPoolLib.GSDK.PlayMode playMode, int expireTime = 2592000, string savePath = "")
    {
        Voice.Service.IMVoice.Initialize((GSDK.IMVoice.PlayMode)playMode, expireTime, savePath);
    }


    public void RegisterFinishRecordEventHandlerCallback(FinishRecordEventHandler finishRecordCallback)
    {
        m_finishRecordCallback = finishRecordCallback;
        Voice.Service.IMVoice.FinishRecordEvent += FinishRecordEventHandlerCallback;
    }


    public void RegisterFinishPlayEventHandlerCallback(FinishPlayEventHandler finishPlayCallback)
    {
        m_finishPlayCallback = finishPlayCallback;
        Voice.Service.IMVoice.FinishPlayEvent += FinishPlayEventCallabck;
    }


    public void RegisterStartPlayEventHandlerCallback(StartPlayEventHandler startPlayCallback)
    {
        m_startPlayCallback = startPlayCallback;
        Voice.Service.IMVoice.StartPlayEvent += StartPlayEventHandlerCallback;
    }


    public void UnRegisterFinishRecordEventHandlerCallback()
    {
        m_finishRecordCallback = null;
        Voice.Service.IMVoice.FinishRecordEvent -= FinishRecordEventHandlerCallback;
    }


    public void UnRegisterFinishPlayEventHandlerCallback()
    {
        m_finishPlayCallback = null;
        Voice.Service.IMVoice.FinishPlayEvent -= FinishPlayEventCallabck;
    }


    public void UnRegisterStartPlayEventHandlerCallback()
    {
        m_startPlayCallback = null;
        Voice.Service.IMVoice.StartPlayEvent -= StartPlayEventHandlerCallback;
    }

    private void StartPlayEventHandlerCallback(GSDK.Result result, GSDK.IMVoice.PlayInfo playInfo)
    {
        if (m_startPlayCallback != null)
        {
            CFUtilPoolLib.GSDK.Result gResult = GetGSDKResult(result);
            CFUtilPoolLib.GSDK.PlayInfo gPlayInfo = GetPlayInfo(playInfo);
            m_startPlayCallback(gResult, gPlayInfo);
        }
    }

    private void FinishRecordEventHandlerCallback(GSDK.Result result, GSDK.IMVoice.FinishRecordInfo finishRecordInfo)
    {
        if (m_finishRecordCallback != null)
        {
            CFUtilPoolLib.GSDK.Result gResult = GetGSDKResult(result);
            CFUtilPoolLib.GSDK.FinishRecordInfo gFnishRecordInfo = GetFinishRecordInfo(finishRecordInfo);
            m_finishRecordCallback(gResult, gFnishRecordInfo);
        }
    }

    private void FinishPlayEventCallabck(GSDK.IMVoice.PlayInfo playInfo)
    {
        if (m_finishPlayCallback != null)
        {
            CFUtilPoolLib.GSDK.PlayInfo gPlayInfo = GetPlayInfo(playInfo);
            m_finishPlayCallback(gPlayInfo);
        }
    }

    public void IMRelease()
    {
        Voice.Service.IMVoice.Release();
    }

    public void IMStartRecord(StartRecordDelegate startRecordCallback)
    {
        m_startRecordCallback = startRecordCallback;
        Voice.Service.IMVoice.StartRecord(StartRecordDelegateCallback);
    }

    private void StartRecordDelegateCallback(GSDK.Result result)
    {
        if (m_startRecordCallback != null)
        {
            CFUtilPoolLib.GSDK.Result gresult = GetGSDKResult(result);
            m_startRecordCallback(gresult);
        }
    }

    public void IMStopRecord()
    {
        Voice.Service.IMVoice.StopRecord();
    }

    public void IMCancelRecord()
    {
        Voice.Service.IMVoice.CancelRecord();
    }

    public void IMStartPlay(string voiceID)
    {
        Voice.Service.IMVoice.StartPlay(voiceID);
    }

    public void IMStartPlay(List<string> voiceIDs)
    {
        for (int i = 0; i < voiceIDs.Count; ++i)
        {
            Voice.Service.IMVoice.StartPlay(voiceIDs[i]);
        }
    }

    public void IMStopPlay()
    {
        Voice.Service.IMVoice.StopPlay();
    }

    public void IMTranscribe(string voiceID, TranscribeDelegate callback)
    {
        m_transcribeCallback = callback;
        Voice.Service.IMVoice.Transcribe(voiceID, TranscribeDelegateCallback);
    }

    private void TranscribeDelegateCallback(GSDK.Result result, GSDK.IMVoice.TranscribeInfo transcribeInfo)
    {
        if (m_transcribeCallback != null)
        {
            CFUtilPoolLib.GSDK.Result gResult = GetGSDKResult(result);
            CFUtilPoolLib.GSDK.TranscribeInfo gTranscribeInfo = GetTranscribeInfo(transcribeInfo);
            m_transcribeCallback(gResult, gTranscribeInfo);
        }
    }

    public void IMFetchLocalPath(string voiceID, FetchLocalPathDelegate callback)
    {
        m_fetchLocalPathCallback = callback;
        Voice.Service.IMVoice.FetchLocalPath(voiceID, FetchLocalPathDelegateCallback);
    }

    private void FetchLocalPathDelegateCallback(GSDK.Result result, GSDK.IMVoice.VoiceFileInfo fileInfo)
    {
        if (m_fetchLocalPathCallback != null)
        {
            CFUtilPoolLib.GSDK.Result gResult = GetGSDKResult(result);
            CFUtilPoolLib.GSDK.VoiceFileInfo gTranscribeInfo = GetVoiceFileInfo(fileInfo);
            m_fetchLocalPathCallback(gResult, gTranscribeInfo);
        }
    }


    #endregion
}
