using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Blueprint.ActorEditor
{
    using Blueprint.Actor;
    using PENet;

    [CustomEditor(typeof(BlueprintActor))]
    public class BlueprintActorInspector : UnityEditor.Editor
    {

        private BlueprintActor blueprintActor;

        public void OnEnable()
        {
            blueprintActor = target as BlueprintActor;
        }

        public override void OnInspectorGUI()
        {
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Open Blueprint Actor", GUILayout.Width(150f), GUILayout.Height(25)))
            {
                if (!BpClient.IsConnected)
                {
                    UnityEditor.EditorUtility.DisplayDialog("警告", "蓝图未连接", "确定");
                    return ;
                }
                // 打开蓝图
            }
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
        }
    }

}