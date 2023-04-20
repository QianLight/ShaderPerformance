using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

using System.IO;

class cubeParam
{
    public string name;
    public TextureImporterShape textureImporterShape;
    public bool isReadable;

    public TextureImporterPlatformSettings AndroidSettings;

    public TextureImporterPlatformSettings IOSSettings;

    public TextureImporterPlatformSettings PCSettings;


}

public class HDR_Clamp 
{
    // Start is called before the first frame update
    public static Shader sha =Shader.Find("ArtTools/HDR_clamp");
    public static Material mat ;
    public static RenderTexture sourceRT;
    public static RenderTexture destRT;
    public static RenderTexture previewRT=new RenderTexture(256,256,24,RenderTextureFormat.ARGBFloat);
    //public static Texture2D source=new Texture2D(1,1);
    public static float clampMax=500;
    public static float clampMin=1;
    public static float clampScale=1;
    public static string outputPath ;
    public static bool isHDR = false;
    private static cubeParam cubeP=new cubeParam();
    public static void Load()
    {

    //     string path = AssetDatabase.GetAssetPath(Selection.activeInstanceID);
    //     Debug.Log(path);

    //     //设置原图可编辑（RGBA） xxx.png
    //     TextureImporter importer = TextureImporter.GetAtPath(path) as TextureImporter;
    //     importer.isReadable = true;
    //     importer.textureShape=TextureImporterShape.Texture2D;
    //     importer.SaveAndReimport();

    //     //读取原图（RGBA）
    //     Texture2D source = AssetDatabase.LoadAssetAtPath<Texture>(path) as Texture2D;
    //    // Cubemap source_Cubemap = AssetDatabase.LoadAssetAtPath<Cubemap>(path) as Cubemap;
    //     float sizeScale = 1;
    //     Debug.Log(source);

    //     Texture2D rgbTex = new Texture2D(source.width, source.height, TextureFormat.RGBAHalf, true);
    //     //Texture2D alphaTex = new Texture2D((int)(source.width * sizeScale), (int)(source.height * sizeScale), TextureFormat.RGB24, true);
    //     Color[] rgbColors = new Color[source.width * source.height];
    //     //Color[] alphaColors = new Color[source.width * source.height];
    //     for (int i = 0; i < source.width; ++i)
    //     {
    //         for (int j = 0; j < source.height; ++j)
    //         {
    //             Color color = source.GetPixel(i, j);
    //             Color rgbColor = color;

    //             rgbColors[source.width * j + i] = color;

    //         }
    //     }
    //     rgbTex.SetPixels(rgbColors);
    //     rgbTex.Apply();

    //     //生成分离图片RGB + Alpha
    //     byte[] bytes = rgbTex.EncodeToEXR();
    //     File.WriteAllBytes(path, bytes);
    //     AssetDatabase.ImportAsset(path);
    //     AssetDatabase.Refresh();

    //     AssetDatabase.Refresh();
    //     Debug.Log("finish");
    /// <summary>
    /// OnRenderImage is called after all rendering is complete to render image.
    /// </summary>
    /// <param name="src">The source RenderTexture.</param>
    /// <param name="dest">The destination RenderTexture.</param>
    /// 
    /// 
    /// 
    /// 
    mat =new Material(sha);
    clampMax=500;
    clampMin=1;
    clampScale=1;

    string path0 = AssetDatabase.GetAssetPath(Selection.activeInstanceID);
    outputPath=path0.Split('.')[0];

    
    if(path0!=null)
    {
        Texture T =AssetDatabase.LoadAssetAtPath<Texture>(path0);
        if(T)
        {
            // if(T.GetType().Equals(typeof(Texture2D)))
            // {
            //     TextureImporter importer = TextureImporter.GetAtPath(path0) as TextureImporter;
            //     collectCubeData(importer);


            //     creatRT( path0);


            //     setRT();
            //     //Graphics.Blit(dest,previewRT);

                
            // }
            // else if(T.GetType().Equals(typeof(Cubemap)))
            // {
                TextureImporter importer = TextureImporter.GetAtPath(path0) as TextureImporter;
                collectCubeData(importer);
                TextureImporterPlatformSettings TIS_PC= importer.GetPlatformTextureSettings("PC");
                TextureImporterPlatformSettings TIS_IOS= importer.GetPlatformTextureSettings("iPhone");
                TextureImporterPlatformSettings TIS_Android= importer.GetPlatformTextureSettings("Android");



                importer.textureShape=TextureImporterShape.Texture2D;

                int PCTexSize=TIS_PC.maxTextureSize;
                int IOSTexSize=TIS_IOS.maxTextureSize;
                int AndroidTexSize=TIS_Android.maxTextureSize;
                TIS_PC.maxTextureSize=2048;
                TIS_IOS.maxTextureSize=2048;
                TIS_Android.maxTextureSize=2048;
                

                //TIS.format=TextureImporterFormat.BC6H;
                //TIS.crunchedCompression=false;
                importer.SetPlatformTextureSettings(TIS_PC);
                importer.SetPlatformTextureSettings(TIS_IOS);
                importer.SetPlatformTextureSettings(TIS_Android);
                

                importer.SaveAndReimport();
                AssetDatabase.Refresh ();

                creatRT( path0);

                setRT();

                //Graphics.Blit(dest,previewRT);
                //importer.isReadable = cubeP.isReadable;

                TIS_PC.maxTextureSize =PCTexSize;
                TIS_IOS.maxTextureSize =IOSTexSize;
                TIS_Android.maxTextureSize =AndroidTexSize;
                importer.SetPlatformTextureSettings(TIS_PC);
                importer.SetPlatformTextureSettings(TIS_IOS);
                importer.SetPlatformTextureSettings(TIS_Android);

                
                importer.textureShape=cubeP.textureImporterShape;
                importer.SaveAndReimport();
            // }
        }
        AssetDatabase.Refresh ();
    }

    
    }
    public static void Clamp()
    {
        setRT();
    }

        public static void Save()
    {
        if (destRT == null) {
            return;
        }
        RenderTexture oldrt = RenderTexture.active;
        RenderTexture.active = destRT;
        Texture2D t = null;
        switch (destRT.format) {
            case RenderTextureFormat.ARGB32:
                t = new Texture2D (destRT.width, destRT.height, TextureFormat.RGB24, false, false);
                break;
            case RenderTextureFormat.ARGBFloat:
                t = new Texture2D (destRT.width, destRT.height, TextureFormat.RGBAFloat, false, true);
                break;
        }
        t.ReadPixels (new Rect (0, 0, destRT.width, destRT.height), 0, 0);
        t.Apply ();
        RenderTexture.active = oldrt;



        string filename="";


        switch (destRT.format) {
            case RenderTextureFormat.ARGB32:
                {
                    filename = outputPath+ ".png";

                    if(Directory.Exists(Path.GetDirectoryName(filename)))
                    {
                        byte[] bytes = t.EncodeToPNG ();
                        FileStream fs = File.OpenWrite (filename);
                        fs.Write (bytes, 0, bytes.Length);
                        fs.Close ();
                    }
                    else
                    {
                        UnityEditor.EditorUtility.DisplayDialog("温馨提示","你要保存的路径不对","对不起，我错了");
                    }
                }
                break;
            case RenderTextureFormat.ARGBFloat:
                {
                    filename = outputPath+".exr";

                    if(Directory.Exists(Path.GetDirectoryName(filename)))
                    {
                        byte[] bytes = t.EncodeToEXR ();
                        FileStream fs = File.OpenWrite (filename);
                        fs.Write (bytes, 0, bytes.Length);
                        fs.Close ();
                        
                    }
                    else
                    {
                        UnityEditor.EditorUtility.DisplayDialog("温馨提示","你要保存的路径不对","对不起，我错了");
                    }
                }
    
                break;
        }
        AssetDatabase.Refresh ();

        TextureImporter importer = TextureImporter.GetAtPath(filename) as TextureImporter;
        importer.streamingMipmaps=true;
        importer.textureShape=TextureImporterShape.TextureCube;
        importer.SaveAndReimport();
        importer.SetPlatformTextureSettings(cubeP.AndroidSettings);
        importer.SetPlatformTextureSettings(cubeP.IOSSettings);
        importer.SetPlatformTextureSettings(cubeP.PCSettings);

        AssetDatabase.Refresh ();
    }

    public static void collectCubeData(TextureImporter myImporter)
    {
        cubeP.textureImporterShape=myImporter.textureShape;
        cubeP.isReadable=myImporter.isReadable;
        cubeP.name = myImporter.name;

        cubeP.AndroidSettings= myImporter.GetPlatformTextureSettings("Android");
        cubeP.IOSSettings= myImporter.GetPlatformTextureSettings("iPhone");
        cubeP.PCSettings= myImporter.GetPlatformTextureSettings("PC");
    }
    public static void creatRT(string mPath)
    {

        Texture2D source = AssetDatabase.LoadAssetAtPath<Texture>(mPath) as Texture2D;
        string extension =Path.GetExtension(mPath);
        if(extension.Equals(".exr") || extension.Equals(".hdr") )
        {
            sourceRT=new RenderTexture(source.width,source.height,24,RenderTextureFormat.ARGBFloat);
            destRT=new RenderTexture(source.width,source.height,24,RenderTextureFormat.ARGBFloat);
        }
        else
        {
            sourceRT=new RenderTexture(source.width,source.height,24,RenderTextureFormat.ARGB32);
            destRT=new RenderTexture(source.width,source.height,24,RenderTextureFormat.ARGB32);
        }
        Graphics.Blit(source,sourceRT);
        Debug.Log(source.width+"-"+source.height);
    }
    public static void setRT()
    {
        Vector4 ClampVec=new Vector4(clampMin,clampMax,clampScale,0);
        mat.SetVector("_clampVec",ClampVec);
        Graphics.Blit(sourceRT,destRT,mat);
    }


}
