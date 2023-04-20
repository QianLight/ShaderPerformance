//#define _DEBUG_RENDERER_BAKER_

#if _DEBUG_RENDERER_BAKER_

using com.pwrd.hlod.editor;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using com.pwrd.hlod;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using Object = UnityEngine.Object;

public class RendererBakeDebuger : MonoBehaviour
{
    [MenuItem("RendererBakeDebuger/Test bake")]
    private static void TestBake()
    {
        var go = Selection.activeObject as GameObject;
        if (go == null)
            return;

        var renderers = new List<Renderer>();
        foreach (var tar in Selection.objects)
        {
            var target = tar as GameObject;
            if (target == null)
                continue;

            foreach (var r in target.GetComponentsInChildren<Renderer>())
            {
                if (!renderers.Contains(r))
                    renderers.Add(r);
            }
        }

        foreach (var tar in Selection.objects)
        {
            var target = tar as GameObject;
            if (target == null)
                continue;

            foreach (var group in target.GetComponentsInChildren<LODGroup>())
            {
                var lods = group.GetLODs();
                for (int i = 1; i < lods.Length; i++)
                {
                    foreach (var r in lods[i].renderers)
                    {
                        if (renderers.Contains(r))
                            renderers.Remove(r);
                    }
                }
            }
        }

        var render = renderers.First();
        var filter = render.transform.GetComponent<MeshFilter>();
        var path = GetAssetHLODFolder();
        var name = render.name;

        var baker = new RendererBaker();
        baker.isDebug = true;
        var tags = FindObjectsOfType<HLODDecalTag>();
        baker.SetDecalData(new List<HLODDecalTag>(tags));
        var texture2D = baker.Bake(render);

        var bytes = texture2D.EncodeToPNG();
        var filePath = Path.Combine(path, name + ".png");
        if (File.Exists(filePath))
        {
            File.Delete(filePath);
        }

        using (var stream = File.Create(filePath))
        {
            stream.Write(bytes, 0, bytes.Length);
        }

        AssetDatabase.Refresh();
    }

    private static string GetAssetHLODFolder()
    {
        var time = DateTime.Now.ToString("MMdd_HHmmss_fff");
        var path = Path.Combine(Application.dataPath, "HLOD/" + time);
        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
        }

        return path;
    }


    private static Rect GetUV2Area(Mesh mesh)
    {
        var area = new Rect();
        area.min = new Vector2(float.MaxValue, float.MaxValue);
        area.max = new Vector2(float.MinValue, float.MinValue);
        var uv2s = mesh.uv2;
        if (uv2s.Length == 0)
        {
            uv2s = mesh.uv;
        }

        foreach (var uv2 in uv2s)
        {
            if (uv2.x < area.xMin)
            {
                area.xMin = uv2.x;
            }

            if (uv2.y < area.yMin)
            {
                area.yMin = uv2.y;
            }

            if (uv2.x > area.xMax)
            {
                area.xMax = uv2.x;
            }

            if (uv2.y > area.yMax)
            {
                area.yMax = uv2.y;
            }
        }

        return area;
    }

    private static string GetOutputFolder()
    {
        var path = Path.Combine(Application.dataPath, "HLOD");
        path += "\\";
        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
        }

        return path;
    }

    static float[] Convert(Vector3[] values)
    {
        var list = new List<float>();
        foreach (var v in values)
        {
            list.Add(v.x);
            list.Add(v.y);
            list.Add(v.z);
        }

        return list.ToArray();
    }

    static float[] Convert(Vector2[] values)
    {
        var list = new List<float>();
        foreach (var v in values)
        {
            list.Add(v.x);
            list.Add(v.y);
        }

        return list.ToArray();
    }

    static float[] Convert(Color[] values)
    {
        var list = new List<float>();
        foreach (var v in values)
        {
            list.Add(v.r);
            list.Add(v.g);
            list.Add(v.b);
            list.Add(v.a);
        }

        return list.ToArray();
    }
}
#endif