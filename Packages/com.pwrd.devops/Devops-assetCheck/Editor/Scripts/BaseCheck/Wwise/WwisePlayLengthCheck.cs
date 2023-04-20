using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AssetCheck
{
    [CheckRuleDescription("Wwise", "检查播放时长", "custom:AssetCheck.AssetHelper.FindAllFiles,*.wem", "")]
    public class WwisePlayLengthCheck : RuleBase
    {
        [PublicParam("播放时长", eGUIType.Input)]
        public float playLength = 2.5f;
        [PublicMethod]
        public bool Check(string path, out string output)
        {
            var wemFile = new WEMFile(path);
            long currentPlayLength = wemFile.MediaLength();
            output = currentPlayLength.ToString();
            return currentPlayLength < playLength;
        }
    }

}