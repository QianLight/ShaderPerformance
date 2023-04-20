using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(CFEngine.GrassManager))]
public class GrassManagerEditor : Editor
{
    private CFEngine.GrassManager mgr = null;

    private void OnEnable()
    {
        mgr = target as CFEngine.GrassManager;
    }
    private void OnDisable()
    {
        mgr = null;
    }
    public override void OnInspectorGUI()
    {
        if (GUILayout.Button("Search grass"))
        {
            mgr.SearchGrass1();
        }
        base.OnInspectorGUI();
    }
}
