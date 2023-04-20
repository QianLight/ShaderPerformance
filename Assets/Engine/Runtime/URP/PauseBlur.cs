using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using static UnityEngine.Rendering.RenderPipelineManager;
using Object = UnityEngine.Object;

namespace CFEngine
{
    public class PauseBlur : BlurCore
    {
        private BlurContext _blurContext;

        public PauseBlur(BlurContext context)
        {
            _blurContext = context;
        }
        public override void Excecute()
        {
            beginCameraRendering -= CrabColor;
            beginCameraRendering += CrabColor;
            
            beginCameraRendering -= StartBlur;
            beginCameraRendering += StartBlur;
        }

        private void CrabColor(ScriptableRenderContext renderContext, Camera camera)
        {
            var cam = Camera.main;
            if (!cam || cam != camera)
            {
                return;
            }
            EnqueuePassCrabColor(cam,MainCameraDisable,_blurContext);
            beginCameraRendering -= CrabColor;
        }
        
        private void StartBlur(ScriptableRenderContext renderContext, Camera camera)
        {
            // var mainCam = Camera.main;
            if (camera.gameObject.layer != LayerMaskName.UI)
            {
                return;
            }

            Blur(renderContext,camera,_blurContext);
            
            if (_blurContext.process == _blurContext.iteration)
            {
                beginCameraRendering -= StartBlur;
            }
        }


        public override void OnClear()
        {
            MainCameraEnable();
        }
        
        void MainCameraEnable()
        {
            if(!_blurContext.settingsIsDirty)
                return;
#if UNITY_EDITOR
#if REAL_PERFORMANCE_MODE
            if(UrpCameraStackTag.sceneCamera.IsNotEmpty())UrpCameraStackTag.sceneCamera.EnableSceneCamera();
#endif
#else
    #if _USE_DEV_BUILD
    #else
            if(UrpCameraStackTag.sceneCamera.IsNotEmpty())UrpCameraStackTag.sceneCamera.EnableSceneCamera();
    #endif
#endif
             
        }

        void MainCameraDisable(Camera currentCamera)
        {
#if UNITY_EDITOR
#if REAL_PERFORMANCE_MODE
            if(UrpCameraStackTag.sceneCamera.IsNotEmpty())UrpCameraStackTag.sceneCamera.DisableSceneCamera();
#endif
#else
    #if _USE_DEV_BUILD
    #else
            if(UrpCameraStackTag.sceneCamera.IsNotEmpty())UrpCameraStackTag.sceneCamera.DisableSceneCamera();
    #endif
#endif
            _blurContext.settingsIsDirty = true;
        }
    }
}