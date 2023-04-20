#if UNITY_EDITOR
using Impostors;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace CFEngine.Editor
{
    public class HouseLodEditorComponetContext
    {
        public static List<GameObject> houseListInScene = new List<GameObject>();
    }
    public class HouseLodEditorComponet<T> : EditorComponet<T> 
        where T: EditorComponet<T>, new()
    {
        protected List<GameObject> houseListInScene = HouseLodEditorComponetContext.houseListInScene;
        public override void Init() { }

        public override void DrawGUI() { }

        public override void Destroy() { }

        public override string Name() { return "HouseLodEditorComponet"; }
        
        public void HouseLodDataField(GameObject data)
        {
            if(GUILayout.Button(data.name,EditorStyles.objectField))
                PropertyEditor.OpenPropertyEditor(data);
        }
    }

    public class HouseLodData : HouseLodEditorComponet<HouseLodData>
    {       
        private string _searchKeyword = "";
        private Vector2 _scrollPosition;
        private float dist0 = 0.4f;
        private float dist1 = 0.3f;
        private float dist2 = 0.2f;
        public override void Init()
        {
            //GetHouseGameObjects();
        }

        public override void DrawGUI()
        {
            GUILayout.BeginHorizontal();
            if (GUILayout.Button(">>>>>>>>>>>>根据父节点搜索<<<<<<<<<<<<<"))
                GetHouseGameObjects();
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label("Search");
            _searchKeyword = EditorGUILayout.TextField(_searchKeyword,EditorStyles.toolbarSearchField);
            if (GUILayout.Button("clean",GUILayout.Width(50)))
                _searchKeyword = "";
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("重组建筑结构", GUILayout.Width(205)))
                RebuildHouseConstruct();
            GUILayout.BeginVertical();
            if (GUILayout.Button("还原建筑结构", GUILayout.Width(205)))
                RecoverHouseConstruct();
            GUILayout.EndVertical();
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            dist0 = EditorGUILayout.Slider("全局LOD0:", dist0, 0, 1, GUILayout.Width(300));
            GUILayout.BeginVertical();
            if (GUILayout.Button("赋值", GUILayout.Width(50)))
            {
                SetScreenTransation(0);
            }
            GUILayout.EndVertical();
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            dist1 = EditorGUILayout.Slider("全局LOD1:", dist1, 0, 1, GUILayout.Width(300));
            GUILayout.BeginVertical();
            if (GUILayout.Button("赋值", GUILayout.Width(50)))
            {
                SetScreenTransation(1);
            }
            GUILayout.EndVertical();
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            dist2 = EditorGUILayout.Slider("全局LOD2:", dist2, 0, 1, GUILayout.Width(300));
            GUILayout.BeginVertical();
            if (GUILayout.Button("赋值", GUILayout.Width(50)))
            {
                SetScreenTransation(2);
            }
            GUILayout.EndVertical();
            GUILayout.EndHorizontal();

            int count;

            _scrollPosition = GUILayout.BeginScrollView(_scrollPosition,GUILayout.Width(0),GUILayout.Height(0));
            if (_searchKeyword.Equals(""))
            {
                count = 0;
                foreach (var oneHouse in houseListInScene)
                {
                    HouseLodDataField(oneHouse);
                    count++;
                }
            }
            else
            {
                count = 0;
                foreach (var oneHouse in houseListInScene)
                {
                    if (oneHouse.name.ToLower().Contains(_searchKeyword.ToLower()))
                    {
                        HouseLodDataField(oneHouse);
                        count++;
                    }
                }
            }
            GUILayout.EndScrollView();
            GUILayout.FlexibleSpace();
            GUILayout.Label("Houses Count: " + count);
        }

        public override void Destroy()
        {
            houseListInScene.Clear();
        }

        public override string Name()
        {
            return "House Lod Data";
        }

        void GetHouseGameObjects()
        {
            houseListInScene.Clear();
            GameObject tempHouse = Selection.activeGameObject;
            if (!tempHouse)
            {
                Debug.LogError("没有选择有效的父节点!");
                return;
            }

            GameObject houses = tempHouse;
            for (int i = 0; i < houses.transform.childCount; i++)
            {
                if (houses.transform.GetChild(i).name == "tree" || houses.transform.GetChild(i).name == "guanmu")
                    Debug.LogError("这是树，不是建筑！");
                else if (!houses.transform.GetChild(i).GetComponent<LODGroup>())
                    Debug.LogError("此节点的第" + i + "个子物体没有LODGroup");
                else
                    houseListInScene.Add(houses.transform.GetChild(i).gameObject);
            }

            //GameObject tempHead = GameObject.Find("StaticPrefabs");
            //if (!tempHead)
            //    return;

            //GameObject head = tempHead;
            //LODGroup[] groups = head.transform.GetComponentsInChildren<LODGroup>();
            //foreach (LODGroup group in groups)
            //{
            //    if (group.transform.parent.name != "tree" && group.transform.parent.name != "guanmu")
            //        houseListInScene.Add(group.gameObject);
            //}
        }

        void RebuildHouseConstruct()
        {
            for(int i = houseListInScene.Count - 1; i >= 0; i--)
            {
                if (houseListInScene[i].name.Contains("_lodgrp"))
                    continue;

                GameObject tempScource = PrefabUtility.GetCorrespondingObjectFromOriginalSource(houseListInScene[i]);
                if (tempScource == null)
                    continue;

                string oriName = tempScource.name;
                int lodCount = GetLODsCount(oriName);

                LOD[] lods = new LOD[lodCount];      
                string[] paths = new string[lodCount];

                for (int j = 0; j < lodCount; j++)
                {
                    string tarName = oriName;
                    if (j == 1)
                    {
                        tarName = oriName + "_LOD1";
                    }
                    else if (j == 2)
                    {
                        tarName = oriName + "_LOD2";
                    }
                    paths[j] = AssetDatabase.FindAssets(string.Format(tarName + " t: {0}", "Model"), new string[] { "Assets/Scenes" })[0];
                    paths[j] = AssetDatabase.GUIDToAssetPath(paths[j]);
                }

                if (houseListInScene[i].GetComponent<LODGroup>() == null)
                    continue;

                if(houseListInScene[i].GetComponent<LODGroup>().GetLODs().Length > 0)
                {
                    lods[0] = houseListInScene[i].GetComponent<LODGroup>().GetLODs()[0];
                    lods[0].screenRelativeTransitionHeight = 0.8f;
                }

                Transform root = houseListInScene[i].transform.parent;
                GameObject head = new GameObject();
                head.transform.parent = root;
                head.name = houseListInScene[i].name + "_lodgrp";
                AddAndDeleteCompt(houseListInScene[i], head);
                houseListInScene[i].transform.parent = head.transform;
                houseListInScene[i] = head;
                Vector3 oriPos = head.transform.GetChild(0).localPosition;
                Quaternion oriRot = head.transform.GetChild(0).localRotation;
                Vector3 oriScl = head.transform.GetChild(0).localScale;

                for (int j = 1; j < paths.Length; j++)
                {
                    GameObject go = AssetDatabase.LoadAssetAtPath<GameObject>(paths[j]);
                    GameObject lodObj = PrefabUtility.InstantiatePrefab(go) as GameObject;
                    lodObj.transform.parent = head.transform;
                    ResetTransform(lodObj, oriPos, oriRot, oriScl);

                    int rendererCnt = (lodObj.transform.childCount == 0) ? 1 : lodObj.transform.childCount;
                    Renderer[] renderers = new Renderer[rendererCnt];
                    for(int k = 0; k < rendererCnt; k++)
                    {
                        renderers[k] = (lodObj.transform.childCount == 0) ? lodObj.transform.GetComponent<Renderer>() : lodObj.transform.GetChild(k).GetComponent<Renderer>();
                    }
                    lods[j] = new LOD(1.0f / (1 + j), renderers);
                }

                if (head.GetComponent<LODGroup>() != null)
                    head.GetComponent<LODGroup>().SetLODs(lods);
            }
        }
        
        void RecoverHouseConstruct()
        {
            for(int i = houseListInScene.Count - 1; i >= 0; i--)
            {
                if (!houseListInScene[i].name.Contains("_lodgrp"))
                    continue;

                LOD[] lods = new LOD[1];
                lods[0] = houseListInScene[i].GetComponent<LODGroup>().GetLODs()[0];
                lods[0].screenRelativeTransitionHeight = houseListInScene[i].GetComponent<LODGroup>().GetLODs()[houseListInScene[i].GetComponent<LODGroup>().GetLODs().Length - 1].screenRelativeTransitionHeight;
                Transform root = houseListInScene[i].transform.parent;
                GameObject oriHouse = houseListInScene[i].transform.GetChild(0).gameObject;
                oriHouse.transform.parent = root;
                AddAndDeleteCompt(houseListInScene[i], oriHouse);
                oriHouse.GetComponent<LODGroup>().SetLODs(lods);
                Object.DestroyImmediate(houseListInScene[i]);
                houseListInScene[i] = oriHouse;
            }
        }

        private int GetLODsCount(string oriName)
        {
            int count = 1;

            for (int i = 0; i < 2; i++)
            {
                string tarName = oriName;
                if (i == 0)
                {
                    tarName = oriName + "_LOD1";
                }
                else if (i == 1)
                {
                    tarName = oriName + "_LOD2";
                }

                if (AssetDatabase.FindAssets(string.Format(tarName + " t: {0}", "Model"), new string[] { "Assets/Scenes" }).Length > 0)
                {
                    count += 1;
                }
            }

            return count;
        }

        private void AddAndDeleteCompt(GameObject oldObj, GameObject newObj)
        {
            UnityEditorInternal.ComponentUtility.CopyComponent(oldObj.GetComponent<LODGroup>());
            UnityEditorInternal.ComponentUtility.PasteComponentAsNew(newObj);
            UnityEditorInternal.ComponentUtility.CopyComponent(oldObj.GetComponent<ImpostorLODGroup>());
            UnityEditorInternal.ComponentUtility.PasteComponentAsNew(newObj);
            Object.DestroyImmediate(oldObj.GetComponent<ImpostorLODGroup>());
            Object.DestroyImmediate(oldObj.GetComponent<LODGroup>());
        }

        void SetScreenTransation(int lodLevel)
        {
            foreach (GameObject obj in houseListInScene)
            {
                LOD[] lods = obj.GetComponent<LODGroup>().GetLODs();
                if (lods.Length == 3)
                {
                    if (lodLevel == 0)
                    {
                        lods[lodLevel] = new LOD(dist0, obj.GetComponent<LODGroup>().GetLODs()[lodLevel].renderers);
                    }
                    else if (lodLevel == 1)
                    {
                        lods[lodLevel] = new LOD(dist1, obj.GetComponent<LODGroup>().GetLODs()[lodLevel].renderers);
                    }
                    else if (lodLevel == 2)
                    {
                        lods[lodLevel] = new LOD(dist2, obj.GetComponent<LODGroup>().GetLODs()[lodLevel].renderers);
                    }
                }
                else if (lods.Length == 2)
                {
                    if (lodLevel == 1)
                    {
                        lods[0] = new LOD(dist1, obj.GetComponent<LODGroup>().GetLODs()[0].renderers);
                    }
                    else if (lodLevel == 2)
                    {
                        lods[1] = new LOD(dist2, obj.GetComponent<LODGroup>().GetLODs()[1].renderers);
                    }
                }
                else if (lods.Length == 1)
                {
                    if (lodLevel == 2)
                    {
                        lods[0] = new LOD(dist2, obj.GetComponent<LODGroup>().GetLODs()[0].renderers);
                    }
                }
                obj.GetComponent<LODGroup>().SetLODs(lods);
                obj.GetComponent<ImpostorLODGroup>().UpdateImpostorLODSetting();
            }
        }

        private void ResetTransform(GameObject tarGo, Vector3 pos, Quaternion rot, Vector3 scl)
        {
            tarGo.transform.localPosition = pos;
            tarGo.transform.localRotation = rot;
            tarGo.transform.localScale = scl;
        }
    }
}

#endif
