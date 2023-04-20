#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngineEditor = UnityEditor.Editor;

namespace CFEngine
{
    [DisallowMultipleComponent, ExecuteInEditMode]
    public class ImpostorBillboard : MonoBehaviour
    {
        public Mesh mesh;
        public Material mat;
        public Material mattemp;
        public Bounds aabb;

        private ComputeBuffer argBuffer;
        private ComputeBuffer dataBuffer;
        private uint instanceCount;
        public  List<Impostorboard> tmpParam = new List<Impostorboard>();
        protected MaterialPropertyBlock mpb;


        private void Update()
        {
            //if (mesh != null && mattemp != null && argBuffer != null && mpb != null)
            //{ 
            //    mattemp.CopyPropertiesFromMaterial(mat);
            //    mattemp.EnableKeyword("_INSTANCE");
            //    Graphics.DrawMeshInstancedIndirect(mesh, 0, mattemp, aabb, argBuffer, 0, mpb);
            //}
        }

        public void refresh()
        {
            FindChild(tmpParam);
            mesh= RuntimeUtilities.BillboardQuad;

            instanceCount = (uint)tmpParam.Count;
            if (instanceCount > 0)
            {
                argBuffer = new ComputeBuffer(5, sizeof(uint), ComputeBufferType.IndirectArguments);
            
                EditorCommon.argArray[0] = mesh.GetIndexCount(0);
                EditorCommon.argArray[1] = instanceCount;
                argBuffer.SetData(EditorCommon.argArray);
                if (mpb == null)
                {
                    mpb = new MaterialPropertyBlock();
                }
                dataBuffer = new ComputeBuffer((int)instanceCount, Impostorboard.GetSize(), ComputeBufferType.Constant);
                dataBuffer.SetData(tmpParam);
                mpb.SetConstantBuffer(ShaderManager.bilboardArray, dataBuffer, 0, (int)instanceCount * Impostorboard.GetSize());
                mattemp = new Material(mat);
                mattemp.EnableKeyword("_INSTANCE");
                //-------TEST
                //if (mesh.vertexCount > 0)
                //{
                    //for (int i = 0; i < mesh.vertexCount; i++)
                    //{
                    //    Debug.Log(mesh.vertices[i]);
                    //}
                    //List<Vector2> _uvs = new List<Vector2>();
                    //mesh.GetUVs(0, _uvs);
                    //for (int i = 0; i < _uvs.Count; i++)
                    //{
                    //    Debug.Log(_uvs[i]);
                    //}
                    //for (int i = 0; i < mesh.triangles.Length; i++)
                    //{
                    //    Debug.Log("triangles" + mesh.triangles[i]);
                    //}
                    //int[] indiches = new int[6];
                    //indiches = mesh.GetIndices(0);
                    //for (int i = 0; i < indiches.Length; i++)
                    //{
                    //    Debug.Log(" indiches " + indiches[i]);
                    //}
                   // Debug.Log(mesh.GetIndexCount(0));

                //}
            }
        }


        public void FindChild(List<Impostorboard> instanceParam)
        {
            //if (instanceData != null)
            //    instanceData.Clear();
            if (instanceParam != null)
                instanceParam.Clear();
            var t = this.transform;
            if (t.childCount > 0)
            {

                var child = t.GetChild(0);
                if (child.TryGetComponent<MeshRenderer>(out var mr) &&
                    child.TryGetComponent<MeshFilter>(out var mf))
                {
                    mesh = mf.sharedMesh;
                    mat = mr.sharedMaterial;
                }
            }
            if (instanceParam != null)
                FindChild(t,instanceParam);
        }

   
        public void FindChild(Transform t, List<Impostorboard> instanceParam)
        {
            if (t.TryGetComponent<MeshRenderer>(out var mr) &&
                t.TryGetComponent<MeshFilter>(out var mf))
            {
                var mesh = mf.sharedMesh;
                var mat = mr.sharedMaterial;
                if (this.mesh == mesh && this.mat == mat && mr.enabled)
                {
                    var pos = t.position;
                    var martrix = t.worldToLocalMatrix;

               //     var scale = t.localScale;
                    if (instanceParam.Count == 0)
                    {
                        aabb = mr.bounds;
                    }
                    else
                    {
                        aabb.Encapsulate(mr.bounds);
                    }
                    instanceParam.Add(new Impostorboard()
                    {
                        param = new Vector4(pos.x, pos.y, pos.z, 1),             
                        WTLmatrixs = martrix,
                        LTWmatrixs = t.localToWorldMatrix,
                        // rot = new Vector4(rot.x, rot.y, rot.z, rot.w)
                    }) ;
                }
            }
            for (int i = 0; i < t.childCount; ++i)
            {
                FindChild(t.GetChild(i), instanceParam);
            }
        }
    }
    public class SFXImpostorBillboardDataProcessor : SFXProcessData
    {
        public override SFXData Process(Transform t, SFXData parent, out float duration, out bool processChind)
        {
            duration = 0;
            processChind = false;
            if (t.TryGetComponent<ImpostorBillboard>(out var sga))
            {
                SFXData sfxData = new ImpostorBillboardData();
                sfxData.Refresh(t, sga, parent, 0, out var d);
                return sfxData;
            }
            return null;
        }
    }

    public partial class ImpostorBillboardData : SFXData
    {
        private ImpostorBillboard sa;
        private ComputeBuffer argBuffer;
        private ComputeBuffer dataBuffer;
        private uint instanceCount;
        public static List<Impostorboard> tmpParam = new List<Impostorboard>();

        public override void OnUpdate(float time, float deltaTime, bool restart, bool lockTime)
        {
            if (sa != null && sa.mesh != null && material != null && argBuffer != null)
            {
                Graphics.DrawMeshInstancedIndirect(sa.mesh, 0, material, sa.aabb, argBuffer, 0, mpb);
            }
        }
        public override void Refresh(Transform t, Component comp, SFXData parent, float time,
      out float duration)
        {
            base.Refresh(t, comp, parent, time, out duration);
            sa = comp as ImpostorBillboard;
            if (sa != null && sa.mesh != null && sa.mat != null)
            {
                Reset();
                sa.FindChild(tmpParam);
                sa.mesh = RuntimeUtilities.BillboardQuad;
                instanceCount = (uint)tmpParam.Count;
                if (instanceCount > 0)
                {
                    argBuffer = new ComputeBuffer(5, sizeof(uint), ComputeBufferType.IndirectArguments);

                    EditorCommon.argArray[0] = sa.mesh.GetIndexCount(0);
                    EditorCommon.argArray[1] = instanceCount;
                    argBuffer.SetData(EditorCommon.argArray);
                    if (mpb == null)
                    {
                        mpb = new MaterialPropertyBlock();
                    }
                    dataBuffer = new ComputeBuffer((int)instanceCount, Impostorboard.GetSize(), ComputeBufferType.Constant);
                    dataBuffer.SetData(tmpParam);
                    mpb.SetConstantBuffer(ShaderManager.bilboardArray, dataBuffer, 0, (int)instanceCount * Impostorboard.GetSize());
                    material = new Material (sa.mat);
                    material.EnableKeyword("_INSTANCE");

                }

            }
        }

        public override void Refresh(float time, out float duration)
        {
            duration = -1;

        }

        public override void Reset()
        {
            if (material != null)
            {
                UnityEngine.Object.DestroyImmediate(material);
                material = null;
            }
            if (dataBuffer != null)
            {
                dataBuffer.Dispose();
                dataBuffer = null;
            }
            if (argBuffer != null)
            {
                argBuffer.Dispose();
                argBuffer = null;
            }
            instanceCount = 0;
        }
    }

    [CustomEditor(typeof(ImpostorBillboard))]
    public class ImpostorBillboardEditor : UnityEngineEditor
    {
        SerializedProperty mesh;
        SerializedProperty mat;
        SerializedProperty mattemp;

        void OnEnable()
        {
            mesh = serializedObject.FindProperty("mesh");
            mat = serializedObject.FindProperty("mat");
            mattemp= serializedObject.FindProperty("mattemp");
        }
        public override void OnInspectorGUI()
        {
            var ga = target as ImpostorBillboard;
            EditorGUILayout.PropertyField(mesh);
            EditorGUILayout.PropertyField(mat);
            EditorGUILayout.PropertyField(mattemp);
            if (GUILayout.Button("Refresh", GUILayout.MaxWidth(80)))
            {
                ga.refresh();

            }
            serializedObject.ApplyModifiedProperties();
        }
    }

}
#endif
