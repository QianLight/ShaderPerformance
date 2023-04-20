#if UNITY_EDITOR
using Unity.Mathematics;
using static Unity.Mathematics.math;
using UnityEditor;
using UnityEngine;

namespace CFEngine
{
    [ExecuteAlways]
    public class BoundsRuler : EditorMonoComponetSelectObject<BoundsRuler>
    {
        protected GameObject _obj;
        public GameObject Obj
        {
            get => _obj;
            set => _obj = value;
        }
        
        private Bounds _bounds;
        public float lenght, width, height;
        // Start is called before the first frame update

        private void OnEnable()
        {
            EditorApplication.update += Measure;
        }

        private void OnDisable()
        {
            EditorApplication.update -= Measure;
        }

        private void Measure()
        {
            if (_obj)
            {
                
                SkinnedMeshRenderer[] meshRenderers = _obj.GetComponentsInChildren<SkinnedMeshRenderer>();
                MeshFilter[] meshFilters = _obj.GetComponentsInChildren<MeshFilter>();
                if (meshRenderers.Length > 0)
                {
                    foreach (var meshRenderer in meshRenderers)
                    {
                        if (meshRenderer.isVisible)
                        {
                            if (_bounds == default)
                            {
                                _bounds = CalculateBounds(meshRenderer.sharedMesh, meshRenderer.transform);
                            }
                            else
                            {
                                _bounds.Encapsulate(CalculateBounds(meshRenderer.sharedMesh, meshRenderer.transform));
                            }
                        }
                    }

                }

                if (meshFilters.Length > 0)
                {
                    foreach (var meshFilter in meshFilters)
                    {
                        MeshRenderer meshRenderer = meshFilter.GetComponent<MeshRenderer>();
                        if (meshRenderer && meshRenderer.enabled)
                        {
                            if (_bounds == default)
                            {
                                _bounds = CalculateBounds(meshFilter.sharedMesh, meshRenderer.transform);
                            }
                            else
                            {
                                _bounds.Encapsulate(CalculateBounds(meshFilter.sharedMesh, meshRenderer.transform));
                            }
                        }
                    }
                }
                
                lenght = _bounds.size.x;
                width = _bounds.size.z;
                height = _bounds.size.y;
                
            }
        }

        Bounds CalculateBounds(Mesh mesh, Transform trans)
        {
            Matrix4x4 objToWorldMat;
            objToWorldMat = trans.localToWorldMatrix;
            Bounds bounds = mesh.bounds;
            bounds.center = (Vector4)math.mul(objToWorldMat,float4(bounds.center,1f));
            bounds.size = (Vector4) mul(objToWorldMat,float4(bounds.size,0));
            return bounds;
        }
        
        
        private void OnDrawGizmos()
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireCube(_bounds.center,_bounds.size);
        }

        public override void Init(GameObject selectedGameObject = null)
        {
            Obj = selectedGameObject;
            gameObject.name = "Bounds Ruler";
        }

        public override void DrawGUI()
        {
            // GUILayout.FlexibleSpace();
            GUILayout.BeginHorizontal();
            {
                GUILayout.BeginVertical();
                {
                    EditorGUILayout.LabelField("Length(长) x: ");
                    EditorGUILayout.LabelField("Width(宽) z: ");
                    EditorGUILayout.LabelField("Height(高) y: ");
                }
                GUILayout.EndVertical();
                
                GUILayout.BeginVertical();
                {
                    EditorGUILayout.TextField(lenght + "m");
                    EditorGUILayout.TextField(width + "m");
                    EditorGUILayout.TextField(height + "m");
                }
                GUILayout.EndVertical();
                
            }
            GUILayout.EndHorizontal();
        }
        
        public override void Destroy()
        {
            if (Application.isPlaying)
            {
                Destroy(gameObject);
            }
            else
            {
                DestroyImmediate(gameObject);
            }
        }

        public override string Name()
        {
            return this.name;
        }
    }
}

#endif