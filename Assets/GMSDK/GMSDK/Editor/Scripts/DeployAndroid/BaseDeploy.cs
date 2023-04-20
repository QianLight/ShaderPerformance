using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

public class BaseDeploy
{

    public static void SetBuildSettingAndroid()
    {
        //Switch to android build target if not
        if (EditorUserBuildSettings.activeBuildTarget != BuildTarget.Android)
            EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Android, BuildTarget.Android);

        //Change build system to gradle
        EditorUserBuildSettings.androidBuildSystem = AndroidBuildSystem.Gradle;
    }

    /// <summary>
    /// 获取需要构建的Scenes
    /// </summary>
    /// <returns></returns>
    public virtual string[] GetBuildScenes()
    {
        List<string> scenes = new List<string>();
        int sceneCount = SceneManager.sceneCountInBuildSettings;
        for (int i = 0; i < sceneCount; i++)
        {
            string path = SceneUtility.GetScenePathByBuildIndex(i);
            if (EditorBuildSettings.scenes[i].enabled && File.Exists(path))
            {
                scenes.Add(path);
            }
            else
            {
                Debug.Log(path + ", doesn't exists.");
            }
        }

        return scenes.ToArray();
    }

    public bool BuildAndroidProject(string projectPath)
    {
        if (projectPath == "")
        {
            Debug.Log("Export Android Project Path is empty.");
            return false;
        }

        //1. 设置为构建Android
        SetBuildSettingAndroid();
        EditorUserBuildSettings.exportAsGoogleAndroidProject = true;

        //2. 开始构建
        string[] levels = GetBuildScenes();
        BuildPlayerOptions playerOptions = new BuildPlayerOptions();
        playerOptions.scenes = levels;
        playerOptions.locationPathName = projectPath;
        playerOptions.target = BuildTarget.Android;
        playerOptions.options = EditorUserBuildSettings.development
            ? BuildOptions.None | BuildOptions.Development
            : BuildOptions.None;
        BuildPipeline.BuildPlayer(playerOptions);
        return true;
    }

    public bool BuildAndroidApk(string apkPath)
    {
        if (apkPath == "")
        {
            Debug.Log("Export Android Apk Path is empty.");
            return false;
        }
        //1. 设置为构建Android
        SetBuildSettingAndroid();
        EditorUserBuildSettings.exportAsGoogleAndroidProject = false;

        //2. 开始构建
        string[] levels = GetBuildScenes();
        BuildPlayerOptions playerOptions = new BuildPlayerOptions();
        playerOptions.scenes = levels;
        playerOptions.locationPathName = apkPath;
        playerOptions.target = BuildTarget.Android;
        playerOptions.options = EditorUserBuildSettings.development
            ? BuildOptions.None | BuildOptions.Development
            : BuildOptions.None;
        string message = EditorUserBuildSettings.development ? "build debug apk." : "build release apk.";
        Debug.Log(message);
        BuildPipeline.BuildPlayer(playerOptions);
        return true;
    }

    public bool BuildAndroidApk(string apkPath, BuildOptions addOptions = BuildOptions.None)
    {
        if (apkPath == "")
        {
            Debug.Log("Export Android Apk Path is empty.");
            return false;
        }

        //1. 设置为构建Android
        SetBuildSettingAndroid();

        //2. 开始构建
        var option = BuildOptions.None; // | BuildOptions.AcceptExternalModificationsToPlayer;
        string[] levels = GetBuildScenes();
        if (EditorUserBuildSettings.development)
        {
            Debug.Log("build debug apk.");
            option |= BuildOptions.Development;
        }
        else
        {
            Debug.Log("build release apk.");
        }

        option |= addOptions;
        BuildPipeline.BuildPlayer(levels, apkPath, BuildTarget.Android, option);
        // var report = BuildPipeline.BuildPlayer(levels, apkPath, BuildTarget.Android, option);
        // if (report.summary.totalErrors > 0)
        // {
        //     UnityEngine.Debug.LogErrorFormat("BuildPlayer error: {0}", report.ToString());
        //     return false;
        // }
        return true;
    }
}