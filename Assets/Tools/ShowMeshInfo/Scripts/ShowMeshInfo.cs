#if UNITY_EDITOR
using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEditor;

[ExecuteInEditMode, DisallowMultipleComponent, AddComponentMenu("ShowMeshInfo")]
public class ShowMeshInfo : MonoBehaviour
{
    public enum Coordinates
    {
        UV1,
        UV2,
        UV3,
        UV4,
        UV5,
        UV6,
        UV7,
        UV8,
    }

    public enum CustomUVComponent
    {
        X,
        Y,
        Z,
        W,
        Black,
    }

    private Mesh _mesh;
    private Mesh _meshLine;
    private Mesh _meshCube;
    [SerializeField] private Material _material;
    private ComputeBuffer _verticesCb;
    private ComputeBuffer _normalsCb;
    private ComputeBuffer _tangentsCb;
    private ComputeBuffer _arg;
    private CommandBuffer _cmdBuffer;

    private Renderer _renderer;
    private bool _isEditing = false;

    public bool AlwaysShow;
    public bool ShowVertices = true;
    public bool ShowNormals = true;
    public bool ShowTangents = false;
    public bool ShowBinormals = false;

    public ShowColorType ColorType = ShowColorType.NotDisplay;

    public Color VertexColor = new Color32(38, 38, 77, 191);
    public Color NormalColor = Color.blue;
    public Color TangentColor = Color.red;
    public Color BinormalColor = Color.green;

    public Coordinates customUVIndex = Coordinates.UV1;
    public CustomUVComponent customUvR = CustomUVComponent.X;
    public CustomUVComponent customUvG = CustomUVComponent.Y;
    public CustomUVComponent customUvB = CustomUVComponent.Z;

    public enum ShowColorType
    {
        NotDisplay = 0,
        VertexColorWithAlpha = 3,
        VertexAlpha = 15,
        VertexColor = 16,
        LocalSpaceNormal = 5,
        LocalSpaceTangent = 6,
        LocalSpaceBinormal = 7,
        TangentSpaceNormal = 8,
        TangentSpaceTangent = 9,
        TangentSpaceBinormal = 10,
        UV = 11,
        UV2 = 12,
        UV3 = 13,
        UV4 = 14,
        CustomUV = 17,
    }

    void OnEnable()
    {
        _renderer = null;
        _mesh = null;
        _meshLine = null;
        _material = null;
        _isEditing = true;
    }

    void OnDisable()
    {
        _isEditing = false;
        Release();
    }

    void Release()
    {
        if (_arg != null)
        {
            _arg.Release();
            _arg = null;
        }

        if (_cmdBuffer != null)
        {
            _cmdBuffer.Release();
            _cmdBuffer = null;
        }

        if (_verticesCb != null)
        {
            _verticesCb.Release();
            _verticesCb = null;
        }

        if (_normalsCb != null)
        {
            _normalsCb.Release();
            _normalsCb = null;
        }

        if (_tangentsCb != null)
        {
            _tangentsCb.Release();
            _tangentsCb = null;
        }
    }

    void OnDestroy()
    {
        Release();
    }

    private void OnDrawGizmosSelected()
    {
        if (!AlwaysShow)
        {
            DrawGizmos();
        }
    }

    void OnDrawGizmos()
    {
        if (AlwaysShow)
        {
            DrawGizmos();
        }
    }

    void DrawGizmos()
    {
        if (!_isEditing) return;

        if (_renderer == null)
        {
            _renderer = GetComponent<Renderer>();
        }

        if (_mesh == null)
        {
            var mf = GetComponent<MeshFilter>();
            if (mf)
            {
                _mesh = mf.sharedMesh;
            }
            else
            {
                var smr = GetComponent<SkinnedMeshRenderer>();
                if (smr)
                {
                    _mesh = smr.sharedMesh;
                }
            }
        }

        if (_mesh && _arg == null)
        {
            _arg = new ComputeBuffer(1, 5 * sizeof(uint), ComputeBufferType.IndirectArguments);
            _arg.SetData(new uint[5] {_mesh.GetIndexCount(0), (uint) _mesh.vertexCount, 0, 0, 0});
        }

        if (_meshLine == null)
        {
            _meshLine = new Mesh();
            _meshLine.vertices = new Vector3[2] {Vector3.zero, Vector3.zero};
            _meshLine.uv = new Vector2[2] {Vector2.one, Vector2.zero};
            _meshLine.SetIndices(new int[2] {0, 1}, MeshTopology.Lines, 0);
        }

        if (_meshCube == null)
        {
            float l = 0.5f;
            var p = new Vector3[]
            {
                new Vector3(-l, -l, l),
                new Vector3(l, -l, l),
                new Vector3(l, -l, -l),
                new Vector3(-l, -l, -l),

                new Vector3(-l, l, l),
                new Vector3(l, l, l),
                new Vector3(l, l, -l),
                new Vector3(-l, l, -l),
            };

            _meshCube = new Mesh();
            _meshCube.vertices = new Vector3[]
            {
                p[0], p[1], p[2], p[3],
                p[7], p[4], p[0], p[3],
                p[4], p[5], p[1], p[0],
                p[6], p[7], p[3], p[2],
                p[5], p[6], p[2], p[1],
                p[7], p[6], p[5], p[4],
            };
            _meshCube.SetIndices(new int[]
            {
                3, 1, 0, 3, 2, 1,
                7, 5, 4, 7, 6, 5,
                11, 9, 8, 11, 10, 9,
                15, 13, 12, 15, 14, 13,
                19, 17, 16, 19, 18, 17,
                23, 21, 20, 23, 22, 21,
            }, MeshTopology.Triangles, 0);
        }

        if (_material == null)
        {
            _material = new Material(Shader.Find("Hidden/ShowMeshInfo/ShowMeshShader"));
            if (_material && _mesh)
            {
                if (_verticesCb == null)
                {
                    _verticesCb = new ComputeBuffer(_mesh.vertices.Length, 12);
                    _verticesCb.SetData(_mesh.vertices);
                    _material.SetBuffer("_Points", _verticesCb);
                }

                if (_mesh.normals.Length > 0)
                {
                    if (_normalsCb == null)
                    {
                        _normalsCb = new ComputeBuffer(_mesh.normals.Length, 12);
                        _normalsCb.SetData(_mesh.normals);
                        _material.SetBuffer("_Normals", _normalsCb);
                    }
                }

                if (_mesh.tangents.Length > 0)
                {
                    if (_tangentsCb == null)
                    {
                        _tangentsCb = new ComputeBuffer(_mesh.tangents.Length, 16);
                        _tangentsCb.SetData(_mesh.tangents);
                        _material.SetBuffer("_Tangents", _tangentsCb);
                    }
                }

                Debug.Log("+++++++++++++++++++++++++++++++++++++++++++++++");
                Debug.Log("Mesh Name = " + _mesh.name);
                Debug.Log("Vertex Count = " + _mesh.vertices.Length);
                Debug.Log("Normal Count = " + _mesh.normals.Length);
                Debug.Log("Tangent Count = " + _mesh.tangents.Length);
                Debug.Log("Color Count = " + _mesh.colors.Length);
                Debug.Log("UV Count = " + _mesh.uv.Length);
                Debug.Log("UV2 Count = " + _mesh.uv2.Length);
                Debug.Log("UV3 Count = " + _mesh.uv3.Length);
                Debug.Log("UV4 Count = " + _mesh.uv4.Length);
                Debug.Log("Triangle Count = " + _mesh.triangles.Length);
                Debug.Log("BindPose Count = " + _mesh.bindposes.Length);
                Debug.Log("Bone Weight Count = " + _mesh.boneWeights.Length);
                Debug.Log("Blend Shape Count = " + _mesh.blendShapeCount);
                Debug.Log("+++++++++++++++++++++++++++++++++++++++++++++++");

                _material.SetFloat("_VertexSize", 0.0075f);
                _material.SetFloat("_NormalSize", 0.1f);
                _material.SetFloat("_TangentSize", 0.075f);
                _material.SetFloat("_BinormalSize", 0.06f);

                _material.SetColor("_VertexColor", VertexColor);
                _material.SetColor("_NormalColor", NormalColor);
                _material.SetColor("_TangentColor", TangentColor);
                _material.SetColor("_BinormalColor", BinormalColor);
            }
        }

        void SetMask(string property, int index)
        {
            Vector4 mask = Vector4.zero;
            if (index >= 0 && index < 4)
                mask[index] = 1;
            _material.SetVector(property, mask);
        }

        if ((int) customUVIndex < 4)
            SetMask("_CustomUVIndexMask0", (int) customUVIndex);
        else
            SetMask("_CustomUVIndexMask1", (int) customUVIndex - 4);
        SetMask("_CustomUVMaskX", (int) customUvR);
        SetMask("_CustomUVMaskY", (int) customUvG);
        SetMask("_CustomUVMaskZ", (int) customUvB);

        if (_cmdBuffer == null)
        {
            _cmdBuffer = new CommandBuffer();
        }

        _cmdBuffer.Clear();

        if (_renderer && _mesh)
        {
            if ((int) ColorType > 0)
            {
                for (var i = 0; i < _mesh.subMeshCount; i++)
                {
                    _cmdBuffer.DrawRenderer(_renderer, _material, i, (int) ColorType);
                }
            }
        }

        _material.SetMatrix("_Transform", transform.localToWorldMatrix);

        if (_meshCube)
        {
            if (ShowVertices)
            {
                _cmdBuffer.DrawMeshInstancedIndirect(_meshCube, 0, _material, 4, _arg);
            }
        }

        if (_meshLine)
        {
            if (ShowNormals)
            {
                _cmdBuffer.DrawMeshInstancedIndirect(_meshLine, 0, _material, 0, _arg);
            }

            if (ShowTangents)
            {
                _cmdBuffer.DrawMeshInstancedIndirect(_meshLine, 0, _material, 1, _arg);
            }

            if (ShowBinormals)
            {
                _cmdBuffer.DrawMeshInstancedIndirect(_meshLine, 0, _material, 2, _arg);
            }
        }

        Graphics.ExecuteCommandBuffer(_cmdBuffer);
    }
}

[CustomEditor(typeof(ShowMeshInfo))]
public class ShowMeshInfoEditor : Editor
{
    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        PropertyField(nameof(ShowMeshInfo.AlwaysShow));
        PropertyField(nameof(ShowMeshInfo.ShowVertices));
        ToggleWithColor(nameof(ShowMeshInfo.ShowNormals), nameof(ShowMeshInfo.NormalColor));
        ToggleWithColor(nameof(ShowMeshInfo.ShowTangents), nameof(ShowMeshInfo.TangentColor));
        ToggleWithColor(nameof(ShowMeshInfo.ShowBinormals), nameof(ShowMeshInfo.BinormalColor));
        SerializedProperty colorTypeSP = PropertyField(nameof(ShowMeshInfo.ColorType));
        if (colorTypeSP != null)
        {
            string[] names = Enum.GetNames(typeof(ShowMeshInfo.ShowColorType));
            int customUVTypeIndex = Array.IndexOf(names, nameof(ShowMeshInfo.ShowColorType.CustomUV));
            if (colorTypeSP.enumValueIndex == customUVTypeIndex)
            {
                EditorGUI.indentLevel++;
                PropertyField(nameof(ShowMeshInfo.customUVIndex));
                PropertyField(nameof(ShowMeshInfo.customUvR));
                PropertyField(nameof(ShowMeshInfo.customUvG));
                PropertyField(nameof(ShowMeshInfo.customUvB));
                EditorGUI.indentLevel--;
            }
        }

        serializedObject.ApplyModifiedProperties();
    }

    private void ToggleWithColor(string toggleName, string colorName)
    {
        SerializedProperty toggleSP = serializedObject.FindProperty(toggleName);
        SerializedProperty colorSP = serializedObject.FindProperty(colorName);
        
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField(toggleSP.displayName, GUILayout.Width(EditorGUIUtility.labelWidth));
        EditorGUILayout.PropertyField(toggleSP, GUIContent.none, GUILayout.Width(15));
        EditorGUILayout.PropertyField(colorSP, GUIContent.none);
        EditorGUILayout.EndHorizontal();
    }

    private SerializedProperty PropertyField(string propertyName)
    {
        SerializedProperty serializedProperty = serializedObject.FindProperty(propertyName);
        EditorGUILayout.PropertyField(serializedProperty);
        return serializedProperty;
    }
}

#endif