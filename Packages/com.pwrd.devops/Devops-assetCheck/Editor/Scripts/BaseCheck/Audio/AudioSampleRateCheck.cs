using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
namespace AssetCheck
{
    [CheckRuleDescription("Audio", "没有使用OptimizeSampleRate的PCM音频", "t:AudioClip", "")]
    public class AudioSampleRateCheck : RuleBase
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
            if (audioImporter.defaultSampleSettings.compressionFormat != AudioCompressionFormat.PCM)
                return true;
            AudioSampleRateSetting currentAudioSampleRateSetting = audioImporter.defaultSampleSettings.sampleRateSetting;
            output = currentAudioSampleRateSetting.ToString();
            return currentAudioSampleRateSetting == AudioSampleRateSetting.OptimizeSampleRate;
        }
    }
}

