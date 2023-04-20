using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
namespace AssetCheck
{
    [CheckRuleDescription("Audio", "该音频中使用了Quality过高的Vorbis与MP3压缩", "t:AudioClip", "")]
    public class AudioQualityCheck : RuleBase
    {
        [PublicParam("压缩质量", eGUIType.Input)]
        public float quality = 0.5f;
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
            if (audioImporter.defaultSampleSettings.compressionFormat != AudioCompressionFormat.Vorbis)
                return true;
            float currentQuality = audioImporter.defaultSampleSettings.quality;
            output = currentQuality.ToString();
            return currentQuality < quality;
        }
    }
}
