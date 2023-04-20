using System.Collections.Generic;
using CFEngine;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

[CustomEditor(typeof(CameraBlockGroup))]
public class CameraBlockGroupEditor : Editor
{
    private bool isFoldout = true; 
    public override void OnInspectorGUI()
    {
        void CreateColliders(CameraBlockGroup cameraBlockGroup)
        {
            string baseName = cameraBlockGroup.name + CameraAvoidBlock.suffix;
            for (int i = 0; i < cameraBlockGroup.renderers.Count; i++)
            {
                string colliderName = baseName + i;
                Renderer renderer = cameraBlockGroup.renderers[i];
                BoxCollider bc = renderer.gameObject.AddComponent<BoxCollider>();
                ComponentUtility.CopyComponent(bc);
                GameObject colliderGo = new GameObject(colliderName, typeof(BoxCollider));
                colliderGo.layer = CameraAvoidBlock.Layer;
                colliderGo.transform.SetParent(renderer.transform, false);
                BoxCollider collider = colliderGo.GetComponent<BoxCollider>();
                ComponentUtility.PasteComponentValues(collider);
                cameraBlockGroup.colliders.Add(collider);
                DestroyImmediate(bc);
            }
        }

        base.OnInspectorGUI();
        if (!(target is CameraBlockGroup cbg) || !cbg)
            return;

        EditorGUI.BeginChangeCheck();

        if (cbg.isOverrideMiscConfig)
        {
            isFoldout = EditorGUILayout.Foldout(isFoldout, "CameraBlockConfig");
            if (isFoldout)
            {
                cbg.config.enterDelayTime = EditorGUILayout.FloatField("进入延迟时间", cbg.config.enterDelayTime);
                cbg.config.enterLerpTime = EditorGUILayout.FloatField("淡入持续时间", cbg.config.enterLerpTime);
                cbg.config.leaveDelayTime = EditorGUILayout.FloatField("推出延迟时间", cbg.config.leaveDelayTime);
                cbg.config.minTransparency = EditorGUILayout.Slider("透明度",cbg.config.minTransparency,0f,1f);
            }
        }

        if (GUILayout.Button("移动选中物体为子节点"))
        {
            foreach (Object obj in Selection.objects)
            {
                if (obj is GameObject go)
                {
                    go.transform.SetParent(cbg.transform, true);
                }
            }
        }

        if (GUILayout.Button("刷新Renderer列表"))
        {
            cbg.renderers.Clear();
            cbg.GetComponentsInChildren(true, cbg.renderers);
        }

        if (GUILayout.Button("一键创建Collider"))
        {
            List<Transform> destroyList = ListPool<Transform>.Get();

            // list递归速度比函数递归快很多倍。
            List<Transform> recursiveList = ListPool<Transform>.Get();
            recursiveList.Add(cbg.transform);
            while (recursiveList.Count > 0)
            {
                Transform item = recursiveList[recursiveList.Count - 1];
                if (item.name.StartsWith(cbg.gameObject.name + CameraAvoidBlock.suffix))
                    destroyList.Add(item);
                recursiveList.RemoveAt(recursiveList.Count - 1);
                for (int j = 0; j < item.childCount; j++)
                    recursiveList.Add(item.GetChild(j));
            }

            ListPool<Transform>.Release(ref recursiveList);

            if (destroyList.Count > 0)
            {
                int selection = EditorUtility.DisplayDialogComplex("一键创建Collider", "检测到有已经存在的碰撞盒，要如何处理？", "删除原有碰撞盒",
                    "保留原有碰撞盒", "取消");
                if (selection == 0)
                {
                    foreach (Transform transform in destroyList)
                        DestroyImmediate(transform.gameObject);
                    RefreshColliderList(cbg);
                    CreateColliders(cbg);
                }
                else if (selection == 1)
                {
                    CreateColliders(cbg);
                }
            }
            else
            {
                CreateColliders(cbg);
            }

            ListPool<Transform>.Release(ref destroyList);
        }

        if (GUILayout.Button("刷新Collider列表"))
        {
            RefreshColliderList(cbg);
        }

        if (GUILayout.Button("删除选中之外的碰撞盒"))
        {
            HashSet<GameObject> set = new HashSet<GameObject>(Selection.gameObjects);
            for (int i = 0; i < cbg.colliders.Count; i++)
            {
                Collider collider = cbg.colliders[i];
                if (!collider)
                    continue;
                if (!set.Contains(collider.gameObject))
                {
                    DestroyImmediate(collider.gameObject);
                    cbg.colliders.RemoveAt(i--);
                }
            }
        }

        if (GUILayout.Button("隐藏"))
        {
            SetGroupState(cbg, false);
        }

        if (GUILayout.Button("显示"))
        {
            SetGroupState(cbg, true);
        }

        if (EditorGUI.EndChangeCheck())
        {
            EditorUtility.SetDirty(target);
        }
    }

    private static void RefreshColliderList(CameraBlockGroup cbg)
    {
        cbg.colliders.Clear();
        cbg.GetComponentsInChildren(true, cbg.colliders);
    }

    private void SetGroupState(CameraBlockGroup cbg, bool state)
    {
        foreach (Renderer renderer in cbg.renderers)
        {
            if (renderer)
            {
                renderer.enabled = state;
            }
        }
    }
}