using System.Collections.Generic;

namespace GMSDK
{
    public class RTCMethodName
    {
        public const string Init = "registerRTC";
        public const string InitRtc = "requestInitRtc";
        public const string RtcJoinRoom = "requestJoinRoom";
        public const string RtcLeaveRoom = "requestLeaveRoom";
        public const string RtcMuteLocalAudioStream = "requestMuteLocalAudioStream";
        public const string RtcMuteRemoteAudioStream = "requestMuteRemoteAudioStream";
        public const string RtcEnableLocalAudio = "requestEnableLocalAudio";
        public const string RtcSetDefaultLocalAudioEnable = "requestSetDefaultLocalAudioEnable";
        public const string RtcMuteAllRemoteAudioStreams = "requestMuteAllRemoteAudioStreams";
        public const string RTCAdjustRemoteAudioVolume = "requestRTCAdjustRemoteAudioVolume";
        public const string RtcUpdateToken = "requestUpdateToken";
        public const string RtcSetClientRole = "requestSetClientRole";
        public const string RTCAdjustRecordingSignalVolume = "requestAdjustRecordingSignalVolume";
        public const string RTCAdjustPlaybackSignalVolume = "requestAdjustPlaybackSignalVolume";
        public const string RTCEnableRangeAudio = "requestEnableRangeAudio";
        public const string RTCSetAudioPerfProfile = "requestSetAudioPerfProfile";
        public const string RTCConfigTeamId = "requestConfigTeamId";
        public const string RTCSetAudioSendMode = "requestSetAudioSendMode";
        public const string RTCSetAudioRecvMode = "requestSetAudioRecvMode";
        public const string RTCUpdateAudioRecvRange = "requestUpdateAudioRecvRange";
        public const string RTCUpdateSelfPosition = "requestUpdateSelfPosition";
    }

    public class RTCResultName
    {
        public const string RtcJoinRoomResult = "requestJoinRoomResult";
        public const string RtcErrorResult = "requestErrorResult";
        public const string RtcUserJoinedResult = "requestUserJoinedResult";
        public const string RtcUserOfflineResult = "requestUserOfflineResult";
        public const string RtcUserMuteResult = "requestUserMuteResult";
        public const string RtcUserMuteRemoteResult = "requestUserMuteRemoteResult";
        public const string RtcVolumeIndicationResult = "requestVolumeIndicationResult";
        public const string RtcConnectionLostResult = "requestConnectionLostResult";
        public const string RtcLeaveRoomResult = "requestLeaveRoomResult";
    }

    /// <summary>
    /// 其他用户音量大小
    /// </summary>
    public class GMRTCAudioVolumeInfo
    {
        public string uid;
        public int volume;
    }

    //加入房间回调信息
    public class JoinRoomResult : CallbackResult
    {
        public string roomId;
        public string userId;
        public int elapsed;
    }

    public class UserJoinedResult : CallbackResult
    {
        public string userId;
        public int elapsed;
    }

    public class UserOfflineResult : CallbackResult
    {
        public string userId;
        public int reason;
    }

    public class AudioMutedResult : CallbackResult
    {
        public string userId;
        public int isMuted;
    }

    public class VolumeIndicationResult : CallbackResult
    {
        public List<GMRTCAudioVolumeInfo> speakers;
        public int totalVolume;
    }

    // 实时语音局部语音音频模式
    public enum GMRTCRangeAudioMode
    {
        GMRTCRangeAudioModeMute = 0, //静音
        GMRTCRangeAudioModeTeam = 1, //小队范围
        GMRTCRangeAudioModeWorld = 2, //世界范围
    }

    // 设置用户机型等级
    public enum GMRTCAudioPerfProfile
    {
        GMRTCAudioPerfProfileAUTO = 0, //自动
        GMRTCAudioPerfProfileLOW = 1, //低端
        GMRTCAudioPerfProfileMID = 2, //中端
        GMRTCAudioPerfProfileHIGH = 3, //高端
    }

    // 实时语音的回调
    public interface IRtcVoiceCallback
    {

        // 本人加入房间的回调
        void OnJoinRoom(JoinRoomResult result);

        // 出现错误的回调
        void OnError(CallbackResult result);

        // 其他人加入房间的回调
        void OnUserJoined(UserJoinedResult result);

        // 其他用户离开房间回调
        void OnUserOffline(UserOfflineResult result);

        // 房间内其他用户关闭/开启发送音频回调
        void OnUserMuteAudio(AudioMutedResult result);

        // 房间内其他用户关闭/开启接受音频回调
        void OnUserMuteRemoteAudio(AudioMutedResult result);


        // 房间内谁正在说话以及说话者音量的回调
        void OnAudioVolumeIndication(VolumeIndicationResult result);

        // 语音连接中断丢失的回调
        void OnConnectionLost(CallbackResult result);

        // 本人离开房间的回调
        void OnLeaveRoom(CallbackResult result);

    }
}

