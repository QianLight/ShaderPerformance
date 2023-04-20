using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

public static class FaceMapCore
{
    public class FaceMapData
    {
        public bool[] data;
        public int width;
        public int height;
    }

    public static bool IsValid(IList<FaceMapFrame> frames, out List<string> errors)
    {
        errors = new List<string>();
        int count = frames.Count;

        // Check count.
        if (count < 2)
        {
            errors.Add($"贴图数量太少，输入数量为{count}.");
            return false;
        }

        for (int i = 0; i < count; i++)
        {
            FaceMapFrame frame = frames[i];

            // Check angles 
            if (frame.angle < 0 || frame.angle > 180)
            {
                errors.Add($"输入角度不正确，应为0~180度。图索槽为{i}，输入角度为{frame.angle}");
                return false;
            }

            // Check texture exist 
            if (!frame.texture)
            {
                errors.Add($"输入贴图为空，贴图槽为{i}。");
                return false;
            }
        }

        Texture2D firstTexture = frames[0].texture;
        int width = firstTexture.width;
        int height = firstTexture.height;

        bool SetTextureImporter(TextureImporter importer)
        {
            bool dirty = false;

            if (!importer.crunchedCompression)
            {
                importer.crunchedCompression = false;
                dirty = true;
            }

            if (!importer.isReadable)
            {
                importer.isReadable = true;
                importer.mipmapEnabled = false;
                importer.sRGBTexture = false;
                importer.textureCompression = TextureImporterCompression.Uncompressed;
                dirty = true;
            }

            return dirty;
        }

        bool dirty = false;
        for (int i = 0; i < count; i++)
        {
            // Check size
            Texture2D texture = frames[i].texture;
            if (texture.width != width || texture.height != height)
            {
                errors.Add($"贴图尺寸不一致，贴图槽为{i}。");
                return false;
            }

            // Check texture importer
            if (AssetDatabase.Contains(texture))
            {
                string path = AssetDatabase.GetAssetPath(texture);
                TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
                if (importer)
                {
                    if (SetTextureImporter(importer))
                    {
                        if (!dirty)
                        {
                            AssetDatabase.StartAssetEditing();
                        }

                        importer.SaveAndReimport();
                        dirty = true;
                    }
                }
            }
        }

        if (dirty)
        {
            AssetDatabase.StopAssetEditing();
        }

        return true;
    }

    #region SDF

    private static float[][] sdfResult;
    private static Thread[] threads;
    private static int width;
    private static int height;
    private static bool processing;
    private static IList<FaceMapFrame> frames;
    private static string savePath;
    private static Action<Texture2D> callback;
    public static bool Processing => processing;

    public static void Generate(IList<FaceMapFrame> frames, string savePath, Action<Texture2D> callback)
    {
        FaceMapCore.frames = frames;
        sdfResult = new float[frames.Count][];
        threads = new Thread[frames.Count];
        processing = true;
        FaceMapCore.savePath = savePath;
        FaceMapCore.callback = callback;

        Texture2D texture = frames[0].texture;
        width = texture.width;
        height = texture.height;

        for (int i = 0; i < frames.Count; i++)
        {
            FaceMapFrame frame = frames[i];
            float distance = Mathf.Sqrt(2) * frame.texture.width;
            FaceMapData data = ExtractTexture(frame.texture);
            int index = i;
            sdfResult[index] = new float[width * height];
            Thread thread = new Thread(() => GenerateSDF(data, distance, ref sdfResult[index]));
            threads[i] = thread;
            thread.Start();
        }

        EditorApplication.update += Update;
    }

    private static void Update()
    {
        // Wait for finish.
        foreach (Thread thread in threads)
            if (thread.IsAlive)
                return;
        
        EditorApplication.update -= Update;
        
        // Generate unity textures.
        List<Texture2D> textures = new List<Texture2D>();
        for (int i = 0; i < sdfResult.Length; i++)
        {
            textures.Add(ToTexture(sdfResult[i], width, height));
        }

        // Merge
        RenderTexture blendRT = Merge(frames, textures);
        
        // Save texture
        Texture2D texture = ToTexture2D(blendRT);
        SaveTexture(ref texture, savePath);
        
        callback?.Invoke(texture);

        // Clear
        callback = null;
        width = height = 0;
        sdfResult = null;
        threads = null;
        processing = false;
    }


    private struct Pixel
    {
        public float alpha, distance;
        public Vector2 gradient;
        public int dX, dY;
    }

    // private static int width, height;
    // private static Pixel[,] pixels;

    public static void GenerateSDF(
        FaceMapData data,
        float postProcessDistance,
        ref float[] result)
    {
        int width = data.width;
        int height = data.height;
        bool[] source = data.data;
        float[] destination = new float[width * height];

        float GetSrcPixel(int x, int y)
        {
            return source[y * width + x] ? 1 : 0;
        }

        float GetDstPixel(int x, int y)
        {
            return destination[y * width + x];
        }

        void SetDstPixel(int x, int y, float value)
        {
            destination[y * width + x] = value;
        }

        float distance = Mathf.Sqrt(2) * width;

        Pixel[,] pixels = new Pixel[width, height];
        int x, y;
        float scale;

        if (distance > 0f)
        {
            for (y = 0; y < height; y++)
            {
                for (x = 0; x < width; x++)
                {
                    pixels[x, y].alpha = 1f - GetSrcPixel(x, y);
                }
            }

            ComputeEdgeGradients(width, height, pixels);
            GenerateDistanceTransform(width, height, pixels);
            if (postProcessDistance > 0f)
            {
                PostProcess(width, height, pixels, postProcessDistance);
            }

            scale = 1f / distance;
            for (y = 0; y < height; y++)
            {
                for (x = 0; x < width; x++)
                {
                    float c = Mathf.Clamp01(pixels[x, y].distance * scale);
                    SetDstPixel(x, y, c);
                }
            }
        }

        if (distance > 0f)
        {
            for (y = 0; y < height; y++)
            {
                for (x = 0; x < width; x++)
                {
                    pixels[x, y].alpha = GetSrcPixel(x, y);
                }
            }

            ComputeEdgeGradients(width, height, pixels);
            GenerateDistanceTransform(width, height, pixels);
            if (postProcessDistance > 0f)
            {
                PostProcess(width, height, pixels, postProcessDistance);
            }

            scale = 1f / distance;
            if (distance > 0f)
            {
                for (y = 0; y < height; y++)
                {
                    for (x = 0; x < width; x++)
                    {
                        float c = 0.5f + (GetDstPixel(x, y) -
                                          Mathf.Clamp01(pixels[x, y].distance * scale)) * 0.5f;
                        SetDstPixel(x, y, c);
                    }
                }
            }
            else
            {
                for (y = 0; y < height; y++)
                {
                    for (x = 0; x < width; x++)
                    {
                        float c = Mathf.Clamp01(1f - pixels[x, y].distance * scale);
                        SetDstPixel(x, y, c);
                    }
                }
            }
        }

        result = destination;
    }

    public static void GenerateSDF(
        Texture2D source,
        Texture2D destination,
        float maxInside,
        float maxOutside,
        float postProcessDistance,
        bool useAlpha = false)
    {
        if (source.height != destination.height || source.width != destination.width)
        {
            Debug.LogError("Source and destination textures must be the same size.");
            return;
        }

        int width = source.width;
        int height = source.height;
        Pixel[,] pixels = new Pixel[width, height];
        int x, y;
        float scale;
        Color c = Color.black;

        if (maxInside > 0f)
        {
            for (y = 0; y < height; y++)
            {
                for (x = 0; x < width; x++)
                {
                    pixels[x, y].alpha = 1f - (useAlpha ? source.GetPixel(x, y).a : source.GetPixel(x, y).r);
                }
            }

            ComputeEdgeGradients(width, height, pixels);
            GenerateDistanceTransform(width, height, pixels);
            if (postProcessDistance > 0f)
            {
                PostProcess(width, height, pixels, postProcessDistance);
            }

            scale = 1f / maxInside;
            for (y = 0; y < height; y++)
            {
                for (x = 0; x < width; x++)
                {
                    c.a = Mathf.Clamp01(pixels[x, y].distance * scale);
                    destination.SetPixel(x, y, c);
                }
            }
        }

        if (maxOutside > 0f)
        {
            for (y = 0; y < height; y++)
            {
                for (x = 0; x < width; x++)
                {
                    pixels[x, y].alpha = (useAlpha ? source.GetPixel(x, y).a : source.GetPixel(x, y).r);
                }
            }

            ComputeEdgeGradients(width, height, pixels);
            GenerateDistanceTransform(width, height, pixels);
            if (postProcessDistance > 0f)
            {
                PostProcess(width, height, pixels, postProcessDistance);
            }

            scale = 1f / maxOutside;
            if (maxInside > 0f)
            {
                for (y = 0; y < height; y++)
                {
                    for (x = 0; x < width; x++)
                    {
                        c.a = 0.5f + (destination.GetPixel(x, y).a -
                                      Mathf.Clamp01(pixels[x, y].distance * scale)) * 0.5f;
                        destination.SetPixel(x, y, c);
                    }
                }
            }
            else
            {
                for (y = 0; y < height; y++)
                {
                    for (x = 0; x < width; x++)
                    {
                        c.a = Mathf.Clamp01(1f - pixels[x, y].distance * scale);
                        destination.SetPixel(x, y, c);
                    }
                }
            }
        }

        for (y = 0; y < height; y++)
        {
            for (x = 0; x < width; x++)
            {
                c = destination.GetPixel(x, y);
                c.r = c.a;
                c.g = c.a;
                c.b = c.a;
                destination.SetPixel(x, y, c);
            }
        }

        pixels = null;
    }

    public static FaceMapData ExtractTexture(Texture2D texture, int coordinate = 0, float threshold = 0.5f)
    {
        int len = texture.width * texture.height;
        bool[] data = new bool[len];
        Color[] colors = texture.GetPixels();
        for (int i = 0; i < colors.Length; i++)
            data[i] = colors[i][coordinate] > threshold;
        return new FaceMapData()
        {
            data = data,
            width = texture.width,
            height = texture.height
        };
    }

    private static void ComputeEdgeGradients(int width, int height, Pixel[,] pixels)
    {
        float sqrt2 = Mathf.Sqrt(2f);
        for (int y = 1; y < height - 1; y++)
        {
            for (int x = 1; x < width - 1; x++)
            {
                ref Pixel p = ref pixels[x, y];
                if (p.alpha > 0f && p.alpha < 1f)
                {
                    // estimate gradient of edge pixel using surrounding pixels
                    float g =
                        -pixels[x - 1, y - 1].alpha
                        - pixels[x - 1, y + 1].alpha
                        + pixels[x + 1, y - 1].alpha
                        + pixels[x + 1, y + 1].alpha;
                    p.gradient.x = g + (pixels[x + 1, y].alpha - pixels[x - 1, y].alpha) * sqrt2;
                    p.gradient.y = g + (pixels[x, y + 1].alpha - pixels[x, y - 1].alpha) * sqrt2;
                    p.gradient.Normalize();
                }
            }
        }
    }

    private static void GenerateDistanceTransform(int width, int height, Pixel[,] pixels)
    {
        // perform anti-aliased Euclidean distance transform
        int x, y;
        // initialize distances
        for (y = 0; y < height; y++)
        {
            for (x = 0; x < width; x++)
            {
                ref Pixel p = ref pixels[x, y];
                p.dX = 0;
                p.dY = 0;
                if (p.alpha <= 0f)
                {
                    // outside
                    p.distance = 1000000f;
                }
                else if (p.alpha < 1f)
                {
                    // on the edge
                    p.distance = ApproximateEdgeDelta(p.gradient.x, p.gradient.y, p.alpha);
                }
                else
                {
                    // inside
                    p.distance = 0f;
                }
            }
        }

        // perform 8SSED (eight-points signed sequential Euclidean distance transform)
        // scan up
        for (y = 1; y < height; y++)
        {
            // |P.
            // |XX
            ref Pixel p = ref pixels[0, y];
            if (p.distance > 0f)
            {
                UpdateDistance(pixels, ref p, 0, y, 0, -1);
                UpdateDistance(pixels, ref p, 0, y, 1, -1);
            }

            // -->
            // XP.
            // XXX
            for (x = 1; x < width - 1; x++)
            {
                p = ref pixels[x, y];
                if (p.distance > 0f)
                {
                    UpdateDistance(pixels, ref p, x, y, -1, 0);
                    UpdateDistance(pixels, ref p, x, y, -1, -1);
                    UpdateDistance(pixels, ref p, x, y, 0, -1);
                    UpdateDistance(pixels, ref p, x, y, 1, -1);
                }
            }

            // XP|
            // XX|
            p = ref pixels[width - 1, y];
            if (p.distance > 0f)
            {
                UpdateDistance(pixels, ref p, width - 1, y, -1, 0);
                UpdateDistance(pixels, ref p, width - 1, y, -1, -1);
                UpdateDistance(pixels, ref p, width - 1, y, 0, -1);
            }

            // <--
            // .PX
            for (x = width - 2; x >= 0; x--)
            {
                p = ref pixels[x, y];
                if (p.distance > 0f)
                {
                    UpdateDistance(pixels, ref p, x, y, 1, 0);
                }
            }
        }

        // scan down
        for (y = height - 2; y >= 0; y--)
        {
            // XX|
            // .P|
            ref Pixel p = ref pixels[width - 1, y];
            if (p.distance > 0f)
            {
                UpdateDistance(pixels, ref p, width - 1, y, 0, 1);
                UpdateDistance(pixels, ref p, width - 1, y, -1, 1);
            }

            // <--
            // XXX
            // .PX
            for (x = width - 2; x > 0; x--)
            {
                p = ref pixels[x, y];
                if (p.distance > 0f)
                {
                    UpdateDistance(pixels, ref p, x, y, 1, 0);
                    UpdateDistance(pixels, ref p, x, y, 1, 1);
                    UpdateDistance(pixels, ref p, x, y, 0, 1);
                    UpdateDistance(pixels, ref p, x, y, -1, 1);
                }
            }

            // |XX
            // |PX
            p = ref pixels[0, y];
            if (p.distance > 0f)
            {
                UpdateDistance(pixels, ref p, 0, y, 1, 0);
                UpdateDistance(pixels, ref p, 0, y, 1, 1);
                UpdateDistance(pixels, ref p, 0, y, 0, 1);
            }

            // -->
            // XP.
            for (x = 1; x < width; x++)
            {
                p = ref pixels[x, y];
                if (p.distance > 0f)
                {
                    UpdateDistance(pixels, ref p, x, y, -1, 0);
                }
            }
        }
    }


    private static void UpdateDistance(Pixel[,] pixels, ref Pixel p, int x, int y, int oX, int oY)
    {
        ref Pixel neighbor = ref pixels[x + oX, y + oY];
        ref Pixel closest = ref pixels[x + oX - neighbor.dX, y + oY - neighbor.dY];

        bool equal = closest.dX == p.dX
                     && closest.dY == p.dY
                     && Math.Abs(closest.alpha - p.alpha) < 1e-6
                     && closest.gradient == p.gradient
                     && Math.Abs(closest.distance - p.distance) < 1e-6;

        if (closest.alpha == 0f || equal)
        {
            // neighbor has no closest yet
            // or neighbor's closest is p itself
            return;
        }

        int dX = neighbor.dX - oX;
        int dY = neighbor.dY - oY;
        float distance = Mathf.Sqrt(dX * dX + dY * dY) + ApproximateEdgeDelta(dX, dY, closest.alpha);
        if (distance < p.distance)
        {
            p.distance = distance;
            p.dX = dX;
            p.dY = dY;
        }
    }

    private static float ApproximateEdgeDelta(float gx, float gy, float a)
    {
        // (gx, gy) can be either the local pixel gradient or the direction to the pixel

        if (gx == 0f || gy == 0f)
        {
            // linear function is correct if both gx and gy are zero
            // and still fair if only one of them is zero
            return 0.5f - a;
        }

        // normalize (gx, gy)
        float length = Mathf.Sqrt(gx * gx + gy * gy);
        gx = gx / length;
        gy = gy / length;

        // reduce symmetrical equation to first octant only
        // gx >= 0, gy >= 0, gx >= gy
        gx = Mathf.Abs(gx);
        gy = Mathf.Abs(gy);
        if (gx < gy)
        {
            float temp = gx;
            gx = gy;
            gy = temp;
        }

        // compute delta
        float a1 = 0.5f * gy / gx;
        if (a < a1)
        {
            // 0 <= a < a1
            return 0.5f * (gx + gy) - Mathf.Sqrt(2f * gx * gy * a);
        }

        if (a < (1f - a1))
        {
            // a1 <= a <= 1 - a1
            return (0.5f - a) * gx;
        }

        // 1-a1 < a <= 1
        return -0.5f * (gx + gy) + Mathf.Sqrt(2f * gx * gy * (1f - a));
    }

    private static void PostProcess(int width, int height, Pixel[,] pixels, float maxDistance)
    {
        // adjust distances near edges based on the local edge gradient
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                ref Pixel p = ref pixels[x, y];
                if ((p.dX == 0 && p.dY == 0) || p.distance >= maxDistance)
                {
                    // ignore edge, inside, and beyond max distance
                    continue;
                }

                float
                    dX = p.dX,
                    dY = p.dY;
                ref Pixel closest = ref pixels[x - p.dX, y - p.dY];
                Vector2 g = closest.gradient;

                if (g.x == 0f && g.y == 0f)
                {
                    // ignore unknown gradients (inside)
                    continue;
                }

                // compute hit point offset on gradient inside pixel
                float df = ApproximateEdgeDelta(g.x, g.y, closest.alpha);
                float t = dY * g.x - dX * g.y;
                float u = -df * g.x + t * g.y;
                float v = -df * g.y - t * g.x;

                // use hit point to compute distance
                if (Mathf.Abs(u) <= 0.5f && Mathf.Abs(v) <= 0.5f)
                {
                    p.distance = Mathf.Sqrt((dX + u) * (dX + u) + (dY + v) * (dY + v));
                }
            }
        }
    }

    public static Texture2D ToTexture(float[] sdf, int width, int height)
    {
        Texture2D texture = new Texture2D(width, height, TextureFormat.RGBAFloat, false);
        texture.hideFlags = HideFlags.HideAndDontSave;
        Color[] colors = new Color[width * height];
        for (int i = 0; i < sdf.Length; i++)
            colors[i] = new Color(sdf[i], sdf[i], sdf[i], sdf[i]);
        texture.SetPixels(colors);
        texture.Apply();
        return texture;
    }

    #endregion

    #region Bake

    public static RenderTexture Merge(IList<FaceMapFrame> frames, IList<Texture2D> sdfs)
    {
        int count = frames.Count;
        Texture2D firstSDF = sdfs[0];
        int width = firstSDF.width;
        int height = firstSDF.height;
        TextureFormat format = firstSDF.format;

        // Prepare params.
        float[] angles = new float[count];
        for (int i = 0; i < count; i++)
            angles[i] = frames[i].angle / 180.0f;
        Texture2DArray texArray = new Texture2DArray(width, height, count, format, false, false);
        for (int i = 0; i < count; i++)
            Graphics.CopyTexture(sdfs[i], 0, 0, texArray, i, 0);

        Material material = new Material(Shader.Find("Hidden/SDF_Bake"));
        material.SetFloatArray("_Angles", angles); //[1,10,30,,60,90....]/180
        material.SetInt("_FrameCount", count);
        material.SetTexture("_Frames", texArray);
        material.SetTexture("_MainTex", firstSDF);

        RenderTexture resultRT = RenderTexture.GetTemporary(width, height, 0, RenderTextureFormat.ARGBFloat,
            RenderTextureReadWrite.Linear);
        Graphics.Blit(firstSDF, resultRT, material, 0);
        return resultRT;
    }

    #endregion

    public static Texture2D ToTexture2D(RenderTexture renderTexture)
    {
        RenderTexture active = RenderTexture.active;
        RenderTexture.active = renderTexture;
        Texture2D texture = new Texture2D(renderTexture.width, renderTexture.height, TextureFormat.ARGB32, true);
        texture.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0, true);
        texture.Apply();
        RenderTexture.active = active;
        return texture;
    }

    public static void SaveTexture(ref Texture2D texture, string path)
    {
        byte[] content = texture.EncodeToTGA();
        Object.DestroyImmediate(texture);
        path = AssetDatabase.GenerateUniqueAssetPath(path) + ".tga";
        File.WriteAllBytes(path, content);
        AssetDatabase.ImportAsset(path);
        texture = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
    }
}