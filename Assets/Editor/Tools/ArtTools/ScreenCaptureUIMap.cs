using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEditor.SceneManagement;
using CFEngine.Editor;
using CFEngine;
public class ScreenCaptureUIMap : EditorWindow {

    [MenuItem ("ArtTools/UIMap")]
    public static void MenuEntry () {
        ScreenCaptureUIMap window = ScreenCaptureUIMap.GetWindow<ScreenCaptureUIMap> ();
        window.Init ();
        window.Show ();
    }

    public void Init () {
        minSize = new Vector2 (400, 400);
        if (string.IsNullOrEmpty (outputFolder)) {
            outputFolder = Path.Combine (Application.dataPath, "ScreenCaptured");
        }
    }

    Vector2 GetMainGameViewResolution () {
        System.Type T = System.Type.GetType ("UnityEditor.GameView,UnityEditor");
        System.Reflection.MethodInfo GetMainGameView = T.GetMethod ("GetMainGameView", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
        System.Object Res = GetMainGameView.Invoke (null, null);

        //Taking game view using the method shown below	
        var gameView = Res;
        var prop = gameView.GetType ().GetProperty ("currentGameViewSize", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var gvsize = prop.GetValue (gameView, new object[0] { });
        var gvSizeType = gvsize.GetType ();

        Vector2 resolution;
        //I have 2 instance variable which this function sets:
        resolution.x = (int) gvSizeType.GetProperty ("width", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance).GetValue (gvsize, new object[0] { });
        resolution.y = (int) gvSizeType.GetProperty ("height", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance).GetValue (gvsize, new object[0] { });

        return resolution;
    }

    public enum TextureType {
        PNG,
        EXR,
    }
    

    public int captureTextureRadio = 1;
   // public Stack<RenderTexture> capturedRTHistory = new Stack<RenderTexture> ();
    public RenderTexture capturedRT = null;
    static public string outputFolder = "Assets";
    public TextureType textureType = TextureType.PNG;
    private bool showTree = true;
    private bool showTerrain = true;
    private bool showOther = true;
    private bool showTag=false;

    private float createdSphereScale=1.0f;
    private bool showClimbPoint00=false;
    private bool showClimbPoint01=true;
    private bool showClimbPoint02=true;
    private bool showClimbPoint03=false;
    private bool showClimbPoint19=false;
    private Color col00=new Color(1,1,0,1);
    private Color col01=new Color(1,0,0,1);
    private Color col02=new Color(0,1,0,1);
    private Color col03=new Color(0,0,1,1);
    private Color col19=new Color(0,1,1,1);
   private List<GameObject> AllCreatedSpheres=new List<GameObject>();
    // string[] str = UnityEditorInternal.InternalEditorUtility.tags;
 
//  public enum tttt {
//     }
    static public String CaptureTag ="Untagged";

    
    private void OnGUI () {
      // tttt aas= (tttt)Enum.Parse(typeof(tttt),"s", true);
        
        using (var scope = new EditorGUILayout.HorizontalScope ()) {
            EditorGUILayout.LabelField ("截图缩放倍数：");
            captureTextureRadio = EditorGUILayout.IntSlider (captureTextureRadio, 1, 16);
        }
        textureType = (TextureType) EditorGUILayout.EnumPopup ("截图格式：", textureType);
        using (var scope = new EditorGUILayout.HorizontalScope ()) {
            EditorGUILayout.TextField ("截图存放位置：", outputFolder);
            if (GUILayout.Button ("...", GUILayout.Width (40))) {
                outputFolder = EditorUtility.OpenFolderPanel ("截图存放位置：", outputFolder, "ScreenCaptured");
            }
        }
        if (GUILayout.Button ("自由截屏")) {
            Capture ();
            Save (capturedRT);
        }

        GUILayout.Space(10);

        showTree = EditorGUILayout.Toggle ("showTree",showTree);
        showTerrain = EditorGUILayout.Toggle ("showTerrain",showTerrain);
        showOther = EditorGUILayout.Toggle ("showOther",showOther);
        GUILayout.Space(10);
        using (var scope = new EditorGUILayout.HorizontalScope ()) {
        showTag = EditorGUILayout.Toggle ("Tags",showTag);
        CaptureTag = EditorGUILayout.TextField("tag",CaptureTag);
        }
        
        if (GUILayout.Button ("高度")) {
            Shader myshader = Shader.Find ("UI/Default Grey");
            Camera cam = Camera.current;
            cam.SetReplacementShader (myshader, "IgnoreProjector");
            Capture ();
        }

        if (GUILayout.Button ("UI地图截屏")) {
            // string tag="Untagged";
            //List<GameObject> NoTrees = new List<GameObject> ();
            List<GameObject> TagGOs = new List<GameObject> ();
            List<GameObject> AllShowed = new List<GameObject> ();
            Renderer[] Renderers = FindObjectsOfType (typeof (Renderer)) as Renderer[];
            List<GameObject> Terrains = new List<GameObject>();
            List<GameObject> Trees = new List<GameObject>();
            List<GameObject> Others = new List<GameObject>();
            Terrain[] TerrainArr = FindObjectsOfType (typeof (Terrain)) as Terrain[];
            foreach (Terrain terrain in TerrainArr)
            {
                if(terrain.gameObject.activeSelf)
                {
                    Terrains.Add(terrain.gameObject);
                    AllShowed.Add(terrain.gameObject);
                }
                
            }
            
            bool[] ObjsActive = new bool[Renderers.Length];

            foreach (Renderer re in Renderers) 
            {
                if(re.gameObject.activeSelf)
                {
                    Material mat = re.sharedMaterial;
                    if(mat)
                    {
                        String treeMatName=mat.name.ToLower();
                        String treeGOName=re.gameObject.name.ToLower();
                        String treeMeshName="";
                        MeshFilter treeMeshFilter =re.gameObject.GetComponent<MeshFilter>();
                        if(treeMeshFilter)
                        {
                            Mesh treeMesh=treeMeshFilter.sharedMesh;
                            if(treeMesh)
                            {
                                treeMeshName=treeMesh.name.ToLower();
                            }  
                        }
                        
                        if(treeMatName.Contains ("tree")||treeGOName.Contains ("tree")||treeMeshName.Contains ("tree"))
                        {
                            Trees.Add(re.gameObject);
                        }
                        else
                        {
                            Others.Add(re.gameObject);
                        } 
                    }
                    
                    AllShowed.Add(re.gameObject);
                    if(re.gameObject.tag.Equals(CaptureTag))
                    TagGOs.Add(re.gameObject);

                }

            }
            
            foreach(GameObject g in AllShowed)
            {
                g.SetActive(false);
            }
            if (showTerrain)
            foreach(GameObject g in Terrains)
            {
                g.SetActive(true);
            }
            if (showTree)
            foreach(GameObject g in Trees)
            {
                g.SetActive(true);
            }
            if (showOther)
            foreach(GameObject g in Others)
            {
                g.SetActive(true);
            }
            if (showTag)
            foreach(GameObject g in TagGOs)
            {
                g.SetActive(true);
            }





            
            // foreach (Renderer re in Renderers) 
            // {
            //     Material mat = re.sharedMaterial;
            //     if (mat) {
            //         if (!mat.name.Contains ("tree")) {
            //             if (re.gameObject.activeSelf) {
            //                 re.gameObject.SetActive(false);
            //                 NoTrees.Add (re.gameObject);
            //             }
            //         }
            //     } else {
            //         if (re.gameObject.activeSelf) {
            //             re.gameObject.SetActive(false);
            //             NoTrees.Add (re.gameObject);
            //         }
            //     }

            // }

            
            // foreach (Terrain T in Terrains) {
            //     T.gameObject.SetActive(false) ;
            //     NoTrees.Add (T.gameObject);
            // }

            
            GameObject camGO = new GameObject ("camera");
            Camera ca = camGO.AddComponent<Camera> ();
            CaptureCustom (ca);
            Save(capturedRT);
            DestroyImmediate(camGO);
            foreach (GameObject x in AllShowed) {
                x.SetActive(true);
            }
        }

createdSphereScale=EditorGUILayout.FloatField("渲染点大小",createdSphereScale);
        using (var scope = new EditorGUILayout.HorizontalScope ()) {
            showClimbPoint00 = EditorGUILayout.Toggle ("攀爬中间点",showClimbPoint00);
            col00=EditorGUILayout.ColorField(col00);
        }
        using (var scope = new EditorGUILayout.HorizontalScope ()) {
            showClimbPoint01 = EditorGUILayout.Toggle ("攀爬入口",showClimbPoint01);
            col01=EditorGUILayout.ColorField(col01);
        }
        using (var scope = new EditorGUILayout.HorizontalScope ()) {
            showClimbPoint02 = EditorGUILayout.Toggle ("攀爬出口",showClimbPoint02);
            col02=EditorGUILayout.ColorField(col02);
        }
        using (var scope = new EditorGUILayout.HorizontalScope ()) {
            showClimbPoint03 = EditorGUILayout.Toggle ("攀爬混合口",showClimbPoint03);
            col03=EditorGUILayout.ColorField(col03);
        }
        using (var scope = new EditorGUILayout.HorizontalScope ()) {
            showClimbPoint19 = EditorGUILayout.Toggle ("攀爬自动入口",showClimbPoint19);
            col19=EditorGUILayout.ColorField(col19);
        }

        if (GUILayout.Button ("攀爬点截图")) {
            List<GameObject> ClimbPoint00List = new List<GameObject> ();
            List<GameObject> ClimbPoint01List = new List<GameObject> ();
            List<GameObject> ClimbPoint02List = new List<GameObject> ();
            List<GameObject> ClimbPoint03List = new List<GameObject> ();
            List<GameObject> ClimbPoint19List = new List<GameObject> ();
            GameObject[] AllClimbPoint =GameObject.FindGameObjectsWithTag("ClimbPoint");
            
            if(AllClimbPoint.Length>0)
            {
                foreach(GameObject g in AllClimbPoint)
                {
                    switch(g.name)
                    {
                        case "0":
                        ClimbPoint00List.Add(g);
                        break;
                        case "1":
                        ClimbPoint01List.Add(g);
                        break;
                        case "2":
                        ClimbPoint02List.Add(g);
                        break;
                        case "3":
                        ClimbPoint03List.Add(g);
                        break;
                        case "19":
                        ClimbPoint19List.Add(g);
                        break;

                        default:
                        break;
                    }    
                }
                GameObject climbpointGo=GameObject.Find("climbpoint");
                GameObject[] rootgos=UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects();
                bool[] rootsIsActive=new bool[rootgos.Length];
                for(int i=0;i<rootgos.Length;i++)
                {
                    rootsIsActive[i]= rootgos[i].activeSelf;
                    rootgos[i].SetActive(false);     
                }

                if(showClimbPoint00)
                foreach(GameObject g in ClimbPoint00List)
                {
                    CreateCimbpointSphere(col00,g.transform.position,createdSphereScale);
                }

                if(showClimbPoint01)
                foreach(GameObject g in ClimbPoint01List)
                {
                    CreateCimbpointSphere(col01,g.transform.position,createdSphereScale);
                }
                if(showClimbPoint02)
                foreach(GameObject g in ClimbPoint02List)
                {
                    CreateCimbpointSphere(col02,g.transform.position,createdSphereScale);
                }
                if(showClimbPoint03)
                foreach(GameObject g in ClimbPoint03List)
                {
                    CreateCimbpointSphere(col03,g.transform.position,createdSphereScale);
                }
                if(showClimbPoint19)
                foreach(GameObject g in ClimbPoint19List)
                {
                    CreateCimbpointSphere(col19,g.transform.position,createdSphereScale);
                }

                GameObject camGO = new GameObject ("camera");
                Camera ca = camGO.AddComponent<Camera> ();
                CaptureCustom (ca);
                Save (capturedRT);
                DestroyImmediate(camGO);

                foreach(GameObject g in AllCreatedSpheres)
                {
                    DestroyImmediate(g);
                }

                for(int i=0;i<rootgos.Length;i++)
                {
                    rootgos[i].SetActive(rootsIsActive[i]);   
                }  
            }
            else
            {
                 Debug.Log("场景中没有攀爬点");
            }
            







            // GameObject camGO = new GameObject ("camera");
            // Camera ca = camGO.AddComponent<Camera> ();
            // CaptureCustom (ca);
            // Save (capturedRT);
            // DestroyImmediate(camGO);

        }

        if (GUILayout.Button ("可行走区域截图")) {
            GameObject climbpointGo=GameObject.Find("GridRoot");
            GameObject[] rootgos=UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects();

            bool[] rootsIsActive=new bool[rootgos.Length];
        if(climbpointGo!=null)
        {
            for(int i=0;i<rootgos.Length;i++)
            {

                rootsIsActive[i]= rootgos[i].activeSelf; 

                if(!rootgos[i].name.Contains("GridRoot"))  
                rootgos[i].SetActive(false);  
                
                  
                
                 
            }
            //climbpointGo.SetActive(true);

                GameObject camGO = new GameObject ("camera");
                Camera ca = camGO.AddComponent<Camera> ();
                CaptureCustom (ca);
                Save (capturedRT);
                DestroyImmediate(camGO);

                foreach(GameObject g in AllCreatedSpheres)
                {
                    DestroyImmediate(g);
                }

            //     for(int i=0;i<rootgos.Length;i++)
            // {
            //     rootgos[i].SetActive(rootsIsActive[i]);   
            // }   
        }
        else
        {
            Debug.Log("先创建行走网格");
        }
            



        }

        // EditorGUILayout.LabelField ("历史截图:");

        // using (var scope = new EditorGUILayout.HorizontalScope ()) {
        //     if (GUILayout.Button ("保存到磁盘")) {
        //         Save (capturedRT);
        //     }
        //     EditorGUILayout.ObjectField (capturedRT, typeof (RenderTexture), true);
        //     if (capturedRT != null) {
        //         switch (capturedRT.format) {
        //             case RenderTextureFormat.ARGB32:
        //                 EditorGUILayout.LabelField ("PNG");
        //                 break;
        //             case RenderTextureFormat.ARGBFloat:
        //                 EditorGUILayout.LabelField ("EXR");
        //                 break;
        //         }
        //     } else {
        //         EditorGUILayout.LabelField ("   ");
        //     }
        // }
        // foreach (RenderTexture rt in capturedRTHistory) {
        //     using (var scope = new EditorGUILayout.HorizontalScope ()) {
        //         if (GUILayout.Button ("保存到磁盘")) {
        //             Save (rt);
        //         }
        //         EditorGUILayout.ObjectField (rt, typeof (RenderTexture), true);

        //         if (rt != null) {
        //             switch (rt.format) {
        //                 case RenderTextureFormat.ARGB32:
        //                     EditorGUILayout.LabelField ("PNG");
        //                     break;
        //                 case RenderTextureFormat.ARGBFloat:
        //                     EditorGUILayout.LabelField ("EXR");
        //                     break;
        //             }
        //         } else {
        //             EditorGUILayout.LabelField ("   ");
        //         }
        //     }
        // }
    }


public void CreateCimbpointSphere(Color col,Vector3 pos,float scale)
{
    GameObject createdSphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
    createdSphere.transform.position=pos;
    createdSphere.transform.localScale=new Vector3(scale,scale,scale) ;
    Material Mat=new Material(Shader.Find("Unlit/Color"));
    Mat.SetColor("_Color",col);
    createdSphere.GetComponent<Renderer>().sharedMaterial=Mat;
    AllCreatedSpheres.Add(createdSphere);
}

    public SceneConfig getSceneConfig () {
        SceneConfig SceneInfo = null;
        SceneContext sceneContext = new SceneContext();
        SceneAssets.GetCurrentSceneContext (ref sceneContext);
        if (SceneInfo == null) 
        {
            string sceneConfigPath = string.Format ("{0}/{1}{2}.asset",
                sceneContext.configDir, sceneContext.name, SceneContext.SceneConfigSuffix);

            SceneInfo = AssetDatabase.LoadAssetAtPath<SceneConfig> (sceneConfigPath);
        }
        return SceneInfo;
    }
    public void Save (RenderTexture rt) {
        if (rt == null) {
            return;
        }
        RenderTexture oldrt = RenderTexture.active;
        RenderTexture.active = rt;
        Texture2D t = null;
        switch (rt.format) {
            case RenderTextureFormat.ARGB32:
                t = new Texture2D (rt.width, rt.height, TextureFormat.RGB24, false, false);
                break;
            case RenderTextureFormat.ARGBFloat:
                t = new Texture2D (rt.width, rt.height, TextureFormat.RGBAFloat, false, true);
                break;
        }
        t.ReadPixels (new Rect (0, 0, rt.width, rt.height), 0, 0);
        t.Apply ();
        RenderTexture.active = oldrt;

        if (!Directory.Exists (outputFolder)) {
            Directory.CreateDirectory (outputFolder);
        }

        switch (rt.format) {
            case RenderTextureFormat.ARGB32:
                {
                    string filename = Path.Combine (outputFolder, rt.name + ".png");
                    byte[] bytes = t.EncodeToPNG ();
                    FileStream fs = File.OpenWrite (filename);
                    fs.Write (bytes, 0, bytes.Length);
                    fs.Close ();
                }
                break;
            case RenderTextureFormat.ARGBFloat:
                {
                    string filename = Path.Combine (outputFolder, rt.name + ".exr");
                    byte[] bytes = t.EncodeToEXR ();
                    FileStream fs = File.OpenWrite (filename);
                    fs.Write (bytes, 0, bytes.Length);
                    fs.Close ();
                }
                break;
        }

        AssetDatabase.Refresh ();
    }

    public void Capture () {
        // if (capturedRT != null) {
        //     capturedRTHistory.Push (capturedRT);
        // }

        Vector2 resolution = GetMainGameViewResolution ();
        Vector2 captureTextureSize;
        captureTextureSize.x = resolution.x * captureTextureRadio;
        captureTextureSize.y = resolution.y * captureTextureRadio;
        


        switch (textureType) {
            case TextureType.PNG:
                capturedRT = new RenderTexture ((int) captureTextureSize.x, (int) captureTextureSize.y, 24, RenderTextureFormat.ARGB32, RenderTextureReadWrite.sRGB);
                break;
            case TextureType.EXR:
                capturedRT = new RenderTexture ((int) captureTextureSize.x, (int) captureTextureSize.y, 24, RenderTextureFormat.ARGBFloat, RenderTextureReadWrite.Linear);
                break;
        }
        capturedRT.name = "Captured_" + capturedRT.width + "x" + capturedRT.height + "_" + DateTime.Now.ToString ("yyyyMMddhmmssfff");
        capturedRT.autoGenerateMips = false;
        capturedRT.Create ();

        Camera[] cameras = Camera.allCameras;
        cameras = cameras.Where (c => c.isActiveAndEnabled).ToArray ();
        cameras = cameras.Where (c => c.targetTexture == null).ToArray ();
        cameras = cameras.OrderBy (c => c.depth).ToArray ();

        foreach (Camera camera in cameras) {
            camera.targetTexture = capturedRT;
            camera.Render ();
            camera.targetTexture = null;
        }
    }

    public void CaptureCustom (Camera Cam) {


        
        // if (capturedRT != null) {
        //     capturedRTHistory.Push (capturedRT);
        // }
    
        // Camera[] allCam= Camera.allCameras;
        // List<RenderingEnvironment> envCameras=new List<RenderingEnvironment>();
        Vector4 fogPara=Shader.GetGlobalVector("_HeightFogParam");
        Shader.SetGlobalVector("_HeightFogParam",new Vector4(0,0,0,0));
        // foreach (Camera ca in allCam)
        // {
 
        //    RenderingEnvironment RE=ca.gameObject.GetComponent<RenderingEnvironment>();

        //    if(RE)
        //    {
               
        //        if(RE.fogEnable==true)
        //        {
        //         Debug.Log(RE.gameObject.name);
        //         RE.fogEnable=false;
        //         envCameras.Add(RE);
        //        }
             
        //    }
        
        // }

        var sceneConfig = getSceneConfig();
        float OneUnitW = sceneConfig.widthCount*EngineContext.ChunkSize*2.56f;
        float OneUnitH = sceneConfig.widthCount*EngineContext.ChunkSize*2.56f;
        Vector2 captureTextureSize;
        captureTextureSize.x = OneUnitW * captureTextureRadio;
        captureTextureSize.y = OneUnitH * captureTextureRadio;
        switch (textureType) {
            case TextureType.PNG:
                capturedRT = new RenderTexture ((int) captureTextureSize.x, (int) captureTextureSize.y, 24, RenderTextureFormat.ARGB32, RenderTextureReadWrite.sRGB);
                break;
            case TextureType.EXR:
                capturedRT = new RenderTexture ((int) captureTextureSize.x, (int) captureTextureSize.y, 24, RenderTextureFormat.ARGBFloat, RenderTextureReadWrite.Linear);
                break;
        }
        capturedRT.name = "Captured_" + capturedRT.width + "x" + capturedRT.height + "_" + DateTime.Now.ToString ("yyyyMMddhmmssfff");
        capturedRT.autoGenerateMips = false;
        capturedRT.Create ();

        // Camera[] cameras = Camera.allCameras;
        // cameras = cameras.Where(c => c.isActiveAndEnabled).ToArray();
        // cameras = cameras.Where(c => c.targetTexture == null).ToArray();
        // cameras = cameras.OrderBy(c => c.depth).ToArray();

        if (Cam.isActiveAndEnabled || Cam.targetTexture == null) {
            Cam.targetTexture = capturedRT;
            
            Cam.transform.position = new Vector3 (sceneConfig.widthCount*EngineContext.ChunkSize*0.5f,500,
            sceneConfig.heightCount*EngineContext.ChunkSize*0.5f);
            Cam.transform.rotation = Quaternion.Euler(new Vector3(90,0,0));
            Cam.orthographic=true;
            Cam.clearFlags=CameraClearFlags.Color;
            Cam.backgroundColor=Color.black;
            Cam.orthographicSize= Mathf.Max(sceneConfig.widthCount*EngineContext.ChunkSize*0.5f,sceneConfig.heightCount*EngineContext.ChunkSize*0.5f);
            Cam.aspect = (float)sceneConfig.widthCount/sceneConfig.heightCount;
            Cam.Render ();
            Cam.targetTexture = null;
        }
        Shader.SetGlobalVector("_HeightFogParam",fogPara);

    }
}
