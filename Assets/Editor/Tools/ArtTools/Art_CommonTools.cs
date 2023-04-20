
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using CFEngine.Editor;
using UnityEditor;
using UnityEngine;
public class vaildateRangeDate
{
    //public vaildateRangeDate(string Name,float value){}
    public string Name;
    public float value;

    public vaildateRangeDate (string Name, float value)
    {
        this.Name = Name;
        this.value = value;
    }

}

public class Art_CommonTools : ArtToolsTemplate
{
    enum ValidationMode
    {
        Albedo,
        Metal,
        Roughness,
        Combined
    }
    enum AlbedoRange
    {
        sRGB_50,
        sRGB_30
    }
    enum MetalRange
    {
        Reflectance70_100,
        Reflectance60_100
    }
    enum OverlayEnum
    {
        Off,
        On
    }

    enum DataRangeType
    {
        Default,
        Scene,
        Character
    }

    enum CheckVertexType
    {
        Color=0,
        UV=1,
        UV2=2,
        UV3=3,
        UV4=4,
        Normal=5,
        Tangent=6
    }

    //public Material[] materials;
    //private object[] objects;
    private List<Renderer> listRenderer = new List<Renderer> ();
    private List<Material> listMat = new List<Material> ();
    private List<Material> listVaMat = new List<Material> ();
    private Shader myShader;
    //private Shader OrShader;
    private GameObject[] myGOs;
    //private Renderer[] myRenderers;
    private bool IsButtonDown = false;
    ValidationMode VMode = ValidationMode.Combined;
    AlbedoRange ARange = AlbedoRange.sRGB_50;
    MetalRange MRange = MetalRange.Reflectance60_100;
    OverlayEnum OE = OverlayEnum.Off;
    DataRangeType dateRangeT = DataRangeType.Default;
    CheckVertexType checkVertexData =CheckVertexType.Color;

    String[] validShaders = new String[]
    {
        "Custom/Scene/UberDynamic",
        "Custom/Scene/Uber",
        "Custom/Role/Cartoon",
        "Custom/Role/Uber",
        "Custom/Editor/Scene/Uber"
    };
    List<List<vaildateRangeDate>> LListVRDs;
    private String LabelStr = "";
    private bool ShowCheckMatData = false;
    private Shader sha;
    private Material mat;
    private Vector2 scroll = new Vector2 ();
    private Texture2D CutT = null;
    private RenderTexture CutRT0=null;
    private RenderTexture CutRT1=null;
    private Shader CutShader=null;
    private GameObject CutGo = null;
    
    private List<RenderTexture> RTs = new List<RenderTexture> ();
    private Vector2Int hv = new Vector2Int ();
    private Vector3 boundMin = new Vector3 ();
    private Vector3 boundMax = new Vector3 ();
    private string TexPath = "";
    private int GUIBoxWidth=0;
    private int GUIBoxHeight=0;
    private Texture TexData=null;
    //private String TexVarName="";
    private Vector4 VarParam=new Vector4(0,0,0,0);
    //private Material testGO=null;
    enum TextureSize
    {
        _32=32,
        _64=64,
        _128=128,
        _256=256,
        _512=512,
        _1024=1024
    }
    private TextureSize textureSize=TextureSize._256;

    public override void OnEnable ()
    {
        sha = Shader.Find ("ArtTools/HDR_clamp");
        mat = new Material (sha);

        // if(VRDs.Count<1)
        // {
        //     VRDs=new List<vaildateRangeDate>{
        //         new vaildateRangeDate("_AlbedoMin",0.941f),
        //         new vaildateRangeDate("_AlbedoMax",1.0f),
        //         new vaildateRangeDate("_MetalMin",0.9f),
        //         new vaildateRangeDate("_MetalMax",0.96f),
        //         new vaildateRangeDate("_RoughLowMin",0.0f),
        //         new vaildateRangeDate("_RoughLowMax",0.9f),
        //         new vaildateRangeDate("_RoughHighMin",0.96f),
        //         new vaildateRangeDate("_RoughHighMax",1.0f)
        //     };
        // }

    }

    public override void OnGUI ()
    {

        if (LListVRDs == null)
        {
            LListVRDs = new List<List<vaildateRangeDate>> ();

            List<vaildateRangeDate> VRDs;
            List<vaildateRangeDate> VRDs_scene;
            List<vaildateRangeDate> VRDs_character;

            LListVRDs.Add (VRDs = new List<vaildateRangeDate>
            {
                new vaildateRangeDate ("_AlbedoMin", 0.941f),
                new vaildateRangeDate ("_AlbedoMax", 1.0f),
                new vaildateRangeDate ("_MetalMin", 0.9f),
                new vaildateRangeDate ("_MetalMax", 0.96f),
                new vaildateRangeDate ("_RoughLowMin", 0.0f),
                new vaildateRangeDate ("_RoughLowMax", 0.05f),
                new vaildateRangeDate ("_RoughHighMin", 0.95f),
                new vaildateRangeDate ("_RoughHighMax", 1.0f)
            });
            LListVRDs.Add (VRDs_scene = new List<vaildateRangeDate>
            {
                new vaildateRangeDate ("_AlbedoMin", 0.942f),
                new vaildateRangeDate ("_AlbedoMax", 1.0f),
                new vaildateRangeDate ("_MetalMin", 0.9f),
                new vaildateRangeDate ("_MetalMax", 0.96f),
                new vaildateRangeDate ("_RoughLowMin", 0.0f),
                new vaildateRangeDate ("_RoughLowMax", 0.25f),
                new vaildateRangeDate ("_RoughHighMin", 0.95f),
                new vaildateRangeDate ("_RoughHighMax", 1.0f)
            });
            LListVRDs.Add (VRDs_character = new List<vaildateRangeDate>
            {
                new vaildateRangeDate ("_AlbedoMin", 0.943f),
                new vaildateRangeDate ("_AlbedoMax", 1.0f),
                new vaildateRangeDate ("_MetalMin", 0.9f),
                new vaildateRangeDate ("_MetalMax", 0.96f),
                new vaildateRangeDate ("_RoughLowMin", 0.0f),
                new vaildateRangeDate ("_RoughLowMax", 0.05f),
                new vaildateRangeDate ("_RoughHighMin", 0.85f),
                new vaildateRangeDate ("_RoughHighMax", 1.0f)
            });
        }

        scroll = GUILayout.BeginScrollView (scroll);

        EditorGUILayout.Space ();

        GUILayout.Label ("-------------------------------GM指令配置------------------------------------------");

        GUILayout.BeginHorizontal ();

        if (GUILayout.Button ("打开GM指令表", GUILayout.MaxWidth (100)))
        {
            String path = Application.persistentDataPath + "/GM.txt";
            bool exists = File.Exists (path);
            if (!exists)
            {
                StreamWriter sw = new StreamWriter (path, true);
                //FileStream fs = new FileStream(path, FileMode.Create);
                List<string> strListToWrite = new List<string> ();
                strListToWrite.Add ("# GM指令写法‘$’加数字是快捷键 下一行是输入的指令 ‘#’是注释文字 ，使用时‘~’+数字键组合按下  下面是范例");
                strListToWrite.Add ("     ");
                strListToWrite.Add ("#加载主城");
                strListToWrite.Add ("$1");
                strListToWrite.Add ("xscene 1");
                for (int i = 0; i < strListToWrite.Count; i++)
                {
                    sw.WriteLine (strListToWrite[i]);
                }

                sw.Close ();
            }
            Process.Start (path);
        }
        GUILayout.EndHorizontal ();

        GUILayout.Label ("-------------------------------PBR材质验证------------------------------------------");

        GUILayout.BeginHorizontal ();
        //GUILayout.Label("");
        if (!IsButtonDown)
        {
            if (GUILayout.Button ("替换验证材质", GUILayout.Width (100)))
            {
                listRenderer = new List<Renderer> ();
                listMat = new List<Material> ();
                listVaMat = new List<Material> ();
                myGOs = null;
                myGOs = Selection.gameObjects;
                LabelStr = "";
                //myRenderers=null;
                foreach (GameObject go in myGOs)
                {
                    Renderer[] myRenderers;
                    if (go)
                    {
                        myRenderers = go.GetComponentsInChildren<Renderer> ();
                        foreach (Renderer R in myRenderers)
                        {

                            if (R.sharedMaterial)
                            {
                                String SName = R.sharedMaterial.shader.name;
                                bool isValidShader = false;

                                foreach (String s in validShaders)
                                {
                                    if (s.Equals (SName))
                                    {
                                        isValidShader = true;
                                    }
                                }
                                if (isValidShader)
                                {
                                    listMat.Add (R.sharedMaterial);
                                    listRenderer.Add (R);
                                }

                            }
                        }
                    }

                }
                myShader = Shader.Find ("pbr_validate_1");
                foreach (Renderer r in listRenderer)
                {
                    Material tempMat = new Material (r.sharedMaterial);
                    tempMat.shader = Shader.Find ("pbr_validate_1");
                    tempMat.SetTexture ("_pbsTex", r.sharedMaterial.GetTexture ("_ProcedureTex0"));
                    r.sharedMaterial = tempMat;
                    listVaMat.Add (tempMat);
                }

                if (listRenderer.Count < 1)
                {
                    LabelStr = "未选中有效物件";
                }
                else
                {
                    IsButtonDown = true;
                }
            }

            GUILayout.Label (LabelStr);
        }
        else
        {
            if (GUILayout.Button ("恢复原始材质", GUILayout.Width (100)))
            {
                for (int i = 0; i < listRenderer.Count; i++)
                {
                    if (listRenderer[i])
                    {
                        listRenderer[i].sharedMaterial = listMat[i];
                    }
                }
                IsButtonDown = false;
            }
            EditorGUI.BeginChangeCheck ();
            VMode = (ValidationMode) EditorGUILayout.EnumPopup (VMode);
            ARange = (AlbedoRange) EditorGUILayout.EnumPopup (ARange);
            MRange = (MetalRange) EditorGUILayout.EnumPopup (MRange);
            OE = (OverlayEnum) EditorGUILayout.EnumPopup (OE);
            foreach (Material m in listVaMat)
            {
                m.SetFloat ("_mode", (int) VMode);
                m.SetFloat ("albedo_range", (int) ARange);
                m.SetFloat ("metal_reflectance_range", (int) MRange);
                m.SetFloat ("_overlay_maps", (int) OE);
            }
            EditorGUI.EndChangeCheck ();
        }

        GUILayout.BeginVertical ();

        ShowCheckMatData = EditorGUILayout.Foldout (ShowCheckMatData, "参数");
        if (ShowCheckMatData)
        {
            dateRangeT = (DataRangeType) EditorGUILayout.EnumPopup (dateRangeT, GUILayout.Width (200));

            foreach (vaildateRangeDate VRD in LListVRDs[(int) dateRangeT])
            {
                EditorGUILayout.FloatField (VRD.Name, VRD.value, GUILayout.Width (200));
                foreach (Material m in listVaMat)
                {
                    m.SetFloat (VRD.Name, VRD.value);
                }
            }
        }
        GUILayout.EndVertical ();
        GUILayout.EndHorizontal ();
        GUILayout.Label ("-------------------------------HDR cubemap查看------------------------------------------");

        if (GUILayout.Button ("HDR查看", GUILayout.Width (100)))
        {

            //Texture2D tex=
            Shader shader = Shader.Find ("HDR_check");
            Material tempMat = new Material (shader);

            List<Cubemap> ListTex = new List<Cubemap> ();
            object[] objs = Selection.objects;
            if (objs != null)
            {
                foreach (object obj in objs)
                {
                    if (obj.GetType ().Equals (typeof (Cubemap)))
                        ListTex.Add ((Cubemap) obj);
                }
                if (ListTex.Count > 0)
                {
                    tempMat.SetTexture ("_Cube0", ListTex[0]);
                    tempMat.SetFloat ("_sliptHeight", 0f);
                    if (ListTex.Count > 1)
                    {
                        tempMat.SetTexture ("_Cube1", ListTex[1]);
                        tempMat.SetFloat ("_sliptHeight", 0.5f);
                    }

                }
            }

            GameObject gO = GameObject.CreatePrimitive (PrimitiveType.Sphere);
            Vector3 CameraDir = SceneView.lastActiveSceneView.camera.transform.forward;
            Vector3 offsetPoint = new Vector3 (CameraDir.x * 2.5f, CameraDir.y * 2.5f, CameraDir.z * 2.5f);
            gO.transform.position = SceneView.lastActiveSceneView.camera.transform.position + offsetPoint;
            gO.GetComponent<Renderer> ().sharedMaterial = tempMat;

        }
        GUILayout.Label ("-------------------------------HDR贴图修改------------------------------------------");
        if (GUILayout.Button ("HDR_Load", GUILayout.Width (100)))
        {
            HDR_Clamp.Load ();
        }

        EditorGUI.BeginChangeCheck ();
        HDR_Clamp.clampMin = EditorGUILayout.FloatField ("clampMin", HDR_Clamp.clampMin, GUILayout.Width (200));
        HDR_Clamp.clampMax = EditorGUILayout.FloatField ("clampMax", HDR_Clamp.clampMax, GUILayout.Width (200));
        HDR_Clamp.clampScale = EditorGUILayout.FloatField ("clampScale", HDR_Clamp.clampScale, GUILayout.Width (200));
        if (EditorGUI.EndChangeCheck ())
        {
            HDR_Clamp.Clamp ();
        }
        if (HDR_Clamp.destRT)
        {
            int GUIBoxWidth = (int) (HDR_Clamp.destRT.width / HDR_Clamp.destRT.height) * 256;
            int GUIBoxHeight = 256;
            GUILayout.Box (HDR_Clamp.destRT, GUILayout.Width (GUIBoxWidth), GUILayout.Height (GUIBoxHeight));
        }
        else
        {
            GUILayout.Box (HDR_Clamp.destRT, GUILayout.Width (256), GUILayout.Height (256));
        }
        EditorGUILayout.BeginHorizontal ();

        if (GUILayout.Button ("HDR_Save", GUILayout.Width (100)))
        {
            HDR_Clamp.Save ();
        }
        HDR_Clamp.outputPath = EditorGUILayout.TextField (HDR_Clamp.outputPath);
        EditorGUILayout.EndHorizontal ();

        GUILayout.Label ("-------------------------------切割Lightmap------------------------------------------");

        CutGo = (GameObject) EditorGUILayout.ObjectField (CutGo, typeof (GameObject), true, GUILayout.MaxWidth (300));
        EditorGUI.BeginChangeCheck();
        CutT = (Texture2D) EditorGUILayout.ObjectField (CutT, typeof (Texture2D), false, GUILayout.MaxWidth (300));
        if(EditorGUI.EndChangeCheck())
        {
            if (CutT != null)
            { 
                if(CutShader==null)
                {
                    CutShader= Shader.Find ("Hidden/CutLightmap");
                }
                if(CutRT0==null) CutRT0 = new RenderTexture (CutT.width, CutT.height, 24);
                if(CutRT0==null) CutRT1 = new RenderTexture (CutT.width, CutT.height, 24);
                Graphics.Blit(CutT,CutRT0);
                TexPath = AssetDatabase.GetAssetPath (CutT);
                int maxH = Math.Min (CutT.height, 256);
                GUIBoxWidth = (int) (CutT.width / CutT.height) * maxH;
                GUIBoxHeight = maxH;
            }
        }
       
        
        GUILayout.BeginHorizontal();
        if (CutT != null)
        { 
            GUILayout.Box (CutRT0, GUILayout.Width (GUIBoxWidth), GUILayout.Height (GUIBoxHeight));
        }
        GUILayout.BeginVertical();
        if (GUILayout.Button ("顺时针90", GUILayout.Width (60)))
        {
            Material mymat = new Material (CutShader);
            mymat.SetFloat("_Angle",90*Mathf.Deg2Rad );
            //Graphics.Blit(CutRT0,CutRT1);
            mymat.SetTexture("_MTex",TextureToTexture2D(CutRT0));
            Graphics.Blit(CutRT0,CutRT0,mymat,1);
        }
        if (GUILayout.Button ("逆时针90", GUILayout.Width (60))) 
        {
            Material mymat = new Material (CutShader);
            mymat.SetFloat("_Angle",-90*Mathf.Deg2Rad );
           
            //Graphics.Blit(CutRT0,CutRT1);
            mymat.SetTexture("_MTex",TextureToTexture2D(CutRT0));
            //mymat.mainTexture=CutRT1;
            Graphics.Blit(CutRT0,CutRT0,mymat,1);
        }



        GUILayout.EndVertical();
        GUILayout.EndHorizontal();

        textureSize=(TextureSize) EditorGUILayout.EnumPopup (textureSize, GUILayout.Width (100));

        GUILayout.BeginHorizontal();
        

        if (GUILayout.Button ("切割Lightmap", GUILayout.MaxWidth (100)))
        {

            List<Vector2Int> chunkInt = new List<Vector2Int> ();
            if (CutT != null && CutGo != null)
            {
                // UnityEngine. Debug.Log((int)(Mathf.Ceil(2.35f)));
                RTs.Clear ();
                //Material mat =new Material(Shader.Find("Hidden/CutLightmap"));
                if (CutGo != null)
                {
                    int width = (int)textureSize;
                    int high =  (int)textureSize;

                    
                    //Debug.Log((int)(Mathf.Ceil(2.35f)));
                    GameObject go = CutGo;
                    MeshRenderer meshRenderer = go.GetComponent<MeshRenderer> ();
                    Mesh m = go.GetComponent<MeshFilter> ().sharedMesh;
                    //List<RenderTexture> RTs=new List<RenderTexture>();
                    boundMin = meshRenderer.bounds.min;
                    boundMax = meshRenderer.bounds.max;

                    //float BoundWidth=boundMax.x-boundMin.x;
                    //float BoundHigh=boundMax.z-boundMin.z;
                    Vector4 matParam0 = new Vector4 (0, 0, 0, 0);
                    matParam0.x = 128 / meshRenderer.bounds.size.x;
                    matParam0.y = 128 / meshRenderer.bounds.size.z;

                    //mat.SetVector("_Param0",matParam0);
                    //Vector3[] verts =m.vertices;
                    //////////////////////////计算模型的boundingbox所占用的chunk快/////////////////////
                    Vector2Int ChunkMin = new Vector2Int ((int) Mathf.Ceil (Mathf.Max(boundMin.x,0)  / 128), (int) Mathf.Ceil (Mathf.Max(boundMin.z,0) / 128));
                    Vector2Int ChunkMax = new Vector2Int ((int) Mathf.Ceil (Mathf.Max(boundMax.x,0) / 128), (int) Mathf.Ceil (Mathf.Max(boundMax.z,0) / 128));
                    hv.x = (int) (ChunkMax.x - ChunkMin.x);
                    hv.y = (int) (ChunkMax.y - ChunkMin.y);
                    for (int j = hv.y; j >= 0; j--)
                    {
                        for (int i = 0; i <= hv.x; i++)
                        {
                            Vector2Int vv = new Vector2Int (i + ChunkMin.x - 1, j + ChunkMin.y - 1);
                            if (vv.x >= 0 && vv.y >= 0)
                            {
                                chunkInt.Add (vv);
                            }
                        }
                    }

                    // for(int i=0 ; i<m.vertexCount ;i++)
                    // {
                    //     var v=go.transform.TransformPoint(verts[i]);
                    //     Vector2Int  vv=new Vector2Int((int)(v.x/128), (int)(v.z/128));

                    //     if(chunkInt.Count==0)
                    //     {
                    //         chunkInt.Add(vv);
                    //         //RenderTexture rt =new RenderTexture(width,high,24);
                    //         //Graphics.Blit(CutT,rt,mat);
                    //         //RTs.Add(rt);

                    //     }
                    //     else
                    //     {
                    //         bool IsN=true;
                    //         foreach(Vector2Int v2 in chunkInt)
                    //         {
                    //             if(vv==v2)
                    //             {
                    //                 IsN=false;
                    //             }
                    //         }

                    //         if(IsN)
                    //         {
                    //             chunkInt.Add(vv);
                    //             //RenderTexture rt =new RenderTexture(width,high,24);
                    //             //Graphics.Blit(CutT,rt,mat);
                    //             //RTs.Add(rt);
                    //         }
                    //     }

                    // }

                    foreach (Vector2Int vvv in chunkInt)
                    {
                        Material mymat = new Material (CutShader);
                        RenderTexture rt = new RenderTexture (width, high, 24);
                        matParam0.z = -boundMin.x / 128;
                        matParam0.w = -boundMin.z / 128;
                        if (vvv.x != 0)
                            matParam0.z += (float) (vvv.x);
                        if (vvv.y != 0)
                            matParam0.w += (float) (vvv.y);
                        matParam0.z *= matParam0.x;
                        matParam0.w *= matParam0.y;
                        mymat.SetVector ("_Param0", matParam0);
                        mymat.SetTexture("_MTex",TextureToTexture2D(CutRT0));
                        Graphics.Blit (CutRT0, rt, mymat,0);
                        rt.name = vvv.x.ToString () + "_" + vvv.y.ToString ();
                        RTs.Add (rt);
                        //UnityEngine.Debug.Log(matParam0);
                    }
                }
            }
        }

        GUILayout.EndHorizontal();

 
        if (RTs.Count > 0)
        {
            
            String start=RTs[0].name.Split('_')[1];
            EditorGUILayout.BeginHorizontal();
            for(int i=0;i<RTs.Count;i++)
            {
                int maxH=Math.Min( CutT.height,128);
                int GUIBoxWidth=(int)(CutT.width/CutT.height)*maxH;
                int GUIBoxHeight=maxH;
                
                String now=RTs[i].name.Split('_')[1];
                if(now.Equals(start))
                {
                    
                }
                else
                {
                    EditorGUILayout.EndHorizontal();
                    start=RTs[i].name.Split('_')[1];
                    EditorGUILayout.BeginHorizontal();
                    
                }
                //EditorGUILayout.BeginVertical();
                //GUILayout.Box(RTs[i],GUILayout.Width(GUIBoxWidth),GUILayout.Height(GUIBoxHeight));
                
                if (GUILayout.Button (RTs[i], GUILayout.Width (128), GUILayout.Height (128)))
                {
                    saveLightmap(i);
                }

                //EditorGUILayout.EndVertical();
            }
            EditorGUILayout.EndHorizontal();

            if (GUILayout.Button ("SaveAllLightmap", GUILayout.Width (150), GUILayout.Height (30)))
            {
                for(int i=0;i<RTs.Count;i++)
                {
                    saveLightmap(i);
                }
            }


/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////


            //for(int i=hv.y;i>=0;i--)
           // {
                //EditorGUILayout.BeginHorizontal();
                //for(int j=0;j<=hv.x;j++)
                //{
                    //int thisInt=(hv.x+1)*j+i;
                    // int maxH=Math.Min( CutT.height,128);
                    // int GUIBoxWidth=(int)(CutT.width/CutT.height)*maxH;
                    // int GUIBoxHeight=maxH;

                    //     GUILayout.Box(RTs[thisInt],GUILayout.Width(GUIBoxWidth),GUILayout.Height(GUIBoxHeight));

                        // GUILayout.Label(RTs[i].name,GUILayout.MaxWidth(100));
                        // Texture2D t = null;

                        // if (GUILayout.Button("保存", GUILayout.MaxWidth(100)))
                        // {
                        //     int width = RTs[(hv.x+1)*j+i].width;
                        //     int height = RTs[(hv.x+1)*j+i].height;
                        //     Texture2D texture2D = new Texture2D(width, height, TextureFormat.RGBAFloat , false, true);
                        //     RenderTexture.active = RTs[(hv.x+1)*j+i];
                        //     texture2D.ReadPixels(new Rect(0, 0, width, height), 0, 0);
                        //     texture2D.Apply();

                        //     byte[] bytes = texture2D.EncodeToEXR(Texture2D.EXRFlags.CompressZIP);

                        //     String FileName=  Path.GetDirectoryName(TexPath)+"\\"+"Lightmap_Terrain_Chunk_"+RTs[i].name+"-0"+".exr";
                        //     FileStream fs = File.OpenWrite (FileName);
                        //     fs.Write (bytes, 0, bytes.Length);
                        //     fs.Close ();
                        //     AssetDatabase.Refresh ();

                        // }



               // }
               // EditorGUILayout.EndHorizontal();
           // }

///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////


            // for (int i = 0; i < RTs.Count; i++)
            // {
            //     int maxH = Math.Min (CutT.height, 128);
            //     int GUIBoxWidth = (int) (CutT.width / CutT.height) * maxH;
            //     int GUIBoxHeight = maxH;

            //     EditorGUILayout.BeginHorizontal();

            //     GUILayout.Box (RTs[i], GUILayout.Width (GUIBoxWidth), GUILayout.Height (GUIBoxHeight));
            //     GUILayout.Label(RTs[i].name,GUILayout.MaxWidth(100));

            //     if (GUILayout.Button ("保存", GUILayout.MaxWidth (100)))
            //     {
            //         int width = RTs[i].width;
            //         int height = RTs[i].height;
            //         Texture2D texture2D = new Texture2D (width, height, TextureFormat.RGBAFloat, false, true);
            //         RenderTexture.active = RTs[i];
            //         texture2D.ReadPixels (new Rect (0, 0, width, height), 0, 0);
            //         texture2D.Apply ();

            //         byte[] bytes = texture2D.EncodeToEXR (Texture2D.EXRFlags.CompressZIP);

            //         String FileName = Path.GetDirectoryName (TexPath) + "\\" + "Lightmap_Terrain_Chunk_" + RTs[i].name + "-0" + ".exr";
            //         FileStream fs = File.OpenWrite (FileName);
            //         fs.Write (bytes, 0, bytes.Length);
            //         fs.Close ();
            //         AssetDatabase.Refresh ();

            //     }
            //     EditorGUILayout.EndHorizontal();
            // }
///////////////////////////////////////////////////////////////////////////
        }
        


        EditorGUILayout.Space ();
        GUILayout.Label ("-------------------------------setShaderGlobalData------------------------------------------");
        GUILayout.BeginHorizontal ();
        //TexVarName=EditorGUILayout.TextField(TexVarName);
        TexData = (Texture) EditorGUILayout.ObjectField ("_EnvCubeTest",TexData, typeof (Texture), true, GUILayout.MaxWidth (200));
        if (GUILayout.Button ("setData", GUILayout.MaxWidth (100)))
        {
            if(TexData!=null )
            {
                Shader.SetGlobalTexture("_EnvCubeTest",TexData);
            }
        }
        GUILayout.EndHorizontal ();

        EditorGUI.BeginChangeCheck ();
        VarParam=EditorGUILayout.Vector4Field("_testParam",VarParam);

       if( EditorGUI .EndChangeCheck())
       {
           Shader.SetGlobalVector("_testParam",VarParam);
           UnityEngine.Debug.Log(VarParam);
       }

        EditorGUILayout.Space();
        GUILayout.Label ("-------------------------------bakeVertexData------------------------------------------");
        checkVertexData = (CheckVertexType) EditorGUILayout.EnumPopup (checkVertexData, GUILayout.Width (200));
        //testGO = (Material) EditorGUILayout.ObjectField (testGO, typeof (Material), true, GUILayout.MaxWidth (200));
        if (GUILayout.Button ("bakeVertexData", GUILayout.MaxWidth (100)))
        {
            GameObject selectedGO=Selection.activeGameObject;
            if(selectedGO!=null)
            {
                MeshFilter meshFilter=selectedGO.GetComponent<MeshFilter>();
                SkinnedMeshRenderer skinnedMeshRenderer = selectedGO.GetComponent<SkinnedMeshRenderer>();
                if(meshFilter!=null || skinnedMeshRenderer!=null)
                {
                    Mesh mesh=null;
                    if(meshFilter!=null)
                        mesh=meshFilter.sharedMesh;
                    if(skinnedMeshRenderer!=null)
                        mesh=skinnedMeshRenderer.sharedMesh;

                    if(mesh!=null)
                    {
                        Shader checkVertexShader = Shader.Find("Art_Tools/BakeVertData"); 
                        if(checkVertexShader !=null)
                        { 
                            Material checkVertexMat=new Material(checkVertexShader);
                            if(checkVertexMat!=null )
                            {
                                    // uvMaterial.SetPass(0);
                                    // Graphics.DrawMeshNow(M,Matrix4x4.identity);
                                RenderTexture rt0 = RenderTexture.GetTemporary(512, 512,24);
                                // Mesh M = objectToBake.GetComponent<MeshFilter>().mesh;
                                Graphics.SetRenderTarget(rt0);
                                GL.Clear(true, true, Color.black);
                                GL.PushMatrix(); 
                                GL.LoadOrtho(); 
                                checkVertexMat.SetFloat("_channel",(int)checkVertexData);




                                checkVertexMat.SetPass(0);
                                Graphics.DrawMeshNow(mesh, Matrix4x4.identity);
                                Graphics.SetRenderTarget(null);
                                // RenderTexture rt2 = RenderTexture.GetTemporary(textureDim.x, textureDim.y);
                                // Graphics.Blit(rt, rt2, uvMaterial);
                                SaveTexture(rt0, mesh.name);
                                RenderTexture.ReleaseTemporary(rt0);
                                //RenderTexture.ReleaseTemporary(rt2);
                                GL.PopMatrix();



                            }
                        }

                        
                    }
                    

                }
            }
            
            

            
        }

        GUILayout.Space(128);
        GUILayout.EndScrollView();
    }
    void saveLightmap(int x)
    {
        //GUILayout.Label(RTs[x].name,GUILayout.MaxWidth(50));

        int width = RTs[x].width;
        int height = RTs[x].height;
        Texture2D texture2D = new Texture2D (width, height, TextureFormat.RGBAFloat, false, true);
        RenderTexture.active = RTs[x];
        texture2D.ReadPixels (new Rect (0, 0, width, height), 0, 0);
        texture2D.Apply ();

        byte[] bytes = texture2D.EncodeToEXR (Texture2D.EXRFlags.CompressZIP);

        String FileName = Path.GetDirectoryName (TexPath) + "/" + "Lightmap_Terrain_Chunk_" + RTs[x].name + "-0" + ".exr";

        string folderName ="SceneLightmapBackup";
        string bakePath = TexPath.Substring(0,TexPath.IndexOf(folderName)+folderName.Length);
        if(Directory.Exists(bakePath))
        {
            String FileFolder = bakePath + "/" + "Terrain_Chunk_" + RTs[x].name;
            if(!Directory.Exists(FileFolder))
            {
                Directory.CreateDirectory(FileFolder);
            }
            FileName=FileFolder + "/" + "Lightmap_Terrain_Chunk_" + RTs[x].name + "-0" + ".exr";
            UnityEngine.Debug.Log(FileName);
        }

        FileStream fs = File.OpenWrite (FileName);
        fs.Write (bytes, 0, bytes.Length);
        fs.Close ();
        AssetDatabase.Refresh ();
        TextureImporter textureImporter = null;
        textureImporter = (TextureImporter)AssetImporter.GetAtPath(FileName);
        textureImporter.textureType = TextureImporterType.Lightmap;
        textureImporter.SaveAndReimport();


        // EditorGUIUtility.PingObject(AssetDatabase.LoadAssetAtPath<UnityEngine.Texture>("FileName"));


    }

    private Texture2D TextureToTexture2D(Texture texture)
    {
        Texture2D texture2D = new Texture2D(texture.width, texture.height, TextureFormat.RGBA32, false);
        RenderTexture currentRT = RenderTexture.active;
        RenderTexture renderTexture = RenderTexture.GetTemporary(texture.width, texture.height, 32);
        Graphics.Blit(texture, renderTexture);

        RenderTexture.active = renderTexture;
        texture2D.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0);
        texture2D.Apply();

        RenderTexture.active = currentRT;
        RenderTexture.ReleaseTemporary(renderTexture);

        return texture2D;
    }

     private void SaveTexture(RenderTexture rt, string mname)
    {
        string fullPath = "Assets/" + mname + ".png";
        byte[] _bytes = toTexture2D(rt).EncodeToPNG();
        File.Delete(fullPath);
        File.WriteAllBytes(fullPath, _bytes);
        AssetDatabase.Refresh ();
        EditorGUIUtility.PingObject(AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(fullPath));

    }
    Texture2D toTexture2D(RenderTexture rTex)
    {
        Texture2D tex = new Texture2D(rTex.width, rTex.height, TextureFormat.RGB24, false);
        RenderTexture.active = rTex;
        tex.ReadPixels(new Rect(0, 0, rTex.width, rTex.height), 0, 0);
        tex.Apply();
        
        return tex;
    }

}