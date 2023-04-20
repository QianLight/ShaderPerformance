using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace AssetCheck
{
    public class BuildConfig : Editor
    {
        [MenuItem("Devops/AssetCheck/DevopsCoreConfig/AssetCheckPathConfig")]
        static void CreateAssetCheckPathConfig()
        {
            ScriptableObject Config = CreateInstance<AssetCheckPathConfig>();
            string assetFolder = Defines.CheckPathConfigPath;
            if (!Directory.Exists(assetFolder))
                Directory.CreateDirectory(assetFolder);
            string assetFile = $"{Defines.CheckPathConfigPath}/{Defines.CheckPathConfigName}";
            if (!File.Exists(assetFile))
                AssetDatabase.CreateAsset(Config, assetFile);
        }

        public static AssetCheckPathConfig CreateTempAssetCheckPathConfig()
        {
            return CreateInstance<AssetCheckPathConfig>();
        }

        [MenuItem("Devops/AssetCheck/DevopsCoreConfig/AssetCheckTagsConfig")]
        public static void CreateAssetCheckTagsConfig()
        {
            ScriptableObject Config = CreateInstance<AssetCheckTags>();
            string assetFolder = Defines.CheckPathConfigPath;
            if (!Directory.Exists(assetFolder))
                Directory.CreateDirectory(assetFolder);
            string assetFile = $"{Defines.CheckPathConfigPath}/{Defines.CheckTagsConfigName}";
            if (!File.Exists(assetFile))
                AssetDatabase.CreateAsset(Config, assetFile);
        }
    }

}
