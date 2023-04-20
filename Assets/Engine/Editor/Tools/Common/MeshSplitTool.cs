//#define UNITY_API

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class MeshSplitTool
{
    public static Mesh Splite(Mesh mesh, List<Plane> planes, bool combineVertices)
    {
        MeshSplitObject i = new MeshSplitObject(mesh);
        Splite(i, planes, combineVertices, out MeshSplitObject o);
        return o.GetMesh();
    }

    public static void Splite(MeshSplitObject meshInfo, List<Plane> planes, bool combineVertices, out MeshSplitObject o)
    {
        MeshSplitObject result = meshInfo;
        for (int i = 0; i < planes.Count; i++)
        {
            Splite(result, planes[i], combineVertices, out MeshSplitObject _, out MeshSplitObject temp);
            result = temp;
        }
        o = result;
    }

    public static void Splite(MeshSplitObject meshInfo, Plane plane, bool combineVertices, out MeshSplitObject a, out MeshSplitObject b)
    {
        Vector3 point = plane.normal * -plane.distance;
        Vector3 normal = plane.normal;
        a = new MeshSplitObject();
        b = new MeshSplitObject();
        bool[] above = new bool[meshInfo.vertices.Count];
        int[] newTriangles = new int[meshInfo.vertices.Count];

        for (int i = 0; i < newTriangles.Length; i++)
        {
            Vector3 vert = meshInfo.vertices[i];
            above[i] = Vector3.Dot(vert - point, normal) >= 0f;
            if (above[i])
            {
                newTriangles[i] = a.vertices.Count;
                a.Add(vert, meshInfo.uvs[i], meshInfo.normals[i], meshInfo.tangents[i]);
            }
            else
            {
                newTriangles[i] = b.vertices.Count;
                b.Add(vert, meshInfo.uvs[i], meshInfo.normals[i], meshInfo.tangents[i]);
            }
        }

        List<Vector3> cutPoint = new List<Vector3>();
        int triangleCount = meshInfo.triangles.Count / 3;
        for (int i = 0; i < triangleCount; i++)
        {
            int _i0 = meshInfo.triangles[i * 3];
            int _i1 = meshInfo.triangles[i * 3 + 1];
            int _i2 = meshInfo.triangles[i * 3 + 2];

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
                    SplitTriangle(meshInfo, a, b, point, normal, newTriangles, up, down0, down1, out pos0, out pos1);
                else
                    SplitTriangle(meshInfo, b, a, point, normal, newTriangles, up, down0, down1, out pos1, out pos0);
                cutPoint.Add(pos0);
                cutPoint.Add(pos1);
            }
        }

        a.center = meshInfo.center;
        a.size = meshInfo.size;
        b.center = meshInfo.center;
        b.size = meshInfo.size;
        if (combineVertices)
        {
            a.CombineVertices(0.001f);
            b.CombineVertices(0.001f);
        }
    }

    private static void SplitTriangle(MeshSplitObject meshInfo, MeshSplitObject top, MeshSplitObject bottom, Vector3 point, Vector3 normal, int[] newTriangles, int up, int down0, int down1, out Vector3 pos0, out Vector3 pos1)
    {
        Vector3 v0 = meshInfo.vertices[up];
        Vector3 v1 = meshInfo.vertices[down0];
        Vector3 v2 = meshInfo.vertices[down1];
        float topDot = Vector3.Dot(point - v0, normal);
        float aScale = Mathf.Clamp01(topDot / Vector3.Dot(v1 - v0, normal));
        float bScale = Mathf.Clamp01(topDot / Vector3.Dot(v2 - v0, normal));
        Vector3 pos_a = v0 + (v1 - v0) * aScale;
        Vector3 pos_b = v0 + (v2 - v0) * bScale;

        Vector2 u0 = meshInfo.uvs[up];
        Vector2 u1 = meshInfo.uvs[down0];
        Vector2 u2 = meshInfo.uvs[down1];
        Vector3 uv_a = (u0 + (u1 - u0) * aScale);
        Vector3 uv_b = (u0 + (u2 - u0) * bScale);

        Vector3 n0 = meshInfo.normals[up];
        Vector3 n1 = meshInfo.normals[down0];
        Vector3 n2 = meshInfo.normals[down1];
        Vector3 normal_a = (n0 + (n1 - n0) * aScale).normalized;
        Vector3 normal_b = (n0 + (n2 - n0) * bScale).normalized;

        Vector4 t0 = meshInfo.tangents[up];
        Vector4 t1 = meshInfo.tangents[down0];
        Vector4 t2 = meshInfo.tangents[down1];
        Vector4 tangent_a = (t0 + (t1 - t0) * aScale).normalized;
        Vector4 tangent_b = (t0 + (t2 - t0) * bScale).normalized;
        tangent_a.w = t1.w;
        tangent_b.w = t2.w;

        int top_a = top.vertices.Count;
        top.Add(pos_a, uv_a, normal_a, tangent_a);
        int top_b = top.vertices.Count;
        top.Add(pos_b, uv_b, normal_b, tangent_b);
        top.triangles.Add(newTriangles[up]);
        top.triangles.Add(top_a);
        top.triangles.Add(top_b);

        int down_a = bottom.vertices.Count;
        bottom.Add(pos_a, uv_a, normal_a, tangent_a);
        int down_b = bottom.vertices.Count;
        bottom.Add(pos_b, uv_b, normal_b, tangent_b);

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

public static class DecaMeshSplitTool
{
    public static Mesh Split(Transform transform, float width, float height, float length, bool combineVertices = false)
    {
        Bounds bounds = GetBoxBounds(transform);
        Func<MeshRenderer, bool> filter = x => x.transform != transform && x.bounds.Intersects(bounds);
        List<(MeshFilter, MeshRenderer)> targets = CollectMeshes(filter);
        Mesh mesh = CombineMeshes(targets);
        List<Plane> boxPlanes = new List<Plane>(6);
        boxPlanes.Add(GetPlane(transform, new Vector3(+0.5f * width, 0, 0)));
        boxPlanes.Add(GetPlane(transform, new Vector3(-0.5f * width, 0, 0)));
        boxPlanes.Add(GetPlane(transform, new Vector3(0, +0.5f * height, 0)));
        boxPlanes.Add(GetPlane(transform, new Vector3(0, -0.5f * height, 0)));
        boxPlanes.Add(GetPlane(transform, new Vector3(0, 0, +0.5f * length)));
        boxPlanes.Add(GetPlane(transform, new Vector3(0, 0, -0.5f * length)));
        MeshSplitObject mso = new MeshSplitObject(mesh);
        MeshSplitTool.Splite(mso, boxPlanes, combineVertices, out MeshSplitObject splitObject);
        for (int i = 0; i < splitObject.vertices.Count; i++)
        {
            Vector3 objectSpacePos = transform.InverseTransformPoint(splitObject.vertices[i]);
            splitObject.uvs[i] = new Vector2(objectSpacePos.x / width + 0.5f, (objectSpacePos.z + length * 0.5f) / width);
        }
        Mesh result = splitObject.GetMesh();
        //result.Optimize();
        return result;
    }

    private static Plane GetPlane(Transform transform, Vector3 direction)
    {
        return new Plane(transform.TransformDirection(direction), transform.TransformPoint(direction));
    }

    private static Bounds GetBoxBounds(Transform transform)
    {
        Bounds bounds = new Bounds(transform.position, default);
        bounds.Encapsulate(transform.InverseTransformPoint(new Vector3(+1, +1, +1)));
        bounds.Encapsulate(transform.InverseTransformPoint(new Vector3(+1, +1, -1)));
        bounds.Encapsulate(transform.InverseTransformPoint(new Vector3(+1, -1, +1)));
        bounds.Encapsulate(transform.InverseTransformPoint(new Vector3(+1, -1, -1)));
        bounds.Encapsulate(transform.InverseTransformPoint(new Vector3(-1, +1, +1)));
        bounds.Encapsulate(transform.InverseTransformPoint(new Vector3(-1, +1, -1)));
        bounds.Encapsulate(transform.InverseTransformPoint(new Vector3(-1, -1, +1)));
        bounds.Encapsulate(transform.InverseTransformPoint(new Vector3(-1, -1, -1)));
        return bounds;
    }

    private static List<(MeshFilter, MeshRenderer)> CollectMeshes(Func<MeshRenderer, bool> filter)
    {
        Scene scene = SceneManager.GetActiveScene();
        GameObject[] roots = scene.GetRootGameObjects();
        List<(MeshFilter, MeshRenderer)> targets = new List<(MeshFilter, MeshRenderer)>();
        for (int i = 0; i < scene.rootCount; i++)
        {
            if (roots[i].activeInHierarchy == false)
                continue;
            var renderers = roots[i].GetComponentsInChildren<MeshRenderer>();
            foreach (var renderer in renderers)
            {
                if (renderer.GetComponent<IgnoreMeshSplit>())
                    continue;
                if (renderer.GetComponent<BoxCollider>())
                    continue;
                if (renderer.GetComponent<SphereCollider>())
                    continue;
                var meshFilter = renderer.GetComponent<MeshFilter>();
                if (meshFilter && meshFilter.sharedMesh)
                {
                    if (filter == null || filter(renderer))
                    {
                        targets.Add((meshFilter, renderer));
                    }
                }
            }
        }

        return targets;
    }

    private static Mesh CombineMeshes(List<(MeshFilter, MeshRenderer)> targets)
    {
#if UNITY_API
        Mesh combinedMesh;
        List<CombineInstance> cis = new List<CombineInstance>();
        foreach ((MeshFilter, MeshRenderer) target in targets)
        {
            var mesh = target.Item1.sharedMesh;
            var renderer = target.Item2;
            for (int i = 0; i < mesh.subMeshCount; i++)
            {
                cis.Add(new CombineInstance()
                {
                    mesh = mesh,
                    lightmapScaleOffset = renderer.lightmapScaleOffset,
                    subMeshIndex = i,
                    transform = renderer.transform.localToWorldMatrix,
                    realtimeLightmapScaleOffset = renderer.realtimeLightmapScaleOffset
                });
            }
        }

        combinedMesh = new Mesh();
        combinedMesh.CombineMeshes(cis.ToArray(), true, true, false);
        combinedMesh.RecalculateBounds();
#else
        List<Vector3> vertices = new List<Vector3>();
        List<int> triangles = new List<int>();
        for (int targetIndex = 0; targetIndex < targets.Count; targetIndex++)
        {
            var meshFilter = targets[targetIndex].Item1;
            var mesh = meshFilter.sharedMesh;
            var sVertices = mesh.vertices;
            var sTriangles = mesh.triangles;

            for (int i = 0; i < sTriangles.Length; i++)
            {
                triangles.Add(vertices.Count + i);
            }

            for (int i = 0; i < sTriangles.Length; i++)
            {
                int sVertexIndex = sTriangles[i];
                var local = sVertices[sVertexIndex];
                var world = meshFilter.transform.TransformPoint(local);
                vertices.Add(world);
            }
        }

        Mesh combinedMesh = new Mesh();
        combinedMesh.vertices = vertices.ToArray();
        combinedMesh.triangles = triangles.ToArray();
        combinedMesh.uv = new Vector2[vertices.Count];
        combinedMesh.normals = new Vector3[vertices.Count];
        combinedMesh.tangents = new Vector4[vertices.Count];
        combinedMesh.RecalculateBounds();

#endif

        return combinedMesh;
    }
}

public class MeshSplitObject
{
    public List<Vector3> vertices;
    public List<int> triangles;
    public List<Vector2> uvs;
    public List<Vector3> normals;
    public List<Vector4> tangents;
    public Vector3 size, center;

    public MeshSplitObject()
    {
        vertices = new List<Vector3>();
        triangles = new List<int>();
        uvs = new List<Vector2>();
        normals = new List<Vector3>();
        tangents = new List<Vector4>();
        size = center = Vector3.zero;
    }

    public MeshSplitObject(Mesh mesh)
    {
        vertices = new List<Vector3>(mesh.vertices);
        triangles = new List<int>(mesh.triangles);
        uvs = new List<Vector2>(mesh.uv);
        normals = new List<Vector3>(mesh.normals);
        tangents = new List<Vector4>(mesh.tangents);
        center = mesh.bounds.center;
        size = mesh.bounds.size;
    }

    public void Add(Mesh mesh)
    {
        for (int i = 0; i < mesh.vertexCount; i++)
        {
            vertices.Add(mesh.vertices[i]);
            uvs.Add(mesh.uv[i]);
            normals.Add(mesh.normals[i]);
            tangents.Add(mesh.tangents[i]);
        }
        int length = triangles.Count;
        for (int i = 0; i < mesh.triangles.Length; i++)
        {
            triangles.Add(mesh.triangles[i] + length);
        }
    }

    public void Add(Vector3 vert, Vector2 uv, Vector3 normal, Vector4 tangent)
    {
        vertices.Add(vert);
        uvs.Add(uv);
        normals.Add(normal);
        tangents.Add(tangent);
    }

    public Mesh GetMesh()
    {
        Mesh mesh = new Mesh();
        mesh.vertices = vertices.ToArray();
        mesh.uv = uvs.ToArray();
        mesh.normals = normals.ToArray();
        mesh.tangents = tangents.ToArray();
        mesh.triangles = triangles.ToArray();
        return mesh;
    }

    public void MapperCube(Rect range)
    {
        if (uvs.Count < vertices.Count)
            uvs = new List<Vector2>(vertices.Count);
        int count = triangles.Count / 3;
        for (int i = 0; i < count; i++)
        {
            int _i0 = triangles[i * 3];
            int _i1 = triangles[i * 3 + 1];
            int _i2 = triangles[i * 3 + 2];

            Vector3 v0 = vertices[_i0] - center + size / 2f;
            Vector3 v1 = vertices[_i1] - center + size / 2f;
            Vector3 v2 = vertices[_i2] - center + size / 2f;
            v0 = new Vector3(v0.x / size.x, v0.y / size.y, v0.z / size.z);
            v1 = new Vector3(v1.x / size.x, v1.y / size.y, v1.z / size.z);
            v2 = new Vector3(v2.x / size.x, v2.y / size.y, v2.z / size.z);

            Vector3 a = v0 - v1;
            Vector3 b = v2 - v1;
            Vector3 dir = Vector3.Cross(a, b);
            float x = Mathf.Abs(Vector3.Dot(dir, Vector3.right));
            float y = Mathf.Abs(Vector3.Dot(dir, Vector3.up));
            float z = Mathf.Abs(Vector3.Dot(dir, Vector3.forward));
            if (x > y && x > z)
            {
                uvs[_i0] = new Vector2(v0.z, v0.y);
                uvs[_i1] = new Vector2(v1.z, v1.y);
                uvs[_i2] = new Vector2(v2.z, v2.y);
            }
            else if (y > x && y > z)
            {
                uvs[_i0] = new Vector2(v0.x, v0.z);
                uvs[_i1] = new Vector2(v1.x, v1.z);
                uvs[_i2] = new Vector2(v2.x, v2.z);
            }
            else if (z > x && z > y)
            {
                uvs[_i0] = new Vector2(v0.x, v0.y);
                uvs[_i1] = new Vector2(v1.x, v1.y);
                uvs[_i2] = new Vector2(v2.x, v2.y);
            }
            uvs[_i0] = new Vector2(range.xMin + (range.xMax - range.xMin) * uvs[_i0].x, range.yMin + (range.yMax - range.yMin) * uvs[_i0].y);
            uvs[_i1] = new Vector2(range.xMin + (range.xMax - range.xMin) * uvs[_i1].x, range.yMin + (range.yMax - range.yMin) * uvs[_i1].y);
            uvs[_i2] = new Vector2(range.xMin + (range.xMax - range.xMin) * uvs[_i2].x, range.yMin + (range.yMax - range.yMin) * uvs[_i2].y);
        }
    }

    public void CombineVertices(float range)
    {
        // TODO 优化
        range *= range;
        for (int i = 0; i < vertices.Count; i++)
        {
            for (int j = i + 1; j < vertices.Count; j++)
            {
                bool dis = (vertices[i] - vertices[j]).sqrMagnitude < range;
                bool uv = (uvs[i] - uvs[j]).sqrMagnitude < range;
                bool dir = Vector3.Dot(normals[i], normals[j]) > 0.999f;
                if (dis && uv && dir)
                {
                    for (int k = 0; k < triangles.Count; k++)
                    {
                        if (triangles[k] == j)
                            triangles[k] = i;
                        if (triangles[k] > j)
                            triangles[k]--;
                    }
                    vertices.RemoveAt(j);
                    normals.RemoveAt(j);
                    tangents.RemoveAt(j);
                    uvs.RemoveAt(j);
                }
            }
        }
    }

    public void Reverse()
    {
        int count = triangles.Count / 3;
        for (int i = 0; i < count; i++)
        {
            int t = triangles[i * 3 + 2];
            triangles[i * 3 + 2] = triangles[i * 3 + 1];
            triangles[i * 3 + 1] = t;
        }
        count = vertices.Count;
        for (int i = 0; i < count; i++)
        {
            normals[i] *= -1;
            Vector4 tan = tangents[i];
            tan.w = -1;
            tangents[i] = tan;
        }
    }

    public static Vector4 CalculateTangent(Vector3 normal)
    {
        Vector3 tan = Vector3.Cross(normal, Vector3.up);
        if (tan == Vector3.zero)
            tan = Vector3.Cross(normal, Vector3.forward);
        tan = Vector3.Cross(tan, normal);
        return new Vector4(tan.x, tan.y, tan.z, 1.0f);
    }
}
