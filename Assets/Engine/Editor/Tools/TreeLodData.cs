#if UNITY_EDITOR
using System.Collections.Generic;
using Impostors;
using UnityEditor;
using UnityEngine;

namespace CFEngine.Editor
{
    public class TreeLodEditorComponetContext
    {
        public static List<GameObject> treeListInScene = new List<GameObject>();
    }
    public class TreeLodEditorComponet<T> : EditorComponet<T> 
        where T: EditorComponet<T>, new()
    {
        protected List<GameObject> treeListInScene = TreeLodEditorComponetContext.treeListInScene;
        public override void Init() { }

        public override void DrawGUI() { }

        public override void Destroy() { }

        public override string Name() { return "TreeLodEditorComponet"; }
        
        public void TreeLodDataField(GameObject data)
        {
            if(GUILayout.Button(data.name,EditorStyles.objectField))
                PropertyEditor.OpenPropertyEditor(data);
        }
    }
    public class TreeLodData : TreeLodEditorComponet<TreeLodData>
    {
        private bool activeObj = false;
        private bool hideObj = true;
        private string _searchKeyword = "";
        private float dist0 = 0.4f;
        private float dist1 = 0.3f;
        private float dist2 = 0.2f;
        private Vector2 _scrollPosition;
        public override void Init()
        {
            //GetTreeGameObjects();
        }

        public override void DrawGUI()
        {
            GUILayout.BeginHorizontal();
            if (GUILayout.Button(">>>>>>>>>>>>根据父节点搜索<<<<<<<<<<<<<"))
                GetTreeGameObjects();
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
                GUILayout.Label("Search");
                _searchKeyword = EditorGUILayout.TextField(_searchKeyword,EditorStyles.toolbarSearchField);
                if (GUILayout.Button("clean",GUILayout.Width(50)))
                    _searchKeyword = "";
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
                if (GUILayout.Button("CollectLODs", GUILayout.Width(205)))
                    SetTreeLodData();
                GUILayout.BeginVertical();
                    if (GUILayout.Button("ResetLODs", GUILayout.Width(205)))
                        ResetTreeLod();
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

            GUILayout.BeginHorizontal();
            GUILayout.Label("全局FadeMode:");
            GUILayout.BeginVertical();
            if(GUILayout.Button("None", GUILayout.Width(80)))
            {
                SetFadeMode(0);
            }
            GUILayout.EndVertical();
            GUILayout.BeginVertical();
            if (GUILayout.Button("CrossFade", GUILayout.Width(80)))
            {
                SetFadeMode(1);
            }
            GUILayout.EndVertical();
            GUILayout.BeginVertical();
            if (GUILayout.Button("SpeedTree", GUILayout.Width(80)))
            {
                SetFadeMode(2);
            }
            GUILayout.EndVertical();
            GUILayout.EndHorizontal();

            int count;

            _scrollPosition = GUILayout.BeginScrollView(_scrollPosition,GUILayout.Width(0),GUILayout.Height(0));
            if (_searchKeyword.Equals(""))
            {
                count = 0;
                foreach (var oneTree in treeListInScene)
                {
                    TreeLodDataField(oneTree);
                    count++;
                }
            }
            else
            {
                count = 0;
                foreach (var oneTree in treeListInScene)
                {
                    if (oneTree.name.ToLower().Contains(_searchKeyword.ToLower()))
                    {
                        TreeLodDataField(oneTree);
                        count++;
                    }
                }
            }
            GUILayout.EndScrollView();
           
            if (GUILayout.Button(hideObj ? "显示所有子物体" : "隐藏所有子物体"))
            {
                if (hideObj)
                {
                    foreach (GameObject tree in treeListInScene)
                    {
                        for (int i = 0; i < tree.transform.childCount; i++)
                        {
                            if(tree.transform.GetChild(i).hideFlags != HideFlags.None)
                                tree.transform.GetChild(i).hideFlags = HideFlags.None;
                        }
                    }
                    hideObj = false;
                }
                else
                {
                    foreach (GameObject tree in treeListInScene)
                    {
                        if (tree.transform.childCount > 2)
                        {
                            for (int i = 2; i < tree.transform.childCount; i++)
                            {
                                tree.transform.GetChild(i).hideFlags = HideFlags.HideInHierarchy;
                            }
                        }
                    }
                    hideObj = true;
                }
            }

            GUILayout.BeginVertical();
            if (GUILayout.Button(activeObj ? "禁用所有子物体" : "激活所有子物体"))
            {
                if (activeObj)
                {
                    foreach (GameObject tree in treeListInScene)
                    {
                        if(tree.transform.childCount > 2)
                        {
                            for (int i = 2; i < tree.transform.childCount; i++)
                            {
                                tree.transform.GetChild(i).gameObject.SetActive(false);
                            }
                        }                    
                    }
                    activeObj = false;
                }
                else
                {
                    foreach (GameObject tree in treeListInScene)
                    {
                        bool trunkHidden = false;

                        if(tree.transform.childCount > 2)
                        {
                            if (tree.transform.GetChild(1).gameObject.name.Contains("trunk") && tree.transform.GetChild(1).gameObject.activeInHierarchy == false
                                || tree.transform.GetChild(0).gameObject.name.Contains("trunk") && tree.transform.GetChild(0).gameObject.activeInHierarchy == false)
                            {
                                trunkHidden = true;
                            }

                            for (int i = 2; i < tree.transform.childCount; i++)
                            {
                                if (trunkHidden && tree.transform.GetChild(i).gameObject.name.Contains("trunk"))
                                {
                                    ;
                                }
                                else
                                {
                                    tree.transform.GetChild(i).gameObject.SetActive(true);
                                }
                            }
                        }                       
                    }
                    activeObj = true;
                }
            }
            GUILayout.EndVertical();

            GUILayout.FlexibleSpace();
            GUILayout.Label("Trees Count: " + count);
        }

        public override void Destroy()
        {
            treeListInScene.Clear();
        }

        public override string Name()
        {
            return "Tree Lod Data";
        }

        void GetTreeGameObjects()
        {
            treeListInScene.Clear();
            GameObject tempTree = Selection.activeGameObject;
            if (!tempTree)
            {
                Debug.LogError("没有选择有效的父节点!");
                return;
            }

            GameObject trees = tempTree;
            for (int i = 0; i < trees.transform.childCount; i++)
            {
                treeListInScene.Add(trees.transform.GetChild(i).gameObject);
            }
        }

        void SetTreeLodData()
        {
            foreach(GameObject tree in treeListInScene)
            {
                LODGroup group = tree.GetComponent<LODGroup>();
                Material leafMat = tree.transform.GetChild(0).gameObject.name.Contains("leaves") ? tree.transform.GetChild(0).GetComponent<MeshRenderer>().sharedMaterial : tree.transform.GetChild(1).GetComponent<MeshRenderer>().sharedMaterial;
                Material trunkMat = tree.transform.GetChild(1).gameObject.name.Contains("trunk") ? tree.transform.GetChild(1).GetComponent<MeshRenderer>().sharedMaterial : tree.transform.GetChild(0).GetComponent<MeshRenderer>().sharedMaterial;

                string tarName = MatchLODName(tree);

                string[] paths = AssetDatabase.FindAssets(string.Format(tarName + " t: {0}", "Model"), new string[]{ "Assets/Scenes/Tree", "Assets/Scenes/Tree_New" });
                if (tarName.Contains("LOD1"))
                {
                    string[] tempPaths = new string[2];
                    tempPaths[1] = AssetDatabase.FindAssets(string.Format(tarName.Replace("LOD1", "LOD2") + " t: {0}", "Model"), new string[] { "Assets/Scenes/Tree", "Assets/Scenes/Tree_New" })[0];
                    tempPaths[0] = paths[0];
                    paths = tempPaths;         
                }

                /*
                if(paths.Length == 3)
                {
                    string[] tmpP = new string[2];
                    tmpP[0] = paths[0];
                    tmpP[1] = paths[1];
                    paths = tmpP;
                } 
                */

                int a = paths.Length;
                if (a == 3) a = 2;

                LOD[] lods = new LOD[a];

                for (int i = 0; i < a; i++)
                {
                    paths[i] = AssetDatabase.GUIDToAssetPath(paths[i]);
                    GameObject go = AssetDatabase.LoadAssetAtPath<GameObject>(paths[i]);

                    Renderer[] renderers = new Renderer[2];

                    if (tree.transform.childCount >= (i + 1) * 2)
                    {
                        renderers[0] = tree.transform.GetChild(i * 2).GetComponent<Renderer>();
                        renderers[1] = tree.transform.GetChild(i * 2 + 1).GetComponent<Renderer>();
                    }
                    else
                    {
                        GameObject leafObj = go.transform.GetChild(0).gameObject.name.Contains("leaves") ? Object.Instantiate(go.transform.GetChild(0).gameObject, tree.transform) : Object.Instantiate(go.transform.GetChild(1).gameObject, tree.transform);
                        GameObject trunkObj = go.transform.GetChild(1).gameObject.name.Contains("trunk") ? Object.Instantiate(go.transform.GetChild(1).gameObject, tree.transform) : Object.Instantiate(go.transform.GetChild(0).gameObject, tree.transform); ;

                        leafObj.SetActive(false);
                        trunkObj.SetActive(false);
                        leafObj.hideFlags = HideFlags.HideInHierarchy;
                        trunkObj.hideFlags = HideFlags.HideInHierarchy;

                        if (tree.transform.GetChild(1).gameObject.name.Contains("trunk") && !tree.transform.GetChild(1).gameObject.activeInHierarchy || tree.transform.GetChild(0).gameObject.name.Contains("trunk") && !tree.transform.GetChild(0).gameObject.activeInHierarchy)
                            trunkObj.SetActive(false);

                        if (leafMat)
                            leafObj.GetComponent<MeshRenderer>().sharedMaterial = leafMat;
                        else
                            Debug.LogError("Leaves Mat Lost!");

                        if (trunkMat)
                            trunkObj.GetComponent<MeshRenderer>().sharedMaterial = trunkMat;
                        else
                            Debug.LogError("Trunk Mat Lost!");

                        renderers[0] = leafObj.GetComponent<Renderer>();
                        renderers[1] = trunkObj.GetComponent<Renderer>();
                    }

                    if (a == 2)
                    {
                        if (i == 0)
                        {
                            lods[i] = new LOD(dist1, renderers);
                        }
                        else if (i == 1)
                        {
                            lods[i] = new LOD(dist2, renderers);
                        }
                    }
                    else if (a == 1)
                    {
                        if (i == 0)
                        {
                            lods[i] = new LOD(dist2, renderers);
                        }
                    }

                }

                //LOD[] lods = new LOD[paths.Length];

                //for (int i = 0; i < paths.Length; i++)
                //{
                //    paths[i] = AssetDatabase.GUIDToAssetPath(paths[i]);
                //    GameObject go = AssetDatabase.LoadAssetAtPath<GameObject>(paths[i]);

                //    Renderer[] renderers = new Renderer[2];

                //    if (tree.transform.childCount >= (i + 1) * 2)
                //    {
                //        renderers[0] = tree.transform.GetChild(i * 2).GetComponent<Renderer>();
                //        renderers[1] = tree.transform.GetChild(i * 2 + 1).GetComponent<Renderer>();
                //    }
                //    else
                //    {
                //        GameObject leafObj = go.transform.GetChild(0).gameObject.name.Contains("leaves") ? Object.Instantiate(go.transform.GetChild(0).gameObject, tree.transform) : Object.Instantiate(go.transform.GetChild(1).gameObject, tree.transform);
                //        GameObject trunkObj = go.transform.GetChild(1).gameObject.name.Contains("trunk") ? Object.Instantiate(go.transform.GetChild(1).gameObject, tree.transform) : Object.Instantiate(go.transform.GetChild(0).gameObject, tree.transform); ;

                //        leafObj.SetActive(false);
                //        trunkObj.SetActive(false);
                //        leafObj.hideFlags = HideFlags.HideInHierarchy;
                //        trunkObj.hideFlags = HideFlags.HideInHierarchy;

                //        if (tree.transform.GetChild(1).gameObject.name.Contains("trunk") && !tree.transform.GetChild(1).gameObject.activeInHierarchy || tree.transform.GetChild(0).gameObject.name.Contains("trunk") && !tree.transform.GetChild(0).gameObject.activeInHierarchy)
                //            trunkObj.SetActive(false);

                //        if (leafMat)
                //            leafObj.GetComponent<MeshRenderer>().sharedMaterial = leafMat;
                //        else
                //            Debug.LogError("Leaves Mat Lost!");

                //        if(trunkMat)
                //            trunkObj.GetComponent<MeshRenderer>().sharedMaterial = trunkMat;
                //        else
                //            Debug.LogError("Trunk Mat Lost!");

                //        renderers[0] = leafObj.GetComponent<Renderer>();
                //        renderers[1] = trunkObj.GetComponent<Renderer>();
                //    }

                //    if (paths.Length == 3)
                //    {
                //        if (i == 0)
                //        {
                //            lods[i] = new LOD(dist0, renderers);
                //        }
                //        else if (i == 1)
                //        {
                //            lods[i] = new LOD(dist1, renderers);
                //        }
                //        else if (i == 2)
                //        {
                //            lods[i] = new LOD(dist2, renderers);
                //        }
                //    }
                //    else if (paths.Length == 2)
                //    {
                //        if (i == 0)
                //        {
                //            lods[i] = new LOD(dist1, renderers);
                //        }
                //        else if (i == 1)
                //        {
                //            lods[i] = new LOD(dist2, renderers);
                //        }
                //    }
                //    else if (paths.Length == 1)
                //    {
                //        if (i == 0)
                //        {
                //            lods[i] = new LOD(dist2, renderers);
                //        }
                //    }

                //}
                group.SetLODs(lods);
                SetFadeMode(0);
            }
        }
        
        void ResetTreeLod()
        {
            foreach (GameObject tree in treeListInScene)
            {
                if(tree.transform.childCount >= 2)
                {
                    LODGroup group = tree.GetComponent<LODGroup>();
                    LOD[] lods = new LOD[1];
                    if(tree.GetComponent<LODGroup>().GetLODs().Length > 0)
                    {
                        lods[0] = tree.GetComponent<LODGroup>().GetLODs()[0];
                     
                    }
                    lods[0].screenRelativeTransitionHeight = 0.2f;
                    group.SetLODs(lods);

                    while (tree.transform.childCount > 2)
                    {
                        GameObject temp = tree.transform.GetChild(tree.transform.childCount - 1).gameObject;
                        temp.transform.parent = null;
                        Object.DestroyImmediate(temp);
                    }
                }
            }
        }

        string MatchLODName(GameObject tree)
        {
            string name = tree.name;
            string tarName = "";

            for (int i = 0; i < name.Length; i++)
            {
                if (name[i] == 'p' && i < name.Length - 5 && name[i + 1] == 'r' && name[i + 5] == 'b' || name[i] == 'f' && i < name.Length - 2 && name[i + 1] == 'b' && name[i + 2] == 'x')
                {
                    tarName = tarName.Substring(0, tarName.Length - 1);
                    break;
                }
                else
                {
                    tarName += name[i];
                }
            }

            return tarName;
        }

        void SetScreenTransation(int lodLevel)
        {
            foreach (GameObject obj in treeListInScene)
            {
                LOD[] lods = obj.GetComponent<LODGroup>().GetLODs();
                if(lods.Length == 3)
                {
                    if(lodLevel == 0)
                    {
                        lods[lodLevel] = new LOD(dist0, obj.GetComponent<LODGroup>().GetLODs()[lodLevel].renderers);
                    }
                    else if(lodLevel == 1)
                    {
                        lods[lodLevel] = new LOD(dist1, obj.GetComponent<LODGroup>().GetLODs()[lodLevel].renderers);
                    }
                    else if(lodLevel == 2)
                    {
                        lods[lodLevel] = new LOD(dist2, obj.GetComponent<LODGroup>().GetLODs()[lodLevel].renderers);
                    }
                }
                else if(lods.Length == 2)
                {
                    if(lodLevel == 1)
                    {
                        lods[0] = new LOD(dist1, obj.GetComponent<LODGroup>().GetLODs()[0].renderers);
                    }
                    else if(lodLevel == 2)
                    {
                        lods[1] = new LOD(dist2, obj.GetComponent<LODGroup>().GetLODs()[1].renderers);
                    }
                }
                else if(lods.Length == 1)
                {
                    if(lodLevel == 2)
                    {
                        lods[0] = new LOD(dist2, obj.GetComponent<LODGroup>().GetLODs()[0].renderers);
                    }
                }
                obj.GetComponent<LODGroup>().SetLODs(lods);
                obj.GetComponent<ImpostorLODGroup>().UpdateImpostorLODSetting();

            }
        }

        void SetFadeMode(int fadeMode)
        {
            foreach(GameObject tree in treeListInScene)
            {
                LODGroup group = tree.GetComponent<LODGroup>();
                group.fadeMode = (LODFadeMode)fadeMode;
            }
        }
    }
}

#endif
