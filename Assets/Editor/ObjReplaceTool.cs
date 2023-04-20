using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace SceneTools
{
    public class ObjReplaceTool : EditorWindow
    {
        private GameObject replaceObj;
        private GameObject rootObj;
        private GameObject orginRootObj;
        private string suffix = String.Empty; //后缀
        private string prefix = string.Empty; //前缀
        private bool isHideOrginObj = true;

        [MenuItem("Tools/场景/物体批量替换工具")]
        public static void OpenWindow()
        {
            GetWindow<ObjReplaceTool>().Show();
        }

        private void OnGUI()
        {
            replaceObj = EditorGUILayout.ObjectField("要替换的物体", replaceObj, typeof(GameObject), true) as GameObject;
            rootObj = EditorGUILayout.ObjectField("生成物体的根节点位置", rootObj, typeof(GameObject), true) as GameObject;
            orginRootObj = EditorGUILayout.ObjectField("老物体移回到的节点", orginRootObj, typeof(GameObject), true) as GameObject;
            suffix = EditorGUILayout.TextField("后缀", suffix);
            prefix = EditorGUILayout.TextField("前缀", prefix);
            isHideOrginObj = EditorGUILayout.Toggle("隐藏原始物体", isHideOrginObj);

            if (GUILayout.Button("替换"))
            {
                if (replaceObj == null)
                {
                    Debug.Log("请放入需要替换的GameObject");
                    return;
                }

                Transform[] selectTrans = Selection.transforms;
                Transform tempTrans;
                GameObject tempObj;
                
                GameObject originPrefab = PrefabUtility.GetCorrespondingObjectFromOriginalSource(replaceObj);
                PropertyModification[] propertyModifications = PrefabUtility.GetPropertyModifications(replaceObj);
                for (int i = 0; i < selectTrans.Length; i++)
                {
                    tempTrans = selectTrans[i];
                    
                    tempObj = PrefabUtility.InstantiatePrefab(originPrefab) as GameObject;
                    PrefabUtility.SetPropertyModifications(tempObj, propertyModifications);
                    if (rootObj != null)
                    {
                        if (tempObj != null) tempObj.transform.parent = rootObj.transform;
                    }

                    if (tempObj != null)
                    {
                        tempObj.name = prefix + replaceObj.gameObject.name + suffix;
                        tempObj.transform.position = tempTrans.position;
                        tempObj.transform.rotation = tempTrans.rotation;
                        tempObj.transform.localScale = tempTrans.localScale;
                    }

                    if (isHideOrginObj)
                    {
                        tempTrans.gameObject.SetActive(false);
                    }

                    if (orginRootObj != null)
                    {
                        selectTrans[i].parent = orginRootObj.transform;
                    }
                }
            }
        }
    }
}