/*******************************************************************
* Copyright © 2017—2021 Perfect World.Co.Ltd. All rights reserved.
* author: 移动项目支持部 ZEUS FRAMEWORK TEAM
********************************************************************/
#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
namespace Zeus.Build
{
    public class ModifyBuildPlayerOptions : IModifyPlayerSettings
    {
        //  -2147483648优先级最高，-2147483647次之
        //[ModifyPlayerSettings(-2147483647)]
        public void OnModifyPlayerSettings(BuildTarget target)
        {
            Debug.Log("ModifyBuildPlayerOptions.OnModifyBuildPlayerOptions");
            //BuildScript.buildPlayerOptions = new BuildPlayerOptions
            //{
            //    scenes = BuildScript.Levels,
            //    target = target
            //};
            BuildScript.buildPlayerOptions.scenes = BuildScript.Levels;
            BuildScript.buildPlayerOptions.target = target;
            switch (target)
            {
                case BuildTarget.iOS:
                    BuildScript.buildPlayerOptions.targetGroup = BuildTargetGroup.iOS;
                    break;
                case BuildTarget.Android:
                    BuildScript.buildPlayerOptions.targetGroup = BuildTargetGroup.Android;
                    break;
                case BuildTarget.StandaloneWindows:
                case BuildTarget.StandaloneWindows64:
                    BuildScript.buildPlayerOptions.targetGroup = BuildTargetGroup.Standalone;
                    break;
                default:
                    break;
            }

            BuildScript.buildPlayerOptions.options = BuildOptions.None;
            bool development = false;
            if (CommandLineArgs.TryGetBool(GlobalBuild.CmdArgsKey.IS_DEVELOPMENT_BUILD, ref development))
            {
                EditorUserBuildSettings.development = development;
                if (development)
                {
                    BuildScript.buildPlayerOptions.options |= BuildOptions.Development;
                    bool autoconnevtProfiler = false;
                    if (CommandLineArgs.TryGetBool(GlobalBuild.CmdArgsKey.IS_AUTOCONNECT_PROFILER, ref autoconnevtProfiler))
                    {
                        EditorUserBuildSettings.connectProfiler = autoconnevtProfiler;
                        if (autoconnevtProfiler)
                        {
                            BuildScript.buildPlayerOptions.options |= BuildOptions.ConnectWithProfiler;
                        }
                    }
                    bool deepProfiling = false;
                    if (CommandLineArgs.TryGetBool(GlobalBuild.CmdArgsKey.IS_DEEP_PROFILING, ref deepProfiling))
                    {
#if UNITY_2019_3_OR_NEWER
                        EditorUserBuildSettings.buildWithDeepProfilingSupport = deepProfiling;
                        if (deepProfiling)
                        {
                            BuildScript.buildPlayerOptions.options |= BuildOptions.EnableDeepProfilingSupport;
                        }
#endif
                    }
                    bool allowDebugging = false;
                    if (CommandLineArgs.TryGetBool(GlobalBuild.CmdArgsKey.IS_ALLOW_DEBUGGING, ref allowDebugging))
                    {
                        EditorUserBuildSettings.allowDebugging = allowDebugging;
                        if (allowDebugging)
                        {
                            BuildScript.buildPlayerOptions.options |= BuildOptions.AllowDebugging;
                        }
                    }
                }
            }

            if (EditorUserBuildSettings.allowDebugging && development)
            {
                BuildScript.buildPlayerOptions.options |= BuildOptions.AllowDebugging;
            }

#if UNITY_2021_2_OR_NEWER
            if (EditorUserBuildSettings.symlinkSources)
            {
                BuildScript.buildPlayerOptions.options |= BuildOptions.SymlinkSources;
            }
#else
            if (EditorUserBuildSettings.symlinkLibraries)
            {
                BuildScript.buildPlayerOptions.options |= BuildOptions.SymlinkLibraries;
            }
#endif

            if (target == BuildTarget.Android)
            {
                bool isBuildAndroidProject = false;
                if (CommandLineArgs.TryGetBool(GlobalBuild.CmdArgsKey.IS_BUILD_ANDROID_PROJECT, ref isBuildAndroidProject))
                {
                    EditorUserBuildSettings.exportAsGoogleAndroidProject = isBuildAndroidProject;
                }
                // if (EditorUserBuildSettings.exportAsGoogleAndroidProject)
                // {
                //     BuildScript.buildPlayerOptions.options |= BuildOptions.AcceptExternalModificationsToPlayer;
                // }



#if UNITY_2021_1_OR_NEWER
                string androidCreateSymbolsStr = AndroidCreateSymbols.Public.ToString();
                CommandLineArgs.TryGetString(GlobalBuild.CmdArgsKey.GENERATE_SYMBOL_FILE, ref androidCreateSymbolsStr);
                AndroidCreateSymbols androidCreateSymbols;
                if (!System.Enum.TryParse(androidCreateSymbolsStr, out androidCreateSymbols))
                {
                    Debug.LogError("未知AndroidCreateSymbols类型:" + androidCreateSymbolsStr);
                }
                EditorUserBuildSettings.androidCreateSymbols = androidCreateSymbols;
#else
                bool is_generate_symbol_file = true;
                CommandLineArgs.TryGetBool(GlobalBuild.CmdArgsKey.IS_GENERATE_SYMBOL_FILE, ref is_generate_symbol_file);
                EditorUserBuildSettings.androidCreateSymbolsZip = is_generate_symbol_file;
#endif
            }

#if UNITY_2021_2_OR_NEWER
            BuildScript.buildPlayerOptions.subtarget = (int)EditorUserBuildSettings.standaloneBuildSubtarget;
#else
            if (EditorUserBuildSettings.enableHeadlessMode)
            {
                BuildScript.buildPlayerOptions.options |= BuildOptions.EnableHeadlessMode;
            }
#endif
            if (EditorUserBuildSettings.connectProfiler && (development || target == BuildTarget.WSAPlayer))
            {
                BuildScript.buildPlayerOptions.options |= BuildOptions.ConnectWithProfiler;
            }
            if (EditorUserBuildSettings.buildScriptsOnly)
            {
                BuildScript.buildPlayerOptions.options |= BuildOptions.BuildScriptsOnly;
            }
#if !UNITY_2018_1_OR_NEWER  ///TODO:暂时没有确定2018之后该API的level
            if (EditorUserBuildSettings.forceOptimizeScriptCompilation)
            {
                BuildScript.buildPlayerOptions.options |= BuildOptions.ForceOptimizeScriptCompilation;
            }
#endif
        }
    }
}
#endif