using UnityEngine;
using UnityEngine.Rendering;

namespace CFEngine.SRP
{
    internal struct PerFrameBuffer
    {
        public static int _Time;
        public static int _SinTime;
        public static int _CosTime;
        public static int _DeltaTime;
        public static int _TimeParameters;
    }

    internal struct PerCameraBuffer
    {
        public static int _InvCameraViewProj;
        public static int _ScaledScreenParams;
        public static int _ScreenParams;
        public static int _WorldSpaceCameraPos;
    }
    internal enum DefaultMaterialType
    {
        Standard,
        Particle,
        Terrain,
        Sprite,
        UnityBuiltinDefault
    }

    public enum RendererType
    {
        Custom,
        ForwardRenderer,
        Forward2DRenderer,
    }
    public enum PipelineDebugLevel
    {
        Disabled,
        Profiling,
    }

    
    [System.Serializable]
    public class StencilStateData
    {
        public bool overrideStencilState = false;
        public int stencilReference = 0;
        public CompareFunction stencilCompareFunction = CompareFunction.Always;
        public StencilOp passOperation = StencilOp.Keep;
        public StencilOp failOperation = StencilOp.Keep;
        public StencilOp zFailOperation = StencilOp.Keep;
    }

    public struct CameraData
    {
        public Camera camera;
        public RenderTexture targetTexture;
        internal Rect pixelRect;
        // internal int pixelWidth;
        // internal int pixelHeight;
        // internal float aspectRatio;
        public float renderScale;
        public SortingCriteria defaultOpaqueSortFlags;
        // public FlagMask Flag;
        public ClearFlag clearFlag;
        // public static uint Flag_CreateRT = 0x00000001;
        // public static uint Flag_IsOverlay = 0x00000002;
        // public static uint Flag_IsDefaultViewport = 0x00000010;
        // public static uint Flag_IsSceneView = 0x00010000;
    }

    public struct RenderingData
    {
        public CullingResults cullResults;
        public CameraData cameraData;        
        public bool supportsDynamicBatching;
#if UNITY_EDITOR
        public bool GameOverdrawView;
        public bool SceneOverdrawView;
#endif
        // public PerObjectData perObjectData;
        // internal bool resolveFinalTarget;
        public EngineContext context;
        public RenderContext rc;

    }
}