using System.Collections.Generic;
using System.IO;
using System.Linq;
using CFEngine;
using UnityEditor;
using UnityEngine;

namespace CFEngine.Editor
{
    internal class MeshAssets
    {

        static Vector3 Average (Vector3[] array, IEnumerable<int> indices)
        {
            Vector3 avg = Vector3.zero;
            int count = 0;

            foreach (int i in indices)
            {
                avg.x += array[i].x;
                avg.y += array[i].y;
                avg.z += array[i].z;

                count++;
            }

            return avg / count;
        }
        static void Cross (float ax, float ay, float az, float bx, float by, float bz, ref float x, ref float y, ref float z)
        {
            x = ay * bz - az * by;
            y = az * bx - ax * bz;
            z = ax * by - ay * bx;
        }
        static Vector3 Normal (Vector3 p0, Vector3 p1, Vector3 p2)
        {
            float ax = p1.x - p0.x,
                ay = p1.y - p0.y,
                az = p1.z - p0.z,
                bx = p2.x - p0.x,
                by = p2.y - p0.y,
                bz = p2.z - p0.z;

            Vector3 cross = Vector3.zero;
            Cross (ax, ay, az, bx, by, bz, ref cross.x, ref cross.y, ref cross.z);
            cross.Normalize ();

            if (cross.magnitude < Mathf.Epsilon)
                return new Vector3 (0f, 0f, 0f); // bad triangle
            else
                return cross;
        }
        public static void RecalculateNormals (Vector3[] vertices, int[] triangles, Vector3[] normal)
        {
            //GetSmoothSeamLookup
            List<List<int>> smooth = null;

            if (normal != null)
            {
                List<List<int>> common;

                int[] ttmp = new int[vertices.Length];
                for (int i = 0; i < ttmp.Length; ++i)
                    ttmp[i] = i;
                common = ttmp.ToLookup (x => (RndVec3) vertices[x]).Select (y => y.ToList ()).ToList ();

                smooth = common
                    .SelectMany (x => x.GroupBy (i => (RndVec3) normal[i]))
                    .Where (n => n.Count () > 1)
                    .Select (t => t.ToList ())
                    .ToList ();

            }
            //calc normal
            Vector3[] perTriangleNormal = new Vector3[vertices.Length];
            int[] perTriangleAvg = new int[vertices.Length];
            int[] tris = triangles;

            for (int i = 0; i < tris.Length; i += 3)
            {
                int a = tris[i], b = tris[i + 1], c = tris[i + 2];

                Vector3 cross = Normal (vertices[a], vertices[b], vertices[c]);

                perTriangleNormal[a].x += cross.x;
                perTriangleNormal[b].x += cross.x;
                perTriangleNormal[c].x += cross.x;

                perTriangleNormal[a].y += cross.y;
                perTriangleNormal[b].y += cross.y;
                perTriangleNormal[c].y += cross.y;

                perTriangleNormal[a].z += cross.z;
                perTriangleNormal[b].z += cross.z;
                perTriangleNormal[c].z += cross.z;

                perTriangleAvg[a]++;
                perTriangleAvg[b]++;
                perTriangleAvg[c]++;
            }

            for (int i = 0; i < vertices.Length; i++)
            {
                normal[i].x = perTriangleNormal[i].x * (float) perTriangleAvg[i];
                normal[i].y = perTriangleNormal[i].y * (float) perTriangleAvg[i];
                normal[i].z = perTriangleNormal[i].z * (float) perTriangleAvg[i];
            }

            if (smooth != null)
            {
                foreach (List<int> l in smooth)
                {
                    Vector3 n = Average (normal, l);

                    foreach (int i in l)
                        normal[i] = n;
                }
            }
        }

        public static Vector4[] SolveTangent (Vector3[] vertices, int[] triangles, Vector3[] normals)
        {
            int triangleCount = triangles.Length / 3;
            int vertexCount = vertices.Length;

            Vector3[] tan1 = new Vector3[vertexCount];
            Vector3[] tan2 = new Vector3[vertexCount];
            Vector4[] tangents = new Vector4[vertexCount];
            for (long a = 0; a < triangleCount; a += 3)
            {
                long i1 = triangles[a + 0];
                long i2 = triangles[a + 1];
                long i3 = triangles[a + 2];
                Vector3 v1 = vertices[i1];
                Vector3 v2 = vertices[i2];
                Vector3 v3 = vertices[i3];

                Vector2 w1 = new Vector2 (v1.x, v1.z);
                Vector2 w2 = new Vector2 (v2.x, v2.z);
                Vector2 w3 = new Vector2 (v3.x, v3.z);

                float x1 = v2.x - v1.x;
                float x2 = v3.x - v1.x;
                float y1 = v2.y - v1.y;
                float y2 = v3.y - v1.y;
                float z1 = v2.z - v1.z;
                float z2 = v3.z - v1.z;
                float s1 = w2.x - w1.x;
                float s2 = w3.x - w1.x;
                float t1 = w2.y - w1.y;
                float t2 = w3.y - w1.y;
                float r = 1.0f / (s1 * t2 - s2 * t1);
                Vector3 sdir = new Vector3 ((t2 * x1 - t1 * x2) * r, (t2 * y1 - t1 * y2) * r, (t2 * z1 - t1 * z2) * r);
                Vector3 tdir = new Vector3 ((s1 * x2 - s2 * x1) * r, (s1 * y2 - s2 * y1) * r, (s1 * z2 - s2 * z1) * r);
                tan1[i1] += sdir;
                tan1[i2] += sdir;
                tan1[i3] += sdir;
                tan2[i1] += tdir;
                tan2[i2] += tdir;
                tan2[i3] += tdir;
            }
            for (long a = 0; a < vertexCount; ++a)
            {
                Vector3 n = normals[a];
                Vector3 t = tan1[a];
                Vector3 tmp = (t - n * Vector3.Dot (n, t)).normalized;
                tangents[a] = new Vector4 (tmp.x, tmp.y, tmp.z);
                tangents[a].w = (Vector3.Dot (Vector3.Cross (n, t), tan2[a]) < 0.0f) ? -1.0f : 1.0f;
            }
            return tangents;
        }

        internal static void MakeMakeReadable (string path)
        {
            Mesh mesh = AssetDatabase.LoadAssetAtPath<Mesh> (path);
            if (mesh != null)
            {
                mesh.UploadMeshData (false);
                SerializedProperty sp = CommonAssets.GetSerializeProperty (mesh, "m_IsReadable");
                if (sp != null)
                {
                    sp.boolValue = true;
                    sp.serializedObject.ApplyModifiedProperties ();
                }
            }
        }
        internal static void GetMeshInfo (Mesh m, out int vertexCount, out int indexCount)
        {
            vertexCount = m.vertexCount;
            indexCount = (int) m.GetIndexCount (0);
        }

        internal static Bounds CalcBounds (Mesh m, Matrix4x4 matrix)
        {
            Vector3[] vertices = m.vertices;
            Bounds bound = new Bounds ();
            for (int i = 0; i < vertices.Length; ++i)
            {
                Vector3 vertex = vertices[i];
                Vector3 worldPos = matrix.MultiplyPoint (vertex);
                if (i == 0)
                {
                    bound.center = worldPos;
                }
                else
                {
                    bound.Encapsulate (worldPos);
                }
            }
            return bound;
        }
        internal static List<CombineInstance> instances = new List<CombineInstance> ();
        internal static Mesh Merge (List<MeshRenderObject> mergeList, int blockID, string sceneName, int startIndex = 0, int endIndex = 0)
        {
            int chunkID = 0;
            int end = endIndex == 0 ? mergeList.Count : endIndex;
            for (int i = startIndex; i < end; ++i)
            {
                var mro = mergeList[i];
                CombineInstance ci = new CombineInstance ();
                ci.mesh = mro.GetMesh ();
                ci.transform = mro.transform.localToWorldMatrix;
                instances.Add (ci);
                // chunkID = mro.chunkID;
            }
            Mesh combineMesh = null;
            if (instances.Count > 1)
            {
                combineMesh = new Mesh ();
                combineMesh.CombineMeshes (instances.ToArray (), true, true, false);
                string path = string.Format ("{0}/Scene/{1}", AssetsConfig.instance.ResourcePath, sceneName);
                string chunkStr = chunkID >= 0 ? chunkID.ToString () : "G";
                string name = string.Format ("M_C{0}_B{1}", chunkStr, blockID);
                combineMesh.name = name;
                combineMesh = CommonAssets.CreateAsset<Mesh> (path, name, ".asset", combineMesh);
            }
            instances.Clear ();
            return combineMesh;
        }

        [MenuItem ("Assets/Tool/Mesh_LightIndex")]
        static void MeshLightIndex ()
        {
            Mesh mesh = new Mesh ();
            mesh.name = "LightIndexGrid";
            mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
            List<Vector3> pos = new List<Vector3> ();
            List<Vector4> uv = new List<Vector4> ();
            List<int> index = new List<int> ();
            float border = 0.1f;
            int lightVoxelSize = (int) (256 / EngineContext.LightGridSize);
            float sizeWithBorder = EngineContext.LightGridSize - border * 2;
            for (int y = 0; y < 2; ++y)
            {
                Vector3 p = new Vector3 ();
                Vector4 posIndex = new Vector4 ();                
                p.y = y * EngineContext.LightGridSize + border;
                posIndex.y = y;
                for (int z = 0; z < lightVoxelSize; ++z)
                {
                    p.z = z * EngineContext.LightGridSize + border;
                    posIndex.z = z;
                    for (int x = 0; x < lightVoxelSize; ++x)
                    {
                        p.x = x * EngineContext.LightGridSize + border;
                        posIndex.x = x;
                        int startIndex = pos.Count;
                        //0
                        pos.Add (p);
                        uv.Add (posIndex);
                        //1
                        pos.Add (p + new Vector3 (sizeWithBorder, 0, 0));
                        uv.Add (posIndex);
                        //2
                        pos.Add (p + new Vector3 (sizeWithBorder, 0, sizeWithBorder));
                        uv.Add (posIndex);
                        //3
                        pos.Add (p + new Vector3 (0, 0, sizeWithBorder));
                        uv.Add (posIndex);

                        //4
                        pos.Add (p + new Vector3 (0, sizeWithBorder, 0));
                        uv.Add (posIndex);
                        //5
                        pos.Add (p + new Vector3 (sizeWithBorder, sizeWithBorder, 0));
                        uv.Add (posIndex);
                        //6
                        pos.Add (p + new Vector3 (sizeWithBorder, sizeWithBorder, sizeWithBorder));
                        uv.Add (posIndex);
                        //7
                        pos.Add (p + new Vector3 (0, sizeWithBorder, sizeWithBorder));
                        uv.Add (posIndex);
                        //bottom
                        index.Add (startIndex + 0);
                        index.Add (startIndex + 1);
                        index.Add (startIndex + 2);

                        index.Add (startIndex + 0);
                        index.Add (startIndex + 2);
                        index.Add (startIndex + 3);
                        //front
                        index.Add (startIndex + 0);
                        index.Add (startIndex + 5);
                        index.Add (startIndex + 1);

                        index.Add (startIndex + 0);
                        index.Add (startIndex + 4);
                        index.Add (startIndex + 5);

                        //right
                        index.Add (startIndex + 1);
                        index.Add (startIndex + 6);
                        index.Add (startIndex + 2);

                        index.Add (startIndex + 1);
                        index.Add (startIndex + 5);
                        index.Add (startIndex + 6);

                        //back
                        index.Add (startIndex + 2);
                        index.Add (startIndex + 6);
                        index.Add (startIndex + 7);

                        index.Add (startIndex + 2);
                        index.Add (startIndex + 7);
                        index.Add (startIndex + 3);

                        //left
                        index.Add (startIndex + 3);
                        index.Add (startIndex + 4);
                        index.Add (startIndex + 0);

                        index.Add (startIndex + 3);
                        index.Add (startIndex + 7);
                        index.Add (startIndex + 4);

                        //top
                        index.Add (startIndex + 4);
                        index.Add (startIndex + 6);
                        index.Add (startIndex + 5);

                        index.Add (startIndex + 4);
                        index.Add (startIndex + 7);
                        index.Add (startIndex + 6);
                    }
                }
            }
            mesh.vertices = pos.ToArray ();
            mesh.SetUVs (0, uv);
            mesh.triangles = index.ToArray ();
            CommonAssets.CreateAsset<Mesh> ("Assets/Engine/Runtime/Shaders/Editor/LightIndexGrid.asset", ".asset", mesh);
        }
    }
}