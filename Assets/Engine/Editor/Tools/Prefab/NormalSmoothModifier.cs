using System.Collections.Generic;
using UnityEngine;

namespace CFEngine.Editor
{
    public class NormalSmoothModifier
    {
        public static void Modify(Mesh mesh, float threshold, float weight, float average)
        {
            Mesh m = mesh;
            Dictionary<Vector3, List<Vector3>> surfaceNormalDictionary = CreateSurfaceNormalDictionary(m);
            Vector3[] weightedNoramls = CalculateWeightedNoraml(surfaceNormalDictionary,m,threshold,weight);
            Vector3[] averageNormals = CalculateAverageNoraml(surfaceNormalDictionary, m);
            List<Vector3> n = new List<Vector3>();
            for (int i = 0; i < weightedNoramls.Length; i++)
            {
                Vector3 normal = Vector3.Lerp(weightedNoramls[i], averageNormals[i], average);
                n.Add(normal);
            }
            m.normals = n.ToArray();
        }

        public static void ModifyNormals(Mesh mesh, float threshold, float weight, float average, out Vector3[] normals)
        {
            Mesh m = mesh;
            Dictionary<Vector3, List<Vector3>> surfaceNormalDictionary = CreateSurfaceNormalDictionary(m);
            Vector3[] weightedNoramls = new Vector3[mesh.vertexCount];
            Vector3[] averageNormals = new Vector3[mesh.vertexCount];
            List<Vector3> n = new List<Vector3>();
            if (average < 1f)
            {
                weightedNoramls = CalculateWeightedNoraml(surfaceNormalDictionary,m,threshold,weight);
            }
            if (average > 0)
            {
                averageNormals = CalculateAverageNoraml(surfaceNormalDictionary, m);
            }

            if (average == 1f)
            {
                normals = averageNormals;
            }
            else if (average == 0f)
            {
                normals = weightedNoramls;
            }
            else
            {
                for (int i = 0; i < weightedNoramls.Length; i++)
                {
                    Vector3 normal = Vector3.Lerp(weightedNoramls[i], averageNormals[i], average);
                    n.Add(normal);
                }
                normals = n.ToArray();
            }
        }
        
        /// <summary>
        /// key: vetex Position, value: surface Normal.
        /// </summary>
        /// <param name="mesh"></param>
        /// <returns></returns>
        private static Dictionary<Vector3, List<Vector3>> CreateSurfaceNormalDictionary(Mesh mesh)
        {
            Dictionary<Vector3, List<Vector3>> surfaceNormalDictionary = new Dictionary<Vector3, List<Vector3>>();
            
            for (int i = 0; i < mesh.triangles.Length - 3 ; i+=3)
            {
                
                Vector3 a = mesh.vertices[mesh.triangles[i+1]] - mesh.vertices[mesh.triangles[i]];
                Vector3 b = mesh.vertices[mesh.triangles[i+2]] - mesh.vertices[mesh.triangles[i]];
                Vector3 normal = Vector3.Cross(a, b);

                for (int j = 0; j < 3; j++)
                {
                    int tri = mesh.triangles[i + j];
                    if (!surfaceNormalDictionary.ContainsKey(mesh.vertices[tri]))
                    {
                        List<Vector3> noramls = new List<Vector3>();
                        surfaceNormalDictionary.Add(mesh.vertices[tri], noramls);
                    }

                    bool containsNormal = false;


                    for (int k = 0; k < surfaceNormalDictionary[mesh.vertices[tri]].Count; k++)
                    {
                        if (surfaceNormalDictionary[mesh.vertices[tri]][k].normalized.Equals(normal.normalized))
                        {
                            surfaceNormalDictionary[mesh.vertices[tri]][k] += normal;
                            containsNormal = true;
                            break;
                        }
                    }

                    if (!containsNormal)
                    {
                        surfaceNormalDictionary[mesh.vertices[tri]].Add(normal);
                    }
                }
            }
            
            return surfaceNormalDictionary;
        }
        
        private static Vector3[] CalculateAverageNoraml(Dictionary<Vector3, List<Vector3>> surfaceNormalDictionary, Mesh mesh)
        {
            List<Vector3> averageNoramls = new List<Vector3>();
            for (int i = 0; i < mesh.vertices.Length; i++)
            {
                List<Vector3> normals = surfaceNormalDictionary[mesh.vertices[i]];

                Vector3 n = Vector3.zero;
                float ws = 0;
                foreach (var normal in normals)
                {
                    n += normal.normalized;
                }
                
                averageNoramls.Add(n.normalized);
            }
            
            return averageNoramls.ToArray();
        }
        
        private static Vector3[] CalculateWeightedNoraml(Dictionary<Vector3, List<Vector3>> surfaceNormalDictionary, Mesh mesh, float threshold, float weight)
        {
            List<Vector3> weightedNoramls = new List<Vector3>();
            for (int i = 0; i < mesh.vertices.Length; i++)
            {
                List<Vector3> normals = surfaceNormalDictionary[mesh.vertices[i]];

                Vector3 ns = Vector3.zero;
                float ws = 0;
                foreach (var normal in normals)
                {
                    ns += normal;
                    ws += Mathf.Sqrt(normal.sqrMagnitude);
                }

                for (int j = 0; j < normals.Count; j++)
                {
                    float l0 = Mathf.Sqrt(normals[j].sqrMagnitude);
                    float l1 = Mathf.Sqrt((ns - normals[j]).sqrMagnitude);
                    float r = (l0 - l1) / ws;
                    if (r > threshold || 1 + r > 1 - threshold)
                    {
                        ns = Vector3.Lerp((ns - normals[j]), normals[j], weight);
                    }
                }
                weightedNoramls.Add(ns.normalized);
            }
            
            return weightedNoramls.ToArray();
        }
    }
}