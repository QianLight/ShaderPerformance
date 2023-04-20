using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;


[CustomEditor(typeof(SmartFootAO), true)]
public class SmartFootAOEditor : Editor
{

    SmartFootAO targetObj;
    private bool EditorDebug = false;
    public override void OnInspectorGUI()
    {
        serializedObject.UpdateIfRequiredOrScript();

        targetObj = target as SmartFootAO;
        GUILayout.Label("ģ������\r\n1����ɫģ�����ҽ�������X�������\r\n2����ɫ��Ҫ��ֱ�ڵ���\r\n3����ɫԭ���ڽŵ�");
        EditorDebug = EditorGUILayout.Toggle("���ó�ʼ��Դ", EditorDebug);
        if (EditorDebug)
        {
            if (GUILayout.Button("��������BoxMesh"))
            {
                SmartFootAOEditorCamera.CreateShareBoxMesh(SmartFootAO.FootAOBoxMeshPath);
            }
            if (GUILayout.Button("��������PlaneMesh"))
            {
                SmartFootAOEditorCamera.CreateSharePlaneMesh(SmartFootAO.FootAOPlaneMeshPath);
            }
        }

        EditorGUILayout.PropertyField(serializedObject.FindProperty("ShowDebug"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("UseLastPrefab"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("RightUseLeft"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("AutoGetRender"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("PartName"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("SlicePPM"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("FootHeight"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("FootAdjust"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("AddScale"));

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.PropertyField(serializedObject.FindProperty("LeftFootRender"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("RightFootRender"));
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.PropertyField(serializedObject.FindProperty("Left3D"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("Right3D"));
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.PropertyField(serializedObject.FindProperty("LeftFootShadow"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("RightFootShadow"));
        EditorGUILayout.EndHorizontal();
        if (GUILayout.Button("�����Ų�AO"))
        {
            //myScript.SaveAsset();
            Vector3 pos = targetObj.transform.position;
            Vector3 scale = targetObj.transform.lossyScale;
            Quaternion rotation = targetObj.transform.rotation;
            try
            {
                targetObj.transform.position = Vector3.zero;
                targetObj.transform.localScale = Vector3.one;
                targetObj.transform.rotation = Quaternion.identity;
                targetObj.CreateSmartAO();
            }
            catch (System.Exception ex)
            {

            }
            finally
            {
                targetObj.transform.position = pos;
                targetObj.transform.localScale = scale;
                targetObj.transform.rotation = rotation;
            }
        }
        if (GUILayout.Button("�ر��㲿AO"))
        {
            targetObj.ShowFootShadow();
        }

        serializedObject.ApplyModifiedProperties();
    }
    private static void FindSkinedMeshRender(Transform root, List<SkinnedMeshRenderer> rs)
    {
        foreach (Transform t in root)
        {
            FindSkinedMeshRender(t, rs);
        }
        SkinnedMeshRenderer sk = root.GetComponent<SkinnedMeshRenderer>();
        if (sk != null && !sk.enabled)
        {
            sk.enabled = true;
            rs.Add(sk);
        }
    }

    //----------------------------------SmartAO��Ҫ���õĽӿ�------------------------------------
    public static void MakeAO(GameObject prefab, string roleName, bool chearCache = false)
    {
        List<SkinnedMeshRenderer> rs = new List<SkinnedMeshRenderer>();
        GameObject obj = GameObject.Instantiate<GameObject>(prefab);
        FindSkinedMeshRender(obj.transform, rs);
        SmartFootAO creater = obj.AddComponent<SmartFootAO>();
        //�������һ����ɫֻ��Ҫ����һ��
        if(chearCache)
        {
            creater.ClearLastFootAO();
        }
        creater.CreateSmartFootAO(roleName);
        foreach(SkinnedMeshRenderer sk in rs)
        {
            sk.enabled = false;
        }
        GameObject.DestroyImmediate(creater);
        UnityEditor.PrefabUtility.SaveAsPrefabAsset(obj, AssetDatabase.GetAssetPath(prefab));
        GameObject.DestroyImmediate(obj);
    }

    [MenuItem("Tools/����/����SmartAO")]
    private static void CreateSmartAOTest()
    {
        GameObject obj = Selection.activeObject as GameObject;
        if(obj != null)
        {
            MakeAO(obj, "luffy", true);
            //MakeAO(lod1, "luffy", false);
            //MakeAO(lod2, "luffy", false);
        }
    }
}
