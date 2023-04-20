using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;


namespace AssetCheck
{
    [CheckRuleDescription("Audio", "双声道的音频", "t:AudioClip", "")]
    public class AudioDoubleCheck : RuleBase
    {
        [PublicMethod]
        public bool Check(string path, out string output)
        {
            output = string.Empty;
            AudioClip audioClip = AssetDatabase.LoadAssetAtPath<AudioClip>(path);
            if (audioClip == null)
                return true;
            if (audioClip.channels == 2)
            {
                output = "双声道音频";
                return false;
            }
            else
                return true;
        }
    }
}