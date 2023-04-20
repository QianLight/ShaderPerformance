using System.Collections;
using System.Collections.Generic;
using B;
using UnityEditor;
using UnityEngine;

namespace CFEngine
{
    public enum WarningType
    {
        圆形,
        方形,
        //环形
    }
    [CustomEditor(typeof(SFXWarningZone))]
    public class SFXWarningZoneInspector : UnityEditor.Editor
    {
        private SFXWarningZone _target;
        //private SerializedProperty _warningType;
        
        
        private SerializedProperty HighPrefab;
        private SerializedProperty LowPrefab;
        private SerializedProperty isHightLight;
        private SerializedProperty HightLightPrefab;
        
        private SerializedProperty areas;
        private SerializedProperty spreads;
        private SerializedProperty HightLightParticle;
        
        private SerializedProperty projectionWarningObj;
        private SerializedProperty HightLightWarningObj;
        private SerializedProperty NotScaleObj;
        
        // private SerializedProperty minLoopWarningLowObj;
        // private SerializedProperty maxLoopWarningLowObj;
        // private SerializedProperty minLoopWarningHighObj;
        // private SerializedProperty maxLoopWarningHighObj;

        private static WarningType _warningType;
        private bool showMust = true;
        private bool showLow = true;
        private bool showHight = true;
        

        private void OnEnable()
        {
            //_target = target as SFXWarningZone;
            //_warningType = serializedObject.FindProperty("_warningType");
            HighPrefab = serializedObject.FindProperty("HighPrefab");
            LowPrefab = serializedObject.FindProperty("LowPrefab");
            isHightLight = serializedObject.FindProperty("isHightLight");
            HightLightPrefab = serializedObject.FindProperty("HightLightPrefab");
            areas = serializedObject.FindProperty("areas");
            spreads = serializedObject.FindProperty("spreads");
            HightLightParticle = serializedObject.FindProperty("HightLightParticle");
            projectionWarningObj = serializedObject.FindProperty("projectionWarningObj");
            //HightLightWarningObj = serializedObject.FindProperty("HightLightWarningObj");
            NotScaleObj = serializedObject.FindProperty("NotScaleObj");
            // minLoopWarningLowObj = serializedObject.FindProperty("minLoopWarningLowObj");
            // maxLoopWarningLowObj = serializedObject.FindProperty("maxLoopWarningLowObj");
            // minLoopWarningHighObj = serializedObject.FindProperty("minLoopWarningHighObj");
            // maxLoopWarningHighObj = serializedObject.FindProperty("maxLoopWarningHighObj");
            
        }

        public override void OnInspectorGUI()
        {
            
            serializedObject.Update();
            showMust = EditorGUILayout.Foldout(showMust, "----必要配置----");
            if (showMust)
            {
                _warningType = (WarningType) EditorGUILayout.EnumPopup("预警配置类型", _warningType);
                // EditorGUILayout.PropertyField(_warningType);
                EditorGUILayout.PropertyField(LowPrefab);
                EditorGUILayout.PropertyField(HighPrefab);
                EditorGUILayout.PropertyField(isHightLight);
                if (isHightLight.boolValue)
                {
                    EditorGUILayout.PropertyField(HightLightPrefab);
                    EditorGUILayout.PropertyField(HightLightParticle);
                }
            }

            if (_warningType == WarningType.圆形)
            {
                ShowCircle();
            }
            if (_warningType == WarningType.方形)
            {
                ShowSquare();
            }
            // if (_warningType == WarningType.环形)
            // {
            //     ShowLoop();
            // }
            serializedObject.ApplyModifiedProperties();
        }

        void ShowCircle()
        {
            showLow = EditorGUILayout.Foldout(showLow, "----中低配置----");
            if (showLow)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(areas);
                EditorGUI.indentLevel--;
                EditorGUILayout.PropertyField(spreads);
            }
            showHight = EditorGUILayout.Foldout(showHight, "----高配置----");
            if (showHight)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(projectionWarningObj);
                EditorGUI.indentLevel--;
            }
        }

        void ShowSquare()
        {
            showLow = EditorGUILayout.Foldout(showLow, "----中低配置----");
            if (showLow)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(areas);
                EditorGUI.indentLevel--;
                EditorGUILayout.PropertyField(spreads);
                //EditorGUILayout.PropertyField(spread1);
            }
            showHight = EditorGUILayout.Foldout(showHight, "----高配置----");
            if (showHight)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(projectionWarningObj);
                EditorGUI.indentLevel--;
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(NotScaleObj);
                EditorGUI.indentLevel--;
            }
        }

        // void ShowLoop()
        // {
        //     showLow = EditorGUILayout.Foldout(showLow, "----中低配置----");
        //     if (showLow)
        //     {
        //         // EditorGUILayout.PropertyField(minLoopWarningLowObj);
        //         // EditorGUILayout.PropertyField(maxLoopWarningLowObj);
        //     }
        //     showHight = EditorGUILayout.Foldout(showHight, "----高配置----");
        //     if (showHight)
        //     {
        //         // EditorGUI.indentLevel++;
        //         // EditorGUILayout.PropertyField(projectionWarningObj);
        //         // EditorGUI.indentLevel--;
        //         // EditorGUILayout.PropertyField(minLoopWarningHighObj);
        //         // EditorGUILayout.PropertyField(maxLoopWarningHighObj);
        //     }
        // }
    }
}

