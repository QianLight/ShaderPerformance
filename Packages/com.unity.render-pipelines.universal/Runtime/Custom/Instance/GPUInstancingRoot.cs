using System;
using System.Collections.Generic;
using CFEngine;
using UnityEngine;

public struct FlagArray
{
    private uint[] array;

    public FlagArray(int count)
    {
        int size = Mathf.CeilToInt(count / (float) sizeof(uint));
        array = new uint[size];
    }

    public bool IsCreated()
    {
        return array != null;
    }
    
    public void SetFlag(int instanceIndex, bool value)
    {
        ref uint targetFlag = ref array[instanceIndex / sizeof(uint)];
        if (value)
            targetFlag |= 1u << (instanceIndex % sizeof(uint));
        else
            targetFlag &= ~(1u << (instanceIndex % sizeof(uint)));
    }

    public bool HasFlag(int index)
    {
        if (array == null)
        {
            return false;
        }
        
        int tempIndex = index / sizeof(uint);
        if (tempIndex >= array.Length)
        {
            return false;
        }
        
        return (array[tempIndex] & (1 << (index % sizeof(uint)))) > 0;
    }
}

public class GPUInstancingRoot : MonoBehaviour
{

    [Serializable]
    public class CustomLodDistance
    {
        public Mesh mesh;
        public List<float> distances = new List<float>();
    }

    private FlagArray flags;
    public List<int> defaultDistance = new List<int> {20, 60};
    public List<CustomLodDistance> customDistance = new List<CustomLodDistance>();
    public List<GPUInstancingGroup> groups = new List<GPUInstancingGroup>();

    private void OnEnable()
    {
        if (!UnityEngine.Rendering.Universal.GameQualitySetting.DrawGrass)
        {
            foreach (GPUInstancingGroup group in groups)
            {
                foreach (MeshRenderer r in group.renderers)
                {
                    if (r)
                    {
                        r.enabled = false;
                    }
                }
            }
            return;
        }

        if (!flags.IsCreated())
        {
            int count = 0;
            foreach (GPUInstancingGroup group in groups)
                count += group.renderers.Count;
            flags = new FlagArray(count);
        }

        int instanceIndex = 0;
        foreach (GPUInstancingGroup group in groups)
        {
            foreach (MeshRenderer r in group.renderers)
            {
                if (r)
                {
                    flags.SetFlag(instanceIndex, r.enabled);
                    r.enabled = false;
                }

                instanceIndex++;
            }

            GPUInstancingManager.Instance.Add(group);
        }
    }


    private void OnDisable()
    {
        if (!UnityEngine.Rendering.Universal.GameQualitySetting.DrawGrass) return;
        
        int index = 0;
        foreach (GPUInstancingGroup group in groups)
        {
            foreach (MeshRenderer groupRenderer in group.renderers)
            {
                if (groupRenderer)
                {
                    groupRenderer.enabled = flags.HasFlag(index++);
                }
            }

            GPUInstancingManager.Instance.Remove(group);
        }
    }
}