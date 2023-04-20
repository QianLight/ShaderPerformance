using System;
using System.Collections;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.Timeline;

[CustomEditor(typeof(FacialExpressionCurve))]
public class FacialExpressionCurveInspector : Editor
{
    public SerializedProperty idle;

    public SerializedProperty A;
    public SerializedProperty E;
    public SerializedProperty I;
    public SerializedProperty O;
    public SerializedProperty U;

    public SerializedProperty eyebrow_sad;
    public SerializedProperty eyebrow_happy;
    public SerializedProperty eyebrow_angry;

    public SerializedProperty eye_Squint;
    public SerializedProperty eye_Earnest;
    public SerializedProperty eye_Stare;

    public SerializedProperty mouth_smile;
    public SerializedProperty mouth_laugh;
    public SerializedProperty mouth_sad;
    public SerializedProperty mouth_angry;

    public SerializedProperty blink;
    //public SerializedProperty getangry;
    //public SerializedProperty normal;
    //public SerializedProperty teether;
    //public SerializedProperty afraid;
    //public SerializedProperty nervous;
    //public SerializedProperty sad;

    private void OnEnable()
    {
        idle = serializedObject.FindProperty("idle");

        A = serializedObject.FindProperty("A");
        E = serializedObject.FindProperty("E");
        I = serializedObject.FindProperty("I");
        O = serializedObject.FindProperty("O");
        U = serializedObject.FindProperty("U");

        eyebrow_sad = serializedObject.FindProperty("eyebrow_sad");
        eyebrow_happy = serializedObject.FindProperty("eyebrow_happy");
        eyebrow_angry = serializedObject.FindProperty("eyebrow_angry");

        eye_Squint = serializedObject.FindProperty("eye_Squint");
        eye_Earnest = serializedObject.FindProperty("eye_Earnest");
        eye_Stare = serializedObject.FindProperty("eye_Stare");

        mouth_smile = serializedObject.FindProperty("mouth_smile");
        mouth_laugh = serializedObject.FindProperty("mouth_laugh");
        mouth_sad = serializedObject.FindProperty("mouth_sad");
        mouth_angry = serializedObject.FindProperty("mouth_angry");
        blink = serializedObject.FindProperty("blink");

        //surprise = serializedObject.FindProperty("surprise");
        //getangry = serializedObject.FindProperty("getangry");
        //normal = serializedObject.FindProperty("normal");
        //teether = serializedObject.FindProperty("teether");
        //afraid = serializedObject.FindProperty("afraid");
        //nervous = serializedObject.FindProperty("nervous");
        //sad = serializedObject.FindProperty("sad");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        EditorGUILayout.Slider(idle, 0, 1, new GUIContent("Idle动作"));

        EditorGUILayout.Slider(A, 0, 1, "元音A");
        EditorGUILayout.Slider(E, 0, 1, "元音E");
        EditorGUILayout.Slider(I, 0, 1, "元音I");
        EditorGUILayout.Slider(O, 0, 1, "元音O");
        EditorGUILayout.Slider(U, 0, 1, "元音U");

        EditorGUILayout.Slider(eyebrow_sad, 0, 1, "眉毛哀伤");
        EditorGUILayout.Slider(eyebrow_happy, 0, 1, "眉毛开心");
        EditorGUILayout.Slider(eyebrow_angry, 0, 1, "眉毛愤怒");

        EditorGUILayout.Slider(eye_Squint, 0, 1, "眼睛眯眼");
        EditorGUILayout.Slider(eye_Earnest, 0, 1, "眼睛认真");
        EditorGUILayout.Slider(eye_Stare, 0, 1, "眼睛瞪大");

        EditorGUILayout.Slider(mouth_smile, 0, 1, "嘴巴微笑");
        EditorGUILayout.Slider(mouth_laugh, 0, 1, "嘴巴大笑");
        EditorGUILayout.Slider(mouth_sad, 0, 1, "嘴巴哀伤");
        EditorGUILayout.Slider(mouth_angry, 0, 1, "嘴巴愤怒");

        EditorGUILayout.Slider(blink, 0, 1, "眨眼");
        //EditorGUILayout.Slider(angry, 0, 1, "愤怒");
        //EditorGUILayout.Slider(furious, 0, 1, "大怒");
        //EditorGUILayout.Slider(giggle, 0, 1, "大笑");
        //EditorGUILayout.Slider(happy, 0, 1, "开心");
        //EditorGUILayout.Slider(low, 0, 1, "伤心");
        //EditorGUILayout.Slider(laugh, 0, 1, "笑");
        //EditorGUILayout.Slider(smile, 0, 1, "微笑");
        //EditorGUILayout.Slider(pain, 0, 1, "悲伤");
        //EditorGUILayout.Slider(serious, 0, 1, "严肃");
        //EditorGUILayout.Slider(surprise, 0, 1, "惊讶");
        //EditorGUILayout.Slider(getangry, 0, 1, "生气");
        //EditorGUILayout.Slider(normal, 0, 1, "眨眼");
        //EditorGUILayout.Slider(teether, 0, 1, "咬牙裂齿");
        //EditorGUILayout.Slider(afraid, 0, 1, "害怕");
        //EditorGUILayout.Slider(nervous, 0, 1, "紧张");
        //EditorGUILayout.Slider(sad, 0, 1, "难过");

        serializedObject.ApplyModifiedProperties();
    }
}
