using System.Threading;
/********************************************************************
	created:	2021/06/25  18:51
	file base:	UrpCameraStackTag
	author:		c a o   f e n g
	
	purpose:	用来设置相机tag属性 overlay 与base状态的切换
*********************************************************************/

using Impostors.Managers;
using Impostors.URP;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using CFClient;
using CFEngine;
using CFEngine.WorldStreamer;
using UnityEditor;
using UnityEngine;
using UnityEngine.CFUI;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering.Universal;

namespace CFEngine
{

    public enum UrpCameraTag
    {
        None = -1,
        Scene = 0,
        UGUI = 1,
        UIScene = 2,
        Stack = 3,
    }

    // [ExecuteInEditMode]
    public class UrpCameraStackTag : MonoBehaviour, ICamera
    {
        public UrpCameraTag tag = UrpCameraTag.None;

        private Camera _camera;
        private UniversalAdditionalCameraData _uacd;
        private bool _isCamTempExchanged;
        
        public Camera Camera => _camera;
        
        public static UrpCameraStackTag uiCamera;
        public static UrpCameraStackTag sceneCamera;
        public int defaultCullingMask;
        
        /* Add by:  Takehsi;
           Date:    2022/1/27;
           purpose: Redesign the logic of Camera stack for manage mulpltie camera appliction state;
                    (some VFX like wave need anothor camera to draw Normal map, this camera is not belong to “Scene” or “UI” camera).
                    And guarantee Linear UI render Pipeline can operate right. 
           */
        public static List<Camera> MainCameraStackProxy = new List<Camera>();
        [HideInInspector]public List<Camera> CameraStack;

        private static bool m_IsLoading = false;

        public static bool IsLoading
        {
            get { return m_IsLoading; }
        }

        private static void UICameraShow(bool loading)
        {
            //Debug.Log("UrpCameraStackTag UICameraShow: loading " + loading+"  m_IsLoading:"+m_IsLoading);

            if (loading && !m_IsLoading && uiCamera._uacd.renderType == CameraRenderType.Overlay)
            {
                m_IsLoading = loading;
                m_LastUrpCameraTag = UrpCameraTag.None;
                uiCamera._uacd.renderType = CameraRenderType.Base;
                if (sceneCamera != null)
                {
                    sceneCamera._camera.enabled = false;
                }

                PopUICamera();
            }
            else if (m_IsLoading && !loading && uiCamera._uacd.renderType == CameraRenderType.Base)
            {
                m_IsLoading = loading;
                uiCamera._uacd.renderType = CameraRenderType.Overlay;
                PushUICamera();
                if (sceneCamera != null)
                {
                    sceneCamera._camera.enabled = true;
                    sceneCamera.RefreshMainCameraStack();
                    if (m_LastUrpCameraTag != UrpCameraTag.None)
                    {
                        sceneCamera.ChangeCameraTag(m_LastUrpCameraTag);
                        m_LastUrpCameraTag = UrpCameraTag.None;
                    }
                  //  Debug.Log("UrpCameraStackTag UICameraShow:" + sceneCamera, sceneCamera.gameObject);
                }
            }

            
        }

        public void Awake()
        {
            if (tag == UrpCameraTag.Scene)
            {
                SceneLoadSystem.InitUICameraShow(UICameraShow);
            }

           // Debug.Log("UrpCameraStackTag Awake:"+tag+"  "+gameObject,gameObject);
            
            Initialize();
            PSComponent.EveryPlayFunc = WaveCameraInitializer.Initializer.Init;
        }

        /// <summary>
        /// <para> Set camera tag and put camera in Main Camera Stack </para><para> カメラがタグを設定し、メインカメラスタックに配置します </para><para> 设置相机标签, 并把相机塞进著相机堆栈 </para>
        /// </summary>
        /// <param name="urpCameraTag"> None, Scene, UGUI, UIScene, Stack </param>

        public void OnEnable()
        {
            if (m_IsLoading)
            {
                sceneCamera.Camera.enabled = false;
            }
            else
            {
                RefreshMainCameraStack();
            }

        }

        private void RefreshMainCameraStack()
        {
            bool isNewUICamera = tag == UrpCameraTag.UGUI && uiCamera == null;
            bool isNewSceneCamera = tag == UrpCameraTag.Scene && sceneCamera == null;
            if (isNewUICamera || isNewSceneCamera)
            {
                Initialize();
            }

            if (tag == UrpCameraTag.Scene)
            {
                /* Copy items in original Camera Stack list to custom list. */
                if (_uacd.cameraStack != null)
                {
                    for (int i = _uacd.cameraStack.Count - 1; i > -1; i--)
                    {
                        if (!MainCameraStackProxy.Contains(_uacd.cameraStack[i]))
                            MainCameraStackProxy.Insert(0, _uacd.cameraStack[i]);
                    }
                }

                /* Point custom list to main camera stack. */
                _uacd.cameraStack = MainCameraStackProxy;
            }
            else if (tag == UrpCameraTag.UGUI)
            {
                /* Make UGUI camera always at last of list */
                if (!MainCameraStackProxy.Contains(_camera))
                    MainCameraStackProxy.Add(_camera);
                else if (MainCameraStackProxy.Last() != _camera)
                {
                    MainCameraStackProxy.Remove(_camera);
                    MainCameraStackProxy.Add(_camera);
                }
            }
            else if (tag == UrpCameraTag.Stack && !MainCameraStackProxy.Contains(_camera))
            {
                MainCameraStackProxy.Insert(0, _camera);
                /* Sequence logic of common camera in stack  can replenish here... */
            }

            //SynchronizeMainCameraStacklist();
        }

        public void OnDisable()
        {
            if (tag == UrpCameraTag.UGUI || tag == UrpCameraTag.Stack)
            {
                if (uiCamera == this)
                    uiCamera = null;
                MainCameraStackProxy.Remove(_camera);
                //SynchronizeMainCameraStacklist();
            }

            if (tag == UrpCameraTag.Scene || tag == UrpCameraTag.UIScene)
                sceneCamera = null;
        }

        private void SynchronizeMainCameraStacklist()
        {
            /* Synchronize Stack list */
            if (sceneCamera != null)
            {
                sceneCamera.CameraStack.Clear();
                sceneCamera.CameraStack.AddRange(MainCameraStackProxy);
            }
        }
        /* End Add */
        
        public void SetRenderEnabled(bool enable)
        {
            if (_uacd)
                _uacd.renderPostProcessing = enable;
            else
                Debug.LogError($"SetRendererEnabled: Get UniversalAdditionalCameraData fail, camera = {_camera}");

            if (enable)
                RevertCullingMask();
            else
                SetCullingMask(0);
        }

        public bool GetPosition(out Vector3 result)
        {
            result = _camera ? _camera.transform.position : Vector3.zero;
            return _camera;
        }
        
        private int GetDefulatCullingMask()
        {
            return defaultCullingMask;
        }
        
        private void SetCullingMask(int cullingMask)
        {
            if (_camera)
                _camera.cullingMask = cullingMask;
        }

        private void RevertCullingMask()
        {
            if (_camera)
                _camera.cullingMask = defaultCullingMask;
        }

        public void Initialize()
        {
            if (tag == UrpCameraTag.None && UrpCameraStackContext.Context.Tag != UrpCameraTag.None)
            {
                using (var context = UrpCameraStackContext.Context)
                {
                    tag = context.Tag;
                    _camera = context.Camera;
                    _uacd = context.Uacd;
                }
            }
            else
            {
                _camera = GetComponent<Camera>();
                _uacd = GetComponent<UniversalAdditionalCameraData>();
            }
            ChangeCameraTagCommon(tag);

            if (tag == UrpCameraTag.Scene)
            {

                Camera cam = gameObject.GetComponent<Camera>();
                EngineUtility.SetMainCamera(cam);
                CinemachineTrack.SetMainCamera(cam);
                UnityEngine.Rendering.Universal.Lighting.SetMainCamera(cam);
                GraphicRaycaster.SetMainCamera(cam);
                
                if (GetComponent<CameraAvoidBlock>() == null)
                {
                    gameObject.AddComponent<CameraAvoidBlock>();
                }

                if (GetComponent<PhysicsCommand>() == null)
                {
                    gameObject.AddComponent<PhysicsCommand>();
                }
                
                // if (GetComponent<DynamicCulling>() == null)
                //{
                //     gameObject.AddComponent<DynamicCulling>();
                // }

                if (GetComponent<SceneLoadMgr>() == null)
                {
                    gameObject.AddComponent<SceneLoadMgr>();
                }

                if (OpenWorld.Instance != null)
                {
                    OpenWorld.Instance.InitByCamera(true);
                }

                CameraStack = _uacd.cameraStack;
            }

            if (ImpostorableObjectsManager._instance != null)
            {
                ImpostorableObjectsManager._instance.enabled = true;
            }


            if (UniversalRenderPipelineProxy._instance != null)
            {
                UniversalRenderPipelineProxy._instance.enabled = true;
            }
            
            defaultCullingMask = _camera.cullingMask;
            //Debug.Log($"{name} default culling mask = {defaultCullingMask}");
        }


        private RenderTexture target;
        private RenderTexture targetDepth;
        public static int _SceneRT = Shader.PropertyToID("_SceneRT");
        public static int _CameraDepthRT = Shader.PropertyToID("_CameraDepthRT");
        private UrpCameraTag lastTag = UrpCameraTag.None;


        private static UrpCameraTag m_LastUrpCameraTag = UrpCameraTag.None;



        /*
        * created:2021/06/25 18:53
        * des : 根据tag 改变相机属性 
        */
        public void ChangeCameraTag(UrpCameraTag targetTag)
        {
          //  Debug.Log("UrpCameraStackTag ChangeCameraTag:" + targetTag+"  m_IsLoading:"+m_IsLoading, gameObject);
            
            // if (lastTag == targetTag) return;
            m_LastUrpCameraTag = targetTag;
            if(m_IsLoading) return;
            ChangeCameraTagCommon(targetTag);
        }
        
        public void ChangeCameraTagCommon(UrpCameraTag targetTag)
        {
            if (lastTag == UrpCameraTag.UIScene)
                ReleaseUIScene_RT();


            if (targetTag == UrpCameraTag.UGUI)
            {
                uiCamera = this;
                uiCamera._camera.gameObject.layer = LayerMaskName.UI;//Add by:Takeshi
            }
            else if (targetTag == UrpCameraTag.Scene)
            {
                /* Change by: Takeshi ;
                   Date:      2022/1/27;
                   Issue:     There are some problem of logic here;
                              commands run here is must base on Target is not UGUI;
                              in this moment，UI Camera is Null;
                              so we can't catch and put the UI camera in list of Cammera stack with following command. 
                              */
                // if (_uacd != null && uiCamera != null)
                // {
                //     _uacd.cameraStack.Add(uiCamera._camera);
                //     //DebugLog.AddLog2("Add baseCamera:" + uiCamera, _uacd);
                // }
                /* end */
                sceneCamera = this;
                EngineContext.sceneCamera = this;
                sceneCamera._camera.gameObject.layer = LayerMaskName.Default;
            }
            else if (targetTag == UrpCameraTag.UIScene)
            {
                UIScene_RT();
            }
            
            lastTag = targetTag;
        }

        public static bool renderPostProcessingWith3DUI = false;

        private bool renderPostProcessing;
        private CameraClearFlags _CameraClearFlags;


        /*
        * created:2021/06/25 18:53
        * des :设置相机的rt 
        */
        private void UIScene_RT()
        {
            if (_camera.IsEmpty() || _uacd.IsEmpty()) return;

            PopUICamera();


            _uacd.cameraStack.Clear();
#if UNITY_EDITOR
            Debug.Log( "UIScene_RT Change:" +  new System.Diagnostics.StackTrace().ToString());
#endif

            renderPostProcessing = _uacd.renderPostProcessing;
            _CameraClearFlags = _camera.clearFlags;

            // _uacd.renderPostProcessing = renderPostProcessingWith3DUI;

            _camera.clearFlags = CameraClearFlags.SolidColor;


            int width = Screen.width;
            int height = Screen.height;

#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                GetGameRenderSize(out width, out height);
            }
#endif
            float scale = UniversalRenderPipeline.asset.renderScale;
            width = (int)(width * scale);
            height = (int)(height * scale);

            target = RenderTexture.GetTemporary(width, height, 16, GraphicsFormat.R8G8B8A8_SRGB,
                UniversalRenderPipeline.msaaSampleCount); //  new RenderTexture(rtd);

            if (sceneCamera != null && sceneCamera._camera != null)
            {
                sceneCamera._camera.gameObject.layer = LayerMaskName.UIScene; /* By:Takeshi 标记相机用于渲染RT */
                sceneCamera._uacd.cameraStack = null;// Add by: Takehsi;
            }

            //try
            //{
            //    target.antiAliasing = UniversalRenderPipeline.msaaSampleCount;
            //}
            //catch (Exception e)
            //{
            //    DebugLog.AddErrorLog2("antiAliasing:" + UniversalRenderPipeline.msaaSampleCount + "  " + e.Message);
            //    target.antiAliasing = 1;
            //}
            
            _camera.targetTexture = target;

            Shader.SetGlobalTexture(_SceneRT, target);
        }


#if UNITY_EDITOR
        public static void GetGameRenderSize(out int width, out int height)
        {
            var gameView = GetMainGameView();

            if (gameView == null)
            {
                width = height = miscSize;
                return;
            }

            var prop = gameView.GetType().GetProperty("targetSize", BindingFlags.NonPublic | BindingFlags.Instance);
            var size = (Vector2) prop.GetValue(gameView, new object[] { });
            width = (int) size.x;
            height = (int) size.y;
        }


        const int miscSize = 1; // Used when no main GameView exists (ex: batchmode)

        static Type s_GameViewType = Type.GetType("UnityEditor.PlayModeView,UnityEditor");
        static string s_GetGameViewFuncName = "GetMainPlayModeView";
        static UnityEditor.EditorWindow GetMainGameView()
        {
            var getMainGameView =
                s_GameViewType.GetMethod(s_GetGameViewFuncName, BindingFlags.NonPublic | BindingFlags.Static);
            if (getMainGameView == null)
            {
                Debug.LogError(string.Format(
                    "Can't find the main Game View : {0} function was not found in {1} type ! Did API change ?",
                    s_GetGameViewFuncName, s_GameViewType));
                return null;
            }

            var res = getMainGameView.Invoke(null, null);
            return (UnityEditor.EditorWindow) res;
        }
#endif

        /*
        * created:2021/06/25 18:54
        * des : 释放相机rt
        */
        private void ReleaseUIScene_RT()
        {
            if (_camera.IsEmpty() || _uacd.IsEmpty()) return;

            PushUICamera();

            _uacd.renderPostProcessing = renderPostProcessing;
            _camera.clearFlags = _CameraClearFlags;


            if (target.IsNotEmpty())
            {
                RenderTexture.ReleaseTemporary(target);
                target = null;
            }

            if (targetDepth.IsNotEmpty())
            {
                RenderTexture.ReleaseTemporary(targetDepth);
                targetDepth = null;
            }

            _camera.targetTexture = null;
            sceneCamera._camera.gameObject.layer = LayerMaskName.Default;
            sceneCamera._uacd.cameraStack = MainCameraStackProxy;// Add by: Takehsi;
        }

        private static void PushUICamera()
        {
            if (uiCamera.IsNotEmpty() && !MainCameraStackProxy.Contains(uiCamera._camera))
            {
                uiCamera._uacd.renderType = CameraRenderType.Overlay;
                MainCameraStackProxy.Add(uiCamera._camera); // Add by: Takehsi;
            }
        }
        
        private static void PopUICamera()
        {
            if (uiCamera.IsNotEmpty() && MainCameraStackProxy.Contains(uiCamera._camera))
            {
                MainCameraStackProxy.Remove(uiCamera._camera); // Add by: Takehsi;
                uiCamera._uacd.renderType = CameraRenderType.Base;
            }
        }

        public void EnableSceneCamera()
        {
            if (sceneCamera.IsNotEmpty() && sceneCamera._camera.IsNotEmpty() && sceneCamera.tag != UrpCameraTag.UIScene && uiCamera.IsNotEmpty())
            {
                sceneCamera._camera.enabled = true;
                PushUICamera();
            }

        }
        public void DisableSceneCamera()
        {
            if (sceneCamera.IsNotEmpty() && sceneCamera._camera.IsNotEmpty() && sceneCamera.tag != UrpCameraTag.UIScene && uiCamera.IsNotEmpty())
            {
                PopUICamera();
                sceneCamera._camera.enabled = false;
            }
        }
    }
}
