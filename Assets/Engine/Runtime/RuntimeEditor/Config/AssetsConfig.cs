#if UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace CFEngine
{

    public enum SpriteSize
    {
        E1x1,
        E2x2,
        E4x4,
        E8x8,
        E16x16,
        E32x32,
        E64x64,
        E128x128,
        E256x256,
        E512x512,
        E1024x1024,
        E2048x2048,
    }
    public enum EditorSceneObjectType
    {
        EditorScene,
        Light,
        ReflectionProbes,
        Collider,
        Enverinment,
        Animation,
        Effect,
        DynamicObject,
        Prefab,
        StaticPrefab,
        LightProbes,
        Audio,
        Instance,
        MeshCombine,

        MeshTerrain,
        UnityTerrain,
        Num,
    }
    public enum ELightMapMode
    {
        None,
        LightmapMat,
        LightmapKeyWord
    }

    [System.Serializable]
    public struct SceneLoadInfo
    {
        public string name;
        public int count;
    }


    [Serializable]
    public class BaseAssetConfig
    {
        public string name = "";
        [NonSerialized]
        public string path = "";

        public virtual IList GetList () { return null; }

        public virtual Type GetListType () { return null; }

        public virtual void OnAdd () { }

        public virtual void OnRemove () { }
        public virtual void OnReorder () { }
        public virtual float GetHeight (int index) { return 0; }
        public virtual void SetHeight (int index, float height) { }
    }

    public class BaseConifg : ScriptableObject
    {
        [NonSerialized]
        public FolderConfig folder;

    }
    public class AssetBaseConifg<T> : BaseConifg where T : BaseConifg
    {
        private static T g_Config;

        public static T instance
        {
            get
            {
                string assetPath = $"Assets/Engine/Editor/EditorResources/{typeof(T).Name}.asset";
                g_Config = AssetDatabase.LoadAssetAtPath<T>(assetPath);
                if (g_Config == null)
                {
                    g_Config = EditorCommon.LoadAsset<T>(assetPath);
                }
                if (g_Config.folder == null)
                {
                    string name = typeof (T).Name;
                    g_Config.folder = FolderConfig.Load (string.Format ("{0}_Folder", name));
                }
                return g_Config;
            }
        }

        public void Save ()
        {
            string name = typeof (T).Name;
            this.folder.Save (string.Format ("{0}_Folder", name));
            EditorCommon.SaveAsset (string.Format ("Assets/Engine/Editor/EditorResources/{0}.asset", name), this);
        }
    }

    public class AssetsConfig : AssetBaseConifg<AssetsConfig>
    {
        #region const
        public string ResourcePath = "Assets/BundleRes";
        public string SceneLibPath = "Assets/Scenes/Scenelib";
        public string EngineResPath = "Assets/Engine/Editor/EditorResources";
        public string sfxDir = "Assets/BundleRes/Runtime/SFX/";
        public string sfxJianxiuDir = "Assets/Jianxiu/BundleRes/Runtime/SFX/";
        public string MatEffectPath = "Assets/Engine/Editor/EditorResources/MatEffectConfig.asset";

        public string Bandpose_Str = "Bandpose";
        public string Fbx_Ext = ".fbx";
        public string Creature_Path = "Assets/Creatures";

        public string ResourceAnimationPath = "Assets/BundleRes/Animation";
        public string ResourceEditorAnimationPath = "Assets/BundleRes/EditorAnimation";
        public string ResourceAnimation = "Animation";

        public string DummyMatFolder = "MatShader";
        public string Table_Path = "Assets/Table/";
        public string Table_Bytes_Path = "Assets/BundleRes/Table/";

        public string TerrainDirStr = "Terrain";
        public string ConfigStr = "_Config";
        public string SceneConfigStr = "_SceneConfig";
        public string SceneChunkStr = "Chunk_";
        public string TerrainBlockStr = "Block_";

        public string AtlasDirStr = "Assets/Scenes/modlelib/atlas";
        public string SpriteAtlasExt = ".spriteatlas";

        public string ReadableMeshSuffix = "_readable.asset";

        //public string EditorSceneRes = "/EditorSceneRes";

        public static string[] EditorGoPath = new string[(int) EditorSceneObjectType.Num]
        {
            "EditorScene",
            "Light",
            "ReflectionProbes",
            "Collider",
            "Enverinment",
            "Animation",
            "Effects",
            "DynamicObjects",
            "Prefabs",
            "StaticPrefabs",
            "LightProbes",
            "Audio",
            "Instance",
            "MeshCombine",
            "MeshTerrain",
            "UnityTerrain",
        };
        public static string[] extList = new string[]
        {
            "Select...",
            ".anim",
            ".asset",
            ".bytes",
            ".scenebytes",
            ".controller",
            ".prefab",
            ".playable",
            ".mat",
            ".txt",
            ".tga",
            ".exr",
            ".png",
            ".xml",
            ".shader",
            ".ttf",
            ".otf",
            ".spriteatlas",
            ".mp4",
            ".unity",
            ".mesh",
            ".resbytes",
        };
        #endregion



        public LightmapParameters[] LightmapParam;
        [HideInInspector]
        public bool commonFolder = false;
        #region material
        //terrain
        public Material TerrainEditMat;
        public Material TerrainMergeMat;
        public Material TerrainMeshMat;
        public Material TerrainBakeBaseMap;

        public Material[] TerrainPreviewMat;

        //tool
        public Material ShadowCaster;
        public Material ShadowCasterCutout;

        // public Material DrawTexChanel;
        //preview
        public Material Outline;
        public Material PreviewInstance;
        // public Material PreveiwTransparent;
        public Material PreviewTerrainQuad;
        // public Material PreviewMeshCloud;
        public Material DrawLightIndex;

        public Material TerrainBake;

        public Material PreviewSH;
        public Material PreviewSH2;

        public Material LightProbeAreaEdit;
        public Material SceneViewPreZ;
        public Material CopyMat;
        
        //Overdraw
        public Material OverdrawOpaque;
        public Material OverdrawTransparent;
        public ComputeShader OverdrawCompute;
        public Material ShadedWireframe;
        #endregion

        #region shader
        public Mesh sphereMesh;
        public Mesh lightIndex;
        // public Shader TerrainPreview;
        public Shader TextureBake;
        public Shader TextureArrayBake;

        public Shader defaultRoleShader;

        public Shader CombineLightmap;
        public Shader BakeTexChanel;
        public Shader CombineNormal;
        public Shader DrawShadowMap;
        public Shader DrawShadowMapExtra;
        public Shader ScenePreview;
        public Shader PreviewTex;
        public Shader PreviewTex2dArray;
        public Shader Preview3D;
        public Shader LightIndexPreview;
        public Shader[] iblShaders;
        public Shader[] layerShader;
        public ComputeShader shBaker;
        #endregion

        #region prefab
        public GameObject dyanmicLights;
        #endregion

        #region meshTex
        [HideInInspector]
        public MeshConfigData meshConfig = new MeshConfigData ();
        [HideInInspector]
        public TexConfigData texConfig = new TexConfigData ();
        #endregion

        public TextAsset debugFile;
        public TextAsset ppDebugFile;
        public struct ShaderDebugContext
        {
            public TextAsset debugHLSL;
            public string[] debugNames;
            public int customStart;
        }
        public static ShaderDebugContext shaderDebugContext;
        public static ShaderDebugContext shaderPPDebugContext;

        public static void RefreshShaderDebugNames (bool isPP, bool force = false)
        {
            if (isPP)
            {
                shaderPPDebugContext.debugHLSL = instance.ppDebugFile;
                RefreshShaderDebugNames (force, ref shaderPPDebugContext);
            }
            else
            {
                shaderDebugContext.debugHLSL = instance.debugFile;
                RefreshShaderDebugNames (force, ref shaderDebugContext);
            }
        }

        private static void RefreshShaderDebugNames (bool force, ref ShaderDebugContext debugContext)
        {
            if (debugContext.debugNames == null || force)
            {
                try
                {
                    if (debugContext.debugHLSL != null)
                    {
                        debugContext.customStart = 100;
                        string path = AssetDatabase.GetAssetPath (debugContext.debugHLSL);
                        bool parse = false;
                        using (FileStream fs = new FileStream (path, FileMode.Open))
                        {
                            List<string> debugTypeStr = new List<string> ();
                            StreamReader sr = new StreamReader (fs);
                            while (!sr.EndOfStream)
                            {
                                string line = sr.ReadLine ();
                                if (parse)
                                {
                                    if (line.StartsWith ("//DEBUG_END"))
                                    {
                                        parse = false;
                                        break;
                                    }
                                    else
                                    {
                                        string[] str = line.Split (' ');
                                        if (str.Length >= 3 && str[0] == "#define")
                                        {
                                            string debugStr = str[1];
                                            if (debugStr.StartsWith ("Debug_F"))
                                            {
                                                debugStr = debugStr.Replace ("Debug_F_", "");
                                                string[] folderStr = debugStr.Split ('_');
                                                if (folderStr.Length == 2)
                                                {
                                                    debugStr = string.Format ("{0}/{1}", folderStr[0], folderStr[1]);
                                                }
                                                debugTypeStr.Add (debugStr);
                                            }
                                            else if (debugStr == "Debug_None")
                                            {
                                                debugTypeStr.Add ("None");
                                            }
                                            else if (debugStr == "Debug_Custom_Start")
                                            {
                                                debugContext.customStart = debugTypeStr.Count;
                                            }
                                        }
                                    }
                                }
                                else
                                {
                                    if (line.StartsWith ("//DEBUG_START"))
                                    {
                                        parse = true;
                                    }
                                }
                            }
                            debugContext.debugNames = debugTypeStr.ToArray ();
                        }

                    }

                }
                catch (Exception)
                {

                }
            }
        }

        public static int FindDebugIndex (string str, bool isPP)
        {
            RefreshShaderDebugNames (isPP);
            var debugNames = isPP?shaderPPDebugContext.debugNames : shaderDebugContext.debugNames;
            if (debugNames != null)
            {
                for (int i = 0; i < debugNames.Length; ++i)
                {
                    if (debugNames[i] == str)
                    {
                        return i;
                    }
                }
            }
            return -1;
        }
        public static bool GetResType (UnityEngine.Object asset, string path, ref byte resType)
        {
            path = path.ToLower ();
            if (asset is Material)
            {
                resType = ResObject.Mat;
                return true;
            }
            if (asset is Mesh)
            {
                resType = ResObject.Mesh;
                return true;
            }
            else if (asset is Texture2D)
            {
                if (path.EndsWith (".tga"))
                    resType = ResObject.Tex_2D;
                else if (path.EndsWith (".png"))
                    resType = ResObject.Tex_2D_PNG;
                else if (path.EndsWith (".exr"))
                    resType = ResObject.Tex_2D_EXR;
                return true;
            }
            else if (asset is Cubemap)
            {
                if (path.EndsWith (".tga"))
                    resType = ResObject.Tex_Cube;
                else if (path.EndsWith (".png"))
                    resType = ResObject.Tex_Cube_PNG;
                else if (path.EndsWith (".hdr"))
                {
                    DebugLog.AddErrorLog ("not support hdr format");
                    return false;
                }
                else if (path.EndsWith (".exr"))
                    resType = ResObject.Tex_Cube_EXR;
                return true;
            }
            else if (asset is GameObject)
            {
                if (path.EndsWith(".prefab"))
                {
                    resType = ResObject.Prefab;
                    return true;
                }
            }
            return false;
        }
        public static bool GetResType (UnityEngine.Object asset, ref byte resType)
        {
            if (asset == null)
                return false;
            return GetResType (asset, AssetDatabase.GetAssetPath (asset).ToLower (), ref resType);

        }
    }

    [CustomEditor (typeof (AssetsConfig))]
    public partial class AssetsConfigEdit : UnityEditor.Editor
    {
        public override void OnInspectorGUI () { }

    }
}
#endif