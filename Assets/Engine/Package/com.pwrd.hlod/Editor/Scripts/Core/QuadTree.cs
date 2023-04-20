using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System;

namespace com.pwrd.hlod.editor
{
    public class QuadTree<T> where T : class
    {
        private int maxDepth;
        public QuadTreeNode rootNode; 

        public Bounds Bounds { get; private set; }
        
        public QuadTree(Bounds bounds, int maxDepth)
        {
            if (maxDepth <= 0)
            {
                throw new ArgumentException("maxDepth", "maxDepth cannot be less than 1.");
            }
            
            Bounds = bounds;
            this.maxDepth = maxDepth;
            rootNode = new QuadTreeNode(bounds);
        }

        public QuadTree(Bounds bounds) : this(bounds, 255) { }

        public void Insert(T item, Bounds bounds)
        {
            Insert(new QuadTreeNodeItem(item, bounds));
        }

        public void Remove(T item)
        {
            rootNode.Remove(item);
        }
        
        public HashSet<T> GetInsideItems(Bounds bounds)
        {
            return rootNode.GetInsideItems(ref bounds);
        }
        
        public HashSet<T> GetIntersectedItems(Bounds bounds)
        {
            return rootNode.GetIntersectedItems(ref bounds);
        }
        
        public bool PredicateItemsCount(Bounds bounds, int thresholdCount)
        {
            if (thresholdCount <= 0)
            {
                throw new ArgumentException("thresholdCount", "thresholdCount cannot be less or equal than 0.");
            }

            int count = 0;
            bool ret = rootNode.PredicateItemsCount(ref bounds, thresholdCount, ref count);
            return ret;
        }
        
        private void Insert(QuadTreeNodeItem item)
        {
            if (!IsvalidBounds(item.bounds))
            {
                throw new ArgumentException("bounds");
            }

            Bounds bounds = item.bounds;
            QuadTreeNode node = rootNode.Insert(item, ref bounds, 1, maxDepth);
        }

        private bool IsvalidBounds(Bounds bounds)
        {
            return IsValidBound(bounds.center.x) && IsValidBound(bounds.center.z)
                                          && IsValidBound(bounds.size.x) && IsValidBound(bounds.size.z);
        }

        private bool IsValidBound(double boundValue)
        {
            return !double.IsNaN(boundValue) && !double.IsInfinity(boundValue);
        }
    
        public class QuadTreeNode
        {
            private List<QuadTreeNodeItem> items = new List<QuadTreeNodeItem>();
            private Bounds bounds;
            private QuadTreeNode topLeftNode;
            private QuadTreeNode topRightNode;
            private QuadTreeNode bottomLeftNode;
            private QuadTreeNode bottomRightNode;
            public int depth;

            public QuadTreeNode(Bounds bounds)
            {
                this.bounds = bounds;
            }

            #region Properties

            private void SetQuadrantNode(Quadrant quadrant, QuadTreeNode node)
            {
                switch (quadrant)
                {
                    case Quadrant.UL:
                        topLeftNode = node;
                        break;
                    case Quadrant.UR:
                        topRightNode = node;
                        break;
                    case Quadrant.DL:
                        bottomLeftNode = node;
                        break;
                    case Quadrant.DR:
                        bottomRightNode = node;
                        break;
                }
            }

            public QuadTreeNode GetQuadrantNode(Quadrant quadrant)
            {
                QuadTreeNode node = null;
                switch (quadrant)
                {
                    case Quadrant.UL:
                        node = topLeftNode;
                        break;
                    case Quadrant.UR:
                        node = topRightNode;
                        break;
                    case Quadrant.DL:
                        node = bottomLeftNode;
                        break;
                    case Quadrant.DR:
                        node = bottomRightNode;
                        break;
                }
                return node;
            }
            
            public List<T> GetItems()
            {
                return items.Select(s => s.data).ToList();
            }

            public HashSet<T> GetAllItems(int depth)
            {
                HashSet<T> nodes = new HashSet<T>();
                if (this.depth <= depth)
                {
                    nodes.UnionWith(items.Select(s => s.data));
                    if (bottomLeftNode != null) nodes.UnionWith(bottomLeftNode.GetItems());
                    if (bottomRightNode != null) nodes.UnionWith(bottomRightNode.GetItems());
                    if (topLeftNode != null) nodes.UnionWith(topLeftNode.GetItems());
                    if (topRightNode != null) nodes.UnionWith(topRightNode.GetItems());
                }
                return nodes;
            }
            
            private HashSet<T> GetAllItems()
            {
                var nodes = new HashSet<T>();
                nodes.UnionWith(items.Select(s => s.data));
                if (bottomLeftNode != null) nodes.UnionWith(bottomLeftNode.GetAllItems());
                if (bottomRightNode != null) nodes.UnionWith(bottomRightNode.GetAllItems());
                if (topLeftNode != null) nodes.UnionWith(topLeftNode.GetAllItems());
                if (topRightNode != null) nodes.UnionWith(topRightNode.GetAllItems());
                return nodes;
            }

            #endregion
            
            public QuadTreeNode Insert(QuadTreeNodeItem item, ref Bounds bounds, int depth, int maxDepth)
            {
                this.depth = depth;
                if (depth < maxDepth)
                {
                    var child = GetItemContainerNode(ref bounds);
                    if (child != null)
                    {
                        return child.Insert(item, ref bounds, depth + 1, maxDepth);
                    }
                }

                if (this.bounds.Contains(bounds.center))
                {
                    items.Add(item);
                }
                return this;
            }

            public HashSet<T> GetInsideItems(ref Bounds bounds)
            {
                if (!bounds.Intersects(this.bounds))
                {
                    return new HashSet<T>();
                }

                var insideNodes = new HashSet<T>();
                if (bounds.Contains(this.bounds.center))
                {
                    insideNodes = GetAllItems();
                    return insideNodes;
                }
                
                insideNodes.UnionWith(GetInsideItems(Quadrant.DL, ref bounds));
                insideNodes.UnionWith(GetInsideItems(Quadrant.DR, ref bounds));
                insideNodes.UnionWith(GetInsideItems(Quadrant.UL, ref bounds));
                insideNodes.UnionWith(GetInsideItems(Quadrant.UR, ref bounds));
                
                insideNodes.UnionWith(GetOwnerInsideItems(ref bounds));
                insideNodes.RemoveWhere(s => s == null);
                return insideNodes;
            }

            public HashSet<T> GetIntersectedItems(ref Bounds bounds)
            {
                if (!bounds.Intersects(this.bounds))
                {
                    return new HashSet<T>();
                }

                var intersectedNodes = new HashSet<T>();
                if (bounds.Contains(this.bounds.center))
                {
                    intersectedNodes = GetAllItems();
                    return intersectedNodes;
                }
                intersectedNodes.UnionWith(GetIntersectedItems(Quadrant.DL, ref bounds));
                intersectedNodes.UnionWith(GetIntersectedItems(Quadrant.DR, ref bounds));
                intersectedNodes.UnionWith(GetIntersectedItems(Quadrant.UL, ref bounds));
                intersectedNodes.UnionWith(GetIntersectedItems(Quadrant.UR, ref bounds));

                intersectedNodes.UnionWith(GetOwnerIntersectedItems(ref bounds));
                intersectedNodes.RemoveWhere(s => s == null);
                return intersectedNodes;
            }

            public void Remove(T item)
            {
                var index = items.FindIndex(s => s.data.Equals(item));
                if (index != -1)
                {
                    items.RemoveAt(index);
                }
                else
                {
                    GetQuadrantNode(Quadrant.DL)?.Remove(item);
                    GetQuadrantNode(Quadrant.DR)?.Remove(item);
                    GetQuadrantNode(Quadrant.UL)?.Remove(item);
                    GetQuadrantNode(Quadrant.UR)?.Remove(item);
                }
            }
            
            public bool PredicateItemsCount(ref Bounds bounds, int thresholdCount, ref int count)
            {
                if (!bounds.Intersects(this.bounds))
                {
                    return true;
                }

                if (bounds.Contains(this.bounds.center))
                {
                    return PredicateItemsCountWithoutCheck(thresholdCount, ref count);
                }

                if (!PredicateIntersectedItemsCount(ref bounds, thresholdCount, ref count))
                {
                    return false;
                }

                if (!PredicateItemsCount(Quadrant.DL, ref bounds, thresholdCount, ref count)) return false;
                if (!PredicateItemsCount(Quadrant.DR, ref bounds, thresholdCount, ref count)) return false;
                if (!PredicateItemsCount(Quadrant.UL, ref bounds, thresholdCount, ref count)) return false;
                if (!PredicateItemsCount(Quadrant.UR, ref bounds, thresholdCount, ref count)) return false;
                return true;
            }

            public bool IsLeaf()
            {
                return topLeftNode == null && topRightNode == null && bottomLeftNode == null && bottomRightNode == null;
            }

            private HashSet<T> GetInsideItems(Quadrant quadrant, ref Bounds bounds)
            {
                var node = GetQuadrantNode(quadrant);
                HashSet<T> nodes = new HashSet<T>();
                if (node != null && node.bounds.Intersects(this.bounds))
                {
                    nodes.UnionWith(node.GetInsideItems(ref bounds));
                }
                return nodes;
            }

            private HashSet<T> GetOwnerInsideItems(ref Bounds bounds)
            {
                HashSet<T> nodes = new HashSet<T>();
                foreach (QuadTreeNodeItem item in items)
                {
                    if (bounds.Contains(item.bounds.center))
                    {
                        nodes.Add(item.data);
                    }
                }
                return nodes;
            }

            private QuadTreeNode GetItemContainerNode(ref Bounds bounds)
            {
                float width = this.bounds.size.x * 0.5f;
                float height = this.bounds.size.z * 0.5f;

                QuadTreeNode child = null;
                if (child == null) child = GetItemContainerNode(Quadrant.DL, ref bounds, width, height);
                if (child == null) child = GetItemContainerNode(Quadrant.DR, ref bounds, width, height);
                if (child == null) child = GetItemContainerNode(Quadrant.UL, ref bounds, width, height);
                if (child == null) child = GetItemContainerNode(Quadrant.UR, ref bounds, width, height);
                return child;
            }
            
            private QuadTreeNode GetItemContainerNode(Quadrant quadrant, ref Bounds bounds, float width, float height)
            {
                var node = GetQuadrantNode(quadrant);
                QuadTreeNode child = null;
                if (node == null)
                {
                    Vector3 center = this.bounds.center;
                    switch (quadrant)
                    {
                        case Quadrant.DL:
                            center = this.bounds.center - Vector3.right * width * 0.5f - Vector3.forward * height * 0.5f;
                            break;
                        case Quadrant.DR:
                            center = this.bounds.center + Vector3.right * width * 0.5f - Vector3.forward * height * 0.5f;
                            break;
                        case Quadrant.UR:
                            center = this.bounds.center + Vector3.right * width * 0.5f + Vector3.forward * height * 0.5f;
                            break;
                        case Quadrant.UL:
                            center = this.bounds.center - Vector3.right * width * 0.5f + Vector3.forward * height * 0.5f;
                            break;
                    }
                    var size = new Vector3(width, this.bounds.size.y, height);
                    Bounds newBounds = new Bounds(center, size);
                    if (newBounds.Contains(bounds.center))
                    {
                        node = new QuadTreeNode(newBounds);
                        SetQuadrantNode(quadrant, node);
                        child = node;
                    }
                }
                else
                {
                    if (node.bounds.Contains(bounds.center))
                    {
                        child = node;
                    }
                }
                return child;
            }

            private HashSet<T> GetOwnerIntersectedItems(ref Bounds bounds)
            {
                var nodes = new HashSet<T>();
                foreach (QuadTreeNodeItem item in items)
                {
                    if (bounds.Intersects(item.bounds))
                    {
                        nodes.Add(item.data);
                    }
                }
                return nodes;
            }

            private HashSet<T> GetIntersectedItems(Quadrant quadrant, ref Bounds bounds)
            {
                var node = GetQuadrantNode(quadrant);
                HashSet<T> nodes = new HashSet<T>();
                if (node != null && node.bounds.Intersects(this.bounds))
                {
                    nodes.UnionWith(node.GetIntersectedItems(ref bounds));
                }
                return nodes;
            }

            private bool PredicateItemsCount(Quadrant quadrant, ref Bounds bounds, int thresholdCount, ref int count)
            {
                var node = GetQuadrantNode(quadrant);
                if (node != null && node.bounds.Intersects(this.bounds))
                {
                    if (!node.PredicateItemsCount(ref bounds, thresholdCount, ref count))
                    {
                        return false;
                    }
                }
                return true;
            }
            
            private bool PredicateItemsCountWithoutCheck(int thresholdCount, ref int count)
            {
                count += items.Count;
                if (count > thresholdCount) return false;
                if (bottomLeftNode != null) if (!bottomLeftNode.PredicateItemsCountWithoutCheck(thresholdCount, ref count)) return false;
                if (bottomRightNode != null) if (!bottomRightNode.PredicateItemsCountWithoutCheck(thresholdCount, ref count)) return false;
                if (topLeftNode != null) if (!topLeftNode.PredicateItemsCountWithoutCheck(thresholdCount, ref count)) return false;
                if (topRightNode != null) if (!topRightNode.PredicateItemsCountWithoutCheck(thresholdCount, ref count)) return false;
                return true;
            }
            
            private bool PredicateIntersectedItemsCount(ref Bounds bounds, int thresholdCount, ref int count)
            {
                foreach (QuadTreeNodeItem item in items)
                {
                    if (bounds.Intersects(item.bounds))
                    {
                        if (++count > thresholdCount)
                        {
                            return false;
                        }
                    }
                }
                return true;
            }
        }
        
        public enum Quadrant
        {
            UR,
            UL,
            DL,
            DR,
        }

        public class QuadTreeNodeItem
        {
            public T data;
            public Bounds bounds;
            
            public QuadTreeNodeItem(T item, Bounds bounds)
            {
                this.data = item;
                this.bounds = bounds;
            }
        }
    }
}