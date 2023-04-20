#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEngine;

namespace CFEngine
{
    public interface IQuadTreeObject
    {
        int BlockId { get; }

        int QuadNodeId { get; set; }

        AABB bounds { get; }
    }

    public class EditorQuardTreeNode
    {
        public AABB aabb;
        public List<IQuadTreeObject> data = new List<IQuadTreeObject> ();
        public bool hasData = false;
        // public Rect box;
    }
    public class QuadTreeContext
    {
        public static int maxLayer = 3;
        public EditorQuardTreeNode[] treeNodes;
        public AABB aabb;
        public int[] indexStack = new int[maxLayer];
        public int chunkId = -1;
        public int layerDepth;
        public int layerWidth;
        public int parentLayersNodeCountAcc;
        public int nodeOffset;
        public int parentNodeIndex;

    }
    public class SceneQuadTree
    {

        public delegate bool QuadTreeNodeCb (int nodeIndex, QuadTreeContext context);
        public delegate void QuadTreeNodePostCb (int nodeIndex, int childResult, QuadTreeContext context);

        public static int EnumQuadTree (
            int layerDepth, int maxLayer,
            int layerWidth, int parentLayersNodeCountAcc, int[] indexStack,
            int nodeOffset, int parentNodeIndex, QuadTreeNodeCb cb, QuadTreeNodePostCb postCb, QuadTreeContext context)
        {
            if (layerDepth < maxLayer)
            {
                indexStack[layerDepth] = nodeOffset;
                int currentLayerNodeIndex = 0;
                int layerCount = layerWidth * layerWidth;
                int layerCountTmp = layerCount;
                for (int i = 0; i < layerDepth; ++i)
                {
                    currentLayerNodeIndex += indexStack[i] * layerCountTmp;
                    layerCountTmp /= 4;
                }

                int nodeIndex = parentLayersNodeCountAcc + currentLayerNodeIndex + nodeOffset;
                //node cb
                context.layerDepth = layerDepth;
                context.layerWidth = layerWidth;
                context.parentLayersNodeCountAcc = parentLayersNodeCountAcc;
                context.nodeOffset = nodeOffset;
                context.parentNodeIndex = parentNodeIndex;

                int resultCount = 0;
                bool result = cb (nodeIndex, context);
                if (result)
                {
                    for (int i = 0; i < 4; ++i)
                    {
                        int count = EnumQuadTree (layerDepth + 1, maxLayer, layerWidth * 2, parentLayersNodeCountAcc + layerCount, indexStack, i, nodeIndex, cb, postCb, context);
                        if (count == 4)
                        {
                            resultCount++;
                        }
                    }

                    context.layerDepth = layerDepth;
                    context.layerWidth = layerWidth;
                    context.parentLayersNodeCountAcc = parentLayersNodeCountAcc;
                    context.nodeOffset = nodeOffset;
                    context.parentNodeIndex = parentNodeIndex;

                    if (postCb != null)
                        postCb (nodeIndex, resultCount, context);
                }
                return resultCount;
            }
            return 4;
        }

        static bool InitQuadTreeNodeCb (int nodeIndex, QuadTreeContext context)
        {
            EditorQuardTreeNode node = context.treeNodes[nodeIndex];
            EditorQuardTreeNode parentNode = context.parentNodeIndex >= 0 ? context.treeNodes[context.parentNodeIndex] : null;

            if (parentNode != null)
            {
                Bounds aabb = new Bounds ();
                float signX = context.nodeOffset % 2 == 0 ? -1 : 1;
                float signY = context.nodeOffset / 2 == 0 ? -1 : 1;
                Vector3 size = parentNode.aabb.size * 0.5f;
                aabb.center = parentNode.aabb.center + new Vector3 (signX * size.x * 0.5f, 0, signY * size.z * 0.5f);
                aabb.size = size;
                node.aabb = AABB.Create(aabb);
                // node.box = new Rect (aabb.min.x, aabb.min.z, aabb.size.x, aabb.size.z);
            }
            else
            {
                node.aabb = context.aabb;
            }

            return true;
        }

        public static void InitQuadTree (QuadTreeContext treeContext, int chunkWidth, int chunkHeight, ref Vector3 chunkOffset)
        {

            int quadNodeCount = 0;
            for (int i = 0, layerWidth = 1; i < QuadTreeContext.maxLayer; ++i)
            {
                quadNodeCount += layerWidth * layerWidth;
                layerWidth *= 2;
            }
            if (treeContext.treeNodes == null)
                treeContext.treeNodes = new EditorQuardTreeNode[quadNodeCount];

            Vector3 aabbCenter = new Vector3 (chunkWidth / 2, 0, chunkHeight / 2) + chunkOffset;
            Vector3 aabbSize = new Vector3 (chunkWidth, 0, chunkHeight);
            Bounds aabb = new Bounds (aabbCenter, aabbSize);
            treeContext.aabb =  AABB.Create(aabb);
            for (int i = 0; i < quadNodeCount; ++i)
            {
                EditorQuardTreeNode node = treeContext.treeNodes[i];
                if (node == null)
                {
                    node = new EditorQuardTreeNode ();
                    treeContext.treeNodes[i] = node;
                }
                node.data.Clear ();
                node.aabb =  AABB.Create(new Bounds (aabbCenter, aabbSize));
                node.hasData = false;
            }

            EnumQuadTree (0, QuadTreeContext.maxLayer, 1, 0, treeContext.indexStack, 0, -1, InitQuadTreeNodeCb, null, treeContext);
        }
        public static void Add2QuadTree (QuadTreeContext treeContext, IQuadTreeObject qto, AABB aabb)
        {
            if (qto != null)
            {
                int blockId = qto.BlockId;
                if (blockId >= 0 && blockId < treeContext.treeNodes.Length)
                {
                    EditorQuardTreeNode node = treeContext.treeNodes[blockId];
                    node.data.Add (qto);
                    if (node.data.Count == 1)
                    {
                        node.aabb = aabb;
                    }
                    else
                        node.aabb.Encapsulate (ref aabb);
                    node.hasData = true;
                }
                else
                {
                    Debug.LogError ("out of tree range:" + blockId.ToString ());
                }
            }

            // EnumQuadTree (0, QuadTreeContext.maxLayer, 1, 0, treeContext.indexStack, 0, -1, Add2QuadTreeCb, Add2QuadTreePostCb, treeContext);
        }
        public static void EndQuadTree (QuadTreeContext treeContext)
        {
            for (int j = 0; j < 4; ++j)
            {
                EditorQuardTreeNode node = treeContext.treeNodes[j + 1];
                int startIndex = j * 4 + 5;
                int endIndex = (j + 1) * 4 + 5;
                bool firstSet = true;
                for (int k = startIndex; k < endIndex; ++k)
                {
                    EditorQuardTreeNode child = treeContext.treeNodes[k];
                    child.hasData = child.data.Count > 0;
                    if (child.hasData)
                    {
                        if (firstSet)
                        {
                            if (node.hasData)
                            {
                                node.aabb.Encapsulate (ref child.aabb);
                            }
                            else
                            {
                                node.aabb = child.aabb;
                            }
                            firstSet = false;
                        }
                        else
                            node.aabb.Encapsulate (ref child.aabb);
                    }

                    node.hasData |= child.hasData;
                }
            }
            EditorQuardTreeNode root = treeContext.treeNodes[0];
            bool setRoot = false;
            for (int k = 1; k < 5; ++k)
            {
                EditorQuardTreeNode child = treeContext.treeNodes[k];
                if (child.hasData)
                {
                    if (!setRoot)
                    {
                        if (root.data.Count == 0)
                        {
                            root.aabb = child.aabb;
                        }
                        else
                        {
                            root.aabb.Encapsulate (ref child.aabb);
                        }
                        setRoot = true;
                    }
                    else
                        root.aabb.Encapsulate (ref child.aabb);
                }

                root.hasData |= child.hasData;
            }

            var min = treeContext.aabb.min;
            min.y = root.aabb.min.y;
            treeContext.aabb.min = min;
            var max = treeContext.aabb.max;
            max.y = root.aabb.max.y;
            treeContext.aabb.max = max;
        }

        public static int FindChunkIndex (ref Vector3 pos, float width, int xCount, int zCount, out int x, out int z)
        {
            x = (int) Mathf.Clamp (pos.x / width, 0, xCount - 1);
            z = (int) Mathf.Clamp (pos.z / width, 0, zCount - 1);
            return x + z * xCount;
        }

        public static void FindChunkIndex(ref Vector3 pos, float width, out int x, out int z)
        {
            x = Mathf.FloorToInt(pos.x / width);
            z = Mathf.FloorToInt(pos.z / width);
        }
        public static int FindChunkIndex (Vector3 pos, float width, int xCount, int zCount, out int x, out int z)
        {
            return FindChunkIndex (ref pos, width, xCount, zCount, out x, out z);
        }
        public static void CalcBlockId (Vector3 size, Vector3 center, int x, int z, int width, int height,
            out int chunkLevel, out int blockIndex, bool forceLevel2 = false)
        {
            float largeSize = size.x > size.z ? size.x : size.z;
            chunkLevel = 2;
            if (largeSize > width / 4)
            {
                chunkLevel = 1;
            }
            if (largeSize > height / 2)
            {
                chunkLevel = 0;
            }
            if (forceLevel2)
                chunkLevel = 2;
            blockIndex = 0;
            if (chunkLevel > 0)
            {
                Vector2 chunkCorner = new Vector4 (x * width, z * height);
                Vector2 chunkCenter = new Vector2 ((x + 0.5f) * width, (z + 0.5f) * height);
                float halfWidth = width * 0.5f;

                int subIndex = FindChunkIndex (new Vector3 (center.x - chunkCorner.x, 0, center.z - chunkCorner.y),
                    halfWidth, 2, 2, out x, out z);
                blockIndex = subIndex + 1;
                if (chunkLevel > 1)
                {
                    if (subIndex == 1)
                    {
                        chunkCorner.x = chunkCenter.x;
                    }
                    else if (subIndex == 2)
                    {
                        chunkCorner.y = chunkCenter.y;
                    }
                    else if (subIndex == 3)
                    {
                        chunkCorner.x = chunkCenter.x;
                        chunkCorner.y = chunkCenter.y;
                    }
                    halfWidth = halfWidth * 0.5f;
                    int subIndex1 = FindChunkIndex (new Vector3 (center.x - chunkCorner.x, 0, center.z - chunkCorner.y),
                        halfWidth, 2, 2, out x, out z);
                    blockIndex = subIndex1 + subIndex * 4 + 5;
                }
            }
        }

        public static int LocalBlockId2World (int blockId)
        {
            switch (blockId)
            {
                case 5:
                    return 0;
                case 6:
                    return 1;
                case 7:
                    return 4;
                case 8:
                    return 5;
                case 9:
                    return 2;
                case 10:
                    return 3;
                case 11:
                    return 6;
                case 12:
                    return 7;
                case 13:
                    return 8;
                case 14:
                    return 9;
                case 15:
                    return 12;
                case 16:
                    return 13;
                case 17:
                    return 10;
                case 18:
                    return 11;
                case 19:
                    return 14;
                case 20:
                    return 15;

            }
            return 0;
        }
    }

}
#endif