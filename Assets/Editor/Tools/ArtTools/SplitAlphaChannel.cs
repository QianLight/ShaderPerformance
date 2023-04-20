using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

using System.IO;



[ExecuteInEditMode]
public class SplitAlphaChannel{
    public static void DivideTexture()
    {
        //获取图片
        string path = AssetDatabase.GetAssetPath(Selection.activeInstanceID);
        Debug.Log(path);

        //设置原图可编辑（RGBA） xxx.png
        TextureImporter importer = TextureImporter.GetAtPath(path) as TextureImporter;
        importer.isReadable = true;
        importer.SaveAndReimport();

        //读取原图（RGBA）
        Texture2D source = AssetDatabase.LoadAssetAtPath<Texture>(path) as Texture2D;
        float sizeScale = 1;
        Texture2D rgbTex = new Texture2D(source.width, source.height, TextureFormat.RGB24, true);
        Texture2D alphaTex = new Texture2D((int)(source.width * sizeScale), (int)(source.height * sizeScale), TextureFormat.RGB24, true);
        Color[] rgbColors = new Color[source.width * source.height];
        Color[] alphaColors = new Color[source.width * source.height];

        for (int i = 0; i < source.width; ++i)
        {
            for (int j = 0; j < source.height; ++j)
            {
                Color color = source.GetPixel(i, j);
                Color rgbColor = color;
                if (color.a == 0.0f)
                {
                    rgbColor.r = 0;
                    rgbColor.g = 0;
                    rgbColor.b = 0;
                }
                rgbColor.a = 1;
                rgbColors[source.width * j + i] = color;

                Color alphaColor = color;
                alphaColor.r = color.a;
                alphaColor.g = color.a;
                alphaColor.b = color.a;
                alphaColor.a = 1;
                alphaColors[source.width * j + i] = alphaColor;
            }
        }
        rgbTex.SetPixels(rgbColors);
        alphaTex.SetPixels(alphaColors);
        rgbTex.Apply();
        alphaTex.Apply();

        //生成分离图片RGB + Alpha
        byte[] bytes = rgbTex.EncodeToPNG();
        File.WriteAllBytes(path.Replace(source.name + ".png", source.name + "_RGB.png"), bytes);
        bytes = alphaTex.EncodeToPNG();
        File.WriteAllBytes(path.Replace(source.name + ".png", source.name + "_A.png"), bytes);
        AssetDatabase.ImportAsset(path.Replace(source.name + ".png", source.name + "_RGB.png"));
        AssetDatabase.ImportAsset(path.Replace(source.name + ".png", source.name + "_A.png"));
        AssetDatabase.Refresh();


        AssetDatabase.Refresh();
        Debug.Log("finish");
    }

}
