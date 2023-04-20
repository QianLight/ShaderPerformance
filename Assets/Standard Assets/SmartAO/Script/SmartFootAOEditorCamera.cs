#if UNITY_EDITOR

using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
public class SmartFootAOEditorCamera
{
    private static int renderLayer = 2;
    private static Camera last;
    public static Camera CreateCamera(string name, Bounds8Point bounds)
    {
        if (last == null)
        {
            GameObject camObj = new GameObject(name);
            camObj.transform.rotation = Quaternion.Euler(90, 0, 0);
            last = camObj.AddComponent<Camera>();
            last.orthographic = true;
            last.backgroundColor = new Color(0, 0, 0, 0);
            last.clearFlags = CameraClearFlags.Color;
            last.cullingMask = 1 << 2;
            //last.enabled = false;
            Camera mainCam = Camera.main;
            if (mainCam != null)
                mainCam.cullingMask &= ~(1 << renderLayer);
        }
        Vector3 pos = (bounds.P0 + bounds.P2) * 0.5f;
        last.transform.position = pos;

        last.nearClipPlane = 0;
        last.farClipPlane = Mathf.Abs(bounds.P4.y - bounds.P0.y);

        last.aspect = Vector3.Magnitude(bounds.P1 - bounds.P2) / Vector3.Magnitude(bounds.P1 - bounds.P0);
        last.orthographicSize = Vector3.Magnitude(bounds.P1 - bounds.P0) * 0.5f;
        return last;
    }

    public static void ClearCamera()
    {
        if (last != null)
        {
            GameObject.DestroyImmediate(last.gameObject);
        }
        last = null;
    }

    public static void Delete()
    {
        if (last != null)
        {
            GameObject.DestroyImmediate(last.gameObject);
            last = null;
        }
    }
    public static Texture3D Create3DTexture(string assetPath, Bounds8Point box, float ppm)
    {
        int sliceCount = Mathf.CeilToInt((box.P0.y - box.P4.y) / ppm);

        float fx = last.orthographicSize / ppm;
        int xCount = Mathf.CeilToInt(fx);
        int yCount = Mathf.CeilToInt(fx / last.aspect);
        //Debug.LogWarning("Create3DTexture:" + sliceCount + "," + xCount + "," + yCount);
        //return null;
        Texture2D[] t2dArray = new Texture2D[sliceCount];
        for (int i = 0; i < sliceCount; i++)
        {
            last.transform.position -= new Vector3(0, ppm, 0);
            RenderTexture xRayLayer = new RenderTexture(xCount, yCount, 24, RenderTextureFormat.R8);
            last.targetTexture = xRayLayer;
            last.Render();

            RenderTexture.active = xRayLayer;
            Texture2D texture2D = new Texture2D(xCount, yCount, TextureFormat.R8, false);
            texture2D.ReadPixels(new Rect(0, 0, xCount, yCount), 0, 0);
            texture2D.Apply();

            t2dArray[i] = texture2D;
            RenderTexture.active = last.targetTexture = null;
            GameObject.DestroyImmediate(xRayLayer);
        }

        Texture3D result = new Texture3D(xCount, yCount, sliceCount, TextureFormat.R8, false);
        result.wrapMode = TextureWrapMode.Clamp;
        var color3d = new Color[xCount * yCount * sliceCount];
        int idx = 0;

        for (int z = 0; z < sliceCount; z++)
        {
            for (int y = 0; y < yCount; ++y)
            {
                for (int x = 0; x < xCount; ++x, ++idx)
                {
                    color3d[idx] = blurColor(true, t2dArray, x, y, z, xCount, yCount, sliceCount);
                }
            }
        }

        result.SetPixels(color3d);
        result.Apply();
        for (int i = 0; i < sliceCount; i++)
        {
            GameObject.DestroyImmediate(t2dArray[i]);
        }
        t2dArray = null;
        //result.isReadable = false;
        if (AssetDatabase.LoadAssetAtPath<Texture3D>(assetPath) != null)
        {
            AssetDatabase.DeleteAsset(assetPath);
        }
        AssetDatabase.CreateAsset(result, assetPath);
        AssetDatabase.Refresh();
        return result;
    }
    public static void CreateShareBoxMesh(string path, float size = 1)
    {
        Mesh mesh = new Mesh();
        float rSize = 0.5f * size;
        Vector3[] vertices = {
            new Vector3 (rSize, rSize, rSize),
            new Vector3 (rSize, rSize, -rSize),
            new Vector3 (-rSize, rSize, -rSize),
            new Vector3 (-rSize, rSize, rSize),

            new Vector3 (rSize, -rSize, rSize),
            new Vector3 (rSize, -rSize, -rSize),
            new Vector3 (-rSize, -rSize, -rSize),
            new Vector3 (-rSize, -rSize, rSize),
        };
        int[] triangles = {
            0, 1, 2,
         0, 2, 3,
            4, 7, 6,
         6, 5, 4,
            1, 0, 4,
         1, 4, 5,
            2, 1, 5,
         2, 5, 6,
            3, 2, 6,
         3, 6, 7,
            0, 3, 7,
         0, 7, 4
        };
        mesh.vertices = vertices;
        mesh.triangles = triangles;

        mesh.RecalculateNormals();
        AssetDatabase.CreateAsset(mesh, path);
        AssetDatabase.Refresh();
    }
    public static void CreateSharePlaneMesh(string path, float size = 1)
    {
        Mesh mesh = new Mesh();
        float rSize = 0.5f * size;
        Vector3[] vertices = {
            new Vector3 (rSize, 0, rSize),
            new Vector3 (rSize, 0, -rSize),
            new Vector3 (-rSize, 0, -rSize),
            new Vector3 (-rSize, 0, rSize)
        };
        Vector2[] uv ={
            new Vector2(1, 0),
            new Vector2(1, 1),
            new Vector2(0, 1),
            new Vector2(0, 0)
        };
        int[] triangles = {
            0, 1, 2,
         0, 2, 3
        };
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.uv = uv;
        mesh.RecalculateNormals();
        AssetDatabase.CreateAsset(mesh, path);
        AssetDatabase.Refresh();
    }
    static Color blurColor(bool blur, Texture2D[] colors, int x, int y, int z, int xLen, int yLen, int zLen)
    {
        if (blur)
        {
            //使用5*5*5采样立方体
            Vector4 v4 = Vector4.zero;
            for (int tmpZ = -2; tmpZ <= 2; tmpZ++)
            {
                for (int tmpY = -2; tmpY <= 2; tmpY++)
                {
                    for (int tmpX = -2; tmpX <= 2; tmpX++)
                    {
                        v4 += sampleColor(colors, x + tmpX, y + tmpY, z + tmpZ, xLen, yLen, zLen, 0.008f);
                    }
                }
            }
            v4 = new Vector4(v4.x / v4.w, v4.y / v4.w, v4.z / v4.w);
            return new Color(v4.x, v4.y, v4.z, v4.w);
        }
        else
        {
            Vector4 v4 = sampleColor(colors, x, y, z, xLen, yLen, zLen, 1);
            return new Color(v4.x, v4.y, v4.z, v4.w);
        }

    }
    static Vector4 sampleColor(Texture2D[] colors, int x, int y, int z, int xLen, int yLen, int zLen, float scale)
    {
        Vector4 result = Vector4.zero;
        if (x > 0 && x < xLen && y > 0 && y < yLen && z > 0 && z < zLen)
        {
            Texture2D loadedtexture = colors[z];
            Color c = loadedtexture.GetPixel(x, y);
            result = new Vector4(c.r * scale, c.g * scale, c.b * scale, c.a * scale);
        }
        return result;
    }
    public static GameObject CreateGameObject(string name, string absPath, string meshPath, string shaderPath, Transform root, Bounds8Point box, Texture3D tex, float adjust, Vector3 addScale)
    {
        GameObject result = null;
        if (root != null)
        {
            bool find = false;
            foreach (Transform t in root)
            {
                if (t.name == name)
                {
                    find = true;
                    result = t.gameObject;
                    result.transform.parent = null;
                    break;
                }
            }
            MeshRenderer mr = null;
            if (find)
            {
                mr = result.GetComponent<MeshRenderer>();
            }
            else
            {
                result = new GameObject(name);
                MeshFilter mf = result.AddComponent<MeshFilter>();
                mr = result.AddComponent<MeshRenderer>();
                Material mat = null;
                string matPath = absPath + tex.name + ".mat";
                mat = AssetDatabase.LoadAssetAtPath<Material>(matPath);
                if (mat == null)
                {
                    mat = new Material(AssetDatabase.LoadAssetAtPath<Shader>(shaderPath));
                    AssetDatabase.CreateAsset(mat, matPath);
                    AssetDatabase.Refresh();
                }
                mr.sharedMaterial = mat;
                mf.sharedMesh = AssetDatabase.LoadAssetAtPath<Mesh>(meshPath);
            }
            try
            {
                mr.sharedMaterial.mainTexture = tex;
            }
            catch (System.Exception ex)
            {

            }
            result.transform.position = (box.P2 + box.P4) / 2.0f + Vector3.down * adjust;
            result.transform.rotation = Quaternion.Euler(90, 0, 0);
            result.transform.localScale = new Vector3(Mathf.Abs(box.P1.x - box.P2.x) * addScale.x, Mathf.Abs(box.P1.z - box.P0.z) * addScale.z, Mathf.Abs(box.P4.y - box.P2.y) * addScale.y);
            result.transform.parent = root;
        }
        return result;
    }
    public static GameObject CopyGameObject(string name, GameObject source, Transform root, Bounds8Point box, float adjust, Vector3 addScale, bool left = false)
    {
        GameObject result = null;
        if (root != null)
        {
            bool find = false;
            foreach (Transform t in root)
            {
                if (t.name == name)
                {
                    find = true;
                    result = t.gameObject;
                    result.transform.parent = null;
                    break;
                }
            }
            if (!find)
            {
                result = GameObject.Instantiate(source);
                result.name = name;
            }
            result.transform.position = (box.P2 + box.P4) / 2.0f + Vector3.down * adjust;
            result.transform.rotation = Quaternion.Euler(90, 0, 0);
            result.transform.localScale = new Vector3(Mathf.Abs(box.P1.x - box.P2.x) * addScale.x * (left ? 1 : -1), Mathf.Abs(box.P1.z - box.P0.z) * addScale.z, Mathf.Abs(box.P4.y - box.P2.y) * addScale.y);
            result.transform.parent = root;
        }
        return result;
    }
    private static Material[] lastMats = null;
    public static void SetRenderVision(Transform root, SkinnedMeshRenderer sk, string sliceShaderPath, bool left)
    {
        lastMats = sk.sharedMaterials;
        Material[] mats = new Material[lastMats.Length];
        if (lastMats != null && lastMats.Length > 0)
        {
            for (int i = 0; i < mats.Length; i++)
            {
                Material mat = AssetDatabase.LoadAssetAtPath<Material>(sliceShaderPath);
                mat.SetFloat("_SliceState", left ? 0 : 1);
                mats[i] = mat;
            }
        }
        sk.sharedMaterials = mats;

        lastLayer = sk.gameObject.layer;
        sk.gameObject.layer = renderLayer;
        //setLayer(root, renderLayer);
        //Debug.LogError(sk.sharedMaterial);
        //Debug.LogError(sk.sharedMaterial.GetFloat("_SliceState"));
    }

    public static void ClearRenderVision(Transform root, SkinnedMeshRenderer sk)
    {
        //setLayer(root, lastLayer);
        sk.gameObject.layer = lastLayer;

        Material[] mats = sk.sharedMaterials;
        sk.sharedMaterials = lastMats;
        mats = null;
        lastMats = null;
    }
    private static int lastLayer = 0;
    static void setLayer(Transform root, int layer)
    {
        if (root.gameObject.layer != layer)
        {
            root.gameObject.layer = layer;
        }

        foreach (Transform t in root)
        {
            setLayer(t, layer);
        }
    }
}
#endif