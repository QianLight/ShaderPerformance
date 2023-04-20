using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEditor.U2D;
using UnityEngine;
using UnityEngine.CFUI;
using UnityEngine.U2D;

class SelectionUtility
{
    #region const
    public static string m_prefab_url = "BundleRes/UI/OPsystemprefab";
    private static string m_sprite_src = "BundleRes/UI/UISource";
    private static string m_tex_src = "BundleRes/UIBackground";
    private static string m_atlas_src = "BundleRes/UI/atlas";
    private static string[] m_files;
    #endregion

    #region ShowDeptError
        
    [MenuItem("Assets/UI/Show Error Dependence")]
    public static void ShowErrorDependence()
    {
        string[] files = GetFiles(m_prefab_url, "*.prefab");
       
        foreach(string file in files)
        {
            string path = file.Substring(file.IndexOf("Assets/"));
            GameObject go = PrefabUtility.LoadPrefabContents(file);
            PrefabUtility.UnloadPrefabContents(go);
        }
    }

    #endregion

    #region PackAtlas
    [MenuItem("Tools/UI/PackAllAtlas")]
    public static void PackAtlas()
    {
        SpriteAtlasUtility.PackAllAtlases(BuildTarget.StandaloneWindows);
    }
    
    [MenuItem("Assets/UI/FlushOrCreateAtlas")]
    public static void CreateOrFlushAtlas()
    {
        string[] selects = Selection.assetGUIDs;
        List<string> validList = new List<string>();
        List<SpriteAtlas> spriteAtlas = new List<SpriteAtlas>();
        SpriteAtlas atlas = null;
        foreach(string guid in selects)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            if (string.IsNullOrEmpty(path) || Path.HasExtension(path))
            {
                Debug.LogError("非文件夹路径:" + path);
                continue;
            }
            if (!path.Contains(m_sprite_src))
            {
                Debug.LogError("非Sprite路径:" + path);
                continue;
            }
            string[] files = GetFiles(path, "*.png", SearchOption.TopDirectoryOnly);
            if(files.Length == 0)
            {
                Debug.LogError("此目录不存在Sprite:" + path);
                continue;
            }
            TryGetOrCreateAtlas(path, ref atlas);
            spriteAtlas.Add(atlas);

        }

        try
        {
            SpriteAtlasUtility.PackAtlases(spriteAtlas.ToArray(), BuildTarget.StandaloneWindows, true);
            EditorUtility.DisplayDialog("Atlas", "Create Or Flush Sucess!", "确定");
        }
        catch (System.Exception e)
        {
            Debug.LogError("PackAtlas:" + e.Message);
        }
        AssetDatabase.SaveAssets();

    }

    private static string m_atlas_path_format = "Assets/BundleRes/UI/atlas/{0}.spriteatlas";
    private static void  TryGetOrCreateAtlas(string path , ref SpriteAtlas sa)
    {
        string filename = path.Substring(path.LastIndexOf('/') + 1);
        string atlas = string.Format(m_atlas_path_format, filename);
        Debug.Log("GetOrCreateAtlas:" + atlas);
        if (!File.Exists(atlas))
        {
            sa = new SpriteAtlas();
            SpriteAtlasPackingSettings packset = new SpriteAtlasPackingSettings
            {
                blockOffset = 1,
                enableRotation = true,
                enableTightPacking = false,
                padding = 4,

            };
            sa.SetPackingSettings(packset);

            SpriteAtlasTextureSettings textureSet = new SpriteAtlasTextureSettings
            {
                readable = true,
                generateMipMaps = false,
                sRGB = true,
                filterMode = FilterMode.Bilinear
            };
            sa.SetTextureSettings(textureSet);
            AssetDatabase.CreateAsset(sa, atlas);
            UnityEngine.Object ot = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(path);
            SpriteAtlasExtensions.Add(sa, new UnityEngine.Object[] { ot });
        }
        else
        {
            sa = AssetDatabase.LoadAssetAtPath<SpriteAtlas>(atlas);
        }
    }

    #endregion

    #region FlushGuid
    [MenuItem("Assets/UI/FlushGuid")]
    public static void FlushResource()
    {
        string[] guids = Selection.assetGUIDs;
        foreach(string guid in guids)
        {
           string path = AssetDatabase.GUIDToAssetPath(guid);
            if (path.Contains(m_prefab_url))
            {
                ParseFiles(path);
            }
            else
            {
                Debug.Log("非Prefab目录!" + path);
            }
        }
        AssetDatabase.SaveAssets();
        EditorUtility.DisplayDialog("FlushGuid", "Flush Sucess!", "确定");
    }

    private static void ParseFiles(string path)
    {
        if (!path.Contains("BundleRes/UI")) return;
        if (!Path.HasExtension(path)) //判断是文件夹
        {
            FlushFolder(path);
        }
        else if (path.EndsWith(".prefab")) //判断是Prefab
        {
            FlushPrefab(path);
        }
        else if (path.EndsWith(".png")) // 判断是图片
        {

        }
    }

    private static void FlushFolder(string path)
    {
        string[] files = GetFiles(path , "*.prefab");
        foreach(string file in files)
        {
            FlushPrefab(file.Substring(file.IndexOf("Assets/")));
        }
    }

    private static void FlushPrefab(string path)
    {
        GameObject go = PrefabUtility.LoadPrefabContents(path);
        if (go != null)
        {
            MaskableGraphic[] mgs = go.GetComponentsInChildren<MaskableGraphic>(true);
            bool result = false;
            foreach (MaskableGraphic mg in mgs)
            {
                if (mg is CFImage)
                {
                    CFImage img = mg as CFImage;
                    if (!string.IsNullOrEmpty(img.m_guid))
                    {
                        string iPath = AssetDatabase.GUIDToAssetPath(img.m_guid);
                        if (string.IsNullOrEmpty(iPath))
                        {
                            Debug.Log("资源不存在:"+img.m_guid);
                            //result |= SetImageDept(ref img, "", "");
                        }
                        else
                        {
                            string atlas = "", spritename = "";
                            GetFolderAndFileName(iPath.Replace(".png", ""), ref spritename, ref atlas);
                            result |= SetImageDept(ref img, atlas, spritename);
                        }
                    }

                }else if(mg is CFRawImage)
                {
                    CFRawImage rmg = mg as CFRawImage;
                    if(!string.IsNullOrEmpty(rmg.m_guid))
                    {
                        string rPath = AssetDatabase.GUIDToAssetPath(rmg.m_guid);
                        if (string.IsNullOrEmpty(rPath))
                        {
                            Debug.Log("资源不存在:" + rmg.m_guid);
                        }
                        else
                        {
                            string filename = string.Empty;
                            if (GetTextureFileName(rPath, ref filename))
                            {
                                result |= SetRmgDept(ref rmg, filename);
                            }
                            else
                            {
                                Debug.LogError("目录结构不正确：" + rPath);
                            }
                           
                        }
                    }
                }
            }

            if (result) PrefabUtility.SaveAsPrefabAsset(go, path);
        }
        PrefabUtility.UnloadPrefabContents(go);
    }

    private static bool SetImageDept(ref CFImage img , string atlas, string spritename)
    {
        if (img.m_AtlasName != atlas || img.m_SpriteName != spritename)
        {
            img.m_AtlasName = atlas;
            img.m_SpriteName = spritename;
            return true;
        }
        else
        {
            return false;
        }
    }

    private static bool SetRmgDept(ref CFRawImage rmg, string filename)
    {
        if(rmg.m_TexPath != filename)
        {
            rmg.m_TexPath = filename;
            return true;
        }
        return false;
    }


    
    [MenuItem("Tools/UI/FlushAllGuid")]
    public static void FlushAllPrefab()
    {
        string path = "/" + m_prefab_url;
        FlushFolder(path);
        EditorUtility.DisplayDialog("FlushAllGuid", "Flush Sucess!", "确定");
    }
  
    #endregion

    #region SaveGuid
    [MenuItem("Assets/UI/Save Guid")]
    public static void SaveGuid()
    {
       
        Dictionary<string, Dictionary<string, string>> nameToPath = null;
        Dictionary<string, string> texToPath = null;
        InitNameAtlasToPath(ref nameToPath);
        InitTextureToPath(ref texToPath);
        string path = Application.dataPath + "/" + m_prefab_url;
        m_files = Directory.GetFiles(path, "*.prefab", SearchOption.AllDirectories);
        foreach(string file in m_files)
        {
         
            string filepath = file.Substring(file.IndexOf("Assets/"));
            GameObject go = PrefabUtility.LoadPrefabContents(filepath);
            if (go == null) continue;
            if(SaveTransform(go.transform,nameToPath,texToPath))
            {
                PrefabUtility.SaveAsPrefabAsset(go, filepath);
                Debug.Log("Save Success!:" + filepath);
            }
            PrefabUtility.UnloadPrefabContents(go);
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        EditorUtility.DisplayDialog("Save Guid", "Save Sucess!","确定");

    }

    private static bool SaveTransform( Transform  transform, Dictionary<string, Dictionary<string, string>> nameToPath,Dictionary<string,string> texToGuid)
    {
        bool result = false;
        MaskableGraphic[] mgs = transform.GetComponentsInChildren<MaskableGraphic>(true);
        if (mgs.Length == 0) return result;
        Dictionary<string, string> m_guid;
        string m_guidStr = string.Empty;
        foreach (MaskableGraphic mg in mgs)
        {
            if (mg is CFImage)
            {
                CFImage src = mg as CFImage;
                if ( string.IsNullOrEmpty(src.m_AtlasName) || string.IsNullOrEmpty(src.m_SpriteName)) continue;
                if(nameToPath.TryGetValue(src.m_AtlasName,out m_guid))
                {
                    if (m_guid.ContainsKey(src.m_SpriteName))
                    {
                        if (!m_guid[src.m_SpriteName].Equals(src.m_guid))
                        {
                            src.m_guid = m_guid[src.m_SpriteName];
                            result |= true;
                        }
                    }
                }
            }

            else if(mg is CFRawImage)
            {
                CFRawImage src = mg as CFRawImage;
                if (string.IsNullOrEmpty(src.m_TexPath)) continue;
                if(texToGuid.TryGetValue(src.m_TexPath, out m_guidStr) && !m_guidStr.Equals(src.m_guid))
                {
                    src.m_guid = m_guidStr;
                    result |= true;
                }
            }
        }
        return result;
    }


    private static void InitNameAtlasToPath(ref Dictionary<string, Dictionary<string, string>> nameToPath )
    {
        nameToPath = new Dictionary<string, Dictionary<string, string>>();
        string path = Application.dataPath + "/" + m_sprite_src;
        string[] folders = Directory.GetFiles(path,"*.png", SearchOption.AllDirectories);
        string foldername = string.Empty, filename = string.Empty;
        foreach(string file in folders)
        {
            string filepath = file.Replace(path, "").Replace(".png","").Substring(1);
            if(GetFolderAndFileName(filepath, ref filename,ref foldername))
            {
                if (!nameToPath.ContainsKey(foldername))
                {
                    nameToPath.Add(foldername, new Dictionary<string, string>());
                }
                nameToPath[foldername].Add(filename, GetGUID(file));
            }
        }
    }

    private static void InitTextureToPath(ref Dictionary<string,string> nameToPath)
    {
        nameToPath = new Dictionary<string, string>();
        string path = Application.dataPath + "/" + m_tex_src;
        string[] folders = Directory.GetFiles(path, "*.png", SearchOption.AllDirectories);
        foreach(string file in folders)
        {
            string filepath = file.Substring(file.IndexOf("UIBackground")).Replace(".png","");
            if (nameToPath.ContainsKey(filepath)) continue;
            nameToPath.Add(filepath, GetGUID(file));
        }
        
    }

    private static string GetGUID(string fullpath)
    {
        string path = fullpath.Substring(fullpath.IndexOf("Assets/"));
        return AssetDatabase.AssetPathToGUID(path);
    }

    private static char[] splitChar = new char[2]{'\\' , '/'};

    private static bool GetFolderAndFileName(string path,ref string filename,ref string foldname)
    {
        string[] folders = path.Split(splitChar);
        if (folders.Length < 2) return false;
        filename = folders[folders.Length - 1];
        foldname = folders[folders.Length - 2];
        return true;
    }

    private static bool GetTextureFileName(string path , ref string filename)
    {
        if (!path.Contains(m_tex_src)) return false;
        filename = path.Substring(path.IndexOf("UIBackground"));
        filename = filename.Replace(".png", ""); 
        return true;
    }
    #endregion

    #region Common
    public static string[] GetFiles(string path, string formatFix,SearchOption option = SearchOption.AllDirectories)
    {
        string np = Application.dataPath +"/" + path.Replace("Assets", "");
        return Directory.GetFiles(np, formatFix, option);
    }
    #endregion


    #region  Replace Mask

     [MenuItem("Tools/UI/Replace Mask To RectMask2D")]
    public static void ReplaceMaskToRectMask2D(){

        string path = Application.dataPath + "/" + m_prefab_url;
        m_files = Directory.GetFiles(path, "*.prefab", SearchOption.AllDirectories);
        foreach(string file in m_files)
        {
         
            string filepath = file.Substring(file.IndexOf("Assets/"));
            GameObject go = PrefabUtility.LoadPrefabContents(filepath);
            if (go == null) continue;
            int flag = 0;
            ChangePrefabMask(go.transform,ref flag);
            if(flag > 0)
            {
                PrefabUtility.SaveAsPrefabAsset(go, filepath);
                Debug.Log("Save Success!:" + filepath);
            }
            PrefabUtility.UnloadPrefabContents(go);
        }
    }

     private static void ChangePrefabMask(Transform trans,ref int changeStatus)
    {
        ReplaceMask(trans.gameObject, ref changeStatus);
        if (trans.childCount == 0) return;
        for(int i = 0; i < trans.childCount;i++)
        {
            Transform child = trans.GetChild(i);
            ChangePrefabMask(child,ref changeStatus);
        }
    }


    private static void ReplaceMask( GameObject prefab,ref int changeStatus){

        Mask mask = prefab.GetComponent<Mask>();
        if (mask == null) return ;

        CFRectMask2D mask2d = prefab.GetComponent<CFRectMask2D>();
        if (mask2d == null) prefab.AddComponent<CFRectMask2D>();

        GameObject.DestroyImmediate(mask,true);
        changeStatus++;
    }

    

    #endregion


        #region  Replace Disable Raycast

     [MenuItem("Tools/UI/Disable Text Raycast")]
    public static void DisableTextRaycast(){

        string path = Application.dataPath + "/" + m_prefab_url;
        m_files = Directory.GetFiles(path, "*.prefab", SearchOption.AllDirectories);
        foreach(string file in m_files)
        {      
            string filepath = file.Substring(file.IndexOf("Assets/"));
            GameObject go = PrefabUtility.LoadPrefabContents(filepath);
            if (go == null) continue;
            int flag = 0;
            DisableGraphicRaycast<CFText>(go.transform,ref flag);
            if(flag > 0)
            {
                PrefabUtility.SaveAsPrefabAsset(go, filepath);
                Debug.Log("Save Success!:" + filepath);
            }
            PrefabUtility.UnloadPrefabContents(go);
        }
    }
     [MenuItem("Tools/UI/Disable All Reycast")]
        public static void DisableMaskableGraphicRaycast(){

        string path = Application.dataPath + "/" + m_prefab_url;
        m_files = Directory.GetFiles(path, "*.prefab", SearchOption.AllDirectories);
        foreach(string file in m_files)
        {
         
            string filepath = file.Substring(file.IndexOf("Assets/"));
            GameObject go = PrefabUtility.LoadPrefabContents(filepath);
            if (go == null) continue;
            int flag = 0;
            DisableGraphicRaycast<Graphic>(go.transform,ref flag);
            if(flag > 0)
            {
                PrefabUtility.SaveAsPrefabAsset(go, filepath);
                Debug.Log("Save Success!:" + filepath);
            }
            PrefabUtility.UnloadPrefabContents(go);
        }
    }

     private static void DisableGraphicRaycast<T>(Transform trans,ref int changeStatus) where T:Graphic
    {
        T[] mbs = trans.GetComponentsInChildren<T>(true);
        if(mbs.Length == 0) return;
        for(int i =0; i < mbs.Length;i++){
            if(mbs[i].raycastTarget){
                changeStatus++;
                mbs[i].raycastTarget = false;
            }
        }
    }
    #endregion


         [MenuItem("Tools/UI/Use Mask")]
        public static void UseMaskControl(){

        string path = Application.dataPath + "/" + m_prefab_url+ "/HandBook";
        m_files = Directory.GetFiles(path, "*.prefab", SearchOption.AllDirectories);
        foreach(string file in m_files)
        {
         
            string filepath = file.Substring(file.IndexOf("Assets/"));
            GameObject go = PrefabUtility.LoadPrefabContents(filepath);
            if (go == null) continue;
            int flag = 0;
            UseMask(go.transform,ref flag);
            if(flag > 0)
            {
                PrefabUtility.SaveAsPrefabAsset(go, filepath);
                Debug.Log("Save Success!:" + filepath);
            }
            PrefabUtility.UnloadPrefabContents(go);
        }
    }

     private static void UseMask(Transform trans,ref int changeStatus)
    {
        MaskableGraphic[] mbs = trans.GetComponentsInChildren<MaskableGraphic>(true);
        if(mbs.Length == 0) return;
        for(int i =0; i < mbs.Length;i++){
            mbs[i].m_UseMask =  true;
            changeStatus++;
        }
    }

}

