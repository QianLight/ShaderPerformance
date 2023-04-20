using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Devops.Core
{
    public class DevopsCoreInfo
    {
        public string devopsIpPort = "";
        public string versionId = "";
        public string buildTimestamp = "";
    }

    public class PerformanceInfo
    {
        public string VersionId;    // 这里的versionId对应的是服务器的versioncode
        public string ServerIp;
        public string WebIpPort;
        public string BuildTimestamp;
        public int Id;  // 这里的id对应的是服务器的versionId
    }

    public class LoginInfo
    {
        public string key;
        public string loginUrl;
        public string getTokenUrl;
    }

    [Serializable]
    public class LoginKeyJsonDataType
    {
        public string key;
        public string loginUrl;
        public int status;
    }

    public class LoginTokenInfo
    {
        public string refresh_token;
        public int status;
        public string token;
    }

    public class DevopsCoreDefine
    {
        public static string PackageVersion = "2.0.0.1";
        public static string DevopsCoreConfigPath = "Packages/com.pwrd.devops/Devops-core/Runtime/Resources/Configs/DevopsCoreConfig.asset";
        public static string DevopsAARPath = "Packages/com.pwrd.devops/Devops-core/Runtime/Plugins/Android";
        public static string DevopsAARBakPath = "Packages/com.pwrd.devops/Devops-core/Runtime/Plugins/AndroidBak";
    }
}