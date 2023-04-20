using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class PerlinNoise : NoiseBase
{
    private ComputeShader computeShader;
    private ComputeShader sliceShader;
    private Texture texture;
    private Vector4 mode;
    private Vector4 weight;

    public override void SetComputeShader(ComputeShader cs, ComputeShader cs2)
    {
        computeShader = cs;
        sliceShader = cs2;
    }

    public void SetChannelMode(NoiseGenerator.Perlin_Worley_Vec4 vec)
    {
        mode = new Vector4((Single)vec.R, (Single)vec.G, (Single)vec.B, (Single)vec.A);
        weight = new Vector4(vec.RValue, vec.GValue, vec.BValue, vec.AValue);
    }

    public override Texture Generate(Int32 size, Vector4 scale, NoiseGenerator.NoiseMapType type, TextureFormat textureFormat, Boolean mipChain)
    {
        Vector3 seed = 1000.0f * (UnityEngine.Random.insideUnitSphere + Vector3.one);
        Vector4 seed2 = new Vector4(UnityEngine.Random.Range(0.0f, 2.0f), UnityEngine.Random.Range(0.0f, 2.0f), UnityEngine.Random.Range(0.0f, 2.0f), UnityEngine.Random.Range(0.0f, 2.0f));

        computeShader.SetVector("Seed", seed);
        computeShader.SetVector("SeedRGBA", seed2);
        computeShader.SetVector("RGBAScale", scale);
        computeShader.SetVector("mode", mode);
        computeShader.SetVector("weight", weight);
        if (type == NoiseGenerator.NoiseMapType.Texture2D)
        {
            RenderTexture rt = new RenderTexture(size, size, 24, RenderTextureFormat.Default);
            rt.enableRandomWrite = true;
            rt.Create();

            computeShader.SetFloat("Tex2DRes", size);
            int kernel = computeShader.FindKernel("PerlinNoise2D");
            computeShader.SetTexture(kernel, "Noise2D", rt);

            computeShader.Dispatch(kernel, size / 16, size / 16, 1);

            texture = new Texture2D(size, size, textureFormat, mipChain);
            RenderTexture.active = rt;
            ((Texture2D)texture).ReadPixels(new Rect(0, 0, size, size), 0, 0);
            ((Texture2D)texture).Apply();
            return texture;
        }
        else if(type == NoiseGenerator.NoiseMapType.Texture3D)
        {
            RenderTexture rt = new RenderTexture(size, size, 0, RenderTextureFormat.Default);
            rt.enableRandomWrite = true;
            rt.dimension = UnityEngine.Rendering.TextureDimension.Tex3D;
            rt.volumeDepth = size;
            rt.Create();

            computeShader.SetFloat("Tex3DRes", size);
            int kernel = computeShader.FindKernel("PerlinNoise3D");
            computeShader.SetTexture(kernel, "Noise3D", rt);

            computeShader.Dispatch(kernel, size / 8, size / 8, size / 8);
            Texture2D[] finalSlices = new Texture2D[size];
            RenderTexture[] layers = new RenderTexture[size];
            for (Int32 i = 0; i < size; i++)
            {
                RenderTexture render = new RenderTexture(size, size, 0, RenderTextureFormat.Default);
                render.dimension = UnityEngine.Rendering.TextureDimension.Tex2D;
                render.enableRandomWrite = true;
                render.wrapMode = TextureWrapMode.Clamp;
                render.Create();

                int kernelIndex = sliceShader.FindKernel("CSMain");
                sliceShader.SetTexture(kernelIndex, "noise", rt);
                sliceShader.SetInt("layer", i);
                sliceShader.SetTexture(kernelIndex, "Result", render);
                sliceShader.Dispatch(kernelIndex, size, size, 1);

                finalSlices[i] = new Texture2D(size, size, textureFormat, mipChain);
                RenderTexture.active = render;
                finalSlices[i].ReadPixels(new Rect(0, 0, size, size), 0, 0);
                finalSlices[i].Apply();
            }

            texture = new Texture3D(size, size, size, textureFormat, true);
            texture.filterMode = FilterMode.Trilinear;
            Color[] outputPixels = ((Texture3D)texture).GetPixels();
            for (int k = 0; k < size; k++)
            {
                Color[] layerPixels = finalSlices[k].GetPixels();
                for (int i = 0; i < size; i++)
                {
                    for (int j = 0; j < size; j++)
                    {
                        outputPixels[i + j * size + k * size * size] = layerPixels[i + j * size];
                    }
                }
            }
            ((Texture3D)texture).SetPixels(outputPixels);
            ((Texture3D)texture).Apply();

            return texture;
        }

        return null;
    }
}
