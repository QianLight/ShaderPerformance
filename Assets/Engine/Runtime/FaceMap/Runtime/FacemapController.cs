using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class FacemapController : MonoBehaviour
{
    public new Light light;
    public Transform bone;

    private static readonly int _FaceLightingParam = Shader.PropertyToID("_FaceLightingParam");
    private static MaterialPropertyBlock mpb;
    private static List<Renderer> renderers = new List<Renderer>();

    private void Update()
    {
        if (!light) return;
        GetComponentsInChildren(renderers);
        Vector3 lightDirWS = light.transform.forward;
        mpb ??= new MaterialPropertyBlock();
        foreach (Renderer r in renderers)
        {
            Matrix4x4 worldToFaceMatrix = default;
            Vector3 ax, ay, az;
            if (r is SkinnedMeshRenderer)
            {
                ax = bone.forward;
                ay = -bone.right;
                az = -bone.up;
            }
            else
            {
                ax = bone.right;
                ay = bone.up;
                az = bone.forward;
            }

            worldToFaceMatrix.m00 = ax.x;
            worldToFaceMatrix.m01 = ay.x;
            worldToFaceMatrix.m02 = az.x;
            worldToFaceMatrix.m10 = ax.y;
            worldToFaceMatrix.m11 = ay.y;
            worldToFaceMatrix.m12 = az.y;
            worldToFaceMatrix.m20 = ax.z;
            worldToFaceMatrix.m21 = ay.z;
            worldToFaceMatrix.m22 = az.z;

            Vector4 faceLightingParam = GetFaceLightingParams(worldToFaceMatrix, lightDirWS);

            r.GetPropertyBlock(mpb);
            mpb.SetVector(_FaceLightingParam, faceLightingParam);
            r.SetPropertyBlock(mpb);
            mpb.Clear();
        }
    }

    public Vector4 GetFaceLightingParams(Matrix4x4 worldToFaceMatrix, Vector3 lightDirWS)
    {
        Vector3 lightDirOS = worldToFaceMatrix.MultiplyVector(lightDirWS.normalized).normalized;
        Vector2 lightDirOS2d = new Vector2(lightDirOS.x, lightDirOS.z).normalized;
        Vector2 lightDirOSRotateInv90 = new Vector2(-lightDirOS2d.y, lightDirOS2d.x);
        float lightRadianceAngleOS = Mathf.Atan2(lightDirOSRotateInv90.y, lightDirOSRotateInv90.x) / Mathf.PI;
        float uvMulX = lightRadianceAngleOS < 0 ? 1f : -1f;
        float uvAddX = lightRadianceAngleOS < 0 ? 0f : 1f;
        float compare = 1 - Mathf.Abs(lightRadianceAngleOS);
        return new Vector4(uvMulX, uvAddX, compare, 0);
    }
}