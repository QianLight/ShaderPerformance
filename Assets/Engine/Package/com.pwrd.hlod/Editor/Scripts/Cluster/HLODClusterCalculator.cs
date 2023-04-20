using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;
using Random = System.Random;

namespace com.pwrd.hlod.editor
{
    // 聚合算法
    public class HLODClusterCalculator
    {
        private static List<Cluster> m_hasMerged = new List<Cluster>();

        private static Dictionary<Cluster, List<MergeData>> m_ClusterAttachMap =
            new Dictionary<Cluster, List<MergeData>>();

        public static List<Cluster> CollectCluster(List<Cluster> clusters, GenerateClusterMethod clusterMethod, ClusterSetting setting, List<Cluster> layerClusters)
        {
            List<Cluster> collectClusters = new List<Cluster>(clusters);

            if (collectClusters.Count == 0) return collectClusters;
            
            CollectCluster(collectClusters, clusterMethod, setting);

            layerClusters.Clear();
            foreach (var cluster in collectClusters)
            {
                layerClusters.Add(cluster);
            }
            //清除无效簇
            // layerClusters.RemoveAll(s => (s.entities == null || s.entities.Count <= 1) && (s.clusters == null || s.clusters.Count == 0));
            
            collectClusters.AddRange(clusters);
            foreach (var cluster in m_hasMerged)
            {
                if (collectClusters.Contains(cluster))
                    collectClusters.Remove(cluster);
            }
            List<Cluster> lowLevelClusters = new List<Cluster>(collectClusters);

            return lowLevelClusters;
        }
        
        private static void CollectCluster(List<Cluster> clusters, GenerateClusterMethod clusterMethod, ClusterSetting setting)
        {
            EditorUtility.DisplayProgressBar("CollectClusterSimple", "", 0.0f);
            m_ClusterAttachMap.Clear();
            if (clusterMethod == GenerateClusterMethod.ChunkArea)
            {
                EditorUtility.DisplayProgressBar("ChunkAreaMerge", "", 0.0f);
                CollectClusterByChunkArea(clusters, setting);
                EditorUtility.ClearProgressBar();
            }
            else if (clusterMethod == GenerateClusterMethod.QuadTree)
            {
                EditorUtility.DisplayProgressBar("QuadTreeMerge", "", 0.0f);
                CollectClusterByQuadTree(clusters, setting);
                EditorUtility.ClearProgressBar();
            }
            else if (clusterMethod == GenerateClusterMethod.DP)
            {
                EditorUtility.DisplayProgressBar("DPMerge", "", 0.0f);
                CollectClusterByDP(clusters, setting);
                EditorUtility.ClearProgressBar();
            }
            else if (clusterMethod == GenerateClusterMethod.KMeans)
            {
                EditorUtility.DisplayProgressBar("KMeansMerge", "", 0.0f);
                CollectKMeans(clusters, setting); 
                EditorUtility.ClearProgressBar();
            }
            else if (clusterMethod == GenerateClusterMethod.BVHTree)
            {
                EditorUtility.DisplayProgressBar("BVHTreeMerge", "", 0.0f);
                CollectClusterByBVHTree(clusters, setting); 
                EditorUtility.ClearProgressBar();
            }
            else if (clusterMethod == GenerateClusterMethod.Octree)
            {
                EditorUtility.DisplayProgressBar("OctreeMerge", "", 0.0f);
                CollectClusterByOctree(clusters, setting); 
                EditorUtility.ClearProgressBar();
            }
            else
            {
                var list = CollectClusterSimple(clusters, setting);

                List<Cluster> clusterList = null;
                if (clusterMethod == GenerateClusterMethod.DeepFirst)
                {
                    EditorUtility.DisplayProgressBar("DeepFirstMerge", "", 0.0f);
                    clusterList = DeepFirstMerge(list, setting);
                }
                else if (clusterMethod == GenerateClusterMethod.UE4_Triple)
                {
                    EditorUtility.DisplayProgressBar("TripleMerge", "", 0.0f);
                    clusterList = TripleMerge(list, setting);
                }
                else
                {

                }

                EditorUtility.ClearProgressBar();

                clusters.Clear();
                clusters.AddRange(clusterList);
            }
        }

        #region Simple Filter
        
        /// <summary>
        /// simple filter
        /// </summary>
        private static List<Cluster> FilterBounds(List<Cluster> clusters, ClusterSetting setting)
        {
            if (setting == null) return clusters;
            //过滤空
            clusters.RemoveAll(s => s == null);
            foreach (var cluster in clusters)
            {
                cluster.entities.RemoveAll(s => s.FetchRenderer() == null || s.FetchRenderer().Count == 0);
            }
            //过滤小于最小直径
            clusters.RemoveAll(s => MaxDiameterIsOutDiameter(s, setting.clusterMinDiameter, setting.clusterMaxDiameter));

            return clusters;
        }
        
        /// <summary>
        /// cluster的最大直径是否超出直径边界
        /// </summary>
        private static bool MaxDiameterIsOutDiameter(Cluster cluster, float minDiameter, float maxDiameter)
        {
            bool flag = false;
            var rendererBoundsMaxDiameter = GetRendererBoundsMaxDiameter(cluster);
            if (rendererBoundsMaxDiameter < minDiameter || rendererBoundsMaxDiameter > maxDiameter)
            {
                flag = true;
            }
            return flag;
        }
        
        /// <summary>
        /// 获取最大直径
        /// </summary>
        private static float GetRendererBoundsMaxDiameter(Cluster cluster)
        {
            var bounds = cluster.bounds;
            return Mathf.Max(Mathf.Max(bounds.size.x, bounds.size.y), bounds.size.z);
        }

        #endregion

        #region ChunkArea

        private static void RemoveMergeCluster(List<Cluster> clusters)
        {
            m_hasMerged.Clear();
            foreach (var cluster in clusters)
            {
                if (cluster.clusters != null && cluster.clusters.Count > 0)
                {
                    foreach (var child in cluster.clusters)
                    {
                        m_hasMerged.Add(child);
                        foreach (var entity in child.entities)
                        {
                            if (cluster.entities.Contains(entity))
                            {
                                cluster.entities.Remove(entity);
                            }
                        }
                    }
                }
            }
        }
        
        private static void CollectClusterByChunkArea(List<Cluster> clusters,  ClusterSetting setting)
        {
            clusters = FilterBounds(clusters, setting);
            var targetClusters = ClusterByChunckArea(clusters, setting.startPos, setting.endPos, setting.horizonalChunckCount, setting.verticalChunckCount);

            RemoveMergeCluster(targetClusters);
            clusters.Clear();
            clusters.AddRange(targetClusters);
        }
        
        /// <summary>
        /// 通过块区域收集（模拟地形地块）
        /// </summary>
        public static List<Cluster> ClusterByChunckArea(List<Cluster> clusters, Vector3 startPos, Vector3 endPos, int horizonalChunckCount = 2, int verticalChunckCount = 2)
        {
            List<ChunckAreaData> chunckAreaDatas = GetChunckAreaDatas(startPos, endPos, horizonalChunckCount, verticalChunckCount);
            return ClusterByChunckAreaDatas(clusters, startPos, endPos, chunckAreaDatas);
        }

        /// <summary>
        /// 通过块区域收集
        /// </summary>
        public static List<Cluster> ClusterByChunckAreaDatas(List<Cluster> clusters, Vector3 startPos, Vector3 endPos, List<ChunckAreaData> chunckAreaDatas)
        {
            if (chunckAreaDatas.Count == 0)
            {
                return clusters;
            }
            List<Cluster> reClusters = new List<Cluster>();
            for (int i = 0; i < chunckAreaDatas.Count; i++)
            {
                var chunkAreaData = chunckAreaDatas[i];
                var list = new List<Cluster>();

                foreach (var cluster in clusters)
                {
                    var pos = cluster.bounds.center;
                    if (pos.x < startPos.x) pos.x = startPos.x;
                    if (pos.z < startPos.z) pos.z = startPos.z;
                    if (pos.x >= endPos.x) pos.x = endPos.x - 1;
                    if (pos.z >= endPos.z) pos.z = endPos.z - 1;
                    if (pos.x >= chunkAreaData.startX &&
                        pos.z >= chunkAreaData.startZ &&
                        pos.x < chunkAreaData.endX &&
                        pos.z < chunkAreaData.endZ)
                    {
                        list.Add(cluster);
                    }
                }
                reClusters.Add(new Cluster(list));
                EditorUtility.DisplayProgressBar("ClusterByChunckAreaDatas", i + "/" + chunckAreaDatas.Count, (float) i / chunckAreaDatas.Count);
            }

            return reClusters;
        }
        
        private static List<ChunckAreaData> GetChunckAreaDatas(Vector3 startPos, Vector3 endPos, int horizonalChunckCount = 2, int verticalChunckCount = 2)
        {
            float sumWidth = endPos.x - startPos.x;
            float sumHigh = endPos.z - startPos.z;
            float perWidth = sumWidth / horizonalChunckCount;
            float perHigh = sumHigh / verticalChunckCount;

            List<ChunckAreaData> chunckAreaDatas = new List<ChunckAreaData>();
            for (int i = 0; i < verticalChunckCount; i++)
            {
                for (int j = 0; j < horizonalChunckCount; j++)
                {
                    var startX = startPos.x + perWidth * j;
                    var startZ = startPos.z + perHigh * i;
                    var endX = startX + perWidth;
                    var endZ = startZ + perHigh;
                    var chunckAreaData = new ChunckAreaData(startX, endX, startZ, endZ);
                    chunckAreaDatas.Add(chunckAreaData);
                }
            }
            return chunckAreaDatas;
        }
        
        public class ChunckAreaData
        {
            public float startX;
            public float startZ;
            public float endX;
            public float endZ;

            public Vector2 center;

            public ChunckAreaData(float startX, float endX, float startZ, float endZ)
            {
                this.startX = startX;
                this.endX = endX;
                this.startZ = startZ;
                this.endZ = endZ;
                InitCenter();
            }
            
            public void InitCenter()
            {
                var centerX = (startX + endX) * 0.5f;
                var centerZ = (startZ + endZ) * 0.5f;
                center = new Vector2(centerX, centerZ);
            }
        }
        
        #endregion
        
        #region QuadTree
        
        private static void CollectClusterByQuadTree(List<Cluster> clusters,  ClusterSetting setting)
        {
            clusters = FilterBounds(clusters, setting);

            Bounds bounds = new Bounds();
            if (clusters.Count > 0)
            {
                bounds = clusters[0].bounds;
                for (int i = 1; i < clusters.Count; i++)
                {
                    bounds.Encapsulate(clusters[i].bounds);
                }
            }
            if (!setting.tree_UseSelectBounds)
            {
                Vector3 center = setting.tree_Center;
                Vector3 size = setting.tree_Size;
                bounds = new Bounds(center, size);
            }
            
            var targetClusters = ClusterByQuadTree(clusters, bounds, setting.tree_Depth);

            RemoveMergeCluster(targetClusters);
            clusters.Clear();
            clusters.AddRange(targetClusters);
        }

        /// <summary>
        /// 通过四叉树收集
        /// </summary>
        public static List<Cluster> ClusterByQuadTree(List<Cluster> clusters, Bounds bounds, int maxDepth)
        {
            QuadTree<Cluster> quadTree = new QuadTree<Cluster>(bounds, maxDepth);
            for (int i = 0; i < clusters.Count; i++)
            {
                var cluster = clusters[i];
                var size = cluster.bounds.size;
                size.y = 0;
                quadTree.Insert(cluster, new Bounds(cluster.bounds.center, size));
                EditorUtility.DisplayProgressBar("ClusterByQuadTree", i + "/" + clusters.Count, (float) i / clusters.Count);
            }
            var cList = GetQuadNodeItems(quadTree.rootNode, maxDepth);
            var reClusters = new List<Cluster>();
            foreach (var list in cList)
            {
                reClusters.Add(new Cluster(list));
            }
            return reClusters;
        }

        /// <summary>
        /// 递归四叉树
        /// </summary>
        private static List<List<Cluster>> GetQuadNodeItems(QuadTree<Cluster>.QuadTreeNode node, int maxDepth)
        {
            var list = new List<List<Cluster>>();
            if (node != null && node.depth <= maxDepth)
            {
                var ll = node.GetQuadrantNode(QuadTree<Cluster>.Quadrant.DL);
                var lr = node.GetQuadrantNode(QuadTree<Cluster>.Quadrant.DR);
                var ul = node.GetQuadrantNode(QuadTree<Cluster>.Quadrant.UL);
                var ur = node.GetQuadrantNode(QuadTree<Cluster>.Quadrant.UR);
                list.AddRange(GetQuadNodeItems(ll, maxDepth));
                list.AddRange(GetQuadNodeItems(lr, maxDepth));
                list.AddRange(GetQuadNodeItems(ul, maxDepth));
                list.AddRange(GetQuadNodeItems(ur, maxDepth));
                
                if (node.GetItems().Count > 0) list.Add(node.GetItems());
            }
            
            return list;
        }
        
        #endregion
        
        #region Octree
        
        private static void CollectClusterByOctree(List<Cluster> clusters,  ClusterSetting setting)
        {
            clusters = FilterBounds(clusters, setting);

            Bounds bounds = new Bounds();
            if (clusters.Count > 0)
            {
                bounds = clusters[0].bounds;
                for (int i = 1; i < clusters.Count; i++)
                {
                    bounds.Encapsulate(clusters[i].bounds);
                }
            }
            if (!setting.tree_UseSelectBounds)
            {
                Vector3 center = setting.tree_Center;
                Vector3 size = setting.tree_Size;
                bounds = new Bounds(center, size);
            }
            
            var targetClusters = ClusterByOctree(clusters, bounds, setting.tree_Depth);
           
            RemoveMergeCluster(targetClusters);
            clusters.Clear();
            clusters.AddRange(targetClusters);
        }

        /// <summary>
        /// 通过八叉树收集
        /// </summary>
        public static List<Cluster> ClusterByOctree(List<Cluster> clusters, Bounds bounds, int maxDepth)
        {
            Octree<Cluster> quadTree = new Octree<Cluster>(bounds, maxDepth);
            for (int i = 0; i < clusters.Count; i++)
            {
                var cluster = clusters[i];
                quadTree.Insert(cluster, cluster.bounds);
                EditorUtility.DisplayProgressBar("ClusterByQuadTree", i + "/" + clusters.Count, (float) i / clusters.Count);
            }
            var cList = GetOctreeNodeItems(quadTree.rootNode, maxDepth);
            var reClusters = new List<Cluster>();
            foreach (var list in cList)
            {
                reClusters.Add(new Cluster(list));
            }
            return reClusters;
        }

        /// <summary>
        /// 递归八叉树
        /// </summary>
        private static List<List<Cluster>> GetOctreeNodeItems(Octree<Cluster>.OctreeNode node, int maxDepth)
        {
            var list = new List<List<Cluster>>();
            if (node != null && node.depth <= maxDepth)
            {
                var URB_Node = node.GetQuadrantNode(Octree<Cluster>.Quadrant.DLB);
                var URF_Node = node.GetQuadrantNode(Octree<Cluster>.Quadrant.DRB);
                var ULF_Node = node.GetQuadrantNode(Octree<Cluster>.Quadrant.DLF);
                var ULB_Node = node.GetQuadrantNode(Octree<Cluster>.Quadrant.DRF);
                var DRB_Node = node.GetQuadrantNode(Octree<Cluster>.Quadrant.ULB);
                var DRF_Node = node.GetQuadrantNode(Octree<Cluster>.Quadrant.URB);
                var DLF_Node = node.GetQuadrantNode(Octree<Cluster>.Quadrant.ULF);
                var DLB_Node = node.GetQuadrantNode(Octree<Cluster>.Quadrant.URF);
                list.AddRange(GetOctreeNodeItems(URB_Node, maxDepth));
                list.AddRange(GetOctreeNodeItems(URF_Node, maxDepth));
                list.AddRange(GetOctreeNodeItems(ULF_Node, maxDepth));
                list.AddRange(GetOctreeNodeItems(ULB_Node, maxDepth));
                list.AddRange(GetOctreeNodeItems(DRB_Node, maxDepth));
                list.AddRange(GetOctreeNodeItems(DRF_Node, maxDepth));
                list.AddRange(GetOctreeNodeItems(DLF_Node, maxDepth));
                list.AddRange(GetOctreeNodeItems(DLB_Node, maxDepth));
                
                if (node.GetItems().Count > 0) list.Add(node.GetItems());
            }
            
            return list;
        }
        
        #endregion

        #region BVHTree

        private static void CollectClusterByBVHTree(List<Cluster> clusters, ClusterSetting setting)
        {
            clusters = FilterBounds(clusters, setting);

            BVHTree<Cluster> bvhTree = new BVHTree<Cluster>();
            foreach (var cluster in clusters)
            {
                bvhTree.InsertBVHData(cluster, cluster.bounds);
            }
            var node = bvhTree.BuildBVHTree(setting.bvh_SplitCount);
            
            var cList = GetBVHData(node, setting.bvh_Depth);
            var targetClusters = new List<Cluster>();
            foreach (var list in cList)
            {
                targetClusters.Add(new Cluster(list));
            }
            
            RemoveMergeCluster(targetClusters);
            clusters.Clear();
            clusters.AddRange(targetClusters);
        }
        
        /// <summary>
        /// 递归BVH树
        /// </summary>
        private static List<List<Cluster>> GetBVHData(BVHTree<Cluster>.BVHNode node, int maxDepth)
        {
            var list = new List<List<Cluster>>();

            var nodes = GetBVHNodes(node, maxDepth);
            foreach (var bvhNode in nodes)
            {
                list.Add(GetBVHNodeItems(bvhNode).ToList());
            }
            
            return list;
        }

        private static List<BVHTree<Cluster>.BVHNode> GetBVHNodes(BVHTree<Cluster>.BVHNode node, int maxDepth)
        {
            List<BVHTree<Cluster>.BVHNode> nodes = new List<BVHTree<Cluster>.BVHNode>();
            if (node.depth > maxDepth) return nodes;
            if (node != null)
            {
                if (node.depth == maxDepth)
                {
                    nodes.Add(node);
                }
                else if (node.left == null && node.right == null)
                {
                    nodes.Add(node);
                }
                else
                {
                    nodes.AddRange(GetBVHNodes(node.left, maxDepth));
                    nodes.AddRange(GetBVHNodes(node.right, maxDepth));
                }
            }
            return nodes;
        }
        
        private static HashSet<Cluster> GetBVHNodeItems(BVHTree<Cluster>.BVHNode node)
        {
            var list = new HashSet<Cluster>();
            if (node != null)
            {
                list.UnionWith(GetBVHNodeItems(node.left));
                list.UnionWith(GetBVHNodeItems(node.right));
                list.UnionWith(node.datas);
            }
            list.RemoveWhere(s => s == null);
            return list;
        }

        #endregion

        #region DP
        public static void CollectClusterByDP(List<Cluster> clusters,  ClusterSetting setting)
        {
            clusters = FilterBounds(clusters, setting);
            
            // var list = CollectClusterByBounds(allClusters, 12.5f);
            var targetClusters = CollectClusterByDP(clusters, setting.clusterMaxDiameter, setting.dp_BestValue);
            
            RemoveMergeCluster(targetClusters);
            clusters.Clear();
            clusters.AddRange(targetClusters);
        }

        private static List<Cluster> CollectClusterByDP(List<Cluster> clusters, float maxDiameter, DPBestValue dpBestValue)
        {
            if (maxDiameter == 0) return clusters;
            //dp n -> dp(n-1)合并
            //dp n-1 -> dp(n-2)合并
            //dp 0 -> all
            int multiple = 2;
            int maxDiameter10 = (int)maxDiameter * multiple;
            List<Cluster>[] dp = new List<Cluster>[maxDiameter10];
            dp[0] = new List<Cluster>(clusters);
            for (int i = 1; i < maxDiameter10; i++)
            {
                var last = dp[i - 1];
                //合并last
                dp[i] = CollectClusterByBounds(last, i * (1.0f / multiple), dpBestValue);
                EditorUtility.DisplayProgressBar("CollectClusterByDP", i + "/" + maxDiameter10, (float) i / maxDiameter10);
            }
            
            return dp[dp.Length - 1];
        }

        public static List<Cluster> CollectClusterByBounds(List<Cluster> clusters, float maxDiameter, DPBestValue dpBestValue)
        {
            var tmp = new List<Cluster>(clusters);
            List<List<Cluster>> ls = new List<List<Cluster>>();
            while (tmp.Count > 0)
            {
                var l = GetBestItem(tmp, maxDiameter, dpBestValue); 
                ls.Add(l);
                tmp.RemoveAll(s => l.Contains(s));
            }

            List<Cluster> targetClusters = new List<Cluster>();
            foreach (var list in ls)
            {
                // List<Entity> entities = new List<Entity>();
                // foreach (var cluster in list)
                // {
                //     entities.AddRange(cluster.entities);
                // }
                targetClusters.Add(new Cluster(list));
            }
            return targetClusters;
        }
        
        //获取最优项
        public static List<Cluster> GetBestItem(List<Cluster> clusters, float maxDiameter, DPBestValue dpBestValue)
        {
            if (clusters.Count <= 1) return new List<Cluster>(clusters);

            Dictionary<Cluster, List<Cluster>> dic = new Dictionary<Cluster, List<Cluster>>();
            for (int i = 0; i < clusters.Count; i++)
            {
                Cluster cur = clusters[i];
                Cluster next;
                Bounds result = cur.bounds;
                dic.Add(cur, new List<Cluster>() {cur});
                for (int j = 0; j < clusters.Count; j++)
                {
                    next = clusters[j];
                    if (cur.Equals(next)) continue;
                    var tmp = new Bounds(result.center, result.size);
                    tmp.Encapsulate(next.bounds);
                    var d = Mathf.Max(tmp.size.x, tmp.size.y, tmp.size.z);
                    if (d < maxDiameter)
                    {
                        result = tmp;
                        dic[cur].Add(next);
                    }
                }
            }

            var l = dic.Values.ToList();
            
            List<Cluster> best = new List<Cluster>();
            if (dpBestValue == DPBestValue.MinBoundsSize)
            {
                //聚合bounds为最优时：
                l.Sort((x, y) =>
                {
                    var bounds1 = new Bounds();
                    var bounds2 = new Bounds();
                    foreach (var cluster in x)
                    {
                        bounds1.Encapsulate(cluster.bounds);
                    }
                    foreach (var cluster in y)
                    {
                        bounds2.Encapsulate(cluster.bounds);
                    }
                
                    if (bounds1.size.magnitude < bounds2.size.magnitude) return 1;
                    else if (bounds1.size.magnitude == bounds2.size.magnitude) return 0;
                    else return -1;
                });
                best = l[l.Count - 1];
            }
            else if (dpBestValue == DPBestValue.MaxCount)
            {
                //聚合数量为最优时：
                l = l.OrderBy(s => s.Count).ToList();
                best = l[l.Count - 1];
            }
            return best;
        }
        #endregion

        #region K-Means
        public static void CollectKMeans(List<Cluster> clusters, ClusterSetting setting)
        {
            List<Bounds> bounds = new List<Bounds>();
            foreach (var cluster in clusters)
            {
                bounds.Add(cluster.bounds);
            }
            
            var meansResults = KMeans.Cluster(bounds.ToArray(), setting.clusterCount, setting.maxIterations, 0);

            List<List<Cluster>> lists = new List<List<Cluster>>();
            for (int i = 0; i < meansResults.clusters.Length; i++)
            {
                List<Cluster> list = new List<Cluster>();
                for (int j = 0; j < meansResults.clusters[i].Length; j++)
                {
                    var cluster = clusters[meansResults.clusters[i][j]];
                    list.Add(cluster);
                }
                lists.Add(list);
            }

            var targetClusters = new List<Cluster>();
            foreach (var list in lists)
            {
                targetClusters.Add(new Cluster(list));
            }
            
            RemoveMergeCluster(targetClusters);
            clusters.Clear();
            clusters.AddRange(targetClusters);
        }
        #endregion

        #region DeepFirst & TripleMerge
        
        //将mainData中的数据能挪到subData的移除
        //不能被移除的,在subData中移除
        private static void SplitAttachData(MergeData operateData, MergeData reduceData)
        {
            var mainLockedList = operateData.lockedChildren;
            var subLockedList = reduceData.children;

            var commonList = new List<Cluster>();
            foreach (var child in operateData.children)
            {
                if (reduceData.children.Contains(child))
                {
                    commonList.Add(child);
                }
            }

            foreach (var cluster in commonList)
            {
                if (!operateData.lockedChildren.Contains(cluster) || reduceData.lockedChildren.Contains(cluster))
                {
                    operateData.children.Remove(cluster);
                    operateData.lockedChildren.Remove(cluster);
                    continue;
                }

                reduceData.children.Remove(cluster);
            }

            operateData.Update();
            reduceData.Update();
        }

        private static List<MergeData> CollectClusterSimple(List<Cluster> clusters, ClusterSetting setting)
        {
            var list = new List<MergeData>();
            if (clusters == null || setting == null || clusters.Count <= 0)
                return list;

            int process = 0;
            for (int i = 0; i < clusters.Count; i++)
            {
                if (i - process > clusters.Count * 0.01)
                {
                    process = i;
                    EditorUtility.DisplayProgressBar("CollectClusterSimple", i + "/" + clusters.Count, (float) i / clusters.Count);
                }

                var lhs = clusters[i];
                float clusterDiameter = Mathf.Max(Mathf.Max(lhs.bounds.size.x, lhs.bounds.size.y), lhs.bounds.size.z);
                if (clusterDiameter > setting.clusterMaxDiameter || clusterDiameter < setting.clusterMinDiameter)
                    continue;

                for (int j = i + 1; j < clusters.Count; j++)
                {
                    var rhs = clusters[j];

                    if (rhs == lhs)
                        continue;

                    var mergeData = MergeData.TryMergeCluster(lhs, rhs, setting);
                    if (mergeData == null)
                        continue;

                    list.Add(mergeData);

                    if (!m_ClusterAttachMap.ContainsKey(lhs))
                    {
                        m_ClusterAttachMap[lhs] = new List<MergeData>();
                    }

                    if (!m_ClusterAttachMap.ContainsKey(rhs))
                    {
                        m_ClusterAttachMap[rhs] = new List<MergeData>();
                    }

                    m_ClusterAttachMap[lhs].Add(mergeData);
                    m_ClusterAttachMap[rhs].Add(mergeData);
                }
            }

            return list;
        }

        private static List<Cluster> DeepFirstMerge(List<MergeData> list, ClusterSetting setting)
        {
            HLODDebug.Log("[HLOD][CollectCluster] Use Deep First Merge Algorithm");
            
            int process = 0;
            int lastProcess = 0;
            for(int i = 0; i < list.Count; i++)
            {
                //防止更新进度条耗时太长
                if ((i - process) > list.Count * 0.01f)
                {
                    process = i;
                    EditorUtility.DisplayProgressBar("DeepFirstMerge", "Init Data", (float) (process) / (list.Count));
                }
                
                var data = list[i];
                data.Init();
            }

            list.Sort();

            //第一轮
            for (int i = 0; i < list.Count; i++)
            {
                var curData = list[i];
                if (!curData.validate)
                    continue;

                if (!curData.isClusterIntersets)
                    break;

                process++;
                EditorUtility.DisplayProgressBar("DeepFirstMerge", process + "/" + list.Count, (float) (process) / (list.Count));
                var stack = new Stack<Cluster>(curData.children);

                while (stack.Count > 0)
                {
                    var cluster = stack.Pop();
                    var dataList = new List<MergeData>(m_ClusterAttachMap[cluster]);
                    for (int j = 0; j < dataList.Count; j++)
                    {
                        var data = dataList[j];
                        if (data == curData || !data.validate || !data.isClusterIntersets)
                            continue;
                        if (curData.CanAdd(data))
                        {
                            var newAdd = curData.Add(data);
                            foreach (var newCluster in newAdd)
                            {
                                stack.Push(newCluster);
                                m_ClusterAttachMap[newCluster].Add(curData);
                            }

                            data.validate = false;
                        }
                        else
                        {
                            SplitAttachData(data, curData);
                        }

                        //如果data被并入,则data直接失效了,不需要再访问
                        //如果data不被并入,则里面的共同cluster被删除,不能再通过cluster关联.
                        foreach (var attachCluster in curData.GetAttachList(data))
                        {
                            m_ClusterAttachMap[attachCluster].Remove(data);
                        }

                        if (!data.validate)
                            process++;
                        //防止长时间无反馈
                        if ((process - lastProcess) > list.Count * 0.001f)
                        {
                            lastProcess = process;
                            EditorUtility.DisplayProgressBar("DeepFirstMerge", process + "/" + list.Count,
                                (float) (process) / (list.Count));
                        }
                    }
                }
            }

            process = 0;
            lastProcess = 0;
            for (int i = 0; i < list.Count; i++)
            {
                var curData = list[i];
                if (!curData.validate)
                    continue;

                process++;
                EditorUtility.DisplayProgressBar("DeepFirstMerge", process + "/" + list.Count,
                    (float) (process) / (list.Count));
                var stack = new Stack<Cluster>(curData.children);

                while (stack.Count > 0)
                {
                    var cluster = stack.Pop();
                    var dataList = new List<MergeData>(m_ClusterAttachMap[cluster]);
                    for (int j = 0; j < dataList.Count; j++)
                    {
                        var data = dataList[j];
                        if (data == curData || !data.validate)
                            continue;
                        if (curData.CanAdd(data))
                        {
                            var newAdd = curData.Add(data);
                            foreach (var newCluster in newAdd)
                            {
                                stack.Push(newCluster);
                                m_ClusterAttachMap[newCluster].Add(curData);
                            }

                            data.validate = false;
                        }
                        else
                        {
                            SplitAttachData(data, curData);
                        }

                        //如果data被并入,则data直接失效了,不需要再访问
                        //如果data不被并入,则里面的共同cluster被删除,不能再通过cluster关联.
                        foreach (var attachCluster in curData.GetAttachList(data))
                        {
                            m_ClusterAttachMap[attachCluster].Remove(data);
                        }

                        if (!data.validate)
                            process++;
                        //防止长时间无反馈
                        if ((process - lastProcess) > list.Count * 0.001f)
                        {
                            lastProcess = process;
                            EditorUtility.DisplayProgressBar("DeepFirstMerge", process + "/" + list.Count,
                                (float) (process) / (list.Count));
                        }
                    }
                }
            }

            var clusterList = new List<Cluster>();
            m_hasMerged.Clear();
            foreach (var data in list)
            {
                if (!data.validate)
                    continue;
                Cluster newCluster = new Cluster(data.children);
                if (newCluster.CollectEntityCount() < setting.mergeRendererMinCount)
                    continue;
                foreach (var child in data.children)
                {
                    m_hasMerged.Add(child);
                }

                clusterList.Add(newCluster);
            }

            return clusterList;
        }

        private static List<Cluster> TripleMerge(List<MergeData> list, ClusterSetting setting)
        {
            HLODDebug.Log("[HLOS][CollectCluster] Use UE4 Merge Algorithm");
            int process = 0;
            for(int i =0;i <list.Count;i++)
            {
                //防止更新进度条耗时太长
                if ((i - process) > list.Count * 0.01f)
                {
                    process = i;
                    EditorUtility.DisplayProgressBar("TripleMerge", "Init Data", (float) (process) / (list.Count));
                }
                var data = list[i];
                data.Init();
            }

            list.Sort();
            int times = 3;
            for (int t = 0; t < times; t++)
            {
                bool hasChanged = false;
                for (int i = 0; i < list.Count; i++)
                {
                    var data = list[i];
                    if (!data.validate)
                    {
                        continue;
                    }

                    EditorUtility.DisplayProgressBar("TripleMerge", i + list.Count * t + "/" + list.Count * 3,
                        (float) (i + list.Count * t) / (list.Count * 3));

                    for (int j = 0; j < i; j++)
                    {
                        var mergeTargetData = list[j];
                        if (!mergeTargetData.validate)
                            continue;

                        if (mergeTargetData.Attach(data))
                        {
                            if (mergeTargetData.CanAdd(data))
                            {
                                mergeTargetData.Add(data);
                                data.validate = false;
                                hasChanged |= true;
                            }
                            else
                            {
                                SplitAttachData(data, mergeTargetData);
                                //data.Reduce(mergeTargetData);
                            }
                        }
                    }
                }

                if (!hasChanged)
                    break;
            }

            var clusterList = new List<Cluster>();
            m_hasMerged.Clear();
            foreach (var data in list)
            {
                if (!data.validate)
                    continue;
                Cluster newCluster = new Cluster(data.children);
                if (newCluster.CollectEntityCount() < setting.mergeRendererMinCount)
                    continue;
                foreach (var child in data.children)
                {
                    m_hasMerged.Add(child);
                }

                clusterList.Add(newCluster);
            }

            return clusterList;
        }

        class MergeData : IComparable<MergeData>
        {
            public Cluster lhs;
            public Cluster rhs;
            public bool validate;
            public List<Cluster> children = new List<Cluster>();
            public List<Cluster> lockedChildren = new List<Cluster>(); //锁定列表,不能被Reduce的数据,但也是相对的,如果都锁定,也可以强行拆分
            public bool isClusterIntersets;
            public Voxel voxel;

            private Bounds m_bounds;
            private ClusterSetting setting;

            public MergeData()
            {
            }

            public void Init()
            {
                validate = true;
                children.Add(lhs);
                children.Add(rhs);

                if (IsClusterIntersets())
                {
                    lockedChildren.Add(rhs);
                    lockedChildren.Add((lhs));
                }

                var lhsBounds = GetBounds(lhs);
                var rhsBounds = GetBounds(rhs);
                m_bounds = lhsBounds;
                m_bounds.Encapsulate(rhsBounds);

                voxel = Voxel.Create(lhs.voxel, rhs.voxel);
            }

            private static Bounds GetBounds(Cluster cluster)
            {
                // Bounds mbounds = new Bounds();
                // bool notset = true;
                // var list = new List<GameObject>();
                // cluster.CollectManifests(list);
                //
                // foreach (var go in list)
                // {
                //     var renders = go.GetComponents<Renderer>();
                //     foreach (var r in renders)
                //     {
                //         if (notset)
                //         {
                //             mbounds = r.bounds;
                //             notset = false;
                //         }
                //         else
                //         {
                //             mbounds.Encapsulate(r.bounds);
                //         }
                //     }
                // }

                return cluster.bounds;
            }

            public List<Cluster> GetAttachList(MergeData data)
            {
                var list = new List<Cluster>();
                foreach (var cluster in children)
                {
                    if (data.children.Contains(cluster))
                        list.Add(cluster);
                }

                return list;
            }

            public List<Cluster> Add(MergeData data)
            {
                var increaseList = new List<Cluster>();
                var lhsBounds = m_bounds;
                var rhsBounds = data.m_bounds;

                var bounds = lhsBounds;
                bounds.Encapsulate(rhsBounds);
                m_bounds = bounds;

                foreach (var child in data.children)
                {
                    if (!children.Contains(child))
                    {
                        children.Add(child);
                        increaseList.Add(child);

                        //和初始的lhs或者rhs相交的,都送到lock里面
                        bool locked = Voxel.Intersects(lhs.voxel, child.voxel) ||
                                      Voxel.Intersects(child.voxel, rhs.voxel);
                        if (locked)
                        {
                            if (!lockedChildren.Contains(child))
                            {
                                lockedChildren.Add(child);
                            }
                        }
                    }
                }

                return increaseList;
            }

            public void Update()
            {
                if (children.Count <= 1)
                {
                    validate = false;
                    return;
                }

                for (int i = 0; i < children.Count; i++)
                {
                    if (i == 0)
                        m_bounds = children[i].bounds;
                    m_bounds.Encapsulate(children[i].bounds);
                }
            }

            public bool CanAdd(MergeData data)
            {
                var lhsBounds = m_bounds;
                var rhsBounds = data.m_bounds;

                var bounds = m_bounds;
                bounds.Encapsulate(data.m_bounds);

                var overlayPercentage = CalcPercent(lhsBounds, rhsBounds);

                var clusterDiameter = Mathf.Max(Mathf.Max(bounds.size.x, bounds.size.y), bounds.size.z);
                if (clusterDiameter > setting.clusterMaxDiameter || clusterDiameter < setting.clusterMinDiameter)
                    return false;

                if (clusterDiameter > setting.clusterMaxDiameter || overlayPercentage < setting.overlayPercentage * 0.01f)
                    return false;

                return true;
            }

            public bool Attach(MergeData data)
            {
                foreach (var child in data.children)
                {
                    if (children.Contains(child))
                        return true;
                }

                return false;
            }

            public int CompareTo(MergeData other)
            {
                bool selfVoxelContact = IsClusterIntersets();
                bool otherVoxelContact = other.IsClusterIntersets();

                if (selfVoxelContact != otherVoxelContact)
                {
                    return selfVoxelContact ? -1 : 1;
                }

                return -voxel.size.CompareTo(other.voxel.size);
            }

            public bool IsClusterIntersets()
            {
                isClusterIntersets = Voxel.Intersects(lhs.voxel, rhs.voxel);
                ;
                return isClusterIntersets;
                // return Voxel.Intersects(lhs.voxel, rhs.voxel);
            }

            public static MergeData TryMergeCluster(Cluster lhs, Cluster rhs, ClusterSetting setting)
            {
                if (lhs == null || rhs == null || setting == null)
                    return null;

                // 合并后包围盒体积
                var lhsBounds = GetBounds(lhs);
                var rhsBounds = GetBounds(rhs);
                Bounds bounds = lhsBounds;
                bounds.Encapsulate(rhsBounds);

                var overlayPercentage = CalcPercent(lhsBounds, rhsBounds);

                float clusterDiameter = Mathf.Max(Mathf.Max(bounds.size.x, bounds.size.y), bounds.size.z);

                if (clusterDiameter > setting.clusterMaxDiameter || overlayPercentage < setting.overlayPercentage * 0.01f)
                    return null;

                var data = new MergeData
                {
                    lhs = lhs, rhs = rhs,
                    setting = setting,
                    validate = true,
                    m_bounds = bounds,
                };

                return data;
            }

            private static float CalcPercent(Bounds lhsBounds, Bounds rhsBounds)
            {
                var bounds = lhsBounds;
                bounds.Encapsulate(rhsBounds);

                float postVolumeSize = bounds.size.x * bounds.size.y * bounds.size.z;

                // 合并前两个包围盒的有效体积
                float lhsVolumeSize = lhsBounds.size.x * lhsBounds.size.y * lhsBounds.size.z;
                float rhsVolumeSize = rhsBounds.size.x * rhsBounds.size.y * rhsBounds.size.z;

                float preVolumeSize = lhsVolumeSize + rhsVolumeSize;
                if (lhsBounds.Contains(rhsBounds.min) && lhsBounds.Contains(rhsBounds.max))
                {
                    preVolumeSize = lhsVolumeSize;
                }
                else if (rhsBounds.Contains(lhsBounds.min) && rhsBounds.Contains(lhsBounds.max))
                {
                    preVolumeSize = rhsVolumeSize;
                }
                else if (lhsBounds.Intersects(rhsBounds))
                {
                    Vector3 min = Vector3.Max(lhsBounds.min, rhsBounds.min);
                    Vector3 max = Vector3.Min(lhsBounds.max, rhsBounds.max);
                    var size = max - min;
                    preVolumeSize -= size.x * size.y * size.z;
                }

                return preVolumeSize / postVolumeSize;
            }
        }
        #endregion
    }
}