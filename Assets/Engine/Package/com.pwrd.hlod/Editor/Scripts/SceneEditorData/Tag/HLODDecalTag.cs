using System;
using System.Collections.Generic;
using com.pwrd.hlod.editor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Serialization;

namespace com.pwrd.hlod.editor
{
    [Serializable]
    public class DecalData
    {
        public Matrix4x4 trans;
        public Texture tex;
        public Vector4 offsetScale;
        public Vector4 uvArea;
        public Vector3 tangent;
        public Vector3 normal;
        public Renderer renderer;
        public Bounds bounds;
    }

    public class HLODDecalTag : MonoBehaviour
    {
#if UNITY_EDITOR
        public bool useCustomed;
        public DecalData decalData;

        public List<Vector4> debuglist = new List<Vector4>();

        public DecalData GetDecalData()
        {
            if (!useCustomed)
            {
                CalcDecalData();
            }

            decalData.renderer = GetComponent<Renderer>();

            return decalData;
        }

        /// <summary>
        /// 对已经是网格的贴花对象使用,自动调整DecalData
        /// </summary>
        [ContextMenu("CalcDecalData")]
        public void CalcDecalData()
        {
            var renderer = GetComponent<Renderer>();
            var mat = renderer.sharedMaterial;
            var meshFilter = GetComponent<MeshFilter>();
            var mesh = meshFilter.sharedMesh;
            //1.通过法线,计算正面,调整包围盒时,正面法线方向缩放保持为1
            //法线应该在y方向, 即(0,1,0) 才正确
            var normal = GetNoraml(mesh);

            //2.计算网格的包围盒,得出一个缩放矩阵,与world2locale矩阵合并.
            var scale = GetScale(mesh, normal);

            //tnt
            var btnMatirx = CalcBTNMatrix(mesh);
            var trans = transform.localToWorldMatrix * (Matrix4x4.Scale(scale) * btnMatirx);
            trans = trans.inverse;

            //3.计算uv偏移 要同时考虑模型的uv和材质球的uv偏移.de
            var os = GetUVArea(mesh, mat);

            //4.计算世界坐标系的法线
            normal = (Vector3) (this.transform.localToWorldMatrix * new Vector4(normal.x, normal.y, normal.z, 1)) - this.transform.position;
            normal.Normalize();

            //.5计算世界坐标切线坐标
            var tangant = new Vector3(1, 0, 0);
            tangant = (Vector3) (this.transform.localToWorldMatrix * new Vector4(tangant.x, tangant.y, tangant.z, 1)) - this.transform.position;
            tangant.Normalize();

            var tex = mat.mainTexture;

            var bounds = GetBounds(trans);

            decalData = new DecalData()
            {
                trans = trans,
                tex = tex,
                offsetScale = os.Item1,
                uvArea = os.Item2,
                normal = normal,
                tangent = tangant,
                renderer = renderer,
                bounds = bounds,
            };
        }

        private Bounds GetBounds(Matrix4x4 trans)
        {
            debuglist.Clear();
            trans = trans.inverse;
            var list = new List<Vector4>()
            {
                new Vector4(0.5f, 0.5f, 0.5f, 1),
                new Vector4(-0.5f, 0.5f, 0.5f, 1),
                new Vector4(0.5f, -0.5f, 0.5f, 1),
                new Vector4(-0.5f, -0.5f, 0.5f, 1),
                new Vector4(0.5f, 0.5f, -0.5f, 1),
                new Vector4(-0.5f, 0.5f, -0.5f, 1),
                new Vector4(0.5f, -0.5f, -0.5f, 1),
                new Vector4(-0.5f, -0.5f, -0.5f, 1),
                new Vector4(-0, 0f, 0f, 1),
            };

            var min = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
            var max = new Vector3(float.MinValue, float.MinValue, float.MinValue);

            foreach (var point in list)
            {
                var pos = trans * point;
                pos /= pos.w;
                min.x = Mathf.Min(min.x, pos.x);
                min.y = Mathf.Min(min.y, pos.y);
                min.z = Mathf.Min(min.z, pos.z);
                max.x = Mathf.Max(max.x, pos.x);
                max.y = Mathf.Max(max.y, pos.y);
                max.z = Mathf.Max(max.z, pos.z);
                debuglist.Add(pos);
            }

            return new Bounds((max + min) / 2, max - min);
        }

        private Matrix4x4 CalcBTNMatrix(Mesh mesh)
        {
            var triangles = mesh.triangles;
            var uvs = mesh.uv;
            var vertices = mesh.vertices;
            var normal = mesh.normals;
            var btnMatrix = Matrix4x4.identity;
            for (int i = 0; i < 1 && i + 2 < triangles.Length; i++)
            {
                var v1 = new VertexInfo()
                {
                    x = vertices[i].x,
                    y = vertices[i].y,
                    z = vertices[i].z,
                    tu = uvs[i].x,
                    tv = uvs[i].y,
                };
                i++;
                var v2 = new VertexInfo()
                {
                    x = vertices[i].x,
                    y = vertices[i].y,
                    z = vertices[i].z,
                    tu = uvs[i].x,
                    tv = uvs[i].y,
                };
                i++;
                var v3 = new VertexInfo()
                {
                    x = vertices[i].x,
                    y = vertices[i].y,
                    z = vertices[i].z,
                    tu = uvs[i].x,
                    tv = uvs[i].y,
                };

                VertexInfo t = new VertexInfo(), b = new VertexInfo();

                CalculateTangentBinormal(v1, v2, v3, ref t, ref b);

                var n = normal[i];

                btnMatrix = new Matrix4x4((-new Vector4(t.x, t.y, t.z)).normalized, n.normalized, -new Vector4(b.x, b.y, b.z).normalized,  new Vector4(0,0,0,1));
            }

            return btnMatrix;
        }

        private class VertexInfo
        {
            public float x, y, z;
            public float tu, tv;
        }

        void CalculateTangentBinormal(VertexInfo vertex1, VertexInfo vertex2, VertexInfo vertex3,
            ref VertexInfo tangent, ref VertexInfo binormal)
        {
            float[] vector1 = new float[3], vector2 = new float[3];
            float[] tuVector = new float[2], tvVector = new float[2];
            float den;
            float length;


            // Calculate the two vectors for this face.
            vector1[0] = vertex2.x - vertex1.x;
            vector1[1] = vertex2.y - vertex1.y;
            vector1[2] = vertex2.z - vertex1.z;

            vector2[0] = vertex3.x - vertex1.x;
            vector2[1] = vertex3.y - vertex1.y;
            vector2[2] = vertex3.z - vertex1.z;

            // Calculate the tu and tv texture space vectors.
            tuVector[0] = vertex2.tu - vertex1.tu;
            tvVector[0] = vertex2.tv - vertex1.tv;

            tuVector[1] = vertex3.tu - vertex1.tu;
            tvVector[1] = vertex3.tv - vertex1.tv;

            // Calculate the denominator of the tangent/binormal equation.
            den = 1.0f / (tuVector[0] * tvVector[1] - tuVector[1] * tvVector[0]);

            // Calculate the cross products and multiply by the coefficient to get the tangent and binormal.
            tangent.x = (tvVector[1] * vector1[0] - tvVector[0] * vector2[0]) * den;
            tangent.y = (tvVector[1] * vector1[1] - tvVector[0] * vector2[1]) * den;
            tangent.z = (tvVector[1] * vector1[2] - tvVector[0] * vector2[2]) * den;

            binormal.x = (tuVector[0] * vector2[0] - tuVector[1] * vector1[0]) * den;
            binormal.y = (tuVector[0] * vector2[1] - tuVector[1] * vector1[1]) * den;
            binormal.z = (tuVector[0] * vector2[2] - tuVector[1] * vector1[2]) * den;

            // Calculate the length of this normal.
            length = Mathf.Sqrt((tangent.x * tangent.x) + (tangent.y * tangent.y) + (tangent.z * tangent.z));

            // Normalize the normal and then store it
            tangent.x = tangent.x / length;
            tangent.y = tangent.y / length;
            tangent.z = tangent.z / length;

            // Calculate the length of this normal.
            length = Mathf.Sqrt((binormal.x * binormal.x) + (binormal.y * binormal.y) + (binormal.z * binormal.z));

            // Normalize the normal and then store it
            binormal.x = binormal.x / length;
            binormal.y = binormal.y / length;
            binormal.z = binormal.z / length;
        }


        private Vector3 GetNoraml(Mesh mesh)
        {
            var result = new Vector3();
            foreach (var normal in mesh.normals)
            {
                result += normal;
            }

            return result.normalized;
        }

        private Vector3 GetScale(Mesh mesh, Vector3 normal)
        {
            //根据网格在三个轴的最大最小值得出缩放
            var min = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
            var max = new Vector3(float.MinValue, float.MinValue, float.MinValue);

            foreach (var vertex in mesh.vertices)
            {
                min.x = Mathf.Min(min.x, vertex.x);
                min.y = Mathf.Min(min.y, vertex.y);
                min.z = Mathf.Min(min.z, vertex.z);
                max.x = Mathf.Max(max.x, vertex.x);
                max.y = Mathf.Max(max.y, vertex.y);
                max.z = Mathf.Max(max.z, vertex.z);
            }


            var scale = new Vector3(max.x - min.x, max.y - min.y, max.z - min.z);
            var maxAxis = Mathf.Max(scale.x, scale.y, scale.z);
            maxAxis = Mathf.Min(maxAxis, 1);

            if (Mathf.Abs(normal.x) >= Mathf.Abs(normal.y) && Mathf.Abs(normal.x) >= Mathf.Abs(normal.z))
            {
                scale.x = Mathf.Max(maxAxis, scale.x);
            }

            if (Mathf.Abs(normal.y) >= Mathf.Abs(normal.x) && Mathf.Abs(normal.y) >= Mathf.Abs(normal.z))
            {
                scale.y = Mathf.Max(maxAxis, scale.y);
            }

            if (Mathf.Abs(normal.z) >= Mathf.Abs(normal.x) && Mathf.Abs(normal.z) >= Mathf.Abs(normal.y))
            {
                scale.z = Mathf.Max(maxAxis, scale.z);
            }

            return scale;
        }

        private (Vector4, Vector4) GetUVArea(Mesh mesh, Material mat)
        {
            //网格的uv范围
            var min = new Vector2(float.MaxValue, float.MaxValue);
            var max = new Vector2(float.MinValue, float.MinValue);
            foreach (var uv in mesh.uv)
            {
                min.x = Mathf.Min(min.x, uv.x);
                min.y = Mathf.Min(min.y, uv.y);
                max.x = Mathf.Max(max.x, uv.x);
                max.y = Mathf.Max(max.y, uv.y);
            }

            var area = new Vector4(max.x - min.x, max.y - min.y, min.x, min.y);
            //材质球的ST
            var o = mat.mainTextureOffset;
            var s = mat.mainTextureScale;

            //(uv * os.xy + os.zw)* area.xy + area.zw
            //uv*os.xy*area.xy + os.zw*area.xy + area.zw
            //(os.xy*area.xy, os.zw*area.xy + area.zw)
            var mainTexOS = new Vector4(area.x * s.x, area.y * s.y, o.x * area.x + area.z, o.y * area.y + area.w);

            //把0-1区间的lightmapUV也映射到原贴花网格的uv
            var lightMapOS = area;
            return (mainTexOS, lightMapOS);
        }

        void OnDrawGizmosSelected()
        {
            if (decalData == null)
            {
                return;
                ;
            }

            Gizmos.matrix = decalData.trans.inverse;
            Gizmos.color = Color.yellow;
            UnityEditor.Handles.color = Color.yellow;

            Gizmos.DrawWireCube(Vector3.zero, Vector3.one);

            Gizmos.color = Color.green;

            Gizmos.DrawLine(Vector3.zero, new Vector3(0, 1, 0));
            Gizmos.color = Color.red;

            Gizmos.DrawLine(Vector3.zero, new Vector3(1, 0, 0));
            Gizmos.color = Color.blue;

            Gizmos.DrawLine(Vector3.zero, new Vector3(0, 0, 1));

            UnityEditor.Handles.ArrowHandleCap(0, transform.position, Quaternion.LookRotation(decalData.normal), 2, EventType.Repaint);
            if (HLODProvider.Instance != null && HLODProvider.Instance.data.debug)
            {
                //画bounds
                Gizmos.matrix = Matrix4x4.identity;
                Gizmos.color = Color.magenta;
                Gizmos.DrawCube(decalData.bounds.center, decalData.bounds.size);
            }
        }
#endif
    }
}