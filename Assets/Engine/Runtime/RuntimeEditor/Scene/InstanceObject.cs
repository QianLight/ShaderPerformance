#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
namespace CFEngine
{

    [DisallowMultipleComponent, ExecuteInEditMode]
    public class InstanceObject : MonoBehaviour, IMatObject
    {
        public uint hash = 0;
        public uint areaMask = 0xffffffff;
        public int chunkIndex = -1;
        public Vector4 worldPosOffset;
        public Texture2D lightmap;
        private Vector4 renderFeature = Vector4.zero;
        [System.NonSerialized]
        public Renderer render;
        [System.NonSerialized]
        private MaterialPropertyBlock mpb = null;
        public Renderer GetRenderer ()
        {
            if (render == null)
            {
                render = GetComponent<Renderer> ();
            }
            return render;
        }
        public Material GetMat ()
        {
            GetRenderer ();
            return render != null?render.sharedMaterial : null;
        }
        public void Refresh (RenderingManager mgr)
        {
            if (GetRenderer () != null)
            {
                Material m = GetMat ();
                if (m != null)
                {
                    EnvironmentExtra.EnableEditorMat (m);
                    var context = EngineContext.instance;
                    if (chunkIndex >= 0 && context != null)
                    {
                        if (mpb == null)
                        {
                            mpb = mgr.GetMpb (render);
                        }
                        mpb.Clear ();
                        int x = chunkIndex % context.xChunkCount;
                        int z = chunkIndex / context.xChunkCount;
                        string path = string.Format ("{0}/{1}",
                            AssetsConfig.EditorGoPath[0],
                            AssetsConfig.EditorGoPath[(int) EditorSceneObjectType.MeshTerrain]);
                        GameObject go = GameObject.Find (path);
                        if (go != null)
                        {
                            var t = go.transform;
                            var chunk = t.Find (string.Format ("Chunk_{0}_{1}", x, z));
                            if (chunk != null)
                            {
                                TerrainObject to;
                                if (chunk.TryGetComponent (out to))
                                {
                                    if (to.terrainObjData.lightmapComponent.ligthmapRes.colorCombineShadowMask && LightmapCombineManager.Instance.CheckIsUseCombineLightmap())
                                    {
                                        lightmap = to.terrainObjData.lightmapComponent.ligthmapRes.colorCombineShadowMask;
                                    }
                                    else
                                    {
                                        lightmap = to.terrainObjData.lightmapComponent.ligthmapRes.color;
                                    }
                                    
                                    mpb.SetVector (ShaderManager._ChunkOffset, worldPosOffset);
                                    if (lightmap != null)
                                        mpb.SetTexture (ShaderManager._CustomLightmap, lightmap);
                                    renderFeature.y = lightmap != null?1 : 0;
                                    renderFeature.z = 1;
                                    mpb.SetVector (ShaderManager._Param, renderFeature);

                                    render.SetPropertyBlock (mpb);
                                }
                            }
                        }
                    }

                }
            }
        }

        //public void Update()
        //{
        //    RenderingManager mgr = RenderingManager.instance;
        //    if(mgr != null)
        //    {
        //        Refresh(mgr);
        //    }
        //}
        public void OnDrawGizmo(EngineContext context)
        {

        }

        public void SetAreaMask(uint area)
        {
            areaMask = area;
        }
    }
}
#endif