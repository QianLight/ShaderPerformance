using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class GameToPrefabHelper : EditorWindow
{
    [MenuItem("Tools/Prefab/GameToPrefab")]
    public static void GameToPrefab()
    {
        var window = CreateWindow<GameToPrefabHelper>();
        window.Show();
    }


    [MenuItem("Tools/Prefab/CheckIsPrefabs")]
    public static void CheckIsPrefabs()
    {
        GameObject[] allSelects = Selection.gameObjects;

        Debug.Log("CheckIsPrefabs:" + allSelects.Length);

        for (int i = 0; i < allSelects.Length; i++)
        {
            GameObject itm = allSelects[i];

            if (!PrefabUtility.IsAnyPrefabInstanceRoot(itm))
            {
                Debug.Log("CheckIsPrefabs False:" + itm, itm);
            }
        }
    }

    private GameObject m_SrcPrefab;

    private Transform m_LastGameObjectRoot;

    private void OnGUI()
    {
        m_SrcPrefab = EditorGUILayout.ObjectField("Ñ¡ÔñPrefab", m_SrcPrefab, typeof(GameObject), true) as GameObject;

        if (m_SrcPrefab == null) return;

        if (GUILayout.Button("Ìæ»»ÎªPrefab"))
        {
            ReplaceToPrefabs();
        }
    }

    private void ReplaceToPrefabs()
    {
        GameObject[] allSelects = Selection.gameObjects;

        for (int i = 0; i < allSelects.Length; i++)
        {
            GameObject itm = allSelects[i];

            if (m_LastGameObjectRoot == null)
            {
                m_LastGameObjectRoot = new GameObject("GPUInstance").transform;
                m_LastGameObjectRoot.transform.parent = itm.transform.parent.parent;
            }

            GameObject newItm = PrefabUtility.InstantiatePrefab(m_SrcPrefab) as GameObject;
            newItm.transform.parent = m_LastGameObjectRoot;
            newItm.name = itm.name;
            newItm.transform.localPosition = itm.transform.localPosition;
            newItm.transform.localRotation = itm.transform.localRotation;
            newItm.transform.localScale = itm.transform.localScale;
            newItm.transform.SetSiblingIndex(itm.transform.GetSiblingIndex());
            //GameObject.DestroyImmediate(itm);
            itm.gameObject.SetActive(false);
        }
    }
}
