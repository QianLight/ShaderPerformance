using UnityEngine;
using UnityEditor;
using UnityEngine.Rendering;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using System.IO;
namespace CFEngine.Editor
{
    internal class TerrainAssets
    {

        static Color[] blendColors;
        

        [MenuItem("Assets/Tool/Editor/Terrain_BlendTex")]
        static void CreateBlendText()
        {
            CommonAssets.enumTex2D.cb = (tex, import, path, context) =>
            {
                Texture2D blendTex = new Texture2D(512, 512, TextureFormat.RGBA32, true, true);
                blendTex.name = tex.name;
                if (blendColors == null)
                {
                    blendColors = new Color[512 * 512];
                    for (int i = 0; i < blendColors.Length; ++i)
                    {
                        blendColors[i] = Color.clear;
                    }
                }
                blendTex.SetPixels(blendColors);
                string dir = Path.GetDirectoryName(path);
                blendTex = CommonAssets.CreateAsset<Texture2D>(dir, blendTex.name, ".png", blendTex);
                return true;
            };
            CommonAssets.EnumAsset<Texture2D>(CommonAssets.enumTex2D, "CreateBlendText");


        }
        static TextGenerationSettings GetGenerationSettings(Vector2 extents, Font font)
        {
            var settings = new TextGenerationSettings();

            settings.generationExtents = extents;
            if (font != null && font.dynamic)
            {
                settings.fontSize = 24;
                settings.resizeTextMinSize = 10;
                settings.resizeTextMaxSize = 40;
            }

            // Other settings
            settings.textAnchor = TextAnchor.UpperLeft;
            settings.alignByGeometry = false;
            settings.scaleFactor = 1;
            settings.color = Color.white;
            settings.font = font;
            settings.pivot = Vector2.zero;
            settings.richText = true;
            settings.lineSpacing = 1.0f;
            settings.fontStyle = FontStyle.Bold;
            settings.resizeTextForBestFit = false;
            settings.updateBounds = false;
            settings.horizontalOverflow = HorizontalWrapMode.Wrap;
            settings.verticalOverflow = VerticalWrapMode.Truncate;

            return settings;
        }
        static Mesh DrawBlock(Material mat, int num, TextGenerator textCache, Font font, Rect viewPort, CommandBuffer cb)
        {
            Vector2 extents = new Vector2(160, 30);

            var settings = GetGenerationSettings(extents, font);

            textCache.Populate(num.ToString(), settings);

            IList<UIVertex> verts = textCache.verts;
            int vertCount = verts.Count - 4;

            Mesh mesh = new Mesh { name = "Text Triangle" };
            mesh.MarkDynamic();

            List<Vector3> pos = new List<Vector3>();
            List<int> indexes = new List<int>();
            List<Vector2> uv = new List<Vector2>();
            int startIndex = 0;
            float border = vertCount == 2 ? 0.01f : 0.05f;
            float textWidth = (0.2f - border * 2) / (vertCount / 4);
            float startX = viewPort.xMin + border;
            for (int i = 0; i < vertCount; i += 4)
            {
                pos.Add(new Vector3(startX, viewPort.yMin + border, 0));
                pos.Add(new Vector3(startX + textWidth, viewPort.yMin + border, 0));
                pos.Add(new Vector3(startX + textWidth, viewPort.yMax - border, 0));
                pos.Add(new Vector3(startX, viewPort.yMax - border, 0));

                indexes.Add(startIndex);
                indexes.Add(startIndex + 1);
                indexes.Add(startIndex + 2);

                indexes.Add(startIndex + 2);
                indexes.Add(startIndex + 3);
                indexes.Add(startIndex);
                startIndex += 4;

                uv.Add(verts[i].uv0);
                uv.Add(verts[i + 1].uv0);
                uv.Add(verts[i + 2].uv0);
                uv.Add(verts[i + 3].uv0);


                startX += textWidth;
            }

            mesh.SetVertices(pos);
            mesh.SetIndices(indexes.ToArray(), MeshTopology.Triangles, 0, false);
            mesh.SetUVs(0, uv);

            cb.DrawMesh(mesh, Matrix4x4.identity, mat, 0, 2);

            return mesh;
        }

        [MenuItem("Assets/Tool/Editor/Terrain_GridTex")]
        static void CreateGridText()
        {
            CommandBuffer cb = new CommandBuffer() { name = "Temp CB" };
            var rt = new RenderTexture(512, 512, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear);
            rt.Create();
            cb.SetRenderTarget(rt);
            TextGenerator textCache = new TextGenerator(2);
            Font font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            Rect viewPort = new Rect();
            float size = 0.2f;
            Material mat = new Material(AssetsConfig.instance.TextureBake);
            List<Mesh> meshs = new List<Mesh>();
            mat.mainTexture = font.material.mainTexture;
            for (int y = 0; y < 10; ++y)
            {
                for (int x = 0; x < 10; ++x)
                {
                    int i = 10 * y + x;
                    viewPort.xMin = -1.0f + (i % 10) * size;
                    viewPort.xMax = viewPort.xMin + size;

                    //viewPort.yMin = -1.0f + (i / 10) * size;
                    //viewPort.yMax = viewPort.yMin + size;

                    viewPort.yMax = 1.0f - (i / 10) * size;
                    viewPort.yMin = viewPort.yMax - size;
                    meshs.Add(DrawBlock(mat, i, textCache, font, viewPort, cb));
                }
            }
            Graphics.ExecuteCommandBuffer(cb);
            CommonAssets.CreateAsset<Texture2D>("Assets/Rendering/Shaders/Scene/Editor", "GridTex", ".png", rt);
            cb.Release();
            rt.Release();
            Object.DestroyImmediate(mat);
            for (int i = 0; i < meshs.Count; ++i)
            {
                Object.DestroyImmediate(meshs[i]);
            }
        }
    }
}