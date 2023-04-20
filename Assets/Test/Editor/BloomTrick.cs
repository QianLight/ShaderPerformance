#if UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class BloomTrick : EditorWindow
{
    public bool Limit = true;
    public float Strength = 0.27f;
        
    private static BloomTrick _ins;
    private static readonly int BLOOM_LIMIT = Shader.PropertyToID("_BloomLimit");
    private static readonly int BLOOM_OFFSET_STRENGTH = Shader.PropertyToID("_BloomOffsetStrength");

    [MenuItem("Test/BloomTrick")]
    public static void ShowWindow()
    {
        _ins = GetWindow<BloomTrick>();
        _ins.Show();
    }

    private void OnGUI()
    {
        Limit = EditorGUILayout.Toggle("Limit", Limit);
        Strength = EditorGUILayout.Slider("Offset", Strength, 0, 1);
        Shader.SetGlobalFloat(BLOOM_LIMIT, Limit?1:0);
        Shader.SetGlobalFloat(BLOOM_OFFSET_STRENGTH, Strength);
    }
}
#endif