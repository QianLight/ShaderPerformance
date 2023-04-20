using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace AssetCheck
{
    [CheckRuleDescription("Audio", "未使用Streaming加载的长音频", "t:AudioClip", "如果音频超过了设置时长，并且没有使用streaming加载的，会被筛出")]
    public class AudioStreamingCheck : RuleBase
    {
        [PublicParam("音频时长（秒）", eGUIType.Input)]
        public float AudioLength = 3.0f;
        [PublicMethod]
        public bool Check(string path, out string output)
        {
            output = string.Empty;
            AudioClip audioClip = AssetDatabase.LoadAssetAtPath<AudioClip>(path);
            if (audioClip == null)
                return true;
            if (audioClip.length < AudioLength)
                return true;
            output = audioClip.loadType.ToString();
            return audioClip.loadType == AudioClipLoadType.Streaming;
        }
    }
}