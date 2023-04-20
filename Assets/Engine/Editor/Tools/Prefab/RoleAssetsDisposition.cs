#if UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.EditorCoroutines.Editor;
using UnityEditor;
using UnityEngine;

namespace CFEngine.Editor
{
    public class RoleAssetsDisposition :RoleEditorComponet<RoleAssetsDisposition>
    {
        // public List<GameObject> timelinePrefabs = new List<GameObject>();
        public List<string> usedRoleNames;
        private static bool _allPrefabhere;

        private Vector2 _scrollPosition;

        public override void Init()
        {
            base.Init();
            _allPrefabhere = false;
        }

        public override void Destroy()
        {
            base.Destroy();
            _allPrefabhere = false;
            // timelinePrefabs.Clear();
        }

        public override string Name()
        {
            return "Role Assets Disposition";
        }

        public override void DrawGUI()
        {
            if (!_allPrefabhere)
            {
                if (GUILayout.Button("Find Prefab used in game"))
                {
                    usedRoleNames = SyncGetNameOfPrefabsInGame();
                }
                GUILayout.Label(RoleEditorComponetContext.label);
                _editorWindow.Repaint();
                return;
            }
            else
            {
                _scrollPosition = GUILayout.BeginScrollView(_scrollPosition,GUILayout.Width(0),GUILayout.Height(0));
                // foreach (var prefab in timelinePrefabs)
                // {
                //     EditorGUILayout.ObjectField(prefab, typeof(GameObject));
                // }

                foreach (var name in usedRoleNames)
                {
                    GUILayout.Label(name);
                }
                GUILayout.EndScrollView();
                GUILayout.Label($"Count: {usedRoleNames.Count.ToString()}");
            }
        }
        
        /// <summary>
        /// 获取 Timeline / 战斗 策划配表中的角色 Prefab 名字。
        /// </summary>
        /// <returns></returns>
        public static List<string> SyncGetNameOfPrefabsInGame()
        {
            List<string> roleNames = new List<string>();
            SyncGetPrefab(roleNames);
            GetEntityPresentation(roleNames);
            roleNames.Sort();
            return roleNames;
        }
        
        private List<string> AsyncGetNameOfPrefabsInGame()
        {
            List<string> roleNames = new List<string>();
            _editorWindow.StartCoroutine(AsyncGetPrefab(roleNames));
            GetEntityPresentation(roleNames);
            roleNames.Sort();
            return roleNames;
        }

        IEnumerator AsyncGetPrefab(List<string> roleNames)
        {
            _allPrefabhere = false;
            int process = 0;
            string[] paths = AssetDatabase.FindAssets(string.Format("t:{0}","Prefab"),new []{"Assets/BundleRes/Timeline/"});
            yield return 0;
            for (int i = 0; i < paths.Length; i++)
            {
                paths[i] = AssetDatabase.GUIDToAssetPath(paths[i]);
                // Debug.Log(paths[i]);
                RoleEditorComponetContext.label = paths[i];
            }

            foreach (var path in paths)
            {
                GameObject data = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                OrignalTimelineData otd = data.GetComponentInChildren<OrignalTimelineData>();
                if (data && otd)
                {
                    OrignalChar[] chars = otd.chars;
                    foreach (var c in chars)
                    {
                        if (!roleNames.Contains(c.prefab.ToLower()))
                        {
                            roleNames.Add(c.prefab.ToLower());
                        }
                    }
                    
                    // timelinePrefabs.Add(data);
                    RoleEditorComponetContext.label = data.name;
                    process++;
                    if (process % 12 == 0)
                        yield return 0;
                }
            }
            _allPrefabhere = true;
            RoleEditorComponetContext.label = "";
        }

        static void SyncGetPrefab(List<string> roleNames)
        {
            _allPrefabhere = false;

            string[] paths = AssetDatabase.FindAssets(string.Format("t:{0}","Prefab"),new []{"Assets/BundleRes/Timeline/"});

            for (int i = 0; i < paths.Length; i++)
            {
                paths[i] = AssetDatabase.GUIDToAssetPath(paths[i]);
                // Debug.Log(paths[i]);
                RoleEditorComponetContext.label = paths[i];
            }

            foreach (var path in paths)
            {
                GameObject data = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                OrignalTimelineData otd = data.GetComponentInChildren<OrignalTimelineData>();
                if (data && otd)
                {
                    OrignalChar[] chars = otd.chars;
                    foreach (var c in chars)
                    {
                        if (!roleNames.Contains(c.prefab.ToLower()))
                        {
                            roleNames.Add(c.prefab.ToLower());
                        }
                    }
                    
                    // timelinePrefabs.Add(data);
                    RoleEditorComponetContext.label = data.name;
                }
            }
            _allPrefabhere = true;
            RoleEditorComponetContext.label = "";
        }
        static void GetEntityPresentation(List<string> roleNames)
        {
            _allPrefabhere = false;
            uint[] ids;
            string[] prefabs;
            XEntityPresentationReader.GetAllEntities(out ids, out prefabs);
            foreach (var prefab in prefabs)
            {
                if (!roleNames.Contains(prefab.ToLower()))
                {
                    roleNames.Add(prefab.ToLower());
                }
            }
            _allPrefabhere = true;
        }
    }
}
#endif