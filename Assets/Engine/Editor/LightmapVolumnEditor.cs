using System.Collections;
using System.Collections.Generic;
using CFEngine;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

[CustomEditor(typeof(LightmapVolumn))]
public class LightmapVolumnEditor : Editor
{
    private List<MeshRenderer> bakeMeshRenderers;
    private GameObject[] allGameObjects;
    private List<GameObject> staticGameObjects;
    public GameObject staticPrefabs;

    

    // public Bloom setOFF;
    // public Bloom setOn;
    //
    private SerializedProperty res;
    private SerializedProperty renders;
    private SerializedProperty probes;
    private SerializedProperty volumnName;
    private SerializedProperty configName;
    private SerializedProperty dataIndex;
    private SerializedProperty chunkIndex;
    private SerializedProperty bALoadBySceneManager;


    private void OnEnable()
    {
        res = serializedObject.FindProperty("res");
        renders = serializedObject.FindProperty("renders");
        probes = serializedObject.FindProperty("probes");
        volumnName = serializedObject.FindProperty("volumnName");
        configName = serializedObject.FindProperty("configName");
        dataIndex = serializedObject.FindProperty("dataIndex");
        chunkIndex = serializedObject.FindProperty("chunkIndex");
        bALoadBySceneManager = serializedObject.FindProperty("bALoadBySceneManager");

    }
    public override void OnInspectorGUI()
    {
        EditorGUILayout.PropertyField(res);
        EditorGUILayout.PropertyField(renders);
        EditorGUILayout.PropertyField(probes);
        EditorGUILayout.PropertyField(volumnName);
        EditorGUILayout.PropertyField(configName);
        EditorGUILayout.PropertyField(dataIndex);
        EditorGUILayout.PropertyField(chunkIndex);
        EditorGUILayout.PropertyField(bALoadBySceneManager);

        
        if (GUILayout.Button("设置投影 OFF"))
        {
            GetbakeMeshRenderers();
            SetCastShadowsOFF();
        }
        if (GUILayout.Button("设置投影 On"))
        {
            GetbakeMeshRenderers();
            SetCastShadowsOn();
        }
    }
    private void GetbakeMeshRenderers()
    {
        bakeMeshRenderers = new List<MeshRenderer>();
        staticGameObjects = new List<GameObject>();
        
        
        allGameObjects = FindObjectsOfType(typeof(GameObject)) as GameObject[];

        for (int i = 0; allGameObjects.Length > i; i++)
        {
            if (allGameObjects[i].isStatic)
            {
                staticGameObjects.Add(allGameObjects[i]);
            }
        }
        for (int i = 0; i < staticGameObjects.Count; i++)
        {
            if (staticGameObjects[i].TryGetComponent(out MeshRenderer bakeRender))
            {
                bakeMeshRenderers.Add(bakeRender);
            }
        }
    }

    private void SetCastShadowsOFF()
    {
        if (bakeMeshRenderers != null)
        {
            foreach (var bakeRender in bakeMeshRenderers)
            {
                bakeRender.shadowCastingMode = ShadowCastingMode.Off;
            }
        }
    }
    private void SetCastShadowsOn()
    {
        if (bakeMeshRenderers != null)
        {
            foreach (var bakeRender in bakeMeshRenderers)
            {
                bakeRender.shadowCastingMode = ShadowCastingMode.On;
            }
        }
    }
}
