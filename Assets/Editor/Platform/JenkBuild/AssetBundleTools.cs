using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using System.IO;
using UnityEditor.SceneManagement;
using CFEngine;

public class AssetBundleTools
{
    [MenuItem("Tools/AssetBundleEnvironment/Build")]
    public static void BuildAssetBundleLoadEnvironment()
    {
        if (Directory.Exists(Application.streamingAssetsPath + "/Bundles/assets/bundleres"))
            Directory.Delete(Application.streamingAssetsPath + "/Bundles/assets", true);
        CFEngine.Editor.BuildBundleConfig.instance.BuildBundle("", -1, CFEngine.Editor.BuildType.PreBuild, false);

        if (Directory.Exists(Application.dataPath.Replace("/Assets", "/OuterPackage/bundleres")))
            Directory.Delete(Application.dataPath.Replace("/Assets", "/OuterPackage/bundleres"), true);
        if (!Directory.Exists(Application.dataPath.Replace("/Assets", "/OuterPackage")))
            Directory.CreateDirectory(Application.dataPath.Replace("/Assets", "/OuterPackage"));
        Directory.Move(Application.streamingAssetsPath + "/Bundles/assets/bundleres",
            Application.dataPath.Replace("/Assets", "/OuterPackage/bundleres"));

        if (File.Exists(Application.dataPath + "/ZeusSetting/ZeusAssetManagerSetting.json"))
            File.Delete(Application.dataPath + "/ZeusSetting/ZeusAssetManagerSetting.json");
        File.Copy(Application.dataPath + "/Editor/Platform/JenkBuild/AssetBundleEnvironment",
            Application.dataPath + "/ZeusSetting/ZeusAssetManagerSetting.json");

        EditorSceneManager.OpenScene("Assets/Scenes/entrance.unity");
        GameObject.Find("GamePoint").GetComponent<XScript>().debugFlag |= (uint)EDebugFlag.Bundle;
    }


    [MenuItem("Tools/AssetBundleEnvironment/BuildForLocal")]
    public static void BuildAssetBundleLoadEnvironmentForLocal()
    {
        if (Directory.Exists(Application.streamingAssetsPath + "/Bundles/assets/bundleres"))
            Directory.Delete(Application.streamingAssetsPath + "/Bundles/assets", true);
        CFEngine.Editor.BuildBundleConfig.instance.BuildBundle("", -1, CFEngine.Editor.BuildType.PreBuild, false);

    }

    [MenuItem("Tools/AssetBundleEnvironment/Reset")]
    public static void ResetAssetBundleLoadEnvironment()
    {
        if (Directory.Exists(Application.dataPath.Replace("/Assets", "/OuterPackage/bundleres")))
            Directory.Delete(Application.dataPath.Replace("/Assets", "/OuterPackage/bundleres"), true);
        if (File.Exists(Application.dataPath + "/ZeusSetting/ZeusAssetManagerSetting.json"))
            File.Delete(Application.dataPath + "/ZeusSetting/ZeusAssetManagerSetting.json");
        File.Copy(Application.dataPath + "/Editor/Platform/JenkBuild/DefaultEnvironment",
            Application.dataPath + "/ZeusSetting/ZeusAssetManagerSetting.json");

        EditorSceneManager.OpenScene("Assets/Scenes/entrance.unity");
    }
}