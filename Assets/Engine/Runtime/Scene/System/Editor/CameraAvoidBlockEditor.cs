using CFEngine;
using UnityEditor;

[CustomEditor(typeof(CameraAvoidBlock))]
public class CameraAvoidBlockEditor : Editor
{
    public override void OnInspectorGUI()
    {
        CameraAvoidBlock.drawBlockGizmos.Value = EditorGUILayout.Toggle("Gizmos Debug", CameraAvoidBlock.drawBlockGizmos.Value);
        base.OnInspectorGUI();
    }
}