using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net;
using System.Net.NetworkInformation;
using System.Text;

public class FPDeviceInfo
{

    public static string GetDeviceInformation()
    {
        StringBuilder sb = new StringBuilder();
        sb.AppendLine("------------基本信息V" + Version.V + "-------------");
        sb.AppendLine("设备模型：" + SystemInfo.deviceModel + "，CPU：" + SystemInfo.processorType);
        sb.AppendLine("系统内存大小（单位：MB）：" + SystemInfo.systemMemorySize + "，显存大小（单位：MB）：" + SystemInfo.graphicsMemorySize);
        sb.AppendLine("设备分辨率：" + Screen.width + "X" + Screen.height + "，DPI:" + Screen.dpi + "，系统分辨率：" + Screen.currentResolution);
        sb.AppendLine("设备名称：" + SystemInfo.deviceName + "，设备类型：" + SystemInfo.deviceType);
        sb.AppendLine("设备唯一标识符：" + SystemInfo.deviceUniqueIdentifier);
        sb.AppendLine("是否支持纹理复制：" + SystemInfo.copyTextureSupport);

        sb.AppendLine("显卡ID：" + SystemInfo.graphicsDeviceID + "，显卡名称：" + SystemInfo.graphicsDeviceName);
        sb.AppendLine("显卡类型：" + SystemInfo.graphicsDeviceType);
        sb.AppendLine("显卡供应商ID：" + SystemInfo.graphicsDeviceVendorID + "显卡供应商：" + SystemInfo.graphicsDeviceVendor);
        sb.AppendLine("显卡版本号：" + SystemInfo.graphicsDeviceVersion);
        sb.AppendLine("是否支持多线程渲染：" + SystemInfo.graphicsMultiThreaded + "，渲染目标数量：" + SystemInfo.supportedRenderTargetCount);
        sb.AppendLine("操作系统：" + SystemInfo.operatingSystem);

        sb.AppendLine("------------特性支持--------------");
        sb.AppendLine("ComputeShaders：" + SystemInfo.supportsComputeShaders + "，SSBO：" + (SystemInfo.supportsComputeShaders && SystemInfo.maxComputeBufferInputsVertex > 0));
        sb.AppendLine("AsyncGPUReadback：" + SystemInfo.supportsAsyncGPUReadback + "，GPU Instancing：" + SystemInfo.supportsInstancing);

        sb.AppendLine("RT特性（StoreAndResolveAction）：" + SystemInfo.supportsStoreAndResolveAction);
        sb.AppendLine("RT格式（ARGBHalf）：" + SystemInfo.SupportsRenderTextureFormat(RenderTextureFormat.ARGBHalf));
        sb.AppendLine("RT格式（Shadowmap）：" + SystemInfo.SupportsRenderTextureFormat(RenderTextureFormat.Shadowmap));
        sb.AppendLine("RT格式（RGB111110）：" + SystemInfo.SupportsRenderTextureFormat(RenderTextureFormat.RGB111110Float));
        sb.AppendLine("RT格式（RGB565）：" + SystemInfo.SupportsRenderTextureFormat(RenderTextureFormat.RGB565));
        sb.AppendLine("纹理格式（ETC1）：" + SystemInfo.SupportsTextureFormat(TextureFormat.ETC_RGB4));
        sb.AppendLine("纹理格式（ETC2）：" + SystemInfo.SupportsTextureFormat(TextureFormat.ETC2_RGBA8));
        sb.AppendLine("纹理格式（ASTC）：" + SystemInfo.SupportsTextureFormat(TextureFormat.ASTC_6x6));
        sb.AppendLine("纹理格式（PVRTC）：" + SystemInfo.SupportsTextureFormat(TextureFormat.PVRTC_RGBA4));
        sb.AppendLine("纹理格式（DXT5）：" + SystemInfo.SupportsTextureFormat(TextureFormat.DXT5));
        sb.Append("渲染管线：" + (UnityEngine.Rendering.GraphicsSettings.renderPipelineAsset == null ? "内置" : "URP"));

        string str = sb.ToString();
        return str;
    }

    public static RenderInfo GetRenderInfo()
    {
        RenderInfo info = new RenderInfo();

#if UNITY_EDITOR
        //switch (UnityEditor.EditorUserBuildSettings.activeBuildTarget)
        //{
        //    case UnityEditor.BuildTarget.Android:
        //        {
        //            info.Platform = "Android";
        //            break;
        //        }
        //    case UnityEditor.BuildTarget.iOS:
        //        {
        //            info.Platform = "IPhonePlayer";
        //            break;
        //        }
        //    case UnityEditor.BuildTarget.StandaloneWindows:
        //        {
        //            info.Platform = "WindowsEditor";
        //            break;
        //        }
        //    case UnityEditor.BuildTarget.StandaloneOSX:
        //        {
        //            info.Platform = "OSXEditor";
        //            break;
        //        }
        //}
        info.Platform = "WindowsEditor";
#else
        info.Platform = Application.platform.ToString();
#endif


        info.URP = UnityEngine.Rendering.GraphicsSettings.renderPipelineAsset != null;
        info.SyncCount = QualitySettings.vSyncCount;
        info.TargetFrame = Application.targetFrameRate;
        info.AactiveColorSpace = QualitySettings.activeColorSpace;
        info.LightCount = QualitySettings.pixelLightCount;
        info.TextureLimit = QualitySettings.globalTextureMipmapLimit;
        info.ShadowmaskMode = QualitySettings.shadowmaskMode;

        if (info.URP)
        {
            //#if PIPELINE_URP
            info.Shadows = UrpAsset.supportsMainLightShadows.ToString();
            info.ShadowResolution = UrpAsset.mainLightShadowmapResolution.ToString();
            info.ShadowDistance = UrpAsset.shadowDistance;
            info.ShadowCascades = UrpAsset.shadowCascadeCount;
            info.ShadowProjection = QualitySettings.shadowProjection.ToString();
            info.ShadowNearPlane = QualitySettings.shadowNearPlaneOffset;
            info.AA = (int)UrpAsset.Antialiasing;
            //info.PostScale = (int)(urpAsset.renderScale * 100);
            info.PostScale = (int)(PerformanceSetting.GetResolutionScale(UrpAsset, info.Platform) * 100.0f);
            //#endif
        }
        else
        {
            info.Shadows = QualitySettings.shadows.ToString();
            info.ShadowResolution = QualitySettings.shadowResolution.ToString();
            info.ShadowDistance = QualitySettings.shadowDistance;
            info.ShadowCascades = QualitySettings.shadowCascades;
            info.ShadowProjection = QualitySettings.shadowProjection.ToString();
            info.ShadowNearPlane = QualitySettings.shadowNearPlaneOffset;
            info.PostScale = 100;
        }

        return info;
    }
    private static UnityEngine.Rendering.Universal.UniversalRenderPipelineAsset urpAsset;
    private static UnityEngine.Rendering.Universal.UniversalRenderPipelineAsset UrpAsset
    {
        get
        {
            if (urpAsset == null)
            {
                urpAsset = UnityEngine.Rendering.GraphicsSettings.renderPipelineAsset as UnityEngine.Rendering.Universal.UniversalRenderPipelineAsset;
            }
            return urpAsset;
        }
    }
    public static void SeSrpBatch(bool enable)
    {
        UrpAsset.useSRPBatcher = enable;
    }
    public static void SetRenderInfo(RenderInfo info)
    {
        bool URP = UnityEngine.Rendering.GraphicsSettings.renderPipelineAsset != null;
        QualitySettings.pixelLightCount = info.LightCount;
        QualitySettings.globalTextureMipmapLimit = info.TextureLimit;
        QualitySettings.shadowmaskMode = info.ShadowmaskMode;
        QualitySettings.vSyncCount = info.SyncCount;
        Application.targetFrameRate = info.TargetFrame;

        if (URP)
        {

            //#if PIPELINE_URP
            //QualitySettings.shadowNearPlaneOffset = info.ShadowNearPlane;
            bool supportsMainLightShadowsBool = true;
            bool.TryParse(info.Shadows, out supportsMainLightShadowsBool);
            UrpAsset.supportsMainLightShadows = supportsMainLightShadowsBool;

            int shadowResolutionInt = 0;
            int.TryParse(info.ShadowResolution, out shadowResolutionInt);
            UrpAsset.mainLightShadowmapResolution = shadowResolutionInt;

            //urpAsset.shadowProjection = info.ShadowProjection;
            UrpAsset.shadowDistance = info.ShadowDistance;
            UrpAsset.shadowCascadeCount = info.ShadowCascades;
            UrpAsset.Antialiasing = (UnityEngine.Rendering.Universal.AntialiasingMode)info.AA;
            //urpAsset.renderScale = info.PostScale * 0.01f;
            PerformanceSetting.SetResolution(UrpAsset, info.PostScale * 0.01f);
        }
        else
        {
            QualitySettings.shadowNearPlaneOffset = info.ShadowNearPlane;
            QualitySettings.shadows = (ShadowQuality)System.Enum.Parse(typeof(ShadowQuality), info.Shadows);
            QualitySettings.shadowResolution = (ShadowResolution)System.Enum.Parse(typeof(ShadowResolution), info.ShadowResolution);
            QualitySettings.shadowProjection = (ShadowProjection)System.Enum.Parse(typeof(ShadowProjection), info.ShadowProjection);
            QualitySettings.shadowDistance = info.ShadowDistance;
            QualitySettings.shadowCascades = info.ShadowCascades;
        }

    }
    ///// <summary>
    ///// 获取mac地址
    ///// </summary>
    //void GetMacAddress()
    //{
    //    NetworkInterface[] nis = NetworkInterface.GetAllNetworkInterfaces();
    //    foreach (NetworkInterface ni in nis)
    //    {
    //        Debug.Log("Name = " + ni.Name);
    //        Debug.Log("Des = " + ni.Description);
    //        Debug.Log("Type = " + ni.NetworkInterfaceType.ToString());
    //        Debug.Log("Mac地址 = " + ni.GetPhysicalAddress().ToString());
    //        texts[16] += "   mac地址：" + ni.GetPhysicalAddress().ToString();
    //    }
    //}
}
