using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MeshFace
{
    [ExecuteInEditMode]
    public class GetAllMashFace : MonoBehaviour
    {
        private  int meshFilterVerts;
        private  uint meshFilterTris;
        private  int skinVerts;
        private  uint skinTris;
        public static uint meshFilterMaxTris;
        private  string meshFilterMaxName;
        public static uint skinMaxTris;
        private  string skinMaxName;
        public static bool isBool,isSave = false;
        private MeshFilter[] meshs;
        private SkinnedMeshRenderer[] skinMeshs;
        private List<MeshFilter> meshFilterList;
        private List<SkinnedMeshRenderer> skinMeshsList;
        public static string outputScene;
        public static string outputChar;
        public static int count = 20;
        string meshFiltersavePath;//文件路径
        string skinMeshssavePath;//文件路径
        // //MeshFilter[] nums;
        // private List<MeshFilter> nums;
        // //SkinnedMeshRenderer[] numss;
        // private List<SkinnedMeshRenderer> numss;
        void Start()
        {      
            GetAllObjects();
            meshFiltersavePath = Path.Combine(Application.persistentDataPath, "meshFiltersaveFile");
            skinMeshssavePath = Path.Combine(Application.persistentDataPath, "skinMeshssaveFile");
        }
        private void Update()
        {
            if (isBool)
            {
                meshFilterList.Clear();
                skinMeshsList.Clear();
                GetAllObjects();
            }
            if (isSave)
            {
                Save();
            }
        }
        void GetAllObjects()
        {
            // Debug.Log(new MeshFilter().GetType());
            // Debug.Log(new SkinnedMeshRenderer().GetType());
            meshFilterList = new List<MeshFilter>();
            skinMeshsList = new List<SkinnedMeshRenderer>();
    
            // nums = new List<MeshFilter>();
            // numss = new List<SkinnedMeshRenderer>();
            
            meshFilterMaxTris = 0;
            skinMaxTris = 0;
            
            meshs = FindObjectsOfType(typeof(MeshFilter)) as MeshFilter[];
            skinMeshs = FindObjectsOfType(typeof(SkinnedMeshRenderer)) as SkinnedMeshRenderer[];
            
            // Scene activeScene = SceneManager.GetActiveScene();
            // var roots = activeScene.GetRootGameObjects();
            // for (int i = 0; i < roots.Length; i++)
            // {
            //     if (roots[i].GetComponentsInChildren<MeshFilter>()!=null)
            //     {
            //         //nums = roots[i].GetComponentsInChildren<MeshFilter>();
            //         nums.AddRange(roots[i].GetComponentsInChildren<MeshFilter>());
            //     }else if (roots[i].GetComponentsInChildren<SkinnedMeshRenderer>() != null)
            //     {
            //         //numss = roots[i].GetComponentsInChildren<SkinnedMeshRenderer>();
            //         numss.AddRange(roots[i].GetComponentsInChildren<SkinnedMeshRenderer>());
            //     }
            //     
            //     
            //     // if (roots[i].GetComponent<MeshFilter>())
            //     // {
            //     //     meshFilterList.Add(roots[i].GetComponent<MeshFilter>());
            //     //     Debug.Log(1);
            //     // }else if (roots[i].GetComponent<SkinnedMeshRenderer>())
            //     // {
            //     //     skinMeshsList.Add(roots[i].GetComponent<SkinnedMeshRenderer>());
            //     //     Debug.Log(2);
            //     // }
            // }
            // Debug.Log("cj:"+nums.Count);
            // Debug.Log("js:"+numss.Count);
            
            if (meshs != null)
            {
                AllMeshFilterSort(meshs);
            }
            
            if (skinMeshs != null)
            {
                AllSkinnedMeshRendererSort(skinMeshs);
            }
            
            //记录
            outputScene = "";
            outputChar = "";
            if (meshFilterList!=null)
            {
                int a = 0;
                foreach (var mesh in meshFilterList)
                {
                    if (a < count)
                    {
                        outputScene += $"{mesh.name}:{mesh.sharedMesh.GetIndexCount(0) / 3}\n";
                    }
                    a++;
                }
            }
            if (skinMeshsList != null)
            {
                int a = 0;
                foreach (var mesh1 in skinMeshsList)
                {
                    if (a < count)
                    {
                        outputChar += $"{mesh1.name}:{mesh1.sharedMesh.GetIndexCount(0) / 3}\n";
                    }
                    a++;
                }
            }
        }
        void AllMeshFilterSort(MeshFilter[] meshs)
        {
            for (var i = 0; i < meshs.Length; i++)
            {
                for (var j = 0; j < meshs.Length - 1 - i; j++)
                {
                    if (meshs[j].sharedMesh!=null&&meshs[j + 1].sharedMesh!=null)
                    {
                        if (meshs[j].sharedMesh.GetIndexCount(0)/3 < meshs[j + 1].sharedMesh.GetIndexCount(0)/3)
                        {
                            var  temp = meshs[j+1];
                            meshs[j + 1] = meshs[j];
                            meshs[j] = temp;
                        }
                    }
                }
            }
            foreach (var mesh in meshs)
            {
                if (mesh.sharedMesh != null)
                {
                    var nub = mesh.sharedMesh.GetIndexCount(0) / 3;
                    meshFilterMaxTris += nub;
                    meshFilterList.Add(mesh);
                }
    
            }
        }
        void AllSkinnedMeshRendererSort(SkinnedMeshRenderer[] skinMeshs)
        {
            for (var i = 0; i < skinMeshs.Length; i++)
            {
                for (var j = 0; j < skinMeshs.Length - 1 - i; j++)
                {
                    if (skinMeshs[j].sharedMesh != null && skinMeshs[j + 1].sharedMesh != null)
                    {
                        if (skinMeshs[j].sharedMesh.GetIndexCount(0)/3 < skinMeshs[j + 1].sharedMesh.GetIndexCount(0)/3)
                        {
                            var  temp = skinMeshs[j+1];
                            skinMeshs[j + 1] = skinMeshs[j];
                            skinMeshs[j] = temp;
                        }
                    }
                }
            }
            foreach (var mesh in skinMeshs)
            {
                if (mesh.sharedMesh != null)
                {
                    var nub = mesh.sharedMesh.GetIndexCount(0) / 3;
                    skinMaxTris += nub;
                    skinMeshsList.Add(mesh);
                }
            }
        }
        // void OnGUI()
        // {
        //     GUIStyle style = new GUIStyle();
        //     GUIStyle style1 = new GUIStyle();
        //     GUIStyleFun(style,style1);
        //     isBool = GUILayout.Button("刷新");
        //     isSave = GUILayout.Button("保存");
        //     
        //     GUILayout.BeginHorizontal();
        //     {
        //         GUILayout.BeginVertical("box");
        //         {
        //             GUILayout.Label("场景：",style1);
        //             GUILayout.Label("总计:"+meshFilterMaxTris);
        //             outputScene = GUILayout.TextArea(outputScene, style);
        //         }
        //         GUILayout.EndVertical();
        //         GUILayout.BeginVertical("box");
        //         {
        //             GUILayout.Label("角色：",style1);
        //             GUILayout.Label("总计:"+skinMaxTris);
        //             outputChar = GUILayout.TextArea(outputChar, style);
        //         }
        //         GUILayout.EndVertical();
        //     }
        //     GUILayout.EndHorizontal();
        // }
        void GUIStyleFun(GUIStyle style,GUIStyle style1)
        {
            style.normal.background = null;
            style.normal.textColor = new Color(0.0f, 0f, 0.0f); 
            style.fontSize = 12;
            style1.normal.background = null;
            style1.normal.textColor = new Color(1.0f, 0f, 0.0f); 
            style1.fontSize = 15;
        }
        void Save()//存储数据
        {
            using (FileStream file = new FileStream(meshFiltersavePath, FileMode.Create))
            {
                byte[] bts = System.Text.Encoding.UTF8.GetBytes(outputScene);
                file.Write(bts, 0, bts.Length);
                Debug.Log(meshFiltersavePath);
            }
            using (FileStream file = new FileStream(skinMeshssavePath, FileMode.Create))
            {
                byte[] bts = System.Text.Encoding.UTF8.GetBytes(outputChar);
                file.Write(bts, 0, bts.Length);
                Debug.Log(skinMeshssavePath);
            }
        }
    }
}
