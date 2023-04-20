using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;


namespace AssetCheck
{
    [CheckRuleDescription("Audio", "检查音频文件时长限制", "t:AudioClip", "")]
    public class AudioLengthCheck : RuleBase
    {
        [PublicParam("音频时长（秒）", eGUIType.Input)]
        public float audioLength = 5.0f;
        [PublicMethod]
        public bool Check(string path, out string output)
        {
            output = string.Empty;
            AudioClip audioClip = AssetDatabase.LoadAssetAtPath<AudioClip>(path);
            if (audioClip == null)
                return true;
            output = audioClip.length.ToString();
            return audioClip.length < audioLength;
        }
    }
}

