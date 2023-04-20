using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using UnityEngine;

public sealed class Version
{
    public const string V = "1.97";
}
public sealed class ThreadTask
{
    public NameValueCollection Para;
    public Stream Output;
    public byte[] Data;
}
[Serializable]
public sealed class SCList
{
    public List<SC> List;
}
[Serializable]
public sealed class SC
{
    public string N;
    public List<OI> OList;
    public List<PP> PList;
}
[Serializable]
public sealed class OI
{
    public int ID;
    public int PID;
    public string N;
    public int E;
    public int R;
    public Vector3 Po;
    public Vector3 Ro;
    public Vector3 Sc;
    public Vector3 Si;
}
[Serializable]
public sealed class PP
{
    public int ID;
    public List<VL> VL;
}
[Serializable]
public sealed class VL
{
    public string Name;
    public bool Enable = false;
}
[Serializable]
public sealed class PVP
{
    public string PostName;
    public List<VP> Parameters;
}
[Serializable]
public sealed class VP
{
    public bool OverrideState;
    public string TypeName;
    public string ParaName;
    public string ValueString;
}
[Serializable]
public sealed class RenderInfo
{
    public string Platform = "";
    public bool URP = false;
    public ColorSpace AactiveColorSpace;
    public int LightCount;
    public int TextureLimit;
    public ShadowmaskMode ShadowmaskMode;
    public string Shadows;
    public string ShadowResolution;
    public string ShadowProjection;
    public float ShadowDistance;
    public float ShadowNearPlane;
    public int ShadowCascades;
    public int SyncCount;
    public int TargetFrame;
    public int PostScale;
    public int AA;
}
[Serializable]
public sealed class RenderMat
{
    public int ID;
    public int Layer = -2;
    //0:None; 1:MeshRenderer;2:SkinnedMeshRenderer,3:Light
    public byte Type = 0;
    public int LightCullingMask = -2;
    public Vector4 LightColor;
    public int LightmapBakeType;
    public int LightShadows;
    public string MeshName = null;
    public bool CastShadow = false;
    public int LightProbeUsage;
    public int ReflectionProbeUsage;

    public string MatName = null;
    public string ShaderName = null;
    public string Keywords = null;

    public List<RenderMatFloat> Floats = null;
    public List<RenderMatVector> Vectors = null;
}
[Serializable]
public sealed class GlobalPara
{

    /// <summary>
    /// 0:float; 1:Vector, 2:Keyword
    /// </summary>
    public byte Type;
    public string FloatName;
    public float Float;
    public string VectorName;
    public Vector4 Vector;
    public bool EnableKeyword = true;
    public string Keyword;
}
[Serializable]
public sealed class RenderMatFloat
{
    public string Name;
    public float Value;
}
[Serializable]
public sealed class RenderMatVector
{
    public string Name;
    public Vector4 Value;
}

[Serializable]
public sealed class FPDebugShaderList
{
    public List<string> Shaders;
}

[Serializable]
public sealed class ReplaceShaderInfo
{
    public ReplaceShaderInfo(string name)
    {
        Name = name;
    }
    public string Name;
    public string ReplaceTo;
    [NonSerialized]
    public List<Material> MatList;
}