using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(BoneRotateAsset))]
public class BoneRotateAssetEditor : Editor
{
    private SerializedObject m_so;
    private SerializedProperty m_time;
    private BoneRotateAsset m_asset;


    private SerializedProperty m_boneProp;
    private SerializedProperty m_startRotEnableProp;
    private SerializedProperty m_endRotEnableProp;
    private SerializedProperty m_startRotProp;
    private SerializedProperty m_endRotProp;
    private SerializedProperty m_tweenTypeProp;
    private SerializedProperty customCurveProp;

    private void OnEnable()
    {
        m_asset = target as BoneRotateAsset;
        m_so = new SerializedObject(m_asset);

        //m_time = m_so.FindProperty("m_Time");
        m_boneProp = m_so.FindProperty("m_bone");
        m_startRotEnableProp = m_so.FindProperty("m_startRotEnable");
        //m_endRotEnableProp = m_so.FindProperty("m_endRotEnable");
        m_startRotProp = m_so.FindProperty("m_startRot");
        m_endRotProp = m_so.FindProperty("m_endRot");
        m_tweenTypeProp = m_so.FindProperty("m_tweenType");
        customCurveProp = m_so.FindProperty("customCurve");
    }


    public override void OnInspectorGUI()
    {
        //EditorGUILayout.PropertyField(m_time, true);
        EditorGUILayout.PropertyField(m_boneProp, true, GUILayout.Width(500));
        EditorGUILayout.PropertyField(m_startRotEnableProp, true, GUILayout.Width(500));
        //EditorGUILayout.PropertyField(m_endRotEnableProp, true, GUILayout.Width(500));
        EditorGUILayout.PropertyField(m_startRotProp, true, GUILayout.Width(500));
        EditorGUILayout.PropertyField(m_endRotProp, true, GUILayout.Width(500));
        EditorGUILayout.PropertyField(m_tweenTypeProp, true, GUILayout.Width(500));
        if(m_tweenTypeProp.enumValueIndex  == (int)BoneRotateAsset.TweenType.Custom)
        {
            EditorGUILayout.PropertyField(customCurveProp, true, GUILayout.Width(500));
        }

        m_so.ApplyModifiedProperties();
    }
}
