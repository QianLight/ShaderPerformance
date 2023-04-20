using UnityEditor;
using UnityEngine;
using System.IO;
using UnityEditor.SceneManagement;

namespace XEditor
{
    public class XBuildWindow : EditorWindow
    {

        private bool enable_bugly = false;
        private bool enable_bundle = true;
        private JenkinsBuild.TPlatform platform = JenkinsBuild.TPlatform.Win32;
        private ChanelConfig.TChanel channel = ChanelConfig.TChanel.Internal;

        [MenuItem("Tools/Build/BuildWindow")]
        static void AnimExportTool()
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            {
                EditorUtility.DisplayDialog("warn", "You need to current scene first!", "ok");
            }
            else
            {
                EditorWindow.GetWindowWithRect(typeof(XBuildWindow), new Rect(0, 0, 600, 800), true, XEditorUtil.Config.buildTool);
            }
        }


        private void OnEnable()
        {
            enable_bundle = false;
            enable_bugly = false;
            platform = GetCurrPlatform();
        }

        private JenkinsBuild.TPlatform GetCurrPlatform()
        {
#if UNITY_ANDROID
             return JenkinsBuild.TPlatform.Android;
#elif UNITY_IOS || UNITY_IPHONE
            return JenkinsBuild.TPlatform.iOS;
#else
            return JenkinsBuild.TPlatform.Win32;
#endif
        }

        void OnGUI()
        {
            GUILayout.BeginVertical();
            EditorGUILayout.LabelField(XEditorUtil.Config.buildTitle);
            EditorGUILayout.Space();

            GUILayout.BeginHorizontal();
            GUILayout.Label("Select Platform:");
            GUILayout.Space(4);
            platform = (JenkinsBuild.TPlatform)EditorGUILayout.EnumPopup(platform);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label("Select Channel:");
            GUILayout.Space(4);
            channel = (ChanelConfig.TChanel)EditorGUILayout.EnumPopup(channel);
            GUILayout.EndHorizontal();
            GUILayout.Label(ChanelConfig.GetDesc(channel));

            GUILayout.Space(4);
            GUILayout.BeginHorizontal();
            enable_bugly = GUILayout.Toggle(enable_bugly, " BUGLY");
            enable_bundle = GUILayout.Toggle(enable_bundle, " BUNDLE");
            GUILayout.EndHorizontal();

            GUILayout.Space(8);
            if (GUILayout.Button("Build"))
            {
                OnClickBuild();
            }
            GUILayout.EndVertical();
        }




        private void OnClickBuild()
        {
            if (platform != GetCurrPlatform())
            {
                if (EditorUtility.DisplayDialog("warn", XEditorUtil.Config.buildSure, "ok"))
                {
                    DoBuild();
                }
            }
            else
            {
                DoBuild();
            }
        }

        private void DoBuild()
        {
            MakeChanel();
            BuildAB();
            BuildPackage();
        }


        private void MakeChanel()
        {
            ChanelConfig.Make(channel);
        }

        private void BuildAB()
        {
            if (enable_bundle)
            {
                JenkinsBuild.BuildAB();
            }
        }

        private void BuildPackage()
        {
            WriteMacro();
            switch (platform)
            {
                case JenkinsBuild.TPlatform.Android:
                    JenkinsBuild.BuildAndroid();
                    break;
                case JenkinsBuild.TPlatform.iOS:
                    JenkinsBuild.BuildIOS();
                    break;
                case JenkinsBuild.TPlatform.Win32:
                    JenkinsBuild.BuildWin32();
                    break;
                default:
                    //TO-DO
                    break;
            }
        }

        private void WriteMacro()
        {
            string macro = "FMOD_LIVEUPDATE;UNITY_POST_PROCESSING_STACK_V2;CINEMACHINE_TIMELINE;PIPELINE_URP";
            if (enable_bugly) macro += ";BUGLY";

            string path = Application.dataPath.Replace("Assets", "") + "Shell/macro.txt";
            File.WriteAllText(path, macro);
        }

    }
}