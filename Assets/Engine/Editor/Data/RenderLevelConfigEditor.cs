using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(RenderLevelConfig))]
public class RenderLevelConfigEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        if (target is RenderLevelConfig config && config)
        {
            if (!RenderLevelConfig.CheckValidate(config, out string error))
            {
                EditorGUILayout.HelpBox(error, MessageType.Error);
            }
        }
    }
}
