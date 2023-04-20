using System;
using System.Collections.Generic;
using UNBridgeLib.LitJson;

namespace GSDK
{
    public static class VoiceLog
    {
        public static void LogDebug(VoiceTag tag, object message)
        {
            GLog.LogDebug(GetVoiceTag(tag) + message);
        }

        public static void LogInfo(VoiceTag tag, object message)
        {
            GLog.LogInfo(GetVoiceTag(tag) + message);
        }

        public static void LogInfo(VoiceTag tag, string message, Result result)
        {
            GLog.LogInfo(GetVoiceTag(tag) + message + " ,Result====" + result);
        }
        
        public static void LogInfo(VoiceTag tag, string message, Object data)
        {
            GLog.LogInfo(GetVoiceTag(tag) + message + " ,data:" + JsonMapper.ToJson(data));
        }
        
        public static void LogInfo(VoiceTag tag, string message, Object data, Result result)
        {
            GLog.LogInfo(GetVoiceTag(tag) + message + " ,data:" + JsonMapper.ToJson(data) + " ,Result====" + result);
        }
        
        public static void LogWarning(VoiceTag tag, object message)
        {
            GLog.LogWarning(GetVoiceTag(tag) + message);
        }

        public static void LogError(VoiceTag tag, object message)
        {
            GLog.LogError(GetVoiceTag(tag) + message);
        }

        public static void LogException(Exception e)
        {
            GLog.LogException(e);
        }

        private static string GetVoiceTag(VoiceTag tag)
        {
            return "{" + tag + "}";
        }
    }

    public enum VoiceTag
    {
        IMVoice = 1,
        ASRVoice = 2,
    }
}