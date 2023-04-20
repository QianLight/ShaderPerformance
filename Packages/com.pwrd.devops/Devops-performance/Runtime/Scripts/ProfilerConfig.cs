using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Devops.Performance
{
    [Serializable]
    public class ProfilerConfig : ScriptableObject
    {
        // app name
        public String AppName;
        //worker ip
        public string WorkerIpAddress;
        //web ip
        public string ReportWebIpAddress;
        //build timestamp
        public string BuildTimestamp;
        //
        public string VersionId;
    }
}