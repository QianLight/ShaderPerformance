using UnityEditor;
using UnityEngine;

namespace GSDK.UnityEditor
{
    
    
    [CustomEditor(typeof(PCConfigSettings))]
    public class PCConfigInspector : Editor
    {
        protected PCConfigSettings pcConfigSettings;
        public override void OnInspectorGUI()
        {
            pcConfigSettings = (PCConfigSettings) target;
            EditorGUILayout.LabelField (new GUIContent("app_id[?]", "`通过应用云平台申请"));
            pcConfigSettings.AppID = EditorGUILayout.TextField(pcConfigSettings.AppID);
            EditorGUILayout.LabelField (new GUIContent("app_name[?]", "通过应用云平台申请的英文名"));
            pcConfigSettings.AppName = EditorGUILayout.TextField(pcConfigSettings.AppName); 
            EditorGUILayout.LabelField (new GUIContent("display_name[?]", "用于在登陆面板展示"));
            pcConfigSettings.Displayname = EditorGUILayout.TextField(pcConfigSettings.Displayname);
            EditorGUILayout.LabelField(new GUIContent("package_name[?]", "应用包名"));
            pcConfigSettings.PackageName = EditorGUILayout.TextField(pcConfigSettings.PackageName);
        }
    }
}