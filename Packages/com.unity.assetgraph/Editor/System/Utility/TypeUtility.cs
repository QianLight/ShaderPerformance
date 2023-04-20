using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using Model = UnityEngine.AssetGraph.DataModel.Version2;

namespace UnityEngine.AssetGraph
{
    public static class TypeUtility
    {

        private static readonly HashSet<Type> IgnoreTypes = new HashSet<Type> {
            typeof(MonoScript),
            typeof(AssetBundleReference),
            typeof(Model.ConfigGraph),
            typeof(Model.ConnectionData),
            typeof(Model.ConnectionPointData),
            typeof(Model.NodeData),
            typeof(AssetReferenceDatabase),
            typeof(AssetBundleBuildMap),
            typeof(AssetProcessEventRecord)
        };

        private static readonly Dictionary<string, Type> Extension2Type = new Dictionary<string, Type> {
           {".mtl",typeof(UnityEditor.DefaultAsset) },
           {".tpsheet",typeof(UnityEditor.DefaultAsset) },
           {".dll",typeof(UnityEditor.DefaultAsset) },
           {".cubemap",typeof(UnityEngine.Cubemap) },
           {".wav",typeof(UnityEngine.AudioClip) },
           {".mp3",typeof(UnityEngine.AudioClip) },
           {".prefab",typeof(UnityEngine.GameObject) },
           {".fbx",typeof(UnityEngine.GameObject) },
           {".obj",typeof(UnityEngine.GameObject) },
           {".bytes",typeof(UnityEngine.TextAsset) },
           {".txt",typeof(UnityEngine.TextAsset) },
           {".cginc",typeof(UnityEngine.TextAsset) },
           {".anim",typeof(UnityEngine.AnimationClip) },
           //{".playable",typeof(UnityEngine.Timeline.TimelineAsset) },
           {".unity",typeof(UnityEditor.SceneAsset) },
           {".mat",typeof(UnityEngine.Material) },
           {".shader",typeof(UnityEngine.Shader) },
           {".renderTexture",typeof(UnityEngine.RenderTexture) },
           {".ttf",typeof(UnityEngine.Font) },
           {".otf",typeof(UnityEngine.Font) },
           {".fontsettings",typeof(UnityEngine.Font) },
           {".webm",typeof(UnityEngine.Video.VideoClip) },
#if UNITY_2017
            {".sbsar",typeof(UnityEditor.SubstanceArchive) },
#endif
           {".flare",typeof(UnityEngine.Flare) },
           {".cs",typeof(UnityEditor.MonoScript) },
           {".rendertexture",typeof(UnityEngine.RenderTexture) },
           {".spriteatlas",typeof(UnityEngine.U2D.SpriteAtlas) },
           {".mask",typeof(UnityEngine.AvatarMask) },
           {".compute",typeof(UnityEngine.ComputeShader) },
           {".shadervariants",typeof(UnityEngine.ShaderVariantCollection) },
           {".controller",typeof(UnityEditor.Animations.AnimatorController) },
        };
        private static readonly Dictionary<string, Type> Extension2FuzzyType = new Dictionary<string, Type> {
            { ".tga",typeof(UnityEngine.Texture) },
            { ".png",typeof(UnityEngine.Texture) },
            { ".bmp",typeof(UnityEngine.Texture) },
            { ".jpg",typeof(UnityEngine.Texture) },
            { ".tif",typeof(UnityEngine.Texture) },
            { ".psd",typeof(UnityEngine.Texture) },
            { ".exr",typeof(UnityEngine.Cubemap) },
            { ".hdr",typeof(UnityEngine.Cubemap) },
            { ".asset",typeof(UnityEditor.DefaultAsset) },
        };
        private static readonly Dictionary<string, Type> Extension2ImportType = new Dictionary<string, Type>
        {
            {".mp3",typeof(UnityEditor.AudioImporter)},
            {".bytes",typeof(UnityEditor.AssetImporter)},
            {".asset",typeof(UnityEditor.AssetImporter)},
            {".exr",typeof(UnityEditor.TextureImporter)},
            {".png",typeof(UnityEditor.TextureImporter)},
            {".unity",typeof(UnityEditor.AssetImporter)},
            {".prefab",System.Reflection.Assembly.Load("UnityEditor").GetType("UnityEditor.PrefabImporter")},
            {".mtl",typeof(UnityEditor.AssetImporter)},
            {".mat",typeof(UnityEditor.AssetImporter)},
            {".obj",typeof(UnityEditor.ModelImporter)},
            {".anim",typeof(UnityEditor.AssetImporter)},
            {".fbx",typeof(UnityEditor.ModelImporter)},
            {".controller",typeof(UnityEditor.AssetImporter)},
            {".playable",typeof(UnityEditor.AssetImporter)},
            {".mask",typeof(UnityEditor.AssetImporter)},
            {".hdr",typeof(UnityEditor.TextureImporter)},
            {".tga",typeof(UnityEditor.TextureImporter)},
            {".bmp",typeof(UnityEditor.TextureImporter)},
            {".otf",typeof(UnityEditor.TrueTypeFontImporter)},
            {".ttf",typeof(UnityEditor.TrueTypeFontImporter)},
            {".shader",typeof(UnityEditor.ShaderImporter)},
            {".cginc",typeof(UnityEditor.ShaderImporter)},
            {".tif",typeof(UnityEditor.TextureImporter)},
            {".shadervariants",typeof(UnityEditor.AssetImporter)},
            {".rendertexture",typeof(UnityEditor.AssetImporter)},
            {".spriteatlas",typeof(UnityEditor.AssetImporter)},
        };

        public static bool IsLoadingAsset(string assetPath)
        {
            if (assetPath.Contains(Model.Settings.Path.BasePath))
            {
                return false;
            }
            Type t = GetMainAssetFuzzyTypeAtPath(assetPath);
            if (t == null)
            {
                return false;
            }
            if (IgnoreTypes.Contains(t))
            {
                return false;
            }
            return true;
        }

        public static Type GetAssetImporterTypeAtPath(string assetPath)
        {
            Type importerType = null;
            AssetImporter importer = null;
            string extension = Path.GetExtension(assetPath).ToLower();
            if (Extension2ImportType.ContainsKey(extension))
            {
                importerType = Extension2ImportType[extension];
            }
            else
            {
                importer = AssetImporter.GetAtPath(assetPath);
                if (importer != null)
                {
                    importerType = importer.GetType();
                }
            }

            if (importerType != null && importerType != typeof(UnityEditor.AssetImporter))
            { }
            else
            {
                if (Extension2Type.ContainsKey(extension))
                {
                    importerType = Extension2Type[extension];
                }
                else
                {
                    importerType = null;
                }
            }

            return importerType;
        }

        public static Type GetMainAssetFuzzyTypeAtPath(string assetPath)
        {
            Type t = null;
            string extension = System.IO.Path.GetExtension(assetPath).ToLower();
            if (Extension2Type.ContainsKey(extension))
            {
                t = Extension2Type[extension];
            }
            else if (Extension2FuzzyType.ContainsKey(extension))
            {
                t = Extension2FuzzyType[extension];
            }
            else
            {
                t = AssetDatabase.GetMainAssetTypeAtPath(assetPath);
            }

            return t;
        }

        /**
         * Get type of asset from give path.
         */
        public static Type GetMainAssetTypeAtPath(string assetPath)
        {
            Type t = null;
            string extension = Path.GetExtension(assetPath).ToLower();
            if (Extension2Type.ContainsKey(extension))
            {

                t = Extension2Type[extension];
            }
            else
            {
                t = AssetDatabase.GetMainAssetTypeAtPath(assetPath);
            }
            /*
            // 5.6.x may return MonoBehaviour as type when main asset is ScriptableObject
            if(t == typeof(MonoBehaviour)) {
                UnityEngine.Object asset = AssetDatabase.LoadMainAssetAtPath(assetPath);
                t = asset.GetType();
            }
            */

            return t;
        }

        public static MonoScript LoadMonoScript(string assemblyQualifiedTypeName)
        {
            if (assemblyQualifiedTypeName == null)
            {
                return null;
            }

            var t = Type.GetType(assemblyQualifiedTypeName);
            if (t == null)
            {
                return null;
            }

            string[] guids = AssetDatabase.FindAssets("t:MonoScript " + t.Name);

            MonoScript s = null;

            if (guids.Length > 0)
            {
                var path = AssetDatabase.GUIDToAssetPath(guids[0]);
                s = AssetDatabase.LoadAssetAtPath<MonoScript>(path);
            }

            return s;
        }
    }
}
