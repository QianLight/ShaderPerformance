using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;
using Random = UnityEngine.Random;

namespace com.pwrd.hlod.editor
{
    // [Serializable]
    public class Voxel
    {
        public const float CUBE_SIZE = 1f;
        public List<Cube> cubes = new List<Cube>();
        private Dictionary<Vector3Int, Cube> cubeMap = new Dictionary<Vector3Int, Cube>();
        public int size => cubes.Count;

        private GameObject[] displayedGOs;
        private Material displayedMaterial;
        private GameObject goRoot;

        [Serializable]
        public class Cube
        {
            public Vector3Int position;
        }

        private void AddCube(int x, int y, int z)
        {
            var position = new Vector3Int(x, y, z);
            if (cubeMap.ContainsKey(position))
            {
                return;
            }

            var cube = new Cube() {position = position};
            cubes.Add(cube);
            cubeMap.Add(cube.position, cube);
        }

        private void AddCube(Cube cube)
        {
            var position = cube.position;
            if (cubeMap.ContainsKey(position))
            {
                return;
            }

            cubes.Add(cube);
            cubeMap.Add(cube.position, cube);
        }

        public void Combine(Voxel outOne)
        {
            foreach (var cube in outOne.cubes)
            {
                this.AddCube(cube);
            }
        }

        public static Voxel Create(Voxel lhs, Voxel rhs)
        {
            var newOne = new Voxel();
            foreach (var cube in lhs.cubes)
            {
                newOne.AddCube(cube);
            }

            foreach (var cube in rhs.cubes)
            {
                newOne.AddCube(cube);
            }

            return newOne;
        }

        /// <summary>
        /// 只有当两个体素有完全一样的box,才算相交
        /// </summary>
        /// <param name="lhs"></param>
        /// <param name="rhs"></param>
        /// <returns></returns>
        public static bool Intersects(Voxel lhs, Voxel rhs)
        {
            var smallOne = lhs.size <= rhs.size ? lhs : rhs;
            var bigOne = lhs.size <= rhs.size ? rhs : lhs;
            foreach (var cube in smallOne.cubes)
            {
                if (bigOne.cubeMap.ContainsKey(cube.position))
                {
                    return true;
                }
            }

            return false;
        }

        public static float Distance(Voxel lhs, Voxel rhs)
        {
            return 0;
        }

        public static Voxel Create(List<Renderer> renderers)
        {
            var entityVoxel = new Voxel();

            List<MeshCollider> meshColliders = new List<MeshCollider>();
            Bounds bounds = default;
            for (int i = 0; i < renderers.Count; i++)
            {
                var renderer = renderers[i];
                if (i == 0)
                {
                    bounds = GetVoxelBounds(renderer.bounds);
                }
                else
                {
                    bounds.Encapsulate(GetVoxelBounds(renderer.bounds));
                }
                
                var go = new GameObject();
                go.transform.parent = renderer.transform.parent;
                go.transform.position = renderer.transform.position;
                go.transform.rotation = renderer.transform.rotation;
                go.transform.localScale = renderer.transform.localScale;
                var meshCollider = go.AddComponent<MeshCollider>();
                var meshFilter = renderer.transform.GetComponent<MeshFilter>();
                if (meshFilter == null) continue;
                var rendererMesh = meshFilter.sharedMesh;
                if (rendererMesh == null)
                {
                    continue;
                }
                meshCollider.sharedMesh = rendererMesh;
                meshColliders.Add(meshCollider);
            }

            GameObject cubeGO = null;
            for (float x = bounds.min.x; x <= bounds.max.x; x += CUBE_SIZE)
            {
                for (float y = bounds.min.y; y <= bounds.max.y; y += CUBE_SIZE)
                {
                    for (float z = bounds.min.z; z <= bounds.max.z; z += CUBE_SIZE)
                    {
                        var colliders = Physics.OverlapBox(new Vector3(x, y, z) + Vector3.one * CUBE_SIZE * 0.5f,
                            Vector3.one * CUBE_SIZE * 0.5f * 1.1f);

                        bool contain = false;
                        foreach (var meshCollider in meshColliders)
                        {
                            if (colliders.Contains(meshCollider))
                            {
                                contain = true;
                                break;
                            }
                        }
                        
                        if (contain)
                        {
                            entityVoxel.AddCube(Mathf.RoundToInt(x / CUBE_SIZE), Mathf.RoundToInt(y / CUBE_SIZE),
                                Mathf.RoundToInt(z / CUBE_SIZE));

                            //debug
                            if (HLODProvider.Instance != null && HLODProvider.Instance.data.debug)
                            {
                                if(cubeGO == null)
                                    cubeGO = new GameObject();
                                var cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
                                cube.transform.parent = cubeGO.transform;
                                cube.transform.localScale = Vector3.one * CUBE_SIZE * 0.9f;
                                cube.transform.localPosition = new Vector3(x, y, z) + Vector3.one * CUBE_SIZE * 0.5f;
                                var r = cube.GetComponent<Renderer>();
                                r.sharedMaterial = new Material(r.sharedMaterial);
                                r.sharedMaterial.SetColor("_BaseColor", Color.green);
                                r.sharedMaterial.SetColor("_BaseColor", Color.red);
                                r.shadowCastingMode = ShadowCastingMode.Off;
                                r.receiveShadows = false;
                            }
                        }
                    }
                }
            }

            foreach (var meshCollider in meshColliders)
            {
                GameObject.DestroyImmediate(meshCollider.gameObject);
            }
            return entityVoxel;
        }

        private static Bounds GetVoxelBounds(Bounds bounds)
        {
            var min = bounds.min;
            var max = bounds.max;
            min = new Vector3(GetVoxelBoundsValue(min.x, true), GetVoxelBoundsValue(min.y, true),
                GetVoxelBoundsValue(min.z, true));
            max = new Vector3(GetVoxelBoundsValue(max.x), GetVoxelBoundsValue(max.y), GetVoxelBoundsValue(max.z));
            return new Bounds((min + max) / 2, max - min);
        }

        private static float GetVoxelBoundsValue(float v, bool min = false)
        {
            float r = v / CUBE_SIZE;
            if (v % CUBE_SIZE != 0)
            {
                r = (min ? Mathf.Floor(r) : Mathf.Ceil(r)) * CUBE_SIZE;
            }
            else
            {
                r *= CUBE_SIZE;
            }

            return r;
        }

        public void Display()
        {
            if (displayedGOs != null)
            {
                return;
            }

            goRoot = new GameObject("Voxel");
            goRoot.hideFlags = HideFlags.DontSaveInBuild;
            goRoot.tag = "EditorOnly";
            displayedGOs = new GameObject[size];
            displayedMaterial = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            displayedMaterial.color = new Color(Random.Range(0, 256) / 255f, Random.Range(0, 256) / 255f, Random.Range(0, 256) / 255f);

            for (int i = 0; i < cubes.Count; i++)
            {
                var cube = cubes[i];
                var primitiveGO = GameObject.CreatePrimitive(PrimitiveType.Cube);
                primitiveGO.hideFlags = HideFlags.DontSaveInBuild;
                primitiveGO.tag = "EditorOnly";
                primitiveGO.GetComponent<MeshRenderer>().material = displayedMaterial;
                primitiveGO.transform.SetParent(goRoot.transform);
                primitiveGO.transform.position = cube.position;
                displayedGOs[i] = primitiveGO;
            }
        }

        public void UnDisplay()
        {
            if (displayedGOs == null)
            {
                return;
            }

            foreach (var displayedGO in displayedGOs)
            {
                GameObject.DestroyImmediate(displayedGO);
            }

            displayedGOs = null;
            
            GameObject.DestroyImmediate(goRoot);
            GameObject.DestroyImmediate(displayedMaterial);
            displayedMaterial = null;
            goRoot = null;
        }
    }
}