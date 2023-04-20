using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Devops.Core
{
    public class AARImport
    {
        static string[] DefaultAARs = { "app-release.aar", "core-1.3.0.aar",  };
        static string[] ShenMoAARs = { "app-release_SM.aar" };
        [MenuItem("Devops/ImportAAR/Default")]
        static void ImportDefault()
        {
            DeleteAllAARs();
            CopyAARsToPluginFolder(DefaultAARs);
        }

        [MenuItem("Devops/ImportAAR/ShenMoDaLu")]
        static void ImportShenMoDaLu()
        {
            DeleteAllAARs();
            CopyAARsToPluginFolder(ShenMoAARs);
        }

        static void DeleteAllAARs()
        {
            string[] fileGUIDs = AssetDatabase.FindAssets("", new string[] { DevopsCoreDefine.DevopsAARPath });
            for (var i = 0; i < fileGUIDs.Length; i++)
            {
                var assetPath = AssetDatabase.GUIDToAssetPath(fileGUIDs[i]);
                AssetDatabase.DeleteAsset(assetPath);
            }
        }

        static void CopyAARsToPluginFolder(string[] files)
        {
            foreach(var file in files) 
            {
                string aarFile = DevopsCoreDefine.DevopsAARPath + "/" + file;
                string aarBakFile = DevopsCoreDefine.DevopsAARBakPath + "/" + file + ".bak";
                AssetDatabase.CopyAsset(aarBakFile, aarFile);
            }
        }
    }
}