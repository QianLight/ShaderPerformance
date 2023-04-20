using AssetCheck;
using AssetCheck.AssetCheckToDevops;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class RuleSyncGUI : EditorWindow
{
    public static void ShowWindow()
    {
        if (EditorWindow.HasOpenInstances<RuleSyncGUI>())
            return;
        RuleSyncGUI ruleGUI = (RuleSyncGUI)EditorWindow.GetWindow(typeof(RuleSyncGUI));
        ruleGUI.maxSize = new Vector2(500, 280);
        ruleGUI.titleContent = new GUIContent("填写提交信息");
        ruleGUI.Show();
    }

    public static void HideWindow()
    {
        if (!EditorWindow.HasOpenInstances<RuleSyncGUI>())
            return;
        RuleSyncGUI ruleGUI = (RuleSyncGUI)EditorWindow.GetWindow(typeof(RuleSyncGUI));
        ruleGUI.Close();
    }

    string msg = string.Empty;

    private void OnGUI()
    {
        GUILayout.BeginVertical();
        Rect textRect = EditorGUILayout.GetControlRect(GUILayout.Height(168));
        msg = EditorGUI.TextArea(textRect, msg);
        if(msg.Length == 0)
        {
            Rect labelRect = textRect.GetTopPart(18);
            EditorGUI.LabelField(labelRect, "请输入内容", GUIDefines.DisableText);
        }
        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        if (GUILayout.Button("取消", GUIDefines.ButtonOrangeOutlineStyle, GUILayout.Width(80)))
        {
            HideWindow();
        }
        if(GUILayout.Button("确定", GUIDefines.ButtonOrangeStyle, GUILayout.Width(80)))
        {
            AssetCheckPathConfig checkPathConfig = AssetDatabase.LoadAssetAtPath<AssetCheckPathConfig>($"{Defines.CheckPathConfigPath}/{Defines.CheckPathConfigName}");
            ToDevopsRules toDevopsRules = DevopsReports.ConfigToDevopsRules(checkPathConfig);
            toDevopsRules.updateMsg = msg;
            DevopsRulesCheck.SyncRuleAndPaths(toDevopsRules);
            HideWindow();
        }
        GUILayout.EndHorizontal();
        GUILayout.EndVertical();
    }
}
