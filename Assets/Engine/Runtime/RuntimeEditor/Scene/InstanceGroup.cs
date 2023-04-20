#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngineEditor = UnityEditor.Editor;
namespace CFEngine
{
    public struct InstanceRenderBatch
    {
        public Mesh mesh;
        public Material mat;
        public int passID;
        public int layer;
        public Bounds aabb;
        public MaterialPropertyBlock mpbRef;

        public ComputeBuffer argBuffer;
        public int argBufferOffset;
    }

    public struct ChunkInstanceGroup
    {
        public int chunkId;
        public int instanceCount;
        public ComputeBuffer dataBuffer;
        public ComputeBuffer argBuffer;
        public List<InstanceRenderBatch> instanceRenderBatch;

    }

    [System.Serializable]
    public class FbxMeshReplace
    {
        public Mesh srcMesh;
        public Mesh replaceMesh;
    }

    [DisallowMultipleComponent, ExecuteInEditMode]
    public class InstanceGroup : MonoBehaviour
    {
        public bool preview = false;
        public List<FbxMeshReplace> meshReplace = new List<FbxMeshReplace>();

        [System.NonSerialized]
        public List<ChunkInstanceGroup> chunkData = new List<ChunkInstanceGroup>();
        public void Reset()
        {
            for (int i = 0; i < chunkData.Count; ++i)
            {
                var cig = chunkData[i];
                if (cig.dataBuffer != null)
                {
                    cig.dataBuffer.Dispose();
                    cig.dataBuffer = null;
                }
                if (cig.dataBuffer != null)
                {
                    cig.argBuffer.Dispose();
                    cig.argBuffer = null;
                }
            }
        }
        void Update()
        {
            if (preview)
            {
                for (int i = 0; i < chunkData.Count; ++i)
                {
                    var cig = chunkData[i];
                    if (cig.instanceRenderBatch != null && cig.dataBuffer != null && cig.argBuffer != null)
                    {
                        for (int j = 0; j < cig.instanceRenderBatch.Count; ++j)
                        {
                            var irb = cig.instanceRenderBatch[j];
                            if (irb.mat != null && irb.mesh != null)
                            {
                                Graphics.DrawMeshInstancedIndirect(irb.mesh,
                                    0,
                                    irb.mat,
                                    irb.aabb,
                                    cig.argBuffer,
                                    irb.argBufferOffset,
                                    irb.mpbRef,
                                    ShadowCastingMode.Off,
                                    false,
                                    0,
                                    null,
                                    LightProbeUsage.BlendProbes,
                                    null);
                            }

                        }

                    }
                }
            }
        }
    //    public void PreviewInstance()
    //    {
    //        chunkData.Clear();
    //        this.enabled = preview;
    //        EditorCommon.SetRenderEnable(transform, !preview, true);
    //        Reset();
    //        if (preview)
    //        {
    //            Dictionary<string, Mesh> instanceMeshMap = new Dictionary<string, Mesh>();
    //            List<InstanceInfo> instanceInfos = new List<InstanceInfo>();
    //            List<uint> argArray = new List<uint>();
    //            for (int i = 0; i < ecd.chunks.Count; ++i)
    //            {
    //                var chunk = ecd.chunks[i];

    //                instanceInfos.Clear();
    //                argArray.Clear();
    //                int argOffset = 0;
    //                ChunkInstanceGroup cig = new ChunkInstanceGroup();
    //                for (int j = 0; j < chunk.blocks.Length; ++j)
    //                {
    //                    var ob = chunk.blocks[j];

    //                    for (int k = 0; k < ob.instanceObjects.Count; ++k)
    //                    {
    //                        var io = ob.instanceObjects[k];
    //                        if (io.mat != null && io.prefabIndex >= 0 && io.prefabIndex < ecd.prefabInfos.Count &&
    //                            io.instanceInfo.Count > 0)
    //                        {
    //                            var prefabInfo = ecd.prefabInfos[io.prefabIndex];
    //                            string meshPath = string.Format("{0}{1}_0.asset",
    //                                LoadMgr.singleton.editorResPath,
    //                                prefabInfo.prefab.name);
    //                            Mesh mesh;
    //                            if (!instanceMeshMap.TryGetValue(meshPath, out mesh))
    //                            {
    //                                mesh = AssetDatabase.LoadAssetAtPath<Mesh>(meshPath);
    //                                instanceMeshMap.Add(meshPath, mesh);
    //                            }
    //                            if (mesh != null)
    //                            {
    //                                int instanceOffset = instanceInfos.Count;
    //                                Bounds aabb = new Bounds();
    //                                uint instanceCount = 0;
    //                                for (int xx = 0; xx < io.instanceInfo.Count; ++xx)
    //                                {
    //                                    var ip = io.instanceInfo[xx];
    //                                    if (ip.visible)
    //                                    {
    //                                        Quaternion qq = ip.rot;
    //                                        instanceInfos.Add(
    //                                            new InstanceInfo()
    //                                            {
    //                                                posScale = new Vector4(ip.pos.x, ip.pos.y, ip.pos.z, ip.scale),
    //                                                rot = new Vector4(qq.x, qq.y, qq.z, qq.w)
    //                                            });
    //                                        if (instanceCount == 0)
    //                                        {
    //                                            aabb = ip.aabb;
    //                                        }
    //                                        else
    //                                        {
    //                                            aabb.Encapsulate(ip.aabb);
    //                                        }
    //                                        instanceCount++;

    //                                    }
    //                                }
    //                                if (instanceCount > 0)
    //                                {
    //                                    if (instanceCount > 250)
    //                                    {
    //                                        DebugLog.AddErrorLog("each block can only use 250 grass on android!");
    //                                    }

    //                                    InstanceRenderBatch rb = new InstanceRenderBatch()
    //                                    {
    //                                        mesh = mesh,
    //                                        mat = AssetsConfig.instance.PreviewInstance,
    //                                        mpbRef = new MaterialPropertyBlock(),
    //                                        aabb = aabb,
    //                                        argBufferOffset = argOffset,
    //                                        layer = (int)instanceCount
    //                                    };
    //                                    if (cig.instanceRenderBatch == null)
    //                                    {
    //                                        cig.instanceRenderBatch = new List<InstanceRenderBatch>();
    //                                    }
    //                                    Texture tex = io.mat.GetTexture(ShaderIDs.MainTex);
    //                                    if (tex != null)
    //                                    {
    //                                        rb.mpbRef.SetTexture(ShaderIDs.MainTex, tex);
    //                                        // rb.mpbRef.SetVector ("_UVST0", io.mat.GetVector ("_UVST0"));
    //                                    }
    //                                    Texture tex1 = io.mat.GetTexture("_MainTex1");
    //                                    if (tex1 != null)
    //                                    {
    //                                        rb.mpbRef.SetTexture("_MainTex1", tex1);
    //                                        // rb.mpbRef.SetVector ("_UVST0", io.mat.GetVector ("_UVST0"));
    //                                    }
    //                                    rb.mpbRef.SetColor("_Color0", io.mat.GetColor("_Color0").gamma);
    //                                    rb.mpbRef.SetVector("_Param0", io.mat.GetVector("_Param0"));
    //                                    rb.mpbRef.SetVector("_Param1", io.mat.GetVector("_Param1"));
    //                                    rb.mpbRef.SetVector("_Param2", io.mat.GetVector("_Param2"));
    //                                    rb.mpbRef.SetColor("_Color1", io.mat.GetColor("_Color1").gamma);
    //                                    rb.mpbRef.SetInt(ShaderManager._InstanceOffset, instanceOffset);
    //                                    cig.instanceRenderBatch.Add(rb);
    //                                    argArray.Add(mesh.GetIndexCount(0));
    //                                    argArray.Add(instanceCount);
    //                                    argArray.Add(0);
    //                                    argArray.Add(0);
    //                                    argArray.Add(0);
    //                                    argOffset += 20;
    //                                }

    //                            }

    //                        }
    //                    }
    //                }
    //                //fill chunk compute data
    //                if (instanceInfos.Count > 0)
    //                {
    //                    cig.argBuffer = new ComputeBuffer(argArray.Count, sizeof(uint), ComputeBufferType.IndirectArguments);
    //                    cig.argBuffer.SetData(argArray);
    //                    cig.dataBuffer = new ComputeBuffer(instanceInfos.Count, sizeof(float) * 8, ComputeBufferType.Default);
    //                    cig.dataBuffer.SetData(instanceInfos);
    //                    cig.chunkId = i;
    //                    cig.instanceCount = instanceInfos.Count;
    //                    chunkData.Add(cig);
    //                    for (int j = 0; j < cig.instanceRenderBatch.Count; ++j)
    //                    {
    //                        InstanceRenderBatch rb = cig.instanceRenderBatch[j];
    //                        rb.mpbRef.SetBuffer(ShaderManager._InstanceBuffer, cig.dataBuffer);
    //                    }
    //                }
    //            }
    //        }
    //    }
    }

    [CanEditMultipleObjects, CustomEditor(typeof(InstanceGroup))]
    public class InstanceGroupEditor : UnityEngineEditor
    {
        Dictionary<Mesh, Mesh> meshMap = new Dictionary<Mesh, Mesh>();
        private void ReplaceMesh(Transform t)
        {
            for (int i = 0; i < t.childCount; ++i)
            {
                var child = t.GetChild(i);
                if (child.TryGetComponent(out MeshFilter mf))
                {
                    var m = mf.sharedMesh;
                    if (meshMap.TryGetValue(m, out var replaceMesh))
                    {
                        mf.sharedMesh = replaceMesh;
                    }
                }
                ReplaceMesh(child);
            }
        }

        public override void OnInspectorGUI()
        {
            InstanceGroup ig = target as InstanceGroup;
            //EditorGUI.BeginChangeCheck();
            //EditorGUILayout.Toggle(ig.preview ? "UnPreview" : "Preview", ig.preview);
            //if (EditorGUI.EndChangeCheck())
            //{
            //    ig.PreviewInstance();
            //}
            for (int i = 0; i < ig.chunkData.Count; ++i)
            {
                var cig = ig.chunkData[i];
                EditorGUILayout.LabelField(string.Format("Chunk {0} Draw Count {1} Instance Count {2}",
                    cig.chunkId, cig.instanceRenderBatch.Count, cig.instanceCount));
                EditorGUI.indentLevel++;

                for (int j = 0; j < cig.instanceRenderBatch.Count; ++j)
                {
                    var irb = cig.instanceRenderBatch[j];
                    EditorGUILayout.LabelField(string.Format("Draw {0} Instance Count {1}",
                        j, irb.layer));
                }
                EditorGUI.indentLevel--;

            }

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Add Replace Mesh", GUILayout.MaxWidth(200)))
            {
                ig.meshReplace.Add(new FbxMeshReplace());
            }
            if (GUILayout.Button("Replace", GUILayout.MaxWidth(100)))
            {
                meshMap.Clear();
                for (int i = 0; i < ig.meshReplace.Count; ++i)
                {
                    var mp = ig.meshReplace[i];
                    meshMap[mp.srcMesh] = mp.replaceMesh;
                }
                ReplaceMesh(ig.transform);
            }
            EditorGUILayout.EndHorizontal();
            int removeIndex = -1;
            for (int i = 0; i < ig.meshReplace.Count; ++i)
            {
                var mp = ig.meshReplace[i];
                EditorGUILayout.BeginHorizontal();
                mp.srcMesh = EditorGUILayout.ObjectField(string.Format("{0}.src", i.ToString()), mp.srcMesh, typeof(Mesh), false) as Mesh;
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.BeginHorizontal();
                mp.replaceMesh = EditorGUILayout.ObjectField("replace", mp.replaceMesh, typeof(Mesh), false) as Mesh;
                if (GUILayout.Button("Delete", GUILayout.MaxWidth(80)))
                {
                    removeIndex = i;
                }
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.Space();

            }
            if (removeIndex >= 0)
            {
                ig.meshReplace.RemoveAt(removeIndex);
            }
        }
    }
}
#endif