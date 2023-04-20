#if UNITY_EDITOR
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using System.IO;

namespace CFEngine.Editor
{

    public class SplitInput 
    {

        //地图切割功能
  
        public static int cutnumx=3;
        public static int cutnumz=3;

        private static List<Vector3> Linelistx=new List<Vector3> ();
        private static List<Vector3> Linelistz = new List<Vector3>();
        private static List<Vector3> Pointlistx=new List<Vector3> ();
        private static List<Vector3> Pointlistz = new List<Vector3>();
        public static Queue<SplitObject> splitstemp = new Queue<SplitObject>();
        public static List<SplitObject> splitsb = new List<SplitObject>();
        public static List<GameObject> NeedDeleteObject = new List<GameObject>();
        public static List<GameObject> ShowprefabObject = new List<GameObject>();
        private static string SAVEPATH;
        private static Plane plane;
        private static LightmapData _lightmapdata = new LightmapData();

        public static  void SetSplitObjectbynum(GameObject splitmesh,int x,Transform TF,string splitmeshpath,bool fadeEffect)
        {
           
            if (splitmesh == null)
                return;
            Mesh _meshss = splitmesh.GetComponent<MeshFilter>().sharedMesh;
            if(_meshss==null|| _meshss.uv2.Length <= 0)
            {
                Debug.LogError("物体" + splitmesh.name + "没有UV2,需要的话可以添加下！");
              //  return;
            }
            Color[] color = null;
            MeshRenderObject _mro = splitmesh.GetComponent<MeshRenderObject>();
            if (_mro)
            {
                _mro.BindAddMeshInfo();          
                if (_mro.additionalVertexStreamMesh != null)
                {
                    color = _mro.additionalVertexStreamMesh.colors;
                }
            }

            GetRightLine(splitmesh);
            NeedDeleteObject.Clear();
            ShowprefabObject.Clear();
            _lightmapdata = new LightmapData();
            GetPath(splitmesh);
        //    return;
            SplitObject split = splitmesh.transform.GetComponent<SplitObject>();
            MeshRenderer _meshrender = splitmesh.transform.GetComponent<MeshRenderer>();
            Mesh _meshdss= splitmesh.transform.GetComponent<MeshFilter>().sharedMesh;
            if (_meshrender)
            {
                _meshrender.additionalVertexStreams = null;
            }
            if (_meshdss)
            {
                _meshdss.colors = color;
            }
            if (split)
            {
                splitstemp.Clear();
                splitsb.Clear();
                splitstemp.Enqueue(split);
                for (int i = 0; i < Linelistx.Count; i++)
                {   
                    Vector3 planeline = Linelistx[i];
                    Vector3 planepoint = Pointlistx[i];
                    bool lastone = false;
                    if (i == Linelistx.Count - 1)
                    {
                        lastone = true;
                    }
                    if (splitstemp.Count > 0)
                    {
                        SplitObject sob = splitstemp.Dequeue();
                        plane = new Plane(Vector3.Cross(planeline, Vector3.up).normalized, planepoint);
                        sob.transform.GetComponent<SplitObject>().Splitmulti(plane, lastone, false);
                    }              
                }
                if (splitsb.Count > 0)
                {
                    for (int i = 0; i < splitsb.Count; i++)
                    {
                        splitstemp.Clear();
                        splitstemp.Enqueue(splitsb[i]);
                        for (int z = 0; z < Linelistz.Count; z++)
                        {
                            bool lastone = false;
                            Vector3 planeline = Linelistz[z];
                            Vector3 planepoint = Pointlistz[z];
                            if (z == Linelistz.Count - 1)
                            {
                                lastone = true;
                            }
                            if (splitstemp.Count > 0)
                            {
                                SplitObject sob = splitstemp.Dequeue();
                                plane = new Plane(Vector3.Cross(planeline, Vector3.up).normalized, planepoint);
                                sob.transform.GetComponent<SplitObject>().Splitmulti(plane, lastone, true);
                            }
                        }
                    }
                }

                if (ShowprefabObject.Count > 0)
                {                 
                    for (int i = 0; i < ShowprefabObject.Count; i++)
                    {
                        // ShowprefabObject[i].transform.SetParent(splitmesh.transform.parent);
                        ShowprefabObject[i].transform.position = Vector3.zero;
                         ShowprefabObject[i].transform.eulerAngles = Vector3.zero;
                        ShowprefabObject[i].transform.localScale = Vector3.one;
                        SavePrefab(ShowprefabObject[i], i, splitmesh,x,TF, splitmeshpath, fadeEffect);

                    }
                    ShowprefabObject.Clear();
                }
                if (NeedDeleteObject.Count > 0)
                {                  
                    for (int i = 1; i < NeedDeleteObject.Count; i++)
                    {
                      Object. DestroyImmediate(NeedDeleteObject[i]);
                    }
                    NeedDeleteObject.Clear();
                }
                split.gameObject.SetActive(false);
            }  
        }

        private static void GetRightLine(GameObject splitmesh)
        {
            BoxCollider splitBC;
            splitBC = splitmesh.GetComponent<BoxCollider>();
            if (!splitBC)
            {
                splitmesh.AddComponent(typeof(BoxCollider));
                splitBC = splitmesh.GetComponent<BoxCollider>();
            }
            //MeshRenderObject splitBC;
            //splitBC = splitmesh.GetComponent<MeshRenderObject>();
            //if (!splitBC)
            //{
            //    splitmesh.AddComponent(typeof(MeshRenderObject));
            //    splitBC = splitmesh.GetComponent<MeshRenderObject>();
            //}
            SplitObject sp = splitmesh.GetComponent<SplitObject>();
            if (!sp)
            {
                splitmesh.AddComponent(typeof(SplitObject));          
            }

            //   List<Vector3> boxpoints = new List<Vector3>();
            Linelistx.Clear();
            Linelistz.Clear();
            Pointlistx.Clear();
            Pointlistz.Clear();

            bool YSMAL = true;
            if (splitBC.size.y > splitBC.size.z)
            {
                YSMAL = false;
            }
            float halfxsize = splitBC.size.x / 2f;
            float halfysize = splitBC.size.y / 2f;
            float halfzsize = splitBC.size.z / 2f;
            Vector3 xpypzp;
            Vector3 xpypzn;
            Vector3 xnypzp;
            Vector3 xnypzn;
            if (YSMAL)
            {
                xpypzp = splitmesh.transform.position + new Vector3(halfxsize, halfysize, halfzsize);
                xpypzn = splitmesh.transform.position + new Vector3(halfxsize, halfysize, -halfzsize);
                xnypzp = splitmesh.transform.position + new Vector3(-halfxsize, halfysize, halfzsize);
                xnypzn = splitmesh.transform.position + new Vector3(-halfxsize, halfysize, -halfzsize);
            }
            else
            {
                xpypzp = splitmesh.transform.position + new Vector3(halfxsize, halfzsize, halfysize);
                xpypzn = splitmesh.transform.position + new Vector3(halfxsize, halfzsize, -halfysize);
                xnypzp = splitmesh.transform.position + new Vector3(-halfxsize, halfzsize, halfysize);
                xnypzn = splitmesh.transform.position + new Vector3(-halfxsize, halfzsize, -halfysize);
            }

            float xadd = (xpypzp.x - xnypzp.x)/cutnumx;

            float zadd = (xpypzp.z - xpypzn.z) /cutnumz;
            //横向
            for (int i = 0; i < cutnumx - 1; i++)
            {
                Vector3 xdown = xnypzn + new Vector3(xadd*(i+1),0,0);
                Vector3 xup = xnypzp + new Vector3(xadd*(i+1), 0, 0);
                Vector3 dirction1 = xup - xdown;
                Linelistx.Add(dirction1);
                Pointlistx.Add(xup);
            }
            //纵向
            for (int i = 0; i < cutnumz - 1; i++)
            {
                Vector3 zdown = xnypzn + new Vector3(0f,0f, zadd * (i + 1));
                Vector3 zup = xpypzn + new Vector3(0f, 0f,zadd * (i + 1));
                Vector3 dirction1 = zup - zdown;
                Linelistz.Add(dirction1);       
                Pointlistz.Add(zup);
            }

        }

        private static void GetPath(GameObject go)
        {
            GameObject source = PrefabUtility.GetCorrespondingObjectFromSource(go) as GameObject;
            if (source != null)
            {
                SAVEPATH = AssetDatabase.GetAssetPath(source);
                if (SAVEPATH.EndsWith(".FBX"))
                {
                    SAVEPATH = SAVEPATH.Substring(0, SAVEPATH.Length - 4);
                }else if (SAVEPATH.EndsWith(".prefab"))
                {
                    SAVEPATH = SAVEPATH.Substring(0, SAVEPATH.Length - 7);
                }
            }
       
          //  PrefabUtility.ReplacePrefab(Selection.activeGameObject, source, ReplacePrefabOptions.ConnectToPrefab | ReplacePrefabOptions.ReplaceNameBased);         
        }

        private static void SavePrefab(GameObject GO, int i, GameObject splitmesh, int x, Transform TF,string splitpath, bool fadeEffect)
        {      
            
            Mesh _mesh = GO.transform.GetComponent<MeshFilter>().sharedMesh;
            string tmpPath = splitpath + "/"+ splitmesh.name+ "_split" + x.ToString() + "_" + i.ToString() + ".asset";
            AssetDatabase.CreateAsset(_mesh, tmpPath);
            GO.name = splitmesh.name + "_split" + x.ToString() + "_" + i.ToString();
            PrefabUtility.SaveAsPrefabAssetAndConnect(GO, splitpath + "/" + splitmesh.name + "_split" + x.ToString() + "_" + i.ToString() + ".prefab", InteractionMode.AutomatedAction);
            GO.name = splitmesh.name + "_split" + x.ToString() + "_" + i.ToString();

            PrefabUtility.RevertPrefabInstance(GO, InteractionMode.AutomatedAction);


            MeshRenderObject _mro = GO.GetComponent<MeshRenderObject>();
            if (_mro)
            {
                Object.DestroyImmediate(_mro);
            }

            //lightmap
            MeshRenderer mrgo = GO.GetComponent<MeshRenderer>();
            if (mrgo)
            {
                if (fadeEffect)
                {
                    TryAddCollider(mrgo.gameObject);
                }

                MeshRenderer _splitmeshrender = splitmesh.GetComponent<MeshRenderer>();
                mrgo.scaleInLightmap = _splitmeshrender.scaleInLightmap;
                mrgo.lightmapIndex = _splitmeshrender.lightmapIndex;
                mrgo.lightmapScaleOffset = _splitmeshrender.lightmapScaleOffset;

                GameObject upgo = splitmesh.transform.parent.gameObject;
                LightmapVolumn _lv = TF.GetComponent<LightmapVolumn>();
                LigthmapRenderData _lightmaprenderdata = new LigthmapRenderData();
                _lightmaprenderdata.render = GO.GetComponent<Renderer>();

                _lightmaprenderdata.lightmapIndex = _splitmeshrender.lightmapIndex;
                _lightmaprenderdata.lightmapScaleOffset = _splitmeshrender.lightmapScaleOffset;
                _lightmaprenderdata.realtimeLightmapIndex = _splitmeshrender.realtimeLightmapIndex;
                _lightmaprenderdata.realtimeLightmapScaleOffset = _splitmeshrender.realtimeLightmapScaleOffset;
                List<LigthmapRenderData> temprenderer = new List<LigthmapRenderData>();
                if (_lv != null)
                {
                    GO.transform.SetParent(TF);
                    if (_lv.renders!=null&&_lv.renders.Length > 0)
                    {
                        for (int a = 0; a < _lv.renders.Length; a++)
                        {

                            if (_lv.renders[a].render!=null&&_lv.renders[a].render.gameObject.name == splitmesh.name)
                            {
                                _lightmaprenderdata.lightmapIndex = _lv.renders[a].lightmapIndex;
                                _lightmaprenderdata.lightmapScaleOffset = _lv.renders[a].lightmapScaleOffset;
                                _lightmaprenderdata.realtimeLightmapIndex = _lv.renders[a].realtimeLightmapIndex;
                                _lightmaprenderdata.realtimeLightmapScaleOffset = _lv.renders[a].realtimeLightmapScaleOffset;
                            }
                            temprenderer.Add(_lv.renders[a]);
                        }
                        temprenderer.Add(_lightmaprenderdata);
                        _lv.renders = temprenderer.ToArray();
                    }
                }
            }
        }

        public static void TryAddCollider(GameObject gameObject)
        {
            if (gameObject == null) return;

            if (gameObject.GetComponent<Collider>() == null)
            {
                BoxCollider bc = gameObject.AddComponent<BoxCollider>();
                bc.isTrigger = true;
            }
        }
    }
}
#endif