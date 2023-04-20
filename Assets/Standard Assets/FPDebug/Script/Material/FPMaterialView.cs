using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FPMaterialView
{
    private static Transform obj = null;
    private static float cameraFar = -1;
    public static void View(Material mat, bool other)
    {
        Camera cam = Camera.main;
        MeshRenderer mr = null;
        if (obj == null)
        {
            if (cam != null)
            {
                GameObject g = new GameObject("FPMaterialView");
                MeshFilter mf = g.AddComponent<MeshFilter>();
                mf.sharedMesh = CreatePlaneMesh();
                mr = g.AddComponent<MeshRenderer>();
                obj = g.transform;
                obj.parent = cam.transform;
                obj.localRotation = Quaternion.identity;

                float fov = cam.fieldOfView;
                float near = cam.nearClipPlane;
                float aspect = cam.aspect;

                float halfHeight = near * Mathf.Tan(fov * 0.5f * Mathf.Deg2Rad);
                obj.localScale = new Vector3(halfHeight * aspect, halfHeight, 1);
                obj.localPosition = Vector3.forward * (cam.nearClipPlane + 0.00001f);
            }
        }
        else
        {
            mr = obj.GetComponent<MeshRenderer>();
        }
        mr.sharedMaterial = mat;

        if (cam != null)
        {
            if (other)
            {
                if (cameraFar > 0)
                {
                    cam.farClipPlane = cameraFar;
                    cameraFar = -1;
                }
            }
            else
            {
                if (cameraFar < 0)
                {
                    cameraFar = cam.farClipPlane;
                    cam.farClipPlane = cam.nearClipPlane + 0.0001f;
                }
            }
        }
    }
    public static Mesh CreatePlaneMesh(float size = 1)
    {
        Mesh mesh = new Mesh();
        float rSize = size;
        Vector3[] vertices = {
            new Vector3 (rSize, rSize, 0),
            new Vector3 (rSize, -rSize, 0),
            new Vector3 (-rSize, -rSize, 0),
            new Vector3 (-rSize, rSize, 0)
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
        return mesh;
    }
    public static void Clear()
    {
        Camera cam = Camera.main;
        if (cam != null && cameraFar > 0)
        {
            cam.farClipPlane = cameraFar;
            cameraFar = -1;
        }

        if (obj != null)
        {
            MeshFilter mf = obj.GetComponent<MeshFilter>();
            if (mf != null && mf.sharedMesh != null)
            {
                GameObject.DestroyImmediate(mf.sharedMesh);
            }
            GameObject.DestroyImmediate(obj.gameObject);
        }
    }
    public static void ClearList()
    {
        matList.Clear();
    }
    private static Dictionary<string, Material> matList = new Dictionary<string, Material>();
    static bool lastShare = true;
    static Material skybox = null;
    public static FPDebugShaderList GetShaderList(bool share, Dictionary<int, GameObject> currentList)
    {
        lastShare = share;
        FPDebugShaderList result = new FPDebugShaderList();
        matList.Clear();
        if (currentList != null)
        {
            string shaderName = null;
            result.Shaders = new List<string>();
            skybox = RenderSettings.skybox;
            if (skybox != null)
            {
                shaderName = skybox.shader.name;
                if (!matList.ContainsKey(shaderName))
                {
                    matList[shaderName] = skybox;
                    result.Shaders.Add(shaderName);
                }
            }
            try
            {
                foreach (KeyValuePair<int, GameObject> kv in currentList)
                {
                    GameObject g = kv.Value;
                    if (g == null)
                        continue;
                    Renderer renderer = g.GetComponent<Renderer>();
                    if (renderer != null)
                    {
                        Material mat = null;
                        if (lastShare)
                            mat = renderer.sharedMaterial;
                        else
                            mat = renderer.material;
                        if (mat != null)
                        {
                            shaderName = mat.shader.name;
                            if (!matList.ContainsKey(shaderName))
                            {
                                matList[shaderName] = mat;
                                result.Shaders.Add(shaderName);
                            }
                        }
                    }
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogError(ex);
            }
        }
        return result;
    }

    public static void showList(string shaderName, bool state, Dictionary<int, GameObject> currentList)
    {
        try
        {
            foreach (KeyValuePair<int, GameObject> kv in currentList)
            {
                GameObject g = kv.Value;
                if (g == null)
                    continue;
                Renderer renderer = g.GetComponent<Renderer>();
                if (renderer != null)
                {
                    Material mat = null;
                    if (lastShare)
                        mat = renderer.sharedMaterial;
                    else
                        mat = renderer.material;
                    if (mat != null && mat.shader != null && mat.shader.name == shaderName && kv.Value != null)
                    {
                        kv.Value.SetActive(state);
                    }
                }
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError(ex);
        }
    }

    private static List<Material> lastReplaceList = new List<Material>();
    private static Shader lastReplaceShader = null;
    public static void replaceShader(string shaderName, bool state, Dictionary<int, GameObject> currentList, Shader shader)
    {
        if (state)
        {
            lastReplaceList.Clear();
            try
            {
                if (skybox != null && shaderName == skybox.shader.name)
                {
                    if (lastReplaceShader == null)
                    {
                        lastReplaceShader = skybox.shader;
                    }
                    lastReplaceList.Add(skybox);
                    skybox.shader = shader;
                    return;
                }

                foreach (KeyValuePair<int, GameObject> kv in currentList)
                {
                    GameObject g = kv.Value;
                    if (g == null)
                        continue;
                    Renderer renderer = kv.Value.GetComponent<Renderer>();
                    if (renderer != null)
                    {
                        Material mat = null;
                        if (lastShare)
                            mat = renderer.sharedMaterial;
                        else
                            mat = renderer.material;

                        if (mat != null && mat.shader != null && mat.shader.name == shaderName && kv.Value != null)
                        {
                            if (lastReplaceShader == null)
                            {
                                lastReplaceShader = mat.shader;
                            }
                            if (!lastReplaceList.Contains(mat))
                            {
                                lastReplaceList.Add(mat);
                            }
                            mat.shader = shader;
                        }
                    }
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogError(ex);
            }
        }
        else
        {
            if (lastReplaceShader != null)
            {
                foreach (Material mat in lastReplaceList)
                {
                    if (mat == null)
                        continue;

                    if (mat != null)
                    {
                        mat.shader = lastReplaceShader;
                    }
                }
                lastReplaceShader = null;
            }
            lastReplaceList.Clear();
        }
    }
    public static void ShowShader(int id, int state, bool otherRender, Dictionary<int, GameObject> currentList, Shader shader)
    {
        if (state == 1)
        {
            int i = 0;
            foreach (KeyValuePair<string, Material> kv in matList)
            {
                if (i == id)
                {
                    View(kv.Value, otherRender);
                    break;
                }
                i++;
            }
        }
        else if (state == 2)
        {
            Clear();
        }
        else if (state == 3)
        {
            int i = 0;
            foreach (KeyValuePair<string, Material> kv in matList)
            {
                if (i == id)
                {
                    showList(kv.Key, false, currentList);
                    break;
                }
                i++;
            }
        }
        else if (state == 4)
        {
            int i = 0;
            foreach (KeyValuePair<string, Material> kv in matList)
            {
                if (i == id)
                {
                    showList(kv.Key, true, currentList);
                    break;
                }
                i++;
            }
        }
        else if (state == 5)
        {
            if (shader != null)
            {
                int i = 0;
                foreach (KeyValuePair<string, Material> kv in matList)
                {
                    if (i == id)
                    {
                        replaceShader(kv.Key, true, currentList, shader);
                        break;
                    }
                    i++;
                }
            }
        }
        else if (state == 6)
        {
            int i = 0;
            foreach (KeyValuePair<string, Material> kv in matList)
            {
                if (i == id)
                {
                    replaceShader(kv.Key, false, currentList, shader);
                    break;
                }
                i++;
            }
        }
    }
}
