#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace CFEngine.Editor
{
    public class RoleMatStatistics : RoleEditorComponet<RoleMatStatistics>
    {
        private List<BandposeData> unbatchedMats = new List<BandposeData>();
        private static HashSet<string> matNames = new HashSet<string>();
        private SearchField _searcher = new SearchField();
        private string _searchKeyword = "";
        private Vector2 _scrollPosition;
        const string CombineNodeName = "__COMBMESH__";
        public override void Init()
        {
        }

        public override void DrawGUI()
        {
            if (GUILayout.Button("Find Unbatched Mat"))
                GetUnbatchedMat();

            GUILayout.BeginHorizontal();
            {
                GUILayout.Label("Search");
                _searchKeyword = _searcher.OnGUI(_searchKeyword);
            }
            GUILayout.EndHorizontal();

            int count;

            _scrollPosition = GUILayout.BeginScrollView(_scrollPosition,GUILayout.Width(0),GUILayout.Height(0));
            if (_searchKeyword.Equals(""))
            {
                count = 0;
                foreach (var unbatchedMat in unbatchedMats)
                {
                    BandposeDataField(unbatchedMat);
                    count++;
                }
            }
            else
            {
                count = 0;
                foreach (var unbatchedMat in unbatchedMats)
                {
                    if (unbatchedMat.name.ToLower().Contains(_searchKeyword.ToLower()))
                    {
                        BandposeDataField(unbatchedMat);
                        count++;
                    }
                }
            }
            GUILayout.EndScrollView();
            
            GUILayout.FlexibleSpace();
            GUILayout.Label("Unbatched Mat Count: " + count);
        }    

        private void GetUnbatchedMat()
        {
            if (!RoleEditorComponetContext.bandposeListInited)
            {
                Debug.LogError("Get Bandpose First!!!");
                return;
            }
            foreach (BandposeData bpd in RoleEditorComponetContext.bandposeDatas)
            {
                for (int i = 0; i < bpd.parts.Count; i++)
                {
                    if (!(bpd.name.Contains("Role") || bpd.name.Contains("Monster")) || bpd.name.Contains("FX") || !bpd.parts[i].material)
                        continue;

                    if (matNames.Contains(bpd.parts[i].material.name))
                    {
                        unbatchedMats.Add(bpd);
                        break;
                    }
                    else
                    {
                        matNames.Add(bpd.parts[i].material.name);
                    }
                }
                matNames.Clear();
            }
        }

        [MenuItem("Tools/批量工具/批量合批测试")]
        public static void CombineTool()
        {
            if (Selection.activeGameObject == null)
            {
                Debug.LogError("没有选中的物件!");
                return;
            }

            CombineObj(Selection.activeGameObject);
        }

        private static void CombineObj(GameObject rootObj)
        {
            List<List<GameObject>> comblist = new List<List<GameObject>>();

            foreach(Transform oneRole in rootObj.transform)
            {
                Dictionary<int, List<GameObject>> idAndParts = new Dictionary<int, List<GameObject>>();

                for (int i = oneRole.childCount - 1; i >= 0; i--)
                {
                    Transform onePart = oneRole.GetChild(i);
                    if(onePart.name == CombineNodeName)
                    {
                        Object.DestroyImmediate(onePart.gameObject);
                        continue;
                    }

                    //if (!onePart.gameObject.activeInHierarchy)
                        //onePart.gameObject.SetActive(true);

                    if (!onePart.GetComponent<SkinnedMeshRenderer>() || !onePart.GetComponent<SkinnedMeshRenderer>().sharedMaterial)
                        continue;

                    int matID = onePart.GetComponent<SkinnedMeshRenderer>().sharedMaterial.GetInstanceID();
                    if (!idAndParts.ContainsKey(matID))
                    {
                        idAndParts.Add(matID, new List<GameObject>());
                    }
                    idAndParts[matID].Add(onePart.gameObject);
                }

                foreach (var kv in idAndParts)
                {
                    var gos = kv.Value;
                    if (gos.Count > 1)
                        comblist.Add(gos);
                }

                idAndParts.Clear();
            }

            for (int i = 0; i < comblist.Count; i++)
            {
                GameObject oneRole = comblist[i][0].transform.parent.gameObject;
                var newChild = new GameObject(CombineNodeName);
                newChild.transform.SetParent(oneRole.transform, false);
                var gos = comblist[i];

                var oldMr = gos[0].GetComponent<SkinnedMeshRenderer>();
                if (!oldMr)
                {
                    Debug.LogError("老的材质出了问题! " + gos[0].name);
                    continue;
                }

                var newSMR = newChild.AddComponent<SkinnedMeshRenderer>();
                newSMR.sharedMesh = CombineMeshRender(oneRole.name, oneRole, gos);
                newSMR.sharedMaterial = oldMr.sharedMaterial;

                int boneCnt = 0;
                for(int j = 0; j < gos.Count; j++)
                {
                    boneCnt += gos[j].GetComponent<SkinnedMeshRenderer>().bones.Length;
                }
                int index = 0;
                for(int j = 0; j < gos.Count; j++)
                {
                    Transform[] newBones = new Transform[boneCnt];
                    Transform[] oldBones = gos[j].GetComponent<SkinnedMeshRenderer>().bones;
                    for(int k = 0; k < oldBones.Length; k++)
                    {
                        newBones[index] = oldBones[k];
                        index++;
                    }
                }
                newSMR.rootBone = oldMr.rootBone;

                foreach (var g in gos)
                {
                    g.SetActive(false);
                }
            }

            AssetDatabase.Refresh();
            EditorSceneManager.MarkAllScenesDirty();
            comblist.Clear();
        }
        
        private static Mesh CombineMeshRender(string prefixName, GameObject rootObj, List<GameObject> input)
        {
            var matrix = rootObj.transform.worldToLocalMatrix;
    
            CombineInstance[] combine = new CombineInstance[input.Count];
            for(int i = 0; i < input.Count; ++i)
            {
                var mf = input[i].GetComponent<SkinnedMeshRenderer>();
                combine[i].mesh = mf.sharedMesh;
                combine[i].transform = matrix * mf.transform.localToWorldMatrix;
            }
            
            Mesh mesh = new Mesh();
            mesh.CombineMeshes(combine, true);
    
            string assetPath = string.Empty;
            for(int i = 0; i < 100; ++i)
            {
                string number = Random.Range(100000, 999999).ToString();
                assetPath = "Assets/Test/CombineMeshTest/" + prefixName + "_"+  number + ".asset";
                if (!File.Exists(assetPath))
                    Directory.CreateDirectory("Assets/Test/CombineMeshTest");
            }
    
            AssetDatabase.CreateAsset(mesh, assetPath);
            AssetDatabase.SaveAssets();
    
            var meshRet = AssetDatabase.LoadAssetAtPath(assetPath, typeof(Mesh)) as Mesh;
            return meshRet;
        }

        public override void Destroy()
        {
            unbatchedMats.Clear();
        }

        public override string Name()
        {
            return "Role Material Statistics";
        }
    }
}
#endif
