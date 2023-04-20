/*******************************************************************
* Copyright © 2017—2021 Perfect World.Co.Ltd. All rights reserved.
* author: 移动项目支持部 ZEUS FRAMEWORK TEAM
********************************************************************/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Zeus.Build
{
    [CreateAssetMenu(fileName = "BuildProperty", menuName = "ScriptableObjects/ZeusBuildProperty", order = 1)]
    public class BuildPropertyScriptableObject : ScriptableObject
    {
        public string buildManifestPath = "/Zeus/Core/Editor/Build/ZeusBuildManifest.xml";
        public string customBuildManifestPath = "/ZeusBuildManifest.xml";
        public string buildParamConfigPath = GlobalBuild.BuildConst.ZEUS_BUILD_PATH;
    }
}

