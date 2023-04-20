using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class FaceBinding
{
    public Transform bone;
    public List<Renderer> renderers = new List<Renderer>();
    private Renderer _renderer;

    public Renderer FaceRenderer
    {
        get => _renderer;
        set => _renderer = value;
    }

}

public static class SDFFaceCore
{
    private static MaterialPropertyBlock mpb;
    private static readonly int _FaceLightingParam = Shader.PropertyToID("_FaceLightingParam");

    public static void ApplyFaceParam(Transform bone, Renderer renderer, Vector3 lightDirection)
    {
        if (!bone || !renderer)
            return;

        Vector3 axisX, axisY;
        bool skinned = renderer is SkinnedMeshRenderer;
        if (skinned)
        {
            axisX = -bone.forward;
            axisY = -bone.right;
        }
        else
        {
            axisX = bone.up;
            axisY = bone.forward;
        }

        ApplyFaceParam(renderer, lightDirection, axisX, axisY, skinned);
    }
    
    private static void ApplyFaceParam(Renderer renderer, Vector3 lightDirection, Vector3 axisX, Vector3 axisY,
        bool skinned)
    {
        Vector4 faceLightingParam = CalculateFacemapParams(axisX, axisY, lightDirection, skinned);
  
        //Shader.SetGlobalVector(_FaceLightingParam, faceLightingParam);
        // if (Application.isPlaying)
        // {
        //     List<Material> materials = CFEngine.ListPool<Material>.Get();
        //     renderer.GetSharedMaterials(materials);
        //     foreach (Material material in materials)
        //     {
        //         if (material)
        //             material.SetVector(_FaceLightingParam, faceLightingParam);
        //     }
        //     CFEngine.ListPool<Material>.Release(ref materials);
        // }
        // else
        {
            mpb = mpb ?? new MaterialPropertyBlock();
            mpb.Clear();
            renderer.GetPropertyBlock(mpb);
            mpb.SetVector(_FaceLightingParam, faceLightingParam);
            renderer.SetPropertyBlock(mpb);
        }
    }

    private static Vector4 CalculateFacemapParams(Vector3 axisX, Vector3 axisY, Vector3 lightDirWS, bool skinned)
    {
        Vector3 axisZ = Vector3.Cross(axisY, axisX);
        float x = Vector3.Dot(lightDirWS, axisX);
        float y = Vector3.Dot(lightDirWS, axisY);
        float z = Vector3.Dot(lightDirWS, axisZ);

        Vector3 lightDirOS = new Vector3(x, y, z);
        if (skinned)
            lightDirOS = Quaternion.Euler(0, 90, 0) * lightDirOS;
        float lightRadianceAngleOS = -Mathf.Atan2(lightDirOS.z, lightDirOS.x) / Mathf.PI;

        float uvMulX;
        float uvAddX;
        float compare;
        if (lightRadianceAngleOS < 0)
        {
            uvMulX = 1f;
            uvAddX = 0f;
            compare = 1 + lightRadianceAngleOS;
        }
        else
        {
            uvMulX = -1f;
            uvAddX = 1f;
            compare = 1 - lightRadianceAngleOS;
        }

        Vector4 param = new Vector4(uvMulX, uvAddX, compare, lightRadianceAngleOS);
        return param;
    }
}