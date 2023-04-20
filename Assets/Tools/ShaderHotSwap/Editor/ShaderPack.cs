using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace UsingTheirs.ShaderHotSwap
{

    public static class ShaderPack
    {
        public static string PackShaders(BuildTarget buildTarget, string outputDir, ShaderData[] shaderDataList)
        {
            var assetBundleName = "shaderList";
            var assetBundlePath = Path.Combine(outputDir, assetBundleName);
            Directory.CreateDirectory(outputDir);

            var buildMap = new AssetBundleBuild[1];
            buildMap[0].assetBundleName = assetBundleName;

            Logger.Log( "[ShaderPack] Shaders Len = " + shaderDataList.Length);

            var assetPaths = new List<string>();
            var assetNames = new List<string>();
            for (int i = 0; i < shaderDataList.Length; ++i)
            {
                // Shader
                var shaderPath = AssetDatabase.GetAssetPath(shaderDataList[i].shader);
                assetPaths.Add(shaderPath);
                assetNames.Add(AssetDatabase.AssetPathToGUID(shaderPath));
                Logger.Log( "[ShaderPack] Asset Path = " + assetPaths[i]);
                
                // Reference Material
                /*
                if (shaderDataList[i].refMaterial != null)
                {
                    var matPath = AssetDatabase.GetAssetPath(shaderDataList[i].refMaterial);
                    assetPaths.Add(matPath);
                    assetNames.Add(AssetDatabase.AssetPathToGUID(matPath));
                }
                */
            }

            buildMap[0].assetNames = assetPaths.ToArray();
            buildMap[0].addressableNames = assetNames.ToArray();

            BuildPipeline.BuildAssetBundles(outputDir, buildMap, BuildAssetBundleOptions.None,
                buildTarget);

            var bytes = File.ReadAllBytes(assetBundlePath);
            var base64 = Convert.ToBase64String(bytes);
            //Debug.Log(base64);

            return base64;
        }
    }

}
