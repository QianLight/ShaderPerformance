/*******************************************************************
* Copyright © 2017—2021 Perfect World.Co.Ltd. All rights reserved.
* author: 移动项目支持部 ZEUS FRAMEWORK TEAM
********************************************************************/
#if UNITY_EDITOR && UNITY_IOS
using UnityEditor;
using Zeus.Build;
using UnityEditor.iOS.Xcode;

namespace Zeus.XCodeEditor
{
    public class AfterBuildXCode : IAfterBuild
    {
        //[AfterBuild]
        public void OnAfterBuild(BuildTarget target, string outputPath)
        {
            if (target != BuildTarget.iOS) { return; }
            
            string projectPath = PBXProject.GetPBXProjectPath(outputPath);
            PBXProject project = new PBXProject();
            project.ReadFromFile(projectPath);
#if UNITY_2019_3_OR_NEWER
            string targetGuid = project.GetUnityMainTargetGuid();
            string unityFrameworkTargetGuid = project.GetUnityFrameworkTargetGuid();
#else
            string targetGuid = project.TargetGuidByName(PBXProject.GetUnityTargetName());
            string unityFrameworkTargetGuid = project.TargetGuidByName("UnityFramework");
#endif

            // 设置签名证书
            if (!PlayerSettings.iOS.appleEnableAutomaticSigning)
            {
                var code_sign_identity = GlobalBuild.Default.CODE_SIGN_IDENTITY;
                CommandLineArgs.TryGetString(GlobalBuild.CmdArgsKey.CODE_SIGN_IDENTITY, ref code_sign_identity);
                project.SetBuildProperty(targetGuid, "CODE_SIGN_IDENTITY", code_sign_identity);

                var provisioning_profile_specifier =  GlobalBuild.Default.PROVISIONING_PROFILE_SPECIFIER;
                CommandLineArgs.TryGetString(GlobalBuild.CmdArgsKey.PROVISIONING_PROFILE_SPECIFIER, ref provisioning_profile_specifier);
                project.SetBuildProperty(targetGuid, "PROVISIONING_PROFILE_SPECIFIER", provisioning_profile_specifier);
            }

            var development_team = GlobalBuild.Default.DEVELOPMENT_TEAM;
            CommandLineArgs.TryGetString(GlobalBuild.CmdArgsKey.DEVELOPMENT_TEAM, ref development_team);
            project.SetTeamId(targetGuid, development_team);
            project.SetBuildProperty(targetGuid, "ENABLE_BITCODE", GlobalBuild.Default.ENABLE_BITCODE);
            project.SetBuildProperty(unityFrameworkTargetGuid, "ENABLE_BITCODE", GlobalBuild.Default.ENABLE_BITCODE);

            bool is_generate_symbol_file = true;
            CommandLineArgs.TryGetBool(GlobalBuild.CmdArgsKey.IS_GENERATE_SYMBOL_FILE, ref is_generate_symbol_file);
            var generate_debug_symbols = "YES";
            if(!is_generate_symbol_file)
                generate_debug_symbols = "NO";
            project.SetBuildProperty(targetGuid, "GCC_GENERATE_DEBUGGING_SYMBOLS", generate_debug_symbols);
            project.SetBuildProperty(unityFrameworkTargetGuid, "GCC_GENERATE_DEBUGGING_SYMBOLS", generate_debug_symbols);

            //  应用修改
            project.WriteToFile(projectPath);
        }
    }
}
#endif