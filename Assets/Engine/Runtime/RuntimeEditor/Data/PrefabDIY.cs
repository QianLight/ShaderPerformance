#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngineEditor = UnityEditor.Editor;

namespace CFEngine
{
    [System.Serializable]
    public class DiyPrefab
    {
        public GameObject go;
        public bool folder;
        public List<string> partNames = new List<string>();
    }

    [DisallowMultipleComponent, ExecuteInEditMode]
    public class PrefabDIY : MonoBehaviour
    {
        public GameObject template;
        public List<DiyPrefab> prefabs = new List<DiyPrefab>();

        [System.NonSerialized]
        public XGameObject xgo;

        public void Refresh()
        {
            if (template != null)
            {
                string prefabName = template.name;
                if (xgo != null)
                {
                    XGameObject.DestroyXGameObject(xgo);
                }
                ref var cc = ref GameObjectCreateContext.createContext;
                cc.Reset();
                cc.name = prefabName;
                cc.flag.SetFlag(GameObjectCreateContext.Flag_SetPrefabName | GameObjectCreateContext.Flag_NotSyncPos);
                cc.immediate = true;
                xgo = XGameObject.CreateXGameObject(ref cc);
                xgo.SetDebugName(prefabName);

                var pc = PrefabConfig.singleton;
                for (int i = 0; i < prefabs.Count; ++i)
                {
                    var prefab = prefabs[i];
                    if (prefab.go != null)
                        pc.AddParts(prefab.go.name);
                }
            }
        }

        public void SetPart(string name, int index, bool destroy)
        {
            if (xgo != null)
            {
                xgo.SetPart(name, index, destroy);
            }
        }

        public void SetAllPart(int diyIndex)
        {
            if (template != null)
            {
                var diy = prefabs[diyIndex];
                string prefabName = template.name;
                if (xgo != null)
                {
                    XGameObject.DestroyXGameObject(xgo);
                }
                ref var cc = ref GameObjectCreateContext.createContext;
                cc.Reset();
                cc.name = prefabName;
                cc.flag.SetFlag(GameObjectCreateContext.Flag_SetPrefabName | GameObjectCreateContext.Flag_NotSyncPos);
                cc.immediate = true;
                xgo = XGameObject.CreateXGameObject(ref cc);
                xgo.InitPart();
                for (int i = 0; i < diy.partNames.Count; ++i)
                {
                    xgo.SetPartIndex(diy.partNames[i], i);
                }
                xgo.EndLoad(ref cc);
                xgo.SetDebugName(prefabName);
            }
        }
    }

    [CustomEditor(typeof(PrefabDIY))]
    public class PrefabDIYEditor : UnityEngineEditor
    {
        SerializedProperty template;
        SerializedProperty prefabs;
        private void OnEnable()
        {
            template = serializedObject.FindProperty("template");
            prefabs = serializedObject.FindProperty("prefabs");
        }

        public override void OnInspectorGUI ()
        {
            serializedObject.Update ();
            var diy = target as PrefabDIY;
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PropertyField(template);
            if (GUILayout.Button("Refresh",GUILayout.Width(100)))
            {
                diy.Refresh();
            }
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Add", GUILayout.Width(100)))
            {
                prefabs.arraySize++;
            }
            EditorGUILayout.LabelField(string.Format("{0}", diy.xgo != null ? "Ready" : "NotReady"));
            EditorGUILayout.EndHorizontal();
            float w = EditorGUIUtility.currentViewWidth;
            int deleteIndex = -1;
            for (int i = 0; i < prefabs.arraySize; ++i)
            {
                SerializedProperty prefab = prefabs.GetArrayElementAtIndex(i);
                var folder = prefab.FindPropertyRelative("folder");
                var goSp = prefab.FindPropertyRelative("go");
                bool isFolder = folder.boolValue;
                GameObject go = goSp.objectReferenceValue as GameObject;
                EditorGUILayout.BeginHorizontal();
                isFolder = EditorGUILayout.Foldout(isFolder, go != null ? go.name : "");
                if (GUILayout.Button("Delete", GUILayout.Width(100)))
                {
                    deleteIndex = i;
                }
                EditorGUILayout.EndHorizontal();
                if (isFolder)
                {
                    var partNames = prefab.FindPropertyRelative("partNames");
                    EditorCommon.BeginGroup("", true, w - 5);
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.PropertyField(goSp, GUIContent.none);
                    if (GUILayout.Button("Refresh", GUILayout.Width(80)))
                    {
                        var t = go.transform;
                        partNames.ClearArray();
                        for (int j = 0; j < t.childCount; ++j)
                        {
                            var c = t.GetChild(j);
                            if(c.TryGetComponent<MeshFilter>(out var mf))
                            {
                                if (mf.sharedMesh != null)
                                {
                                    partNames.InsertArrayElementAtIndex(partNames.arraySize);
                                    SerializedProperty partName = partNames.GetArrayElementAtIndex(j);
                                    partName.stringValue = string.Format("{0}_{1}", go.name, j.ToString());
                                }

                            }

                        }
                    }
                    if (GUILayout.Button("SetAll", GUILayout.Width(80)))
                    {
                        diy.SetAllPart(i);
                    }
                    EditorGUILayout.EndHorizontal();

                    EditorGUI.indentLevel++;
                    for (int j = 0; j < partNames.arraySize; ++j)
                    {
                        SerializedProperty partName = partNames.GetArrayElementAtIndex(j);
                        EditorGUILayout.BeginHorizontal();
                        EditorGUILayout.LabelField(string.Format("Part_{0}:{1}", j.ToString(), partName.stringValue));
                        if (GUILayout.Button("Set", GUILayout.Width(80)))
                        {
                            diy.SetPart(partName.stringValue, j, false);
                        }
                        if (GUILayout.Button("Remove", GUILayout.Width(80)))
                        {
                            diy.SetPart(partName.stringValue, j, true);
                        }
                        EditorGUILayout.EndHorizontal();
                    }
                    EditorGUI.indentLevel--;
                    EditorCommon.EndGroup();

                }
                folder.boolValue = isFolder;
            }
            if(deleteIndex!=-1)
            {
                prefabs.DeleteArrayElementAtIndex(deleteIndex);
            }
            serializedObject.ApplyModifiedProperties();
        }
    }
}
#endif