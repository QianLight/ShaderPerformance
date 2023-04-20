using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
namespace Ender
{
    public class EnderSettingsModel
    {
        public string ip = "";
        public bool verified = false;
        public bool isiOSPlatform = false;
        //Anywhere add
        public EnderRemoteConstants.EnderType enderType = EnderRemoteConstants.EnderType.EnderLocal;
        public bool remoteVerified = false;
        public int currentDeployStatus = EnderRemoteConstants.DeployStatus.Init;
        public string appId = "";
        public string sdkVersion = "";
        public EnderRemoteConstants.EnderPlatform platform = EnderRemoteConstants.EnderPlatform.Android;
        public string channelId = "";
        public string uid = "";
        public string deviceSerial = "";
        public string packageUrl = "";
        public string packageBuildTime = "";
        public long deviceDeadLine;
        public double deployProgress;
        public int androidOsVersion;
        public string dynamicCode = "";

        public void ClearRemoteFlag()
        {
            uid = "";
            channelId = "";
            deviceSerial = "";
            currentDeployStatus = EnderRemoteConstants.DeployStatus.Init;
            remoteVerified = false;
            packageUrl = "";
            packageBuildTime = "";
            deviceDeadLine = 0;
            deployProgress = 0.0;
            androidOsVersion = 0;
            dynamicCode = "";
        }
    }
}
#endif