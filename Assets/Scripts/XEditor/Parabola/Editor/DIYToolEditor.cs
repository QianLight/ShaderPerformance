#if UNITY_EDITOR
using UnityEngine;
using CFUtilPoolLib;
using UnityEditor;


[CustomEditor(typeof(DIYVisualTool))]
class DIYToolEditor : Editor
{
    DIYVisualTool tool;
    DiyCamera.RowData[] Table;
    string[] desc;

    static int start, end;
    static bool folder;


    private void OnEnable()
    {
        tool = target as DIYVisualTool;

        DiyCamera table = new DiyCamera();
        XTableReader.ReadFile("Table/DiyCamera", table);
        Table = table.Table;
        desc = new string[Table.Length];
        for (int i = 0; i < desc.Length; i++)
        {
            desc[i] = Table[i].comment;
        }
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        EditorGUILayout.Space();
        DrawConfig();
    }

    private void DrawConfig()
    {
        folder = EditorGUILayout.Foldout(folder, "表格快速预览工具");

        if (tool.targetTf == null)
        {
            EditorGUILayout.HelpBox("Setup Env First!", MessageType.Error);
        }
        else if (folder)
        {
            EditorGUI.BeginChangeCheck();

            EditorGUILayout.BeginVertical(EditorStyles.textField);
            start = EditorGUILayout.Popup("起始点", start, desc);
            end = EditorGUILayout.Popup("终止点", end, desc);
            if (start == end)
            {
                EditorGUILayout.HelpBox("起始点和终止点不能相同", MessageType.Error);
            }

            EditorGUILayout.EndVertical();
            if (EditorGUI.EndChangeCheck())
            {
                var row = Table[start];
                tool.origTf.position = A2Vector(row.pos);
                tool.origTf.localEulerAngles = A2Vector(row.rot);
                tool.origFov = row.fov;

                row = Table[end];
                tool.targetTf.position = A2Vector(row.pos);
                tool.targetTf.localEulerAngles = A2Vector(row.rot);
                tool.targetFov = row.fov;
            }
        }
    }


    private Vector3 A2Vector(float[] arr)
    {
        Vector3 v = new Vector3(arr[0], arr[1], arr[2]);
        return v;
    }

}

#endif