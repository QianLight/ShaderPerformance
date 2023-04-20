using System.Linq;
using UnityEngine;
using System;
using System.Collections.Generic;
using Unity.Collections;

namespace com.pwrd.hlod.editor
{
    public class BVHTree<T> where T : class
    {
        public class BVHNode
        {
            public int depth;
            public Bounds bounds;
            public BVHNode left;
            public BVHNode right;
            public List<T> datas; // id for bvh renders

            public BVHNode()
            {
                datas = new List<T>();
            }
        }
        
        public struct BVHData
        {
            public T data;
            public Bounds bounds;
            
            public BVHData(T item, Bounds bounds)
            {
                this.data = item;
                this.bounds = bounds;
            }
        }

        private List<BVHData> bvhDataList = new List<BVHData>();

        public void InsertBVHData(T item, Bounds bounds)
        {
            bvhDataList.Add(new BVHData(item, bounds));
        }
        
        public BVHNode BuildBVHTree(int splitCount = 64)
        {
            BVHNode root = CreateBVHRecursive(bvhDataList, bvhDataList.Count, splitCount);
            return root;
        }

        static Bounds CalculateBoundsAndSAH(BVHData[] bvhDatas, int validCount, ref float sah)
        {
            var min = Vector3.one * float.MaxValue;
            var max = Vector3.one * float.MinValue;

            int count = Mathf.Min(validCount, bvhDatas.Length);
            for (int i = 0; i < count; ++i)
            {
                min = Vector3.Min(min, bvhDatas[i].bounds.min);
                max = Vector3.Max(max, bvhDatas[i].bounds.max);
            }
            
            Vector3 size = max - min;
            //sah = (size.x * size.y + size.y * size.z + size.x * size.z);
            sah = (size.x * size.y + size.y * size.z + size.x * size.z) * count;
            //sah = size.x * size.y * size.z;
            return new Bounds() { min = min, max = max };
        }
        
        static float TrySplit(List<BVHData> inputDatas, int validCount, int axis, float split, ref BVHData[] leftDatas, ref BVHData[] rightDatas, ref int leftCount, ref int rightCount)
        {
            leftCount = 0;
            rightCount = 0;

            for (int i = 0; i < validCount; ++i)
            {
                if (inputDatas[i].bounds.center[axis] <= split)
                {
                    leftDatas[leftCount++] = inputDatas[i];
                }
                else
                {
                    rightDatas[rightCount++] = inputDatas[i];
                }
            }

            float costL = 0, costR = 0;
            CalculateBoundsAndSAH(leftDatas,  leftCount,  ref costL);
            CalculateBoundsAndSAH(rightDatas, rightCount, ref costR);
            return costL + costR; 
        }
        
        static BVHNode CreateBVHRecursive(List<BVHData> inputDatas, int validCount, int splitCount, int recursiveCount = 0)
        {
            float minCost = 0;
            var totalBounds = CalculateBoundsAndSAH(inputDatas.ToArray(), validCount, ref minCost);          

            int bestAxis = -1;
            float bestSplit = 0;

            if (validCount >= 4)
            {
                var leftCount = 0;
                var rightCount = 0;
                var leftBuffer  = new BVHData[validCount];
                var rightBuffer = new BVHData[validCount];

                Vector3 size = totalBounds.size;
                Vector3 min  = totalBounds.min;
                Vector3 max  = totalBounds.max;

                minCost = float.MaxValue;

                for (var axis = 0; axis < 3; ++axis)
                {
                    if (size[axis] < 0.001f)
                        continue;

                    var step = size[axis] / (splitCount / (recursiveCount + 1));

                    if (step < 0.01f)
                        continue;

                    float splitStart = min[axis] + step;
                    float splitEnd = max[axis] - step;

                    for (float split = splitStart; split < splitEnd; split += step)
                    {
                        float cost = TrySplit(inputDatas, validCount, axis, split, ref leftBuffer, ref rightBuffer, ref leftCount, ref rightCount);

                        if (leftCount <= 1 || rightCount <= 1)
                            continue;

                        if (cost < minCost)
                        {
                            minCost = cost;
                            bestAxis = axis;
                            bestSplit = split;
                        }
                    }
                }
            }

            BVHNode node = new BVHNode();
            node.bounds = totalBounds;
            node.depth = recursiveCount;

            if (bestAxis == -1)
            {
                // create leaf node
                // float temp = 0;
                //node.bounds = CalculateBoundsAndSAH(inputDatas, validCount, ref temp);
                for (int i = 0; i < validCount; ++i)
                {
                    node.datas.Add(inputDatas[i].data);
                }
            }
            else
            {
                var leftCount = 0;
                var rightCount = 0;
                var leftBuffer  = new BVHData[validCount];
                var rightBuffer = new BVHData[validCount];

                TrySplit(inputDatas, validCount, bestAxis, bestSplit, ref leftBuffer, ref rightBuffer, ref leftCount, ref rightCount);
                {
                    var leftNode = CreateBVHRecursive(leftBuffer.ToList().GetRange(0, leftCount), leftCount, splitCount, recursiveCount + 1);
                    var rightNode = CreateBVHRecursive(rightBuffer.ToList().GetRange(0, rightCount), rightCount, splitCount, recursiveCount + 1);
                    node.left = leftNode;
                    node.right = rightNode;
                }
            }
            return node;
        }
    }
}