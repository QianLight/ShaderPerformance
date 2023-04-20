using System;
using System.Collections.Generic;
using UnityEngine;

namespace com.pwrd.hlod.editor
{
    [Serializable]
    public class Cluster
    {
        public string name = "";
        public List<Cluster> clusters = new List<Cluster>();
        public List<Entity> entities = new List<Entity>();
        public HLODResultData hlodResult;
        public float usePercent;
        public Bounds bounds;
        public bool openAlphaTest;
        public float quality;
        
        //override
        public bool useOverrideSetting = false;
        public bool firstChangeOverrideState = true;
        public MeshReductionSetting meshReductionSetting;

        public bool ignoreGenerator;

        private Voxel m_voxel;
        public Voxel voxel
        {
            get
            {
                if (m_voxel == null)
                {
                    m_voxel = CalculateVoxel();
                }
                return m_voxel;
            }
        }

        public Cluster(Entity entity)
        {
            if (!entities.Contains(entity)) entities.Add(entity);
            bounds = CalculateBounds();
        }

        public Cluster(List<Entity> entities)
        {
            AddEntites(entities);
            bounds = CalculateBounds();
        }

        public Cluster(Cluster lhs, Cluster rhs)
        {
            Merge(lhs);
            Merge(rhs);
        }

        public Cluster(List<Cluster> clusters)
        {
            foreach (var cluster in clusters)
            {
                Merge(cluster);
            }

            bounds = CalculateBounds();
            CalculateVoxel();
        }

        public void Merge(Cluster other)
        {
            if (other == null)
                return;
            if (other.clusters.Count <= 0 && other.entities.Count <= 1)
                AddEntites(other.entities);
            else
                clusters.Add(other);
            bounds = CalculateBounds();

            name = "";
        }

        public void AddEntites(Cluster other)
        {
            if (other.entities.Count >= 0)
                AddEntites(other.entities);
            bounds = CalculateBounds();
        }

        protected void AddEntites(List<Entity> entities)
        {
            for (int i = 0; i < entities.Count; i++)
            {
                if (!this.entities.Contains(entities[i]))
                {
                    this.entities.Add(entities[i]);
                }
            }
        }
        
        public void UpdateIgnoreGenerator(bool ignoreGenerator)
        {
            this.ignoreGenerator = ignoreGenerator;
            foreach (var cluster in clusters)
            {
                cluster.UpdateIgnoreGenerator(ignoreGenerator);
            }
        }

        protected Bounds CalculateBounds()
        {
            bool initialize = false;
            foreach (var entity in entities)
            {
                if (!initialize)
                {
                    initialize = true;
                    bounds = entity.extraData.bounds;
                }
                else
                    bounds.Encapsulate(entity.extraData.bounds);
            }

            foreach (var cluster in clusters)
            {
                if (!initialize)
                {
                    initialize = true;
                    bounds = cluster.bounds;
                }
                else
                    bounds.Encapsulate(cluster.bounds);
            }

            return bounds;
        }

        protected Voxel CalculateVoxel()
        {
            Voxel voxel = new Voxel();
            foreach (var entity in entities)
            {
                voxel.Combine(entity.voxel);
            }

            foreach (var cluster in clusters)
            {
                voxel.Combine(cluster.voxel);
            }
            return voxel;
        }

        public void CollectManifests(List<GameObject> manifests)
        {
            foreach (var cluster in clusters)
                cluster.CollectManifests(manifests);

            foreach (var entity in entities)
                manifests.Add(entity.manifest);
        }

        public void CollectRenderers(List<Renderer> renderers)
        {
            foreach (var cluster in clusters)
                cluster.CollectRenderers(renderers);

            foreach (var entity in entities)
            {
                if (!entity.isDecal)
                    renderers.AddRange(entity.FetchRenderer());
            }
        }

        public void CollectStatics(ref int triangleCount, ref int materialCount)
        {
            foreach (var cluster in clusters)
                cluster.CollectStatics(ref triangleCount, ref materialCount);

            foreach (var entity in entities)
            {
                triangleCount += entity.extraData.triangleCount;
                materialCount += entity.extraData.materialCount;
            }
        }

        public int CollectEntityCount()
        {
            int count = 0;
            foreach (var cluster in clusters)
                count += cluster.CollectEntityCount();

            count += entities.Count;
            return count;
        }

        public void DisplayVoxel()
        {
            foreach (var entity in entities)
            {
                entity.DisplayVoxel();
            }
            
            foreach (var cluster in clusters)
            {
                cluster.DisplayVoxel();
            }
        }

        public void UnDisplayVoxel()
        {
            foreach (var entity in entities)
            {
                entity.UnDisplayVoxel();
            }
            
            foreach (var cluster in clusters)
            {
                cluster.UnDisplayVoxel();
            }
        }
    }
}