using CFEngine;
using UnityEditor;

using UnityEngineEditor = UnityEditor.Editor;
[CanEditMultipleObjects, CustomEditor (typeof (XScript))]
public class ScriptDebug : UnityEngineEditor
{
    // Use this for initialization
    public override void OnInspectorGUI ()
    {
        serializedObject.Update ();
        XScript script = target as XScript;
        if (script != null)
        {
            EditorGUI.BeginChangeCheck ();
            bool isDebugBundle = (script.debugFlag & (uint) EDebugFlag.Bundle) != 0;
            isDebugBundle = EditorGUILayout.Toggle ("Debug Bundle", isDebugBundle);
            script.openLog = EditorGUILayout.Toggle(new UnityEngine.GUIContent("Debug Log", "此开关仅编辑器有效，打包时用 DISABLE_LOG 宏控制unity底层log开关"), script.openLog);
            UnityEngine.Debug.unityLogger.logEnabled = script.openLog;
            if (EditorGUI.EndChangeCheck ())
            {
                Undo.RecordObject (target, "Debug Bundle");
                if (isDebugBundle)
                {
                    script.debugFlag |= (uint) EDebugFlag.Bundle;
                }
                else
                {
                    script.debugFlag &= ~((uint) EDebugFlag.Bundle);
                }
            }
        }
    }
}