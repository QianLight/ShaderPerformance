using System;
using UNBridgeLib;
using UNBridgeLib.LitJson;
using GMSDK;
using UnityEngine;

namespace GMSDK
{
    public class RTCCallbackHandler : BridgeCallBack
    {
        // 实时语音的回调
        public IRtcVoiceCallback rtcVoiceCallback;

        public RTCCallbackHandler()
        {
            OnFailed = OnFailCallback;
            OnTimeout = OnTimeoutCallback;
        }

        /// <summary>
        /// 加入房间回调
        /// </summary>
        /// <param name="data"></param>
        public void OnJoinRoomCallback(JsonData jd)
        {
            LogUtils.D("RTC - OnJoinRoomCallback");
            JoinRoomResult result = SdkUtil.ToObject<JoinRoomResult>(jd.ToJson());
            if (rtcVoiceCallback != null)
            {
                SdkUtil.InvokeAction<JoinRoomResult>(rtcVoiceCallback.OnJoinRoom, result);
            }
        }

        /// <summary>
        /// 实时语音发生错误回调
        /// </summary>
        /// <param name="data"></param>
        public void OnErrorCallback(JsonData jd)
        {
            LogUtils.D("RTC - OnErrorCallback");
            CallbackResult result = SdkUtil.ToObject<CallbackResult>(jd.ToJson());
            if (rtcVoiceCallback != null)
            {
                SdkUtil.InvokeAction<CallbackResult>(rtcVoiceCallback.OnError, result);
            }
        }

        /// <summary>
        /// 其他用户加入房间语音回调
        /// </summary>
        /// <param name="data"></param>
        public void OnUserJoinedCallback(JsonData jd)
        {
            LogUtils.D("RTC - OnUserJoinedCallback");
            UserJoinedResult result = SdkUtil.ToObject<UserJoinedResult>(jd.ToJson());
            if (rtcVoiceCallback != null)
            {
                SdkUtil.InvokeAction<UserJoinedResult>(rtcVoiceCallback.OnUserJoined, result);
            }
        }

        /// <summary>
        /// 其他用户离开房间回调
        /// </summary>
        /// <param name="data">0: 用户主动离开; 1：因过长时间收不到对方数据包，超时掉线</param>
        public void OnUserOfflineCallback(JsonData jd)
        {
            LogUtils.D("RTC - OnUserOfflineCallback");
            UserOfflineResult result = SdkUtil.ToObject<UserOfflineResult>(jd.ToJson());
            if (rtcVoiceCallback != null)
            {
                SdkUtil.InvokeAction<UserOfflineResult>(rtcVoiceCallback.OnUserOffline, result);
            }
        }

        /// <summary>
        /// 房间内其他用户关闭/开启发送音频回调
        /// </summary>
        /// <param name="data"></param>
        public void OnUserMuteAudioCallback(JsonData jd)
        {
            LogUtils.D("RTC - OnUserMuteAudioCallback");
            AudioMutedResult result = SdkUtil.ToObject<AudioMutedResult>(jd.ToJson());
            if (rtcVoiceCallback != null)
            {
                SdkUtil.InvokeAction<AudioMutedResult>(rtcVoiceCallback.OnUserMuteAudio, result);
            }
        }

        /// <summary>
        /// 房间内其他用户关闭/开启接受音频回调
        /// </summary>
        /// <param name="data"></param>
        public void OnUserMuteRemoteAudioCallback(JsonData jd)
        {
            LogUtils.D("RTC - OnUserMuteRemoteAudioCallback");
            AudioMutedResult result = SdkUtil.ToObject<AudioMutedResult>(jd.ToJson());
            if (rtcVoiceCallback != null)
            {
                SdkUtil.InvokeAction<AudioMutedResult>(rtcVoiceCallback.OnUserMuteRemoteAudio, result);
            }
        }

        /// <summary>
        /// 提示频道内谁正在说话以及说话者音量的回调
        /// </summary>
        /// <param name="data"></param>
        public void OnAudioVolumeIndicationCallback(JsonData jd)
        {
            LogUtils.D("RTC - OnAudioVolumeIndicationCallback");
            VolumeIndicationResult result = SdkUtil.ToObject<VolumeIndicationResult>(jd.ToJson());
            if (rtcVoiceCallback != null)
            {
                SdkUtil.InvokeAction<VolumeIndicationResult>(rtcVoiceCallback.OnAudioVolumeIndication, result);
            }
        }

        /// <summary>
        /// 语音连接中断的回调
        /// </summary>
        /// <param name="data"></param>
        public void OnConnectionLostCallback(JsonData jd)
        {
            LogUtils.D("RTC - OnConnectionLostCallback");
            CallbackResult result = SdkUtil.ToObject<CallbackResult>(jd.ToJson());
            if (rtcVoiceCallback != null)
            {
                SdkUtil.InvokeAction<CallbackResult>(rtcVoiceCallback.OnConnectionLost, result);
            }
        }

        /// <summary>
        /// 用户自己离开房间回调
        /// </summary>
        /// <param name="data"></param>
        public void OnLeaveRoomCallback(JsonData jd)
        {
            LogUtils.D("RTC - OnLeaveRoomCallback");
            CallbackResult result = SdkUtil.ToObject<CallbackResult>(jd.ToJson());
            if (rtcVoiceCallback != null)
            {
                SdkUtil.InvokeAction<CallbackResult>(rtcVoiceCallback.OnLeaveRoom, result);
            }
        }

        /// <summary>
        /// 失败统一回调，用于调试接口
        /// </summary>
        public void OnFailCallback(int code, string failMsg)
        {
            LogUtils.E("RTC - OnFailCallback");
            LogUtils.E("接口访问失败 " + code.ToString() + " " + failMsg);
        }
        /// <summary>
        /// 超时统一回调
        /// </summary>
        public void OnTimeoutCallback()
        {
            JsonData jd = new JsonData();
            jd["code"] = -321;
            jd["message"] = "RTC - request time out";
            if (this.OnSuccess != null)
            {
                this.OnSuccess(jd);
            }
        }
    }
}