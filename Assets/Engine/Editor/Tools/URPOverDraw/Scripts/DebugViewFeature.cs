//
// URP Debug Views for Unity
// (c) 2019 PH Graphics
//

using UnityEngine.Rendering.Universal;

namespace URPDebugViews
{
    public class DebugViewFeature : ScriptableRendererFeature
    {
        private DebugViewPass _debugViewPass;

        public override void Create()
        {
            _debugViewPass = new DebugViewPass();
        }

        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
        {
            renderer.EnqueuePass(_debugViewPass);
        }
    }
}