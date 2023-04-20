#define IMPOSTORS_UNITY_PIPELINE_URP
//#if IMPOSTORS_UNITY_PIPELINE_URP
using System;
using System.Linq;
using System.Reflection;
using Impostors.Managers;
using Impostors.RenderPipelineProxy;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Impostors.URP
{
    public class UniversalRenderPipelineProxy : RenderPipelineProxyBase
    {
        [SerializeField]
        private Camera _cameraWhereToScheduleCommandBufferExecution = default;

        public Camera mainCamera
        {
            set
            {
                _cameraWhereToScheduleCommandBufferExecution = value;
            }


            get
            {
                return _cameraWhereToScheduleCommandBufferExecution;
            }
        }

        [SerializeField]
        private UpdateImpostorsTexturesFeature _updateImpostorsTextureFeature = default;

        public static UniversalRenderPipelineProxy _instance;

        protected override void OnEnable()
        {
            _instance = this;

            if (Camera.main == null)
            {
                this.enabled = false;
                return;
            }


            base.OnEnable();
            if (_cameraWhereToScheduleCommandBufferExecution == null)
            {
                _cameraWhereToScheduleCommandBufferExecution = Camera.main;
                //Debug.LogError($"[{nameof(UniversalRenderPipelineProxy)}] Camera is NULL! Please, assign camera in the inspector.", this);
            }

            _updateImpostorsTextureFeature=null;
            OnValidate();
        }

        protected override void SubscribeToOnPreCull()
        {
            UnityEngine.Rendering.RenderPipelineManager.beginCameraRendering += RenderPipelineManagerOnBeginCameraRendering;
        }

        protected override void UnsubscribeFromOnPreCull()
        {
            UnityEngine.Rendering.RenderPipelineManager.beginCameraRendering -= RenderPipelineManagerOnBeginCameraRendering;
        }

        private void RenderPipelineManagerOnBeginCameraRendering(ScriptableRenderContext context, Camera camera)
        {

            // Shader.SetGlobalFloat(ShaderProperties._FogToggle, 0);
            // Shader.SetGlobalFloat(ShaderProperties._ImpostorAlpha, 1);
            OnPreCullCalled(camera);
            //  Shader.SetGlobalFloat(ShaderProperties._FogToggle, 1);
            // Shader.SetGlobalFloat(ShaderProperties._ImpostorAlpha, 0);
        }

        public override void ScheduleImpostorTextureRendering(CommandBuffer commandBuffer)
        {
            _updateImpostorsTextureFeature.Clear(_cameraWhereToScheduleCommandBufferExecution);
            _updateImpostorsTextureFeature.AddCommandBuffer(_cameraWhereToScheduleCommandBufferExecution, commandBuffer);
        }

        public override void DrawMesh(Mesh mesh, Vector3 position, Quaternion rotation, Material material, int layer,
            Camera camera,
            int submeshIndex, MaterialPropertyBlock materialPropertyBlock, bool castShadows, bool receiveShadows,
            bool useLightProbes)
        {
            Graphics.DrawMesh(mesh, position, rotation, material, layer, camera, submeshIndex, materialPropertyBlock,
                castShadows, receiveShadows, useLightProbes);
        }

//#if UNITY_EDITOR
        private void OnValidate()
        {
            if (_updateImpostorsTextureFeature == null)
            {
                try
                {
                    var urpAsset = GraphicsSettings.renderPipelineAsset as UniversalRenderPipelineAsset;
                    var scriptableRendererData = (ScriptableRendererData) urpAsset.GetType().GetProperty(
                            "scriptableRendererData",
                            BindingFlags.NonPublic | BindingFlags.Instance)
                        .GetValue(urpAsset);

                    _updateImpostorsTextureFeature =
                        (UpdateImpostorsTexturesFeature) scriptableRendererData.rendererFeatures.FirstOrDefault(x =>
                            x is UpdateImpostorsTexturesFeature);

                    if (_updateImpostorsTextureFeature == null)
                    {
                        Debug.LogError(
                            $"Impostors won't work without additional render feature in your RendererData!\n" +
                            $"Please, add '{nameof(UpdateImpostorsTexturesFeature)}' to your RendererData:\n" +
                            $"1. Click on this message to highlight RendererData asset in Project's view,\n" +
                            $"2. Select RendererData asset,\n" +
                            $"3. At the bottom of the Inspector click the 'Add Renderer Feature' button,\n" +
                            $"4. From popup select {nameof(UpdateImpostorsTexturesFeature)},\n" +
                            $"5. Return back to {nameof(ImpostorableObjectsManager)} and assign newly created feature.",
                            scriptableRendererData);
                    }
                }
                catch
                {
                    Debug.LogError($"Looks like Impostors cannot access required RendererFeature. " +
                                   $"Please, assign '{nameof(UpdateImpostorsTexturesFeature)}' as shown in the documentation in URP section.", this);
                }
            }
        }
//#endif
    }
}
//#endif