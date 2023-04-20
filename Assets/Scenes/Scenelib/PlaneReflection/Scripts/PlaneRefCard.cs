using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using Object = UnityEngine.Object;

[ExecuteAlways]
public class PlaneRefCard : MonoBehaviour
{
    const string MainCamera = "Main Camera";
    public enum ResolutionMulltiplier { Full, Half, Third, Quarter }
    [Serializable]
    public class PlanarReflectionSettings
    {
        public bool refreshRT;
        public ResolutionMulltiplier m_ResolutionMultiplier = ResolutionMulltiplier.Full;
        public float m_ClipPlaneOffset = 0.07f;
        public LayerMask m_ReflectLayers = -1;
        public bool m_Shadows;
        public bool m_skybox;
    }
    
    [SerializeField]
    public PlanarReflectionSettings m_settings = new PlanarReflectionSettings();
    public GameObject targetPlane;
    public float m_planeOffset;

    private static Camera _reflectionCamera;
    private readonly int _planarReflectionTextureId = Shader.PropertyToID("_ReflectionTex");
    private RenderTexture _reflectionTexture;
    private LayerMask _layerMaskUI;

    public bool isKawase;
    public string passTag = "KawasePass";
    private RenderTargetIdentifier buffer01;
    private RenderTargetIdentifier buffer02;
    public Material PassMat = null;


    [Range(0, 10)] public int loop = 2;
    [Range(0f, 5)] public float blur = 0.5f;
    private void OnEnable() {
        UnityEngine.Rendering.RenderPipelineManager.beginCameraRendering += runPlannarReflection;  // 订阅 beginCameraRendering 事件，加入平面反射函数
        _layerMaskUI = LayerMask.NameToLayer("UI");
    }
    private void OnDisable() {
        Cleanup();
    }

    private void OnDestroy() {
        Cleanup();
    }
    /// <summary>
    /// 释放相机
    /// </summary>
    private void Cleanup() {
        UnityEngine.Rendering.RenderPipelineManager.beginCameraRendering -= runPlannarReflection;
        if(_reflectionCamera) {  // 释放相机
            _reflectionCamera.targetTexture = null;
            SafeDestroy(_reflectionCamera.gameObject);
        }
        if (_reflectionTexture) {  // 释放纹理
            RenderTexture.ReleaseTemporary(_reflectionTexture);
        }
    }
    /// <summary>
    /// 安全的销毁相机
    /// </summary>
    /// <param name="obj:需要消耗的obj"></param>
    private static void SafeDestroy(Object obj) {
        if (Application.isEditor) {
            DestroyImmediate(obj);  //TODO
        }
        else {
            Destroy(obj);   //TODO
        }
    }
    private void runPlannarReflection(ScriptableRenderContext context, Camera camera) 
    {
        if (camera.cameraType == CameraType.Reflection  || camera.cameraType == CameraType.Preview)
            return;
        if (_reflectionCamera == null) {
            _reflectionCamera = CreateReflectCamera();
        }
        if (targetPlane == null) {
            targetPlane = gameObject;
        }

        if (Application.isPlaying)
        {
            if (camera.name != MainCamera || camera.gameObject.layer == _layerMaskUI)
            {
                return;
            }
        }
        
        var data = new PlanarReflectionSettingData();
        data.Set();
        // 同步相机设置
        UpdateReflectionCamera(camera);
        CreatePlanarReflectionTexture(camera);
        
        UniversalRenderPipeline.RenderSingleCamera(context, _reflectionCamera);
        
        if (isKawase&&_reflectionCamera&&PassMat)
        {
            BlurRT(context);
        }
        else
        {
            Shader.SetGlobalTexture(_planarReflectionTextureId, _reflectionTexture);
        }
        
        data.Restore();
    }
    private Camera CreateReflectCamera()
    {
        // new一个类型为Camera的对象
        GameObject go = new GameObject(gameObject.name + " Planar Reflection Camera",typeof(Camera));
        // 添加一个
        UniversalAdditionalCameraData cameraData = go.AddComponent(typeof(UniversalAdditionalCameraData)) as UniversalAdditionalCameraData;
        cameraData.requiresColorOption = CameraOverrideOption.Off;
        cameraData.requiresDepthOption = CameraOverrideOption.Off;
        cameraData.renderShadows = false;
        cameraData.SetRenderer(0);  // 根据 render list 的索引选择 render TODO

        Transform t = transform;
        Camera reflectionCamera = go.GetComponent<Camera>();
        reflectionCamera.transform.SetPositionAndRotation(transform.position, t.rotation);  // 相机初始位置设为当前 gameobject 位置
        reflectionCamera.depth = -10;  // 渲染优先级 [-100, 100]
        reflectionCamera.enabled = false;
        go.hideFlags = HideFlags.HideAndDontSave;

        return reflectionCamera;
    }
    private void UpdateReflectionCamera(Camera curCamera) {
        UpdateCamera(curCamera, _reflectionCamera);
        if (targetPlane == null) {
            Debug.LogError("target plane is null!");
        }
        Vector3 planeNormal = targetPlane.transform.up;
        Vector3 planePos = targetPlane.transform.position + planeNormal * m_planeOffset;
        float d = -Vector3.Dot(planeNormal, planePos);
        var reflectionPlane = new Vector4(planeNormal.x, planeNormal.y, planeNormal.z, d);
        Matrix4x4 reflectionMatri = CalculateReflectionMatrix(reflectionPlane);
        _reflectionCamera.worldToCameraMatrix = curCamera.worldToCameraMatrix * reflectionMatri;
        _reflectionCamera.cullingMask = m_settings.m_ReflectLayers; // never render water layer
        // 0 推导
        // 斜截视锥体 解决插入地表的显示错误
        // var clipPlane = CameraSpacePlane(_reflectionCamera, planePos, planeNormal, 1.0f);
        // if(Mathf.Abs(clipPlane.w)>=curCamera.farClipPlane)
        // {
        //     return;
        // }
        // var newProjectionMat = CalculateObliqueMatrix(curCamera, clipPlane);
        // _reflectionCamera.projectionMatrix = newProjectionMat;
        // 1 推导
        Vector4 clipPlane = _reflectionCamera.worldToCameraMatrix.inverse.transpose * reflectionPlane;
        //_reflectionCamera.projectionMatrix = GetObliqueMatrix(_reflectionCamera, clipPlane);
        // 2 官方提供的矩阵
        _reflectionCamera.projectionMatrix = _reflectionCamera.CalculateObliqueMatrix(clipPlane);

    }
    private void UpdateCamera(Camera src, Camera dest) {
        if (dest == null) return;
        // dest.CopyFrom(src);
        dest.aspect = src.aspect;
        dest.cameraType = src.cameraType;// 这个参数不同步就错
        if (m_settings.m_skybox)
        {
            dest.clearFlags = src.clearFlags;
        }
        else
        {
            dest.clearFlags = CameraClearFlags.Color;
            dest.backgroundColor = Color.black;
        }
        dest.fieldOfView = src.fieldOfView;
        dest.depth = src.depth;
        dest.farClipPlane = src.farClipPlane;
        dest.focalLength = src.focalLength;
        dest.useOcclusionCulling = false;
        if (dest.gameObject.TryGetComponent(out UniversalAdditionalCameraData camData)) {  // TODO
            camData.renderShadows = m_settings.m_Shadows; // 关闭反射光相机的阴影
        }
    }
    /// <summary>
    /// 计算给定平面周围的反射矩阵
    /// </summary>
    /// <param name="plane : 视空间平面"></param>
    /// <returns></returns>
    private static Matrix4x4 CalculateReflectionMatrix(Vector4 plane)
    {
        Matrix4x4 reflectionMatri = Matrix4x4.identity;
        reflectionMatri.m00 = (1F - 2F * plane[0] * plane[0]);
        reflectionMatri.m01 = (-2F * plane[0] * plane[1]);
        reflectionMatri.m02 = (-2F * plane[0] * plane[2]);
        reflectionMatri.m03 = (-2F * plane[3] * plane[0]);

        reflectionMatri.m10 = (-2F * plane[1] * plane[0]);
        reflectionMatri.m11 = (1F - 2F * plane[1] * plane[1]);
        reflectionMatri.m12 = (-2F * plane[1] * plane[2]);
        reflectionMatri.m13 = (-2F * plane[3] * plane[1]);

        reflectionMatri.m20 = (-2F * plane[2] * plane[0]);
        reflectionMatri.m21 = (-2F * plane[2] * plane[1]);
        reflectionMatri.m22 = (1F - 2F * plane[2] * plane[2]);
        reflectionMatri.m23 = (-2F * plane[3] * plane[2]);

        reflectionMatri.m30 = 0F;
        reflectionMatri.m31 = 0F;
        reflectionMatri.m32 = 0F;
        reflectionMatri.m33 = 1F;

        return reflectionMatri;
    }
    /// <summary>
    /// 设置反射相机的RT分辨率
    /// </summary>
    /// <param name="cam : 传入反射相机"></param>
    /// <param name="scale : URPasset RT缩放系数"></param>
    /// <returns></returns>
    private int2 ReflectionResolution(Camera cam, float scale) {
        var x = (int)(cam.pixelWidth * scale * GetScaleValue());
        var y = (int)(cam.pixelHeight * scale * GetScaleValue());
        return new int2(x, y);
    }
    /// <summary>
    /// 获取设置中RT缩放值
    /// </summary>
    /// <returns></returns>
    private float GetScaleValue() {
        switch(m_settings.m_ResolutionMultiplier) {
            case ResolutionMulltiplier.Full:
                return 1f;
            case ResolutionMulltiplier.Half:
                return 0.5f;
            case ResolutionMulltiplier.Third:
                return 0.33f;
            case ResolutionMulltiplier.Quarter:
                return 0.25f;
            default:
                return 0.5f; // default to half res
        }
    }
    /// <summary>
    /// 创建平面反射纹理
    /// </summary>
    /// <param name="cam : 传入相机用作创建反射RT"></param>
    private void CreatePlanarReflectionTexture(Camera cam) {
        if (_reflectionTexture == null) {
            var res = ReflectionResolution(cam, UniversalRenderPipeline.asset.renderScale);  // 获取 RT 的大小
            const bool useHdr10 = true;
            const RenderTextureFormat hdrFormat = useHdr10 ? RenderTextureFormat.RGB111110Float : RenderTextureFormat.DefaultHDR;
            _reflectionTexture = RenderTexture.GetTemporary(res.x, res.y, 16, GraphicsFormatUtility.GetGraphicsFormat(hdrFormat, true));
        }
        _reflectionCamera.targetTexture =  _reflectionTexture; // 将 RT 赋予相机
    }
    private Vector4 CameraSpacePlane(Camera cam, Vector3 pos, Vector3 normal, float sideSign) {
        var offsetPos = pos + normal * m_settings.m_ClipPlaneOffset;
        var m = cam.worldToCameraMatrix;
        var cameraPosition = m.MultiplyPoint(offsetPos);
        var cameraNormal = m.MultiplyVector(normal).normalized * sideSign;
        return new Vector4(cameraNormal.x, cameraNormal.y, cameraNormal.z, -Vector3.Dot(cameraPosition, cameraNormal));
    }
    private Matrix4x4 CalculateObliqueMatrix(Camera cam, Vector4 plane) {
        Vector4 Q_clip = new Vector4(Mathf.Sign(plane.x), Mathf.Sign(plane.y), 1f, 1f);
        Vector4 Q_view = cam.projectionMatrix.inverse.MultiplyPoint(Q_clip);

        Vector4 scaled_plane = plane * 2.0f / Vector4.Dot(plane, Q_view);
        Vector4 M3 = scaled_plane - cam.projectionMatrix.GetRow(3);
                
        Matrix4x4 new_M = cam.projectionMatrix;
        new_M.SetRow(2, M3);

        // 使用 unity API
        // var new_M = cam.CalculateObliqueMatrix(plane);
        return new_M;
    }
    // 计算斜截矩阵
    private Matrix4x4 GetObliqueMatrix(Camera camera, Vector4 viewSpaceClipPlane)
    {
        // Custom
        var M = camera.projectionMatrix;
        var m4 = new Vector4(M.m30, M.m31, M.m32, M.m33);
        var viewC = viewSpaceClipPlane;
        var clipC = M.inverse.transpose * viewC;

        var clipQ = new Vector4(Mathf.Sign(clipC.x), Mathf.Sign(clipC.y), 1, 1);
        var viewQ = M.inverse * clipQ;

        var a = 2 * Vector4.Dot(m4, viewQ) / Vector4.Dot(viewC, viewQ); 
        var aC = a * viewC;
        var newM3 = aC - m4;

        M.m20 = newM3.x;
        M.m21 = newM3.y;
        M.m22 = newM3.z;
        M.m23 = newM3.w;

        return M;

        // Unity API
        //return camera.CalculateObliqueMatrix(viewSpaceClipPlane);
    }
    private void BlurRT(ScriptableRenderContext scriptableRenderContext)
    {
        int buf01 = Shader.PropertyToID("bufferblur1");
        int buf02 = Shader.PropertyToID("bufferblur2");
        buffer01 = new RenderTargetIdentifier(buf01);
        buffer02 = new RenderTargetIdentifier(buf02);
        CommandBuffer cmd = CommandBufferPool.Get(passTag);
        cmd.GetTemporaryRT(buf01,_reflectionTexture.width,_reflectionTexture.height,0,FilterMode.Bilinear,RenderTextureFormat.ARGB32);
        cmd.GetTemporaryRT(buf02,_reflectionTexture.width,_reflectionTexture.height,0,FilterMode.Bilinear,RenderTextureFormat.ARGB32);
        cmd.SetGlobalFloat("_Blur",0);
        cmd.Blit(_reflectionTexture,buffer01,PassMat);
        for (int i = 0; i < loop; i++)
        {
            cmd.SetGlobalFloat("_Blur",(i+1)*blur);
            cmd.Blit(buffer01,buffer02,PassMat);

            var temRT = buffer01;  // 1rt 
            buffer01 = buffer02; // 1rt 
            buffer02 = temRT; //1rt 
        }
        cmd.Blit(buffer01,_reflectionTexture,PassMat);
        cmd.ReleaseTemporaryRT(buf01);
        cmd.ReleaseTemporaryRT(buf02);
        
        scriptableRenderContext.ExecuteCommandBuffer(cmd);
        CommandBufferPool.Release(cmd);
        Shader.SetGlobalTexture(_planarReflectionTextureId, _reflectionTexture);
    }
}
class PlanarReflectionSettingData {
    private readonly bool _fog;
    private readonly int _maxLod;
    private readonly float _lodBias;
    private bool _invertCulling;

    public PlanarReflectionSettingData() {
        _fog = RenderSettings.fog;
        _maxLod = QualitySettings.maximumLODLevel;
        _lodBias = QualitySettings.lodBias;
    }

    public void Set() {
        _invertCulling = GL.invertCulling;
        GL.invertCulling = !_invertCulling;  // 因为镜像后绕序会反，将剔除反向
        RenderSettings.fog = false; // disable fog for now as it's incorrect with projection
        //QualitySettings.maximumLODLevel = 1;
        //QualitySettings.lodBias = _lodBias * 0.5f;
    }

    public void Restore() {
        GL.invertCulling = _invertCulling;
        RenderSettings.fog = _fog;
        QualitySettings.maximumLODLevel = _maxLod;
        QualitySettings.lodBias = _lodBias;
    }
}
