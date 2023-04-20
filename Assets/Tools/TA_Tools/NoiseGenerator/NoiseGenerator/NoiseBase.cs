using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml.Schema;
using UnityEngine;

public abstract class NoiseBase
{
    public abstract Texture Generate(Int32 size, Vector4 scale, NoiseGenerator.NoiseMapType type, TextureFormat textureFormat, bool mipChain);

    public virtual void SetComputeShader(ComputeShader cs, ComputeShader cs2)
    {

    }
}
