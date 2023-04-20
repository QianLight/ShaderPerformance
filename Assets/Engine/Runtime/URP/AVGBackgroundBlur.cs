using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using static UnityEngine.Rendering.RenderPipelineManager;
using Object = UnityEngine.Object;

namespace CFEngine
{
    public class AVGBackgroundBlur : BlurCore
    {
        private BlurContext _blurContext;
        private Camera _cameraGrabColor;
        private Renderer _renderer;
        private bool _isAlready;
        public AVGBackgroundBlur(BlurContext context)
        {
            _blurContext = context;
        }

        public override void Excecute()
        {
            _renderer = _blurContext.rendererGameObject.GetComponent<Renderer>();
            if (_renderer)
            {
                _renderer.enabled = false;
            }
            
            beginCameraRendering -= CrabColor;
            beginCameraRendering += CrabColor;
            
            endCameraRendering -= StartBlur;
            endCameraRendering += StartBlur;
        }

        public override void OnClear()
        {
            _isAlready = false;
            ReSetMainCamera();
        }

        private void CrabColor(ScriptableRenderContext renderContext, Camera camera)
        {
            var mainCam = Camera.main;
            if (!mainCam)
                return;

            _cameraGrabColor = CreateCameraGrabColor(mainCam);
            
            if (camera != _cameraGrabColor)
            {
                return;
            }
            
            EnqueuePassCrabColor(_cameraGrabColor,MainCameraShotRoleOnly, _blurContext);
            beginCameraRendering -= CrabColor;
        }

        private void StartBlur(ScriptableRenderContext renderContext, Camera camera)
        {
            if (!_isAlready)
                return;

            var mainCam = Camera.main;
            if (!mainCam || camera != mainCam)
            {
                return;
            }

            Blur(renderContext,camera,_blurContext);

            if (_blurContext.process == _blurContext.iteration)
            {
                endCameraRendering -= StartBlur;
            }
        }


        private Camera CreateCameraGrabColor(Camera mainCam)
        {
            if (mainCam && !_cameraGrabColor)
            {
                Camera cam = CreateCameraDisposable(mainCam);
                return cam;
            }

            return _cameraGrabColor;
        }


        Camera CreateCameraDisposable (Camera mainCamera)
        {
            var ob = new GameObject();
            var cam = ob.AddComponent<Camera>();
            var uacd = ob.AddComponent<UniversalAdditionalCameraData>();
            uacd.renderPostProcessing = true;
            cam.CopyFrom(mainCamera);
            cam.cullingMask ^= 1 << LayerMask.NameToLayer("Role");
            cam.depth = mainCamera.depth - 1;
             
            endCameraRendering -= DestroyCam;
            endCameraRendering += DestroyCam;
            
            void DestroyCam(ScriptableRenderContext context, Camera camera)
            {
                if (camera != cam)
                    return;
                
                Object.Destroy(cam.gameObject);
                endCameraRendering -= DestroyCam;
                
                _isAlready = true;
                
                var cmd = CommandBufferPool.Get();
                cmd.Clear();
                cmd.SetGlobalTexture(_blurContext.staticSceneBlurTexture,_blurContext.tmpRT);
                context.ExecuteCommandBuffer(cmd);
                CommandBufferPool.Release(cmd);
                if (_renderer)
                    _renderer.enabled = true;
            }
            
            return cam;
        }
        
        void MainCameraShotRoleOnly(Camera currentCamera)
        {
            Camera cam = Camera.main;
            if(!cam)
                return;

            if (!_blurContext.settingsIsDirty)
            {
                _blurContext.savedLayermask = cam.cullingMask;
                cam.cullingMask = 1 << LayerMask.NameToLayer("Role");
                _blurContext.settingsIsDirty = true;
            }
        }
        
        void ReSetMainCamera()
        {
            Camera cam = Camera.main;
            if(!cam)
                return;
            if (_blurContext.settingsIsDirty)
            {
                cam.cullingMask = _blurContext.savedLayermask;
            }
        }
    }
}