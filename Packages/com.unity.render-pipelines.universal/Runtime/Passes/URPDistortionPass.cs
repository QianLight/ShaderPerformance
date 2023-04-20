using System;
using System.Collections.Generic;
using CFEngine;
using UnityEngine;
using UnityEngine.Experimental.Rendering.Universal;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using RenderTargetHandle = UnityEngine.Rendering.Universal.RenderTargetHandle;

public class URPDistortionPass : ScriptableRenderPass
{
    private static readonly int _URPDistortionTex = Shader.PropertyToID("_URPDistortionTex");
    private static readonly Color clearColor = new Color(0.5f, 0.5f, 0f, 0f);
    public static RenderTargetHandle _DistortionRT = new RenderTargetHandle();

    private static string normalKeyword = "_CUSTOM_DISTORTION";
    private static string uiKeyword = "_CUSTOM_UI_DISTORTION";

    private static string keyword;
    private static string anotherKeyword;
    // private static readonly RenderTargetIdentifier _rtId = _URPDistortionTex;
    // private static readonly int rtId = Shader.PropertyToID("_DistortionRT");


    private static Matrix4x4 _viewMatrix;
    private static Matrix4x4 _projMatrix;

    protected DrawingSettings m_DrawingSettings;
    protected FilteringSettings m_FilteringSettings;
    // protected FlagMask flag;

    protected static uint Flag_IsInit = 0x00000001;
    protected static uint Flag_IsOpaque = 0x00000002;
    //#if UNITY_EDITOR
    protected ProfilingSampler m_ProfilingSampler;
    //#endif
    protected ShaderTagId m_PassShaderTagId;
    protected SortingCriteria m_sortFlags;

    RenderQueueType renderQueueType;
    URPDistortion.CustomCameraSettings m_CameraSettings;

    public Material overrideMaterial { get; set; }
    public int overrideMaterialPassIndex { get; set; }

    List<ShaderTagId> m_ShaderTagIdList = new List<ShaderTagId>();

    Vector2Int widthHeight = Vector2Int.zero;
    public static void SetMatrixes(Matrix4x4 viewMatrix, Matrix4x4 projMatrix)
    {
        _viewMatrix = viewMatrix;
        _projMatrix = projMatrix;
    }
    public void SetDetphState(bool writeEnabled, CompareFunction function = CompareFunction.Less)
    {
        m_RenderStateBlock.mask |= RenderStateMask.Depth;
        m_RenderStateBlock.depthState = new DepthState(writeEnabled, function);
    }

    public void SetStencilState(int reference, CompareFunction compareFunction, StencilOp passOp, StencilOp failOp, StencilOp zFailOp)
    {
        StencilState stencilState = StencilState.defaultValue;
        stencilState.enabled = true;
        stencilState.SetCompareFunction(compareFunction);
        stencilState.SetPassOperation(passOp);
        stencilState.SetFailOperation(failOp);
        stencilState.SetZFailOperation(zFailOp);

        m_RenderStateBlock.mask |= RenderStateMask.Stencil;
        m_RenderStateBlock.stencilReference = reference;
        m_RenderStateBlock.stencilState = stencilState;
    }

    RenderStateBlock m_RenderStateBlock;

    public URPDistortionPass()
    {
        
    }
        
    public URPDistortionPass(
        RenderPassEvent renderPassEvent,
        string[] shaderTags,
        RenderQueueType renderQueueType,
        int layerMask)
    {
        // base.profilingSampler = new ProfilingSampler(nameof(RenderObjectsPass));
        //#if UNITY_EDITOR
        m_ProfilingSampler = new ProfilingSampler("URPDistortion");
        //#endif
        this.renderPassEvent = renderPassEvent;
        this.renderQueueType = renderQueueType;
        this.overrideMaterial = null;
        this.overrideMaterialPassIndex = 0;
        RenderQueueRange renderQueueRange = (renderQueueType == RenderQueueType.Transparent)
            ? RenderQueueRange.transparent
            : RenderQueueRange.opaque;
        m_FilteringSettings = new FilteringSettings(renderQueueRange, Int32.MaxValue);

        if (shaderTags != null && shaderTags.Length > 0)
        {
            foreach (var passName in shaderTags)
                m_ShaderTagIdList.Add(new ShaderTagId(passName));
        }
        else
        {
            m_ShaderTagIdList.Add(new ShaderTagId("SRPDefaultUnlit"));
            m_ShaderTagIdList.Add(new ShaderTagId("UniversalForward"));
            m_ShaderTagIdList.Add(new ShaderTagId("UniversalForwardOnly"));
            m_ShaderTagIdList.Add(new ShaderTagId("LightweightForward"));
        }

        m_RenderStateBlock = new RenderStateBlock(RenderStateMask.Nothing);
        m_CameraSettings = new URPDistortion.CustomCameraSettings();
        // _DistortionRT 
        _DistortionRT.Init("_URPDistortionRT");
    }

    // internal URPDistortionPass(URPProfileId profileId, RenderPassEvent renderPassEvent, string[] shaderTags, RenderQueueType renderQueueType, int layerMask, RenderObjects.CustomCameraSettings cameraSettings)
    // : this(renderPassEvent, shaderTags, renderQueueType, layerMask, cameraSettings)
    // {
    //     m_ProfilingSampler = ProfilingSampler.Get(profileId);
    // }

    public override void Configure(CommandBuffer cmd, RenderTextureDescriptor cameraTextureDescriptor)
    {
        // base.Configure(cmd, cameraTextureDescriptor);
        widthHeight = new Vector2Int(cameraTextureDescriptor.width, cameraTextureDescriptor.height);
        // ConfigureTarget(source);
    }
    public void Setup()
    {
        m_ProfilingSampler = new ProfilingSampler("URPDistortion");
        //#endif
        this.renderPassEvent = RenderPassEvent.AfterRenderingTransparents;
        this.renderQueueType = RenderQueueType.Transparent;
        this.overrideMaterial = null;
        this.overrideMaterialPassIndex = 0;
        RenderQueueRange renderQueueRange = (renderQueueType == RenderQueueType.Transparent)
            ? RenderQueueRange.transparent
            : RenderQueueRange.opaque;
        m_FilteringSettings = new FilteringSettings(RenderQueueRange.transparent, 0);
        //
        // if (shaderTags != null && shaderTags.Length > 0)
        // {
        //     foreach (var passName in shaderTags)
                m_ShaderTagIdList.Add(new ShaderTagId("Distortion"));
        // }
        // else
        // {
        //     m_ShaderTagIdList.Add(new ShaderTagId("SRPDefaultUnlit"));
        //     m_ShaderTagIdList.Add(new ShaderTagId("UniversalForward"));
        //     m_ShaderTagIdList.Add(new ShaderTagId("UniversalForwardOnly"));
        //     m_ShaderTagIdList.Add(new ShaderTagId("LightweightForward"));
        // }

        m_RenderStateBlock = new RenderStateBlock(RenderStateMask.Nothing);
        // m_CameraSettings = new URPDistortion.CustomCameraSettings();
        // _DistortionRT 
        _DistortionRT.Init("_URPDistortionRT");
        m_CameraSettings = new URPDistortion.CustomCameraSettings();
    }

    public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
    {

        if (GameQualitySetting.SFXLevel <= RenderQualityLevel.Low)
        {
            Shader.DisableKeyword(uiKeyword);
            Shader.DisableKeyword(normalKeyword);
            return;
        }

       // var name = renderingData.cameraData.camera.name;
        // var uiCam = renderingData.cameraData.camera.gameObject.layer == LayerMaskName.UI;
        // var uiSceneCam = renderingData.cameraData.camera.gameObject.layer == LayerMaskName.UIScene;
        keyword = EngineContext.none3DCamera ? uiKeyword : normalKeyword;
        anotherKeyword = EngineContext.none3DCamera ? normalKeyword : uiKeyword;
        Shader.EnableKeyword(keyword);
        Shader.DisableKeyword(anotherKeyword);
    #if UNITY_EDITOR
        if(Application.isPlaying && EngineContext.IsRunning || !EngineContext.IsRunning)
    #else
        if (EngineContext.instance != null)
    #endif
        {
        #if UNITY_EDITOR
            if (Application.isPlaying)
            {
        #endif  
                if (SetByFlag()) return;
        #if UNITY_EDITOR
            }
            else
            {
                Shader.EnableKeyword(keyword);
            }
        #endif
            // #if UNITY_EDITOR
            //             CommandBuffer cmd = CommandBufferPool.Get(m_ProfilingSampler.name);
            //             using (new ProfilingScope(cmd, m_ProfilingSampler))
            //             {
            //                 context.ExecuteCommandBuffer(cmd);
            //                 cmd.Clear();
            //
            //
            //                 // if (RenderContext.gameOverdrawViewMode 
            //                 //     && renderingData.cameraData.camera.cameraType.Equals(CameraType.Game) 
            //                 //     ||RenderContext.sceneOverdrawViewMode 
            //                 //     && renderingData.cameraData.camera.cameraType.Equals(CameraType.SceneView) )
            //                 // {
            //                 //     // bool isOp =flag.HasFlag(Flag_IsOpaque);
            //                 //         ShaderTagId id = new ShaderTagId("Overdraw" + m_PassShaderTagId.name);
            //                 //         // m_DrawingSettings.overrideMaterial = m_FilteringSettings.renderQueueRange.Equals(RenderQueueRange.opaque)? AssetsConfig.instance.OverdrawOpaque : AssetsConfig.instance.OverdrawTransparent;
            //                 //         m_DrawingSettings.SetShaderPassName(0, new ShaderTagId("SRPDefaultUnlit"));
            //                 //         m_DrawingSettings.SetShaderPassName(1, id);
            //                 //         var sortingSettings = m_DrawingSettings.sortingSettings;
            //                 //         sortingSettings.criteria = m_sortFlags;
            //                 //         m_DrawingSettings.sortingSettings = sortingSettings;
            //                 // }
            //                 // else
            //                 // {
            //                     // m_DrawingSettings.overrideMaterial = null;
            //                     m_DrawingSettings.SetShaderPassName(0, m_PassShaderTagId);
            //                         CreateDrawingSettings(
            //                             m_PassShaderTagId,
            //                             ref renderingData,
            //                             m_sortFlags);//ref m_DrawingSettings 
            //                 // }
            // #endif


            // NOTE: Do NOT mix ProfilingScope with named CommandBuffers i.e. CommandBufferPool.Get("name").
            // Currently there's an issue which results in mismatched markers.
            CommandBuffer cmd = CommandBufferPool.Get();
#if UNITY_EDITOR
            using (var scope = new ProfilingScope(cmd, m_ProfilingSampler))
            {
                // context.ExecuteCommandBuffer(cmd);
                // cmd.Clear();
#endif
                SortingCriteria sortingCriteria = (renderQueueType == RenderQueueType.Transparent)
                    ? SortingCriteria.CommonTransparent
                    : renderingData.cameraData.defaultOpaqueSortFlags;

                DrawingSettings drawingSettings = CreateDrawingSettings(m_ShaderTagIdList, ref renderingData, sortingCriteria);
                drawingSettings.overrideMaterial = overrideMaterial;
                drawingSettings.overrideMaterialPassIndex = overrideMaterialPassIndex;

                ref CameraData cameraData = ref renderingData.cameraData;
                Camera camera = cameraData.camera;

                // In case of camera stacking we need to take the viewport rect from base camera
                Rect pixelRect = renderingData.cameraData.pixelRect;
                float cameraAspect = (float)pixelRect.width / (float)pixelRect.height;


                cmd.GetTemporaryRT(_DistortionRT.id, widthHeight.x >> 1, widthHeight.y >> 1, 0, FilterMode.Bilinear, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear);
                // TODO: 传入depth做ztest。
                //RenderTargetIdentifier depthBuffer = default;
                //cmd.SetRenderTarget(_DistortionRT.id, RenderBufferLoadAction.Clear, RenderBufferStoreAction.Store,
                //    depthBuffer, RenderBufferLoadAction.Load, RenderBufferStoreAction.Store);
                cmd.SetRenderTarget(_DistortionRT.id, RenderBufferLoadAction.DontCare, RenderBufferStoreAction.Store);
                cmd.ClearRenderTarget(false, true, clearColor);
                _viewMatrix = renderingData.cameraData.camera.worldToCameraMatrix;
                _projMatrix = renderingData.cameraData.camera.projectionMatrix;
                cmd.SetViewProjectionMatrices(_viewMatrix, _projMatrix);
                cmd.SetGlobalTexture(_URPDistortionTex, _DistortionRT.id);
                context.ExecuteCommandBuffer(cmd);
                cmd.Clear();
                context.DrawRenderers(renderingData.cullResults, ref drawingSettings, ref m_FilteringSettings,
                    ref m_RenderStateBlock);
                if (m_CameraSettings.overrideCamera && m_CameraSettings.restoreCamera && !cameraData.xr.enabled)
                {
                    RenderingUtils.SetViewAndProjectionMatrices(cmd, cameraData.GetViewMatrix(), cameraData.GetGPUProjectionMatrix(), false);
                }
                // scope.Dispose();
#if UNITY_EDITOR
            }
            context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);
#endif
        }
        else
        {
            Shader.DisableKeyword(keyword);
        }

        
    }

    private static bool SetByFlag()
    {
        if (EngineContext.instance == null) return false;
        
        if (EngineContext.instance.stateflag.HasFlag(EngineContext.SFlag_Distortion))
        {
            Shader.EnableKeyword(keyword);
            // return;
        }
        else
        {
            Shader.DisableKeyword(keyword);
            return true;
        }

        return false;
    }

    public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
    {
        base.OnCameraSetup(cmd, ref renderingData);
        //cmd = new CommandBuffer {name = "Distortion Cmd"};
    }

    public override void OnCameraCleanup(CommandBuffer cmd)
    {
        // base.OnCameraCleanup(cmd);
        cmd.DisableShaderKeyword("_CUSTOM_DISTORTION");
        cmd.DisableShaderKeyword("_CUSTOM_UI_DISTORTION");
        cmd.ReleaseTemporaryRT(_DistortionRT.id);
        // EngineContext.none3DCamera = false;
        //cmd.Release();
    }

    public override void FrameCleanup(CommandBuffer cmd)
    {
        // base.FrameCleanup(cmd);

    }
}
