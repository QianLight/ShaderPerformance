using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class UITool {

    [MenuItem("Tools/UI/合 批 分 析")]
    public static void OpenUIAssistant()
    {
        UIAssistantWindow windows = EditorWindow.GetWindow<UIAssistantWindow>();
        windows.autoRepaintOnSceneChange = true;
        windows.titleContent = new GUIContent("UIAssistant");
        windows.maxSize = new Vector2(1600, 1000);
        windows.minSize = new Vector2(200, 500);

        windows.Show();
    }


    [MenuItem("GameObject/UI/合 批 分 析" , priority = 3)]
    public static void ShowUIAssistant()
    {
        UIAssistantWindow windows = EditorWindow.GetWindow<UIAssistantWindow>();
        windows.autoRepaintOnSceneChange = true;
        windows.titleContent = new GUIContent("UIAssistant");
        windows.maxSize = new Vector2(1600, 1000);
        windows.minSize = new Vector2(200, 500);
        windows.Show();
        windows.ShowSelection();
    }
}
