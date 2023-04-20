using System.Collections.Generic;
using GMSDK;

namespace GameRTC
{
    public class GameRTCMethodName
    {
        public const string registerGameRTC = "registerGameRTC";
        public const string initialize = "Initialize";
        public const string initGameRTCEngine = "InitGameRTCEngine";
        public const string release = "Release";
        public const string getSdkVersion = "GetSdkVersion";
        public const string joinRoom = "JoinRoom";
        public const string leaveRoom = "LeaveRoom";
        public const string updateToken = "UpdateToken";
        public const string updateReceiveRange = "UpdateReceiveRange";
        public const string updatePosition = "UpdatePosition";
        public const string updateOrientation = "UpdateOrientation";
        public const string enableMicrophone = "EnableMicrophone";
        public const string enableAudioSend = "EnableAudioSend";
        public const string enableSpeakerphone = "EnableSpeakerphone";
        public const string enableAudioReceive = "EnableAudioReceive";
        public const string setRecordingVolume = "SetRecordingVolume";
        public const string setPlaybackVolume = "SetPlaybackVolume";
        public const string setRemoteAudioPlaybackVolume = "SetRemoteAudioPlaybackVolume";
        public const string setAudioScenario = "SetAudioScenario";
        public const string setAudioProfile = "SetAudioProfile";
        public const string setVoiceChangerType = "SetVoiceChangerType";
        public const string setVoiceReverbType = "SetVoiceReverbType";
    }

    public class GameRTCResultName
    {
        public const string onJoinRoomResult = "OnJoinRoomResultEventHandler";
        public const string onLeaveRoom = "OnLeaveRoomEventHandler";
        public const string onUserJoined = "OnUserJoinedEventHandler";
        public const string onUserLeave = "OnUserLeaveEventHandler";
        public const string onMicrophoneEnabled = "OnMicrophoneEnabledEventHandler";
        public const string onAudioSendEnabled = "OnAudioSendEnabledEventHandler";
        public const string onSpeakerphoneEnabled = "OnSpeakerphoneEnabledEventHandler";
        public const string onAudioVolumeIndication = "OnAudioVolumeIndicationEventHandler";
        public const string onNetworkQuality = "OnNetworkQualityEventHandler";
        public const string onConnectionStateChanged = "OnConnectionStateChangedEventHandler";
        public const string onRoomWarning = "OnRoomWarningEventHandler";
        public const string onRoomError = "OnRoomErrorEventHandler";
        public const string onEngineWarning = "OnEngineWarningEventHandler";
    }

    public class JoinRoomResult : CallbackResult
    {
        public string roomID;
        public string userID;
        public JoinRoomErrorCode errorCode;
        public int isRejoined;
        public int elapsed;
    }

    public class LeaveRoomResult : CallbackResult
    {
        public string roomID;
    }

    public class UserJoinedResult : CallbackResult
    {
        public string roomID;
        public string userID;
    }

    public class UserLeaveResult : CallbackResult
    {
        public string roomID;
        public string userID;
        public UserLeaveReasonType reason;
    }

    public class MicrophoneEnabledResult : CallbackResult
    {
        public string roomID;
        public string userID;
        public int enable;
    }

    public class AudioSendEnabledResult : CallbackResult
    {
        public string roomID;
        public string userID;
        public int enable;
    }

    public class SpeakerphoneEnabledResult : CallbackResult
    {
        public string roomID;
        public string userID;
        public int enable;
    }

    public class AudioVolumeIndicationResult : CallbackResult
    {
        public string roomID;
        public List<GameRTCAudioVolumeInfo> speakers;
        public int totalVolume;
    }

    public class NetworkQualityResult : CallbackResult
    {
        public string roomID;
        public string userID;
        public NetworkQuality txQuality;
        public NetworkQuality rxQuality;
    }

    public class ConnectionStateChangedResult : CallbackResult
    {
        public ConnectionState state;
    }

    public class RoomWarningResult : CallbackResult
    {
        public string roomID;
        public RoomWarnCode warn;
    }

    public class RoomErrorResult : CallbackResult
    {
        public string roomID;
        public RoomErrorCode err;
    }

    public class EngineWarningResult : CallbackResult
    {
        public EngineWarnCode warn;
    }

    public interface IGameRTCCallback
    {
        void OnJoinRoomResult(JoinRoomResult result);

        void OnLeaveRoom(LeaveRoomResult result);

        void OnUserJoined(UserJoinedResult result);

        void OnUserLeave(UserLeaveResult result);

        void OnMicrophoneEnabled(MicrophoneEnabledResult result);

        void OnAudioSendEnabled(AudioSendEnabledResult result);

        void OnSpeakerphoneEnabled(SpeakerphoneEnabledResult result);

        void OnAudioVolumeIndication(AudioVolumeIndicationResult result);

        void OnConnectionStateChanged(ConnectionStateChangedResult result);

        void OnNetworkQuality(NetworkQualityResult result);

        void OnRoomWarning(RoomWarningResult result);

        void OnRoomError(RoomErrorResult result);

        void OnEngineWarning(EngineWarningResult result);
    }
}