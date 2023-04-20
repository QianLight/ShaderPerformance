using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace com.pwrd.hlod.editor
{
    [Serializable]
    public class Entity
    {
        [Serializable]
        public class ManifestExtraData
        {
            public Bounds bounds;
            public int triangleCount = 0;
            public int materialCount = 0;
        }

        public GameObject prefabRoot;
        public GameObject manifest;
        public ManifestExtraData extraData;
        public LODGroup parentLODGroup;
        public bool isDecal;

        private Voxel m_voxel;
        public Voxel voxel
        {
            get
            {
                if (m_voxel == null)
                {
                    m_voxel = Voxel.Create(FetchRenderer());
                }
                return m_voxel;
            }
        }

        public Entity(GameObject manifest)
        {
            this.manifest = manifest;
            if (manifest)
            {
                this.prefabRoot = PrefabUtility.GetOutermostPrefabInstanceRoot(manifest);
                if (this.prefabRoot == null) this.prefabRoot = manifest;
                parentLODGroup = manifest.GetComponentInParent<LODGroup>();
            }
            FetchExtraData();

            var hlodTag = manifest.GetComponent<HLODDecalTag>();
            if (hlodTag != null)
            {
                isDecal = true;
            }
        }

        public virtual List<Renderer> FetchRenderer()
        {
            if (manifest == null)
                return null;

            return new List<Renderer>() {manifest.GetComponent<Renderer>()};
        }

        public virtual GameObject FectchControlNodeByLODGroup()
        {
            if (parentLODGroup != null)
                return parentLODGroup.gameObject;
            return manifest;
        }
        
        public virtual GameObject FectchControlNodeByPrefabRoot()
        {
            if (prefabRoot != null)
                return prefabRoot;
            return manifest;
        }

        public List<Renderer> FectchLodGroupRenderers()
        {
            var list = new List<Renderer>();
            if (parentLODGroup == null)
                return list;

            var lods = parentLODGroup.GetLODs();
            for (int i = 1; i < lods.Length; i++)
            {
                var lod = lods[i];
                list.AddRange(lod.renderers);
            }

            return list;
        }

        public void DisplayVoxel()
        {
            voxel.Display();
        }

        public void UnDisplayVoxel()
        {
            voxel.UnDisplay();
        }

        private Mesh GetMesh(Renderer renderer)
        {
            var go = renderer.gameObject;
            var meshFilter = go.GetComponent<MeshFilter>();
            if (meshFilter != null)
                return meshFilter.sharedMesh;
            var skinMeshRenderer = renderer as SkinnedMeshRenderer;
            if (skinMeshRenderer != null)
                return skinMeshRenderer.sharedMesh;
            return null;
        }

        private bool FetchExtraData()
        {
            if (manifest == null)
                return false;

            var renderers = FetchRenderer();
            if (renderers.Count == 0)
                return false;

            extraData = new ManifestExtraData();

            Bounds bounds = default;
            for (int i = 0; i < renderers.Count; i++)
            {
                var renderer = renderers[i];
                var mesh = GetMesh(renderer);
                if (mesh == null)
                    continue;

                var originValue = GetMeshReadable(mesh);
                if (!originValue)
                {
                    SetReadable(mesh, true);
                }

                extraData.triangleCount += mesh.triangles.Length / 3;
                extraData.materialCount += renderer.sharedMaterials.Length;
                if (!originValue)
                {
                    SetReadable(mesh, false);
                }

                if (i == 0)
                {
                    bounds = renderer.bounds;
                }
                else
                {
                    bounds.Encapsulate(renderer.bounds);
                }
            }
            extraData.bounds = bounds;

            return true;
        }

        private static bool GetMeshReadable(Object mesh)
        {
            var so = new SerializedObject(mesh);
            var property = so.FindProperty("m_IsReadable");
            var originValue = property.boolValue;
            return originValue;
        }

        private static void SetReadable(Object mesh, bool value)
        {
            var so = new SerializedObject(mesh);
            var property = so.FindProperty("m_IsReadable");
            property.boolValue = value;
            so.ApplyModifiedProperties();
        }
    }
}