using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

[ExecuteInEditMode]
[RequireComponent(typeof(Camera), typeof(UniversalAdditionalCameraData))]
public class Mirror : MonoBehaviour
{
    public enum MirrorCameraOverride
    {
        UseSourceCameraSettings,
        Off,
    }


    public enum OutputScope
    {
        Global,
        Local,
    }

    [SerializeField]
    float m_Offset;


    [SerializeField]
    float m_TextureScale;

    [SerializeField]
    MirrorCameraOverride m_AllowHDR;

    [SerializeField]
    MirrorCameraOverride m_AllowMSAA;
    public Transform m_PlaneTrans;

    Camera m_ReflectionCamera;
    UniversalAdditionalCameraData m_CameraData;
    RenderTexture m_RenderTexture;
    RenderTextureDescriptor m_PreviousDescriptor;

    public Mirror()
    {
        m_Offset = 0.01f;
        m_TextureScale = 1.0f;
        m_AllowHDR = MirrorCameraOverride.UseSourceCameraSettings;
        m_AllowMSAA = MirrorCameraOverride.UseSourceCameraSettings;
    }

    public float offest
    {
        get => m_Offset;
        set => m_Offset = value;
    }

    public float textureScale
    {
        get => m_TextureScale;
        set => m_TextureScale = value;
    }

    public MirrorCameraOverride allowHDR
    {
        get => m_AllowHDR;
        set => m_AllowHDR = value;
    }

    public MirrorCameraOverride allowMSAA
    {
        get => m_AllowMSAA;
        set => m_AllowMSAA = value;
    }

    Camera reflectionCamera
    {
        get
        {
            if (m_ReflectionCamera == null)
                m_ReflectionCamera = GetComponent<Camera>();
            return m_ReflectionCamera;
        }
    }

    UniversalAdditionalCameraData cameraData
    {
        get
        {
            if (m_CameraData == null)
                m_CameraData = GetComponent<UniversalAdditionalCameraData>();
            return m_CameraData;
        }
    }

    void OnEnable()
    {
        // Callbacks
        UnityEngine.Rendering.RenderPipelineManager.beginCameraRendering += BeginCameraRendering;

        // Initialize Components
        InitializeCamera();
    }

    void OnDisable()
    {
        // Callbacks
        UnityEngine.Rendering.RenderPipelineManager.beginCameraRendering -= BeginCameraRendering;

        // Dispose RenderTexture
        SafeDestroyObject(m_RenderTexture);
    }
    void InitializeCamera()
    {
        // Setup Camera
        reflectionCamera.cameraType = CameraType.Reflection;
        reflectionCamera.targetTexture = m_RenderTexture;

        // Setup AdditionalCameraData
        cameraData.renderShadows = false;
        cameraData.requiresColorOption = CameraOverrideOption.Off;
        cameraData.requiresDepthOption = CameraOverrideOption.Off;
    }
    RenderTextureDescriptor GetDescriptor(Camera camera)
    {
        // Get scaled Texture size
        var width = (int)Mathf.Max(camera.pixelWidth * textureScale, 4);
        var height = (int)Mathf.Max(camera.pixelHeight * textureScale, 4);

        // Get Texture format
        var hdr = allowHDR == MirrorCameraOverride.UseSourceCameraSettings ? camera.allowHDR : false;
        var renderTextureFormat = hdr ? RenderTextureFormat.DefaultHDR : RenderTextureFormat.Default;
        return new RenderTextureDescriptor(width, height, renderTextureFormat, 16) { autoGenerateMips = true, useMipMap = true };
    }
    void BeginCameraRendering(ScriptableRenderContext context, Camera camera)
    {
        // Never render Mirrors for Preview or Reflection cameras
        if (!camera.CompareTag("MainCamera"))
        {
            return;
        }
        if (m_PlaneTrans == null)
            return;

        // Profiling command
        CommandBuffer cmd = CommandBufferPool.Get($"Mirror {gameObject.GetInstanceID()}");
        using (new ProfilingSample(cmd, $"Mirror {gameObject.GetInstanceID()}"))
        {
            ExecuteCommand(context, cmd);

            // Test for Descriptor changes
            var descriptor = GetDescriptor(camera);
            if (!descriptor.Equals(m_PreviousDescriptor))
            {
                // Dispose RenderTexture
                if (m_RenderTexture != null)
                {
                    SafeDestroyObject(m_RenderTexture);
                }

                // Create new RenderTexture
                m_RenderTexture = new RenderTexture(descriptor);
                m_PreviousDescriptor = descriptor;
                reflectionCamera.targetTexture = m_RenderTexture;
            }

            // Execute
            RenderMirror(context, camera);
            SetShaderUniforms(context, m_RenderTexture, cmd);
        }
        ExecuteCommand(context, cmd);
    }

    void RenderMirror(ScriptableRenderContext context, Camera camera)
    {
        Vector3 pos = m_PlaneTrans.position;
        Vector3 normal = m_PlaneTrans.up;
        UpdateCameraModes(camera, reflectionCamera);

        float d = -Vector3.Dot(normal, pos) - m_Offset;
        Vector4 reflectionPlane = new Vector4(normal.x, normal.y, normal.z, d);
        Matrix4x4 reflection = Matrix4x4.zero;
        CalculateReflectionMatrix(ref reflection, reflectionPlane);

        Vector3 oldpos = camera.transform.position;
        Vector3 newpos = reflection.MultiplyPoint(oldpos);
        reflectionCamera.worldToCameraMatrix = camera.worldToCameraMatrix * reflection;
        // Setup oblique projection matrix so that near plane is our reflection
        // plane. This way we clip everything below/above it for free.
        Vector4 clipPlane = CameraSpacePlane(reflectionCamera, pos, normal, 1.0f);
        Matrix4x4 projection = camera.CalculateObliqueMatrix(clipPlane);
        reflectionCamera.projectionMatrix = projection;

        GL.invertCulling = true;
        reflectionCamera.transform.position = newpos;
        Vector3 euler = camera.transform.eulerAngles;
        reflectionCamera.transform.eulerAngles = new Vector3(0, euler.y, euler.z);
        UniversalRenderPipeline.RenderSingleCamera(context, reflectionCamera);
        reflectionCamera.transform.position = oldpos;
        GL.invertCulling = false;
    }
    private static void CalculateReflectionMatrix(ref Matrix4x4 reflectionMat, Vector4 plane)
    {
        reflectionMat.m00 = (1F - 2F * plane[0] * plane[0]);
        reflectionMat.m01 = (-2F * plane[0] * plane[1]);
        reflectionMat.m02 = (-2F * plane[0] * plane[2]);
        reflectionMat.m03 = (-2F * plane[3] * plane[0]);

        reflectionMat.m10 = (-2F * plane[1] * plane[0]);
        reflectionMat.m11 = (1F - 2F * plane[1] * plane[1]);
        reflectionMat.m12 = (-2F * plane[1] * plane[2]);
        reflectionMat.m13 = (-2F * plane[3] * plane[1]);

        reflectionMat.m20 = (-2F * plane[2] * plane[0]);
        reflectionMat.m21 = (-2F * plane[2] * plane[1]);
        reflectionMat.m22 = (1F - 2F * plane[2] * plane[2]);
        reflectionMat.m23 = (-2F * plane[3] * plane[2]);

        reflectionMat.m30 = 0F;
        reflectionMat.m31 = 0F;
        reflectionMat.m32 = 0F;
        reflectionMat.m33 = 1F;
    }

    Vector4 CameraSpacePlane(Camera camera, Vector3 pos, Vector3 normal, float sideSign)
    {
        // Calculate mirror plane in camera space.
        Vector3 offsetPos = pos + normal * m_Offset;
        var cpos = camera.worldToCameraMatrix.MultiplyPoint(offsetPos);
        var cnormal = camera.worldToCameraMatrix.MultiplyVector(normal).normalized * sideSign;
        return new Vector4(cnormal.x, cnormal.y, cnormal.z, -Vector3.Dot(cpos, cnormal));
    }
    void SetShaderUniforms(ScriptableRenderContext context, RenderTexture renderTexture, CommandBuffer cmd)
    {
        cmd.SetGlobalTexture("_ReflectionMap", renderTexture);
        ExecuteCommand(context, cmd);

    }
    void ExecuteCommand(ScriptableRenderContext context, CommandBuffer cmd)
    {
        context.ExecuteCommandBuffer(cmd);
        cmd.Clear();
    }

    #region Object
    void SafeDestroyObject(Object obj)
    {
        if (obj == null)
            return;

#if UNITY_EDITOR
        DestroyImmediate(obj);
#else
            Destroy(obj);
#endif
    }
    #endregion

    private void UpdateCameraModes(Camera src, Camera dest)
    {
        if (dest == null)
            return;
        // set camera to clear the same way as current camera
        dest.clearFlags = src.clearFlags;
        dest.backgroundColor = src.backgroundColor;

        // update other values to match current camera.
        // even if we are supplying custom camera&projection matrices,
        // some of values are used elsewhere (e.g. skybox uses far plane)
        dest.farClipPlane = src.farClipPlane;
        dest.nearClipPlane = src.nearClipPlane;
        dest.orthographic = src.orthographic;
        dest.fieldOfView = src.fieldOfView;
        dest.aspect = src.aspect;
        dest.orthographicSize = src.orthographicSize;
    }

}