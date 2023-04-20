#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;

namespace Devops.Core
{
    public class CommandLineTool
    {
        public static string GetEnvironmentVariable(string key)
        {
            string[] allArgs = System.Environment.GetCommandLineArgs();
            for (int i = 0; i < allArgs.Length; i++)
            {
                if (allArgs[i].Equals(key) && (i + 1) < allArgs.Length)
                {
                    return allArgs[i + 1];
                }
            }
            return string.Empty;
        }
    }

    public class CreateConfig : Editor
    {
        public static void CreateCoreConfig()
        {
            ScriptableObject Config = CreateInstance<DevopsCoreConfig>();
            string assetFolder = "Packages/com.pwrd.devops/Devops-core/Runtime/Resources/Configs";
            if (!Directory.Exists(assetFolder))
                Directory.CreateDirectory(assetFolder);
            string assetFile = assetFolder + "/DevopsCoreConfig.asset";
            if (!File.Exists(assetFile))
                AssetDatabase.CreateAsset(Config, assetFile);
        }

        static void SetDevopsCoreInfo()
        {
            string[] args = System.Environment.GetCommandLineArgs();
            string strIp = CommandLineTool.GetEnvironmentVariable("-ip");
            string strVersionId = CommandLineTool.GetEnvironmentVariable("-versionId");
            if (strIp == string.Empty || strVersionId == string.Empty)
            {
                Debug.LogError("Devops Can not get ip or versionId");
                return;
            }
            CreateCoreConfig();
            DevopsCoreConfig config = AssetDatabase.LoadAssetAtPath<DevopsCoreConfig>(DevopsCoreDefine.DevopsCoreConfigPath);
            config.DevopsIpPort = strIp;
            config.VersionId = strVersionId;
            EditorUtility.SetDirty(config);
            AssetDatabase.SaveAssets();
        }
    }
}
#endif