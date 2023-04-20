using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace AssetCheck
{
    [CheckRuleDescription("Audio", "未使用PCM格式的音频", "t:AudioClip", "")]
    public class AudioPCMCheck : RuleBase
    {
        [PublicMethod]
        public bool Check(string path, out string output)
        {
            output = string.Empty;
            AudioClip audioClip = AssetDatabase.LoadAssetAtPath<AudioClip>(path);
            if (audioClip == null)
                return true;
            AudioImporter audioImporter = AssetImporter.GetAtPath(path) as AudioImporter;
            if (audioImporter == null)
                return true;
            AudioCompressionFormat currentCompressionFormat = audioImporter.defaultSampleSettings.compressionFormat;
            output = currentCompressionFormat.ToString();
            return currentCompressionFormat == AudioCompressionFormat.PCM;
        }
    }
}

