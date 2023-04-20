#if UNITY_EDITOR
using UnityEditor;
#endif

using UnityEngine;

public enum target_type
{
    level = 1 << 0,
    ai = 1 << 1,
    designateAI=1<<2,
}
public class XSpawnWall : XWall
{
    public string exString;
    public etrigger_type TriggerType;
    public int target=1;
    public uint aiID;

    public enum etrigger_type
    {
        once,
        always
    }

    protected override void OnTriggered ()
    {
        // if (exString != null && exString.Length > 0)
        // {
        //     _interface.SetExternalString(exString, (TriggerType == etrigger_type.once));
        // }
    }
}

#if UNITY_EDITOR
[CanEditMultipleObjects, CustomEditor (typeof (XSpawnWall))]
public class XSpawnWallEditor : XWallEditor
{
    SerializedProperty exString;
    SerializedProperty TriggerType;
    new SerializedProperty target;
    SerializedProperty aiID;

    public override void OnEnable ()
    {
        base.OnEnable();
        exString = serializedObject.FindProperty ("exString");
        TriggerType = serializedObject.FindProperty ("TriggerType");
        target = serializedObject.FindProperty("target");
        aiID = serializedObject.FindProperty("aiID");
    }
    public override void OnInspectorGUI ()
    {
        base.OnInspectorGUI ();
        EditorGUILayout.PropertyField (exString);
        EditorGUILayout.PropertyField (TriggerType);
        target.intValue = (int)(target_type)EditorGUILayout.EnumFlagsField("TargetType",(target_type)(target.intValue));
        EditorGUILayout.PropertyField(aiID);
        serializedObject.ApplyModifiedProperties ();
    }
}
#endif