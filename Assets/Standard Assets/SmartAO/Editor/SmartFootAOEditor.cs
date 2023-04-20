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
        GUILayout.Label("模型需求：\r\n1）角色模型左右脚需沿着X轴向对齐\r\n2）角色需要垂直于地面\r\n3）角色原点在脚底");
        EditorDebug = EditorGUILayout.Toggle("设置初始资源", EditorDebug);
        if (EditorDebug)
        {
            if (GUILayout.Button("创建共享BoxMesh"))
            {
                SmartFootAOEditorCamera.CreateShareBoxMesh(SmartFootAO.FootAOBoxMeshPath);
            }
            if (GUILayout.Button("创建共享PlaneMesh"))
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
        if (GUILayout.Button("创建脚部AO"))
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
        if (GUILayout.Button("关闭足部AO"))
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

    //----------------------------------SmartAO需要调用的接口------------------------------------
    public static void MakeAO(GameObject prefab, string roleName, bool chearCache = false)
    {
        List<SkinnedMeshRenderer> rs = new List<SkinnedMeshRenderer>();
        GameObject obj = GameObject.Instantiate<GameObject>(prefab);
        FindSkinedMeshRender(obj.transform, rs);
        SmartFootAO creater = obj.AddComponent<SmartFootAO>();
        //这个设置一个角色只需要设置一次
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

    [MenuItem("Tools/引擎/测试SmartAO")]
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
