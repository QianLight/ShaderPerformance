//#if UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using CFEngine.WorldStreamer;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

namespace CFEngine
{
    [Serializable]
    public class LigthmapRes
    {
        public Texture2D color;
        public Texture2D shadowMask;
        public Texture2D dir;
        public Texture2D combine;

        public Texture2D colorCombineShadowMask;
        //[System.NonSerialized]
        //public ushort id;
    }


    [Serializable]
    public class LigthmapRenderData
    {
        public Renderer render;
        public int lightmapIndex;
        public Vector4 lightmapScaleOffset;
        public int realtimeLightmapIndex;
        public Vector4 realtimeLightmapScaleOffset;
    }

    [Serializable]
    public class LigthmapComponent
    {
        [Range(0, 5)]
        public float lightMapScale = 1.0f;
        public int lightMapIndex = -1;
        public int lightMapVolumnIndex = -1;
        public Vector4 lightmapUVST = new Vector4(1, 1, 0, 0);
        public LigthmapRes ligthmapRes = new LigthmapRes();
        public void Clear()
        {
            lightmapUVST = new Vector4(0, 0, 1, 1);
            lightMapIndex = -1;
            lightMapVolumnIndex = -1;
            ligthmapRes.color = null;
            ligthmapRes.shadowMask = null;
            ligthmapRes.combine = null;
            ligthmapRes.colorCombineShadowMask = null;
        }

        public void Copy(LigthmapComponent src)
        {
            lightMapScale = src.lightMapScale;
            lightmapUVST = src.lightmapUVST;
            lightMapIndex = src.lightMapIndex;
            lightMapVolumnIndex = src.lightMapVolumnIndex;
            ligthmapRes.color = src.ligthmapRes.color;
            ligthmapRes.shadowMask = src.ligthmapRes.shadowMask;
            ligthmapRes.combine = src.ligthmapRes.combine;
            ligthmapRes.colorCombineShadowMask = src.ligthmapRes.colorCombineShadowMask;
        }

        public void SetLightmapData(int lightMapIndex, Vector4 uvst)
        {
            this.lightMapIndex = lightMapIndex;
            lightmapUVST = uvst;
        }

        public void SetLightmapRes(Texture2D color, Texture2D shadowMask, Texture2D dir, Texture2D colorCombineShadowMask)
        {
            ligthmapRes.color = color;
            ligthmapRes.shadowMask = shadowMask;
            ligthmapRes.combine = color;
            ligthmapRes.colorCombineShadowMask = colorCombineShadowMask;
        }

        public void BindLightMap(LigthmapRes[] res, Renderer render, int nAddIndex)
        {
            if (lightMapIndex >= 0 && lightMapIndex < res.Length)
            {
                LigthmapRes resItem = res[lightMapIndex];
                ligthmapRes.combine = resItem.combine;
                if (render == null) return;

                render.lightmapIndex = lightMapIndex + nAddIndex;
                render.realtimeLightmapIndex = lightMapIndex + nAddIndex;
                render.lightmapScaleOffset = lightmapUVST;
                render.realtimeLightmapScaleOffset = lightmapUVST;
            }
        }

        public void BindLightMap(int volumnIndex)
        {
            lightMapVolumnIndex = volumnIndex;
        }
    }

    public interface ILightmapObject
    {
        float LightmapScale { get; set; }
        int LightmapVolumnIndex { get; set; }
        Renderer GetRenderer();
        void ClearLightmap();
        void BeginBake();
        void EndBake();
        void SetLightmapData(int lightMapIndex, Vector4 uvst);
        void SetLightmapRes(Texture2D color, Texture2D shadowMask, Texture2D dir, Texture2D colorCombineShadowMask);
        void BindLightMap(LigthmapRes[] res, int nAddIndex);
        void BindLightMap(int volumnIndex);
    }

    [ExecuteInEditMode]
    public class LightmapVolumn : MonoBehaviour
    {
        public LigthmapRes[] res;
        public LigthmapRenderData[] renders;
        public LightProbes probes;
        public string volumnName;
        public string configName;
        public int dataIndex = -1;
        public int chunkIndex = -1;
        
        public bool bALoadBySceneManager = false;

        private static bool hasExcuteShowLightMap = false;

        public static void ResetState(bool b)
        {
            hasExcuteShowLightMap = b;
        }

        private void Awake()
        {
            if(EngineUtility.IsBuildingGame) return;
            
            if (!bALoadBySceneManager)
                LoadRenderLightmapsOnce();
        }

        public static void LoadRenderLightmapsOnce(bool isNeedRemove0 = false)
        {
            if (!hasExcuteShowLightMap)
            {
                hasExcuteShowLightMap = true;
                LoadRenderLightmaps(isNeedRemove0);
            }
        }

        private void OnEnable()
        {
            LightmapManager.RegisterLightmapVolumn(this);
        }

        private void OnDisable()
        {
            LightmapManager.UnRegisterLightmapVolumn(this);
        }


        public void SetLightProbes()
        {
            if (probes != null && probes.positions.Length > 0)
            {
                LightmapSettings.lightProbes = probes;
            }

#if UNITY_EDITOR
            Debug.LogWarning("LightmapVolumn.SetLightProbes seccess:" + probes);
#endif
        }


        private void OnDestroy()
        {
            hasExcuteShowLightMap = false;
            
#if UNITY_EDITOR
            sharedMaterialDic.Clear();
#endif
        }

        [ContextMenu("LoadRenderLightmapsBySelf")]
        public void LoadRenderLightmapsBySelf()
        {
            LoadRenderLightmaps();
        }


        [ContextMenu("NextSceneRefreshLightMapTrue")]
        public void NextSceneTrue()
        {
            ResetState(false);
        }

        public static void LoadRenderLightmaps(bool isNeedRemove0 = false)
        {
            GameObject obj = GameObject.Find("EditorScene");
            if (obj != null)
            {
                Transform editorSceneTf = obj.transform;
                RenderLightmaps(editorSceneTf.gameObject.GetComponentsInChildren<LightmapVolumn>(true), isNeedRemove0);
            }
        }

#if UNITY_EDITOR
        private static Dictionary<Material, Material> sharedMaterialDic = new Dictionary<Material, Material>();
#endif

        public static void TryCheckLightmapRenderCorrect(LightmapVolumn[] childVolumns)
        {

            Dictionary<Renderer, LightmapVolumn> allRenders = new Dictionary<Renderer, LightmapVolumn>();
            
            for (int i = 0; i < childVolumns.Length; i++)
            {
                LightmapVolumn lightmapVolumn = childVolumns[i];
                
                if(lightmapVolumn.renders==null) continue;
                
                for (int j = 0; j < lightmapVolumn.renders.Length; ++j)
                {
                    LigthmapRenderData lightData = lightmapVolumn.renders[j];
                    if (lightData == null || lightData.render == null)
                    {
#if UNITY_EDITOR
                        Debug.LogWarning("TryCheckLightmapRenderCorrect null:" + lightmapVolumn, lightmapVolumn);
#endif
                        continue;
                    }

                    if (allRenders.ContainsKey(lightData.render))
                    {
#if UNITY_EDITOR
                        Debug.LogWarning("存在同一个render绑定在了不同的LightmapVolumn里面 :" + lightData.render, lightData.render);
                        Debug.LogWarning("当前的 :" + lightmapVolumn, lightmapVolumn);
                        Debug.LogWarning("上一个: :" + allRenders[lightData.render], allRenders[lightData.render]);
#endif
                    }
                    else
                    {
                        allRenders.Add(lightData.render, lightmapVolumn);
                    }
                }
            }
        }

        public static void RenderLightmaps(LightmapVolumn[] childVolumns, bool isNeedRemove0 = false)
        {

#if UNITY_EDITOR
            Debug.LogWarning("RenderLightmaps:" + childVolumns.Length);
#endif

            List<LightmapData> allLightMaps = new List<LightmapData>();

            bool bIsLastBakeFind = false;

            for (int i = 0; i < childVolumns.Length; ++i)
            {
                LightmapVolumn lightmapVolumn = childVolumns[i];

                if (lightmapVolumn.renders.Length == 0) continue;

                List<LightmapData> tmpList = TrysetLightmapSettings(lightmapVolumn);
                if (tmpList.Count == 0) continue;

                if (!Application.isPlaying)
                    Debug.Log("LightmapVolumn:" + lightmapVolumn.renders.Length, lightmapVolumn.gameObject);
                if (isNeedRemove0 && !bIsLastBakeFind && HasRenderLightmaps(lightmapVolumn))
                {
                    allLightMaps.AddRange(tmpList);
                    bIsLastBakeFind = true;
                    continue;
                }

                if (!Application.isPlaying)
                    Debug.Log("LightmapVolumn TrySetRenderLightmaps:" + lightmapVolumn.renders.Length,
                        lightmapVolumn.gameObject);

                TrySetRenderLightmaps(lightmapVolumn, allLightMaps);
                allLightMaps.AddRange(tmpList);

                //lightmapVolumn.SetLightProbes();
            }

            if (allLightMaps.Count > 0)
            {
                LightmapSettings.lightmaps = allLightMaps.ToArray();
            }

#if UNITY_EDITOR
            Debug.LogWarning("lightmaps:" + LightmapSettings.lightmaps.Length);
#endif
        }

        private static List<LightmapData> TrysetLightmapSettings(LightmapVolumn lightmapVolumn)
        {
            List<LightmapData> listTmp = new List<LightmapData>();
            
            if (lightmapVolumn.res == null) return listTmp;

            foreach (LigthmapRes lightData in lightmapVolumn.res)
            {
                if (lightData == null || (lightData.color == null && lightData.colorCombineShadowMask == null))
                {
                    listTmp.Clear();
                    
#if UNITY_EDITOR
                    Debug.LogWarning("TrysetLightmapSettings data null:  " + lightmapVolumn.gameObject,
                        lightmapVolumn.gameObject);
#endif
                    break;
                }
                
                LightmapData newItemData = new LightmapData();

                if (lightData.colorCombineShadowMask != null && LightmapCombineManager.Instance.CheckIsUseCombineLightmap())
                {
                    newItemData.lightmapColor = lightData.colorCombineShadowMask;
                }
                else
                {
                    newItemData.lightmapColor = lightData.color;
                }
                newItemData.lightmapDir = lightData.dir;
                newItemData.shadowMask = lightData.shadowMask;
                
                listTmp.Add(newItemData);
            }
            return listTmp;
        }

        private static bool HasRenderLightmaps(LightmapVolumn lightmapVolumn)
        {
            if (lightmapVolumn.renders == null) return false;
            for (int j = 0; j < lightmapVolumn.renders.Length; j++)
            {
                LigthmapRenderData lightData = lightmapVolumn.renders[j];
                if (lightData == null || lightData.render == null) continue;

                if (lightData.render.lightmapIndex < 10000 && lightData.render.lightmapIndex >= 0) return true;
            }

            return false;
        }


        private static string UberName = "URP/Scene/Uber";
        private static Shader UberShader = null;
        private static void InitUberShader()
        {
            if (UberShader == null)
            {
                UberShader=Shader.Find(UberName);
            }
        }

        private static void TrySetRenderLightmaps(LightmapVolumn lightmapVolumn, List<LightmapData> allLightMaps)
        {
            if (lightmapVolumn.renders == null) return;

           // InitUberShader();
            
            for (int j = 0; j < lightmapVolumn.renders.Length; j++)
            {
                LigthmapRenderData lightData = lightmapVolumn.renders[j];
                if (lightData == null || lightData.render == null) continue;

                if (lightData.lightmapIndex > 10000) continue;

                lightData.render.lightmapIndex = lightData.lightmapIndex + allLightMaps.Count;
                lightData.render.lightmapScaleOffset = lightData.lightmapScaleOffset;
                lightData.render.realtimeLightmapIndex = lightData.realtimeLightmapIndex;
                lightData.render.realtimeLightmapScaleOffset = lightData.realtimeLightmapScaleOffset;

                if (Application.isPlaying && lightData.render.sharedMaterial != null)
                {
                    if (lightData.render.shadowCastingMode != ShadowCastingMode.ShadowsOnly)
                    {
                        lightData.render.shadowCastingMode = ShadowCastingMode.Off;
                    }

                    Material mat = lightData.render.sharedMaterial;
#if UNITY_EDITOR
                    if (!sharedMaterialDic.ContainsKey(mat))
                    {
                        Material matNew = new Material(mat);
                        sharedMaterialDic.Add(mat, matNew);
                        mat = matNew;
                    }
                    else
                    {
                        mat = sharedMaterialDic[mat];
                    }

                    lightData.render.sharedMaterial = mat;
#endif

         
//#if !UNITY_EDITOR
                    // if (UberShader != null && mat != null && mat.shader != null && mat.shader.name.Equals(UberName))
                    // {
                    //     int nRenderQueue = mat.renderQueue;
                    //     mat.shader = UberShader;
                    //     mat.renderQueue = nRenderQueue;
                    // }
//#endif


                    //mat.EnableKeyword("SHADOWS_SHADOWMASK");
                    //mat.EnableKeyword("LIGHTMAP_ON");
                    //mat.EnableKeyword("_FORCE_SHADOWMASK");
                }
            }
        }


        public static string CalcLightmapHash(string name, Vector3 pos, Quaternion rot, Vector3 scale)
        {
            return string.Format(
                "{0}_{1}_P_{2:F2}.{3:F2}.{4:F2}_R{5:F2}.{6:F2}.{7:F2}_S{8:F2}.{9:F2}.{10:F2}",
                name,
                pos.x, pos.y, pos.z,
                rot.x, rot.y, rot.z, rot.w,
                scale.x, scale.y, scale.z);
        }

    }
}
//#endif