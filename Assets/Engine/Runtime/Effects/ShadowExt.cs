using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Rendering;
#if UNITY_EDITOR
using UnityEditor;
#endif
namespace CFEngine
{
    public enum ShadowMode
    {
        OnlyPlayerShadow,
        HardShadow,
        SoftShaow,
    }

    [Serializable]
    public sealed class ShadowModeParam : ParamOverride<ShadowMode, ShadowModeParam> { }

#if UNITY_EDITOR

    [Serializable]
    public class SceneObjectGroup
    {
        [NonSerialized]
        public Transform t;
        public string path;
        public bool enable = true;
        public void OnEnable ()
        {
            if (!string.IsNullOrEmpty (path))
            {
                GameObject go = GameObject.Find (path);
                t = go != null?go.transform : null;
            }
        }
    }

    [Serializable]
    public sealed class SceneObjectGroups
    {
        public List<SceneObjectGroup> groups = new List<SceneObjectGroup> ();
        [NonSerialized]
        public bool refresh = false;
    }

    //[Serializable]
    //public sealed class SceneObjectGroupsParam : ParamOverride<SceneObjectGroups, SceneObjectGroupsParam>, ICustomDraw
    //{
    //    public void OnEnable ()
    //    {
    //        var groups = value.groups;
    //        for (int i = 0; i < groups.Count; ++i)
    //        {
    //            groups[i].OnEnable ();
    //        }
    //    }

    //    public void OnDrawInspector ()
    //    {
    //        var groups = value.groups;
    //        GUILayout.BeginHorizontal ();
    //        EditorGUILayout.BeginVertical (GUI.skin.box,
    //            GUILayout.MinWidth (0),
    //            GUILayout.MinHeight (0),
    //            GUILayout.MaxWidth (1000),
    //            GUILayout.MaxHeight (groups.Count * 20 + 40));

    //        EditorGUILayout.BeginHorizontal ();
    //        if (GUILayout.Button ("AddQuadTreeNode"))
    //        {
    //            groups.Add (new SceneObjectGroup ());
    //        }
    //        if (GUILayout.Button ("RefreshQuadTreeNode"))
    //        {
    //            value.refresh = true;
    //        }
    //        EditorGUILayout.EndHorizontal ();
    //        int removeIndex = -1;
    //        for (int i = 0; i < groups.Count; ++i)
    //        {
    //            EditorGUILayout.BeginHorizontal ();
    //            var group = groups[i];
    //            Transform t = EditorGUILayout.ObjectField (group.t, typeof (Transform), true) as Transform;
    //            if (group.t != t)
    //            {
    //                group.t = t;

    //                string path = EditorCommon.GetSceneObjectPath (t);
    //                group.path = path;
    //                group.t = t;
    //            }
    //            group.enable = EditorGUILayout.Toggle ("Enable", group.enable);
    //            if (GUILayout.Button ("Delete"))
    //            {
    //                removeIndex = i;
    //            }
    //            EditorGUILayout.EndHorizontal ();
    //        }
    //        EditorGUILayout.EndVertical ();
    //        GUILayout.EndHorizontal ();
    //        if (removeIndex >= 0)
    //        {
    //            groups.RemoveAt (removeIndex);
    //        }
    //    }
    //}

    public interface IOctTreeData
    {
        Renderer GetRenderer ();
    }

    public class SceneOctTreeData : IOctTreeData, ISharedObject
    {
        public Renderer r;
        public Mesh mesh;
        public AABB objectBoundWS;
        public uint cullCounter;
        public MeshRenderObject mro;
        public bool valid = false;
        public Renderer GetRenderer ()
        {
            return r;
        }
        public void Reset ()
        {
            r = null;
            mesh = null;
            cullCounter = 0;
            mro = null;
            valid = false;
        }
    }

    public struct ShadowCullContext
    {
        public Vector3 lightDir;
        public float invSin;
        public RenderContext rc;
        public uint cullCounter;
        public HashSet<int> shadowObjPool;
        public Dictionary<Mesh, List<MeshRenderObject>> singleShadowMesh;

        public static Material GetShadowCasterMat (Renderer r, MaterialPropertyBlock mpb)
        {
            Material mat = r.sharedMaterial;
            Material shadowCastMat = AssetsConfig.instance.ShadowCaster;
            if (mat != null)
            {
                if (mat.IsKeywordEnabled ("_ALPHA_TEST") || mat.IsKeywordEnabled ("_ALPHA_BLEND"))
                {
                    shadowCastMat = AssetsConfig.instance.ShadowCasterCutout;
                    if (mat.HasProperty (ShaderIDs.MainTex))
                    {
                        Texture tex = mat.GetTexture (ShaderIDs.MainTex);
                        if (tex != null)
                        {
                            mpb.SetTexture (ShaderIDs.MainTex, tex);
                        }

                    }

                }
            }
            return shadowCastMat;
        }
    }

    public class OctTreeNode : ISharedObject
    {
        public static uint Visible = 0x00000001;
        public AABB quadBound;
        public AABB realBound;
        public OctTreeNode[] nodes = new OctTreeNode[8];
        public List<IOctTreeData> data = null;
        private int dataCount = 0;
        private int dataCap = 0;
        public float minNodeSize = 8 - 0.5f;
        public uint flag = 0;
        public void Reset ()
        {

        }
        public void SetFlag (uint f, bool add)
        {
            if (add)
            {
                flag |= f;
            }
            else
            {
                flag &= ~(f);
            }
        }

        public bool HasFlag (uint f)
        {
            return (flag & f) != 0;
        }

        public void Init (float width, float length, float height,
            float offsetX, float offsetZ, float offsetY)
        {
            float halfWidth = width * 0.5f;
            float halfLength = length * 0.5f;
            float halfHeight = height * 0.5f;

            quadBound.min = new Vector3 (offsetX, offsetY, offsetZ);
            quadBound.max = new Vector3 (offsetX + width, offsetY + height, offsetZ + length);

            realBound.min = new Vector3 (-10000, -10000, -10000);
            realBound.max = realBound.min;
            if (data != null)
            {
                for (int i = 0; i < dataCap; ++i)
                {
                    SceneOctTreeData sotd = data[i] as SceneOctTreeData;
                    if (sotd != null)
                    {
                        if (sotd.valid)
                            SharedObjectPool<SceneOctTreeData>.Release(sotd);
                    }
                    data[i] = null;
                }
                dataCount = 0;
                //data.Clear ();
            }
            float minSize = halfWidth > halfLength ? halfLength : halfWidth;
            if (minSize > minNodeSize)
            {
                for (int y = 0; y < 2; ++y)
                {
                    for (int z = 0; z < 2; ++z)
                    {
                        for (int x = 0; x < 2; ++x)
                        {
                            int index = y * 4 + z * 2 + x;
                            var node = nodes[index];
                            if (node == null)
                            {
                                node = new OctTreeNode ();
                                nodes[index] = node;
                            }
                            node.Init (halfWidth, halfLength, halfHeight,
                                offsetX + x * halfWidth,
                                offsetZ + z * halfLength,
                                offsetY + y * halfHeight);
                        }
                    }
                }
            }
        }

        public void Clear(float width, float length)
        {
            float halfWidth = width * 0.5f;
            float halfLength = length * 0.5f;
            if (data != null)
            {
                for (int i = 0; i < dataCap; ++i)
                {
                    SceneOctTreeData sotd = data[i] as SceneOctTreeData;
                    if (sotd != null)
                    {
                        if (sotd.valid)
                            SharedObjectPool<SceneOctTreeData>.Release(sotd);
                    }
                    data[i] = null;
                }
                dataCount = 0;
                //data.Clear ();
            }
            float minSize = halfWidth > halfLength ? halfLength : halfWidth;
            if (minSize > minNodeSize)
            {
                for (int y = 0; y < 2; ++y)
                {
                    for (int z = 0; z < 2; ++z)
                    {
                        for (int x = 0; x < 2; ++x)
                        {
                            int index = y * 4 + z * 2 + x;
                            var node = nodes[index];
                            if (node != null)
                                node.Clear(halfWidth, halfLength);
                        }
                    }
                }
            }
        }
        public void AddAABB (ref AABB objBound)
        {
            if (realBound.min.x < -9999)
            {
                realBound = objBound;
            }
            else
            {
                realBound.Encapsulate (ref objBound);
            }
            if (data == null)
            {
                data = new List<IOctTreeData> ();
            }
        }
        public void AddToNode (IOctTreeData otd)
        {
            if (dataCount < dataCap)
            {
                data[dataCount] = otd;
            }
            else
            {
                data.Add(otd);                
                dataCap++;
            }
            dataCount++;
        }
        public bool Add (IOctTreeData otd, ref AABB objBound)
        {
            if (EngineUtility.IntersectsAABB (ref quadBound.min, ref quadBound.max,
                    ref objBound.min, ref objBound.max))
            {
                if (realBound.min.x < -9999)
                {
                    realBound = quadBound;
                }
                else
                {
                    realBound.Encapsulate (ref quadBound);
                }
                if (data == null)
                {
                    data = new List<IOctTreeData> ();
                }
                bool childHas = false;

                for (int y = 0; y < 2; ++y)
                {
                    for (int z = 0; z < 2; ++z)
                    {
                        for (int x = 0; x < 2; ++x)
                        {
                            int index = y * 4 + z * 2 + x;
                            var node = nodes[index];
                            if (node != null)
                            {
                                childHas |= node.Add (otd, ref objBound);
                            }
                        }
                    }
                }
                if (!childHas)
                {
                    AddToNode(otd);
                    childHas = true;
                }
                return childHas;
            }
            return false;
        }

        public void Draw (bool drawVisible)
        {
            if (data != null && dataCount > 0)
            {
                if (drawVisible)
                {
                    if (HasFlag (Visible))
                        Gizmos.DrawWireCube (quadBound.center, quadBound.size);
                }
                else
                    Gizmos.DrawWireCube (quadBound.center, quadBound.size);
            }

            for (int y = 0; y < 2; ++y)
            {
                for (int z = 0; z < 2; ++z)
                {
                    for (int x = 0; x < 2; ++x)
                    {
                        int index = y * 4 + z * 2 + x;
                        var node = nodes[index];
                        if (node != null)
                        {
                            node.Draw (drawVisible);
                        }
                    }
                }
            }
        }

        static RenderBatch tmpBatch = new RenderBatch ();
        private void AddBatch (SceneOctTreeData sotd,
            RenderContext rc,
            ref AABB renderAABB, ref ShadowDrawContext drawContext)
        {
            var mpb = sotd.mro.GetMPB ();
            if (mpb != null)
            {
                Material shadowCastMat = ShadowCullContext.GetShadowCasterMat (sotd.r, mpb);
                bool batchValid = false;
                if (sotd.mesh != null)
                {
                    batchValid = true;
                    tmpBatch.mesh = sotd.mesh;
                    tmpBatch.matrix = sotd.r.localToWorldMatrix;
                    tmpBatch.mat = shadowCastMat;
                    tmpBatch.render = sotd.r;
                    tmpBatch.passID = drawContext.passID;
                    tmpBatch.mpbRef = mpb;
                }
                else if (sotd.r != null)
                {
                    batchValid = true;
                    tmpBatch.matrix = sotd.r.localToWorldMatrix;
                    tmpBatch.mat = shadowCastMat;
                    tmpBatch.render = sotd.r;
                    tmpBatch.render.SetPropertyBlock (mpb);
                    tmpBatch.passID = drawContext.passID;
                    tmpBatch.mpbRef = mpb;
                }
                if (batchValid)
                {                    
                    var db = DrawBatch.Create (ref tmpBatch);
                    drawContext.shadowDrawCall.Push (db);
                    drawContext.Add(ref renderAABB);
#if UNITY_EDITOR
                    if (drawContext.shadowBatchDebug == null)
                        drawContext.shadowBatchDebug = new List<RenderBatch> ();
                    drawContext.shadowBatchDebug.Add (tmpBatch);
#endif
                }
            }
        }

        public void SetChildFlag (uint f, bool add)
        {
            SetFlag (f, add);
            for (int y = 0; y < 2; ++y)
            {
                for (int z = 0; z < 2; ++z)
                {
                    for (int x = 0; x < 2; ++x)
                    {
                        int index = y * 4 + z * 2 + x;
                        var node = nodes[index];
                        if (node != null)
                        {
                            node.SetChildFlag (f, add);
                        }
                    }
                }
            }
        }
        public void Cull (ref ShadowCullContext cc, ref ShadowContext csm)
        {
            //if (EngineUtility.IntersectsParallel3D (
            //        ref csm.parallel3D,
            //        ref realBound,
            //        ref cc.lightDir,
            //        cc.invSin,
            //        true))
            // if (true)
            if (csm.IsOverlapBounds(ref realBound))
            {
                if (data != null && dataCount > 0)
                {
                    for (int i = 0; i < dataCount; ++i)
                    {
                        SceneOctTreeData sotd = data[i] as SceneOctTreeData;
                        if (sotd.cullCounter != cc.cullCounter)
                        {
                            if (csm.IsOverlapBounds(ref sotd.objectBoundWS))
                            {
                                Bounds renderAABB = sotd.r.bounds;
                                AABB aabb = AABB.Create(renderAABB);
                                AddBatch(sotd, cc.rc, ref aabb, ref csm.drawContext);
                            }

                            //if (EngineUtility.IntersectsParallel3D (
                            //        ref csm.parallel3D,
                            //        ref sotd.objectBoundWS,
                            //        ref cc.lightDir,
                            //        cc.invSin,
                            //        true))
                            //{
                            //    Bounds renderAABB = sotd.r.bounds;
                            //    AABB aabb = AABB.Create (renderAABB);
                            //    AddBatch (sotd, cc.rc, ref aabb, ref csm.drawContext);
                            //}

                            if (sotd.mro.flag.HasFlag (SceneObject.EnableSelfShadow))
                            {
                                Mesh mesh = sotd.mro.GetMesh ();
                                if (mesh != null)
                                {
                                    if (cc.singleShadowMesh == null)
                                    {
                                        cc.singleShadowMesh = new Dictionary<Mesh, List<MeshRenderObject>>();
                                    }
                                    if (!cc.singleShadowMesh.TryGetValue (mesh, out var lst))
                                    {
                                        lst = new List<MeshRenderObject> ();
                                        cc.singleShadowMesh.Add (mesh, lst);
                                    }
                                    lst.Add (sotd.mro);
                                }

                            }
                            sotd.cullCounter = cc.cullCounter;
                        }

                    }
                    SetFlag (Visible, true);
                }
                else
                {
                    SetFlag (Visible, false);
                }

                for (int y = 0; y < 2; ++y)
                {
                    for (int z = 0; z < 2; ++z)
                    {
                        for (int x = 0; x < 2; ++x)
                        {
                            int index = y * 4 + z * 2 + x;
                            var node = nodes[index];
                            if (node != null)
                            {
                                node.Cull (ref cc, ref csm);
                            }
                        }
                    }
                }
            }
            else
            {
                SetChildFlag (Visible, false);
            }
        }
    }

#endif    

}