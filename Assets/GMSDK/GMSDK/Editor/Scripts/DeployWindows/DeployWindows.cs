using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using System.IO;

public class DeployWindows
{
    public static string[] GetBuildScenes()
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

    public static void BuildWindowsExe(string exePath, bool is64bit = true)
    {
        // var buildSettingsScenes = new EditorBuildSettingsScene[1];
        // var sceneToAdd = new EditorBuildSettingsScene("Assets/Demo/Scenes/PC/PCMain.unity", true); 
        // buildSettingsScenes[0] = sceneToAdd; 
        //EditorBuildSettings.scenes = newSettings;
        //var scenes = buildSettingsScenes;
        var scenes = GetBuildScenes();

        // 设置构建环境为 Win64
        if (EditorUserBuildSettings.activeBuildTarget != BuildTarget.StandaloneWindows64)
        {
            EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Standalone,
                BuildTarget.StandaloneWindows64);
        }

        // 配置 PlayerSettings
        // #pragma warning disable 0618
        // PlayerSettings.defaultIsFullScreen = false;
        // PlayerSettings.defaultScreenWidth = 360;
        // PlayerSettings.defaultScreenHeight = 640;
        // PlayerSettings.displayResolutionDialog = ResolutionDialogSetting.Disabled;
        // PlayerSettings.resizableWindow = true;
        // #pragma warning restore 0618
        #if UNITY_2019_4_OR_NEWER
        PlayerSettings.SetScriptingBackend(BuildTargetGroup.Standalone, ScriptingImplementation.IL2CPP);
        #endif

        // 构建
        BuildPipeline.BuildPlayer(scenes, exePath, is64bit ? BuildTarget.StandaloneWindows64 : BuildTarget.StandaloneWindows, BuildOptions.None);

    }
}
