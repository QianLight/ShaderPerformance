

using System.Collections.Generic;

using UnityEngine;

namespace CFEngine
{
    /// <summary>
    /// 模型切割脚本
    /// </summary>

    public class SplitObject : MonoBehaviour
    {
        public MeshInfos MeshInfo { get; set; }
        void Start()
        {
           
            if (MeshInfo == null)
            {
                MeshInfo = new MeshInfos(gameObject);
            }
        }
        public void UpdateMesh(params MeshInfos[] info)
        {
            CombineInstance[] coms = new CombineInstance[info.Length];
            for (int i = 0; i < info.Length; i++)
            {
                coms[i].mesh = info[i].GetMesh();
                coms[i].transform = Matrix4x4.identity;
            }
            Mesh mesh = new Mesh();
            try
            {
                mesh.CombineMeshes(coms);
            }
            catch (System.Exception)
            {
                Debug.LogError(name);
            }
         //   mesh.CombineMeshes(coms);
            mesh.RecalculateBounds();
            //mesh.RecalculateNormals();
            GetComponent<MeshFilter>().mesh = mesh;
            if (GetComponent<MeshCollider>())
            {
                GetComponent<MeshCollider>().sharedMesh = mesh;
            }
     

            BoxCollider bc = this.transform.GetComponent<BoxCollider>();
            if (bc != null)
            {
                DestroyImmediate(bc);
            }

            MeshInfo = new MeshInfos(mesh);
            MeshInfo.center = info[0].center;
            MeshInfo.size = info[0].size;
        }

#if UNITY_EDITOR
        public void Splitmulti(Plane plane,bool islasetone=false,bool ISZSPLIT=false)
        {
            if (MeshInfo == null)
            {
                MeshInfo = new MeshInfos(gameObject);
            }

        //    Vector3 point = transform.InverseTransformPoint(plane.normal * -plane.distance);//closed point
          //  Vector3 normal = transform.InverseTransformDirection(plane.normal);
            Vector3 point = plane.normal * -plane.distance;//closed point
            Vector3 normal = plane.normal;
            normal.Scale(transform.localScale);
            normal.Normalize();
            MeshInfos a = new MeshInfos(this.transform);
            MeshInfos b = new MeshInfos(this.transform);
            //
            bool[] above = new bool[MeshInfo.vertices.Count];
            int[] newTriangles = new int[MeshInfo.vertices.Count];

            for (int i = 0; i < newTriangles.Length; i++)
            {
                Vector3 vert = MeshInfo.vertices[i];
                above[i] = Vector3.Dot(vert - point, normal) >= 0f;
                Vector2 _tempuv2=Vector2.zero;
                if (MeshInfo.haveuvs2)
                {
                     _tempuv2 = MeshInfo.uvs2[i];
                }
                Color _tempcolor=Color.black;
                if (MeshInfo.havecolors)
                {
                    _tempcolor = MeshInfo.colors[i];
                }
                if (above[i])
                {
                     newTriangles[i] = a.vertices.Count;                      
                     a.Add(vert, MeshInfo.uvs[i], MeshInfo.normals[i], MeshInfo.tangents[i], _tempuv2, _tempcolor);                         
                }
                else
                {
                    newTriangles[i] = b.vertices.Count;
                    b.Add(vert, MeshInfo.uvs[i], MeshInfo.normals[i], MeshInfo.tangents[i], _tempuv2, _tempcolor);
                }
            }
            List<Vector3> cutPoint = new List<Vector3>();
            int triangleCount = MeshInfo.triangles.Count / 3;
            for (int i = 0; i < triangleCount; i++)
            {
                int _i0 = MeshInfo.triangles[i * 3];
                int _i1 = MeshInfo.triangles[i * 3 + 1];
                int _i2 = MeshInfo.triangles[i * 3 + 2];

                bool _a0 = above[_i0];
                bool _a1 = above[_i1];
                bool _a2 = above[_i2];
                if (_a0 && _a1 && _a2)
                {
                    a.triangles.Add(newTriangles[_i0]);
                    a.triangles.Add(newTriangles[_i1]);
                    a.triangles.Add(newTriangles[_i2]);
                }
                else if (!_a0 && !_a1 && !_a2)
                {
                    b.triangles.Add(newTriangles[_i0]);
                    b.triangles.Add(newTriangles[_i1]);
                    b.triangles.Add(newTriangles[_i2]);
                }
                else
                {
                    int up, down0, down1;
                    if (_a1 == _a2 && _a0 != _a1)
                    {
                        up = _i0;
                        down0 = _i1;
                        down1 = _i2;
                    }
                    else if (_a2 == _a0 && _a1 != _a2)
                    {
                        up = _i1;
                        down0 = _i2;
                        down1 = _i0;
                    }
                    else
                    {
                        up = _i2;
                        down0 = _i0;
                        down1 = _i1;
                    }
                    Vector3 pos0, pos1;
                    if (above[up])
                        SplitTriangle(a, b, point, normal, newTriangles, up, down0, down1, out pos0, out pos1);
                    else
                        SplitTriangle(b, a, point, normal, newTriangles, up, down0, down1, out pos1, out pos0);
                    cutPoint.Add(pos0);
                    cutPoint.Add(pos1);
                }
            }

            a.CombineVertices(0.001f);
            a.center = MeshInfo.center;
            a.size = MeshInfo.size;
            b.CombineVertices(0.001f);
            b.center = MeshInfo.center;
            b.size = MeshInfo.size;

            GameObject apaty = null;
            GameObject bpaty = null;
            if (a.vertices.Count >= 3)
            {
                apaty = Instantiate(gameObject);//X轴计算时（第一次）是小块，Z轴计算时是大块       
                apaty.transform.position = this.transform.position;
                apaty.GetComponent<SplitObject>().UpdateMesh(a);
                apaty.name = apaty.name + "_a";
                apaty.layer = gameObject.layer;
            }
            if (b.vertices.Count >= 3)
            {
                bpaty = Instantiate(gameObject);     
                bpaty.transform.position = this.transform.position;
                bpaty.GetComponent<SplitObject>().UpdateMesh(b);
                bpaty.name = bpaty.name + "_b";
                bpaty.layer = gameObject.layer;
            }
            //需要拆分的

            //第一步准备切割的所有子网格，第二步就不需要了！
            if (!ISZSPLIT)
            {
                if (bpaty)
                {
                    CFEngine.Editor.SplitInput.splitstemp.Enqueue(bpaty.GetComponent<SplitObject>());
                }
                if (apaty)
                {
                    CFEngine.Editor.SplitInput.splitsb.Add(apaty.GetComponent<SplitObject>());
                }
                //加上最后的
                if (islasetone && bpaty)
                {
                    CFEngine.Editor.SplitInput.splitsb.Add(bpaty.GetComponent<SplitObject>());
                }
            }
            else
            {
                if (apaty)
                {
                    CFEngine.Editor.SplitInput.splitstemp.Enqueue(apaty.GetComponent<SplitObject>());
                }
                if (bpaty)
                {
                    CFEngine.Editor.SplitInput.ShowprefabObject.Add(bpaty);
                }
                if (islasetone && apaty)
                {
                    CFEngine.Editor.SplitInput.ShowprefabObject.Add(apaty);
                }
            }
            if (!CFEngine.Editor.SplitInput.NeedDeleteObject.Contains(gameObject))
            {
                CFEngine.Editor.SplitInput.NeedDeleteObject.Add(gameObject);
            }       
        }

#endif
        void SplitTriangle(MeshInfos top, MeshInfos bottom, Vector3 point, Vector3 normal, int[] newTriangles, int up, int down0, int down1, out Vector3 pos0, out Vector3 pos1)
        {
            Vector3 v0 = MeshInfo.vertices[up];
            Vector3 v1 = MeshInfo.vertices[down0];
            Vector3 v2 = MeshInfo.vertices[down1];
            float topDot = Vector3.Dot(point - v0, normal);
            float aScale = Mathf.Clamp01(topDot / Vector3.Dot(v1 - v0, normal));
            float bScale = Mathf.Clamp01(topDot / Vector3.Dot(v2 - v0, normal));
            Vector3 pos_a = v0 + (v1 - v0) * aScale;
            Vector3 pos_b = v0 + (v2 - v0) * bScale;

            Vector2 u0 = MeshInfo.uvs[up];
            Vector2 u1 = MeshInfo.uvs[down0];
            Vector2 u2 = MeshInfo.uvs[down1];
            Vector3 uv_a = (u0 + (u1 - u0) * aScale);
            Vector3 uv_b = (u0 + (u2 - u0) * bScale);


            Vector2 uv2_a = Vector2.zero;
            Vector2 uv2_b = Vector2.zero;
            Color _color_a= Color.black;
            Color _color_b = Color.black;
   

            //uv2
            if (MeshInfo.haveuvs2)
            {
                Vector2 u2_0 = MeshInfo.uvs2[up];
                Vector2 u2_1 = MeshInfo.uvs2[down0];
                Vector2 u2_2 = MeshInfo.uvs2[down1];
                 uv2_a = (u2_0 + (u2_1 - u2_0) * aScale);
                 uv2_b = (u2_0 + (u2_2 - u2_0) * bScale);
            }
            //color
            if (MeshInfo.havecolors)
            {
                Color _color_0 = MeshInfo.colors[up];
                Color _color_1 = MeshInfo.colors[up];
                Color _color_2 = MeshInfo.colors[up];
                 _color_a = (_color_0 + (_color_1 - _color_0) * aScale);
                 _color_b = (_color_0 + (_color_2 - _color_0) * bScale);
            }


            Vector3 n0 = MeshInfo.normals[up];
            Vector3 n1 = MeshInfo.normals[down0];
            Vector3 n2 = MeshInfo.normals[down1];
            Vector3 normal_a = (n0 + (n1 - n0) * aScale).normalized;
            Vector3 normal_b = (n0 + (n2 - n0) * bScale).normalized;

            Vector4 t0 = MeshInfo.tangents[up];
            Vector4 t1 = MeshInfo.tangents[down0];
            Vector4 t2 = MeshInfo.tangents[down1];
            Vector4 tangent_a = (t0 + (t1 - t0) * aScale).normalized;
            Vector4 tangent_b = (t0 + (t2 - t0) * bScale).normalized;
            tangent_a.w = t1.w;
            tangent_b.w = t2.w;

            int top_a = top.vertices.Count;
            top.Add(pos_a, uv_a, normal_a, tangent_a, uv2_a, _color_a);
            int top_b = top.vertices.Count;
            top.Add(pos_b, uv_b, normal_b, tangent_b, uv2_b, _color_b);
            top.triangles.Add(newTriangles[up]);
            top.triangles.Add(top_a);
            top.triangles.Add(top_b);

            int down_a = bottom.vertices.Count;
            bottom.Add(pos_a, uv_a, normal_a, tangent_a, uv2_a, _color_a);
            int down_b = bottom.vertices.Count;
            bottom.Add(pos_b, uv_b, normal_b, tangent_b, uv2_b, _color_b);

            bottom.triangles.Add(newTriangles[down0]);
            bottom.triangles.Add(newTriangles[down1]);
            bottom.triangles.Add(down_b);

            bottom.triangles.Add(newTriangles[down0]);
            bottom.triangles.Add(down_b);
            bottom.triangles.Add(down_a);

            pos0 = pos_a;
            pos1 = pos_b;
        }

    }
}