using System;
using UnityEngine;

namespace CFEngine
{
    [RequireComponent(typeof(Camera))]
#if UNITY_EDITOR
    [DisallowMultipleComponent, ExecuteInEditMode]
    [AddComponentMenu("Rendering/Render Layer", 1000)]
#endif
    public sealed class RenderLayer : MonoBehaviour
    {
        [NonSerialized] public bool enablePostProcessing = true;
        public Action prePostProcessing;
        public Action postPostProcessing;
        public Color captureColor = new Color(0.1058824f, 0.1647059f, 0.2941177f, 1f);

#if UNITY_EDITOR
        private static RenderLayer instance;

        public static RenderLayer Instance =>
            instance ? instance : instance = EngineContext.instance.CameraRef.GetComponent<RenderLayer>();

        [System.NonSerialized] public static EnvProfile envProfile;

        [System.NonSerialized] public static RenderTexture caputerRT;

        void OnEnable()
        {
        }

        private void OnDisable()
        {
        }
#endif

        public static void OnUpdate()
        {
            //EngineContext context = EngineContext.instance;
            // var mgr = EngineContext.renderManager;
            // if (context != null && mgr != null)
            // {
            //    mgr.Update(context);

            //    mgr.Render(context);

            //    EngineProfiler.EndFrame(context);
            //}
        }

        private void LateUpdate()
        {
            // prePostProcessing?.Invoke();

            // if (enablePostProcessing)
            // {
            //     OnUpdate();
            // }

            //postPostProcessing?.Invoke();
        }
    }
}