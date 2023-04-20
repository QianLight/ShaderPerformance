using System;
using UNBridgeLib;
using UNBridgeLib.LitJson;
using UnityEngine;
using GMSDK;

namespace GameRTC
{
    public class GameRTCCallbackGSDK : BridgeCallBack
    {
        public IGameRTCCallback GameRTCCallback;

        public GameRTCCallbackGSDK()
        {
            OnFailed = OnFailCallback;
            OnTimeout = OnTimeOutCallback;
        } 

        public void OnJoinRoomResultCallback(JsonData jd)
        {
            LogUtils.D("GameRTC - OnJoinRoomResultCallback");
            JoinRoomResult result = SdkUtil.ToObject<JoinRoomResult>(jd.ToJson());
            if (GameRTCCallback != null) 
            {
                SdkUtil.InvokeAction<JoinRoomResult>(GameRTCCallback.OnJoinRoomResult, result);
            }
        }

        public void OnLeaveRoomCallback(JsonData jd)
        {
            LogUtils.D("GameRTC - OnLeaveRoomCallback");
            LeaveRoomResult result = SdkUtil.ToObject<LeaveRoomResult>(jd.ToJson());
            if (GameRTCCallback != null)
            {
                SdkUtil.InvokeAction<LeaveRoomResult>(GameRTCCallback.OnLeaveRoom, result);
            }
        }

        public void OnUserJoinedCallback(JsonData jd)
        {
            LogUtils.D("GameRTC - OnUserJoinedCallback");
            UserJoinedResult result = SdkUtil.ToObject<UserJoinedResult>(jd.ToJson());
            if (GameRTCCallback != null)
            {
                SdkUtil.InvokeAction<UserJoinedResult>(GameRTCCallback.OnUserJoined, result);
            }
        }

        public void OnUserLeaveCallback(JsonData jd)
        {
            LogUtils.D("GameRTC - OnUserLeaveCallback");
            UserLeaveResult result = SdkUtil.ToObject<UserLeaveResult>(jd.ToJson());
            if (GameRTCCallback != null)
            {
                SdkUtil.InvokeAction<UserLeaveResult>(GameRTCCallback.OnUserLeave, result);
            }
        }

        public void OnMicrophoneEnabledCallback(JsonData jd)
        {
            LogUtils.D("GameRTC - OnMicrophoneEnabledCallback");
            MicrophoneEnabledResult result = SdkUtil.ToObject<MicrophoneEnabledResult>(jd.ToJson());
            if (GameRTCCallback != null)
            {
                SdkUtil.InvokeAction<MicrophoneEnabledResult>(GameRTCCallback.OnMicrophoneEnabled, result);
            }
        }

        public void OnAudioSendEnabledCallback(JsonData jd)
        {
            LogUtils.D("GameRTC - OnAudioSendEnabledCallback");
            AudioSendEnabledResult result = SdkUtil.ToObject<AudioSendEnabledResult>(jd.ToJson());
            if (GameRTCCallback != null)
            {
                SdkUtil.InvokeAction<AudioSendEnabledResult>(GameRTCCallback.OnAudioSendEnabled, result);
            }
        }

        public void OnSpeakerphoneEnabledCallback(JsonData jd)
        {
            LogUtils.D("GameRTC - OnSpeakerphoneEnabledCallback");
            SpeakerphoneEnabledResult result = SdkUtil.ToObject<SpeakerphoneEnabledResult>(jd.ToJson());
            if (GameRTCCallback != null)
            {
                SdkUtil.InvokeAction<SpeakerphoneEnabledResult>(GameRTCCallback.OnSpeakerphoneEnabled, result);
            }
        }

        public void OnAudioVolumeIndicationCallback(JsonData jd)
        {
            LogUtils.D("GameRTC - OnAudioVolumeIndicationCallback");
            AudioVolumeIndicationResult result = SdkUtil.ToObject<AudioVolumeIndicationResult>(jd.ToJson());
            if (GameRTCCallback != null)
            {
                SdkUtil.InvokeAction<AudioVolumeIndicationResult>(GameRTCCallback.OnAudioVolumeIndication, result);
            }
        }

        public void OnNetworkQualityCallback(JsonData jd)
        {
            LogUtils.D("GameRTC - OnNetworkQualityCallback");
            NetworkQualityResult result = SdkUtil.ToObject<NetworkQualityResult>(jd.ToJson());
            if (GameRTCCallback != null)
            {
                SdkUtil.InvokeAction<NetworkQualityResult>(GameRTCCallback.OnNetworkQuality, result);
            }
        }

        public void OnConnectionStateChangedCallback(JsonData jd)
        {
            LogUtils.D("GameRTC - OnConnectionStateChangedCallback");
            ConnectionStateChangedResult result = SdkUtil.ToObject<ConnectionStateChangedResult>(jd.ToJson());
            if (GameRTCCallback != null)
            {
                SdkUtil.InvokeAction<ConnectionStateChangedResult>(GameRTCCallback.OnConnectionStateChanged, result);
            }
        }

        public void OnRoomWarningCallback(JsonData jd)
        {
            LogUtils.D("GameRTC - OnRoomWarningCallback");
            RoomWarningResult result = SdkUtil.ToObject<RoomWarningResult>(jd.ToJson());
            if (GameRTCCallback != null)
            {
                SdkUtil.InvokeAction<RoomWarningResult>(GameRTCCallback.OnRoomWarning, result);
            }
        }

        public void OnRoomErrorCallback(JsonData jd)
        {
            LogUtils.D("GameRTC - OnRoomErrorCallback");
            RoomErrorResult result = SdkUtil.ToObject<RoomErrorResult>(jd.ToJson());
            if (GameRTCCallback != null)
            {
                SdkUtil.InvokeAction<RoomErrorResult>(GameRTCCallback.OnRoomError, result);
            }
        }

        public void OnEngineWarningCallback(JsonData jd)
        {
            LogUtils.D("GameRTC - OnEngineWarningCallback");
            EngineWarningResult result = SdkUtil.ToObject<EngineWarningResult>(jd.ToJson());
            if (GameRTCCallback != null)
            {
                SdkUtil.InvokeAction<EngineWarningResult>(GameRTCCallback.OnEngineWarning, result);
            }
        }

        public void OnFailCallback(int code, string failMsg)
        {
            LogUtils.E("GameRTC - OnFailCallback");
            LogUtils.E("Interface access failed: " + code.ToString() + " " + failMsg);
        }

        public void OnTimeOutCallback()
        {
            JsonData jd = new JsonData();
            jd["code"] = -321;
            jd["message"] = "GameRTC - request time out";
            if (this.OnSuccess != null)
            {
                this.OnSuccess(jd);
            }
        }
    }
}