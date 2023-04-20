using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace Devops.Core
{
    [Serializable]
    public class DevopsCoreConfig : ScriptableObject
    {
        public string DevopsIpPort;
        public string VersionId;
        public string BuildTimestamp;
    }
}