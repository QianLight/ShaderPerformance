using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace SceneTools
{
    public class ReNameTool : EditorWindow
    {
        public class PropItems : ScriptableObject
        {
            public List<GameObject> objs = new List<GameObject>();
        }

        [MenuItem("Tools/场景/批量重命名")]
        private static void ShowWindow()
        {
            var window = GetWindow<ReNameTool>();
            window.minSize = new Vector2(400, 300);
            window.titleContent = new GUIContent("ReNameTool");
            window.Show();
        }

        private PropItems props;
        private SerializedObject serializedObject;
        private SerializedProperty assetLstProperty;

        private string newNameStr = "";
        private int startIndex = 1;

        private GUILayoutOption descWidth = GUILayout.Width(70);
        private GUILayoutOption contentWidth = GUILayout.Width(200);

        private GUILayoutOption objContentWidth = GUILayout.Width(300);
        private GUILayoutOption btnContentWidth = GUILayout.Width(280);

        private Vector2 curPos;

        protected void OnEnable()
        {
            props = ScriptableObject.CreateInstance<PropItems>();
            serializedObject = new SerializedObject(props);
            assetLstProperty = serializedObject.FindProperty("objs");
        }

        private void OnDestroy()
        {
            props = null;
            serializedObject = null;
            assetLstProperty = null;
        }

        protected void OnGUI()
        {
            //更新
            serializedObject.Update();

            EditorGUILayout.Space(10);

            EditorGUILayout.BeginHorizontal();
            GUILayout.Space(10);
            EditorGUILayout.LabelField("新的名字", descWidth);
            newNameStr = GUILayout.TextArea(newNameStr, contentWidth);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(1);

            EditorGUILayout.BeginHorizontal();
            GUILayout.Space(10);
            EditorGUILayout.LabelField("起始索引", descWidth);
            startIndex = EditorGUILayout.IntField(startIndex, contentWidth);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(10);
            EditorGUILayout.BeginHorizontal();
            GUILayout.Space(10);
            if (GUILayout.Button("清空列表", btnContentWidth))
            {
                PropItems tempProps = serializedObject.targetObject as PropItems;
                if (tempProps == null || tempProps.objs == null)
                {
                    return;
                }

                tempProps.objs.Clear();
                serializedObject.ApplyModifiedProperties();
            }

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            GUILayout.Space(10);
            if (GUILayout.Button("批量重命名", btnContentWidth))
            {
                PropItems tempProps = serializedObject.targetObject as PropItems;
                if (tempProps == null || tempProps.objs == null)
                {
                    return;
                }

                for (int i = 0; i < tempProps.objs.Count; i++)
                {
                    tempProps.objs[i].name = newNameStr + (startIndex + i);
                }

                serializedObject.ApplyModifiedProperties();
            }

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(10);
            curPos = EditorGUILayout.BeginScrollView(curPos);
            //显示属性
            //第二个参数必须为true，否则无法显示子节点即List内容
            EditorGUILayout.PropertyField(assetLstProperty, true, objContentWidth);

            EditorGUILayout.EndScrollView();
        }
    }
}