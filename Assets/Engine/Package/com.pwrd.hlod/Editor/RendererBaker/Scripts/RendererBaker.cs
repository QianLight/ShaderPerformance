using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Athena.MeshSimplify;
using UnityEditor;
using UnityEngine;
#if HLOD_USE_HDRP
using UnityEngine.Rendering.HighDefinition;
#endif
#if HLOD_USE_URP
using UnityEngine.Rendering.Universal;
#endif
using Object = UnityEngine.Object;

namespace com.pwrd.hlod.editor
{
    public enum BakeRTSize
    {
        _256 = 256,
        _512 = 512,
        _1024 = 1024,
        _2048 = 2048,
        _4096 = 4096
    }
    
    public class RendererBaker
    {
        protected Camera m_cam;
        protected Renderer m_renderer;
        protected MeshFilter m_filter;
        protected int m_layerMask = 22; //默认设置较高层级，防止重复
        protected RenderTexture m_rt;
        protected int m_RTSize = 2048;
        protected int m_useLODIndex;
        protected bool m_useHighRendererLightmap;
        protected ShaderBindConfig shaderBindConfig;
        protected List<HLODDecalTag> m_decalData = new List<HLODDecalTag>();


        #region Out Interface

        public bool isDebug { get; set; }

        public void Init(Renderer renderer)
        {
            InitBaker(renderer);
        }
        
        public Texture2D Bake(Renderer renderer, TextureChannel channel, bool useAlphaTest = false)
        {
            if (channel == TextureChannel.Albedo)
                return BakeDiffuse(renderer, useAlphaTest);
            return BakeTexture(renderer, channel);
        }
        
        public void Release()
        {
            if (m_cam != null)
            {
                GameObject.DestroyImmediate(m_cam.gameObject);
            }

            if (m_rt != null)
            {
                RenderTexture.ReleaseTemporary(m_rt);
            }

            if (m_renderer != null)
            {
                GameObject.DestroyImmediate(m_renderer.gameObject);
            }
        }
        
        public Texture2D BakeDiffuse(Renderer renderer, bool useAlphaTest)
        {
            //烘焙漫反射
            var tex = BakeTexture(renderer, TextureChannel.Albedo);

            //烘焙贴花
            var newTex = BakeDecal(tex);
            
            if (isDebug)
            {
                return tex;
            }
            TryReleaseOldTex(tex, newTex);
            tex = newTex;

            if (!useAlphaTest)
            {
                //填充透明区域颜色
                newTex = FillTransparentPixel(tex);
                TryReleaseOldTex(tex, newTex);
                tex = newTex;
            }

            return tex;
        }
        
        public void SetUseHighRendererLightmap(bool useHighRendererLightmap)
        {
            m_useHighRendererLightmap = useHighRendererLightmap;
        }
        
        public void SetUseLODIndex(int useLODIndex)
        {
            m_useLODIndex = useLODIndex;
        }
        
        public void SetDecalData(List<HLODDecalTag> list)
        {
            m_decalData = list;
        }

        public void SetShaderBindConfigData(ShaderBindConfig shaderBindConfig)
        {
            this.shaderBindConfig = shaderBindConfig;
        }

        public void SetRendererBakerSetting(RendererBakerSetting setting)
        {
            var unusedLayer = GetUnusedLayer();
            if (setting.useCustomBakeLayerMask || unusedLayer == -1)
            {
                unusedLayer = setting.bakeLayerMask;
            }
            m_layerMask = unusedLayer;
            m_RTSize = (int)setting.bakeRTSize;
        }

        #endregion

        #region Inner Method
        
        #region Init

        protected void InitBaker(Renderer renderer)
        {
            //HLODDebug.Log("[RendererBaker] Init start");

            var transform = renderer.transform;
            Camera cam = null;
            {
                var camName = "__RENDERER_BAKER_CAM__";
                GameObject camGo = GameObject.Find(camName);
                if (camGo == null)
                {
                    camGo = new GameObject(camName);
                    camGo.hideFlags = HideFlags.DontSave;
                    camGo.AddComponent<Camera>();
#if HLOD_USE_URP
                    camGo.AddComponent<UniversalAdditionalCameraData>();
#endif
#if HLOD_USE_HDRP
                    camGo.AddComponent<HDAdditionalCameraData>();
#endif
                    
                }

                var renderPos = renderer.bounds.center;
                camGo.hideFlags = HideFlags.None;
                cam = camGo.GetComponent<Camera>();
                cam.cullingMask = 1 << m_layerMask;
                cam.clearFlags = CameraClearFlags.SolidColor;
                cam.backgroundColor = Color.clear;
                cam.orthographic = true;
                cam.useOcclusionCulling = false;
#if HLOD_USE_URP
                var camData = camGo.GetComponent<UniversalAdditionalCameraData>();
                camData.renderShadows = true;
                camData.renderPostProcessing = false;
#endif
#if HLOD_USE_HDRP
                var camHDData = camGo.GetComponent<HDAdditionalCameraData>();
                camHDData.volumeLayerMask = 0;
#endif

                var dir = Vector3.up;
                var bounds = renderer.bounds;
                var length = (bounds.max - bounds.min).magnitude;
                cam.orthographicSize = length * 0.5f + 1;
                var h = length * 0.5f + 1;
                cam.transform.position = dir * h + bounds.center;
                cam.nearClipPlane = 0.1f;
                cam.farClipPlane = length + 2;
                cam.transform.LookAt(renderPos);
            }

            //复制一个物体出来
            var go = GameObject.Instantiate(renderer.gameObject);
            go.transform.parent = renderer.transform.parent;
            go.transform.position = renderer.transform.position;
            go.transform.rotation = renderer.transform.rotation;
            go.transform.localScale = renderer.transform.localScale;
            var oldlayerMask = go.layer;
            go.layer = m_layerMask;
            go.hideFlags = HideFlags.None;
            m_renderer = go.GetComponent<Renderer>();
            m_filter = go.GetComponent<MeshFilter>();
            m_cam = cam;
            var size = GetMaxTexSize(renderer);
            if (size.sqrMagnitude < 1)
                size = new Vector2Int(m_RTSize, m_RTSize);
            size = new Vector2Int(m_RTSize, m_RTSize);
            m_rt = RenderTexture.GetTemporary(new RenderTextureDescriptor(size.x, size.y));

            SetLightmap(m_renderer, renderer);
            
            //HLODDebug.Log("[RendererBaker] Init end");
        }

        protected Vector2Int GetMaxTexSize(Renderer renderer)
        {
            var mats = renderer.sharedMaterials;
            int maxX = 0;
            int maxY = 0;
            foreach (var mat in mats)
            {
                var tex = GetMainTexture(mat);
                if (tex == null)
                    continue;

                maxX = Mathf.Max(maxX, tex.width);
                maxY = Mathf.Max(maxY, tex.height);
            }

            return new Vector2Int(maxX, maxY);
        }

        private Texture GetMainTexture(Material mat)
        {
            if (mat == null)
                return null;
            var names = mat.GetTexturePropertyNames();
            foreach (var name in names)
            {
                if (mat.HasProperty(name))
                {
                    var tex = mat.GetTexture(name);
                    if (string.IsNullOrEmpty(AssetDatabase.GetAssetPath(tex)))
                        continue;
                    return tex;
                }
            }

            return null;
        }
        #endregion

        #region Release

        private void TryReleaseOldTex(Texture2D oldTex, Texture2D newTex)
        {
            if (isDebug)
            {
                return;
            }

            if (oldTex == newTex)
                return;

            TryDestoryTex(ref oldTex);
        }

        private void TryDestoryTex(ref Texture2D tex)
        {
            if(isDebug)
                return;
            
            if (tex != null)
            {
                GameObject.DestroyImmediate(tex);
                tex = null;
            }
        }

        private void DestoryRT(ref RenderTexture rt)
        {
            if(isDebug)
                return;
            if (rt != null)
            {
                RenderTexture.ReleaseTemporary(rt);
                rt = null;
            }
        }

        private void DestroyTexture(ref Texture2D cacheTexture)
        {
            if(isDebug)
                return;
            if (cacheTexture)
            {
                UnityEngine.Object.DestroyImmediate(cacheTexture);
                cacheTexture = null;
            }
        }
        #endregion

        #region Bake Diffuse
        private Texture2D BakeTexture(Renderer renderer, TextureChannel channel)
        {
            var go = m_renderer.gameObject;

            var targetRender = go.GetComponent<Renderer>();
            var materials = targetRender.sharedMaterials;
            var newMats = new Material[materials.Length];
            for (int i = 0; i < materials.Length; i++)
            {
                var mat = GetBakeMaterial(materials[i], renderer, channel);
                newMats[i] = mat;
            }

            targetRender.sharedMaterials = newMats;
            var cam = m_cam;
            var dst = m_rt;
            cam.targetTexture = dst;
            cam.Render();

            RenderTexture.active = dst;
            var dstTex = new Texture2D(dst.width, dst.height);
            dstTex.ReadPixels(new Rect(0, 0, dst.width, dst.height), 0, 0, false);
            dstTex.Apply();
            cam.targetTexture = null;

            if (!isDebug)
            {
                targetRender.sharedMaterials = materials;
            }

            return dstTex;
        }

        protected Material GetBakeMaterial(Material mat, Renderer renderer, TextureChannel channel)
        {
            if (mat == null)
                return mat;
            mat = NewMat(mat, GetBindShader(mat.shader));
            mat.EnableKeyword("_RENDERER_BAKE_");
            //Shader.EnableKeyword("_ISTERRAINLAYER");
            //Shader.EnableKeyword("_RENDERER_BAKE_ORIGIN_COLOR");

             mat.DisableKeyword("_ENABLE_HLOD_BAKE_ALBEDO");
             mat.DisableKeyword("_ENABLE_HLOD_BAKE_NORMAL");
             mat.DisableKeyword("_ENABLE_HLOD_BAKE_METALLIC");
             mat.DisableKeyword("_ENABLE_HLOD_BAKE_OCCLUSION");
             mat.DisableKeyword("_ENABLE_HLOD_BAKE_LIGHTMAP");
             mat.EnableKeyword("_ENABLE_HLOD_BAKE_" + channel.ToString().ToUpper());

            mat.SetFloat("_Cull", 0);
            return mat;
        }

        private Shader GetBindShader(Shader originShader)
        {
            if (shaderBindConfig != null)
            {
                if (shaderBindConfig.bakeShaderList != null && shaderBindConfig.bakeShaderList.Count > 0)
                {
                    foreach (var tuple in shaderBindConfig.bakeShaderList)
                    {
                        if (tuple != null && tuple.originShader == originShader && tuple.bakeShader)
                        {
                            return tuple.bakeShader;
                        }
                    }
                }
                
                if (shaderBindConfig.defaultBakeShader != null)
                {
                    return shaderBindConfig.defaultBakeShader;
                }
            }
            return Shader.Find("Hidden/RendererBake/Standard");
        }

       
        protected Material NewMat(Material mat, Shader shader)
        {
       
            var tmp = new Material(mat);
            tmp.shader = shader;
            var newMat = new Material(shader);
            newMat.name = mat.name;
            newMat.CopyPropertiesFromMaterial(tmp);
            Shader.DisableKeyword("_ISTERRAINLAYER");
            if (mat.HasProperty("_BumpMap") && !mat.HasProperty("_NormalMap"))
            {
                newMat.SetTexture("_NormalMap", mat.GetTexture("_BumpMap"));
            }
            if (mat.HasProperty("_MainTex") && !mat.HasProperty("_BaseMap"))
            {
                newMat.SetTexture("_BaseMap", mat.GetTexture("_MainTex"));
            }
            
            if (mat.HasProperty("_BlendTex") )
            {
                newMat.SetTexture("_BlendTex", mat.GetTexture("_BlendTex"));
                newMat.SetTexture("_Layer0Tex", mat.GetTexture("_MainTex"));
                newMat.SetTexture("_Layer1Tex", mat.GetTexture("_MainTex1"));
                newMat.SetTexture("_Layer2Tex", mat.GetTexture("_MainTex2"));
                newMat.SetColor("_Color0", mat.GetColor("_Color0"));
                newMat.SetColor("_Color1", mat.GetColor("_Color1"));
                newMat.SetColor("_Color2", mat.GetColor("_Color2"));
                Shader.EnableKeyword("_ISTERRAINLAYER");
                
            }

            foreach (var keyword in mat.shaderKeywords)
            {
                if (mat.IsKeywordEnabled(keyword))
                {
                    newMat.EnableKeyword(keyword);
                }
            }
            return newMat;
        }

        #endregion

        #region Bake Decal

        private Texture2D BakeDecal(Texture2D bakedTex)
        {
            var list = GetDecalData();

            foreach (var data in list)
            {
                if (!data.bounds.Intersects(m_renderer.bounds))
                    continue;
                var newTex = DoBakeDecal(bakedTex, data);
                
                TryDestoryTex(ref bakedTex);
                bakedTex = newTex;
            }

            return bakedTex;
        }

        private List<DecalData> GetDecalData()
        {
            var list = new List<DecalData>();
            foreach (var decalTag in m_decalData)
            {
                list.Add(decalTag.GetDecalData());
            }

            return list;
        }

        private Texture2D DoBakeDecal(Texture2D bakedTex, DecalData decal)
        {
            //在烘焙好的贴图基础上,每次增加一个贴花
            var targetRender = m_renderer;
            var materials = targetRender.sharedMaterials;
            var newMats = new Material[materials.Length];
            for (int i = 0; i < materials.Length; i++)
            {
                var mat = GetDecalMaterial(bakedTex, decal);
                newMats[i] = mat;
                if (isDebug)
                {
                    StartDebugTask(mat, bakedTex, decal);
                }
            }

            targetRender.materials = newMats;
            SetLightmap(targetRender, decal.renderer);
            var cam = m_cam;
            var dst = m_rt;
            cam.targetTexture = dst;
            cam.Render();

            RenderTexture.active = dst;
            var dstTex = new Texture2D(dst.width, dst.height);
            dstTex.ReadPixels(new Rect(0, 0, dst.width, dst.height), 0, 0, false);
            dstTex.Apply();
            cam.targetTexture = null;

            //targetRender.sharedMaterials = materials;
            return dstTex;
        }

        private Material GetDecalMaterial(Texture baked, DecalData data)
        {
            var mat = new Material(data.renderer.sharedMaterial);
            mat.shader = Shader.Find("Athena/Bake/Decal");
            SetDecalMaterial(mat, baked, data);
            return mat;
        }

        private void SetDecalMaterial(Material mat, Texture baked, DecalData data)
        {
            mat.SetTexture("_BakedTexture", baked);
            mat.SetMatrix("_DecalTrans", data.trans);
            mat.SetTexture("_BaseMap", data.tex);
            mat.SetVector("_BaseMap_ST", data.offsetScale);
            mat.SetVector("_DecalNormal", data.normal);
            //mat.SetVector("_DecalTangent", data.tangent);
            mat.SetVector("_UVArea", data.uvArea);
        }

        #endregion

        #region Set Lightmap

        protected void SetLightmap(Renderer targetRender, Renderer originRenderer)
        {
            var mr = targetRender as MeshRenderer;
            var originMr = originRenderer as MeshRenderer;
            if (m_useLODIndex != 0 && m_useHighRendererLightmap)
            {
                var lodgroup = originRenderer.GetComponentInParent<LODGroup>();
                if (lodgroup)
                {
                    var lods = lodgroup.GetLODs();
                    if (lods.Length > 0)
                    {
                        if (string.IsNullOrWhiteSpace(originRenderer.name) && !originRenderer.name.Contains("_")) return;
                        var splitIndex = originRenderer.name.LastIndexOf("_");
                        if (splitIndex >= 0)
                        {
                            var highMedelName = originRenderer.name.Remove(splitIndex);
                            foreach (var lodRenderer in lods[0].renderers)
                            {
                                if (lodRenderer == null) continue;
                                if (lodRenderer.name.Equals(highMedelName))
                                {
                                    CopyLightmap(lodRenderer as MeshRenderer, targetRender as MeshRenderer);
                                }
                                else
                                {
                                    CopyLightmap(originRenderer as MeshRenderer, targetRender as MeshRenderer);
                                }
                            }
                        }
                    }
                }
            }
            else
            {
                CopyLightmap(originRenderer as MeshRenderer, targetRender as MeshRenderer);
            }
        }

        private void CopyLightmap(MeshRenderer originRenderer, MeshRenderer targetRender)
        {
            if (targetRender && originRenderer)
            {
                targetRender.gameObject.isStatic = true;
                targetRender.receiveGI = ReceiveGI.Lightmaps;
                targetRender.lightmapIndex = originRenderer.lightmapIndex;
                targetRender.lightmapScaleOffset = originRenderer.lightmapScaleOffset;
                targetRender.scaleInLightmap = originRenderer.scaleInLightmap;
                targetRender.stitchLightmapSeams = originRenderer.stitchLightmapSeams;
                targetRender.realtimeLightmapIndex = originRenderer.realtimeLightmapIndex;
                targetRender.realtimeLightmapScaleOffset = originRenderer.realtimeLightmapScaleOffset;
                EditorUtility.SetDirty(targetRender);
            }
        }
        
        private int GetUnusedLayer()
        {
	        int layer = -1;
	        for (int i = 8; i < 32; i++)
	        {
		        var name = LayerMask.LayerToName(i);
		        if (string.IsNullOrWhiteSpace(name))
		        {
			        layer = i;
			        break;
		        }
	        }
	        return layer;
        }

        #endregion

        #region Bake Fill Transparent

        private Texture2D FillTransparentPixel(Texture2D originTex)
        {
            
            int maxSize = 1024;
            //1.缩放原图片,保证不要过大
            var newRT = RenderTexture.GetTemporary(Mathf.Min(originTex.width, maxSize),
                Mathf.Min(originTex.height, maxSize));
            var shader = Shader.Find("Hidden/RendererBake/Blit");
            var unlitMat = new Material(shader);
            unlitMat.SetTexture("_OriginMap", originTex);
            Graphics.Blit(newRT, newRT, unlitMat);
            
            var tex = new Texture2D(newRT.width, newRT.height);
            RenderTexture.active = newRT;
            tex.ReadPixels(new Rect(0, 0, tex.width, tex.height), 0, 0, false);
            
            //2.填充缩小后的图片
            int nWidth = tex.width;
            int nHeight = tex.height;
            Color[] aPixels = new Color[nWidth * nHeight];
            bool[,] map = new bool[nWidth, nHeight];
            List<(int, int)> coloredPointList = new List<(int, int)>();
            for (int x = 0; x < nWidth; x++)
            {
                for (int y = 0; y < nHeight; y++)
                {
                    var index = x + y * nWidth;
                    var color = tex.GetPixel(x, y);
                    if (!IsTransparentPixel(color))
                    {
                        coloredPointList.Add((x, y));
                        map[x, y] = true;
                    }

                    aPixels[index] = tex.GetPixel(x, y);
                }
            }

            DestroyTexture(ref tex);
            
            var xOffset = new int[] {-1, 0, 1, 1, 1, 0, -1, -1};
            var yOffset = new int[] {1, 1, 1, 0, -1, -1, -1, 0};

            var around = new Queue<(int, int)>(nWidth * nHeight);
            foreach (var tuple in coloredPointList)
            {
                for (int i = 0; i < xOffset.Length; i++)
                {
                    int x = tuple.Item1 + xOffset[i];
                    int y = tuple.Item2 + yOffset[i];
                    if (x < 0 || x >= nWidth || y < 0 || y >= nHeight)
                        continue;

                    if (!map[x, y])
                    {
                        around.Enqueue(tuple);
                        break;
                    }
                }
            }

            while (around.Count > 0)
            {
                var tuple = around.Dequeue();
                var color = aPixels[tuple.Item1 + tuple.Item2 * nWidth];
                for (int i = 0; i < xOffset.Length; i++)
                {
                    int x = tuple.Item1 + xOffset[i];
                    int y = tuple.Item2 + yOffset[i];
                    if (x < 0 || x >= nWidth || y < 0 || y >= nHeight)
                        continue;

                    int index = x + y * nWidth;
                    if (!map[x, y])
                    {
                        aPixels[index] = new Color(color.r, color.g, color.b, aPixels[index].a);
                        map[x, y] = true;
                        around.Enqueue((x, y));
                    }
                }
            }

            Texture2D texResult = new Texture2D(nWidth, nHeight);
            texResult.SetPixels(aPixels);
            texResult.wrapMode = TextureWrapMode.Repeat;
            texResult.Apply();
            
            //3.将填充后的图片与原图片混合.
            DestoryRT(ref newRT);
            newRT = RenderTexture.GetTemporary(originTex.width, originTex.height);
            var blendMat = new Material(Shader.Find("Hidden/RendererBake/Blit"));
            blendMat.SetTexture("_OriginMap", originTex);
            blendMat.SetTexture("_MiniMap", texResult);
            blendMat.EnableKeyword("_FILL_");
            
            Graphics.Blit(newRT, newRT, blendMat);
            
            DestroyTexture(ref texResult);

            var newTex = new Texture2D(originTex.width, originTex.height);
            RenderTexture.active = newRT;
            newTex.ReadPixels(new Rect(0, 0, originTex.width, originTex.height), 0, 0, false);
            newTex.Apply();

            TryDestoryTex(ref texResult);
            TryDestoryTex(ref tex);
            DestoryRT(ref newRT);

            return newTex;
        }

        private bool IsTransparentPixel(Color color)
        {
            return Mathf.Approximately(color.a, 0);
        }

        #endregion

        #endregion

        #region Debug

        private void StartDebugTask(Material mat, Texture baked, DecalData data)
        {
            //debug 实时渲染用
            EditorTaskProxy.Instance.StartTask(SetDebugMaterail(mat, baked, data));
        }

        private IEnumerator SetDebugMaterail(Material mat, Texture baked, DecalData data)
        {
            //var go = GameObject.Find("TestQuadForDecalBake");
            var go = GameObject.Instantiate(data.renderer.gameObject, data.renderer.gameObject.transform.parent);
            var tag = go.GetComponent<HLODDecalTag>();
            while (true)
            {
                yield return null;
                tag.CalcDecalData();
                data = tag.GetDecalData();
                SetDecalMaterial(mat, baked, data);
            }
        }
        #endregion
    }
}