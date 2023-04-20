using UNBridgeLib;
using UNBridgeLib.LitJson;

namespace GMSDK
{
    public class BaseRTCSDK
    {
        public BaseRTCSDK()
        {
#if UNITY_ANDROID
            UNBridge.Call(RTCMethodName.Init, null);
#endif
        }

        /// <summary>
        /// 初始化实时语音
        /// </summary>
		/// <param name="clientRole">1: 表示设置用户角色为主播；2: 表示设置用户角色为观众(局部语音模式下不可调用)</param>
        public void InitRtcVoice(int clientRole = 1)
        {
            LogUtils.D("InitRtcVoice1");
            JsonData param = new JsonData();
            param["clientRole"] = clientRole;
            UNBridge.Call(RTCMethodName.InitRtc, param);
        }

        /// <summary>
        /// 实时语音加入房间
        /// </summary>
        /// <param name="roomId">房间Id.</param>
        /// <param name="uid">房间内使用的用户id.</param>
        /// <param name="token">实时语音token信息.</param>
        /// <param name="callback">回调.</param>
        public void RtcVoiceJoinRoom(string roomId, string uid, string token, IRtcVoiceCallback callback)
        {
            LogUtils.D("RtcVoiceJoinRoom");
            JsonData param = new JsonData();
            param["roomId"] = roomId;
            param["uid"] = uid;
            param["token"] = token;
            RtcListenJoinRoomEvent(callback);
            RtcListenErrorEvent(callback);
            RtcListenUserJoinedEvent(callback);
            RtcListenUserOfflineEvent(callback);
            RtcListenUserMuteEvent(callback);
            RtcListenUserMuteRemoteEvent(callback);
            RtcListenVolumeIndicationOfSpeakersEvent(callback);
            RtcListenConnectionLostEvent(callback);
            RtcListenLeaveRoomEvent(callback);

            UNBridge.Call(RTCMethodName.RtcJoinRoom, param);
        }

        // 监听加入房间事件
        private void RtcListenJoinRoomEvent(IRtcVoiceCallback callback)
        {
            RTCCallbackHandler unCallBack = new RTCCallbackHandler();
            unCallBack.rtcVoiceCallback = callback;
            unCallBack.OnSuccess = new OnSuccessDelegate(unCallBack.OnJoinRoomCallback);
            UNBridge.Listen(RTCResultName.RtcJoinRoomResult, unCallBack);
        }

        // 监听发生错误事件
        private void RtcListenErrorEvent(IRtcVoiceCallback callback)
        {
            RTCCallbackHandler unCallBack = new RTCCallbackHandler();
            unCallBack.rtcVoiceCallback = callback;
            unCallBack.OnSuccess = new OnSuccessDelegate(unCallBack.OnErrorCallback);
            UNBridge.Listen(RTCResultName.RtcErrorResult, unCallBack);
        }

        // 监听其他用户加入房间事件
        private void RtcListenUserJoinedEvent(IRtcVoiceCallback callback)
        {
            RTCCallbackHandler unCallBack = new RTCCallbackHandler();
            unCallBack.rtcVoiceCallback = callback;
            unCallBack.OnSuccess = new OnSuccessDelegate(unCallBack.OnUserJoinedCallback);
            UNBridge.Listen(RTCResultName.RtcUserJoinedResult, unCallBack);
        }

        // 监听其他用户离开房间事件
        private void RtcListenUserOfflineEvent(IRtcVoiceCallback callback)
        {
            RTCCallbackHandler unCallBack = new RTCCallbackHandler();
            unCallBack.rtcVoiceCallback = callback;
            unCallBack.OnSuccess = new OnSuccessDelegate(unCallBack.OnUserOfflineCallback);
            UNBridge.Listen(RTCResultName.RtcUserOfflineResult, unCallBack);
        }

        // 监听房间内其他用户关闭/开启发送音频回调事件
        private void RtcListenUserMuteEvent(IRtcVoiceCallback callback)
        {
            RTCCallbackHandler unCallBack = new RTCCallbackHandler();
            unCallBack.rtcVoiceCallback = callback;
            unCallBack.OnSuccess = new OnSuccessDelegate(unCallBack.OnUserMuteAudioCallback);
            UNBridge.Listen(RTCResultName.RtcUserMuteResult, unCallBack);
        }

        // 监听房间内其他用户关闭/开启收听音频回调事件
        private void RtcListenUserMuteRemoteEvent(IRtcVoiceCallback callback)
        {
            RTCCallbackHandler unCallBack = new RTCCallbackHandler();
            unCallBack.rtcVoiceCallback = callback;
            unCallBack.OnSuccess = new OnSuccessDelegate(unCallBack.OnUserMuteRemoteAudioCallback);
            UNBridge.Listen(RTCResultName.RtcUserMuteRemoteResult, unCallBack);
        }


        // 监听频道内谁正在说话以及说话者音量的回调事件
        private void RtcListenVolumeIndicationOfSpeakersEvent(IRtcVoiceCallback callback)
        {
            RTCCallbackHandler unCallBack = new RTCCallbackHandler();
            unCallBack.rtcVoiceCallback = callback;
            unCallBack.OnSuccess = new OnSuccessDelegate(unCallBack.OnAudioVolumeIndicationCallback);
            UNBridge.Listen(RTCResultName.RtcVolumeIndicationResult, unCallBack);
        }
        private void RtcListenConnectionLostEvent(IRtcVoiceCallback callback)
        {
            RTCCallbackHandler unCallBack = new RTCCallbackHandler();
            unCallBack.rtcVoiceCallback = callback;
            unCallBack.OnSuccess = new OnSuccessDelegate(unCallBack.OnConnectionLostCallback);
            UNBridge.Listen(RTCResultName.RtcConnectionLostResult, unCallBack);
        }
        private void RtcListenLeaveRoomEvent(IRtcVoiceCallback callback)
        {
            RTCCallbackHandler unCallBack = new RTCCallbackHandler();
            unCallBack.rtcVoiceCallback = callback;
            unCallBack.OnSuccess = new OnSuccessDelegate(unCallBack.OnLeaveRoomCallback);
            UNBridge.Listen(RTCResultName.RtcLeaveRoomResult, unCallBack);
        }

        /// <summary>
        /// 关闭打开音频采集，目前仅P5需要使用（不等于静音，需要重启音频，会消耗时间，一般业务使用muteLocalAudioStream本地静音）
        /// </summary>
        /// <param name="mute">true: 打开麦克风采集，false: 关闭麦克风采集</param>
        public int RtcVoiceEnableLocalAudio(bool enabled)
        {
            LogUtils.D("RtcVoiceEnableLocalAudio");
            JsonData param = new JsonData();
            param["enabled"] = enabled;
            object res = UNBridge.CallSync(RTCMethodName.RtcEnableLocalAudio, param);
            if (res != null)
            {
                return (int)res;
            }
            return -1;
        }

        /// <summary>
        /// 加入房间前调用，选择默认情况下关闭/打开音频采集，目前仅P5需要使用（不等于静音，需要重启音频，会消耗时间，一般业务使用muteLocalAudioStream本地静音）
        /// </summary>
        /// <param name="mute">true: 默认打开麦克风采集，false: 默认关闭麦克风采集</param>
        public int RtcVoiceSetDefaultEnableLocalAudio(bool enabled)
        {
            LogUtils.D("RtcVoiceEnableLocalAudio");
            JsonData param = new JsonData();
            param["enabled"] = enabled;
            object res = UNBridge.CallSync(RTCMethodName.RtcSetDefaultLocalAudioEnable, param);
            if (res != null)
            {
                return (int)res;
            }
            return -1;
        }

        /// <summary>
		/// 局部语音模式下不可调用
        /// 本地麦克风静音配置
        /// </summary>
        /// <param name="mute">true: 静音，false: 取消静音</param>
        public int RtcVoiceMuteLocalAudioStream(bool mute)
        {
            LogUtils.D("RtcVoiceMuteLocalAudioStream");
            JsonData param = new JsonData();
            param["mute"] = mute;
            object res = UNBridge.CallSync(RTCMethodName.RtcMuteLocalAudioStream, param);
            if (res != null)
            {
                return (int)res;
            }
            return -1;
        }

        /// <summary>
        /// 对指定用户静音配置
        /// </summary>
        /// <param name="uid">指定用户id</param>
        /// <param name="mute">true: 静音，false: 取消静音</param>
        public int RtcVoiceMuteRemoteAudioStream(string uid, bool mute)
        {
            LogUtils.D("RtcVoiceMuteRemoteAudioStream");
            JsonData param = new JsonData();
            param["uid"] = uid;
            param["mute"] = mute;
            object res = UNBridge.CallSync(RTCMethodName.RtcMuteRemoteAudioStream, param);
            if (res != null)
            {
                return (int)res;
            }
            return -1;
        }

        /// <summary>
		/// 局部语音模式下不可调用
        /// 对所有用户静音配置
        /// </summary>
        /// <param name="mute">true: 静音，false: 取消静音</param>
        public int RtcVoiceMuteAllRemoteAudioStreams(bool mute)
        {
            LogUtils.D("RtcVoiceMuteAllRemoteAudioStreams");
            JsonData param = new JsonData();
            param["mute"] = mute;
            object res = UNBridge.CallSync(RTCMethodName.RtcMuteAllRemoteAudioStreams, param);
            if (res != null)
            {
                return (int)res;
            }
            return -1;
        }

        /// <summary>
        /// 更新Token（返回token失效错误码后调用）
        /// </summary>
        /// <param name="token">实时语音token信息.</param>
        public int RtcVoiceUpdateToken(string token)
        {
            LogUtils.D("RtcVoiceUpdateToken");
            JsonData param = new JsonData();
            param["token"] = token;
            object res = UNBridge.CallSync(RTCMethodName.RtcUpdateToken, param);
            if (res != null)
            {
                return (int)res;
            }
            return -1;
        }

        /// <summary>
        /// 实时语音离开房间
        /// </summary>
        public int RtcVoiceLeaveRoom()
        {
            LogUtils.D("RtcVoiceLeaveRoom");
            object res = UNBridge.CallSync(RTCMethodName.RtcLeaveRoom, null);
            if (res != null)
            {
                return (int)res;
            }
            return -1;
        }

        /// <summary>
        /// 对指定用户音量进行配置
        /// </summary>
        /// <param name="volume">音量大小</param>
        /// <param name="uid">指定用户id</param>
        public int RTCVoiceAdjustRemoteAudioVolume(int volume, string uid)
        {
            LogUtils.D("RTCVoiceAdjustRemoteAudioVolume");
            JsonData param = new JsonData();
            param["volume"] = volume;
            param["uid"] = uid;
            object res = UNBridge.CallSync(RTCMethodName.RTCAdjustRemoteAudioVolume, param);
            if (res != null)
            {
                return (int)res;
            }
            return -1;
        }
        /// 设置是否开启区域语音
        public int RtcEnableRangeAudio(bool enable)
        {
            LogUtils.D("RtcEnableRangeAudio");
            JsonData param = new JsonData();
            param["enable"] = enable;
            object res = UNBridge.CallSync(RTCMethodName.RTCEnableRangeAudio, param);
            if (res != null)
            {
                return (int)res;
            }
            return -1;
        }

        /// <summary>
		/// 局部语音模式下不可调用
        /// 设置用户角色，建议在加入房间后进行设置
        /// </summary>
        /// <param name="role">1: 表示设置用户角色为主播；2: 表示设置用户角色为观众</param>
		public int RTCVoiceSetClientRole(int role)
        {
            LogUtils.D("RTCVoiceSetClientRole");
            JsonData param = new JsonData();
            param["clientRole"] = role;
            object res = UNBridge.CallSync(RTCMethodName.RtcSetClientRole, param);
            if (res != null)
            {
                return (int)res;
            }
            return -1;
        }

        /// <summary>
        /// 调节麦克风音量
        /// </summary>
        /// <param name="volume">录音音量可在0~200范围内进行调节，每次退出房间后会重置
        ///  - 0   : 静音
        ///  - 100 : 原始音量
        ///  - 200 : 最大可为原始音量的 2 倍(自带溢出保护)</param>
        public int RTCAdjustRecordingSignalVolume(int volume)
        {
            LogUtils.D("RTCAdjustRecordingSignalVolume");
            JsonData param = new JsonData();
            param["volume"] = volume;
            object res = UNBridge.CallSync(RTCMethodName.RTCAdjustRecordingSignalVolume, param);
            if (res != null)
            {
                return (int)res;
            }
            return -1;
        }

        /// <summary>
        /// 调节收听音量
        /// </summary>
        /// <param name="volume">收听音量可在0~200范围内进行调节，每次退出房间后会重置
        ///  - 0   : 静音
        ///  - 100 : 原始音量
        ///  - 200 : 最大可为原始音量的 2 倍(自带溢出保护)</param>
        public int RTCAdjustPlaybackSignalVolume(int volume)
        {
            LogUtils.D("RTCAdjustPlaybackSignalVolume");
            JsonData param = new JsonData();
            param["volume"] = volume;
            object res = UNBridge.CallSync(RTCMethodName.RTCAdjustPlaybackSignalVolume, param);
            if (res != null)
            {
                return (int)res;
            }
            return -1;
        }

        /// <summary>
        /// 配置 TeamId，必须在加入房间前调用，不支持设置后修改。
        /// 如果设置TeamId，则是局部语音模式，如果不设置TeamId，则默认是普通语音模式
        /// </summary>
        /// <param name="teamId">小队ID</param>
        public int RTCConfigTeamId(string teamId)
        {
            LogUtils.D("RTCConfigTeamId");
            JsonData param = new JsonData();
            param["teamId"] = teamId;
            object res = UNBridge.CallSync(RTCMethodName.RTCConfigTeamId, param);
            if (res != null)
            {
                return (int)res;
            }
            return -1;
        }

        /// <summary>
        /// （仅在设置了teamId的局部语音模式下生效）
        /// 向小队或世界发音频，可在加入房间前调用，也可在加入房间后调用
        /// </summary>
        /// <param name="mode">模式选择，向小队发送或者向世界发送</param>
        public int RTCSetAudioSendMode(GMRTCRangeAudioMode mode)
        {
            LogUtils.D("RTCSetAudioSendMode");
            JsonData param = new JsonData();
            param["mode"] = (int)mode;
            object res = UNBridge.CallSync(RTCMethodName.RTCSetAudioSendMode, param);
            if (res != null)
            {
                return (int)res;
            }
            return -1;
        }

        /// <summary>
        /// （仅在设置了teamId的局部语音模式下生效）
        /// 收听小队或世界音频，可在加入房间前调用，也可在加入房间后调用
        /// </summary>
        /// <param name="mode">模式选择，向小队发送或者向世界发送</param>
        public int RTCSetAudioRecvMode(GMRTCRangeAudioMode mode)
        {
            LogUtils.D("RTCSetAudioRecvMode");
            JsonData param = new JsonData();
            param["mode"] = (int)mode;
            object res = UNBridge.CallSync(RTCMethodName.RTCSetAudioRecvMode, param);
            if (res != null)
            {
                return (int)res;
            }
            return -1;
        }

        /// <summary>
        /// 设置用户机型等级
        /// </summary>
        public int RTCSetAudioPerfProfile(GMRTCAudioPerfProfile profile)
        {
            LogUtils.D("RTCSetAudioSendMode");
            JsonData param = new JsonData();
            param["profile"] = (int)profile;
            object res = UNBridge.CallSync(RTCMethodName.RTCSetAudioPerfProfile, param);
            if (res != null)
            {
                return (int)res;
            }
            return -1;
        }

        /// <summary>
        /// （仅在设置了teamId的局部语音模式下生效）
        ///  设置自己可听见的范围，minRange表示最小衰减距离，距离为
        /// (0, minRange)时，音量不衰减。
        /// (minRange, maxRange)时，按距离衰减
        /// (maxRange, ∞)之间，听不到声音
        /// 衰减函数：
        /// if ( x < minRange) g(x) = 1.0
        /// else if ( x > maxRange) g(x) = 0.0
        /// else  f(x) =  minRange / x
        /// </summary>
        /// <param name="minRange">收听范围</param>
        /// <param name="maxRange">收听范围</param>
        public int RTCUpdateAudioRecvRange(int minRange, int maxRange)
        {
            LogUtils.D("RTCUpdateAudioRecvRange");
            JsonData param = new JsonData();
            param["minRange"] = minRange;
            param["maxRange"] = maxRange;
            object res = UNBridge.CallSync(RTCMethodName.RTCUpdateAudioRecvRange, param);
            if (res != null)
            {
                return (int)res;
            }
            return -1;
        }

        /// <summary>
        /// （仅在设置了teamId的局部语音模式下生效）
        ///  更新自己的位置
        /// </summary>
        /// <param name="x">x坐标</param>
        /// <param name="y">y坐标</param>
        /// <param name="z">z坐标</param>
        public int RTCUpdateSelfPosition(int x, int y, int z)
        {
            LogUtils.D("RTCUpdateSelfPosition");
            JsonData param = new JsonData();
            param["x"] = x;
            param["y"] = y;
            param["z"] = z;
            object res = UNBridge.CallSync(RTCMethodName.RTCUpdateSelfPosition, param);
            if (res != null)
            {
                return (int)res;
            }
            return -1;
        }
    }
}