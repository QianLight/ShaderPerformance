#if UNITY_EDITOR
using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using UnityEditor;
using UnityEditor.ProjectWindowCallback;
using UnityEngine;

namespace CFEngine.SRP
{
    public class SparkRenderPipelineEditorResources : ScriptableObject
    {
        [Serializable, ReloadGroup]
        public sealed class ShaderResources
        {
            [Reload ("Shaders/Autodesk Interactive/Autodesk Interactive.shadergraph")]
            public Shader autodeskInteractivePS;

            [Reload ("Shaders/Autodesk Interactive/Autodesk Interactive Transparent.shadergraph")]
            public Shader autodeskInteractiveTransparentPS;

            [Reload ("Shaders/Autodesk Interactive/Autodesk Interactive Masked.shadergraph")]
            public Shader autodeskInteractiveMaskedPS;

            // [Reload ("Shaders/Terrain/TerrainDetailLit.shader")]
            private Shader terrainShader = null;
            public Shader terrainDetailLitPS
            {
                get
                {
                    if (terrainShader == null)
                        terrainShader = Shader.Find ("Hidden/Custom/Editor/TerrainEdit");
                    return terrainShader;
                }
            }
            public Shader defaultShader;
            // [Reload ("Shaders/Terrain/WavingGrass.shader")]
            public Shader terrainDetailGrassPS
            {
                get
                {
                    if (defaultShader == null)
                        defaultShader = Shader.Find ("Unlit/Transparent");
                    return defaultShader;
                }
            }

            // [Reload ("Shaders/Terrain/WavingGrassBillboard.shader")]
            public Shader terrainDetailGrassBillboardPS
            {
                get
                {
                    if (defaultShader == null)
                        defaultShader = Shader.Find ("Unlit/Transparent");
                    return defaultShader;
                }
            }

            [Reload ("Shaders/Nature/SpeedTree7.shader")]
            public Shader defaultSpeedTree7PS;

            [Reload ("Shaders/Nature/SpeedTree8.shader")]
            public Shader defaultSpeedTree8PS;
        }

        [Serializable, ReloadGroup]
        public sealed class MaterialResources
        {
            [Reload ("Runtime/Materials/Lit.mat")]
            public Material lit;

            [Reload ("Runtime/Materials/ParticlesLit.mat")]
            public Material particleLit;

            [Reload ("Runtime/Materials/TerrainLit.mat")]
            public Material terrainLit;
        }

        public ShaderResources shaders;
        public MaterialResources materials;
        [NonSerialized]
        static internal SparkRenderPipelineEditorResources m_EditorResourcesAsset;

        static internal SparkRenderPipelineEditorResources editorResources
        {
            get
            {
                if (m_EditorResourcesAsset == null)
                {
                    string path = string.Format("{0}/Runtime/Data/SRPEditorAsset.asset",ResourceReloader.packagePath);
                    if(File.Exists(path))
                    {
                        m_EditorResourcesAsset = AssetDatabase.LoadAssetAtPath<SparkRenderPipelineEditorResources>(path);
                    }
                    else
                    {
                        var asset = SparkRenderPipelineEditorResources.CreateInstance<SparkRenderPipelineEditorResources>();
                        asset.name = "SRPEditorAsset";
                        m_EditorResourcesAsset = EditorCommon.CreateAsset<SparkRenderPipelineEditorResources>(path,".asset",asset);
                    }
                }
                    // m_EditorResourcesAsset = LoadResourceFile<SparkRenderPipelineEditorResources> ();

                return m_EditorResourcesAsset;
            }
        }
        // public static SparkRenderPipelineAsset Create ()
        // {

        //     return stance;
        // }

        // static SparkRendererData CreateRendererData (RendererType type)
        // {
        //     switch (type)
        //     {
        //         case RendererType.ForwardRenderer:
        //             return CreateInstance<ForwardRendererData> ();
        //             // 2D renderer is experimental
        //             // case RendererType._2DRenderer:
        //             //     return CreateInstance<Experimental.Rendering.Universal.Renderer2DData> ();
        //             //     // Forward Renderer is the fallback renderer that works on all platforms
        //         default:
        //             return CreateInstance<ForwardRendererData> ();
        //     }
        // }

        // static SparkRendererData CreateRendererAsset (string path, RendererType type, bool relativePath = true)
        // {
        //     SparkRendererData data = CreateRendererData (type);
        //     string dataPath;
        //     if (relativePath)
        //         dataPath =
        //         $"{Path.Combine(Path.GetDirectoryName(path), Path.GetFileNameWithoutExtension(path))}_Renderer{Path.GetExtension(path)}";
        //     else
        //         dataPath = path;
        //     AssetDatabase.CreateAsset (data, dataPath);
        //     return data;
        // }

        [SuppressMessage ("Microsoft.Performance", "CA1812")]
        internal class CreateUniversalPipelineAsset : EndNameEditAction
        {
            public override void Action (int instanceId, string pathName, string resourceFile)
            {
                //Create asset
                AssetDatabase.CreateAsset (CreateInstance<SparkRenderPipelineAsset> (), pathName);
            }
        }

        [MenuItem (@"Assets/Tool/SRP/Pipeline Asset")]
        static void CreateSparkPipeline ()
        {
            ProjectWindowUtil.StartNameEditingIfProjectWindowExists (0, CreateInstance<CreateUniversalPipelineAsset> (),
                "SparkRenderPipelineAsset.asset", null, null);
        }

        public static T LoadResourceFile<T> () where T : ScriptableObject
        {
            T resourceAsset = null;
            var guids = AssetDatabase.FindAssets (typeof (T).Name + " t:scriptableobject", new [] { "Assets" });
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath (guid);
                resourceAsset = AssetDatabase.LoadAssetAtPath<T> (path);
                if (resourceAsset != null)
                    break;
            }

            // There's currently an issue that prevents FindAssets from find resources withing the package folder.
            if (resourceAsset == null)
            {
                string path = ResourceReloader.packagePath + "/Runtime/Data/" + typeof (T).Name + ".asset";
                resourceAsset = AssetDatabase.LoadAssetAtPath<T> (path);
            }

            // Validate the resource file
            ResourceReloader.TryReloadAllNullIn (resourceAsset, ResourceReloader.packagePath);

            return resourceAsset;
        }
    }

    [UnityEditor.CustomEditor (typeof (SparkRenderPipelineEditorResources), true)]
    class SparkRenderPipelineEditorResourcesEditor : UnityEditor.Editor
    {

        public override void OnInspectorGUI ()
        {
            DrawDefaultInspector ();

            // Add a "Reload All" button in inspector when we are in developer's mode
            if (UnityEditor.EditorPrefs.GetBool ("DeveloperMode") && GUILayout.Button ("Reload All"))
            {
                var resources = target as SparkRenderPipelineEditorResources;
                resources.materials = null;
                resources.shaders = null;
                ResourceReloader.ReloadAllNullIn (target, ResourceReloader.packagePath);
            }
        }
    }
}
#endif