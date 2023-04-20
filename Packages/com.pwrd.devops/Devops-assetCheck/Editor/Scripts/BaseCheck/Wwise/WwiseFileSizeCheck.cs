using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AssetCheck
{
    [CheckRuleDescription("Wwise", "检查文件大小", "custom:AssetCheck.AssetHelper.FindAllFiles,*.wem", "")]
    public class WwiseFileSizeCheck : RuleBase
    {
        [PublicParam("磁盘占用(k)", eGUIType.Input)]
        public long diskSize = 1048576;
        [PublicMethod]
        public bool Check(string path, out string output)
        {
            var wemFile = new WEMFile(path);
            long currentDiskSize = wemFile.MediaFileSize();
            output = currentDiskSize.ToString();
            return currentDiskSize < diskSize;
        }
    }

}