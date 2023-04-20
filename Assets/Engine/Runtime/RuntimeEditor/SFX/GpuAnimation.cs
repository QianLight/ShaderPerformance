#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngineEditor = UnityEditor.Editor;
namespace CFEngine
{
    [DisallowMultipleComponent, ExecuteInEditMode]
    public class GpuAnimation : MonoBehaviour
    {
        public Mesh mesh;
        public Material mat;
        public float duration = 1;

        public Bounds aabb;
        public void FindChild (List<InstanceInfo> instanceData, List<GpuAnimParam> instanceParam)
        {
            if (instanceData != null)
                instanceData.Clear ();
            if (instanceParam != null)
                instanceParam.Clear ();
            var t = this.transform;
            if (t.childCount > 0)
            {
                var child = t.GetChild (0);
                if (child.TryGetComponent<MeshRenderer> (out var mr) &&
                    child.TryGetComponent<MeshFilter> (out var mf))
                {
                    mesh = mf.sharedMesh;
                    mat = mr.sharedMaterial;
                }
            }
            if (instanceData != null && instanceParam != null)
                FindChild (t, instanceData, instanceParam);
        }
        public void FindChild (Transform t, List<InstanceInfo> instanceData, List<GpuAnimParam> instanceParam)
        {
            if (t.TryGetComponent<MeshRenderer> (out var mr) &&
                t.TryGetComponent<MeshFilter> (out var mf) &&
                t.TryGetComponent<SetGPUAnimaParam> (out var param))
            {
                var mesh = mf.sharedMesh;
                var mat = mr.sharedMaterial;
                if (this.mesh == mesh && this.mat == mat && mr.enabled)
                {
                    var pos = t.localPosition;
                    var rot = t.localRotation;
                    var scale = t.localScale;
                    if (instanceData.Count == 0)
                    {
                        aabb = mr.bounds;
                    }
                    else
                    {
                        aabb.Encapsulate (mr.bounds);
                    }
                    instanceData.Add (new InstanceInfo ()
                    {
                        posScale = new Vector4 (pos.x, pos.y, pos.z, scale.x),
                            rot = new Vector4 (rot.x, rot.y, rot.z, rot.w)
                    });
                    instanceParam.Add (new GpuAnimParam ()
                    {
                        param = new Vector4 (param._DelayTime, 0, 0, 0),
                    });
                }
            }
            for (int i = 0; i < t.childCount; ++i)
            {
                FindChild (t.GetChild (i), instanceData, instanceParam);
            }
        }

    }
    public class SFXGpuAnimationDataProcessor : SFXProcessData
    {
        public override SFXData Process(Transform t, SFXData parent, out float duration, out bool processChind)
        {
            duration = 0;
            processChind = false;
            if (t.TryGetComponent<GpuAnimation>(out var sga))
            {
                SFXData sfxData = new GpuAnimationData();
                sfxData.Refresh(t, sga, parent, 0, out var d);
                return sfxData;
            }
            return null;
        }
    }
    public partial class GpuAnimationData : SFXData
    {
        private GpuAnimation ga;
        private ComputeBuffer argBuffer;
        private ComputeBuffer dataBuffer;
        private ComputeBuffer paramBuffer;
        private uint instanceCount;
        private static List<InstanceInfo> tmpData = new List<InstanceInfo> ();
        public static List<GpuAnimParam> tmpParam = new List<GpuAnimParam> ();
        public override void OnUpdate (float time, float deltaTime, bool restart, bool lockTime)
        {
            if (ga != null && ga.mesh != null && material != null && argBuffer != null)
            {
                ga.aabb.center = this.t.position;
                var matrix = this.t.localToWorldMatrix;
                mpb.SetMatrix(ShaderManager.custom_ObjectToWorld, matrix);
                mpb.SetVector (ShaderManager._CustomTime, new Vector4 (time / ga.duration, 0, 0, 0));
                Graphics.DrawMeshInstancedIndirect (ga.mesh, 0, material, ga.aabb, argBuffer, 0, mpb);
            }
        }

        public override void Refresh (Transform t, Component comp, SFXData parent, float time,
            out float duration)
        {
            base.Refresh (t, comp, parent, time, out duration);
            ga = comp as GpuAnimation;
            if (ga != null && ga.mesh != null && ga.mat != null)
            {
                duration = ga.duration;
                Reset ();

                ga.FindChild (tmpData, tmpParam);
                instanceCount = (uint) tmpData.Count;
                if (instanceCount > 0)
                {
                    argBuffer = new ComputeBuffer (5, sizeof (uint), ComputeBufferType.IndirectArguments);

                    EditorCommon.argArray[0] = ga.mesh.GetIndexCount (0);
                    EditorCommon.argArray[1] = instanceCount;
                    argBuffer.SetData (EditorCommon.argArray);
                    if (mpb == null)
                    {
                        mpb = new MaterialPropertyBlock ();
                    }
                    dataBuffer = new ComputeBuffer ((int) instanceCount, InstanceInfo.GetSize (), ComputeBufferType.Constant);
                    dataBuffer.SetData (tmpData);
                    mpb.SetConstantBuffer (ShaderManager.prsArray, dataBuffer, 0, (int) instanceCount * InstanceInfo.GetSize ());

                    paramBuffer = new ComputeBuffer ((int) instanceCount, GpuAnimParam.GetSize (), ComputeBufferType.Constant);
                    paramBuffer.SetData (tmpParam);
                    mpb.SetConstantBuffer (ShaderManager.paramArray, paramBuffer, 0, (int) instanceCount * GpuAnimParam.GetSize ());

                    material = ga.mat;//new Material (ga.mat);
                    //material.hideFlags = HideFlags.DontSave;
                    material.EnableKeyword ("_INSTANCE");
                    material.EnableKeyword ("_BONE_2");
                }

            }
        }

        public override void Refresh (float time, out float duration)
        {
            duration = -1;
            if (ga != null && ga.mesh != null && ga.mat != null)
            {
                duration = ga.duration;
            }
        }

        public override void Reset ()
        {
            //if (material != null)
            //{
            //    UnityEngine.Object.DestroyImmediate (material);
            //    material = null;
            //}
            if (dataBuffer != null)
            {
                dataBuffer.Dispose ();
                dataBuffer = null;
            }
            if (argBuffer != null)
            {
                argBuffer.Dispose ();
                argBuffer = null;
            }
            instanceCount = 0;
        }
    }

    [CustomEditor (typeof (GpuAnimation))]
    public class GpuAnimationEditor : UnityEngineEditor
    {
        SerializedProperty mesh;
        SerializedProperty mat;
        SerializedProperty duration;

        void OnEnable ()
        {
            mesh = serializedObject.FindProperty ("mesh");
            mat = serializedObject.FindProperty ("mat");
            duration = serializedObject.FindProperty ("duration");
        }
        public override void OnInspectorGUI ()
        {
            var ga = target as GpuAnimation;
            EditorGUILayout.PropertyField (mesh);
            EditorGUILayout.PropertyField (mat);
            EditorGUILayout.PropertyField (duration);
            if (GUILayout.Button ("Refresh", GUILayout.MaxWidth (80)))
            {
                ga.FindChild (null, null);
            }
            serializedObject.ApplyModifiedProperties ();
        }
    }
}
#endif