using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Impostors.Editor;
using Impostors;
using CFEngine;
using Impostors.Managers;
using UnityEngine.Rendering.Universal;
using Impostors.URP;
using MeshLOD;
using Unity.EditorCoroutines.Editor;
using UnityEngine.SocialPlatforms;

public class ImpostorHelper
{
    public static bool IsImpostors = false;
    
    public static float screenRelativeTransitionHeight = 0.1f;
    public static bool IsForceRes = false;
    public static bool IsDiscardOriLOD = false;
    public static float deltaCameraAngle = 10f;

    public static float zOffset = 0.25f;
    
    public static TextureResolution forceMinTextureResolution = TextureResolution._64x64;
    public static TextureResolution forceMaxTextureResolution = TextureResolution._128x128;
    

    public static void CopyToCfg(ImpostorsCfg target)
    {
        target.screenRelativeTransitionHeight = screenRelativeTransitionHeight;
        target.deltaCameraAngle = deltaCameraAngle;
        target.IsForceRes = IsForceRes;
        target.IsDiscardOriLOD = IsDiscardOriLOD;
        target.forceMinTextureResolution = forceMinTextureResolution;
        target.forceMaxTextureResolution = forceMaxTextureResolution;
    }

    public static void CopyFromCfg(ImpostorsCfg src)
    {
        screenRelativeTransitionHeight = src.screenRelativeTransitionHeight;
        deltaCameraAngle = src.deltaCameraAngle;
        IsForceRes = src.IsForceRes;
        IsDiscardOriLOD = src.IsDiscardOriLOD;
        forceMinTextureResolution = src.forceMinTextureResolution;
        forceMaxTextureResolution = src.forceMaxTextureResolution;
    }

    public static float cutout = 0.8f;
    public static float cutoutTransparentFill = 1.0f;


    private static List<int> ignoreList = new List<int>()
    {
        LayerMask.NameToLayer("CULL_LOD0"),
        LayerMask.NameToLayer("CULL_LOD1"),
        LayerMask.NameToLayer("CULL_LOD2")
    };

    [MenuItem("GameObject/Impostor/ImpostorLodChilds", false, 0)]
    public static void ImpostorLodChilds()
    {
        GameObject[] gameobjects = Selection.gameObjects;
        ToolsImpostorLod(gameobjects);
    }

    [MenuItem("GameObject/Impostor/Force/ImpostorLodChilds", false, 0)]
    public static void ImpostorLodChildsForce()
    {
        GameObject[] gameobjects = Selection.gameObjects;

        for (int i = 0; i < gameobjects.Length; i++)
        {
            GameObject obj = gameobjects[i];
            ImpostorsCfg cfg = obj.GetComponent<ImpostorsCfg>();

            if (cfg == null)
                cfg = obj.AddComponent<ImpostorsCfg>();

        }

        ToolsImpostorLod(gameobjects);
    }


    [MenuItem("GameObject/Impostor/PrintMemory", false, 0)]
    public static void PrintMemory()
    {
        ImpostorLODGroupsManager lodMgr = GameObject.FindObjectOfType<ImpostorLODGroupsManager>();

        Debug.Log(lodMgr.GetUsedBytes());
    }

    [MenuItem("GameObject/Impostor/PrintTextureUseMemory", false, 0)]
    public static void PrintTextureUseMemory()
    {
        float textureMemoryUsed = ImpostorLODGroupsManager.Instance.RenderTexturePool.CaculateUseMemory();
        Debug.Log(textureMemoryUsed + " m");
    }

    public static void AdllImpostorToChild()
    {
        Transform selectionTf = Selection.activeTransform;

        ImpostorLODGroupsManager lodMgr = GameObject.FindObjectOfType<ImpostorLODGroupsManager>();
        if (lodMgr == null)
        {
            GameObject objEditorScene = GameObject.Find("EditorScene");
            Selection.activeGameObject = objEditorScene;
            ImpostorsEditorTools.CreateSceneManagers();
            Selection.activeGameObject = selectionTf.gameObject;
        }
        

        for (int j = 0; j < selectionTf.childCount; j++)
        {
            GameObject obj = selectionTf.GetChild(j).gameObject;
            AdllImpostorToChildItem(obj);
        }
    }

    private static void AdllImpostorToChildItem(GameObject obj)
    {
        if (ignoreList.Contains(obj.layer)) return;

        LODGroup oldLOD = obj.GetComponent<LODGroup>();
        if (oldLOD != null)
        {
            if (!IsDiscardOriLOD) return;
            GameObject.DestroyImmediate(oldLOD);
        }

        MeshRenderer[] mrs = obj.GetComponentsInChildren<MeshRenderer>();

        if (!IsForceRes)
        {
            for (int i = 0; i < mrs.Length; i++)
            {
                MeshRenderer mr = mrs[i];
                if (mr.lightmapIndex >= 0 && mr.lightmapIndex < 10000)
                {
                    continue;
                }
                
                return;
            }
        }



        LODGroup lodGroup = obj.AddComponent<LODGroup>();
        LOD[] lods = new LOD[1];
        lods[0].screenRelativeTransitionHeight = screenRelativeTransitionHeight;
        lods[0].renderers = mrs;
        lodGroup.SetLODs(lods);
        lodGroup.enabled = false;

        ImpostorsEditorTools.SetupImpostorLODGroupToObject(lodGroup);

        ImpostorLODGroup impostorLODGroup = lodGroup.GetComponent<ImpostorLODGroup>();

        impostorLODGroup.deltaCameraAngle = deltaCameraAngle;
        
        //float ratio = ImpostorsUtility.MaxRatio(impostorLODGroup.Size);
        if (impostorLODGroup.QuadSize < 10)
        {
            impostorLODGroup.maxTextureResolution = TextureResolution._32x32;
        }
        else if (impostorLODGroup.QuadSize < 15)
        {
            impostorLODGroup.maxTextureResolution = TextureResolution._64x64;
        }

        if (IsForceRes)
        {
            impostorLODGroup.minTextureResolution = forceMinTextureResolution;
            impostorLODGroup.maxTextureResolution = forceMaxTextureResolution;
        }
    }

    private static string isPreivewImpostor = "isPreivewImpostor";

    public static bool BIsPreivewImpostor
    {
        get
        {
            if (PlayerPrefs.HasKey(isPreivewImpostor) && PlayerPrefs.GetInt(isPreivewImpostor) == 1)
            {
                return true;
            }

            return false;
        }
    }

    private static string loadGameAtHere = "loadGameAtHere";
    private static string MeshLODCameraIntervalDistance = "MeshLODCameraIntervalDistance";
    
    [MenuItem("GameObject/Impostor预览", false, 0)]
    public static void PreivewImpostor()
    {
        
        PlayerPrefs.SetInt(isPreivewImpostor,1);
        
        EnvironmentExtra ee = GameObject.FindObjectOfType<EnvironmentExtra>();
        if (ee != null)
        {
            PlayerPrefs.SetInt(loadGameAtHere, ee.loadGameAtHere ? 1 : 0);
            ee.loadGameAtHere = false;
        }

        MeshLODRoot meshLODRoot = GameObject.FindObjectOfType<MeshLODRoot>();
        LODGroup[] groups = GameObject.FindObjectsOfType<LODGroup>();
        MeshLODGroup tempMeshLODGroup = null;
        if (meshLODRoot != null && meshLODRoot.meshLODGroupList != null)
        {
            for (int i = 0; i < groups.Length; i++)
            {
                if (groups[i] == null || groups[i].gameObject == null)
                {
                    continue;
                }
                
                bool isHave = false;
                for (int k = 0; k < meshLODRoot.meshLODGroupList.Count; k++)
                {
                    tempMeshLODGroup = meshLODRoot.meshLODGroupList[k];
                    if (tempMeshLODGroup.bindTransform == null)
                    {
                        continue;
                    }
                    if (groups[i].gameObject == tempMeshLODGroup.bindTransform.gameObject)
                    {
                        isHave = true;
                        break;
                    }
                }
                
                groups[i].enabled = !isHave;
            }
        }
        else
        {
            for (int i = 0; i < groups.Length; i++)
            {
                groups[i].enabled = true;
            }
        }

        MeshLODEntrancePoint meshLODEntrancePoint = GameObject.FindObjectOfType<MeshLODEntrancePoint>();
        if (meshLODEntrancePoint != null)
        {
            PlayerPrefs.SetFloat(MeshLODCameraIntervalDistance, meshLODEntrancePoint.cameraIntervalDistance);
            meshLODEntrancePoint.cameraIntervalDistance = 0.01f;
        }
        
        EditorApplication.isPlaying = true;
        
    }

    [InitializeOnLoadMethod]
    static void InitPreivewImpostor()
    {
        EditorApplication.playModeStateChanged += ChangedPlaymodeState;
    }
    
    static void ChangedPlaymodeState(PlayModeStateChange obj)
    {
        //EditorApplication.playModeStateChanged -= ChangedPlaymodeState;
        
        
        switch (obj)
        {
            case PlayModeStateChange.EnteredEditMode:

                if (BIsPreivewImpostor)
                {
                    PlayerPrefs.SetInt(isPreivewImpostor, 0);
                    EnvironmentExtra ee = GameObject.FindObjectOfType<EnvironmentExtra>();

                    if (ee != null)
                    {
                        ee.loadGameAtHere = PlayerPrefs.GetInt(loadGameAtHere) == 1;
                    }
                    
                    LODGroup[] groups = GameObject.FindObjectsOfType<LODGroup>();
                    for (int i = 0; i < groups.Length; i++)
                    {
                        groups[i].enabled = false;
                    }
                    
                    MeshLODEntrancePoint meshLODEntrancePoint = GameObject.FindObjectOfType<MeshLODEntrancePoint>();
                    if (meshLODEntrancePoint != null)
                    {
                        meshLODEntrancePoint.cameraIntervalDistance = PlayerPrefs.GetFloat(MeshLODCameraIntervalDistance, meshLODEntrancePoint.cameraIntervalDistance);
                    }
                    
                }

                break;
            case PlayModeStateChange.ExitingEditMode:
                break;
            case PlayModeStateChange.EnteredPlayMode:
                if (BIsPreivewImpostor)
                {
                    
                }

                break;
            case PlayModeStateChange.ExitingPlayMode:
                break;
        }
    }
    
    public static void ToolsImpostorLod(GameObject[] gameobjects)
    {
        ImpostorLODGroupsManager lodMgr = GameObject.FindObjectOfType<ImpostorLODGroupsManager>();
        lodMgr.cutout = cutout;
        lodMgr._cutoutTransparentFill = cutoutTransparentFill;


        for (int i = 0; i < gameobjects.Length; i++)
        {
            GameObject child = gameobjects[i];

            ImpostorsCfg cfg = child.GetComponent<ImpostorsCfg>();

            if (child.GetComponent<ImpostorLODGroup>() != null) continue;

            for (int j = 0; j < child.transform.childCount; j++)
            {
                GameObject obj = child.transform.GetChild(j).gameObject;

                ToolsImpostorLodByGameObject(obj, cfg);
            }
        }
    }

    public static void ToolsImpostorLodByGameObject(GameObject obj, ImpostorsCfg parentCfg)
    {
        if (ignoreList.Contains(obj.layer)&&parentCfg==null) return;

        if (obj.GetComponent<LODGroup>() != null) return;
        MeshRenderer[] mrs = obj.GetComponentsInChildren<MeshRenderer>();

        if (parentCfg == null)
        {
            for (int i = 0; i < mrs.Length; i++)
            {
                MeshRenderer mr = mrs[i];
                if (mr.lightmapIndex >= 0 && mr.lightmapIndex < 10000)
                {
                    continue;
                }
                return;
            }
        }



        LODGroup lodGroup = obj.AddComponent<LODGroup>();
        LOD[] lods = new LOD[1];
        //lods[0].screenRelativeTransitionHeight = screenRelativeTransitionHeight;
        lods[0].renderers = mrs;
        lodGroup.SetLODs(lods);


        ImpostorsEditorTools.SetupImpostorLODGroupToObject(lodGroup);

        ImpostorLODGroup impostorLODGroup = lodGroup.GetComponent<ImpostorLODGroup>();

        //float ratio = ImpostorsUtility.MaxRatio(impostorLODGroup.Size);
        if (impostorLODGroup.QuadSize < 10)
        {
            impostorLODGroup.maxTextureResolution = TextureResolution._32x32;
        }
        else if (impostorLODGroup.QuadSize < 15)
        {
            impostorLODGroup.maxTextureResolution = TextureResolution._64x64;
        }

        // if (parentCfg != null && _ImpostorsCfg.isForceChildWithMaxTextureResolution)
        // {
        //     impostorLODGroup.maxTextureResolution = parentCfg.maxTextureResolution;
        // }


        bool isIngoreImpostor = false;

        if (impostorLODGroup.QuadSize < 6.0f && parentCfg == null)
        {
            isIngoreImpostor = true;
        }

        // ImpostorsCfg selfCfg = obj.GetComponent<ImpostorsCfg>();
        // if (selfCfg != null)
        // {
        //     impostorLODGroup.maxTextureResolution = selfCfg.maxTextureResolution;
        //
        //     if (selfCfg.isIngoreImpostor)
        //     {
        //         isIngoreImpostor = true;
        //     }
        // }

        if (isIngoreImpostor)
        {
            GameObject.DestroyImmediate(impostorLODGroup);
            GameObject.DestroyImmediate(lodGroup);
        }
    }


    [MenuItem("GameObject/Impostor/ImpostorLodSelections", false, 0)]
    public static void ImpostorLodSelections()
    {
        GameObject[] gameobjects = Selection.gameObjects;
        for (int i = 0; i < gameobjects.Length; i++)
        {
            GameObject obj = gameobjects[i];
            ToolsImpostorLodByGameObject(obj, obj.GetComponent<ImpostorsCfg>());
        }
    }


    [MenuItem("GameObject/Impostor/Force/ImpostorLodSelections", false, 0)]
    public static void ImpostorLodSelectionsForce()
    {
        GameObject[] gameobjects = Selection.gameObjects;
        for (int i = 0; i < gameobjects.Length; i++)
        {
            GameObject obj = gameobjects[i];
            ImpostorsCfg cfg = obj.GetComponent<ImpostorsCfg>();

            if (cfg == null)
                cfg = obj.AddComponent<ImpostorsCfg>();

            ToolsImpostorLodByGameObject(obj, cfg);
        }
    }


    [MenuItem("GameObject/Impostor/ImpostorRemoveLod", false, 0)]
   public static void ImpostorRemoveLod()
    {
        GameObject[] gameobjects = Selection.gameObjects;
        for (int i = 0; i < gameobjects.Length; i++)
        {
            GameObject child = gameobjects[i];

            for (int j = 0; j < child.transform.childCount; j++)
            {
                GameObject obj = child.transform.GetChild(j).gameObject;
                ImpostorLODGroup ilod = obj.GetComponent<ImpostorLODGroup>();
                if (ilod != null)
                {
                    GameObject.DestroyImmediate(ilod);
                }
                else
                {
                    continue;
                }

                LODGroup lod = obj.GetComponent<LODGroup>();
                if (lod != null)
                {
                    GameObject.DestroyImmediate(lod);
                }
            }
        }
    }
   
   public static void ImpostorSetZOffset()
   {
       GameObject[] gameobjects = Selection.gameObjects;
       for (int i = 0; i < gameobjects.Length; i++)
       {
           GameObject child = gameobjects[i];

           for (int j = 0; j < child.transform.childCount; j++)
           {
               GameObject obj = child.transform.GetChild(j).gameObject;
               ImpostorLODGroup ilod = obj.GetComponent<ImpostorLODGroup>();
               if (ilod == null)
               {
                   continue;
               }

               ilod.zOffset = zOffset;
           }
       }
   }
   
    [MenuItem("GameObject/Impostor/ImpostorAddCamera", false, 0)]
    static void ImpostorAddCamera()
    {
        //EditorApplication.playModeStateChanged += ImpostorAddCameraPlayModeState;
        //EditorApplication.EnterPlaymode();

        EditorCoroutineUtility.StartCoroutineOwnerless(ImpostorAddCameraEnum());
    }


    //static void ImpostorAddCameraPlayModeState(PlayModeStateChange pmsc)
    //{

    //    EditorApplication.playModeStateChanged -= ImpostorAddCameraPlayModeState;

    //    Camera cam = GameObject.FindObjectOfType<Camera>();
    //    if (cam != null) return;
    //    EditorCoroutineUtility.StartCoroutineOwnerless(ImpostorAddCameraEnum());
    //}

    static IEnumerator ImpostorAddCameraEnum()
    {
        yield return new EditorWaitForSeconds(1);

        Camera cam = new GameObject("Camera").gameObject.AddComponent<Camera>();
        cam.tag = "MainCamera";
        cam.gameObject.AddComponent<SceneViewCameraFollower>();

        UniversalAdditionalCameraData uacd = cam.GetUniversalAdditionalCameraData();
        uacd.renderPostProcessing = true;

        Light[] allLights = GameObject.FindObjectsOfType<Light>(true);
        for (int i = 0; i < allLights.Length; i++)
        {
            Light lig = allLights[i];
            if (lig.type != LightType.Directional) continue;
            if (lig.gameObject.name == "mainLight")
            {
                lig.gameObject.SetActive(true);
                break;
            }
        }


        ImpostorableObjectsManager iom = GameObject.FindObjectOfType<ImpostorableObjectsManager>();
        iom.mainCamera = cam;
        //iom.debugModeEnabled = true;
        iom.isShowByGameRole = false;
        iom.enabled = true;

        UniversalRenderPipelineProxy urpp = GameObject.FindObjectOfType<UniversalRenderPipelineProxy>();
        urpp.mainCamera = cam;
        urpp.enabled = true;


        EnvironmentVolume ev = GameObject.FindObjectOfType<EnvironmentVolume>();
        if (ev != null)
        {
            if (ev.m_CameraCull)
            {
                float[] layerCull = cam.layerCullDistances;
                layerCull[LayerMask.NameToLayer(ev.m_CullLod0.LayerName)] = ev.m_CullLod0.Distance;
                layerCull[LayerMask.NameToLayer(ev.m_CullLod1.LayerName)] = ev.m_CullLod1.Distance;
                layerCull[LayerMask.NameToLayer(ev.m_CullLod2.LayerName)] = ev.m_CullLod2.Distance;
                cam.layerCullDistances = layerCull;
            }
        }
    }


    [MenuItem("GameObject/Impostor/EnableOrDisable", false, 0)]
    static void EnableOrDisable()
    {

        ImpostorableObjectsManager iom = GameObject.FindObjectOfType<ImpostorableObjectsManager>();
        bool state = !iom.enabled;
        iom.enabled = state;

        UniversalRenderPipelineProxy urpp = GameObject.FindObjectOfType<UniversalRenderPipelineProxy>();
        urpp.enabled = state;

        LODGroup[] groups = GameObject.FindObjectsOfType<LODGroup>();
        for (int i = 0; i < groups.Length; i++)
        {
            groups[i].enabled = state;
        }


    }

}
