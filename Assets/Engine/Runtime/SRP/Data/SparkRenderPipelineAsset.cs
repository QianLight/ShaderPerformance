using System;
using UnityEngine;
using UnityEngine.Rendering;
#if UNITY_EDITOR

using UnityEditor;
#endif

namespace CFEngine.SRP
{
    public class SparkRenderPipelineAsset : RenderPipelineAsset
    {
        private Shader m_DefaultShader = null;
        private SparkRenderer m_2DRenderer = null;
        [SerializeField] StencilStateData m_DefaultStencilState = new StencilStateData ();
        [SerializeField] public bool enableSRPBatch = true;

        // Advanced settings
        // [NonSerialized] public FlagMask Flag;
        // public static uint Flag_SRPBatcher = 0x00000001;
        // public static uint Flag_DynamicBatching = 0x00000002;

        private static SparkRenderPipeline s_Instance = null;
#if UNITY_EDITOR

        [UnityEditor.Callbacks.DidReloadScripts]
        static void OnEditorReload () { s_Instance = null; }
#endif
        public static SparkRenderPipeline instance
        {
            get
            {
                if (s_Instance == null || s_Instance.disposed)
                {
                    s_Instance = new SparkRenderPipeline ();

                }

                return s_Instance;
            }
        }
        public SparkRenderer MainRenderer
        {
            get
            {
                if (ForwardRenderer.mainRenderer == null)
                {
                    ForwardRenderer.mainRenderer = new ForwardRenderer ();
                }
                return ForwardRenderer.mainRenderer;
            }
        }
        public SparkRenderer UIRenderer
        {
            get
            {
                if (ForwardUIRenderer.uiRenderer == null)
                {
                    ForwardUIRenderer.uiRenderer = new ForwardUIRenderer ();
                }
                return ForwardUIRenderer.uiRenderer;
            }
        }

        public StencilStateData defaultStencilState => m_DefaultStencilState;

        public override string[] renderingLayerMaskNames => DefaultGameObjectLayer.layerMaskName;
        protected SparkRenderPipelineAsset () : base ()
        {
            if (m_2DRenderer == null)
                m_2DRenderer = null;
            if (QualitySettingData.current == null)
            {
                QualitySettingData.current = QualitySettingData.none;
            }
            PassManager.singleton.Init (this);
        }

        protected override RenderPipeline CreatePipeline ()
        {
#if UNITY_EDITOR
            if (!EngineContext.IsRunning)
            {
                EngineContext.UseSrp = true;
            }

#endif
            return instance;
        }

#if UNITY_EDITOR
        Material GetDefaultMaterial (DefaultMaterialType materialType)
        {
            return null;
        }
#endif

        Material GetMaterial (DefaultMaterialType materialType)
        {
#if UNITY_EDITOR
            var editorResources = SparkRenderPipelineEditorResources.editorResources;
            if (editorResources == null)
                return null;

            var material = GetDefaultMaterial (materialType);
            if (material != null)
                return material;

            switch (materialType)
            {
                case DefaultMaterialType.Standard:
                    return editorResources.materials.lit;

                case DefaultMaterialType.Particle:
                    return editorResources.materials.particleLit;

                case DefaultMaterialType.Terrain:
                    return editorResources.materials.terrainLit;

                    // Unity Builtin Default
                default:
                    return null;
            }
#else
            return null;
#endif
        }

        public override Material defaultMaterial
        {
            get { return GetMaterial (DefaultMaterialType.Standard); }
        }

        public override Material defaultParticleMaterial
        {
            get { return GetMaterial (DefaultMaterialType.Particle); }
        }

        public override Material defaultLineMaterial
        {
            get { return GetMaterial (DefaultMaterialType.Particle); }
        }

        public override Material defaultTerrainMaterial
        {
            get { return GetMaterial (DefaultMaterialType.Terrain); }
        }

        public override Material defaultUIMaterial
        {
            get { return GetMaterial (DefaultMaterialType.UnityBuiltinDefault); }
        }

        public override Material defaultUIOverdrawMaterial
        {
            get { return GetMaterial (DefaultMaterialType.UnityBuiltinDefault); }
        }

        public override Material defaultUIETC1SupportedMaterial
        {
            get { return GetMaterial (DefaultMaterialType.UnityBuiltinDefault); }
        }

        public override Material default2DMaterial
        {
            get { return GetMaterial (DefaultMaterialType.Sprite); }
        }

        public override Shader defaultShader
        {
            get
            {
                // if (m_DefaultShader == null)
                //     m_DefaultShader = Shader.Find (ShaderUtils.GetShaderPath (ShaderPathID.Lit));

                return m_DefaultShader;
            }
        }

#if UNITY_EDITOR
        public override Shader autodeskInteractiveShader
        {
            get { return SparkRenderPipelineEditorResources.editorResources.shaders.autodeskInteractivePS; }
        }

        public override Shader autodeskInteractiveTransparentShader
        {
            get { return SparkRenderPipelineEditorResources.editorResources.shaders.autodeskInteractiveTransparentPS; }
        }

        public override Shader autodeskInteractiveMaskedShader
        {
            get { return SparkRenderPipelineEditorResources.editorResources.shaders.autodeskInteractiveMaskedPS; }
        }

        public override Shader terrainDetailLitShader
        {
            get { return SparkRenderPipelineEditorResources.editorResources.shaders.terrainDetailLitPS; }
        }

        public override Shader terrainDetailGrassShader
        {
            get { return SparkRenderPipelineEditorResources.editorResources.shaders.terrainDetailGrassPS; }
        }

        public override Shader terrainDetailGrassBillboardShader
        {
            get { return SparkRenderPipelineEditorResources.editorResources.shaders.terrainDetailGrassBillboardPS; }
        }

        public override Shader defaultSpeedTree7Shader
        {
            get { return SparkRenderPipelineEditorResources.editorResources.shaders.defaultSpeedTree7PS; }
        }

        public override Shader defaultSpeedTree8Shader
        {
            get { return SparkRenderPipelineEditorResources.editorResources.shaders.defaultSpeedTree8PS; }
        }
#endif

    }
}