
using System.Collections.Generic;
using System.Diagnostics;
using CFClient;

#if UNITY_EDITOR
using UnityEditor;
#endif

using UnityEngine;

namespace CFEngine
{
    public enum BlurType
    {
        None,
        SceneBlur,
        TextureBlur,
        CameraRT,
    }

    [DisallowMultipleComponent, ExecuteInEditMode]
    public class UIScene : MonoBehaviour
        
#if UNITY_EDITOR
        , IEnvContainer
#endif
    
    {
        public bool snapShotFolder = true;
        public List<Transform> uiSceneSnapShot = new List<Transform> ();
        public bool staticSnapShotFolder = true;

        public List<Transform> uiSceneStaticSnapShot = new List<Transform> ();

        public bool cameraFolder = true;
        public Transform uiSceneCamera;
        public BlurType blurType = BlurType.None;
        public Color sceneBlurColor;
        public Texture2D backgroundTex;
        public bool lightFolder = true;

        public bool IsNoRT = false;
        //public EnvAreaProfile profile;
        //public bool alongLightDir = false;
        public bool renderScene = false;
        //public bool staticBindLight = false;
        public bool selfShadow = false;
        public bool ignoreCameraPos = false;
        public bool planeShadow = false;
        public Color planeshadowcolor;
        [Range(0f,1f)]
        public float planeshadowfade;
        private Transform trans = null;
        //private Transform players = null;

        [System.NonSerialized]
        public static UIScene updateUIScene = null;

        public static int delayFrame = -1;
        public Transform GetTransform ()
        {
            if (trans == null)
            {
                trans = this.transform;
            }
            return trans;
        }
        public void Reset ()
        {
            //if (!EngineContext.IsRunning)
            //{
            //    EngineContext context = EngineContext.instance;
            //    if (context != null)
            //    {
            //        //if (profile != null)
            //        //    UISceneSystem.ResetEnvParam (context, profile.envBlock);
            //        context.backGroundTex = null;
            //    }
            //    int UISceneLayer = LayerMask.NameToLayer ("UIScene");
            //    for (int i = 0; i < uiSceneSnapShot.Count; ++i)
            //    {
            //        var snapShot = uiSceneSnapShot[i];
            //        if (snapShot != null)
            //        {
            //            for (int j = 0; j < snapShot.childCount; ++j)
            //            {
            //                var child = snapShot.GetChild (j);
            //                if (child.gameObject.activeInHierarchy)
            //                {
            //                    int layer = child.gameObject.layer;
            //                    if (layer == UISceneLayer)
            //                    {
            //                        var render = EditorCommon.GetRenderers (child.gameObject);
            //                        for (int k = 0; k < render.Count; ++k)
            //                        {
            //                            var r = render[k];
            //                            r.SetPropertyBlock (null);
            //                        }
            //                    }
            //                }
            //            }
            //        }

            //    }
            //    LightingModify.uiSceneLights = null;
            //}

        }


        private bool CheckUrpRtUse()
        {
            
            if (UrpCameraStackTag.sceneCamera.IsEmpty()) return false;
            
            if (IsNoRT) return false;
            
            return true;
        }

        void OnEnable()
        {
            if (CheckUrpRtUse())
                UrpCameraStackTag.sceneCamera.ChangeCameraTag(UrpCameraTag.UIScene);
            
            Shader.SetGlobalFloat("_IsUIScene", 1);
            EngineContext.uiCount++;
        }

        void OnDisable()
        {
            if (CheckUrpRtUse())
                UrpCameraStackTag.sceneCamera.ChangeCameraTag(UrpCameraTag.Scene);
            
            Reset();


            Shader.SetGlobalFloat("_IsUIScene", 0);
            EngineContext.uiCount--;
        }

        public void Update ()
        {
            //EngineContext context = EngineContext.instance;
            //if (updateUIScene != null && !EngineContext.IsRunning)
            //{
            //    //if (profile != null)
            //    //{
            //    //    context.logicflag.SetFlag (EngineContext.Flag_CameraFovDirty, true);
            //    //    EnvArea.GenEnvParamBlock (profile.envBlock);
            //    //    LightingModify.uiSceneLights = profile;
            //    //    //// UISceneSystem.OverrideEnvParam (profile.envBlock);

            //    //}
            //    if (context != null && context.CameraTransCache != null)
            //    {
            //        GetTransform();
            //        trans.position = UISceneSystem.uiScenePosOffset;
            //        if (uiSceneCamera != null)
            //        {
            //            uiSceneCamera.localPosition = Vector3.zero;
            //            uiSceneCamera.localRotation = Quaternion.identity;
            //        }
            //        context.CameraTransCache.position = UISceneSystem.uiScenePosOffset;
            //        context.CameraTransCache.rotation = Quaternion.identity;// uiSceneCamera.rotation;
            //        if (players == null)
            //        {
            //            players = trans.Find("Players");
            //        }
            //        if (players != null)
            //        {
            //            players.localPosition = Vector3.zero;
            //            players.localRotation = Quaternion.identity;
            //            players.localScale = Vector3.one;
            //        }
            //        for (int i = 0; i < uiSceneSnapShot.Count; ++i)
            //        {
            //            var t = uiSceneSnapShot[i];
            //            if (t != null)
            //            {
            //                if (!t.TryGetComponent<UIScenePlayer>(out var uiPlayer))
            //                {
            //                    uiPlayer = t.gameObject.AddComponent<UIScenePlayer>();
            //                    if (t.childCount > 0)
            //                    {
            //                        var child = t.GetChild(0);
            //                        uiPlayer.pos = child.localPosition;
            //                        uiPlayer.rot = child.localEulerAngles;
            //                        uiPlayer.scale = child.localScale;
            //                    }
            //                }
            //            }
            //        }
            //        //if (delayFrame == 0)
            //        //{
            //        //    UISceneSystem.OverrideEnvParam (context, profile.envBlock, true);
            //        //}
            //        if (delayFrame > -1)
            //        {
            //            delayFrame--;
            //        }
            //    }
            // //   Shader.SetGlobalColor(ShaderManager._ColorPlane, new Vector4(planeshadowcolor.r, planeshadowcolor.g, planeshadowcolor.b, planeshadowfade));
            //    SetPlaneDrawData(context);
            //}
            ////if(updateUIScene != null)
            ////{
            //    Shader.SetGlobalColor(ShaderManager._ColorPlane, new Vector4(planeshadowcolor.r, planeshadowcolor.g, planeshadowcolor.b, planeshadowfade));
            ////}
        }

        private static GameObject uiSceneDebugGo;
        private static UIScene uiSceneObj;
#region runtime debug
        //[Conditional ("DEBUG")]
        //public static void InitDebug ()
        //{
        //    UISceneSystem.uiSceneDebug = OnUISceneDebug;
        //    UISceneSystem.uiSceneDebugObj = OnUISceneDebug;
        //}

        //static void OnUISceneDebug (ref UISceneObj uso, bool enable)
        //{
        //    if (enable)
        //    {
        //        uiSceneDebugGo = new GameObject (string.Format ("_DebugUIScene_{0}", uso.name));               
        //        uiSceneObj = uiSceneDebugGo.AddComponent<UIScene> ();
        //        uiSceneObj.uiSceneCamera = new GameObject ("Camera").transform;
        //        uiSceneObj.uiSceneCamera.parent = uiSceneObj.transform;
        //        var players = new GameObject ("Players").transform;
        //        players.parent = uiSceneObj.transform;
        //        for (int i = 0; i < uso.snapShortCount; ++i)
        //        {
        //            var player = new GameObject ("Player_" + i.ToString ()).transform;
        //            player.parent = players.transform;
        //            uiSceneObj.uiSceneSnapShot.Add (player);
        //        }
        //        //static
        //        var staicplayers = new GameObject("StaticPlayers").transform;
        //        staicplayers.parent = uiSceneObj.transform;
        //        for (int i = 0; i < uso.prefabCount; ++i)
        //        {
        //            var player = new GameObject("Player_" + i.ToString()).transform;
        //            player.parent = staicplayers.transform;
        //            uiSceneObj.uiSceneStaticSnapShot.Add(player);
        //        }
        //        uiSceneObj.backgroundTex = uso.backGroundTex != null?uso.backGroundTex.obj as Texture2D : null;
        //        uiSceneObj.sceneBlurColor = uso.color;
        //        uiSceneObj.blurType = (BlurType) uso.blurType;
        //        //uiSceneObj.alongLightDir = uso.flag.HasFlag (UISceneObj.Flag_AlongLightDir);
        //        uiSceneObj.renderScene = uso.flag.HasFlag (UISceneObj.Flag_RenderScene);
        //        uiSceneObj.selfShadow = uso.flag.HasFlag (UISceneObj.Flag_SelfShadow);
        //        uiSceneObj.planeShadow = uso.flag.HasFlag(UISceneObj.Flag_PlaneShadow);
        //        uiSceneObj.planeshadowcolor = new Color(uso.PlaneshadowParam.x,uso.PlaneshadowParam.y,uso.PlaneshadowParam.z);
        //        uiSceneObj.planeshadowfade = uso.PlaneshadowParam.w;

        //        //string path = string.Format ("{0}/Common/{1}.asset", AssetsConfig.instance.ResourcePath, uso.name);
        //        uiSceneObj.profile = EnvAreaProfile.Clone(uso.envBlock, uso.name);

        //        uiSceneDebugGo.transform.position = UISceneSystem.uiScenePosOffset;
        //    }
        //    else
        //    {
        //        if (uiSceneObj != null)
        //        {
        //            for (int i = 0; i < uiSceneObj.uiSceneSnapShot.Count; ++i)
        //            {
        //                var player = uiSceneObj.uiSceneSnapShot[i];
        //                while (player.childCount > 0)
        //                {
        //                    var child = player.GetChild (0);
        //                    child.parent = null;
        //                }
        //            }
        //            uiSceneObj = null;
        //        }
        //        if (uiSceneDebugGo != null)
        //        {
        //            GameObject.Destroy (uiSceneDebugGo);
        //            uiSceneDebugGo = null;

        //        }
        //    }
        //}

        //static void OnUISceneDebug (ref UISceneObj uso, int state, int index, XGameObject xgo,
        //    bool setTrans, bool setPRS,
        //    ref Vector3 pos, ref Quaternion rot, ref Vector3 scale,
        //    ref Vector3 addPos, ref Quaternion addRot, ref Vector3 addScale)
        //{
        //    if (state == UISceneSystem.Debug_SnapShot)
        //    {
        //        if (index < uiSceneObj.uiSceneSnapShot.Count)
        //        {
        //            var player = uiSceneObj.uiSceneSnapShot[index];
        //            if (setTrans)
        //            {
        //                xgo.SetParentTrans(player);
        //                //t.parent = player;
        //            }
        //            if (setPRS)
        //            {
        //                player.localPosition = pos;
        //                player.localRotation = rot;
        //                player.localScale = scale;

        //                xgo.SetLocalPRS(ref addPos, ref addRot, ref addScale, 0, XLocalPRSAsyncData.SetPRS);
        //                //t.localPosition = addPos;
        //                //t.localRotation = addRot;
        //                //t.localScale = addScale;
        //            }
        //        }
        //    }
        //    else if (state == UISceneSystem.Debug_StaticSnapShot)
        //    {
        //        if (index < uiSceneObj.uiSceneStaticSnapShot.Count)
        //        {
        //            var player = uiSceneObj.uiSceneStaticSnapShot[index];
        //            if (setTrans)
        //            {
        //                xgo.SetParentTrans(player);
        //            }
        //        }
        //    }
        //}

#endregion
        public static void GetPrefabs (Transform t, List<Transform> prefabs)
        {
            // if (EditorCommon.IsPrefabOrFbx (t.gameObject))
            // {
            //     prefabs.Add (t);
            // }
            // else
            //{
            //    for (int j = 0; j < t.childCount; ++j)
            //    {
            //        var child = t.GetChild (j);
            //        if (child.gameObject.activeInHierarchy)
            //        {
            //            prefabs.Add (child);
            //        }
            //    }
            //}
        }

        private void SetPlaneDrawData(EngineContext context, bool setdrawCalls = true)
        {
            //if (planeShadow && uiSceneSnapShot.Count > 0)
            //{
            //    for (int i = 0; i < uiSceneSnapShot.Count; i++)
            //    {
            //        if (i > 2)
            //            break;
            //        var trans = uiSceneSnapShot[i];
            //        if (trans.childCount > 0)
            //        {
            //            ref var planeshadowInfo = ref context.planeshadowInfos[i];
            //            var h = trans.transform.position.y;
            //            if (setdrawCalls)
            //            {
            //                for (int x = 0; x < trans.childCount; x++)
            //                {
            //                    GetTransform(ref planeshadowInfo, trans.GetChild(x), context, h);
            //                }
            //            }
            //        }
            //    }
            //}
        }


        private void GetTransform(ref SelfShadow ss, Transform t, EngineContext context, float h)
        {
            //if (t.gameObject.activeInHierarchy && t.TryGetComponent(out Renderer r))
            //{
            //    Material depthShadowMat = WorldSystem.GetEffectMat(EEffectMaterial.RolePlaneShadow);
            //    var db = SharedObjectPool<DrawBatch>.Get();
            //    db.render = r;
            //    db.mat = depthShadowMat;
            //    db.mpbRef = CommonObject<MaterialPropertyBlock>.Get();
            //    db.mpbRef.SetFloat(ShaderManager._RoleHeight, h);
            //    db.render.SetPropertyBlock(db.mpbRef);
            //    ss.drawCalls.Push(db);
            //}
            //for (int i = 0; i < t.childCount; ++i)
            //{
            //    GetTransform(ref ss, t.GetChild(i), context, h);
            //}
        }

        void OnDrawGizmos()
        {

            //EngineContext context = EngineContext.instance;
            //if (uiSceneCamera != null && context != null && context.CameraRef != null)
            //{
            //    var c = Gizmos.color;
            //    Gizmos.color = Color.green;
            //    Gizmos.matrix = Matrix4x4.TRS(uiSceneCamera.position, uiSceneCamera.rotation, Vector3.one);
            //    Gizmos.DrawFrustum(Vector3.zero, context.CameraRef.fieldOfView,
            //        context.CameraRef.farClipPlane, context.CameraRef.nearClipPlane, context.CameraRef.aspect);
            //    Gizmos.matrix = Matrix4x4.identity;
            //    Gizmos.color = c;
            //}
        }

    }
}