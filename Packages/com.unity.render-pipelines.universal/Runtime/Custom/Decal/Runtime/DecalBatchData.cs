using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class DecalBatchData
{
    public Material Material;
    public Matrix4x4[] Matrix4X4s;
    public int Count;

    public DecalBatchData(Material material)
    {
        Material = material;
        Matrix4X4s = new Matrix4x4[DecalConfig.MaxSingleBatchCount];
    }

    public void AddOne(Matrix4x4 localToWorldMatrix)
    {
        if (Count >= Matrix4X4s.Length)
        {
            Debug.Log("DecalBatchDataNew 矩阵容量不够"); //自动扩容暂时不需要
            return;
        }
        Matrix4X4s[Count] = localToWorldMatrix;
        Count++;
    }

    public void ResetMatrixCount()
    {
        Count = 0;
    }

    public void Dispose()
    {
        Material = null;
        Matrix4X4s = null;
    }
}
