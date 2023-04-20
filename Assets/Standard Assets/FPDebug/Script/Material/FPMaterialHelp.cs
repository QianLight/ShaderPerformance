using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System;
using UnityEngine;
using UnityEngine.Rendering;

public sealed class FPMaterialHelp
{
    public static RenderMat GetMaterialPara(GameObject obj, int id)
    {
        RenderMat rmat = new RenderMat();
        MeshRenderer mr = obj.GetComponent<MeshRenderer>();
        SkinnedMeshRenderer smr = obj.GetComponent<SkinnedMeshRenderer>();
        Light lig = obj.GetComponent<Light>();
        Material mat = null;
        rmat.ID = id;
        rmat.Type = 0;
        rmat.Layer = obj.layer;
        if (mr != null)
        {
            rmat.Type = 1;
            MeshFilter mf = obj.GetComponent<MeshFilter>();
            if(mf != null)
            {
                rmat.MeshName = mf.sharedMesh == null ? null : mf.sharedMesh.name;
            }
            rmat.CastShadow = mr.shadowCastingMode == ShadowCastingMode.On;
            rmat.LightProbeUsage = (int)mr.lightProbeUsage;
            rmat.ReflectionProbeUsage =(int) mr.reflectionProbeUsage;
            mat = mr.sharedMaterial;
        }
        else if(smr != null)
        {
            rmat.Type = 2;
            rmat.CastShadow = smr.shadowCastingMode == ShadowCastingMode.On;
            rmat.LightProbeUsage = (int)smr.lightProbeUsage;
            rmat.ReflectionProbeUsage = (int)smr.reflectionProbeUsage;
            mat = smr.sharedMaterial;
        }
        else if (lig != null)
        {
            rmat.Type = 3;
            rmat.LightCullingMask = lig.cullingMask;
            rmat.LightColor = lig.color;
#if UNITY_EDITOR
            rmat.LightmapBakeType = (int)lig.lightmapBakeType;
#endif
            rmat.LightShadows = (int)lig.shadows;
        }
        if (mat != null)
        {
            rmat.MatName = mat.name;
            rmat.ShaderName = mat.shader.name;
            rmat.Keywords = ShaderKeywords2String(mat.shaderKeywords);
            GetShaderProperty(mat, rmat);
        }
        return rmat;
    }
    public static void SetMaterialPara(GameObject obj, RenderMat para)
    {
        obj.layer = para.Layer;
        Material mat = null;
        if (para.Type == 1)
        {
            MeshRenderer mr = obj.GetComponent<MeshRenderer>();
            if(para.CastShadow)
            {
                mr.shadowCastingMode = ShadowCastingMode.On;
            }
            else
            {
                mr.shadowCastingMode = ShadowCastingMode.Off;
            }
            mr.lightProbeUsage = (LightProbeUsage)para.LightProbeUsage;
            mr.reflectionProbeUsage = (ReflectionProbeUsage)para.ReflectionProbeUsage;
            mat = mr.sharedMaterial;
        }
        else if (para.Type == 2)
        {
            SkinnedMeshRenderer smr = obj.GetComponent<SkinnedMeshRenderer>();
            if (para.CastShadow)
            {
                smr.shadowCastingMode = ShadowCastingMode.On;
            }
            else
            {
                smr.shadowCastingMode = ShadowCastingMode.Off;
            }
            smr.lightProbeUsage = (LightProbeUsage)para.LightProbeUsage;
            smr.reflectionProbeUsage = (ReflectionProbeUsage)para.ReflectionProbeUsage;
            mat = smr.sharedMaterial;
        }
        else if (para.Type == 3)
        {
            Light lig = obj.GetComponent<Light>();
            lig.cullingMask = para.LightCullingMask;
            lig.color = para.LightColor;
#if UNITY_EDITOR
            lig.lightmapBakeType = (LightmapBakeType)para.LightmapBakeType;
#endif
            lig.shadows = (LightShadows)para.LightShadows;
        }
        if (mat != null)
        {
            mat.shaderKeywords= String2ShaderKeywords(para.Keywords);
            SetShaderProperty(mat, para);
        }
    }
    public static string ShaderKeywords2String(string[] keys)
    {
        System.Text.StringBuilder sb = new System.Text.StringBuilder();
        for(int i = 0; i < keys.Length; i++)
        {
            if(i != 0)
            {
                sb.Append(" ");
            }
            sb.Append(keys[i]);
        }
        return sb.ToString();
    }
    public static string[] String2ShaderKeywords(string keys)
    {
        return keys.Split(new char[] { ' '}, StringSplitOptions.RemoveEmptyEntries);
    }
    public static void GetShaderProperty(Material mat, RenderMat rm)
    {
        rm.Vectors = new List<RenderMatVector>();
        rm.Floats = new List<RenderMatFloat>();
        Shader shader = mat.shader;
        if(shader != null)
        {
            int count = shader.GetPropertyCount();
            for (int i = 0; i < count; i++)
            {
                ShaderPropertyType type = shader.GetPropertyType(i);
                ShaderPropertyFlags flag = shader.GetPropertyFlags(i);
                string name = shader.GetPropertyName(i);
                if (flag == ShaderPropertyFlags.HideInInspector)
                    continue;
                if (type == ShaderPropertyType.Vector || type == ShaderPropertyType.Color)
                {
                    RenderMatVector rmv = new RenderMatVector();
                    rmv.Name = name;
                    rmv.Value = mat.GetVector(name);
                    rm.Vectors.Add(rmv);
                }
                if (type == ShaderPropertyType.Float || type == ShaderPropertyType.Range)
                {
                    RenderMatFloat rmf = new RenderMatFloat();
                    rmf.Name = name;
                    rmf.Value = mat.GetFloat(name);
                    rm.Floats.Add(rmf);
                }
            }
        }
    }
    public static void SetShaderProperty(Material mat, RenderMat rm)
    {
        for(int i = 0; i < rm.Vectors.Count; i++)
        {
            RenderMatVector rmv = rm.Vectors[i];
            if(mat.HasProperty(rmv.Name))
            {
                mat.SetVector(rmv.Name, rmv.Value);
            }
        }
        for (int i = 0; i < rm.Vectors.Count; i++)
        {
            RenderMatFloat rmf = rm.Floats[i];
            if (mat.HasProperty(rmf.Name))
            {
                mat.SetFloat(rmf.Name, rmf.Value);
            }
        }
    }
}
