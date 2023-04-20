using CFUtilPoolLib;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using CFEngine.Editor;
public class MeshUV
{
    // [MenuItem ("Assets/MeshUV")]
    public static void RefreshMeshWithUV()
    {
        Object[] os = Selection.GetFiltered(typeof(Mesh), SelectionMode.Unfiltered | SelectionMode.Deep);

        foreach (UnityEngine.Object o in os)
        {
            _RefreshMeshWithUV(AssetDatabase.GetAssetPath(o));
        }

        //_Test();
    }

    static void _RefreshMeshWithUV(string importedPath)
    {
        Mesh meshRaw = AssetDatabase.LoadAssetAtPath<Mesh>(importedPath);
        string path = importedPath;
        path = path.Substring(0, path.LastIndexOf(meshRaw.name));
        Mesh mesh = UnityEngine.Object.Instantiate<Mesh>(meshRaw);
        mesh.name = meshRaw.name + "_UV";
        //if (removeUV2)
        //    newMesh.uv2 = null;
        mesh.uv3 = null;
        mesh.uv4 = null;
        //if (removeColor)
        //    newMesh.colors = null;
        MeshUtility.SetMeshCompression(mesh, ModelImporterMeshCompression.Low);
        MeshUtility.Optimize(mesh);

        //XMesh xmesh = _Test();
        XMesh xmesh = new XMesh();
        MeshUV data = new MeshUV();
        for (int i = 0; i < mesh.subMeshCount; ++i)
        {
            xmesh.SetFrom(mesh, i);

            _BuildMeshUV(xmesh, data);
            _Process(data);
            _BuildXMesh(data, xmesh);

            xmesh.Export(mesh);
        }

        CommonAssets.CreateAsset<Mesh>(path, mesh.name, ".asset", mesh);
    }

    class XVertex
    {
        public Vector3 point;
        public Vector2 uv;
        public Vector2 uv2;
        public Vector3 normal;
        public Vector4 tangent;
        public int index;
        public XVertex() { }
        public XVertex(XVertex other)
        {
            other.CopyTo(this);
        }
        public override string ToString()
        {
            return "(" + uv.x + ", " + uv.y + ")";
        }

        public void Transposition()
        {
            float t = uv.x;
            uv.x = uv.y;
            uv.y = t;
        }

        public void CopyTo(XVertex other)
        {
            other.point = point;
            other.uv = uv;
            other.uv2 = uv2;
            other.normal = normal;
            other.tangent = tangent;
        }

        public void Lerp(float start, float end, float r, XVertex startV, XVertex endV)
        {
            point = _Lerp(start, end, r, startV.point, endV.point);
            uv2 = _Lerp(start, end, r, startV.uv2, endV.uv2);
            normal = _Lerp(start, end, r, startV.normal, endV.normal);
            tangent = _Lerp(start, end, r, startV.tangent, endV.tangent);
        }
    }
    class XEdge
    {
        public XVertex from;
        public XVertex to;
        public XEdge pair;
        public XFace face;
        public XEdge next;
        public XEdge prev;
        public override string ToString()
        {
            return from.ToString() + "->" + to.ToString();
        }

        public void SetPrev(XEdge myPrev, XEdge prevNext)
        {
            prev.next = prevNext;
            prevNext.prev = prev;

            prev = myPrev;
            myPrev.next = this;
        }
        public void SetNext(XEdge myNext, XEdge nextPrev)
        {
            next.prev = nextPrev;
            nextPrev.next = next;

            next = myNext;
            myNext.prev = this;
        }
    }
    class XFace
    {
        public XEdge edge;

        public void RefreshEdges()
        {
            XEdge e = edge;
            do
            {
                e.face = this;
                e = e.next;
            }
            while (edge != e);
        }
    }

    class XMesh
    {
        public List<Vector3> vertices = new List<Vector3>();// { new Vector3(-0.5f, -0.5f, 0.0f), new Vector3(0.5f, 0.5f, 0.0f), new Vector3(1.5f, 0.5f, 0.0f), new Vector3(1.5f, -1.5f, 0.0f) };
        public List<Vector2> uvs = new List<Vector2>();// { new Vector2(-0.5f, -0.5f), new Vector2(0.5f, 0.5f), new Vector2(1.5f, 0.5f), new Vector2(1.5f, -1.5f) };
        public List<Vector2> uv2s = new List<Vector2>();// { new Vector2(-0.5f, -0.5f), new Vector2(0.5f, 0.5f), new Vector2(1.5f, 0.5f), new Vector2(1.5f, -1.5f) };
        public List<Vector3> normals = new List<Vector3>();
        public List<Vector4> tangents = new List<Vector4>();


        //public List<Vector3> vertices = new List<Vector3>() { new Vector3(0.0f, 0.5f, 0.0f), new Vector3(0.5f, 0.0f, 0.0f), new Vector3(0.0f, -0.5f, 0.0f), new Vector3(-0.5f, 0.0f, 0.0f) };
        //public List<Vector2> uvs = new List<Vector2>() { new Vector2(0.0f, 0.5f), new Vector2(0.5f, 0.0f), new Vector2(0.0f, -0.5f), new Vector2(-0.5f, 0.0f) };
        //public List<Vector2> uv2s = new List<Vector2>() { new Vector2(0.0f, 0.5f), new Vector2(0.5f, 0.0f), new Vector2(0.0f, -0.5f), new Vector2(-0.5f, 0.0f) };
        public List<int> triangles = new List<int>();// { 0, 1, 2, 0, 2, 3 };
        private int m_SubMesh = 0;
        public void SetFrom(Mesh mesh, int subMesh = 0)
        {
            vertices.Clear();
            uvs.Clear();
            uv2s.Clear();
            normals.Clear();
            tangents.Clear();
            triangles.Clear();

            mesh.GetVertices(vertices);
            mesh.GetUVs(0, uvs);
            mesh.GetUVs(1, uv2s);
            mesh.GetNormals(normals);
            mesh.GetTangents(tangents);
            mesh.GetTriangles(triangles, subMesh);

            m_SubMesh = subMesh;
        }

        public void Export(Mesh mesh)
        {
            mesh.SetVertices(vertices);
            mesh.SetUVs(0, uvs);
            mesh.SetUVs(1, uv2s);
            mesh.SetNormals(normals);
            mesh.SetTangents(tangents);
            mesh.SetTriangles(triangles, m_SubMesh);
        }
    }

    static void _Process(MeshUV data)
    {
        ///> 分割y方向
        data._ProcessX();
        ///> 转置分割x方向
        data._TranspositionAll();
        data._ProcessX();
        data._TranspositionAll();

        ///> 分割后的多边形 三角化
        data._Triangulation();

        ///> y方向切割整数顶点和平移
        data._CheckU();
        ///> 转置和x方向
        data._TranspositionAll();
        data._CheckU();
        data._TranspositionAll();

        ///> 切割平移后，双边砍成单边（如果后续没别的处理，只是输出的话，此步不必须）
        data._CutEdgePairs();

        ///> 新增的顶点标序号，便于输出
        data._ReIndexVertices();

    }
    static XMesh _Test()
    {
        XMesh mesh = new XMesh();
        MeshUV data = new MeshUV();
        _BuildMeshUV(mesh, data);

        _Process(data);

        _BuildXMesh(data, mesh);
        return mesh;
    }
    static void _BuildXMesh(MeshUV data, XMesh outMesh)
    {
        outMesh.vertices.Clear();
        outMesh.uvs.Clear();
        outMesh.uv2s.Clear();
        outMesh.normals.Clear();
        outMesh.tangents.Clear();
        outMesh.triangles.Clear();
        foreach (XVertex vertex in data.vertices)
        {
            outMesh.vertices.Add(vertex.point);
            outMesh.uvs.Add(vertex.uv);
            if (data.hasUV2)
                outMesh.uv2s.Add(vertex.uv2);
            if (data.hasNormal)
                outMesh.normals.Add(vertex.normal);
            if (data.hasTangent)
                outMesh.tangents.Add(vertex.tangent);
        }

        foreach (XFace face in data.faces)
        {
            XEdge edge = face.edge;
            do
            {
                outMesh.triangles.Add(edge.from.index);
                edge = edge.next;
            }
            while (edge != face.edge);
        }
    }
    static void _BuildMeshUV(XMesh mesh, MeshUV outData)
    {
        outData.vertices.Clear();
        outData.edges.Clear();
        outData.faces.Clear();
        outData.hasUV2 = mesh.uv2s.Count == mesh.uvs.Count;
        outData.hasNormal = mesh.normals.Count == mesh.uvs.Count;
        outData.hasTangent = mesh.tangents.Count == mesh.uvs.Count;

        Dictionary<int, XEdge> edgepairs = new Dictionary<int, XEdge>();

        for (int i = 0; i < mesh.vertices.Count; ++i)
        {
            XVertex vertex = new XVertex();
            vertex.point = mesh.vertices[i];
            vertex.uv = mesh.uvs[i];
            if (outData.hasUV2)
            {
                vertex.uv2 = mesh.uv2s[i];
            }
            if (outData.hasNormal)
            {
                vertex.normal = mesh.normals[i];
            }
            if (outData.hasTangent)
            {
                vertex.tangent = mesh.tangents[i];
            }
            outData.vertices.Add(vertex);
        }

        for (int i = 0; i + 2 < mesh.triangles.Count; i += 3)
        {
            int a = mesh.triangles[i];
            int b = mesh.triangles[i + 1];
            int c = mesh.triangles[i + 2];

            XEdge ab = _CreateEdge(outData, a, b, edgepairs);
            XEdge bc = _CreateEdge(outData, b, c, edgepairs);
            XEdge ca = _CreateEdge(outData, c, a, edgepairs);

            ab.next = bc;
            bc.prev = ab;
            bc.next = ca;
            ca.prev = bc;
            ca.next = ab;
            ab.prev = ca;

            XFace face = new XFace();
            face.edge = ab;

            ab.face = face;
            bc.face = face;
            ca.face = face;

            outData.edges.Add(ab);
            outData.edges.Add(bc);
            outData.edges.Add(ca);

            outData.faces.Add(face);
        }
    }

    static XEdge _CreateEdge(MeshUV data, int fromIdx, int toIdx, Dictionary<int, XEdge> edgepairs)
    {
        XEdge ab = new XEdge();
        ab.from = data.vertices[fromIdx];
        ab.to = data.vertices[toIdx];

        int mykey = data.vertices.Count * fromIdx + toIdx;
        int pairKey = data.vertices.Count * toIdx + fromIdx;

        XEdge pair = null;
        if(edgepairs.TryGetValue(pairKey, out pair))
        {
            ab.pair = pair;
            pair.pair = ab;
        }
        else
        {
            edgepairs[mykey] = ab;
        }
        return ab;
    }

    List<XVertex> vertices = new List<XVertex>();
    List<XEdge> edges = new List<XEdge>();
    List<XFace> faces = new List<XFace>();
    bool hasUV2 = false;
    bool hasNormal = false;
    bool hasTangent = false;
    static bool _SameValue(float a, float b)
    {
        return a == b;
    }

    static Vector2 _Lerp(float x1, float x2, float x, Vector2 y1, Vector2 y2)
    {
        Vector2 v = new Vector2();
        v.x = _Lerp(x1, x2, x, y1.x, y2.x);
        v.y = _Lerp(x1, x2, x, y1.y, y2.y);
        return v;
    }

    static Vector3 _Lerp(float x1, float x2, float x, Vector3 y1, Vector3 y2)
    {
        Vector3 v = new Vector3();
        v.x = _Lerp(x1, x2, x, y1.x, y2.x);
        v.y = _Lerp(x1, x2, x, y1.y, y2.y);
        v.z = _Lerp(x1, x2, x, y1.z, y2.z);
        return v;
    }
    static Vector4 _Lerp(float x1, float x2, float x, Vector4 y1, Vector4 y2)
    {
        Vector4 v = new Vector4();
        v.x = _Lerp(x1, x2, x, y1.x, y2.x);
        v.y = _Lerp(x1, x2, x, y1.y, y2.y);
        v.z = _Lerp(x1, x2, x, y1.z, y2.z);
        v.w = _Lerp(x1, x2, x, y1.w, y2.w);
        return v;
    }
    static float _Lerp(float x1, float x2, float x, float y1, float y2)
    {
        if (x1 == x2)
            return y1;
        return (y1 - y2) * (x - x2) / (x1 - x2) + y2;
    }

    static void _ProcessNewPair(XEdge edge, Dictionary<KeyValuePair<XVertex, XVertex>, XEdge> newEdges)
    {
        KeyValuePair<XVertex, XVertex> key = new KeyValuePair<XVertex, XVertex>(edge.to, edge.from);
        XEdge other;
        if (newEdges.TryGetValue(key, out other))
        {
            other.pair = edge;
            edge.pair = other;
        }
        else
        {
            newEdges.Add(new KeyValuePair<XVertex, XVertex>(edge.from, edge.to), edge);
        }
    }
    struct CrossInfo
    {
        public enum Type
        {
            TwoEdges,
            OneEdge,
        }
        public XEdge a;
        public XEdge b;
        public float f;
        public Type type;
    }
    void _TranspositionAll()
    {
        foreach (XVertex v in vertices)
        {
            v.Transposition();
        }
    }
    void _ReIndexVertices()
    {
        for (int i = 0; i < vertices.Count; ++i)
        {
            vertices[i].index = i;
        }
    }
    void _CutEdgePairs()
    {
        foreach (XEdge edge in edges)
        {
            if (edge.pair != null && edge.from != edge.pair.to)
            {
                edge.pair.pair = null;
                edge.pair = null;
            }
        }
    }
    void _ProcessX()
    {
        float minX = float.MaxValue;
        float maxX = float.MinValue;
        for(int i = 0; i < vertices.Count; ++i)
        {
            minX = Mathf.Min(minX, vertices[i].uv.x);
            maxX = Mathf.Max(maxX, vertices[i].uv.x);
        }

        minX = (float)((int)minX);
        maxX = (float)((int)maxX);

        List<XEdge> crossEdges = new List<XEdge>();
        List<int> crossEdgesInfo = new List<int>();
        List<CrossInfo> crossFaces = new List<CrossInfo>();
        for (float f = minX; f <= maxX; f += 1.0f)
        {
            crossFaces.Clear();
            foreach (XFace face in faces)
            {
                XEdge edge = face.edge;

                crossEdges.Clear();
                crossEdgesInfo.Clear();

                bool edgeOnGrid = false;
                do
                {
                    if (_SameValue(edge.from.uv.x, f) && _SameValue(edge.to.uv.x, f))
                    {
                        edgeOnGrid = true;
                        break;
                    }

                    float v = (edge.from.uv.x - f) * (edge.to.uv.x - f);
                    ///> 一个在左边，一个在右边
                    if (v < 0.0f)
                    {
                        crossEdges.Add(edge);
                        crossEdgesInfo.Add(1);
                    }
                    ///> 与一个顶点重合。只看边的尾端
                    else if (edge.to.uv.x - f == 0.0f)
                    {
                        crossEdges.Add(edge);
                        crossEdgesInfo.Add(0);
                    }

                    edge = edge.next;
                }
                while (edge != face.edge);

                ///> 边与坐标轴垂直，无需分割
                if (edgeOnGrid)
                    continue;

                ///> 无交点
                if (crossEdges.Count == 0)
                    continue;
                
                if (crossEdges.Count > 2)
                {
                    XDebug.singleton.AddErrorLog("Cross Edges count = ", crossEdges.Count.ToString());
                    return;
                }

                ///> 只与一个顶点重合，无需分割
                if (crossEdges.Count == 1)
                {
                    continue;
                }

                ///> 与两条边相交
                {
                    CrossInfo info = new CrossInfo();
                    info.type = CrossInfo.Type.TwoEdges;
                    info.f = f;

                    for (int j = 0; j < crossEdges.Count; ++j)
                    {
                        if (crossEdgesInfo[j] == 0)
                        {
                            info.a = crossEdges[j];
                            info.type = CrossInfo.Type.OneEdge;
                        }
                        else
                        {
                            if (info.b == null)
                                info.b = crossEdges[j];
                            else
                                info.a = crossEdges[j];
                        }
                    }

                    crossFaces.Add(info);
                }
            }

            Dictionary<XEdge, XVertex> newVertices = new Dictionary<XEdge, XVertex>();
            Dictionary<KeyValuePair<XVertex, XVertex>, XEdge> newEdges = new Dictionary<KeyValuePair<XVertex, XVertex>, XEdge>();
            ///> 开始处理相交的边
            foreach (CrossInfo info in crossFaces)
            {
                XFace oldFace = info.a.face;
                this.faces.Remove(oldFace);

                XVertex pA = null;
                XVertex pB = null;

                if (info.type == CrossInfo.Type.TwoEdges)
                {
                    if (info.a.pair == null || !newVertices.TryGetValue(info.a.pair, out pA))
                    {
                        pA = new XVertex();
                        pA.uv.x = info.f;
                        pA.uv.y = _Lerp(info.a.from.uv.x, info.a.to.uv.x, f, info.a.from.uv.y, info.a.to.uv.y);
                        pA.Lerp(info.a.from.uv.x, info.a.to.uv.x, f, info.a.from, info.a.to);

                        newVertices.Add(info.a, pA);
                        vertices.Add(pA);
                    }

                    if (info.b.pair == null || !newVertices.TryGetValue(info.b.pair, out pB))
                    {
                        pB = new XVertex();
                        pB.uv.x = info.f;
                        pB.uv.y = _Lerp(info.b.from.uv.x, info.b.to.uv.x, f, info.b.from.uv.y, info.b.to.uv.y);
                        pB.Lerp(info.b.from.uv.x, info.b.to.uv.x, f, info.b.from, info.b.to);

                        newVertices.Add(info.b, pB);
                        vertices.Add(pB);
                    }
                }
                else
                {
                    pA = info.a.to;
                    
                    if (info.b.pair == null || !newVertices.TryGetValue(info.b.pair, out pB))
                    {
                        pB = new XVertex();
                        pB.uv.x = info.f;
                        pB.uv.y = _Lerp(info.b.from.uv.x, info.b.to.uv.x, f, info.b.from.uv.y, info.b.to.uv.y);
                        pB.Lerp(info.b.from.uv.x, info.b.to.uv.x, f, info.b.from, info.b.to);

                        newVertices.Add(info.b, pB);
                        vertices.Add(pB);
                    }
                }
                for (int dir = 0; dir < 2; ++dir)
                {
                    XEdge oldFormer = null;
                    XEdge oldLatter = null;
                    XVertex startPoint;
                    XVertex endPoint;
                    if (dir == 0)
                    {
                        oldFormer = info.type == CrossInfo.Type.TwoEdges ? info.a : info.a.next;
                        oldLatter = info.b;
                        startPoint = pB;
                        endPoint = pA;
                    }
                    else
                    {
                        oldFormer = info.b;
                        oldLatter = info.a;
                        startPoint = pA;
                        endPoint = pB;
                    }

                    XEdge newFormer = new XEdge();
                    XEdge newLatter = new XEdge();
                    XEdge newCut = new XEdge();

                    ///> 形成一个三角形，都是新边
                    if (oldFormer.next == oldLatter)
                    {
                        newFormer.next = newLatter;
                        newFormer.prev = newCut;
                        newLatter.prev = newFormer;
                        newLatter.next = newCut;
                        newCut.next = newFormer;
                        newCut.prev = newLatter;
                    }
                    ///> 多边形，有旧边
                    else
                    {
                        newFormer.next = oldFormer.next;
                        oldFormer.next.prev = newFormer;
                        newFormer.prev = newCut;

                        newLatter.next = newCut;
                        oldLatter.prev.next = newLatter;
                        newLatter.prev = oldLatter.prev;

                        newCut.next = newFormer;
                        newCut.prev = newLatter;
                    }

                    newFormer.from = endPoint;
                    newFormer.to = oldFormer.to;
                    newLatter.from = oldLatter.from;
                    newLatter.to = startPoint;
                    newCut.from = startPoint;
                    newCut.to = endPoint;

                    XFace newFace = new XFace();
                    newFace.edge = newFormer;
                    faces.Add(newFace);

                    newFace.RefreshEdges();

                    edges.Remove(oldFormer);
                    edges.Remove(oldLatter);
                    edges.Add(newFormer);
                    edges.Add(newLatter);
                    edges.Add(newCut);

                    _ProcessNewPair(newFormer, newEdges);
                    _ProcessNewPair(newLatter, newEdges);
                    _ProcessNewPair(newCut, newEdges);
                }
            }
        }
    }

    void _Triangulation()
    {
        List<XFace> toDoList = new List<XFace>();
        foreach(XFace face in faces)
        {
            XEdge edge = face.edge;
            int count = 0;
            do
            {
                ++count;
                edge = edge.next;
            }
            while (edge != face.edge);

            if (count > 3)
                toDoList.Add(face);
        }

        foreach(XFace face in toDoList)
        {
            XEdge formerEdge;
            XEdge latterEdge;
            XEdge newEdge;
            XEdge newEdge2;

            XEdge firstEdge = face.edge;
            XEdge nextEdge = firstEdge;

            while (true)
            {
                formerEdge = nextEdge;
                latterEdge = nextEdge.next;
                nextEdge = latterEdge.next;

                if (nextEdge == formerEdge || nextEdge == latterEdge || nextEdge == formerEdge.prev)
                    break;

                newEdge = new XEdge();
                newEdge.from = latterEdge.to;
                newEdge.to = formerEdge.from;

                newEdge2 = new XEdge();
                newEdge2.from = newEdge.to;
                newEdge2.to = newEdge.from;

                formerEdge.SetPrev(newEdge, newEdge2);
                latterEdge.SetNext(newEdge, newEdge2);

                newEdge2.face = face;
                face.edge = newEdge2;

                newEdge2.pair = newEdge;
                newEdge.pair = newEdge2;

                XFace newFace = new XFace();
                newFace.edge = newEdge;
                newFace.RefreshEdges();
                faces.Add(newFace);
            }
        }
    }

    void _CheckU()
    {
        Dictionary<XFace, int> faceDelta = new Dictionary<XFace, int>();
        ///> 如果不在此Dic，表明此顶点只在一个区间。否则，另一个区间的deltaX和新顶点，存在pair里。不可能还有第三个区间
        Dictionary<XVertex, KeyValuePair<int, XVertex>> newVertexs = new Dictionary<XVertex, KeyValuePair<int, XVertex>>();
        ///> 原始顶点的deltaX
        Dictionary<XVertex, int> vertexs = new Dictionary<XVertex, int>();

        foreach (XFace face in faces)
        {
            float minU = float.MaxValue;
            XEdge edge = face.edge;
            do
            {
                XVertex v = edge.from;
                minU = Mathf.Min(v.uv.x, minU);
                edge = edge.next;
            }
            while (edge != face.edge);

            float targetMinU = minU - (int)minU;
            if (targetMinU < 0.0f)
                targetMinU += 1.0f;

            ///> 
            float deltaUf = targetMinU - minU;
            int deltaU = (int)(deltaUf > 0.0f ? (deltaUf + 0.5f) : (deltaUf - 0.5f));

            faceDelta[face] = deltaU;

            edge = face.edge;
            do
            {
                XVertex v = edge.from;
                
                if(!vertexs.ContainsKey(v))
                {
                    vertexs[v] = deltaU;
                }
                else if (deltaU != vertexs[v])
                {
                    ///> 只可能能增加一个点
                    if (!newVertexs.ContainsKey(v))
                    {
                        XVertex newV = new XVertex(v);
                        newV.uv.x += deltaU;
                        newVertexs[v] = new KeyValuePair<int, XVertex>(deltaU, newV);
                        this.vertices.Add(newV);
                    }
                }

                edge = edge.next;
            }
            while (edge != face.edge);
        }
        
        ///> 移动原始顶点
        foreach (var pair in vertexs)
        {
            pair.Key.uv.x += pair.Value;
        }

        ///> 看新顶点
        if (newVertexs.Count > 0)
        {
            foreach (XFace face in faces)
            {
                int deltaU = 0;
                if (!faceDelta.TryGetValue(face, out deltaU))
                    continue;

                XEdge edge = face.edge;
                do
                {
                    XVertex v = edge.from;
                    KeyValuePair<int, XVertex> vPair;
                    XVertex newV = null;
                    if (newVertexs.TryGetValue(v, out vPair))
                    {
                        if (vPair.Value != null && vPair.Key == deltaU)
                        {
                            newV = vPair.Value;
                        }
                    }

                    if (newV != null)
                    {
                        edge.from = newV;
                        edge.prev.to = newV;
                    }

                    edge = edge.next;
                }
                while (edge != face.edge);
            }
        }
    }
}
