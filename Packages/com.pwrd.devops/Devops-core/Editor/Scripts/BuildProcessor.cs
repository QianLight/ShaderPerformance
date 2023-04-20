#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace Devops.Core
{
    public class BuildProcessor : IPreprocessBuildWithReport
    {
        public int callbackOrder { get { return 0; } }
        public void OnPreprocessBuild(BuildReport report)
        {
            CreateConfig.CreateCoreConfig();
            DevopsCoreConfig config = AssetDatabase.LoadAssetAtPath<DevopsCoreConfig>(DevopsCoreDefine.DevopsCoreConfigPath);
            config.BuildTimestamp = report.summary.buildStartedAt.Ticks.ToString();
            EditorUtility.SetDirty(config);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
    }
}
#endif