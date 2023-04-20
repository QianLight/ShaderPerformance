using System;
using CFEngine;
using System.Collections;
using System.Collections.Generic;
using GSDK;
using UnityEngine;
using UnityEngine.PlayerLoop;
using UnityEngine.Rendering.Universal;

public class WaveCamSetting : MonoBehaviour
{
    private Camera m_waveCam;
    bool _isInit;
    void Start()
    {
        m_waveCam = this.GetComponent<Camera>();
        m_waveCam.aspect = 1f;
        m_waveCam.orthographic = true;
        m_waveCam.backgroundColor = Color.black;
        this.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
        _isInit = false;
    }

    private void Update()
    {
        if (GameQualitySetting.SFXLevel != RenderQualityLevel.Ultra
            && GameQualitySetting.SFXLevel != RenderQualityLevel.High)
        {
            m_waveCam.enabled = false;
            return;
        }

        m_waveCam.enabled = true;
        
        this.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
        Shader.SetGlobalVector("_WaveCameraPos", new Vector4(m_waveCam.transform.position.x, m_waveCam.transform.position.y, m_waveCam.transform.position.z, m_waveCam.orthographicSize));
    }
}

public class WaveCameraInitializer: CameraInitializer
{
    private static WaveCameraInitializer _waveCamInitializer = new WaveCameraInitializer();
    public static WaveCameraInitializer Initializer
    {
        get
        {
            if (_waveCamInitializer == null)
                _waveCamInitializer = new WaveCameraInitializer();
            return _waveCamInitializer;
        }
    }
    private WaveCameraInitializer() { }
    
    private static Camera _waveCamera;
    public override void Init()
    {
        if (_waveCamera == null)
        {
            CreateWaveCamera();
        }

        // Active and Enable Wave Camera
        {
            _waveCamera.gameObject.SetActive(true);
            _waveCamera.enabled = true;
            var position = UrpCameraStackTag.sceneCamera.Camera.transform.position;
            _waveCamera.transform.position = new Vector3(position.x, 1000, position.z);
        }
    }
    private void CreateWaveCamera()
    {
        _waveCamera = UrpCameraStackContext.CreateNewCamera(out UniversalAdditionalCameraData waveCamUacd,CameraRenderType.Overlay,"waveParticleCamera",UrpCameraTag.Stack);
        _waveCamera.orthographic = true;
        _waveCamera.orthographicSize = 100;
        _waveCamera.cullingMask = 1 << 31; // Culling Mask: InVisiblity
        waveCamUacd.renderShadows = false;
        waveCamUacd.SetRenderer(1); // Renderer: WaveParticleRenderer

        _waveCamera.gameObject.AddComponent<WaveCamSetting>();
        
    }
}
