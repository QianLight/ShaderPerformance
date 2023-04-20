/*******************************************************************
* Copyright © 2017—2021 Perfect World.Co.Ltd. All rights reserved.
* author: 移动项目支持部 ZEUS FRAMEWORK TEAM
********************************************************************/
#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
namespace Zeus.Build
{
    public class ModifyPlayerSettings : IModifyPlayerSettings
    {
        //  2147483647优先级最低
        //[ModifyPlayerSettings(2147483647)]
        public void OnModifyPlayerSettings(BuildTarget target)
        {
            Debug.Log("ModifyPlayerSettings.OnModifyPlayerSettings");

            if (CommandLineArgs.ContainKey(GlobalBuild.CmdArgsKey.PRODUCT_NAME))
            {
                PlayerSettings.productName = CommandLineArgs.GetString(GlobalBuild.CmdArgsKey.PRODUCT_NAME);
            }
            if (CommandLineArgs.ContainKey(GlobalBuild.CmdArgsKey.COMPANY_NAME))
            {
                PlayerSettings.companyName = CommandLineArgs.GetString(GlobalBuild.CmdArgsKey.COMPANY_NAME);
            }
            if (CommandLineArgs.ContainKey(GlobalBuild.CmdArgsKey.APPLICATION_IDENTIFIER))
            {
                PlayerSettings.applicationIdentifier = CommandLineArgs.GetString(GlobalBuild.CmdArgsKey.APPLICATION_IDENTIFIER);
            }
            if (CommandLineArgs.ContainKey(GlobalBuild.CmdArgsKey.BUNDLE_VERSION))
            {
                PlayerSettings.bundleVersion = CommandLineArgs.GetString(GlobalBuild.CmdArgsKey.BUNDLE_VERSION);
            }
            //architecture参数0 - None, 1 - ARM64, 2 - Universal
            PlayerSettings.SetArchitecture(EditorUserBuildSettings.selectedBuildTargetGroup, 1);
            switch (target)
            {
                case BuildTarget.iOS:
                    if(CommandLineArgs.ContainKey(GlobalBuild.CmdArgsKey.APPLE_ENABLE_AUTOMATIC_SIGNING))
                    {
                        PlayerSettings.iOS.appleEnableAutomaticSigning = CommandLineArgs.GetBool(GlobalBuild.CmdArgsKey.APPLE_ENABLE_AUTOMATIC_SIGNING);
                    }
                    if (CommandLineArgs.ContainKey(GlobalBuild.CmdArgsKey.BUNDLE_NUMBER))
                    {
                        PlayerSettings.iOS.buildNumber = CommandLineArgs.GetString(GlobalBuild.CmdArgsKey.BUNDLE_NUMBER);
                    }
                    break;
                case BuildTarget.Android:
                    if (CommandLineArgs.ContainKey(GlobalBuild.CmdArgsKey.BUNDLE_NUMBER))
                    {
                        PlayerSettings.Android.bundleVersionCode = CommandLineArgs.GetInt(GlobalBuild.CmdArgsKey.BUNDLE_NUMBER);
                    }
                    if (CommandLineArgs.ContainKey(GlobalBuild.CmdArgsKey.KEYSTORE_PASSWORD))
                    {
                        PlayerSettings.Android.keystorePass = CommandLineArgs.GetString(GlobalBuild.CmdArgsKey.KEYSTORE_PASSWORD);
                    }

                    if (CommandLineArgs.ContainKey(GlobalBuild.CmdArgsKey.SCRIPTING_BACKEND))
                    {
                        switch (CommandLineArgs.GetString(GlobalBuild.CmdArgsKey.SCRIPTING_BACKEND))
                        {
                            case "IL2CPP":
                                PlayerSettings.SetScriptingBackend(BuildTargetGroup.Android, ScriptingImplementation.IL2CPP);
                                PlayerSettings.Android.targetArchitectures = AndroidArchitecture.All;
                                break;
                            case "Mono":
                                PlayerSettings.SetScriptingBackend(BuildTargetGroup.Android, ScriptingImplementation.Mono2x);
                                PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARMv7;
                                break;
                            default:
                                PlayerSettings.SetScriptingBackend(BuildTargetGroup.Android, ScriptingImplementation.IL2CPP);
                                PlayerSettings.Android.targetArchitectures = AndroidArchitecture.All;
                                break;
                        }
                    }
                    else
                    {
                        PlayerSettings.SetScriptingBackend(BuildTargetGroup.Android, ScriptingImplementation.IL2CPP);
                    }

                    string targetStr = null;
                    
                    if (CommandLineArgs.TryGetString(GlobalBuild.CmdArgsKey.ANDROID_TARGET_ARCHITECTURES,
                        ref targetStr))
                    {
                        if (PlayerSettings.GetScriptingBackend(BuildTargetGroup.Android) !=
                            ScriptingImplementation.Mono2x && !string.IsNullOrEmpty(targetStr))
                        {
                            var targetArchitecture =
                                (AndroidArchitecture) System.Enum.Parse(typeof(AndroidArchitecture), targetStr, true);
                            
                            PlayerSettings.Android.targetArchitectures = targetArchitecture;
                        }
                    }

                    if (string.IsNullOrEmpty(targetStr))
                    {
                        PlayerSettings.Android.targetArchitectures =
                            AndroidArchitecture.ARM64 | AndroidArchitecture.ARMv7;
                    }
                    
                    break;
                case BuildTarget.StandaloneWindows:
                case BuildTarget.StandaloneWindows64:
                default:
                    break;
            }
        }
    }
}
#endif