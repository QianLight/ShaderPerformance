using System;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class SSPlanarReflectionFeature : ScriptableRendererFeature
{
    static bool Enable = true;
    public SSRPSetting setting;
    private SSPlanarReflectionPass _ssrpPass;
    private ReflectData _data;
    private Dictionary<Camera, CameraData> _cameraDataDic;
    public static RenderTexture m_reflectTexture;
    [Serializable]
    public class SSRPSetting
    {
        public ComputeShader ssrpCS;
        [HideInInspector][Range(0.1f, 1f)]  public  float Scale = 1f;
        [HideInInspector] public  float WaterLevel = 0f;
        [HideInInspector]public float stretchIntensity = 4.5f;
        [HideInInspector]public float stretchThreshold = 0.2f;

        public RenderPassEvent renderEvent = RenderPassEvent.AfterRenderingOpaques;
        public bool blur;
        public Shader blurShader;
        public float m_blurOffset = 1f;
        public int m_blurCount = 2;

        public void Init()
        {
#if UNITY_EDITOR
            if (ssrpCS == null)
                ssrpCS = AssetDatabase.LoadAssetAtPath<ComputeShader>(
                    "Packages/com.unity.render-pipelines.universal/Runtime/SSPR/SSPlanarReflection.compute");
            
#endif
        }
    }

    public class ReflectData
    {
        public SSRPSetting setting;
        public RenderTexture reflectTexture;
        public ComputeBuffer computeBuffer;
        public int groupX;

        public int groupY;
        public Material blurMaterial;
    }

    private class CameraData
    {
        public RenderTexture reflTexture;
        public ComputeBuffer computeBuffer;
        public void ClearDepth()
        {


            if (computeBuffer != null)
            {
                computeBuffer.Dispose();
                computeBuffer = null;
            }
        }
        
        public void Clear()
        {
            if (reflTexture != null)
            {
                Debug.Log("Clear");
                reflTexture.Release();
                reflTexture = null;
            }

            if (computeBuffer != null)
            {
                computeBuffer.Dispose();
                computeBuffer = null;
            }
        }
    }

    public bool m_isHUAWEI = false;
    public override void Create()
    {
        Clear();
        if (setting == null)
        {
            setting = new SSRPSetting();
            setting.Init();
        }
        if (SystemInfo.deviceModel.ToUpper().Contains("HUAWEI"))
        {
            m_isHUAWEI = true;
        }
        else
        {

            m_isHUAWEI = false;
        }
        _cameraDataDic = new Dictionary<Camera, CameraData>(1);
        _data = new ReflectData();
        _ssrpPass = new SSPlanarReflectionPass();
    }

    private ScreenSpacePlannarReflection _sspr;
    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        var stack = VolumeManager.instance.stack;
        _sspr = stack.GetComponent<ScreenSpacePlannarReflection>();
        if (_sspr.IsActive()&&!m_isHUAWEI)
        {
            setting.Scale = _sspr.m_scale.value;
            setting.WaterLevel = _sspr.m_waterLevel.value;
            setting.stretchIntensity = _sspr.m_StretchInensity.value;
            setting.stretchThreshold = _sspr.m_StretchThreshold.value;
            SetData(ref renderingData);
            _ssrpPass.Setup(_data);
            _ssrpPass.source = renderer.cameraColorTarget;
            Shader.SetGlobalTexture("_SSPlanarReflectionTexture", m_reflectTexture);
            renderer.EnqueuePass(_ssrpPass);
        }
        else
        {
            Shader.SetGlobalTexture("_SSPlanarReflectionTexture",Texture2D.blackTexture);
        }

    }

    private void SetData(ref RenderingData renderingData)
    {
        ref var desc = ref renderingData.cameraData.cameraTargetDescriptor;
        var camera = renderingData.cameraData.camera;
        if (!renderingData.cameraData.isMainCamera)
        {
            return;
        }
        if (!_cameraDataDic.TryGetValue(camera, out var cameraData))
        {
            cameraData = new CameraData();
            _cameraDataDic.Add(camera, cameraData);
        }

        var width = Mathf.CeilToInt(desc.width * setting.Scale);
        var height = Mathf.CeilToInt(desc.height * setting.Scale);
        _data.setting = setting;
        _data.groupX = Mathf.CeilToInt(width / 8f);
        _data.groupY = Mathf.CeilToInt(height / 8f);
        _data.computeBuffer = GetComputeBuffer(cameraData);
       //_data.reflectTexture =  GetReflectTexture(width, height, cameraData,"_SSPlanarReflectionTexture");
        if (m_reflectTexture == null)
        {
            m_reflectTexture =CreateRenderTexture(width, height,"_SSPlanarReflectionTexture");
        }
        else
        {
            if (m_reflectTexture.width != width ||m_reflectTexture.height != height)
            {
                m_reflectTexture.Release();
                m_reflectTexture = CreateRenderTexture(width, height,"_SSPlanarReflectionTexture");
            }
        }


        if (setting.blur && _data.blurMaterial == null)
            _data.blurMaterial = new Material(setting.blurShader);
    }

    private void OnDestroy()
    {
        if (m_reflectTexture != null)
        {
            m_reflectTexture.Release();
        }
        Clear();
    }

    private void OnDisable()
    {
        if (m_reflectTexture != null)
        {
            m_reflectTexture.Release();
        }

        Clear();
    }

    private void Clear()
    {
        if (_cameraDataDic == null) return;
        foreach (var cameraData in _cameraDataDic.Values)
        {
            cameraData.Clear();
        }

        _cameraDataDic.Clear();
    }

    private ComputeBuffer GetComputeBuffer(CameraData cameraData)
    {
        var length = _data.groupX * _data.groupY * 64;

        if (cameraData.computeBuffer == null)
        {
            cameraData.computeBuffer = new ComputeBuffer(length, sizeof(uint));
        }
        else if (cameraData.computeBuffer.count < length)
        {
            cameraData.computeBuffer.Dispose();
            cameraData.computeBuffer = new ComputeBuffer(length, sizeof(uint));
        }

        return cameraData.computeBuffer;
    }
    


    private RenderTexture GetReflectTexture(int width, int height, CameraData cameraData,string name)
    {
        if (cameraData.reflTexture == null)
        {
            cameraData.reflTexture = CreateRenderTexture(width, height,name);

        }
        else
        {
            if (cameraData.reflTexture.width != width || cameraData.reflTexture.height != height)
            {
                cameraData.reflTexture.Release();
                cameraData.reflTexture = CreateRenderTexture(width, height,name);
            }
        }
        return cameraData.reflTexture;
    }

    private RenderTexture CreateRenderTexture(int width, int height,string name)
    {
        var reflectTex = new RenderTexture(width, height, 0, GraphicsFormat.R32G32B32A32_SFloat);
        reflectTex.name = name;//"_SSPlanarReflectionTexture";
        reflectTex.enableRandomWrite = true;
        // reflectTex.filterMode = FilterMode.Point;
        reflectTex.Create();
        return reflectTex;
    }

    public static void SetSSPREnable(bool val)
    {
        Enable = val;
    }
}

public class SSPlanarReflectionPass : ScriptableRenderPass
{
    static readonly int _Param1 = Shader.PropertyToID("_Param1");
    static readonly int _Param2 = Shader.PropertyToID("_Param2");
    static readonly int _PlanarReflectionTexture = Shader.PropertyToID("_SSPlanarReflectionTexture");
    static readonly int _PlanarReflectionDepthTexture = Shader.PropertyToID("_SSPlanarReflectionDepthTexture");
    static readonly int _ReflectBuffer = Shader.PropertyToID("_ReflectBuffer");
    static readonly int _CompareBuffer = Shader.PropertyToID("_CompareBuffer");

    static readonly int _ReflectBlurTemp = Shader.PropertyToID("_ReflectBlurTemp");
    static readonly int _sourceTemp = Shader.PropertyToID("_SourceTemp");
    const string m_ProfilerTag = "SSRP Project";
    const int KernelClear = 0;
    const int KernelProject =1;

    const int KernelFillHole = 2;
    public float m_blurOffset = 1;
    public int m_blurCount = 2;
    public RenderTargetIdentifier source;
    private ComputeShader _ssrpCS;
    private SSPlanarReflectionFeature.ReflectData _data;

    public void Setup(SSPlanarReflectionFeature.ReflectData data)
    {
        _data = data;
        if (data == null || data.setting == null)
        {
            return;
        }
        renderPassEvent = data.setting.renderEvent - 1;
        _ssrpCS = data.setting.ssrpCS;
        m_blurOffset = data.setting.m_blurOffset;
        m_blurCount = data.setting.m_blurCount;
    }
    

    public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
    {
        if (_ssrpCS == null) return;
        if (!CheckSystemSupported())
        {
            Shader.SetGlobalTexture("_SSPlanarReflectionTexture",Texture2D.blackTexture);
            SSPlanarReflectionFeature.SetSSPREnable(false);
            return;
        }

        var camera = renderingData.cameraData.camera;
        if (!renderingData.cameraData.isMainCamera)
        {
            return;
        }
        CommandBuffer cmd = CommandBufferPool.Get(m_ProfilerTag);
        // set params
        cmd.SetComputeVectorParam(_ssrpCS, _Param1,
            new Vector4(_data.groupX * 8, _data.groupY * 8, _data.setting.WaterLevel, _data.setting.Scale));
        
        var cameraDirX = camera.transform.eulerAngles.x;
        cameraDirX = cameraDirX > 180 ? cameraDirX - 360 : cameraDirX;
        cameraDirX *= 0.00001f;
        cmd.SetComputeVectorParam(_ssrpCS, _Param2,
            new Vector4(_data.setting.stretchIntensity, _data.setting.stretchThreshold, cameraDirX));

        RenderTextureDescriptor opaqueDesc = renderingData.cameraData.cameraTargetDescriptor;
        cmd.GetTemporaryRT(_sourceTemp,opaqueDesc);
        cmd.Blit(source,_sourceTemp);
        cmd.SetGlobalTexture("_TempSourceTexture",_sourceTemp);

        // clear
        cmd.SetComputeTextureParam(_ssrpCS, KernelClear, _PlanarReflectionTexture, SSPlanarReflectionFeature.m_reflectTexture);
        cmd.SetComputeBufferParam(_ssrpCS, KernelClear, _ReflectBuffer, _data.computeBuffer);
        cmd.DispatchCompute(_ssrpCS, KernelClear, _data.groupX, _data.groupY, 1);
        
        // project
        cmd.SetComputeTextureParam(_ssrpCS, KernelProject, _PlanarReflectionTexture, SSPlanarReflectionFeature.m_reflectTexture);
        cmd.SetComputeBufferParam(_ssrpCS, KernelProject, _ReflectBuffer, _data.computeBuffer);
        cmd.DispatchCompute(_ssrpCS, KernelProject, _data.groupX, _data.groupY, 1);
        
        
        // fill hole
        cmd.SetComputeTextureParam(_ssrpCS, KernelFillHole, _PlanarReflectionTexture, SSPlanarReflectionFeature.m_reflectTexture);
        cmd.SetComputeBufferParam(_ssrpCS, KernelFillHole, _ReflectBuffer, _data.computeBuffer);
        cmd.DispatchCompute(_ssrpCS, KernelFillHole, _data.groupX, _data.groupY, 1);


        // set reflect result
        if (_data.setting.blur)
        {
            cmd.GetTemporaryRT(_ReflectBlurTemp, SSPlanarReflectionFeature.m_reflectTexture.descriptor, FilterMode.Bilinear);
            _data.blurMaterial.SetFloat("_BlurSize",m_blurOffset);
            for (int i = 0; i < m_blurCount; i++)
            {
                cmd.Blit(SSPlanarReflectionFeature.m_reflectTexture, _ReflectBlurTemp, _data.blurMaterial);
                cmd.Blit(_ReflectBlurTemp, SSPlanarReflectionFeature.m_reflectTexture, _data.blurMaterial);
            }
            cmd.ReleaseTemporaryRT(_ReflectBlurTemp);
        }
        //SSPlanarReflectionFeature.m_reflectTexture = _data.reflectTexture;
        cmd.SetGlobalTexture(_PlanarReflectionTexture, SSPlanarReflectionFeature.m_reflectTexture);

        // compute shader之后需要重新设置RenderTarget（？）
        cmd.Blit(_sourceTemp,source);
        cmd.ReleaseTemporaryRT(_sourceTemp);
        
        context.ExecuteCommandBuffer(cmd);
        CommandBufferPool.Release(cmd);
    }

    public static bool CheckSystemSupported()
    {
        
        var supportCs = SystemInfo.supportsComputeShaders&& SystemInfo.maxComputeBufferInputsCompute >0 ;
        if (GameQualitySetting.SFXLevel != RenderQualityLevel.Ultra 
           )
        {
            supportCs = false;
        }

        #if UNITY_EDITOR
                supportCs = true;
        #endif
        return supportCs;
    }
}