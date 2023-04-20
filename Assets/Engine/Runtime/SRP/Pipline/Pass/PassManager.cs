using CFEngine;
using UnityEngine;
using UnityEngine.Rendering;

namespace CFEngine.SRP
{
    internal sealed class PassManager : CFSingleton<PassManager>
    {
        public DrawObjectsPass opaquePass;
        public PreZPass preZPass;
        public OutlinePass outlinePass;
        public DrawSkyboxPass skyboxPass;

        public CustomTransparentPass customTransparentPass;
        public DrawObjectsPass transparentPass;
        public DistortionPass distortionPass;

        public StencilState defaultStencilState;

        public void Init (SparkRenderPipelineAsset data)
        {
            StencilStateData stencilData = data.defaultStencilState;
            defaultStencilState = StencilState.defaultValue;
            defaultStencilState.enabled = stencilData.overrideStencilState;
            defaultStencilState.SetCompareFunction (stencilData.stencilCompareFunction);
            defaultStencilState.SetPassOperation (stencilData.passOperation);
            defaultStencilState.SetFailOperation (stencilData.failOperation);
            defaultStencilState.SetZFailOperation (stencilData.zFailOperation);
            if (opaquePass == null)
            {
                opaquePass = new DrawObjectsPass (
                    "Render Opaques", true,
                    RenderQueueRange.opaque);
            }
            if (preZPass == null)
            {
                preZPass = new PreZPass(
                    "Render PreZ",
                    RenderQueueRange.opaque);
            }

            if (outlinePass == null)
            {
                outlinePass = new OutlinePass (
                    "Render Outline",
                    RenderQueueRange.opaque);
            }
            
            if (skyboxPass == null)
            {
                skyboxPass = new DrawSkyboxPass ("Render Skybox",RenderQueueRange.opaque);
            }
            if (customTransparentPass == null)
            {
                customTransparentPass = new CustomTransparentPass (
                    "Render Custom Transparent",
                    RenderQueueRange.opaque);
            }
            if (transparentPass == null)
            {
                transparentPass = new DrawObjectsPass (
                    "Render Transparent", false,
                    RenderQueueRange.transparent);
            }
            if (distortionPass == null)
            {
                distortionPass = new DistortionPass("Render Distortion", RenderQueueRange.transparent);
            }
        }
    }
}