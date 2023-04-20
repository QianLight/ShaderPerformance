using System;
using UnityEngine;
using static UnityEngine.Rendering.Universal.GameQualitySetting;

namespace UnityEngine.Rendering.Universal
{
    [Serializable, VolumeComponentMenu("Post-processing/EnvironmentVolume")]
    public sealed class EnvironmentVolume : VolumeComponent, IPostProcessComponent
    {

        
        [Tooltip("Override Camera")]
        public BoolParameter OverrideCamera = new BoolParameter(false);

        [Tooltip("Camera FOV")]
        public ClampedFloatParameter CameraFOV = new ClampedFloatParameter(60, 1, 179);

        [Tooltip("Camera Near")]
        public FloatParameter CameraNear = new FloatParameter(0.3f);

        [Tooltip("Camera Far")]
        public FloatParameter CameraFar = new FloatParameter(1000);
        
        [Tooltip("Camera Clear Flag")]
        public CameraClearFlagsParameter CameraClearFlagsParam = new CameraClearFlagsParameter(CameraClearSigns.Skybox);

        [Tooltip("Camera Close AA")]
        public BoolParameter CameraCloseAA = new BoolParameter(false);

        [Tooltip("Override ShadowDistance")]
        public BoolParameter OverrideShadowDistance = new BoolParameter(false);

        [Tooltip("Shadow color")]
        public ColorParameter ShadowColor = new ColorParameter(Color.black);

        [Tooltip("Shadow Distance")]
        public FloatParameter ShadowDistance = new FloatParameter(25);

        [Tooltip("Depth Bias")]
        public FloatParameter DepthBias = new FloatParameter(1);

        [Tooltip("Normal Bias")]
        public FloatParameter NormalBias = new FloatParameter(0.005f);

        [Tooltip("Light Dir")]
        public StringParameter Lightbject = new StringParameter(null);

        [Tooltip("Control objects")]
        public IntParameter ControlCount = new IntParameter(0);

        [Tooltip("Controlo objects")]
        public StringParameter[] ControlObjects = new StringParameter[0];

        [Tooltip("Terrain Feature")]
        public TerrarinFuncParameter TerrainFeature = new TerrarinFuncParameter(TerrainFunc.None, false);

        [SerializeField] public bool m_CameraCull = true;
        [SerializeField] public Vector4 m_QulityDistanceScale = new Vector4(0.7f, 0.8f, 0.9f, 1.0f);
        [SerializeField] public CullLod m_CullLod0 = new CullLod("CULL_LOD0", 100);
        [SerializeField] public CullLod m_CullLod1 = new CullLod("CULL_LOD1", 50);
        [SerializeField] public CullLod m_CullLod2 = new CullLod("CULL_LOD2", 30);


        #region CloudShadow
        private static readonly int _CloudShadowTex = Shader.PropertyToID("_CloudShadowTex");
        private static readonly int _CloudShadowIntensity = Shader.PropertyToID("_CloudShadowIntensity");
        private static readonly int _MainLightSpaceMatrix = Shader.PropertyToID("_MainLightSpaceMatrix");
        private static readonly int _CloudPannerX = Shader.PropertyToID("_CloudPannerX");
        private static readonly int _CloudPannerY = Shader.PropertyToID("_CloudPannerY");
        private static readonly int _MainTil = Shader.PropertyToID("_MainTil");
        private static readonly int _CloudVerticalAttenuation = Shader.PropertyToID("_CloudVerticalAttenuation");
        [Tooltip("云朵投影Mask图")] public TextureParameter CloudShadowTex = new TextureParameter(null,false);
        public BoolParameter         EnableCloudShadow    = new BoolParameter(false);
        public FloatParameter        CloudShadowIntensity = new FloatParameter(0);
        public ClampedFloatParameter CloudShadowSpeedX    = new ClampedFloatParameter(0,-1f,1f);
        public ClampedFloatParameter CloudShadowSpeedY    = new  ClampedFloatParameter(0,-1f,1f);
        public ClampedFloatParameter CloudShadowScale     = new ClampedFloatParameter(0,-10f,10f);
        [Tooltip("云朵投影垂直方向阴影衰减")]
        public ClampedFloatParameter CloudVerticalAttenuation = new ClampedFloatParameter(.0f,0f,1f);

        #endregion

        private float cameraFov = 0;
        private float cameraNear = 0;
        private float cameraFar = 0;
        private Camera lastCamera;
        private bool lastEnable;
        private CameraClearFlags lastCameraClearFlags = CameraClearFlags.SolidColor;
        private AntialiasingMode lastAAModel;
        public override void OnFireChange(Camera camera, Transform volumeRoot, bool enable)
        {
            if (camera.IsEmpty()) return;
            lastEnable = enable;
            if (lastEnable)
            {
                LastVolumeRoot = volumeRoot;
            }
            base.OnFireChange(camera, volumeRoot, lastEnable);
            //Debug.LogWarning("OnFireChange:" + volumeRoot + "," + enable);
            SetControlObjectsAll(lastEnable); 

            SetOverrideCamera(camera, lastEnable);

            SetOverrideShadow(lastEnable);

            SetCloudShadow(lastEnable);
        }

        Matrix4x4 GetMainLightSpaceMatrix4X4()
        {
            var form = new Vector3();
            var to = new Vector3(1, 0, 0);
            var up = new Vector3(0, -1, 0);
            var obj = Lightbject.GetObjectByName();
            if (obj != null)
            {
                form = obj.position;
                to = form + obj.forward;
                up = obj.up;
            }
            Matrix4x4 modelView = Matrix4x4.LookAt(form, to, up);
 
            float cookieSize = 1000;
            Matrix4x4 newModel = new Matrix4x4();
            Matrix4x4.Inverse3DAffine(modelView, ref newModel);
            Matrix4x4 worldToLight = GL.GetGPUProjectionMatrix(Matrix4x4.Ortho(-cookieSize, cookieSize, -cookieSize, cookieSize, 0.1f, cookieSize), false) * newModel;
            return worldToLight;
        }
        
        public void SetCloudShadow(bool enable)
        {
            if (enable && CloudShadowTex.value != null && CloudShadowIntensity.value > 0 && EnableCloudShadow.value)
            {
                Shader.SetGlobalTexture(_CloudShadowTex,CloudShadowTex.value);
                Shader.SetGlobalFloat(_CloudPannerX,CloudShadowSpeedX.value);
                Shader.SetGlobalFloat(_CloudPannerY,CloudShadowSpeedY.value);
                Shader.SetGlobalFloat(_MainTil,CloudShadowScale.value);
                Shader.SetGlobalFloat(_CloudVerticalAttenuation,CloudVerticalAttenuation.value);
                Shader.SetGlobalFloat(_CloudShadowIntensity,CloudShadowIntensity.value);
                var lightSpace = GetMainLightSpaceMatrix4X4();
                Shader.SetGlobalMatrix(_MainLightSpaceMatrix,lightSpace);
            }
            else
            {
                Shader.SetGlobalTexture(_CloudShadowTex,null);
                Shader.SetGlobalFloat(_CloudShadowIntensity,0);
            }

        }
        private void SetTerrainFeature(bool isEnable)
        {
            if (isEnable)
            {
                if(TerrainFeature.overrideState)
                {
                    GameQualitySetting.TerrainFeature = TerrainFeature.value;
                    switch (TerrainFeature.value)
                    {
                        case TerrainFunc.PointLight:
                            {
                                GameQualitySetting.SetMixFuncKeyword(TerrainFeature.value);
                                break;
                            }
                        case TerrainFunc.TerrainBlend:
                            {
                                TerrainFunc resultKey = TerrainFunc.None;
                                if (GameQualitySetting.TerrainFeature == TerrainFunc.TerrainBlend)
                                    resultKey = GameQualitySetting.SetMixFuncKeyword(TerrainFunc.TerrainBlend);

                                if (resultKey == TerrainFunc.TerrainBlend)
                                    UniversalRenderPipeline.asset.SetFeature("TerrainBlending", true);
                                break;
                            }
                    }
                }
                else
                {
                    GameQualitySetting.TerrainFeature = TerrainFunc.None;
                    GameQualitySetting.SetMixFuncKeyword(GameQualitySetting.TerrainFeature);
                }
            }
            else
            {
                UniversalRenderPipeline.asset.SetFeature("TerrainBlending", false);
            }
        }

        public void SetControlObjectsAll(bool enable)
        {
            Volume[] volumes = VolumeManager.instance.GetVolumes(UnityEngine.LayerMask.GetMask("Default"));
            if (volumes.Length > 0)
            {
                if (enable)
                {
                    bool find = false;
                    foreach (Volume v in volumes)
                    {
                        if (v.enabled)
                        {
                            if (v.gameObject.transform == LastVolumeRoot)
                            {
                                find = true;
                            }
                            else
                            {
                                if (v.profileRef != null)
                                {
                                    foreach (var component in v.profileRef.components)
                                    {
                                        if (!component.active)
                                            continue;

                                        if (component.name == "EnvironmentVolume")
                                        {
                                            EnvironmentVolume tmpEv = component as EnvironmentVolume;
                                            tmpEv.SetControlObjects(false);
                                        }
                                    }
                                }
                            }
                        }
                    }

                    if (find)
                    {
                        SetControlObjects(true);
                    }
                }
                else
                {
                    SetControlObjects(enable);
                }
            }
            else
            {
                SetControlObjects(enable);
            }

            if (enable && Application.isPlaying)
            {
                GameObject obj = GameObject.Find("EditorScene/IMPOSTORS");
                if (obj != null)
                {
                    obj.SendMessage("DoImpostorEnable", SendMessageOptions.DontRequireReceiver);
                }
            }
        }

        public void SetControlObjects(bool enable)
        {
            if (!string.IsNullOrEmpty(Lightbject.value))
            {
                Lightbject.SetObject(enable);
            }

            if (ControlObjects != null)
            {
                for (int i = 0; i < ControlObjects.Length; i++)
                {
                    StringParameter sp = ControlObjects[i];
                    if (sp != null && !string.IsNullOrEmpty(sp.value))
                    {
                        Transform tmp = sp.SetObject(enable);
                        if(tmp != null && enable)
                        {
                            GameObject g = tmp.gameObject;
                            if (g != null)
                            {
                                g.SendMessage("SetLightProbes", SendMessageOptions.DontRequireReceiver);
                            }
                        }
                    }
                }
            }
        }

        public void SetOverrideCamera(Camera camera, bool enable)
        {
            if (camera == null)
                return;
            if (OverrideCamera.value)
            {
                if (enable)
                {
                    cameraFov = camera.fieldOfView;
                    cameraNear = camera.nearClipPlane;
                    cameraFar = camera.farClipPlane;

                    camera.fieldOfView = CameraFOV.value;
                    camera.nearClipPlane = CameraNear.value;
                    camera.farClipPlane = CameraFar.value;
                    lastCamera = camera;
                }
                else
                {
                    camera.fieldOfView = cameraFov;
                    camera.nearClipPlane = cameraNear;
                    camera.farClipPlane = cameraFar;
                    lastCamera = null;
                }
            }
            //set Default
            else
            {
                camera.farClipPlane = 800f;
                lastCamera = camera;
            }

            if(enable)
            {
                if(m_CameraCull)
                {
                    int lod0Layer = LayerMask.NameToLayer(m_CullLod0.LayerName);
                    int lod1Layer = LayerMask.NameToLayer(m_CullLod1.LayerName);
                    int lod2Layer = LayerMask.NameToLayer(m_CullLod2.LayerName);
#if UNITY_EDITOR
                    camera.cullingMask |= 1 << lod0Layer | 1 << lod1Layer | 1 << lod2Layer;
#endif
                    if(Lightbject.overrideState && !string.IsNullOrEmpty(Lightbject.value))
                    {
                        Transform t = Lightbject.GetObjectByName();
                        if(t != null)
                        {
                            Light l = t.GetComponent<Light>();
                            if(l != null)
                            {
                                l.cullingMask |= 1 << lod0Layer | 1 << lod1Layer | 1 << lod2Layer;
                            }
                        }
                    }
                    float[] layerCull = camera.layerCullDistances;
                    float scale = getQulityLevelScale();
                    layerCull[lod0Layer] = m_CullLod0.Distance * scale;
                    layerCull[lod1Layer] = m_CullLod1.Distance * scale;
                    layerCull[lod2Layer] = m_CullLod2.Distance * scale;
                    camera.layerCullDistances = layerCull;
                }
            }
            else
            {
                if (m_CameraCull)
                {
                    float[] layerCull = camera.layerCullDistances;
                    layerCull[LayerMask.NameToLayer(m_CullLod0.LayerName)] = camera.farClipPlane;
                    layerCull[LayerMask.NameToLayer(m_CullLod1.LayerName)] = camera.farClipPlane;
                    layerCull[LayerMask.NameToLayer(m_CullLod2.LayerName)] = camera.farClipPlane;
                    camera.layerCullDistances = layerCull;
                }
            }

            if (enable)
            {
                if (CameraClearFlagsParam.overrideState)
                {
                    lastCameraClearFlags = camera.clearFlags;
                    camera.clearFlags = (CameraClearFlags)CameraClearFlagsParam.value;
                }
            }
            else
            {
                if (CameraClearFlagsParam.overrideState)
                {
                    camera.clearFlags = lastCameraClearFlags;
                }
            }

            if(enable)
            {
                if(CameraCloseAA.overrideState && CameraCloseAA.value)
                {
                    lastAAModel = UniversalRenderPipeline.asset.Antialiasing;
                    UniversalRenderPipeline.asset.Antialiasing = AntialiasingMode.None;
                }
            }
            else
            {
                if (CameraCloseAA.overrideState && CameraCloseAA.value)
                {
                    UniversalRenderPipeline.asset.Antialiasing = lastAAModel;
                }
            }
        }

        public void SetCamera()
        {
            SetOverrideCamera(lastCamera, lastEnable);
        }
        public void SetOverrideShadow(bool enable)
        {
            if (OverrideShadowDistance.value)
            {
                SetShadowDistance(enable ? 0 : 2, ShadowDistance.value, DepthBias.value, NormalBias.value);
            }
            else
            {
                if (enable)
                {
                    SetShadowDistance(1, 25, 1, 0.005f);
                }
            }
            if(ShadowColor.overrideState && enable)
            {
                Shader.SetGlobalColor("urp_ShadowColor", ShadowColor.value);
            }
            else
            {
                Shader.SetGlobalColor("urp_ShadowColor", Color.black);
            }
            SetTerrainFeature(lastEnable);
        }

        private float lastDistance = 0, lastDepthBias = 0, lastNormalBias = 0;

        public object LightmapVolumn { get; private set; }

        void SetShadowDistance(int state, float distance, float depthBias, float normalBias)
        {
            UniversalRenderPipelineAsset urpAsset = GraphicsSettings.renderPipelineAsset as UniversalRenderPipelineAsset;
            distance = distance * getQulityLevelScale();
            if (urpAsset != null)
            {
                switch (state)
                {
                    case 0:
                        {
                            lastDistance = urpAsset.shadowDistance;
                            lastDepthBias = urpAsset.shadowDepthBias;
                            lastNormalBias = urpAsset.shadowNormalBias;
                            urpAsset.shadowDistance = distance;
                            urpAsset.shadowDepthBias = depthBias;
                            urpAsset.shadowNormalBias = normalBias;
                            break;
                        }
                    case 1:
                        {
                            urpAsset.shadowDistance = distance;
                            urpAsset.shadowDepthBias = depthBias;
                            urpAsset.shadowNormalBias = normalBias;
                            break;
                        }
                    case 2:
                        {
                            urpAsset.shadowDistance = lastDistance;
                            urpAsset.shadowDepthBias = lastDepthBias;
                            urpAsset.shadowNormalBias = lastNormalBias;
                            break;
                        }
                }

            }
        }
        float getQulityLevelScale()
        {
            switch(GameQualitySetting.ResolutionLevel)
            {
                case RenderQualityLevel.Ultra:
                    {
                        return m_QulityDistanceScale.z;
                    }
                case RenderQualityLevel.High:
                    {
                        return m_QulityDistanceScale.w;
                    }
                case RenderQualityLevel.Medium:
                    {
                        return m_QulityDistanceScale.y;
                    }
                default:
                    return m_QulityDistanceScale.x;
            }
        }
        public static Transform LastVolumeRoot = null;
        public bool GUIUpdate()
        {
            if (Target == null)
            {
                return false;
            }
            else
            {
                if ((LastVolumeRoot != null) && (LastVolumeRoot != Target))
                    return false;
            }

            if (OverrideCamera.value && lastCamera != null)
            {
                lastCamera.fieldOfView = CameraFOV.value;
                lastCamera.nearClipPlane = CameraNear.value;
                lastCamera.farClipPlane = CameraFar.value;
            }
        
            if (OverrideShadowDistance.value)
            {
                SetShadowDistance(1, ShadowDistance.value, DepthBias.value, NormalBias.value);
            }
            if (ShadowColor.overrideState)
            {
                Shader.SetGlobalColor("urp_ShadowColor", ShadowColor.value);
            }
            else
            {
                Shader.SetGlobalColor("urp_ShadowColor", Color.black);
            }
            return true;
        }


        //protected override void OnEnable()
        //{
        //    base.OnEnable();
        //    if (!string.IsNullOrEmpty(RootObject.value))
        //    {
        //        RootObject.SetObject(false);
        //    }

        //}
        /*
         */
        public bool IsActive() => Lightbject.value != null;

        public bool IsTileCompatible() => true;
    }
    [System.Serializable]
    public class CullLod
    {
        public CullLod(string name, float distance)
        {
            LayerName = name;
            Distance = distance;
        }
        public string LayerName;
        public float Distance;
    }

    public enum CameraClearSigns
    {
        Skybox = 1,
        SolidColor = 2,
    }

    
    [Serializable]
    public sealed class CameraClearFlagsParameter : VolumeParameter<CameraClearSigns> 
    { 
        public CameraClearFlagsParameter(CameraClearSigns value, bool overrideState = false) : base(value, overrideState) 
        { } 
    }

    [Serializable]
    public sealed class TerrarinFuncParameter : VolumeParameter<TerrainFunc>
    {
        public TerrarinFuncParameter(TerrainFunc value, bool overrideState = false) : base(value, overrideState)
        { }
    }
}
