using System;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace CFEngine
{
    public class UrpCameraStackContext : IDisposable
    {
        private static UrpCameraStackContext _context;
        public static readonly object ContextLock = new object();
        private UrpCameraStackContext(){}
        private bool _isDisposing;

        public static UrpCameraStackContext Context
        {
            get
            {
                lock (ContextLock)
                {
                    if (_context == null)
                        _context = new UrpCameraStackContext();
                    return _context;
                }
            }
        }
        
        private UrpCameraTag _tag = UrpCameraTag.None;
        private Camera _camera;
        private UniversalAdditionalCameraData _uacd;

        public UrpCameraTag Tag => _tag;
        public Camera Camera => _camera;
        public UniversalAdditionalCameraData Uacd => _uacd;

        private Camera NewCamera(out UniversalAdditionalCameraData uacd,CameraRenderType cameraRenderType, string cameraName ="Camera", UrpCameraTag tag = UrpCameraTag.None)
        {
            GameObject gameObject = new GameObject();
            gameObject.name = cameraName;
            Camera camera = gameObject.AddComponent<Camera>();
            _camera = camera;
            uacd = gameObject.AddComponent<UniversalAdditionalCameraData>();
            uacd.renderType = cameraRenderType;
            _uacd = uacd;
            _tag = tag;
            gameObject.AddComponent<UrpCameraStackTag>();
            return camera;
;        }
        public static Camera CreateNewCamera(out UniversalAdditionalCameraData uacd,CameraRenderType cameraRenderType, string cameraName = "Camera", UrpCameraTag tag = UrpCameraTag.None)
        {
            using (Context)
            {
                lock (ContextLock)
                {
                    return Context.NewCamera(out uacd, cameraRenderType, cameraName, tag);
                }
            }
        }

        public void Dispose()
        {
            Clear();
            if (!_isDisposing)
                Dispose(true);
        }
        public void Dispose(bool isDisposing)
        {
            _isDisposing = true;
            lock (ContextLock)
            {
                _context = null;
            }
        }

        public void Clear()
        {
            _tag = UrpCameraTag.None;
            _camera = null;
            _uacd = null;
        }
    }

    public abstract class CameraInitializer
    {
        public abstract void Init();
    }
}