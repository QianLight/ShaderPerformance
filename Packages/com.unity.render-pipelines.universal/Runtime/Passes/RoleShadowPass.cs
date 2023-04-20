using System;

namespace UnityEngine.Rendering.Universal.Internal
{
    /// <summary>
    /// Renders a shadow map for the main Light.
    /// </summary>
    public class RoleShadowCasterPass : ScriptableRenderPass
    {
        //URP Data
        float m_Scale = 3.0f;
        int m_RoleShadowResolution = 1024;
        float m_RoleDepthBias = 0.0f;
        float m_RoleNormalBias = 0.0f;
        LayerMask m_RoleShadowLayerMask = -1;
        bool m_RoleShadowEnable = true;
        float m_RoleShadowSize = 15f;
        float m_RoleShadowFar = 100f;
        float m_RoleShadowNear = 0.3f;

        RenderTargetHandle m_RoleShadowmap;
        RenderTexture m_RoleShadowmapTexture;
        Matrix4x4 m_RoleShadowMatrices;

        int m_ShadowBufferBits = 32;
        Vector4 m_Distances;
        ShadowSliceData shadowSliceData;
        Matrix4x4 viewMatrix;
        Matrix4x4 projMatrix;

        ProfilingSampler m_ProfilingSetupSampler = new ProfilingSampler("Setup Role Shadowmap");

        int _RoleShadowOffset0_ID;
        int _RoleShadowOffset1_ID;
        int _RoleShadowOffset2_ID;
        int _RoleShadowOffset3_ID;
        int _RolwShadowMapSize_ID;
        int _ROLE_ENALBE_ID;

        Matrix4x4 m_projMatrix;


        public RoleShadowCasterPass(RenderPassEvent evt)
        {
            base.profilingSampler = new ProfilingSampler(nameof(RoleShadowCasterPass));
            renderPassEvent = evt;

            _RoleShadowOffset0_ID = Shader.PropertyToID("_RoleShadowOffset0");
            _RoleShadowOffset1_ID = Shader.PropertyToID("_RoleShadowOffset1");
            _RoleShadowOffset2_ID = Shader.PropertyToID("_RoleShadowOffset2");
            _RoleShadowOffset3_ID = Shader.PropertyToID("_RoleShadowOffset3");
            _RolwShadowMapSize_ID = Shader.PropertyToID("_RolwShadowMapSize");
            _ROLE_ENALBE_ID = Shader.PropertyToID("_ROLE_ENALBE");
            
            m_RoleShadowmap.Init("_RoleShadowmapTexture");

        }

        public void InitProjMatrix()
        {
            m_projMatrix = Matrix4x4.zero;
            m_projMatrix[0, 0] = 2 / m_RoleShadowSize;
            m_projMatrix[1, 1] = 2 / m_RoleShadowSize;
            m_projMatrix[2, 2] = -1 / (m_RoleShadowFar - m_RoleShadowNear);
            m_projMatrix[2, 3] = -m_RoleShadowNear / (m_RoleShadowFar - m_RoleShadowNear);
            m_projMatrix[3, 3] = 1;
        }

        public bool SetupResult(bool success)
        {
            if (success)
            {
                Shader.SetGlobalInt(_ROLE_ENALBE_ID, 1);
            }
            else
            {
                Shader.SetGlobalInt(_ROLE_ENALBE_ID, 0);
            }
            return success;
        }

        public bool Setup(ref RenderingData renderingData)
        {
            //using var profScope = new ProfilingScope(null, m_ProfilingSetupSampler);

            if (!renderingData.shadowData.supportsMainLightShadows)
                return SetupResult(false);


            int shadowLightIndex = renderingData.lightData.mainLightIndex;
            if (shadowLightIndex == -1)
                return SetupResult(false);


            m_Scale = renderingData.cameraData.roleShadowScale;
            m_RoleShadowResolution = (int)renderingData.cameraData.roleShadowResolution;
            m_RoleDepthBias = renderingData.cameraData.roleDepthBias;
            m_RoleNormalBias = renderingData.cameraData.roleNormalBias;
            m_RoleShadowLayerMask = renderingData.cameraData.roleShadowLayerMask;
            m_RoleShadowEnable = renderingData.cameraData.roleShadowEnable;
            m_RoleShadowSize = renderingData.cameraData.roleShadowSize;
            m_RoleShadowFar = renderingData.cameraData.roleShadowFar;
            m_RoleShadowNear = renderingData.cameraData.roleShadowNear;

            if (!m_RoleShadowEnable)
                return SetupResult(false);

            InitProjMatrix();

            VisibleLight shadowLight = renderingData.lightData.visibleLights[shadowLightIndex];
            Light light = shadowLight.light;

            if (light.shadows == LightShadows.None)
            {
                return SetupResult(false);
            }

            Bounds bounds;
            if (!renderingData.cullResults.GetShadowCasterBounds(shadowLightIndex, out bounds))
                return SetupResult(false);

            int shadowResolution = ShadowUtils.GetMaxTileResolutionInAtlas(m_RoleShadowResolution, m_RoleShadowResolution, 1);
            
            bool success = ShadowUtils.ExtractDirectionalLightMatrix(ref renderingData.cullResults, ref renderingData.shadowData,
    shadowLightIndex, 0, m_RoleShadowResolution, m_RoleShadowResolution, shadowResolution, light.shadowNearPlane,
    out m_Distances, out shadowSliceData, out viewMatrix, out projMatrix);

            if (!success)
                return SetupResult(false);

            return SetupResult(true);
        }
        private ShaderTagId m_ShaderTagId = new ShaderTagId("ShadowCaster");

        Matrix4x4 GetShadowTransform(Matrix4x4 proj, Matrix4x4 view)
        {
            // Currently CullResults ComputeDirectionalShadowMatricesAndCullingPrimitives doesn't
            // apply z reversal to projection matrix. We need to do it manually here.
            if (SystemInfo.usesReversedZBuffer)
            {
                proj.m20 = -proj.m20;
                proj.m21 = -proj.m21;
                proj.m22 = -proj.m22;
                proj.m23 = -proj.m23;
            }

            Matrix4x4 worldToShadow = proj * view;

            var textureScaleAndBias = Matrix4x4.identity;
            textureScaleAndBias.m00 = 0.5f;
            textureScaleAndBias.m11 = 0.5f;
            textureScaleAndBias.m22 = 0.5f;
            textureScaleAndBias.m03 = 0.5f;
            textureScaleAndBias.m23 = 0.5f;
            textureScaleAndBias.m13 = 0.5f;

            // Apply texture scale and offset to save a MAD in shader.
            return textureScaleAndBias * worldToShadow;
        }
        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            
            int shadowLightIndex = renderingData.lightData.mainLightIndex;
            if (shadowLightIndex == -1)
                return;

            var settings = new ShadowDrawingSettings(renderingData.cullResults, shadowLightIndex);
            
            CommandBuffer cmd = CommandBufferPool.Get();
            using (new ProfilingScope(cmd, ProfilingSampler.Get(URPProfileId.RoleShadow)))
            {
                context.ExecuteCommandBuffer(cmd);
                cmd.Clear();
                cmd.SetGlobalTexture(m_RoleShadowmap.id, m_RoleShadowmapTexture);

                cmd.SetViewport(new Rect(shadowSliceData.offsetX, shadowSliceData.offsetY, shadowSliceData.resolution, shadowSliceData.resolution));
                projMatrix = m_projMatrix;
                projMatrix[0, 0] *= m_Scale;
                projMatrix[1, 1] *= m_Scale;
                var ltw = renderingData.lightData.visibleLights[shadowLightIndex].light.transform.localToWorldMatrix;

                var view = ltw.inverse;
                view.m20 = -view.m20;
                view.m21 = -view.m21;
                view.m22 = -view.m22;
                view.m23 = -view.m23;

                cmd.SetViewProjectionMatrices(view, projMatrix);
                cmd.SetGlobalMatrix("_ROLE_SHADOW_MAT", GetShadowTransform(projMatrix, view));
                SetParameterCmd(cmd);
                cmd.SetGlobalVector("_ShadowBias", GetShadowBias(ref renderingData.shadowData, projMatrix, m_RoleShadowResolution));

                context.ExecuteCommandBuffer(cmd);
                cmd.Clear();

                var drawSettings = CreateDrawingSettings(m_ShaderTagId, ref renderingData, SortingCriteria.CommonOpaque);

                FilteringSettings m_FilteringSettings = new FilteringSettings(RenderQueueRange.opaque, m_RoleShadowLayerMask);
                m_FilteringSettings.renderingLayerMask = renderingData.renderLayerMask;
                context.DrawRenderers(renderingData.cullResults, ref drawSettings,ref m_FilteringSettings);

                cmd.DisableScissorRect();
                context.ExecuteCommandBuffer(cmd);
                cmd.Clear();

            }
            context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);
        }
        Vector4 GetShadowBias(ref ShadowData shadowData, Matrix4x4 lightProjectionMatrix, float shadowResolution)
        {
            float frustumSize;
            frustumSize = 2.0f / lightProjectionMatrix.m00;

            float texelSize = frustumSize / shadowResolution;
            float depthBias = -m_RoleDepthBias * texelSize;
            float normalBias = -m_RoleNormalBias * texelSize;

            if (shadowData.supportsSoftShadows)
            {
                const float kernelRadius = 2.5f;
                depthBias *= kernelRadius;
                normalBias *= kernelRadius;
            }

            return new Vector4(depthBias, normalBias, 0.0f, 0.0f);
        }
        public void SetParameterCmd(CommandBuffer cmd)
        {
            float invHalfShadowAtlasWidth = (1.0f / m_RoleShadowResolution) * 0.5f;
            float invHalfShadowAtlasHeight = (1.0f / m_RoleShadowResolution) * 0.5f;

            cmd.SetGlobalVector(_RoleShadowOffset0_ID, new Vector4(-invHalfShadowAtlasWidth, -invHalfShadowAtlasHeight, 0.0f, 0.0f));
            cmd.SetGlobalVector(_RoleShadowOffset1_ID, new Vector4(invHalfShadowAtlasWidth, -invHalfShadowAtlasHeight, 0.0f, 0.0f));
            cmd.SetGlobalVector(_RoleShadowOffset2_ID, new Vector4(-invHalfShadowAtlasWidth, invHalfShadowAtlasHeight, 0.0f, 0.0f));
            cmd.SetGlobalVector(_RoleShadowOffset3_ID, new Vector4(invHalfShadowAtlasWidth, invHalfShadowAtlasHeight, 0.0f, 0.0f));

            cmd.SetGlobalVector(_RolwShadowMapSize_ID, new Vector4(invHalfShadowAtlasWidth, invHalfShadowAtlasHeight, m_RoleShadowResolution, m_RoleShadowResolution));

        }

        public override void OnCameraCleanup(CommandBuffer cmd)
        {
            if (cmd == null)
                throw new ArgumentNullException("cmd");

            if (m_RoleShadowmapTexture)
            {
                RenderTexture.ReleaseTemporary(m_RoleShadowmapTexture);
                m_RoleShadowmapTexture = null;
            }
        }


        public override void Configure(CommandBuffer cmd, RenderTextureDescriptor cameraTextureDescriptor)
        {
            m_RoleShadowmapTexture = ShadowUtils.GetTemporaryShadowTexture(m_RoleShadowResolution, m_RoleShadowResolution, m_ShadowBufferBits);
            m_RoleShadowmapTexture.name = "Role Shadow";
            ConfigureTarget(new RenderTargetIdentifier(m_RoleShadowmapTexture));
            ConfigureClear(ClearFlag.All, Color.black);
        }
    }
}