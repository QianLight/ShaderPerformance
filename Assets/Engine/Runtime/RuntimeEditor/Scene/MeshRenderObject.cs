#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngineEditor = UnityEditor.Editor;


namespace CFEngine
{
    [System.Serializable]
    public struct ChunkInfo
    {
        public int chunkID;
        public int x;
        public int z;
        public int blockID;
        public int virtualChunkID;
        public bool isGlobalObject;

        public void Copy (ref ChunkInfo ci)
        {
            chunkID = ci.chunkID;
            virtualChunkID = ci.virtualChunkID;
            x = ci.x;
            z = ci.z;
            blockID = ci.blockID;
            isGlobalObject = ci.isGlobalObject;
        }
        public void Reset ()
        {
            chunkID = -1;
            virtualChunkID = -1;
        }
    }

    [DisallowMultipleComponent, ExecuteInEditMode]
    public class MeshRenderObject : MonoBehaviour, IQuadTreeObject, ILightmapObject, IMatObject, ISceneAnim
    {
        //struct VertexCache
        //{
        //    public int x;
        //    public int y;
        //    public int z;
        //}
        public int id = 0;
        public uint areaMask = 0xffffffff;
        public string prefabName = "";
        public Bounds sceneAABB;
        public AABB aabb;

        //block info
        public ChunkInfo chunkInfo;
        public int layer = 0;
        public bool drawAABB = false;
        public FlagMask flag;

        #region submesh
        public ChunkSubMesh chunkSubMesh;
        #endregion

        #region material
        public Shader shader;
        public bool multiMat = false;
        public bool fadeEffect = false;

        private Vector4 renderFeature = Vector4.zero;

        public static bool SetFocusType = false;
        #endregion

        #region lod
        public SceneGroupObject group = null;
        public LodDist lodDist;
        public bool notCull = false;
        public bool forceGlobalObj = false;
        #endregion

        #region shadow
        private Vector3 lastPos = new Vector3 (-10000, 0, 0);
        private Quaternion lastRot = new Quaternion (-10000, 0, 0, 0);
        private Vector3 lastScale = new Vector3 (-10000, 1, 1);
        private ShadowCastingMode lastShadowMode = ShadowCastingMode.On;
        // public bool dynamicShadow = false;
        public bool lastSelfShadow = false;
        public bool forceCastShadow = false;
        
        #endregion

        #region brush
        public Mesh additionalVertexStreamMesh = null;

        [System.NonSerialized]
        public bool editingBrush = false;
        #endregion

        #region PVS
        public PVSOcclude pvsObjState = PVSOcclude.PVSOccluder;
        #endregion

        [System.NonSerialized]
        private MaterialPropertyBlock mpb = null;

        [System.NonSerialized]
        private Renderer render;

        [System.NonSerialized]
        private MeshFilter mf;

        public SceneGroupObject groupObject = null;

        public string exString = "";
        public bool uvAnimation = false;

        public int BlockId { get { return chunkInfo.blockID; } }
        public int QuadNodeId { get; set; }
        public AABB bounds { get { return aabb; } }
        #region lightmap

        public LigthmapComponent lightmapComponent = new LigthmapComponent ();

        public float LightmapScale { get { return lightmapComponent.lightMapScale; } set { lightmapComponent.lightMapScale = value; } }
        public int LightmapVolumnIndex { get { return lightmapComponent.lightMapVolumnIndex; } set { lightmapComponent.lightMapVolumnIndex = value; } }
        public Texture2D CombinedTex { get { return lightmapComponent.ligthmapRes.combine; } set { lightmapComponent.ligthmapRes.combine = value; } }

        public void ClearLightmap ()
        {
            lightmapComponent.Clear ();
        }


        public void BeginBake ()
        {
            //GetRenderer ();
            //if (render != null)
            //{
            //    Material mat = render.sharedMaterial;

            //    if (mat != null)
            //    {
            //        Shader s = mat.shader;
            //        if (s.name != "Standard")
            //        {
            //            render.SetPropertyBlock (null);
            //            Color c = mat.GetColor ("_Color0");
            //            BlendMode mode = EditorCommon.GetBlendMode (mat);
            //            mat.shader = Shader.Find ("Standard");
            //            mat.SetColor ("_Color", c);
            //            if (mode == BlendMode.Cutout || mode == BlendMode.CutoutTransparent)
            //                EditorCommon.SetStandardCutoutMode (mat);
            //        }

            //    }
            //}

        }
        public void EndBake()
        {
            //if (shader != null)
            //{
            //    GetRenderer ();
            //    if (render != null)
            //    {
            //        Material mat = render.sharedMaterial;
            //        if (mat != null)
            //        {
            //            mat.shader = shader;
            //            mat.SetColor ("_Color", Color.white);
            //        }
            //    }
            //}
        }

        public void SetLightmapData (int lightMapIndex, Vector4 uvst)
        {
            lightmapComponent.SetLightmapData (lightMapIndex, uvst);
        }
        public void SetLightmapRes (Texture2D color, Texture2D shadowMask, Texture2D dir, Texture2D colorCombineShadowMask)
        {
            lightmapComponent.SetLightmapRes (color, shadowMask, dir,colorCombineShadowMask);
        }

        public void BindLightMap (LigthmapRes[] res, int nAddIndex)
        {
            lightmapComponent.BindLightMap (res,render,nAddIndex);
        }
        public void BindLightMap (int volumnIndex)
        {
            lightmapComponent.BindLightMap (volumnIndex);
        }
        #endregion

        private Transform t;
        private Transform GetTrans ()
        {
            if (t == null)
            {
                t = this.transform;
            }
            return t;
        }
        public Renderer GetRenderer ()
        {
            if (render == null)
            {
                render = GetComponent<Renderer> ();
            }
            return render;
        }
        public MaterialPropertyBlock GetMPB ()
        {
            return mpb;
        }
        public bool IsRenderValid ()
        {
            return GetRenderer () != null && GetRenderer ().enabled && GetRenderer ().gameObject.activeInHierarchy;
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
        public Material GetMat ()
        {
            GetRenderer ();
            return render != null?render.sharedMaterial : null;
        }
        public Mesh GetRuntimeMesh ()
        {
            mf = GetMeshFilter ();
            return mf != null?mf.sharedMesh : null;
        }
        public string GetdAddMeshPath (ref string addMeshName, ref string dir)
        {
            var mesh = GetMesh ();
            string assetPath;
            dir = AssetsPath.GetAssetDir (mesh, out assetPath);
            string fbxname;
            if (AssetsPath.GetFileName (assetPath, out fbxname))
            {
                addMeshName = string.Format ("{0}_{1}_vc", fbxname, mesh.name);
                string meshPath = string.Format ("{0}/{1}.asset", dir, addMeshName);
                return meshPath;
            }
            return null;
        }

        public UnityEngine.Object BindAddMeshInfo ()
        {
            string addMeshName = null;
            string dir = null;
            string path = GetdAddMeshPath (ref addMeshName, ref dir);
            if (!string.IsNullOrEmpty (path))
            {
                additionalVertexStreamMesh = AssetDatabase.LoadAssetAtPath<Mesh> (path);
                return UpdateAddMesh ();
            }

            return null;
        }

        public UnityEngine.Object ReBindAddMeshInfo()
        {
            string addMeshName = null;
            string dir = null;
            string path = GetdAddMeshPath(ref addMeshName, ref dir);
            if (!string.IsNullOrEmpty(path))
            {
                string extPath = string.Format("{0}/{1}_ext.bytes", dir, addMeshName);
                if (File.Exists(extPath))
                {
                    var mesh = GetMesh();
                    var addMesh = AssetDatabase.LoadAssetAtPath<Mesh>(path);
                    if (addMesh != null && mesh != null)
                    {
                        using (FileStream fs = new FileStream(extPath, FileMode.Open))
                        {
                            BinaryReader br = new BinaryReader(fs);
                            int count = br.ReadInt32();
                            if (count > 0)
                            {
                                // var vertices = mesh.vertices;
                                // var colors = addMesh.colors;

                                // if (colors != null && vertices != null)
                                // {
                                //     var newColor = new Color[vertices.Length];
                                //     var vc = new List<VertexCache> ();
                                //     for (int i = 0; i < count; ++i)
                                //     {
                                //         int x = br.ReadInt32 ();
                                //         int y = br.ReadInt32 ();
                                //         int z = br.ReadInt32 ();
                                //         vc.Add (new VertexCache () { x = x, y = y, z = z });
                                //     }

                                //     for (int i = 0; i < vertices.Length; ++i)
                                //     {
                                //         ref var pos = ref vertices[i];
                                //         int x = (int) (pos.x * 100);
                                //         int y = (int) (pos.y * 100);
                                //         int z = (int) (pos.z * 100);
                                //         int index = vc.FindIndex ((v) => v.x == x && v.y == y && v.z == z);
                                //         if (index >= 0 && index < colors.Length)
                                //         {
                                //             newColor[i] = colors[index];
                                //         }
                                //     }

                                //     var newAddMesh = new Mesh ();

                                //     newAddMesh.name = addMesh.name;
                                //     newAddMesh.vertices = addMesh.vertices;
                                //     newAddMesh.colors = newColor;

                                //     AssetDatabase.DeleteAsset (path);
                                //     additionalVertexStreamMesh = EditorCommon.CreateAsset<Mesh> (path, ".asset", newAddMesh);
                                //     UpdateAddMesh ();
                                // }


                                var newAddMesh = copyVC(mesh, addMesh);
                                EditorCommon.SaveAsset(newAddMesh);
                                //AssetDatabase.DeleteAsset (path);
                                //additionalVertexStreamMesh = EditorCommon.CreateAsset<Mesh> (path, ".asset", newAddMesh);
                                additionalVertexStreamMesh = newAddMesh;

                                return UpdateAddMesh();
                            }
                        }
                    }
                }
            }

            return null;
        }


        public Mesh copyVC(Mesh m , Mesh addm )
        {
            var vertices = m.vertices;
            var triangles = m.triangles;
            var addMeshVertices = addm.vertices;
            var colors = addm.colors;
            //var newAddMesh = new Mesh ();

            if (colors != null && vertices != null)
            {
                var newColor = new Color[vertices.Length];
                for (int i = 0; i < vertices.Length; ++i)
                {
                    for(int j=0 ; j < addMeshVertices.Length ; j++ )
                    {
                        bool xx =Mathf.Abs (vertices[i].x - addMeshVertices[j].x)<0.1f;
                        bool yy =Mathf.Abs (vertices[i].y - addMeshVertices[j].y)<0.1f;
                        bool zz =Mathf.Abs (vertices[i].z - addMeshVertices[j].z)<0.1f;
                        if(xx&yy&zz)
                        {
                            newColor[i]=colors[j];
                        }
                    }
                }
                //newAddMesh.name = addm.name;
                addm.vertices = vertices;
                addm.triangles = triangles;
                addm.colors = newColor;
            }
            return addm;
        }


        public UnityEngine.Object UpdateAddMesh()
        {
            var mr = GetRenderer() as MeshRenderer;
            if (mr == null) return null;

            Mesh _mesh = transform.GetComponent<MeshFilter>().sharedMesh;

            if (_mesh == null || additionalVertexStreamMesh == null) return null;
            
            Debug.Log(
                "UpdateAddMesh:" + _mesh.vertexCount + "  additionalVertexStreamMesh:" +
                additionalVertexStreamMesh.vertexCount, gameObject);
            
            if (_mesh.vertexCount == additionalVertexStreamMesh.vertexCount)
            {
                mr.additionalVertexStreams = additionalVertexStreamMesh;
                return mr;
            }
            else
            {
                Debug.LogError(
                    "UpdateAddMesh:" + _mesh.vertexCount + "  additionalVertexStreamMesh:" +
                    additionalVertexStreamMesh.vertexCount, gameObject);
            }
            
            return null;
        }


        public void ClearAddMesh()
        {
            var mr = GetRenderer() as MeshRenderer;
            if (mr != null)
            {
                Mesh _mesh = transform.GetComponent<MeshFilter>().sharedMesh;
                if (_mesh != null)
                {
                    mr.additionalVertexStreams = null;
                }
            }
        }

        public void DebugAddMesh()
        {
            var mr = GetRenderer() as MeshRenderer;
            if (mr != null)
            {
                if (mr.additionalVertexStreams != null)
                {
                    Debug.Log("DebugAddMesh:" + mr.additionalVertexStreams, mr.additionalVertexStreams);
                }
                else
                {
                    Debug.Log("DebugAddMesh: null!");
                }
            }
        }

        public void Refresh (RenderingManager mgr)
        {
            if (GetRenderer () != null)
            {
                if (mpb == null)
                {
                    mpb = mgr.GetMpb (render);
                }
                mpb.Clear ();
                bool lightmap = RuntimeUtilities.BindLightmap (mpb, lightmapComponent);
                renderFeature.y = lightmap?1 : 0;
                renderFeature.z = 1;
                renderFeature.w = flag.HasFlag (SceneObject.EnableSelfShadow) ? 1 : 0;
                mpb.SetVector (ShaderManager._Param, renderFeature);
                Material m = GetMat ();
                if (m != null)
                {
                    EnvironmentExtra.EnableEditorMat (m);
                }
                render.SetPropertyBlock (mpb);
                UpdateAddMesh ();
            }
        }

        public void OnDrawGizmo(EngineContext context)
        {
            if (GetRenderer() == null) return;


            if (chunkInfo.isGlobalObject&&
                this.gameObject.activeInHierarchy
                && GetRenderer().shadowCastingMode != ShadowCastingMode.Off)
            {
                Gizmos.DrawWireCube(aabb.center, aabb.size);
            }
        }
        public void SetAreaMask(uint area)
        {
            areaMask = area;
        }
        public void Refresh()
        {
            if (GetRenderer() != null)
            {
                if (mpb != null)
                {
                    render.SetPropertyBlock(mpb);
                }
                UpdateAddMesh();
            }
            Refresh(RenderingManager.instance);
        }

        public void Clear (RenderingManager mgr)
        {
            if (GetRenderer () != null)
            {
                Material m = GetMat ();
                if (m != null)
                {
                    m.SetVector (ShaderManager._Param, Vector4.zero);
                    EnvironmentExtra.EnableEditorMat (m);
                }
                render.SetPropertyBlock (null);
            }
        }

        public bool UpdateShadowState (EngineContext context)
        {
            GetRenderer ();
            bool change = true;
            var trans = this.transform;
            float d0 = (lastPos - trans.position).sqrMagnitude;
            var q = trans.rotation;
            Vector4 qv = new Vector4 (q.x - lastRot.x, q.y - lastRot.y, q.z - lastRot.z, q.w - lastRot.w);
            float d1 = qv.sqrMagnitude;
            float d2 = (lastScale - trans.lossyScale).sqrMagnitude;
            bool selfShadow = flag.HasFlag (SceneObject.EnableSelfShadow);
            if (d0 < 0.001f && d1 < 0.001f && d2 < 0.001f &&
                render.shadowCastingMode == lastShadowMode &&
                selfShadow == lastSelfShadow)
            {
                change = false;
            }
            else
            {
                change = true;
            }

            lastPos = trans.position;
            lastRot = trans.rotation;
            lastScale = trans.lossyScale;
            lastShadowMode = render.shadowCastingMode;
            lastSelfShadow = selfShadow;
            return change;
        }

        private void OnDrawGizmosSelected ()
        {
            var c = Gizmos.color;
            //if (editingBrush)
            //{
            //    var mesh = GetMesh ();
            //    if (mesh != null)
            //    {
            //        var tran = GetTrans ();
                    
            //        Gizmos.color = Color.black;
            //        Gizmos.DrawWireMesh (mesh, 0, tran.position, tran.rotation, tran.lossyScale);
                    
            //    }
            //}
            if(drawAABB)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawWireCube(aabb.center, aabb.size);
                if (chunkSubMesh != null)
                {
                    chunkSubMesh.OnDrawGizmos();
                }
            }
            Gizmos.color = c;
        }



        #region sceneAnim
        public void SetAnim(string exString, uint f)
        {
            flag.SetFlag(SceneObject.HasAnim | f, true);
            if (!string.IsNullOrEmpty(exString) &&
                this.exString != exString)
            {
                DebugLog.AddErrorLog2("multi animation exstrinig: src {0} current{1} obj:{2}",
                    this.exString,
                    exString, this.name);
            }
            this.exString = exString;
        }
        private void SetUVOffset(Material m, int uvKey, ref Vector2 offset)
        {
            if (m.HasProperty(uvKey))
            {
                var uvstSrc = m.GetVector(uvKey);
                var uvst = mpb.GetVector(uvKey);
                uvst.x = uvstSrc.x;
                uvst.y = uvstSrc.y;
                uvst.z += offset.x;
                uvst.w += offset.y;
                mpb.SetVector(uvKey, uvst);
            }
        }
        public void SetUVOffset(ref Vector2 offset)
        {
            if (GetRenderer() != null)
            {
                if (render.HasPropertyBlock())
                {
                    if (mpb != null)
                    {
                        Material m = GetMat();
                        if (m != null)
                        {
                            SetUVOffset(m, ShaderManager._ShaderKeyEffectKey[(int)EShaderKeyID._UVST0], ref offset);
                            SetUVOffset(m, ShaderManager._ShaderKeyEffectKey[(int)EShaderKeyID._UVST1], ref offset);
                            render.SetPropertyBlock(mpb);
                        }
                    }
                }
            }
        }
        #endregion

        #region submesh
        private int CalcPointChunk(ref Vector3 p,int xCount, int zCount)
        {
            int x = Mathf.Clamp((int)(p.x / EngineContext.ChunkSize), 0, xCount - 1);
            int z = Mathf.Clamp((int)(p.z / EngineContext.ChunkSize), 0, zCount - 1);
            return x + z * xCount;
        }

        private void Add2SubMesh(int chunkId, Dictionary<int, SubMesh> splitmesh, int i0, int i1, int i2)
        {
            if (!splitmesh.TryGetValue(chunkId, out var sub))
            {
                sub = new SubMesh();
                splitmesh[chunkId] = sub;
            }
            sub.index.Add(i0);
            sub.index.Add(i1);
            sub.index.Add(i2);
        }

        private int CalcPointChunk(ref Vector3 p0, ref Vector3 p1, ref Vector3 p2, int xCount, int zCount)
        {
            int chunkId0 = CalcPointChunk(ref p0, xCount, zCount);
            int chunkId1 = CalcPointChunk(ref p1, xCount, zCount);
            int chunkId2 = CalcPointChunk(ref p2, xCount, zCount);
            if (chunkId0 == chunkId1 && chunkId1 == chunkId2)
            {
                return chunkId0;
            }
            else
            {
                return Mathf.Min(chunkId0, Mathf.Min(chunkId1, chunkId2));
            }

        }
        public void SplitMesh()
        {
            var mesh = GetMesh();
            if (mesh != null)
            {                
                EngineContext context = EngineContext.instance;
                int xCount = context.Width / EngineContext.ChunkSize;

                int zCount = context.Height / EngineContext.ChunkSize;
                var matrix = transform.localToWorldMatrix;
                var matrix2 = Matrix4x4.TRS(Vector3.zero, transform.rotation, transform.lossyScale);
                Dictionary<int, SubMesh> splitmesh = new Dictionary<int, SubMesh>();
                var vertices = mesh.vertices;
                var normals = mesh.normals;
                var tangents = mesh.tangents;
                for (int i = 0; i < vertices.Length; ++i)
                {
                    vertices[i] = matrix.MultiplyPoint(vertices[i]);
                    normals[i] = matrix2.MultiplyPoint(normals[i]);
                    tangents[i] = matrix2.MultiplyPoint(tangents[i]);
                }
               
                var uv = mesh.uv;
                var uv2 = mesh.uv2;
                if(uv2.Length!=uv.Length)
                {
                    uv2 = null;
                }
                Color[] color = null;
                if (additionalVertexStreamMesh != null)
                {
                    color = additionalVertexStreamMesh.colors;
                    if (color.Length != vertices.Length)
                    {
                        color = null;
                    }
                }
                var indexes = mesh.GetTriangles(0);
                for (int i = 0; i < indexes.Length; i += 3)
                {
                    var i0 = indexes[i];
                    var i1 = indexes[i + 1];
                    var i2 = indexes[i + 2];
                    var p0 = vertices[i0];
                    var p1 = vertices[i1];
                    var p2 = vertices[i2];
                    int chunkId = CalcPointChunk(ref p0, ref p1, ref p2, xCount, zCount);
                    Add2SubMesh(chunkId, splitmesh, i0, i1, i2);
                }
                if (splitmesh.Count > 1)
                {
                    if (chunkSubMesh == null)
                    {
                        chunkSubMesh = ScriptableObject.CreateInstance<ChunkSubMesh>();
                    }
                    GameObject prefab = PrefabUtility.GetCorrespondingObjectFromSource(this.gameObject) as GameObject;
                    chunkSubMesh.name = string.Format("{0}_sub", prefab.name);
                    chunkSubMesh.subMesh.Clear();
                    string dir = AssetsPath.GetAssetDir(prefab, out var path);

                    Dictionary<int, int> reIndex = new Dictionary<int, int>();
                    var it = splitmesh.GetEnumerator();
                    while (it.MoveNext())
                    {
                        reIndex.Clear();
                        int currentIndex = 0;
                        var sub = it.Current.Value;
                        SubMeshData subMeshData = new SubMeshData();
                        for (int i = 0; i < sub.index.Count; ++i)
                        {
                            var srcIndex = sub.index[i];
                            if (!reIndex.TryGetValue(srcIndex, out var index))
                            {
                                index = currentIndex;
                                reIndex.Add(srcIndex, currentIndex);
                                var pos = vertices[srcIndex];
                                subMeshData.pos.Add(pos);
                                subMeshData.normal.Add(normals[srcIndex]);
                                subMeshData.tangent.Add(tangents[srcIndex]);
                                subMeshData.uv.Add(uv[srcIndex]);
                                if (uv2 != null)
                                {
                                    subMeshData.uv2.Add(uv2[srcIndex]);
                                }
                                if (color != null)
                                {
                                    subMeshData.color.Add(color[srcIndex]);
                                }
                                if (currentIndex == 0)
                                {
                                    sub.aabb.Init(pos, Vector3.zero);
                                }
                                else
                                {
                                    sub.aabb.Encapsulate(pos);
                                }
                                currentIndex++;
                            }
                            subMeshData.index.Add(index);
                        }
                        var m = new Mesh();
                        m.SetVertices(subMeshData.pos);
                        m.SetNormals(subMeshData.normal);
                        m.SetTangents(subMeshData.tangent);
                        m.SetUVs(0, subMeshData.uv);
                        if (subMeshData.uv2.Count > 0)
                            m.SetUVs(1, subMeshData.uv2);
                        if (subMeshData.color.Count > 0)
                            m.SetColors(subMeshData.color);
                        m.SetIndices(subMeshData.index, MeshTopology.Triangles, 0);
                        m.name = string.Format("{0}_{1}", chunkSubMesh.name, it.Current.Key.ToString());
                        string meshpath = string.Format("{0}/{1}.asset", LoadMgr.singleton.editorResPath, m.name);
                        if (File.Exists(meshpath))
                            AssetDatabase.DeleteAsset(meshpath);
                        m.UploadMeshData(true);
                        m = EditorCommon.CreateAsset<Mesh>(meshpath, ".asset", m);
                        sub.m = m;
                        chunkSubMesh.subMesh.Add(sub);
                        SceneAssets.CalcBlock(context, ref sub.aabb, ref sub.chunkInfo, false, false);
                    }
                    chunkSubMesh = EditorCommon.CreateAsset<ChunkSubMesh>(dir, chunkSubMesh.name, ".asset", chunkSubMesh);
                }
                else
                {
                    chunkSubMesh = null;
                }
                
            }
        }

        public void BindSubMesh()
        {
            GameObject prefab = PrefabUtility.GetCorrespondingObjectFromSource(this.gameObject) as GameObject;
            string subMeshName = string.Format("{0}_sub", prefab.name);
            string dir = AssetsPath.GetAssetDir(prefab, out var path);
            string subMeshPath = string.Format("{0}/{1}.asset", dir, subMeshName);
            chunkSubMesh = AssetDatabase.LoadAssetAtPath<ChunkSubMesh>(subMeshPath);
        }
        #endregion
    }

    [CanEditMultipleObjects, CustomEditor (typeof (MeshRenderObject))]
    public class MeshRenderObjectEditor : UnityEngineEditor
    {
        SerializedProperty drawAABB;
        SerializedProperty lightmapScale;
        SerializedProperty layer;
        SerializedProperty additionalVertexStreamMesh;
        SerializedProperty notCull;
        SerializedProperty forceGlobalObj;
        SerializedProperty chunkSubMesh;
        SerializedProperty fadeEffect;
        SerializedProperty pvsObjState;
        SerializedProperty forceCastShadow;
        static GUIStyle layerPopup;
        static GUIContent layerContent;

        void OnEnable ()
        {
            drawAABB = serializedObject.FindProperty("drawAABB");
            lightmapScale = serializedObject.FindProperty ("lightmapComponent.lightMapScale");
            layer = serializedObject.FindProperty ("layer");
            additionalVertexStreamMesh = serializedObject.FindProperty ("additionalVertexStreamMesh");
            notCull = serializedObject.FindProperty("notCull");
            forceGlobalObj = serializedObject.FindProperty("forceGlobalObj");
            chunkSubMesh = serializedObject.FindProperty("chunkSubMesh");
            fadeEffect = serializedObject.FindProperty("fadeEffect");
            pvsObjState = serializedObject.FindProperty("pvsObjState");
            forceCastShadow = serializedObject.FindProperty("forceCastShadow");
        }
        public override void OnInspectorGUI ()
        {
            serializedObject.Update ();
            MeshRenderObject mro = target as MeshRenderObject;
            //////////////////////////////////////////////////////////////////
            EditorCommon.BeginGroup("Common");
            EditorGUILayout.LabelField("ResName", mro.prefabName);
            EditorGUILayout.IntField("ChunkID", mro.chunkInfo.chunkID);
            EditorGUILayout.IntField("VirtualChunkID", mro.chunkInfo.virtualChunkID);
            EditorGUILayout.IntField("BlockID", mro.chunkInfo.blockID);
            EditorGUILayout.Toggle("GlobalObject", mro.chunkInfo.isGlobalObject);
            EditorGUILayout.IntField("X", mro.chunkInfo.x);
            EditorGUILayout.IntField("Z", mro.chunkInfo.z);
            EditorGUILayout.PropertyField(drawAABB);
            EditorGUILayout.Vector3Field ("Min", mro.aabb.min);
            EditorGUILayout.Vector3Field ("Max", mro.aabb.max);
            EditorGUILayout.LabelField ("ExString", mro.exString);
            EditorGUILayout.IntField ("AreaMask", (int) mro.areaMask);
            EditorGUILayout.PropertyField(fadeEffect);
            EditorGUILayout.PropertyField(forceCastShadow);            
            EditorGUILayout.PropertyField(pvsObjState);
            if (layerPopup == null)
                layerPopup = new GUIStyle (EditorStyles.popup);
            if (layerContent == null)
                layerContent = EditorGUIUtility.TrTextContent ("Layer", "The layer that this GameObject is in.\n\nChoose Add Layer... to edit the list of available layers.");

            Rect layerRect = GUILayoutUtility.GetRect (GUIContent.none, layerPopup);
            EditorGUI.BeginProperty (layerRect, GUIContent.none, layer);
            if (mro.layer < 0)
                mro.layer = 0;
            EditorGUI.BeginChangeCheck ();
            int l = EditorGUI.LayerField (layerRect, layerContent, mro.layer, layerPopup);
            if (EditorGUI.EndChangeCheck ())
            {
                layer.intValue = l;
            }
            EditorGUI.EndProperty ();
            EditorCommon.EndGroup ();
            EditorGUILayout.Space ();

            //////////////////////////////////////////////////////////////////
            EditorCommon.BeginGroup("SplitMesh");
            EditorGUILayout.PropertyField(chunkSubMesh);
            if (mro.chunkSubMesh != null)
                mro.chunkSubMesh.OnInspector();
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("BakeSubMesh", GUILayout.MaxWidth(100)))
            {
                mro.SplitMesh();
            }
            if (GUILayout.Button("BindMesh", GUILayout.MaxWidth(100)))
            {
                mro.BindSubMesh();
            }
            if (GUILayout.Button("Clear", GUILayout.MaxWidth(100)))
            {
                chunkSubMesh.objectReferenceValue = null;
            }
            EditorGUILayout.EndHorizontal();
            EditorCommon.EndGroup();
            EditorGUILayout.Space();
            //////////////////////////////////////////////////////////////////
            EditorCommon.BeginGroup ("Group");
            if (mro.group != null)
            {
                EditorGUILayout.ObjectField ("Group", mro.group, typeof (SceneGroupObject), true);
            }
            else
            {
                // EditorGUILayout.Toggle ("DistCull", mro.flag.HasFlag (SceneObject.DistCull));
                EditorGUILayout.FloatField ("lodDist2", mro.lodDist.lodDist2);
                EditorGUILayout.FloatField ("fadeDist2", mro.lodDist.fadeDist2);
                //if (GUILayout.Button ("TestLod", GUILayout.MaxWidth (80)))
                //{
                //    SceneGroupObject.CalcLod (ref mro.aabb, ref mro.lodDist);
                //}
            }
            EditorGUILayout.PropertyField(notCull);
            EditorGUILayout.PropertyField(forceGlobalObj);
            
            EditorCommon.EndGroup ();
            //////////////////////////////////////////////////////////////////
            EditorCommon.BeginGroup ("Shadow");
            bool selfShadow = mro.flag.HasFlag (SceneObject.EnableSelfShadow);
            selfShadow = EditorGUILayout.Toggle ("SelfShadow", selfShadow);
            mro.flag.SetFlag (SceneObject.EnableSelfShadow, selfShadow);
            EditorCommon.EndGroup ();
            //////////////////////////////////////////////////////////////////
            EditorGUILayout.LabelField ("LightMap", EditorStyles.boldLabel);
            EditorGUILayout.ObjectField ("Shader", mro.shader, typeof (Shader), false);
            RuntimeUtilities.OnLightmapInspectorGUI (mro.lightmapComponent, lightmapScale);
            EditorGUILayout.Space ();
            //////////////////////////////////////////////////////////////////
            EditorGUILayout.LabelField ("Brush", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField (additionalVertexStreamMesh);
            EditorGUILayout.BeginHorizontal ();
            if (GUILayout.Button ("Bind", GUILayout.MaxWidth (80)))
            {
                SaveAddMeshToScene(mro.BindAddMeshInfo ());
            }

            if (GUILayout.Button("ReBind", GUILayout.MaxWidth(80)))
            {
                SaveAddMeshToScene(mro.ReBindAddMeshInfo());
            }

            if (GUILayout.Button ("Clear", GUILayout.MaxWidth (80)))
            {
                mro.ClearAddMesh();
            }
            
            if (GUILayout.Button ("Debug", GUILayout.MaxWidth (80)))
            {
                mro.DebugAddMesh();
            }
            
            EditorGUILayout.EndHorizontal ();
            EditorGUILayout.Space ();
            //////////////////////////////////////////////////////////////////
            EditorGUILayout.LabelField ("Misc", EditorStyles.boldLabel);
            if (GUILayout.Button ("Refresh", GUILayout.MaxWidth (80)))
            {
                mro.Refresh ();
            }
            EditorGUILayout.Space ();

            serializedObject.ApplyModifiedProperties ();
        }

        public void SaveAddMeshToScene(UnityEngine.Object obj)
        {
            EditorCommon.SaveAsset(obj);
            SceneAssets.SceneModify(true);
        }
    }
}
#endif