
using System;
using UnityEngine;
using UnityEditor;
using CFUtilPoolLib;
using UnityEditor.SceneManagement;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using CFEngine.Editor;
using CFEngine;

public class GrassData 
{
    public Transform oldTrans=null;
    public Vector3 oldRotEuler;
    public Transform newTrans=null;
    public GameObject newGO=null;
    public Quaternion rot;

}

public class Art_SceneTools : ArtToolsTemplate{
    public bool isPlay=false;
    //static bool _IsPlay=false;
    public int sceneID =-1;
    public GameObject newGO=null;
    private GameObject bakeGO=null;

    GameObject[] gameObjects;

    List<GrassData>  ListGrassD=new List<GrassData>();
    // Quaternion rot ;
    // GameObject newGOIns;
    enum rotChannel
    {
        x=0,
        y=1,
        z=2
    }
    // rotChannel rotC_0 =rotChannel.x;
    // rotChannel rotC_1 =rotChannel.y;
    // rotChannel rotC_2 =rotChannel.z;

    // [MenuItem("ArtTools/Scene_Tools")]
    // static void Init()
    // {
    //     // Get existing open window or if none, make a new one:
    //     Scene_Tools window = (Scene_Tools)EditorWindow.GetWindow(typeof(Scene_Tools));
    //     window.Show();

    // }
    // public class testEdit : ScriptableObject 
    // {
    //    public bool _IsPlay;
    //    public int sceneID;

    // }

    // static string costomDataPath=null;


    //  [MenuItem("ArtTools/playGame %l")]
    //     static void playGame()
    //     {
    //         // testEdit test = testEdit.CreateInstance<testEdit> ();
    //         // test.name = "testEdit";
    //         // test._IsPlay=true;
    //         // test.sceneID=-1;
    //         // costomDataPath= "Assets/"+test.name+".asset";
    //         // UnityEditor.AssetDatabase.CreateAsset (test, costomDataPath);

    //        GameObject MCamera= GameObject.FindGameObjectWithTag("MainCamera");
          
    //        if(MCamera)
    //         {
    //             CFEngine.EnvironmentExtra environmentExtra=MCamera.GetComponent<CFEngine.EnvironmentExtra>();
    //             if(environmentExtra)
    //             {
    //                 environmentExtra.enabled=true;
    //                 environmentExtra.loadGameAtHere=true;
    //             }
    //             else
    //             {
    //                 GameObject g = new GameObject("loadGameAtHere");
    //                 CFEngine.EnvironmentExtra e=g.AddComponent<CFEngine.EnvironmentExtra>();
    //                 e.enabled=true;
    //                 e.loadGameAtHere=true;
    //             }
    //         }
    //         else
    //         {
    //              GameObject g = new GameObject("loadGameAtHere");
    //              CFEngine.EnvironmentExtra e=g.AddComponent<CFEngine.EnvironmentExtra>();
    //              e.enabled=true;
    //              e.loadGameAtHere=true;
    //         }
 

    //         EditorApplication.isPlaying = true;
           


    //     }


             [MenuItem("ArtTools/CopyWaterParam")]
        static void CopyWaterParam()
        {
            GameObject[] gameObjects= Selection.gameObjects;
            Material waterMaterial=null;
            List<Material> otherMaterial=new List<Material>();


            foreach(GameObject g in gameObjects)
            {
                Renderer r = g.GetComponent<Renderer>();
                if(r)
                {
                    if(r.sharedMaterial)
                    {
                        bool isWater=false;
                        isWater= r.sharedMaterial.shader.name.Equals("Custom/Scene/Water");
                        if(isWater==true && waterMaterial==null)
                        {
                            waterMaterial=r.sharedMaterial;
                            
                        }
                        else
                        {
                            otherMaterial.Add(r.sharedMaterial);
                        }
                    }
                    
                }
                
                    
            }
            

            if(waterMaterial)
            {
                foreach(Material myMat in otherMaterial)
                {
                    myMat.SetVector("_ParamA",waterMaterial.GetVector("_ParamA"));
                    myMat.SetVector("_ParamB",waterMaterial.GetVector("_ParamB"));
                    myMat.SetVector("_ParamC",waterMaterial.GetVector("_ParamC"));
                    myMat.SetVector("_Wave1",waterMaterial.GetVector("_Wave1"));
                    myMat.SetVector("_Wave2",waterMaterial.GetVector("_Wave2"));
                    myMat.SetVector("_Wave3",waterMaterial.GetVector("_Wave3"));
                    myMat.SetVector("_Wave4",waterMaterial.GetVector("_Wave4"));
                    myMat.SetVector("_Wave5",waterMaterial.GetVector("_Wave5"));
                    myMat.SetVector("_Wave6",waterMaterial.GetVector("_Wave6"));
                    myMat.SetVector("_Wave7",waterMaterial.GetVector("_Wave7"));
                    myMat.SetVector("_Wave8",waterMaterial.GetVector("_Wave8"));
                    myMat.SetVector("_Wave9",waterMaterial.GetVector("_Wave9"));
                    myMat.SetVector("_SteepnessFadeout",waterMaterial.GetVector("_SteepnessFadeout"));
                }
            }
            
           



        }
    Vector2 v;

    public override void OnGUI() 
    {
        // EditorGUILayout.Space();
        // EditorGUILayout.Space();
        // EditorGUILayout.Space();
        
         v= GUILayout.BeginScrollView(v);

        // GUILayout.BeginHorizontal();
        // GUILayout.Label("场景Lightmap烘焙");
        // if (GUILayout.Button("##准备烘焙", GUILayout.MaxWidth(100)))
        // {
        //     //PrepareBake();

        // }
        // if (GUILayout.Button("##结束烘焙", GUILayout.MaxWidth(100)))
        // {
        //     //EndBake();

        // }
        // GUILayout.EndHorizontal();

        // EditorGUILayout.Space();

        // GUILayout.BeginHorizontal();
        // GUILayout.Label("CutOut转换");
        // if (GUILayout.Button("##PrcessSceneMat", GUILayout.MaxWidth(200)))
        // {
        //     //XEditor.AssetModify.PrcessSceneMat();

        // }

        // GUILayout.EndHorizontal();

        // EditorGUILayout.Space();

        // GUILayout.BeginHorizontal();
        // GUILayout.Label("怪物行走绿格子");
        // if (GUILayout.Button("Map Editor (Window)", GUILayout.MaxWidth(200)))
        // {
        //     EditorWindow.GetWindow(typeof(XEditor.MapEditor));

        // }

        // GUILayout.EndHorizontal();

        // EditorGUILayout.Space();

        // // GUILayout.BeginHorizontal();
        // // GUILayout.Label("2.5D半透明碰撞生成");
        // // if (GUILayout.Button("2.5D Model Collider Linker (Win)", GUILayout.MaxWidth(200)))
        // // {
        // //     EditorWindow.GetWindow<XLinkerEditor>(@"Linker Editor");

        // // }

        // // GUILayout.EndHorizontal();

        // EditorGUILayout.Space();

        // GUILayout.BeginHorizontal();
        // GUILayout.Label("FBX生成Prefab");
        // if (GUILayout.Button("CreatePrefab", GUILayout.MaxWidth(200)))
        // {
        //     AutoCreatePrefab.CreatePrefabFromFBX();
        // }
        // GUILayout.EndHorizontal();

        // EditorGUILayout.Space();

        // GUILayout.BeginHorizontal();
        // GUILayout.Label("FBX压缩设置");
        // if (GUILayout.Button("##Model Settings (Window)", GUILayout.MaxWidth(200)))
        // {
        //     //EditorWindow.GetWindowWithRect<XResModelImportEditorWnd>(new Rect(0, 0, 900, 500), true, @"XRes Import Editor");
        // }
        // GUILayout.EndHorizontal();

        // EditorGUILayout.Space();

        // GUILayout.BeginHorizontal();
        // GUILayout.Label("寻路生成辅助工具");
        // if (GUILayout.Button("Navigation Helper (Window)", GUILayout.MaxWidth(200)))
        // {
        //     EditorWindow.GetWindow(typeof(NavigationHelper));
        // }
        // GUILayout.EndHorizontal();

        // EditorGUILayout.Space();

        // GUILayout.BeginHorizontal();
        // GUILayout.Label("Lightmap尺寸修改");
        // if (GUILayout.Button("LightMapHelper (Window)", GUILayout.MaxWidth(200)))
        // {
        //     EditorWindow.GetWindow(typeof(LightMapHelper));
        // }
        // GUILayout.EndHorizontal();

        // EditorGUILayout.Space();

        // GUILayout.BeginHorizontal();
        // GUILayout.Label("Skybox添加到摄像机");
        // if (GUILayout.Button("##SkyboxHelper (Window)", GUILayout.MaxWidth(200)))
        // {
        //     //EditorWindow.GetWindow<SkyboxHelper>().Show();
        // }
        // GUILayout.EndHorizontal();

        EditorGUILayout.Space();

        // GUILayout.BeginHorizontal();
        // GUILayout.Label("运行项目");
        //  sceneID =EditorGUILayout.IntField("场景编号",sceneID) ;
        // if (GUILayout.Button("LoadGameAtHere", GUILayout.MaxWidth(200)))
        // {
        //     //GameObject[] gos = Selection.gameObjects;
        //    // GameObject go;
        //     //if (gos.Length > 0)
        //     //{
        //     //    for (int i = 0; i < gos.Length; i++)
        //     //    {
        //     //        if (!gos[i].GetComponent<LoadGameAtHere>())
        //     //            gos[i].AddComponent<LoadGameAtHere>();
        //     //    }
        //     //}
        //     //else
        //     //{
        //     //    if (EditorApplication.isPlaying)
        //     //    {
        //     //         go = new GameObject("Empty");
        //     //        go.AddComponent<LoadGameAtHere>();
        //     //    }

        //     //}
        //     EditorApplication.isPlaying = true;
        //     isPlay = true;
        // }

        // GUILayout.EndHorizontal();

       

        // EditorGUILayout.Space();

        // GUILayout.BeginHorizontal();
        // GUILayout.Label("添加脚本");

        // if (GUILayout.Button("ColliderShow", GUILayout.MaxWidth(200)))
        // {
        //     GameObject[] gos = Selection.gameObjects;
        //     if (gos.Length > 0)
        //     {
        //         for (int i = 0; i < gos.Length; i++)
        //         {
        //             if (!gos[i].GetComponent<ColliderShow>())
        //                 gos[i].AddComponent<ColliderShow>();
        //         }
        //     }
        // }
        // GUILayout.EndHorizontal();


         EditorGUILayout.Space();

        GUILayout.BeginHorizontal();
        GUILayout.Label("UI地图截图工具");

        if (GUILayout.Button("UI地图截图工具", GUILayout.MaxWidth(200)))
        {
           EditorWindow.GetWindow(typeof(ScreenCaptureUIMap));
        }
        GUILayout.EndHorizontal();



        EditorGUILayout.Space();
        GUILayout.BeginHorizontal();
        GUILayout.Label("Alpha通道拆分");

        if (GUILayout.Button("Alpha通道拆分", GUILayout.MaxWidth(200)))
        {
            SplitAlphaChannel.DivideTexture();
        }
        GUILayout.EndHorizontal();


        EditorGUILayout.Space();
        GUILayout.Label ("-------------------------------草prefab替换------------------------------------------");
        newGO= (GameObject) EditorGUILayout.ObjectField ("NewPrefab",newGO, typeof (GameObject), true, GUILayout.MaxWidth (300));
        //GUILayout.BeginHorizontal();
        //GUILayout.Label("prefab替换");

        if (GUILayout.Button("草prefab替换", GUILayout.MaxWidth(200)))
        {
            //originalGO = (GameObject) EditorGUILayout.ObjectField (originalGO, typeof (GameObject), true, GUILayout.MaxWidth (300));
            ListGrassD.Clear();

            gameObjects = Selection.gameObjects;
            foreach(GameObject g in gameObjects)
            {
                Vector3 GORot=g.transform.localEulerAngles ;
                Vector3 tempRot =new Vector3(0,0,0);
                Quaternion rot = Quaternion.Euler(GORot);
                // rot=setRot(rot,GORot.x,0);
                // rot=setRot(rot,GORot.y,1);
                // rot=setRot(rot,GORot.z,2);
                GameObject newGOIns = GameObject.Instantiate(newGO,g.transform.position,rot,g.transform.parent);
                newGOIns.transform.localScale = g.transform.localScale;
                InstanceObject instanceObject = g.GetComponent<InstanceObject>();

                
                if(instanceObject!=null)
                {
                    var chunkIndex = instanceObject.chunkIndex;
                    var lightmap = instanceObject.lightmap;
                    var render=  instanceObject.render;
                    var worldPosOffset = instanceObject.worldPosOffset;
                    UnityEngine. Debug.Log(chunkIndex+"__"+lightmap+"__"+render+"__"+worldPosOffset);


                    InstanceObject instanceObjectNewGo = newGO.GetComponent<InstanceObject>();

                    if(instanceObjectNewGo == null)
                    {
                        instanceObjectNewGo = newGOIns.AddComponent<InstanceObject>();
                        //instanceObjectNewGo = instanceObject;
                        instanceObjectNewGo.chunkIndex = chunkIndex;
                        instanceObjectNewGo.lightmap = lightmap;
                        instanceObjectNewGo.render = render;
                        instanceObjectNewGo.worldPosOffset =worldPosOffset;
                    }
                    else
                    {
                        //instanceObjectNewGo = instanceObject;
                        instanceObjectNewGo.chunkIndex = chunkIndex;
                        instanceObjectNewGo.lightmap = lightmap;
                        instanceObjectNewGo.render = render;
                        instanceObjectNewGo.worldPosOffset =worldPosOffset;
                    }
                    GrassData grassData=new GrassData();
                    grassData.oldRotEuler=g.transform.localEulerAngles;
                    grassData.newGO = newGOIns;
                    grassData.rot=rot;
                    ListGrassD.Add(grassData);
                }

                Renderer r =g.GetComponent<Renderer>();
                Renderer rNew =newGOIns.GetComponent<Renderer>();
                if(r!=null && rNew!=null )
                {
                    Material m =r.sharedMaterial;
                    if(m!=null)
                        rNew.sharedMaterial=m;
                }
                GameObject.DestroyImmediate(g);

            }

            
        }




        // rotC_0 = (rotChannel) EditorGUILayout.EnumPopup (rotC_0, GUILayout.MaxWidth(75));
        // rotC_1 = (rotChannel) EditorGUILayout.EnumPopup (rotC_1, GUILayout.MaxWidth(75));
        // rotC_2 = (rotChannel) EditorGUILayout.EnumPopup (rotC_2, GUILayout.MaxWidth(75));



        // if (GUILayout.Button("刷新", GUILayout.MaxWidth(100)))
        // {
        //     for(int i=0;i<ListGrassD.Count;i++)
        //     {
        //         Vector3 GORot=ListGrassD[i].oldRotEuler;
        //        // Transform newGOTrans =ListGrassD[i].newTrans;
        //         Quaternion rot = ListGrassD[i].rot;
        //         rot.eulerAngles=Vector3.zero; 
        //         rot=setRot(rot,GORot[(int)rotC_0],(int)rotC_0);
        //         rot=setRot(rot,GORot[(int)rotC_1],(int)rotC_1);
        //         rot=setRot(rot,GORot[(int)rotC_2],(int)rotC_2);
        //         ListGrassD[i].newGO.transform.rotation=rot;
        //     }

        // }


        //GUILayout.EndHorizontal();




        // EditorGUILayout.Space();
        // GUILayout.BeginHorizontal();
        // GUILayout.Label("角色预览工具");

        // if (GUILayout.Button("角色预览工具", GUILayout.MaxWidth(200)))
        // {
        //    EditorWindow.GetWindow(typeof(XEditor.PrefabPreviewWindow));
        // }
        // GUILayout.EndHorizontal();

        // EditorGUILayout.Space();
        // GUILayout.BeginHorizontal();
        // GUILayout.Label("大地形工具");

        // if (GUILayout.Button("ToolsWindow(Window)", GUILayout.MaxWidth(200)))
        // {
        //     EditorWindow.GetWindow(typeof(ToolsEditor));
        // }
        // GUILayout.EndHorizontal();
        EditorGUILayout.Space();
        GUILayout.Label ("-------------------------------splitMesh------------------------------------------");

        EditorGUILayout.Space();
        bakeGO= (GameObject) EditorGUILayout.ObjectField ("splitMesh",bakeGO, typeof (GameObject), true, GUILayout.MaxWidth (300));
        if (GUILayout.Button("splitMesh", GUILayout.MaxWidth(200)))
        {
            GameObject selectedGO=Selection.activeGameObject;
            GameObject prefab = PrefabUtility.GetCorrespondingObjectFromSource(selectedGO) as GameObject;
            string assetPath = AssetDatabase.GetAssetPath(prefab);
            // MeshFilter mf =selectedGO.GetComponent<MeshFilter>();
            // Mesh m =mf.sharedMesh;
            // Vector3[] verts =m.vertices;
            // int[] tris=m.triangles;
            // Mesh m_a=new Mesh();
            // Mesh m_b=new Mesh();
            UnityEngine.Debug.Log(Selection.assetGUIDs.Length);
            UnityEngine.Debug.Log(selectedGO.name);
            UnityEngine.Debug.Log(AssetDatabase.GetAssetPath((UnityEngine.Object)selectedGO));
            

        }


        EditorGUILayout.BeginFadeGroup(100);
        GUILayout.Label ("--");
        EditorGUILayout.EndFadeGroup();
        
        // EditorGUILayout.BeginFoldoutHeaderGroup(false,"BakeMeshModifData");
        // GUILayout.Label ("--");
        // EditorGUILayout.EndFoldoutHeaderGroup();


        
        GUILayout.EndScrollView();
    }

    public Quaternion setRot(Quaternion r , float value,int channel )
    {
        Vector3 angle=r.eulerAngles;
        switch(channel)
        {
            case 0:
                angle.x=value;
                break;

            case 1:
                angle.y=value;
                break;

            case 2:
                angle.z=value;
                break;
        }
        
        UnityEngine.Debug.Log(channel+"__"+value);
        return Quaternion.Euler(angle);
    }



    // public override void Update()
    // {
        
    //     // if (isPlay)
    //     // {
    //     //     if (EditorApplication.isPlaying)
    //     //     {
    //     //         GameObject g = GameObject.Find("Empty");
    //     //         if (!g)
    //     //         {
    //     //             g = new GameObject("Empty");
    //     //         }
    //     //         if (!g.GetComponent<LoadGameAtHere>())
    //     //             g.AddComponent<LoadGameAtHere>();
    //     //         isPlay = false;
    //     //     }
    //     // }
 

    //         //if (null == XInterfaceMgr.singleton.GetInterface<IEntrance> (0))
    //         //{
    //             if (EditorApplication.isPlaying)
    //             {
    //                 UnityEngine.Debug.Log(11111);
    //                 if (isPlay)
    //                 {
    //                     UnityEngine.SceneManagement.SceneManager.LoadScene (0);
    //                     CFEngine.BlockSceneLoadSystem.gotoSceneID = sceneID;
    //                     isPlay = false;
    //                 }
    //             }
               
    //             // if (useCurrentScene)
    //             // {
    //             //     XDebug.editorSceneReplace = UnityEngine.SceneManagement.SceneManager.GetActiveScene ().name;
    //             // }
    //             // else
    //             // {
    //             //     XDebug.editorSceneReplace = "";
    //             // }
    //             // if (gotoScene)
    //             //     XDebug.sceneID = sceneID;
    //             // else
    //             //     XDebug.sceneID = -1;
                
    //         //}
    // }
    
}