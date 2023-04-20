#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[DisallowMultipleComponent]
[ExecuteInEditMode]

/********************************************************************
 FileName: MeshDecal.cs
 Description: 生成贴合目标对象的贴花Mesh
 history: 30:12:2018 by puppet_master
 https://blog.csdn.net/puppet_master
*********************************************************************/
public class MeshDecal
{
    private Mesh currMesh = null;

    private List<Vector3> vertices = new List<Vector3>();
    private List<Vector3> normals = new List<Vector3>();
    private List<int> indices = new List<int>();
    private List<Vector2> texcoords = new List<Vector2>();

    Vector3 size;
    Transform transform;
    CollisionChecker checker;

    public Mesh Split(Transform t, float width, float height, float length)
    {
        transform = t;
        size = new Vector3(width, height, length);
        var mrs = MonoBehaviour.FindObjectsOfType<MeshRenderer>();
        checker = new CollisionChecker(width, height, length);

        Bounds bounds = new Bounds(transform.position, new Vector3(0.1f, 0.1f, 0.1f));
        bounds.Encapsulate(transform.TransformPoint(width, height, length));
        bounds.Encapsulate(transform.TransformPoint(-width, height, length));
        bounds.Encapsulate(transform.TransformPoint(width, -height, length));
        bounds.Encapsulate(transform.TransformPoint(-width, -height, length));
        bounds.Encapsulate(transform.TransformPoint(width, height, -length));
        bounds.Encapsulate(transform.TransformPoint(-width, height, -length));
        bounds.Encapsulate(transform.TransformPoint(width, -height, -length));
        bounds.Encapsulate(transform.TransformPoint(-width, -height, -length));

        List<Poly> totalPolyList = new List<Poly>();
        foreach (var r in mrs)
        {
            if (!r.enabled)
                continue;
            //剔除Decal自身
            if (r.transform == transform)
                continue;
            if (r.GetComponent<IgnoreMeshSplit>())
                continue;
            if (r.GetComponent<BoxCollider>())
                continue;
            if (r.GetComponent<SphereCollider>())
                continue;
            //遍历所有的MeshRenderer判断和自身立方体相交的Mesh进行Decal Mesh生成
            if (bounds.Intersects(r.bounds))
            {
                var polylist = GenerateDecalMesh(r);
                totalPolyList.AddRange(polylist);
            }
        }

        //PolyIntersection.ProcessIntersaction(totalPolyList);
        //PolyIntersection.ProcessCover(totalPolyList);

        foreach (var poly in totalPolyList)
        {
            AddPolygon(poly.vertices.ToArray(), poly.normal);
        }

        //将存储的数据生成Unity使用的Mesh
        return GenerateUnityMesh();
    }


    public List<Poly> GenerateDecalMesh(MeshRenderer target)
    {
        var mesh = target.GetComponent<MeshFilter>().sharedMesh;
        //GC很高，可以优化
        var meshVertices = mesh.vertices;
        var meshTriangles = mesh.triangles;

        var targetToDecalMatrix = transform.worldToLocalMatrix * target.transform.localToWorldMatrix;
        
        List<Poly> polyList = new List<Poly>();
        for (int i = 0; i < meshTriangles.Length; i = i + 3)
        {
            var index1 = meshTriangles[i];
            var index2 = meshTriangles[i + 1];
            var index3 = meshTriangles[i + 2];

            var vertex1 = meshVertices[index1];
            var vertex2 = meshVertices[index2];
            var vertex3 = meshVertices[index3];
            //将网格的三角形转化到Decal自身立方体的坐标系中
            vertex1 = targetToDecalMatrix.MultiplyPoint(vertex1);
            vertex2 = targetToDecalMatrix.MultiplyPoint(vertex2);
            vertex3 = targetToDecalMatrix.MultiplyPoint(vertex3);

            var dir1 = vertex1 - vertex2;
            var dir2 = vertex1 - vertex3;
            var normalDir = Vector3.Cross(dir1, dir2).normalized;

            var vectorList = new List<Vector3>();
            vectorList.Add(vertex1);
            vectorList.Add(vertex2);
            vectorList.Add(vertex3);
            //if (Vector3.Angle(Vector3.forward, -normalDir) <= 90.0f)
            {
                checker.CheckCollision(vectorList);
                //if (vectorList.Count > 0)
                //    AddPolygon(vectorList.ToArray(), normalDir);
                if (vectorList.Count > 0)
                {
                    polyList.Add(new Poly(vectorList, normalDir));
                }
            }
        }
        return polyList;
        ////PolyIntersection.ProcessIntersaction(polyList);
        ////PolyIntersection.ProcessCover(polyList);

        ////foreach (var poly in polyList)
        ////{
        ////    AddPolygon(poly.vertices.ToArray(), poly.normal);
        ////}
    }

    public void AddPolygon(Vector3[] poly, Vector3 normal)
    {
        int ind1 = AddVertex(poly[0], normal);

        for (int i = 1; i < poly.Length - 1; i++)
        {
            int ind2 = AddVertex(poly[i], normal);
            int ind3 = AddVertex(poly[i + 1], normal);

            indices.Add(ind1);
            indices.Add(ind2);
            indices.Add(ind3);
        }
    }

    private int AddVertex(Vector3 vertex, Vector3 normal)
    {
        //优先寻找是否包含该顶点
        int index = FindVertex(vertex);
        if (index == -1)
        {
            vertices.Add(transform.TransformPoint(vertex));
            normals.Add(normal);
            ////物体空间的坐标作为uv，需要从（-0.5，0.5）转化到（0，1）区间
            //float u = Mathf.Lerp(0.0f, 1.0f, vertex.x + 0.5f);
            //float v = Mathf.Lerp(0.0f, 1.0f, vertex.z + 0.5f);
            texcoords.Add(new Vector2(vertex.x / size.x + 0.5f, (vertex.z + size.z * 0.5f) / size.x));
            return vertices.Count - 1;
        }
        else
        {
            //已包含时，将该顶点的法线与新插入的顶点进行平均，共享的顶点，需要修改法线
            normals[index] = (normals[index] + normal).normalized;
            return index;
        }
    }

    private int FindVertex(Vector3 vertex)
    {
        for (int i = 0; i < vertices.Count; i++)
        {
            if (Vector3.Distance(vertices[i], vertex) < 0.01f) return i;
        }
        return -1;
    }

    public void HandleZFighting(float distance)
    {
        for (int i = 0; i < vertices.Count; i++)
        {
            vertices[i] += normals[i] * distance;
        }
    }

    public Mesh GenerateUnityMesh()
    {
        currMesh = new Mesh();
        HandleZFighting(0.001f);


        currMesh.Clear(true);

        currMesh.vertices = vertices.ToArray();
        currMesh.normals = normals.ToArray();
        currMesh.triangles = indices.ToArray();
        currMesh.uv = texcoords.ToArray();

        vertices.Clear();
        normals.Clear();
        indices.Clear();
        texcoords.Clear();

        //meshFilter.sharedMesh = currMesh;
        return currMesh;
    }

}

public class CollisionChecker
{

    private List<Plane> planList = new List<Plane>();

    public CollisionChecker(float width, float height, float length)
    {
        //front
        planList.Add(new Plane(Vector3.forward, 0.5f * length));
        //back
        planList.Add(new Plane(Vector3.back, 0.5f * length));
        //up
        planList.Add(new Plane(Vector3.up, 0.5f * height));
        //down
        planList.Add(new Plane(Vector3.down, 0.5f * height));
        //left
        planList.Add(new Plane(Vector3.left, 0.5f * width));
        //right
        planList.Add(new Plane(Vector3.right, 0.5f * width));

    }

    private void CheckCollision(Plane plane, List<Vector3> vectorList)
    {
        var newList = new List<Vector3>();
        for (var current = 0; current < vectorList.Count; current++)
        {
            var next = (current + 1) % vectorList.Count;
            var v1 = vectorList[current];
            var v2 = vectorList[next];
            var currentPointIn = plane.GetSide(v1);
            if (currentPointIn == true)
                newList.Add(v1);

            if (plane.GetSide(v2) != currentPointIn)
            {
                float distance;
                var ray = new Ray(v1, v2 - v1);
                plane.Raycast(ray, out distance);
                var newPoint = ray.GetPoint(distance);
                newList.Add(newPoint);
            }
        }
        vectorList.Clear();
        vectorList.AddRange(newList);
    }

    public void CheckCollision(List<Vector3> vectorList)
    {
        foreach (var curPlane in planList)
        {
            CheckCollision(curPlane, vectorList);
        }
    }

}

public class Poly
{
    public List<Vector3> vertices { get; private set; }
    public Vector3 normal { get; private set; }

    public Plane plane { get; private set; }
    public Bounds bounds { get => m_Bounds; }
    Bounds m_Bounds;
    public Poly(List<Vector3> verts, Vector3 n)
    {
        vertices = verts;
        normal = n;
        plane = new Plane(n, verts[0]);
        m_Bounds = new Bounds(verts[0], Vector3.zero);
        foreach (var v in verts)
            m_Bounds.Encapsulate(v);
    }
    public Poly(List<Vector3> verts)
    {
        vertices = verts;
        normal = Vector3.Cross((verts[1] - verts[0]).normalized, (verts[2] - verts[1]).normalized);
        plane = new Plane(normal, verts[0]);
        m_Bounds = new Bounds(verts[0], Vector3.zero);
        foreach (var v in verts)
            m_Bounds.Encapsulate(v);
    }
}

#endif