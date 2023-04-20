using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AssetCheck
{
    [CheckRuleDescription("Wwise", "检查码率", "custom:AssetCheck.AssetHelper.FindAllFiles,*.wem", "")]
    public class WwiseAverageBytesPerSecondCheck : RuleBase
    {
        [PublicParam("码率", eGUIType.Input)]
        public uint AverageBytesPerSecond = 10000;
        long currentAverageBytesPerSecond = 0;
        [PublicMethod]
        public bool Check(string path, out string output)
        {
            var wemFile = new WEMFile(path);
            currentAverageBytesPerSecond = wemFile.AverageBytesPerSecond;
            output = currentAverageBytesPerSecond.ToString();
            return currentAverageBytesPerSecond < AverageBytesPerSecond;
        }
    }
}