#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngineEditor = UnityEditor.Editor;
namespace CFEngine
{
    [System.Serializable]
    public class TerrainObjData
    {
        public bool isValid = true;
        public Texture2D blendTex;
        public Texture2D[] splatTex = new Texture2D[4];
        public Texture2D[] pbsTex = new Texture2D[4];
        public Vector4 blendTexScale;
        public int splatCount = 0;
        public LigthmapComponent lightmapComponent = new LigthmapComponent ();

        public void Copy (TerrainObjData src)
        {
            isValid = src.isValid;
            blendTexScale = src.blendTexScale;
            blendTex = src.blendTex;
            splatCount = src.splatCount;
            for (int i = 0; i < src.splatCount; ++i)
            {
                splatTex[i] = src.splatTex[i];
                pbsTex[i] = src.pbsTex[i];
            }
            lightmapComponent.Copy (src.lightmapComponent);
        }
    }

    [DisallowMultipleComponent, ExecuteInEditMode]
    public class TerrainObject : MonoBehaviour, ILightmapObject, IMatObject
    {
        public TerrainObjData terrainObjData = new TerrainObjData ();
        public Renderer render;
        public int chunkID;
        private Vector4 renderFeature = Vector4.zero;

        [System.NonSerialized]
        public bool preview = false;

        #region terrainHeight
        [System.NonSerialized]
        public ComputeBuffer dataBuffer;
        [System.NonSerialized]
        public float[] heights;
        public static readonly int _GridSize = Shader.PropertyToID ("_GridSize");
        public static readonly int vertexHeight = Shader.PropertyToID ("vertexHeight");
        #endregion
        private Material mat = null;
        private MaterialPropertyBlock mpb = null;
        [System.NonSerialized]
        private MeshFilter mf;
        [System.NonSerialized]
        public static Vector4 globalPbsParam = new Vector4 (1, 0.1f, 1, 0);
        #region lightmap

        public float LightmapScale { get { return terrainObjData.lightmapComponent.lightMapScale; } set { terrainObjData.lightmapComponent.lightMapScale = value; } }
        public Texture2D CombinedTex { get { return terrainObjData.lightmapComponent.ligthmapRes.combine; } set { terrainObjData.lightmapComponent.ligthmapRes.combine = value; } }

        public int LightmapVolumnIndex { get { return terrainObjData.lightmapComponent.lightMapVolumnIndex; } set { terrainObjData.lightmapComponent.lightMapVolumnIndex = value; } }

        public void ClearLightmap ()
        {
            terrainObjData.lightmapComponent.Clear ();
        }

        public void BeginBake ()
        {
            GetRenderer ();
            if (render != null)
            {
                Material mat = render.sharedMaterial;

                if (mat != null && mat != AssetsConfig.instance.TerrainBake)
                {
                    mat = AssetsConfig.instance.TerrainBake;
                    if (terrainObjData.blendTex != null)
                    {
                        mat.SetTexture ("_BlendTex", terrainObjData.blendTex);
                    }
                    else
                    {
                        mat.SetTexture ("_BlendTex", Texture2D.whiteTexture);
                    }
                    for (int i = 0; i < terrainObjData.splatCount; ++i)
                    {
                        if (terrainObjData.splatTex[i] != null)
                        {
                            mat.SetTexture ("_MainTex" + i.ToString (), terrainObjData.splatTex[i]);
                        }
                    }
                    mat.SetVector ("_Scale", terrainObjData.blendTexScale);
                    // if (terrainObjData.splatCount > 1 && terrainObjData.blendTex != null)
                    // {
                    //     mat.SetTexture (ShaderManager._ShaderKeyBlendTex, terrainObjData.blendTex);
                    // }
                    // int texCount = 0;
                    // for (int i = 0; i < terrainObjData.splatCount; ++i)
                    // {
                    //     if (terrainObjData.splatTex[i] != null)
                    //     {
                    //         texCount++;
                    //         mat.SetTexture (ShaderManager._ShaderKeySplatTex[i], terrainObjData.splatTex[i]);

                    //     }
                    //     if (terrainObjData.pbsTex[i] != null)
                    //     {
                    //         mat.SetTexture (ShaderManager._ShaderKeyTerrainPbsTex[i], terrainObjData.pbsTex[i]);

                    //     }
                    // }

                    // mat.SetVector (ShaderManager._ShaderKeyTerrainScale, terrainObjData.blendTexScale);
                    // mat.SetVector (ShaderManager._ShaderKeyTerrainEffect, globalPbsParam);
                    // for (int i = 0; i < terrainKeywords.Length; ++i)
                    // {
                    //     if ((terrainObjData.splatCount - 1) == i)
                    //     {
                    //         mat.EnableKeyword (terrainKeywords[i]);
                    //     }
                    //     else
                    //     {
                    //         mat.DisableKeyword (terrainKeywords[i]);
                    //     }
                    // }

                    render.SetPropertyBlock (null);
                    render.sharedMaterial = mat;
                }
            }
        }
        public void EndBake ()
        {
            GetRenderer ();
            if (render != null)
            {
                Refresh (RenderingManager.instance);
            }
        }

        public void SetLightmapData (int lightMapIndex, Vector4 uvst)
        {
            terrainObjData.lightmapComponent.SetLightmapData (lightMapIndex, uvst);
        }
        public void SetLightmapRes (Texture2D color, Texture2D shadowMask, Texture2D dir, Texture2D colorCombineShadowMask)
        {
            terrainObjData.lightmapComponent.SetLightmapRes (color, shadowMask, dir, colorCombineShadowMask);
        }
        public void BindLightMap (LigthmapRes[] res, int nAddIndex)
        {
            terrainObjData.lightmapComponent.BindLightMap (res,render,nAddIndex);
        }
        public void BindLightMap (int volumnIndex)
        {
            terrainObjData.lightmapComponent.BindLightMap (volumnIndex);
        }
        #endregion
        public Renderer GetRenderer ()
        {
            if (render == null)
            {
                render = GetComponent<Renderer> ();
            }
            return render;
        }

        private MeshFilter GetMeshFilter ()
        {
            if (mf == null)
            {
                mf = gameObject.GetComponent<MeshFilter> ();
            }
            return mf;
        }
        public Mesh GetMesh ()
        {
            GetMeshFilter ();
            return mf != null?mf.sharedMesh : null;
        }
        void SetMaterial (Material mat)
        {
            if (GetRenderer ())
            {
                render.sharedMaterial = mat;
            }
            this.mat = mat;
        }

        public void Refresh (RenderingManager mgr)
        {
            if (GetRenderer ())
            {
                if (mpb == null)
                {
                    mpb = mgr.GetMpb (render);
                }
                if (mpb != null)
                {
                    mpb.Clear ();
                    if (terrainObjData.splatCount > 1 && terrainObjData.blendTex != null)
                    {
                        mpb.SetTexture (ShaderManager._ShaderKeyBlendTex, terrainObjData.blendTex);
                    }
                    int texCount = 0;
                    for (int i = 0; i < terrainObjData.splatCount; ++i)
                    {
                        if (terrainObjData.splatTex[i] != null)
                        {
                            texCount++;
                            mpb.SetTexture (ShaderManager._ShaderKeySplatTex[i], terrainObjData.splatTex[i]);

                        }
                        if (terrainObjData.pbsTex[i] != null)
                        {
                            mpb.SetTexture (ShaderManager._ShaderKeyTerrainPbsTex[i], terrainObjData.pbsTex[i]);

                        }
                    }

                    if (texCount > 0)
                    {
                        SetMaterial (AssetsConfig.instance.TerrainPreviewMat[texCount - 1]);
                    }

                    mpb.SetVector (ShaderManager._ShaderKeyTerrainScale, terrainObjData.blendTexScale);
                    mpb.SetVector (ShaderManager._ShaderKeyTerrainEffect, globalPbsParam);
                    bool lightmap = RuntimeUtilities.BindLightmap (mpb, terrainObjData.lightmapComponent);
                    renderFeature.y = lightmap?1 : 0;
                    renderFeature.z = 1;
                    mpb.SetVector (ShaderManager._Param, renderFeature);
                    render.SetPropertyBlock (mpb);
                    if (mat != null)
                    {
                        EnvironmentExtra.EnableEditorMat (mat);
                    }
                }
            }
        }
        public void OnDrawGizmo(EngineContext context)
        {

        }
        public void SetAreaMask(uint area)
        {
        }
        public void Copy (TerrainObjData tod)
        {
            terrainObjData.Copy (tod);
            Refresh (RenderingManager.instance);
        }

        void Update ()
        {
            if (preview && dataBuffer != null)
            {
                var terrainHeight = AssetsConfig.instance.PreviewTerrainQuad;
                if (terrainHeight != null && GetRenderer ())
                {
                    Transform t = transform;
                    mpb.SetBuffer (vertexHeight, dataBuffer);
                    float gridSizeInv = 1.0f / EngineContext.terrainGridCount;
                    mpb.SetVector (_GridSize, new Vector4 (t.position.x, t.position.z, gridSizeInv, gridSizeInv));
                    Graphics.DrawProcedural (terrainHeight,
                        render.bounds,
                        MeshTopology.Quads,
                        4,
                        64 * 64,
                        null,
                        mpb);
                }

            }
        }
    }

    [CanEditMultipleObjects, CustomEditor (typeof (TerrainObject))]
    public class TerrainObjectEditor : UnityEngineEditor
    {
        SerializedProperty isValid;
        SerializedProperty lightmapScale;

        private void OnEnable ()
        {
            isValid = serializedObject.FindProperty ("terrainObjData.isValid");
            lightmapScale = serializedObject.FindProperty ("terrainObjData.lightmapComponent.lightMapScale");
        }

        public override void OnInspectorGUI ()
        {
            serializedObject.Update ();
            TerrainObject to = target as TerrainObject;
            EditorGUILayout.PropertyField (isValid);
            RuntimeUtilities.OnLightmapInspectorGUI (to.terrainObjData.lightmapComponent, lightmapScale);
            serializedObject.ApplyModifiedProperties ();
        }
    }

}

#endif