using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

[Serializable]
public static class DecalConfig
{
    public static int MaxSingleBatchCount = 100;
    public static int MaxMapX = 500;
    public static int MaxMapY = 200;
    public static int MaxMapZ = 500;
    public static int Row = 5;
    public static int Col = 5;

    public static float[] CullingDistances = new[] {100f};
    // public Shader DecalShader;
    public static string DecalShaderName = "URP/Scene/Decal";
    
    private static Mesh _decalMesh;
    public static Mesh InstanceMesh
    {
        get
        {
            if (_decalMesh == null)
                _decalMesh = CoreUtils.CreateCubeMesh(new Vector4(-0.5f, -0.5f, -0.5f, 1.0f), new Vector4(0.5f, 0.5f, 0.5f, 1.0f));
            return _decalMesh;
        }
    }
}