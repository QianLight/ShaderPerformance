using System;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Rendering;
#if UNITY_EDITOR
using UnityEngine.Experimental.GlobalIllumination;
using Lightmapping = UnityEngine.Experimental.GlobalIllumination.Lightmapping;
#endif

namespace CFEngine.SRP
{
    public sealed partial class SparkRenderPipeline : RenderPipeline
    {
        private RenderingData rd;
        private ScriptableCullingParameters scp;
#if UNITY_EDITOR
        ReflectFun frameDebugEnable;
#endif
        public static SparkRenderPipelineAsset asset
        {
            get
            {
                return GraphicsSettings.renderPipelineAsset as SparkRenderPipelineAsset;
            }
        }

        public SparkRenderPipeline ()
        {
#if UNITY_EDITOR
            SetSupportedRenderingFeatures ();
            var unityEditorAssembly = EditorCommon.GetUnityEditorInternalAssembly();
            if (unityEditorAssembly != null)
            {
                var frameDebuggerUtilityType = unityEditorAssembly.GetType("UnityEditorInternal.FrameDebuggerUtility");
                if(frameDebuggerUtilityType != null)
                {
                    frameDebugEnable = EditorCommon.GetInternalFunction(frameDebuggerUtilityType, "IsLocalEnabled", true, false, false, false);
                }
            }
            CustomSceneView.SetupDrawMode();
#endif

            PerFrameBuffer._Time = Shader.PropertyToID ("_Time");
            PerFrameBuffer._SinTime = Shader.PropertyToID ("_SinTime");
            PerFrameBuffer._CosTime = Shader.PropertyToID ("_CosTime");
            PerFrameBuffer._DeltaTime = Shader.PropertyToID ("_DeltaTime");
            PerFrameBuffer._TimeParameters = Shader.PropertyToID ("_TimeParameters");

            PerCameraBuffer._InvCameraViewProj = Shader.PropertyToID ("_InvCameraViewProj");
            PerCameraBuffer._ScreenParams = Shader.PropertyToID ("_ScreenParams");
            PerCameraBuffer._ScaledScreenParams = Shader.PropertyToID ("_ScaledScreenParams");
            PerCameraBuffer._WorldSpaceCameraPos = Shader.PropertyToID ("_WorldSpaceCameraPos");

            QualitySettings.antiAliasing = 1;
            Graphics.activeTier = GraphicsTier.Tier1;
            Shader.globalRenderPipeline = "SparkRenderPipeline";
        }

        protected override void Dispose (bool disposing)
        {
            //Shader.globalRenderPipeline = "";
            base.Dispose (disposing);
            SupportedRenderingFeatures.active = new SupportedRenderingFeatures ();

#if UNITY_EDITOR
            SceneViewDrawMode.ResetDrawMode ();
            Lightmapping.ResetDelegate ();
#endif            
        }

        void SetShaderTimeValues (float time, float deltaTime, float smoothDeltaTime)
        {
            // We make these parameters to mirror those described in `https://docs.unity3d.com/Manual/SL-UnityShaderVariables.html
            float timeEights = time / 8f;
            float timeFourth = time / 4f;
            float timeHalf = time / 2f;

            // Time values
            Vector4 timeVector = time * new Vector4 (1f / 20f, 1f, 2f, 3f);
            Vector4 sinTimeVector = new Vector4 (Mathf.Sin (timeEights), Mathf.Sin (timeFourth), Mathf.Sin (timeHalf), Mathf.Sin (time));
            Vector4 cosTimeVector = new Vector4 (Mathf.Cos (timeEights), Mathf.Cos (timeFourth), Mathf.Cos (timeHalf), Mathf.Cos (time));
            Vector4 deltaTimeVector = new Vector4 (deltaTime, 1f / deltaTime, smoothDeltaTime, 1f / smoothDeltaTime);
            Vector4 timeParametersVector = new Vector4 (time, Mathf.Sin (time), Mathf.Cos (time), 0.0f);

            Shader.SetGlobalVector (PerFrameBuffer._Time, timeVector);
            Shader.SetGlobalVector (PerFrameBuffer._SinTime, sinTimeVector);
            Shader.SetGlobalVector (PerFrameBuffer._CosTime, cosTimeVector);
            Shader.SetGlobalVector (PerFrameBuffer._DeltaTime, deltaTimeVector);
            Shader.SetGlobalVector (PerFrameBuffer._TimeParameters, timeParametersVector);
        }

        protected override void Render (ScriptableRenderContext renderContext, Camera[] cameras)
        {
            if (cameras == null)
                return;
            rd.rc = RenderContext.singleton;
            GraphicsSettings.lightsUseLinearIntensity = true;
#if UNITY_EDITOR
            GraphicsSettings.useScriptableRenderPipelineBatching = false;
#else
            GraphicsSettings.useScriptableRenderPipelineBatching = asset.enableSRPBatch;
#endif

            EngineContext context = EngineContext.instance;
            rd.context = context;
            Camera mainCamera = null;
            Camera uiCamera = null;
            Camera offscreenCamera = null;
#if !UNITY_EDITOR
            bool renderOffscreenCamera = false;
#endif
            if (context != null)
            {
                mainCamera = context.CameraRef;
                uiCamera = context.uiCamera;
                offscreenCamera = context.Camera_OffScreen;
#if UNITY_EDITOR
                //if application is paused,script will not update and Graphics.DrawMesh is not called
                //need to manual upate,but if frame debug enabled script will update and not manual update
                if (EngineContext.IsRunning && UnityEditor.EditorApplication.isPaused)
                {
                    if (frameDebugEnable == null || !(bool)frameDebugEnable.Call(null,null))
                    {
                        RenderLayer.OnUpdate();
                    }
                }
#else
                if (offscreenCamera == cameras[0])
                    renderOffscreenCamera = true;
#endif
                var rc = rd.rc;
                if (rc.IsValid())
                {
                    RuntimeUtilities.CommitCmd(ref renderContext, rc.preWorkingCmd);
                }
            }
            BeginFrameRendering (renderContext, cameras);
            if (context != null)
            {
                SetupPerFrameShaderConstants (context);
            }
#if UNITY_EDITOR
            RenderCamerasEditor (ref renderContext, mainCamera, uiCamera, cameras);
#else
            if (renderOffscreenCamera)
            {
                RenderOffscreenCamera (ref renderContext, offscreenCamera);
            }
            else
            {
                RenderCameras (ref renderContext, mainCamera, uiCamera, cameras);
            }
#endif
            EndFrameRendering (renderContext, cameras);
        }

        private void RenderCameras (ref ScriptableRenderContext renderContext,
            Camera mainCamera, Camera uiCamera, Camera[] cameras)
        {
            if (uiCamera == null)
            {
                for (int i = 0; i < cameras.Length; ++i)
                {
                    var c = cameras[i];
                    if (c.gameObject.layer == DefaultGameObjectLayer.UILayer)
                    {
                        uiCamera = c;
                        break;
                    }
                }
            }

            var settings = asset;
            if (mainCamera != null)
            {
                BeginCameraRendering (renderContext, mainCamera);
                RenderCamera (ref renderContext, mainCamera, settings.MainRenderer);
                EndCameraRendering (renderContext, mainCamera);
            }
            if (uiCamera != null)
            {
                BeginCameraRendering (renderContext, uiCamera);
                RenderCamera (ref renderContext, uiCamera, settings.UIRenderer);
                EndCameraRendering (renderContext, uiCamera);
            }
        }

        private void RenderOffscreenCamera (ref ScriptableRenderContext renderContext,
            Camera c)
        {
            BeginCameraRendering (renderContext, c);
            RenderCamera (ref renderContext, c, OffscreenRenderer.offscreenRenderer);
            EndCameraRendering (renderContext, c);
        }
#if UNITY_EDITOR
        class CameraDataComparer : IComparer<Camera>
        {
            public int Compare (Camera lhs, Camera rhs)
            {
                return (int) lhs.depth - (int) rhs.depth;
            }
        }
        CameraDataComparer cdc = new CameraDataComparer ();
        void SortCameras (Camera[] cameras)
        {
            if (cameras.Length <= 1)
                return;
            Array.Sort (cameras, cdc);
        }

        private void RenderCamerasEditor (ref ScriptableRenderContext renderContext,
            Camera mainCamera, Camera uiCamera, Camera[] cameras)
        {
            SortCameras (cameras);
            for (int i = 0; i < cameras.Length; ++i)
            {
                var c = cameras[i];

                bool isOverLay = c.gameObject.layer == DefaultGameObjectLayer.UILayer;
                RenderCamera (ref renderContext, c, GetRenderer (c, isOverLay));
            }
        }

        private SparkRenderer GetRenderer (Camera c, bool isOverLay)
        {
            if (c.cameraType == CameraType.Game)
            {
                if (isOverLay)
                {
                    return asset.UIRenderer;
                }
                var name = c.name;
                if (name.StartsWith ("Preview"))
                {
                    return OffscreenRenderer.offscreenRenderer;
                }
                else if (name.StartsWith ("Main Camera"))
                {
#if UNITY_EDITOR
                    if (EngineContext.IsRunning)
#endif
                    {
                        if (rd.context != null && !rd.context.renderflag.HasFlag (EngineContext.RFlag_RenderEnable))
                        {
                            return null;
                        }
                    }

                    return asset.MainRenderer;
                }
                else if (name.StartsWith ("OffSceenCamera Camera"))
                {
                    return OffscreenRenderer.offscreenRenderer;
                }

            }
            else if (c.cameraType == CameraType.SceneView)
            {
                return SceneViewRenderer.sceneViewRenderer;
            }
            else if (c.cameraType == CameraType.Preview)
            {
                return OffscreenRenderer.offscreenRenderer;
            }
            return null;

        }
#endif
        private void RenderCamera (ref ScriptableRenderContext context, Camera camera, SparkRenderer renderer)
        {
            if (renderer != null)
            {
                if (renderer.InitCamera (ref rd, camera))
                {
                    var engineContext = rd.context;
                    // if (engineContext != null && camera == engineContext.CameraRef && engineContext.HasFlag (EngineContext.CameraStack))
                    // {
                    //     RenderCameraStack (ref context, ref rd.cameraData, renderer);
                    // }
                    // else
                    {
                        RenderSingleCamera (ref context, ref rd.cameraData, renderer);
                    }
                }
            }
        }

        private void RenderCameraStack (
            ref ScriptableRenderContext context,
            ref CameraData cameraData,
            SparkRenderer renderer, string name, int index)
        {
            Camera camera = cameraData.camera;
            EngineContext engineContext = EngineContext.instance;
            ref var dc = ref engineContext.dummyCameras[index];
#if UNITY_EDITOR
            engineContext.CameraTransCache.position = dc.pos;
            engineContext.CameraTransCache.rotation = dc.rot;
#endif
            BeginCameraRendering (context, camera);
            if (!camera.TryGetCullingParameters (out scp))
                return;

            SetupPerCameraShaderConstants (ref cameraData);
#if UNITY_EDITOR
            ProfilingSampler sampler = new ProfilingSampler (name);
            CommandBuffer cmd = CommandBufferPool.Get (sampler.name);
            using (new ProfilingScope (cmd, sampler))
#endif

            {
                renderer.Setup (ref context, ref scp, ref rd);
                rd.cullResults = context.Cull (ref scp);

                if (index == 0)
                {
                    renderer.Execute (ref context, ref rd);
                }
                else
                {
                    renderer.ExecuteStack (ref context, ref rd);
                }

            }
#if UNITY_EDITOR
            context.ExecuteCommandBuffer (cmd);
            CommandBufferPool.Release (cmd);
#endif

            context.Submit ();
            EndCameraRendering (context, camera);
        }
        private void RenderCameraStack (ref ScriptableRenderContext context, ref CameraData cameraData, SparkRenderer renderer)
        {

            renderer.ExecuteBeforeRendering (ref context, ref rd);
            RenderCameraStack (ref context, ref cameraData, renderer, "Camera0", 0);
            RenderCameraStack (ref context, ref cameraData, renderer, "Camera1", 1);
            RenderCameraStack (ref context, ref cameraData, renderer, "Camera2", 2);
            Camera camera = cameraData.camera;
            BeginCameraRendering (context, camera);
            renderer.ExecuteAfterRendering (ref context, ref rd);
            context.Submit ();
            EndCameraRendering (context, camera);

        }

        private void RenderSingleCamera (ref ScriptableRenderContext context, ref CameraData cameraData, SparkRenderer renderer)
        {
            Camera camera = cameraData.camera;

            BeginCameraRendering (context, camera);
            SetupPerCameraShaderConstants (ref cameraData);
            if (!camera.TryGetCullingParameters (out scp))
                return;
#if UNITY_EDITOR
            ProfilingSampler sampler = new ProfilingSampler (camera.name);
            CommandBuffer cmd = CommandBufferPool.Get (sampler.name);
            using (new ProfilingScope (cmd, sampler))
#endif
            {
                renderer.Setup (ref context, ref scp, ref rd);
                rd.cullResults = context.Cull (ref scp);
                renderer.ExecuteBeforeRendering(ref context, ref rd);
                renderer.Execute (ref context, ref rd);
            }
#if UNITY_EDITOR
            context.ExecuteCommandBuffer (cmd);
            CommandBufferPool.Release (cmd);
#endif
            renderer.ExecuteAfterRendering (ref context, ref rd);
            context.Submit ();
            EndCameraRendering (context, camera);
        }

#if UNITY_EDITOR
        static void SetSupportedRenderingFeatures ()
        {

            SupportedRenderingFeatures.active = new SupportedRenderingFeatures ()
            {
                reflectionProbeModes = SupportedRenderingFeatures.ReflectionProbeModes.None,
                defaultMixedLightingModes = SupportedRenderingFeatures.LightmapMixedBakeModes.IndirectOnly,
                mixedLightingModes = SupportedRenderingFeatures.LightmapMixedBakeModes.Subtractive |
                SupportedRenderingFeatures.LightmapMixedBakeModes.IndirectOnly,
                lightmapBakeTypes = LightmapBakeType.Baked | LightmapBakeType.Mixed,
                lightmapsModes = LightmapsMode.CombinedDirectional | LightmapsMode.NonDirectional,
                lightProbeProxyVolumes = false,
                motionVectors = false,
                receiveShadows = false,
                reflectionProbes = true
            };
            SceneViewDrawMode.SetupDrawMode ();

            Lightmapping.SetDelegate (lightsDelegate);
        }

        static Lightmapping.RequestLightsDelegate lightsDelegate = (Light[] requests, NativeArray<LightDataGI> lightsOutput) =>
        {
            LightDataGI lightData = new LightDataGI ();

            for (int i = 0; i < requests.Length; i++)
            {
                Light light = requests[i];
                switch (light.type)
                {
                    case UnityEngine.LightType.Directional:
                        DirectionalLight directionalLight = new DirectionalLight ();
                        LightmapperUtils.Extract (light, ref directionalLight);
                        lightData.Init (ref directionalLight);
                        break;
                    case UnityEngine.LightType.Point:
                        PointLight pointLight = new PointLight ();
                        LightmapperUtils.Extract (light, ref pointLight);
                        lightData.Init (ref pointLight);
                        break;
                    case UnityEngine.LightType.Spot:
                        SpotLight spotLight = new SpotLight ();
                        LightmapperUtils.Extract (light, ref spotLight);
                        spotLight.innerConeAngle = light.innerSpotAngle * Mathf.Deg2Rad;
                        //spotLight.angularFalloff = AngularFalloffType.AnalyticAndInnerAngle;
                        lightData.Init (ref spotLight);
                        break;
                    case UnityEngine.LightType.Area:
                        RectangleLight rectangleLight = new RectangleLight ();
                        LightmapperUtils.Extract (light, ref rectangleLight);
                        rectangleLight.mode = LightMode.Baked;
                        lightData.Init (ref rectangleLight);
                        break;
                    case UnityEngine.LightType.Disc:
                        DiscLight discLight = new DiscLight ();
                        LightmapperUtils.Extract (light, ref discLight);
                        discLight.mode = LightMode.Baked;
                        lightData.Init (ref discLight);
                        break;
                    default:
                        lightData.InitNoBake (light.GetInstanceID ());
                        break;
                }

                lightData.falloff = FalloffType.InverseSquared;
                lightsOutput[i] = lightData;
            }
            // LightDataGI lightData = new LightDataGI();

            // for (int i = 0; i < requests.Length; i++)
            // {
            //     Light light = requests[i];
            //     lightData.InitNoBake(light.GetInstanceID());
            //     lightsOutput[i] = lightData;
            // }
        };
#endif
        void SetupPerFrameShaderConstants (EngineContext context)
        {
#if UNITY_EDITOR
            float time = Application.isPlaying ? context.time : Time.realtimeSinceStartup;
#else
            float time = context.time;
#endif
            float deltaTime = context.deltaTime;
            float smoothDeltaTime = context.smoothDeltaTime;
            SetShaderTimeValues (time, deltaTime, smoothDeltaTime);
        }

        static void SetupPerCameraShaderConstants (ref CameraData cameraData)
        {
            Camera camera = cameraData.camera;

            Rect pixelRect = cameraData.pixelRect;
            float scaledCameraWidth = (float) pixelRect.width * cameraData.renderScale;
            float scaledCameraHeight = (float) pixelRect.height * cameraData.renderScale;
            Shader.SetGlobalVector (PerCameraBuffer._ScaledScreenParams, new Vector4 (scaledCameraWidth, scaledCameraHeight, 1.0f + 1.0f / scaledCameraWidth, 1.0f + 1.0f / scaledCameraHeight));
            Shader.SetGlobalVector (PerCameraBuffer._WorldSpaceCameraPos, camera.transform.position);
            float cameraWidth = (float) pixelRect.width;
            float cameraHeight = (float) pixelRect.height;
            Shader.SetGlobalVector (PerCameraBuffer._ScreenParams, new Vector4 (cameraWidth, cameraHeight, 1.0f + 1.0f / cameraWidth, 1.0f + 1.0f / cameraHeight));

            Matrix4x4 projMatrix = GL.GetGPUProjectionMatrix (camera.projectionMatrix, false);
            Matrix4x4 viewMatrix = camera.worldToCameraMatrix;
            Matrix4x4 viewProjMatrix = projMatrix * viewMatrix;
            Matrix4x4 invViewProjMatrix = Matrix4x4.Inverse (viewProjMatrix);
            Shader.SetGlobalMatrix (PerCameraBuffer._InvCameraViewProj, invViewProjMatrix);
        }
    }
}