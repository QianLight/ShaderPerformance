using System;
using System.Collections.Generic;
using GMSDK;
using UNBridgeLib.LitJson;

namespace GSDK
{
    internal static class VoiceInnerTools
    {
        private static readonly Dictionary<ClientRole, int> _clientRoleDictionary = new Dictionary<ClientRole, int>()
        {
            {ClientRole.Broadcaster, 1},
            {ClientRole.Audience, 2}
        };

        private static readonly Dictionary<RTCRangeAudioMode, GMRTCRangeAudioMode> _rangeAudioModeDictionary =
            new Dictionary<RTCRangeAudioMode, GMRTCRangeAudioMode>()
            {
                {RTCRangeAudioMode.Mute, GMRTCRangeAudioMode.GMRTCRangeAudioModeMute},
                {RTCRangeAudioMode.Team, GMRTCRangeAudioMode.GMRTCRangeAudioModeTeam},
                {RTCRangeAudioMode.World, GMRTCRangeAudioMode.GMRTCRangeAudioModeWorld}
            };
        
        private static readonly Dictionary<PerformanceProfileLevel,GMRTCAudioPerfProfile> _perfProfileDictionary =
            new Dictionary<PerformanceProfileLevel, GMRTCAudioPerfProfile>()
            {
                {PerformanceProfileLevel.Auto,GMRTCAudioPerfProfile.GMRTCAudioPerfProfileAUTO},
                {PerformanceProfileLevel.Low,GMRTCAudioPerfProfile.GMRTCAudioPerfProfileLOW},
                {PerformanceProfileLevel.Medium,GMRTCAudioPerfProfile.GMRTCAudioPerfProfileMID},
                {PerformanceProfileLevel.High,GMRTCAudioPerfProfile.GMRTCAudioPerfProfileHIGH},
            };
        
        public static JoinRoomInfo Convert(JoinRoomResult result)
        {
            return new JoinRoomInfo()
            {
                RoomID = result.roomId,
                UserID = result.userId,
                Elapsed = result.elapsed
            };
        }

        public static OtherUserJoinInfo Convert(UserJoinedResult result)
        {
            return new OtherUserJoinInfo()
            {
                UserID = result.userId,
                Elapsed = result.elapsed
            };
        }

        public static OtherUserLeaveInfo Convert(UserOfflineResult result)
        {
            return new OtherUserLeaveInfo()
            {
                Reason = (result.reason == 0 ? LeaveRoomReason.Initiative : LeaveRoomReason.NetworkTimeout),
                UserID = result.userId
            };
        }

        public static OtherUserMuteAudioInfo Convert(AudioMutedResult result)
        {
            return new OtherUserMuteAudioInfo()
            {
                UserID = result.userId,
                IsMuted = (result.isMuted == 1)
            };
        }

        public static AudioVolumeIndicationInfo Convert(VolumeIndicationResult result)
        {
            return new AudioVolumeIndicationInfo()
            {
                Speakers = Convert(result.speakers),
                TotalVolume = result.totalVolume
            };
        }

        private static List<SpeakerInfo> Convert(List<GMRTCAudioVolumeInfo> targetInfos)
        {
            List<SpeakerInfo> resultInfos = new List<SpeakerInfo>();
            if (targetInfos != null)
            {
                foreach (var info in targetInfos)
                {
                    resultInfos.Add(new SpeakerInfo()
                    {
                        UserID = info.uid,
                        Volume = info.volume
                    });
                }
            }
            return resultInfos;
        }

        public static int Convert(ClientRole clientRole)
        {
            return _clientRoleDictionary[clientRole];
        }

        public static GMRTCRangeAudioMode Convert(RTCRangeAudioMode mode)
        {
            return _rangeAudioModeDictionary[mode];
        }

        public static GMRTCAudioPerfProfile Convert(PerformanceProfileLevel level)
        {
            return _perfProfileDictionary[level];
        }
        
        public static Result ConvertRTCError(CallbackResult result)
        {
            if (result.code == 0)
            {
                return new Result(GSDK.ErrorCode.Success, 0, 0, result.message);
            }
            return new Result(result.code, result.message, result.extraErrorCode, result.extraErrorMessage, result.additionalInfo);
        }
    }
    
    
    public static class RTCLog
    {
        private static readonly string TAG = "{RTC}";

        public static void LogDebug(object message)
        {
            GLog.LogDebug(TAG + message);
        }

        public static void LogInfo(object message)
        {
            GLog.LogInfo(TAG + message);
        }

        public static void LogInfo(string message, Result result)
        {
            GLog.LogInfo(TAG + message + " ,Result====" + result);
        }
        
        public static void LogInfo(string message, Object data)
        {
            GLog.LogInfo(TAG + message + " ,data:" + JsonMapper.ToJson(data));
        }
        
        public static void LogInfo(string message, Object data, Result result)
        {
            GLog.LogInfo(TAG + message + " ,data:" + JsonMapper.ToJson(data) + " ,Result====" + result);
        }
        
        public static void LogWarning(object message)
        {
            GLog.LogWarning(TAG + message);
        }

        public static void LogError(object message)
        {
            GLog.LogError(TAG + message);
        }

        public static void LogException(Exception e)
        {
            GLog.LogException(e);
        }
    }
}