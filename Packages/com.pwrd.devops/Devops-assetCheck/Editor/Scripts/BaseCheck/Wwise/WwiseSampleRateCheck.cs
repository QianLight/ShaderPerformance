using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace AssetCheck
{
    [CheckRuleDescription("Wwise", "检查采样率", "custom:AssetCheck.AssetHelper.FindAllFiles,*.wem", "")]
    public class WwiseSampleRateCheck : RuleBase
    {
        [PublicParam("采样率", eGUIType.Input)]
        public uint SampleRate = 100;
        [PublicMethod]
        public bool Check(string path, out string output)
        {
            var wemFile = new WEMFile(path);
            long currentSampleRate = wemFile.SampleRate;
            output = currentSampleRate.ToString();
            return currentSampleRate < SampleRate;
        }
    }
}