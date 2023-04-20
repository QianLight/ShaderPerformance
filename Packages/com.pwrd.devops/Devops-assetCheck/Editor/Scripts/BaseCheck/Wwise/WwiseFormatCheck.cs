using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AssetCheck
{
    [CheckRuleDescription("Wwise", "压缩格式检查", "custom:AssetCheck.AssetHelper.FindAllFiles,*.wem", "")]
    public class WwiseFormatCheck : RuleBase
    {
        const string formatTypes = "opus, wem opus, vorbis, pcm, adpcm, unknown";
        [PublicParam("压缩格式", eGUIType.StringMaskField, formatTypes)]
        public int eWwiseFormatMask = 0;

        [PublicMethod]
        public bool Check(string path, out string output)
        {
            var wemFile = new WEMFile(path);
            string currentFormat = wemFile.AudioFormat;
            output = currentFormat.ToString();
            string[] formats = AssetHelper.StringToMultiple(formatTypes);
            string[] selectedFormats = AssetHelper.SelectMultipleStrings(formats, eWwiseFormatMask);
            foreach (var format in selectedFormats)
            {
                if (format.Equals(currentFormat))
                    return true;
            }
            return false;
        }
    }
}