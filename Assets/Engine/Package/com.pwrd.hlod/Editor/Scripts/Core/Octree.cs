using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace com.pwrd.hlod.editor
{
    public class Octree<T> where T : class
    {
        private int maxDepth;
        public OctreeNode rootNode; 
        public Bounds Bounds { get; private set; }

        public Octree(Bounds bounds, int maxDepth)
        {
            if (maxDepth <= 0)
            {
                throw new System.ArgumentException("maxDepth", "maxDepth cannot be less than 1.");
            }
            
            Bounds = bounds;
            this.maxDepth = maxDepth;
            rootNode = new OctreeNode(bounds);
        }

        public void Insert(T item, Bounds bounds)
        {
            Insert(new OctreeNodeItem(item, bounds));
        }
        
        public void Remove(T item)
        {
            rootNode.Remove(item);
        }
        
        private void Insert(OctreeNodeItem item)
        {
            if (!IsvalidBounds(item.bounds))
            {
                throw new System.ArgumentException("bounds");
            }

            Bounds bounds = item.bounds;
            OctreeNode node = rootNode.Insert(item, bounds, 1, maxDepth);
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
        
        public class OctreeNode
        {
            public int depth;
            public List<OctreeNodeItem> items = new List<OctreeNodeItem>();
            public Bounds bounds;
            
            public OctreeNode URB_Node;
            public OctreeNode URF_Node;
            public OctreeNode ULF_Node;
            public OctreeNode ULB_Node;
            public OctreeNode DRB_Node;
            public OctreeNode DRF_Node;
            public OctreeNode DLF_Node;
            public OctreeNode DLB_Node;
            
            public List<OctreeNode> subNodes = new List<OctreeNode>();

            public OctreeNode(Bounds bounds, List<OctreeNodeItem> items)
            {
                this.bounds = bounds;
                var min = this.bounds.min;
                var max = this.bounds.max;
                var center = this.bounds.center;
                var size = this.bounds.size;
                
                // center = this.bounds.center - Vector3.right * width * 0.5f - Vector3.forward * height * 0.5f;
                if (items.Count == 0) return;

                
            }

            public OctreeNode(Bounds bounds)
            {
                this.bounds = bounds;
            }

            public OctreeNode Insert(OctreeNodeItem item, Bounds bounds, int depth, int maxDepth)
            {
                this.depth = depth;
                if (depth < maxDepth)
                {
                    var child = GetItemContainerNode(bounds);
                    if (child != null)
                    {
                        return child.Insert(item, bounds, depth + 1, maxDepth);
                    }
                }

                if (this.bounds.Contains(bounds.center))
                {
                    items.Add(item);
                }
                return this;
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
                    GetQuadrantNode(Quadrant.DLB)?.Remove(item);
                    GetQuadrantNode(Quadrant.DRB)?.Remove(item);
                    GetQuadrantNode(Quadrant.DLF)?.Remove(item);
                    GetQuadrantNode(Quadrant.DRF)?.Remove(item);
                    GetQuadrantNode(Quadrant.ULB)?.Remove(item);
                    GetQuadrantNode(Quadrant.URB)?.Remove(item);
                    GetQuadrantNode(Quadrant.ULF)?.Remove(item);
                    GetQuadrantNode(Quadrant.URF)?.Remove(item);
                }
            }
            
            private OctreeNode GetItemContainerNode(Bounds bounds)
            {
                OctreeNode child = null;
                if (child == null) child = GetItemContainerNode(Quadrant.DLB, bounds);
                if (child == null) child = GetItemContainerNode(Quadrant.DRB, bounds);
                if (child == null) child = GetItemContainerNode(Quadrant.DLF, bounds);
                if (child == null) child = GetItemContainerNode(Quadrant.DRF, bounds);
                if (child == null) child = GetItemContainerNode(Quadrant.ULB, bounds);
                if (child == null) child = GetItemContainerNode(Quadrant.URB, bounds);
                if (child == null) child = GetItemContainerNode(Quadrant.ULF, bounds);
                if (child == null) child = GetItemContainerNode(Quadrant.URF, bounds);
                return child;
            }
            
            private OctreeNode GetItemContainerNode(Quadrant quadrant, Bounds bounds)
            {
                var node = GetQuadrantNode(quadrant);
                OctreeNode child = null;
                if (node == null)
                {
                    var size = this.bounds.size;
                    Vector3 center = this.bounds.center;
                    switch (quadrant)
                    {
                        case Quadrant.DLB:
                            center = this.bounds.center - Vector3.right * size.x * 0.25f - Vector3.up * size.y * 0.25f - Vector3.forward * size.z * 0.25f;
                            break;
                        case Quadrant.DRB:
                            center = this.bounds.center + Vector3.right * size.x * 0.25f - Vector3.up * size.y * 0.25f - Vector3.forward * size.z * 0.25f;
                            break;
                        case Quadrant.DLF:
                            center = this.bounds.center - Vector3.right * size.x * 0.25f - Vector3.up * size.y * 0.25f + Vector3.forward * size.z * 0.25f;
                            break;
                        case Quadrant.DRF:
                            center = this.bounds.center + Vector3.right * size.x * 0.25f - Vector3.up * size.y * 0.25f + Vector3.forward * size.z * 0.25f;
                            break;
                        case Quadrant.ULB:
                            center = this.bounds.center - Vector3.right * size.x * 0.25f + Vector3.up * size.y * 0.25f - Vector3.forward * size.z * 0.25f;
                            break;
                        case Quadrant.URB:
                            center = this.bounds.center + Vector3.right * size.x * 0.25f + Vector3.up * size.y * 0.25f - Vector3.forward * size.z * 0.25f;
                            break;
                        case Quadrant.ULF:
                            center = this.bounds.center - Vector3.right * size.x * 0.25f + Vector3.up * size.y * 0.25f + Vector3.forward * size.z * 0.25f;
                            break;
                        case Quadrant.URF:
                            center = this.bounds.center + Vector3.right * size.x * 0.25f + Vector3.up * size.y * 0.25f + Vector3.forward * size.z * 0.25f;
                            break;
                    }
                    
                    size = this.bounds.size * 0.5f;
                    Bounds newBounds = new Bounds(center, size);
                    if (newBounds.Contains(bounds.center))
                    {
                        node = new OctreeNode(newBounds);
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
            
            public OctreeNode GetQuadrantNode(Quadrant quadrant)
            {
                OctreeNode node = null;
                switch (quadrant)
                {
                    case Quadrant.URB: node = URB_Node; break;
                    case Quadrant.URF: node = URF_Node; break;
                    case Quadrant.ULF: node = ULF_Node; break;
                    case Quadrant.ULB: node = ULB_Node; break;
                    case Quadrant.DRB: node = DRB_Node; break;
                    case Quadrant.DRF: node = DRF_Node; break;
                    case Quadrant.DLF: node = DLF_Node; break;
                    case Quadrant.DLB: node = DLB_Node; break;
                }
                return node;
            }
            
            private void SetQuadrantNode(Quadrant quadrant, OctreeNode node)
            {
                switch (quadrant)
                {
                    case Quadrant.URB: URB_Node = node; break;
                    case Quadrant.URF: URF_Node = node; break;
                    case Quadrant.ULF: ULF_Node = node; break;
                    case Quadrant.ULB: ULB_Node = node; break;
                    case Quadrant.DRB: DRB_Node = node; break;
                    case Quadrant.DRF: DRF_Node = node; break;
                    case Quadrant.DLF: DLF_Node = node; break;
                    case Quadrant.DLB: DLB_Node = node; break;
                }
            }
            
            public List<T> GetItems()
            {
                return items.Select(s => s.data).ToList();
            }
        }
        
        public enum Quadrant
        {
            URB,
            URF,
            ULF,
            ULB,
            DRB,
            DRF,
            DLF,
            DLB
        }
        
        public class OctreeNodeItem
        {
            public T data;
            public Bounds bounds;
            
            public OctreeNodeItem(T item, Bounds bounds)
            {
                this.data = item;
                this.bounds = bounds;
            }
        }
    }
}