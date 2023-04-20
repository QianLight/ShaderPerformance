using System;
using System.Collections.Generic;
using CFClient;
using CFEngine;
using Unity.Mathematics;
using UnityEngine.Rendering.Universal.Internal;

namespace UnityEngine.Rendering.Universal
{
    [Serializable]
    public struct LightInfo
    {
        public Color color;
        public Vector3 LightDir;
        
        // [NonSerialized][Range(-180f, 180f)] public float horizontal;
        // [NonSerialized][Range(-90f, 90f)] public float vertical;
        
        public void SetDirection(Vector3 direction)
        {
            // direction = direction.normalized;
            // horizontal = Mathf.Atan2(direction.z, direction.x) * Mathf.Rad2Deg;
            // float horizontalLength = Mathf.Sqrt(direction.z * direction.z + direction.x * direction.x);
            // vertical = Mathf.Atan2(direction.y, horizontalLength) * Mathf.Rad2Deg;

            LightDir = direction;
        }

        public Vector3 GetDirection()
        {
            // Vector3 direction;
            // float radHorizontal = horizontal * Mathf.Deg2Rad;
            // float radVertical = vertical * Mathf.Deg2Rad;
            // float y = Mathf.Sin(radVertical);
            // float horizontalLength = Mathf.Cos(radVertical);
            // float x = horizontalLength * Mathf.Cos(radHorizontal);
            // float z = horizontalLength * Mathf.Sin(radHorizontal);
            // direction = new Vector3(x, y, z);
            // direction.Normalize();
            // return direction;

            return LightDir;
        }

        public static LightInfo CreateDefault()
        {
            LightInfo li = new LightInfo();
            li.color = Color.black;
            li.SetDirection(new Vector3(0.5f,0.6f,0.6f));
            return li;
        }
    }

    public enum LightFieldType
    {
        MainLight,
        AddLight,
        WaterLight,
    }

    [Serializable]
    public sealed class LightParameter : VolumeParameter<LightInfo>
    {
        public Light light;
        public LightFieldType type;
        public bool mainLight;

        public LightParameter(LightFieldType type, LightInfo value, bool mainLight = false,
            bool overrideState = false) : base(value,
            overrideState)
        {
            this.type = type;
            this.mainLight = mainLight;
        }

        public override void Interp(LightInfo @from, LightInfo to, float t)
        {
            m_Value.color = Color.Lerp(@from.color, to.color, t);
            m_Value.SetDirection(Vector3.Slerp(@from.GetDirection(), to.GetDirection(), t));
        }

        public void Update()
        {
            m_Value = new LightInfo();
            m_Value.color = light.color;
            m_Value.SetDirection(-light.transform.forward);
        }
    }

    [Serializable, VolumeComponentMenu("Post-processing/Lighting")]
    public sealed class Lighting : VolumeComponent
    {
        public BoolParameter cameraSpaceLighting = new BoolParameter(false);
        public BoolParameter usingDefaultLight = new BoolParameter(false);
        private Camera Camera;
        public LightParameter mainLight = new LightParameter(LightFieldType.MainLight, LightInfo.CreateDefault(), true);
        public LightParameter addLight = new LightParameter(LightFieldType.AddLight, LightInfo.CreateDefault());
        public ColorParameter roleLightColor = new ColorParameter(Color.white, true, false, true);

        public ClampedFloatParameter roleHeightSHAlpha = new ClampedFloatParameter(1f, 0f, 0.5f);
        public ClampedFloatParameter roleHeightSHFadeout = new ClampedFloatParameter(1f, 0f, 1.5f);
        public ClampedFloatParameter minRoleHAngle = new ClampedFloatParameter(-20f, -45f, 0f);
        public ClampedFloatParameter maxRoleHAngle = new ClampedFloatParameter(10f, -80f, 45f);
        public ClampedFloatParameter roleFaceHAngle = new ClampedFloatParameter(0f, -90f, 90f);
        public BoolParameter addRoleLight = new BoolParameter(false);
        public ClampedFloatParameter roleLightRotOffset = new ClampedFloatParameter(0f, 0f, 360f);
        public ClampedFloatParameter leftRightControl = new ClampedFloatParameter(1f, -5f, 5f);
        public ClampedFloatParameter upDownControl = new ClampedFloatParameter(1f, -5f, 5f);
        public LightParameter waterLight = new LightParameter(LightFieldType.WaterLight, LightInfo.CreateDefault());
        public ClampedFloatParameter shadowRimIntensity = new ClampedFloatParameter(0.3f, 0f, 1f);
        public BoolParameter customFaceShadowDir = new BoolParameter(false);
        public Vector2Parameter faceShadowDir = new Vector2Parameter(Vector2.zero);

        // ui scene parameters
        public BoolParameter planarShadow = new BoolParameter(false);
        public ColorParameter planarShadowColor = new ColorParameter(new Color(0, 0, 0, 0.5f));
        public MinFloatParameter planarShadowFalloff = new MinFloatParameter(2f, 0f);
        public Vector3Parameter planarShadowDir = new Vector3Parameter(Vector3.up);

        public BoolParameter selfShadow = new BoolParameter(false, true);
        public MinFloatParameter Roleshadow_MaxDistance = new MinFloatParameter(12f, 4f, true);
        public ClampedFloatParameter Roleshadow_NormalBias = new ClampedFloatParameter(1f, 0f, 10f, true);

		//与庆宝和艾建确认过了，树叶的运动速度放在这里（2021.12.20）
		public Vector3Parameter windDirection = new Vector3Parameter(new Vector3(1,1,0));
		public ClampedFloatParameter windFrenquency = new ClampedFloatParameter(10f, -50f, 50f);
		public ClampedFloatParameter windSpeed = new ClampedFloatParameter(0.5f, -50f, 50f);
        #region Uniforms

        public static readonly int _MainLightColor0 = Shader.PropertyToID("_MainLightColor0");
        public static readonly int _MainLightDir0 = Shader.PropertyToID("_MainLightDir0");
        public static readonly int _MainLightDir1 = Shader.PropertyToID("_MainLightDir1");
        public static readonly int _DepthShadow = Shader.PropertyToID("_DepthShadow");
        public static readonly int _AddLightDir0 = Shader.PropertyToID("_AddLightDir0");
        public static readonly int _AddLightColor0 = Shader.PropertyToID("_AddLightColor0");
        public static readonly int _MainLightRoleColor = Shader.PropertyToID("_MainLightRoleColor");
        public static readonly int _GlobalLightParam = Shader.PropertyToID("_GlobalLightParam");
        public static readonly int _WaterLightDir = Shader.PropertyToID("_WaterLightDir");
        public static readonly int _GameViewWorldSpaceCameraPos = Shader.PropertyToID("_GameViewWorldSpaceCameraPos");
        public static readonly int _ShadowRimIntensity = Shader.PropertyToID("_ShadowRimIntensity");
        public static readonly int _RoleSHParam = Shader.PropertyToID("_RoleSHParam");

        public static readonly int _PlanarShadowParam = Shader.PropertyToID("_PlanarShadowParam");
        public static readonly int _PlanarShadowColor = Shader.PropertyToID("_PlanarShadowColor");
        public static readonly int _GlobalFaceShadowParam = Shader.PropertyToID("_GlobalFaceShadowParam");

        public static readonly int _AmbientWindParam = Shader.PropertyToID("_AmbientWindParam");
        public static readonly int _AmbientWindParam1 = Shader.PropertyToID("_AmbientWindParam1");
        private static readonly int _ShadowPos = Shader.PropertyToID("_ShadowPos");

        #endregion

        protected override void OnEnable()
        {
            base.OnEnable();
            Camera = GetMainCamera();

            if (planarShadow.value)
            {
                EntityExtSystem.setPerRendererFeature -= SetSampleShadowPosition;
                EntityExtSystem.setPerRendererFeature += SetSampleShadowPosition;
            }
        }


        protected override void OnDisable()
        {
            base.OnDisable();
            EntityExtSystem.setPerRendererFeature -= SetSampleShadowPosition;
        }

        private void SetSampleShadowPosition(EngineContext context, XGameObject xgo, RendererInstance ri)
        {
            if (!planarShadow.value || !ri.render || !ri.render.enabled)
                return;
            
            var mat = ri.shareMaterial;
            if (!mat)
                return;
            mat.SetVector(_ShadowPos,xgo.Position);
        }
        
        private void Awake()
        {
            Shader.SetGlobalVector(_MainLightDir0, mainLight.value.GetDirection());
        }
        public void UpdateLights()
        {
            //main light
            LightInfo mainLight = this.mainLight.value;
            Shader.SetGlobalVector(_MainLightDir0, mainLight.GetDirection());
            Shader.SetGlobalVector(_MainLightColor0, mainLight.color);

            // SDFFaceUpdateStrategy.instance.UpdateSDFFaceParam = UpdateRoleMainLightDir;

            UpdateRoleLightDir(mainLight.GetDirection());

            Vector4 addLightColor = new Vector4(
                addLight.value.color.r,
                addLight.value.color.g,
                addLight.value.color.b,
                addRoleLight.value ? 1 : 0
            );
            Vector3 addLightDir = addLight.value.GetDirection();
            Vector4 addLightDirWithIntensity = new Vector4(
                addLightDir.x,
                addLightDir.y,
                addLightDir.z,
                addLight.value.color.grayscale
            );
            Shader.SetGlobalVector(_AddLightDir0, addLightDirWithIntensity);
            Shader.SetGlobalVector(_AddLightColor0, addLightColor);
            Shader.SetGlobalVector(_MainLightRoleColor, roleLightColor.value);

            Shader.SetGlobalVector(_WaterLightDir, waterLight.value.GetDirection());

            Shader.SetGlobalFloat(_ShadowRimIntensity, shadowRimIntensity.value);

            float remapMul = roleHeightSHAlpha.value;
            float remapAdd = 1f - roleHeightSHAlpha.value;
            float heightMul = 1f / roleHeightSHFadeout.value;
            float heightAdd = 1f / roleHeightSHFadeout.value;
            Vector4 shScales = new Vector4(remapMul, remapAdd, heightMul, heightAdd);
            Shader.SetGlobalVector(_RoleSHParam, shScales);

            var context = EngineContext.instance;
            if (context != null)
            {
                //Vector3 dir = EngineContext.roleLightDir;
                //VoxelLightingSystem.UpdateRoleLightDir(context, context.roleLightMode, ref dir);
                if (context.CameraTransCache)
                {
                    Shader.SetGlobalVector(_GameViewWorldSpaceCameraPos, context.CameraTransCache.position);
                }
            }
        }

        public override void OnOverrideFinish()
        {
            base.OnOverrideFinish();
            UpdateLights();
            UpdateShadowPlane();
            UpdateFaceShadow();
            UpdateAmbientWind();
        }

        private void UpdateFaceShadow()
        {
            Vector4 faceShadowParam = faceShadowDir.value;
            faceShadowParam.z = customFaceShadowDir.value ? 1f : 0f;
            Shader.SetGlobalVector(_GlobalFaceShadowParam, faceShadowParam);
        }

        private void UpdateShadowPlane()
        {
            UniversalRenderPipeline.planarShadowEnabled = planarShadow.value;
            Shader.SetGlobalColor(_PlanarShadowColor, planarShadowColor.value);
            Vector4 packedParams = planarShadowDir.value.normalized;
            packedParams.w = planarShadowFalloff.value;
            Shader.SetGlobalVector(_PlanarShadowParam, packedParams);
        }

        private void UpdateRoleLightDir(Vector3 dir)
        {
            EngineContext context = EngineContext.instance;
            if (context != null && context.roleLightMode == RoleLightMode.WorldSpace)
            {
                UpdateRoleMainLightDir();
                UpdateRoleShadowDir(context);
            }
        }

        private RenderBinding _RenderBindingTmp;
        private FaceData _faceData;
        private FaceBinding _FaceBindingTmp;
        private Renderer _RendererTmp;
        
        public void UpdateRoleMainLightDir()
        {
            Vector3 roleLightDir = CalculateRoleLightDir();

//             // Face 模型应该只有一个，没有嵌套遍历的必要，但是LOD机制导致要遍历，不科学
//             int nLenght1 = RenderBinding.AllInstance.Count;
//             int faceDatasLen = RenderBinding.FaceDatas.Count;
//             
// #if UNITY_EDITOR
//             if (!Application.isPlaying)
//             {
//                 for (int i = 0; i < nLenght1; i++)
//                 {
//                     _RenderBindingTmp = RenderBinding.AllInstance[i];
//                 
//                     int nLenght2 = _RenderBindingTmp.faces.Count;
//                     for (int j = 0; j < nLenght2; j++)
//                     {
//                         _FaceBindingTmp = _RenderBindingTmp.faces[j];
//                 
//                         int nLenght3 = _FaceBindingTmp.renderers.Count;
//                         for (int k = 0; k < nLenght3; k++)
//                         {
//                             _RendererTmp = _FaceBindingTmp.renderers[k];
//                             SDFFaceCore.ApplyFaceParam(_FaceBindingTmp.bone, _RendererTmp, -roleLightDir);
//                         }
//                     }
//                 }
//             }
//             else
// #endif
//             {
//                 // for (int i = 0; i < nLenght1; i++)
//                 // {
//                 //     _RenderBindingTmp = RenderBinding.AllInstance[i];
//                 //     _FaceBindingTmp = _RenderBindingTmp.Face;
//                 //     _RendererTmp = _FaceBindingTmp.FaceRenderer;
//                 //     SDFFaceCore.ApplyFaceParam(_FaceBindingTmp.bone, _RendererTmp, -roleLightDir);
//                 // }
//
//                 for (int i = 0; i < faceDatasLen; i++)
//                 {
//                     _faceData = RenderBinding.FaceDatas[i];
//                     SDFFaceCore.ApplyFaceParam(_faceData.Bone, _faceData.Renderer, -roleLightDir);
//                 }
//             }

            EngineContext.roleLightDir = roleLightDir;
            Shader.SetGlobalVector(_MainLightDir1, -roleLightDir);
        }

        private static Camera m_MainCamera;
        public static void SetMainCamera(Camera cam)
        {
            m_MainCamera = cam;
        }

        public static Camera GetMainCamera()
        {
            if (!m_MainCamera)
            {
                return Camera.main;
            }
            return m_MainCamera;
        }
        
        public Vector3 CalculateRoleLightDir()
        {
            Vector3 dir;
            if (usingDefaultLight.value && !cameraSpaceLighting.value)
            {
                dir = ForwardLights.MainLightInfo._Position;
                return -CalculateLightAngle(dir);;
            }
            if (cameraSpaceLighting.value)
            {
                if (!Camera)
                {
                    Camera = GetMainCamera();
                }
                if (Camera)
                {
                    float3 mainlightDir = new Vector3(0.5f,0.6f,0.6f);
                    float4 camSpaceDir = math.mul(math.float4(mainlightDir.xyz,0),(float4x4)Camera.worldToCameraMatrix);
                    dir = (Vector4)camSpaceDir;
                }
                else
                {
                    dir = mainLight.value.GetDirection();
#if UNITY_EDITOR
                    Debug.LogError($"Camera Space Lighting is Not Validate! Cannot Find <Main Camera>");
#endif
                }
                
            }
            else
            {
                dir = mainLight.value.GetDirection();
            }
            
            dir = CalculateLightOffset(dir);
            dir = CalculateLightAngle(dir);
            
            return -dir;
        }

        private Vector3 CalculateLightOffset(Vector3 dir)
        {
            Quaternion rot = Quaternion.Euler(0, roleLightRotOffset.value, 0);
            dir = rot * dir;
            return dir;
        }
        
        Vector3 CalculateLightAngle(Vector3 dir)
        {
            float minSin = Mathf.Sin(minRoleHAngle.value * Mathf.Deg2Rad);
            float maxSin = Mathf.Sin(maxRoleHAngle.value * Mathf.Deg2Rad);
            dir.y = Mathf.Clamp(dir.y, minSin, maxSin);
            dir = Vector3.Normalize(dir);
            return dir;
        }

        private void UpdateRoleShadowDir(EngineContext context)
        {
            Vector4 roleFaceShaodowDirParam = new Vector4(
                leftRightControl.value,
                upDownControl.value,
                0,
                context.renderflag.HasFlag(EngineContext.RFlag_UISceneShadow) ? 1 : 0
            );
            Shader.SetGlobalVector(_DepthShadow, roleFaceShaodowDirParam);
        }

        public override void OnFireChange(Camera camera, Transform root, bool enable)
        {
            base.OnFireChange(camera, root, enable);
            // if (enable && selfShadow.value)
            // {
            //     UniversalRenderPipelineAsset urpAsset =
            //         GraphicsSettings.renderPipelineAsset as UniversalRenderPipelineAsset;
            //     if (urpAsset != null)
            //     {
            //         urpAsset.shadowDistance = Roleshadow_MaxDistance.value;
            //         urpAsset.shadowCascadeCount = 3;
            //         urpAsset.shadowDepthBias = 1f;
            //         urpAsset.shadowNormalBias = Roleshadow_NormalBias.value;
            //         urpAsset.mainLightShadowmapResolution = 4096;
            //     }
            // }
        }

        private void UpdateAmbientWind()
        {
            Vector4 ambientWindParam = new Vector4(windDirection.value.x, windDirection.value.y, windDirection.value.z, 1);
            Vector4 ambientWindParam1 = new Vector4(windFrenquency.value, windSpeed.value, 1, 1);

            Shader.SetGlobalVector(_AmbientWindParam, ambientWindParam);
            Shader.SetGlobalVector(_AmbientWindParam1, ambientWindParam1);
        }


        // TODO: 删除灯光时希望能清空Light颜色，但是OnDestroy的时候Reset会导致进游戏的时候情况profile的值，需要拿PlayMode判断一下。
        // protected override void OnDestroy()
        // {
        //     base.OnDestroy();
        //     UpdateLights();
        // }
    }
}