using System.Collections.Generic;
using CFUtilPoolLib;
using GameRTC;

/// <summary>
/// GSDK接口——实时语音接口
/// </summary>
public partial class GSDKSystem
{
    private GameRTCService m_gameRTCService;
    private CFUtilPoolLib.GSDK.OnJoinRoomResultEventHandler m_gameJoinRoomEventCallback;
    private CFUtilPoolLib.GSDK.OnLeaveRoomEventHandler m_gameLeaveRoomEventCallback;
    private CFUtilPoolLib.GSDK.OnUserJoinedEventHandler m_gameOtherUserJoinEventCallback;
    private CFUtilPoolLib.GSDK.OnUserLeaveEventHandler m_gameOtherUserLeaveEventCallback;
    private CFUtilPoolLib.GSDK.OnAudioVolumeIndicationEventHandler m_gameAudioVolumeIndicationEventCallback;

    #region 新的RTC接口
    public void GameRTCInitialize(CFUtilPoolLib.GSDK.GameRTCEngineParams initParams)
    {
        if (m_gameRTCService == null)
        {
            m_gameRTCService = new GameRTCService();
        }
        GameRTC.GameRTCEngineParams rtcParams = new GameRTC.GameRTCEngineParams();
        rtcParams.AreaCode = initParams.AreaCode;
        rtcParams.IsLeaveRoomReleaseEngine = initParams.IsLeaveRoomReleaseEngine;
        rtcParams.AppID = initParams.AppID;
        rtcParams.Params = initParams.Params;
        m_gameRTCService.Initialize(rtcParams);
        XDebug.singleton.AddGreenLog("GameRTCInitialize rtcParams.AreaCode=" + rtcParams.AreaCode + ",rtcParams.IsLeaveRoomReleaseEngine=" + rtcParams.IsLeaveRoomReleaseEngine + "," +
            ",rtcParams.AppID=" + rtcParams.AppID);
    }

    public void GameRTCJoinRoom(string roomID, string userID, string token, CFUtilPoolLib.GSDK.GameRTCRoomConfig roomConfig)
    {
        if (m_gameRTCService == null) return;
        GameRTC.GameRTCRoomConfig config = new GameRTC.GameRTCRoomConfig();
        config.EnableRangeAudio = roomConfig.EnableRangeAudio;
        config.EnableSpatialAudio = roomConfig.EnableSpatialAudio;
        config.AudioVolumeIndicationInterval = roomConfig.AudioVolumeIndicationInterval;
        config.RoomType = (GameRTC.RoomType)roomConfig.RoomType;
        m_gameRTCService.JoinRoom(roomID, userID, token, config);
        XDebug.singleton.AddGreenLog("GameRTCJoinRoom config.EnableRangeAudio=" + config.EnableRangeAudio + ",config.EnableSpatialAudio=" + config.EnableSpatialAudio +
            ",config.AudioVolumeIndicationInterval=" + config.AudioVolumeIndicationInterval + ",config.RoomType=" + config.RoomType);
    }

    public void GameRTCLeaveRoom(string roomID)
    {
        if (m_gameRTCService == null) return;
        m_gameRTCService.LeaveRoom(roomID);
    }

    public void GameRTCMuteRecvAudioStream(string userID, bool mute)
    {
        if (m_gameRTCService == null) return;
    }

    /// <summary>
    /// 通过EnableSpeakerphone控制能否听到别人的声音
    /// </summary>
    /// <param name="roomID"></param>
    /// <param name="enable"></param>
    public void GameRTCMuteAllRecvAudioStreams(string roomID, bool enable)
    {
        if (m_gameRTCService == null) return;
        m_gameRTCService.EnableSpeakerphone(roomID, enable);
        XDebug.singleton.AddGreenLog("GameRTCMuteAllRecvAudioStreams roomID=" + roomID + ",enable=" + enable);
    }

    /// <summary>
    /// 通过EnableMicrophone控制自己采集和发送
    /// </summary>
    /// <param name="mute"></param>
    public void GameRTCMuteSendAudioStream(string roomID, bool enable)
    {
        if (m_gameRTCService == null) return;
        m_gameRTCService.EnableMicrophone(roomID, enable);
        XDebug.singleton.AddGreenLog("GameRTCMuteSendAudioStream roomID=" + roomID + ",enable=" + enable);
    }

    /// <summary>
    /// 自己加入房间
    /// </summary>
    /// <param name="joinRoomEventHandler"></param>
    public void GameRTCRegisterJoinRoomEvent(CFUtilPoolLib.GSDK.OnJoinRoomResultEventHandler joinRoomEventHandler)
    {
        if (m_gameRTCService == null) return;
        m_gameJoinRoomEventCallback = joinRoomEventHandler;
        m_gameRTCService.OnJoinRoomResultEvent += OnJoinRoomResultEventCallback;
        XDebug.singleton.AddGreenLog("GameRTCRegisterJoinRoomEvent");
    }

    public void OnJoinRoomResultEventCallback(string roomID, string userID, GameRTC.JoinRoomErrorCode errorCode, bool isRejoined, int elapsed)
    {
        if (m_gameJoinRoomEventCallback != null)
        {
            m_gameJoinRoomEventCallback(roomID, userID, (CFUtilPoolLib.GSDK.JoinRoomErrorCode)errorCode, isRejoined, elapsed);
            XDebug.singleton.AddGreenLog("OnJoinRoomResultEventCallback roomID=" + roomID + ",userID=" + userID + ",errorCode=" + errorCode + ",isRejoined=" + isRejoined + ",elapsed=" + elapsed);
        }
    }

    /// <summary>
    /// 自己离开房间
    /// </summary>
    /// <param name="leaveRoomEventHandler"></param>
    public void GameRTCRegisterLeaveRoomEvent(CFUtilPoolLib.GSDK.OnLeaveRoomEventHandler leaveRoomEventHandler)
    {
        if (m_gameRTCService == null) return;
        m_gameLeaveRoomEventCallback = leaveRoomEventHandler;
        m_gameRTCService.OnLeaveRoomEvent += OnLeaveRoomEventCallback;
        XDebug.singleton.AddGreenLog("GameRTCRegisterLeaveRoomEvent");
    }

    public void OnLeaveRoomEventCallback(string roomID)
    {
        if (m_gameLeaveRoomEventCallback != null)
        {
            m_gameLeaveRoomEventCallback(roomID);
            XDebug.singleton.AddGreenLog("OnLeaveRoomEventCallback roomID=" + roomID);
        }
    }

    /// <summary>
    /// 别人加入房间
    /// </summary>
    /// <param name="otherUserJoinEventHandler"></param>
    public void GameRTCRegisterOtherUserJoinEventHandler(CFUtilPoolLib.GSDK.OnUserJoinedEventHandler otherUserJoinEventHandler)
    {
        if (m_gameRTCService == null) return;
        m_gameOtherUserJoinEventCallback = otherUserJoinEventHandler;
        m_gameRTCService.OnUserJoinedEvent += OnUserJoinedEventCallback;
        XDebug.singleton.AddGreenLog("GameRTCRegisterOtherUserJoinEventHandler");
    }

    public void OnUserJoinedEventCallback(string roomID, string userID)
    {
        if (m_gameOtherUserJoinEventCallback != null)
        {
            m_gameOtherUserJoinEventCallback(roomID, userID);
            XDebug.singleton.AddGreenLog("OnUserJoinedEventCallback roomID=" + roomID + ",userID=" + userID);
        }
    }

    /// <summary>
    /// 别人离开房间
    /// </summary>
    /// <param name="otherUserLeaveEventHandler"></param>
    public void GameRTCRegisterOtherUserLeaveEventHandler(CFUtilPoolLib.GSDK.OnUserLeaveEventHandler otherUserLeaveEventHandler)
    {
        if (m_gameRTCService == null) return;
        m_gameOtherUserLeaveEventCallback = otherUserLeaveEventHandler;
        m_gameRTCService.OnUserLeaveEvent += OnUserLeaveEventCallback;
    }

    public void OnUserLeaveEventCallback(string roomID, string userID, GameRTC.UserLeaveReasonType reason)
    {
        if (m_gameOtherUserLeaveEventCallback != null)
        {
            m_gameOtherUserLeaveEventCallback(roomID, userID, (CFUtilPoolLib.GSDK.UserLeaveReasonType)reason);
            XDebug.singleton.AddGreenLog("OnUserLeaveEventCallback roomID=" + roomID + ",userID=" + userID + ",reason=" + reason);
        }
    }

    /// <summary>
    /// 音量变化监听
    /// </summary>
    /// <param name="audioVolumeIndicationEventHandler"></param>
    public void GameRTCRegisterAudioVolumeIndicationEventHandler(CFUtilPoolLib.GSDK.OnAudioVolumeIndicationEventHandler audioVolumeIndicationEventHandler)
    {
        if (m_gameRTCService == null) return;
        m_gameAudioVolumeIndicationEventCallback = audioVolumeIndicationEventHandler;
        m_gameRTCService.OnAudioVolumeIndicationEvent += OnAudioVolumeIndicationEventCallback;
        XDebug.singleton.AddGreenLog("GameRTCRegisterAudioVolumeIndicationEventHandler");
    }

    private List<CFUtilPoolLib.GSDK.GameRTCAudioVolumeInfo> m_gSpeakers = new List<CFUtilPoolLib.GSDK.GameRTCAudioVolumeInfo>();
    private List<CFUtilPoolLib.GSDK.GameRTCAudioVolumeInfo> m_cachedSpeakers = new List<CFUtilPoolLib.GSDK.GameRTCAudioVolumeInfo>();
    private void OnAudioVolumeIndicationEventCallback(string roomID, List<GameRTC.GameRTCAudioVolumeInfo> speakers, int totalVolume)
    {
        if (m_gameAudioVolumeIndicationEventCallback != null)
        {
            if (speakers != null)
            {
                for (int i = 0; i < speakers.Count; ++i)
                {
                    CFUtilPoolLib.GSDK.GameRTCAudioVolumeInfo info = GetGameRTCAudioVolumeInfo();
                    info.Volume = speakers[i].Volume;
                    info.UserID = speakers[i].UserID;
                    m_gSpeakers.Add(info);
                }
            }
            m_gameAudioVolumeIndicationEventCallback(roomID, m_gSpeakers, totalVolume);
            ReturnGameRTCAudioVolumeInfo();
        }
    }

    private CFUtilPoolLib.GSDK.GameRTCAudioVolumeInfo GetGameRTCAudioVolumeInfo()
    {
        CFUtilPoolLib.GSDK.GameRTCAudioVolumeInfo info;
        if (m_cachedSpeakers.Count > 0)
        {
            info = m_cachedSpeakers[m_cachedSpeakers.Count - 1];
            m_cachedSpeakers.RemoveAt(m_cachedSpeakers.Count - 1);
        }
        else
        {
            info = new CFUtilPoolLib.GSDK.GameRTCAudioVolumeInfo();
        }
        return info;
    }

    private void ReturnGameRTCAudioVolumeInfo()
    {
        m_cachedSpeakers.AddRange(m_gSpeakers);
        m_gSpeakers.Clear();
    }

    public void GameRTCUnRegisterJoinRoomEvent()
    {
        if (m_gameRTCService == null) return;
        m_gameJoinRoomEventCallback = null;
        m_gameRTCService.OnJoinRoomResultEvent -= OnJoinRoomResultEventCallback;
        XDebug.singleton.AddGreenLog("GameRTCUnRegisterJoinRoomEvent");
    }

    public void GameRTCUnRegisterLeaveRoomEvent()
    {
        if (m_gameRTCService == null) return;
        m_gameLeaveRoomEventCallback = null;
        m_gameRTCService.OnLeaveRoomEvent -= OnLeaveRoomEventCallback;
        XDebug.singleton.AddGreenLog("GameRTCUnRegisterLeaveRoomEvent");
    }

    public void GameRTCUnRegisterOtherUserJoinEventHandler()
    {
        if (m_gameRTCService == null) return;
        m_gameOtherUserJoinEventCallback = null;
        m_gameRTCService.OnUserJoinedEvent -= OnUserJoinedEventCallback;
        XDebug.singleton.AddGreenLog("GameRTCUnRegisterOtherUserJoinEventHandler");
    }

    public void GameRTCUnRegisterOtherUserLeaveEventHandler()
    {
        if (m_gameRTCService == null) return;
        m_gameOtherUserLeaveEventCallback = null;
        m_gameRTCService.OnUserLeaveEvent -= OnUserLeaveEventCallback;
        XDebug.singleton.AddGreenLog("GameRTCUnRegisterOtherUserLeaveEventHandler");
    }

    public void GameRTCUnRegisterAudioVolumeIndicationEventHandler()
    {
        if (m_gameRTCService == null) return;
        m_gameAudioVolumeIndicationEventCallback = null;
        m_gameRTCService.OnAudioVolumeIndicationEvent -= OnAudioVolumeIndicationEventCallback;
        XDebug.singleton.AddGreenLog("GameRTCUnRegisterAudioVolumeIndicationEventHandler");
    }
    #endregion
}
