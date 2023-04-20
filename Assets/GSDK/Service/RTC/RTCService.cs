using System;
using System.Collections.Generic;
using GMSDK;

namespace GSDK
{
    public class RTCService : IRTCService, IRtcVoiceCallback
    {
        #region Events

        public event LeaveRoomEventHandler LeaveRoomEvent;
        public event ErrorEventHandler ErrorEvent;
        public event OtherUserJoinEventHandler OtherUserJoinEvent;
        public event OtherUserLeaveEventHandler OtherUserLeaveEvent;
        public event OtherUserMuteSendAudioEventHandler OtherUserMuteSendAudioEvent;
        public event OtherUserMuteRecvAudioEventHandler OtherUserMuteRecvAudioEvent;
        public event AudioVolumeIndicationEventHandler AudioVolumeIndicationEvent;
        public event ConnectionLostEventHandler ConnectionLostEvent;

        #endregion

        #region Variables
        
        // VoiceSDK实例
        private readonly BaseRTCSDK _sdk;
        private JoinRoomDelegate _joinRoomCallback;
        
        #endregion

        #region Methods

        public RTCService()
        {
            _sdk = GMRTCMgr.instance.SDK;
        }
        
        public void Initialize(ClientRole clientRole)
        {
            _sdk.InitRtcVoice(VoiceInnerTools.Convert(clientRole));
            RTCLog.LogInfo(string.Format("Initialize ,clientRole:{0}", clientRole));
        }

        public void JoinRoom(string roomID, string userID, string token, JoinRoomDelegate joinRoomCallback)
        {
            this._joinRoomCallback = joinRoomCallback;
            _sdk.RtcVoiceJoinRoom(roomID, userID, token, this);
            RTCLog.LogInfo(string.Format("JoinRoom ,roomId:{0} ,uid:{1}", roomID, userID));
        }

        public void LeaveRoom()
        {
            var code = _sdk.RtcVoiceLeaveRoom();
            RTCLog.LogInfo("LeaveRoom ,code:" + code);
        }

        public void UpdateToken(string token)
        {
            var code = _sdk.RtcVoiceUpdateToken(token);
            RTCLog.LogInfo("UpdateToken ,code:" + code);
        }
        public void EnableLocalAudio(bool enabled){
            var code = _sdk.RtcVoiceEnableLocalAudio(enabled);
            RTCLog.LogInfo("EnableLocalAudio ,code:" + code);
        }

        public void SetDefaultEnableLocalAudio(bool enabled){
            var code = _sdk.RtcVoiceSetDefaultEnableLocalAudio(enabled);
            RTCLog.LogInfo("SetDefaultEnableLocalAudio ,code:" + code);
        }
        
        public void AdjustSendAudioVolume(int volume)
        {
            var code = _sdk.RTCAdjustRecordingSignalVolume(volume);
            RTCLog.LogInfo(string.Format("AdjustSendAudioVolume ,volume:{0} ,code:{1}", volume, code));
        }

        public void MuteSendAudioStream(bool mute)
        {
            var code = _sdk.RtcVoiceMuteLocalAudioStream(mute);
            RTCLog.LogInfo(string.Format("MuteSendAudioStream ,mute:{0} ,code:{1}", mute, code));
        }

        public void AdjustRecvAudioVolume(string userID, int volume)
        {
            var code = _sdk.RTCVoiceAdjustRemoteAudioVolume(volume, userID);
            RTCLog.LogInfo(string.Format("AdjustRecvAudioVolume ,volume:{0} ,uid:{1} ,code:{2}", volume, userID, code));
        }

        public void AdjustAllRecvAudioVolume(int volume)
        {
            var code = _sdk.RTCAdjustPlaybackSignalVolume(volume);
            RTCLog.LogInfo(string.Format("AdjustAllRecvAudioVolume ,volume:{0} ,code:{1}", volume, code));
        }

        public void MuteRecvAudioStream(string userID, bool mute)
        {
            var code = _sdk.RtcVoiceMuteRemoteAudioStream(userID, mute);
            RTCLog.LogInfo(string.Format("MuteRecvAudioStream ,uid:{0} ,mute:{1} ,code:{2}", userID, mute, code));
        }

        public void MuteAllRecvAudioStreams(bool mute)
        {
            var code = _sdk.RtcVoiceMuteAllRemoteAudioStreams(mute);
            RTCLog.LogInfo(
                string.Format("MuteAllRecvAudioStreams ,mute:{0} ,code:{1}", mute, code));
        }

        public void SetClientRole(ClientRole clientRole)
        {
            var code = _sdk.RTCVoiceSetClientRole(VoiceInnerTools.Convert(clientRole));
            RTCLog.LogInfo(string.Format("SetClientRole ,clientRole:{0} ,code:{1}", clientRole, code));
        }

        public void SetPerformanceProfileLevel(PerformanceProfileLevel performanceProfileLevel)
        {
            _sdk.RTCSetAudioPerfProfile(VoiceInnerTools.Convert(performanceProfileLevel));
            RTCLog.LogInfo(string.Format("SetPerformanceProfileLevel ,performanceProfileLevel:{0}",performanceProfileLevel.ToString()));
        }

        public bool SetTeamID(string teamID)
        {
            var code = _sdk.RTCConfigTeamId(teamID);
            RTCLog.LogInfo(string.Format("SetTeamId ,teamId:{0} ,code:{1}", teamID, code));
            return code == 0;
        }

        public bool SetAudioSendMode(RTCRangeAudioMode mode)
        {
            var code = _sdk.RTCSetAudioSendMode(VoiceInnerTools.Convert(mode));
            RTCLog.LogInfo(string.Format("SetAudioSendMode ,mode:{0} ,code:{1}", mode, code));
            return code == GSDK.ErrorCode.Success;
        }

        public bool SetAudioRecvMode(RTCRangeAudioMode mode)
        {
            var code = _sdk.RTCSetAudioRecvMode(VoiceInnerTools.Convert(mode));
            RTCLog.LogInfo(string.Format("SetAudioRecvMode ,mode:{0} ,code:{1}", mode, code));
            return code == GSDK.ErrorCode.Success;
        }

        public bool EnableRangeAudio(bool enable)
        {
            var code = _sdk.RtcEnableRangeAudio(enable);
            RTCLog.LogInfo(string.Format("EnableRangeAudio ,enable:{0}",enable));
            return code == GSDK.ErrorCode.Success;
        }

        public bool UpdateAudioRecvRange(int minRange, int maxRange)
        {
            var code = _sdk.RTCUpdateAudioRecvRange(minRange, maxRange);
            RTCLog.LogInfo(string.Format("UpdateAudioRecvRange ,minRange:{0} ,maxRange:{1} ,code:{2}", minRange, maxRange, code));
            return code == GSDK.ErrorCode.Success;
        }

        public bool UpdateSelfPosition(int x, int y, int z)
        {
            var code = _sdk.RTCUpdateSelfPosition(x, y, z);
            RTCLog.LogInfo(string.Format("UpdateSelfPosition ,x:{0} ,y:{1} ,z:{2} ,code:{3}", x, y, z, code));
            return code == GSDK.ErrorCode.Success;
        }

        #region IRTCVoiceCallback实现

        public void OnJoinRoom(JoinRoomResult joinRoomResult)
        {
            if (_joinRoomCallback != null)
            {
                try
                {
                    var result = VoiceInnerTools.ConvertRTCError(joinRoomResult);
                    var joinRoomInfo = VoiceInnerTools.Convert(joinRoomResult);
                    RTCLog.LogInfo("Perform JoinRoomCallback", joinRoomInfo, result);
                    _joinRoomCallback(result, joinRoomInfo);
                }
                catch (Exception e)
                {
                    RTCLog.LogException(e);
                }
            }
            else
            {
                RTCLog.LogWarning("JoinRoomCallback is null");
            }
        }

        public void OnLeaveRoom(CallbackResult callbackResult)
        {
            if (LeaveRoomEvent != null)
            {
                try
                {
                    var result = VoiceInnerTools.ConvertRTCError(callbackResult);
                    RTCLog.LogInfo("Perform LeaveRoomEvent", result);
                    LeaveRoomEvent();
                }
                catch (Exception e)
                {
                    RTCLog.LogException(e);
                }
            }
            else
            {
                RTCLog.LogWarning("LeaveRoomEvent is null");
            }
        }

        public void OnError(CallbackResult callbackResult)
        {
            if (ErrorEvent != null)
            {
                try
                {
                    var result = VoiceInnerTools.ConvertRTCError(callbackResult);
                    RTCLog.LogInfo("Perform ErrorEvent", result);
                    ErrorEvent(result);
                }
                catch (Exception e)
                {
                    RTCLog.LogException(e);
                }
            }
            else
            {
                RTCLog.LogWarning("ErrorEvent is null");
            }
        }

        public void OnUserJoined(UserJoinedResult userJoinedResult)
        {
            if (OtherUserJoinEvent != null)
            {
                try
                {
                    var result = VoiceInnerTools.ConvertRTCError(userJoinedResult);
                    var otherUserJoinedInfo = VoiceInnerTools.Convert(userJoinedResult);
                    RTCLog.LogInfo("Perform OtherUserJoinEvent", otherUserJoinedInfo, result);
                    OtherUserJoinEvent(otherUserJoinedInfo);
                }
                catch (Exception e)
                {
                    RTCLog.LogException(e);
                }
            }
            else
            {
                RTCLog.LogWarning("OtherUserJoinEvent is null");
            }
        }

        public void OnUserOffline(UserOfflineResult userOfflineResult)
        {
            if (OtherUserLeaveEvent != null)
            {
                try
                {
                    var result = VoiceInnerTools.ConvertRTCError(userOfflineResult);
                    var otherUserLeaveInfo = VoiceInnerTools.Convert(userOfflineResult);
                    RTCLog.LogInfo("Perform OtherUserLeaveEvent", otherUserLeaveInfo, result);
                    OtherUserLeaveEvent(otherUserLeaveInfo);
                }
                catch (Exception e)
                {
                    RTCLog.LogException(e);
                }
            }
            else
            {
                RTCLog.LogWarning("OtherUserLeaveEvent is null");
            }
        }

        public void OnUserMuteAudio(AudioMutedResult audioMutedResult)
        {
            if (OtherUserMuteSendAudioEvent != null)
            {
                try
                {
                    var result = VoiceInnerTools.ConvertRTCError(audioMutedResult);
                    var otherUserMuteAudioInfo = VoiceInnerTools.Convert(audioMutedResult);
                    RTCLog.LogInfo("Perform OtherUserMuteSendAudioEvent", otherUserMuteAudioInfo, result);
                    OtherUserMuteSendAudioEvent(otherUserMuteAudioInfo);
                }
                catch (Exception e)
                {
                    RTCLog.LogException(e);
                }
            }
            else
            {
                RTCLog.LogWarning("OtherUserMuteSendAudioEvent is null");
            }
        }

        public void OnUserMuteRemoteAudio(AudioMutedResult audioMutedResult)
        {
            if (OtherUserMuteRecvAudioEvent != null)
            {
                try
                {
                    var result = VoiceInnerTools.ConvertRTCError(audioMutedResult);
                    var otherUserMuteAudioInfo = VoiceInnerTools.Convert(audioMutedResult);
                    RTCLog.LogInfo("Perform OtherUserMuteRecvAudioEvent", otherUserMuteAudioInfo, result);
                    OtherUserMuteRecvAudioEvent(otherUserMuteAudioInfo);
                }
                catch (Exception e)
                {
                    RTCLog.LogException(e);
                }
            }
            else
            {
                RTCLog.LogWarning("OtherUserMuteRecvAudioEvent is null");
            }
        }

        public void OnAudioVolumeIndication(VolumeIndicationResult volumeIndicationResult)
        {
            if (AudioVolumeIndicationEvent != null)
            {
                try
                {
                    var result = VoiceInnerTools.ConvertRTCError(volumeIndicationResult);
                    var audioVolumeIndicationInfo = VoiceInnerTools.Convert(volumeIndicationResult);
                    RTCLog.LogInfo("Perform AudioVolumeIndicationEvent", audioVolumeIndicationInfo, result);
                    AudioVolumeIndicationEvent(audioVolumeIndicationInfo);
                }
                catch (Exception e)
                {
                    RTCLog.LogException(e);
                }
            }
            else
            {
                RTCLog.LogWarning("AudioVolumeIndicationEvent is null");
            }
        }

        public void OnConnectionLost(CallbackResult callbackResult)
        {
            if (ConnectionLostEvent != null)
            {
                try
                {
                    var result = VoiceInnerTools.ConvertRTCError(callbackResult);
                    RTCLog.LogInfo("Perform ConnectionLostEvent", result);
                    ConnectionLostEvent();
                }
                catch (Exception e)
                {
                    RTCLog.LogException(e);
                }
            }
            else
            {
                RTCLog.LogWarning("ConnectionLostEvent is null");
            }
        }

        #endregion

        #endregion
    }
}