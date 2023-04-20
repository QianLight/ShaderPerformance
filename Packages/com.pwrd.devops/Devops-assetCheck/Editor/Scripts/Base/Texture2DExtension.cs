using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Texture2DExtension
{
    public const float PRECISION = 0.000001f;
    public const float ONEPRECISION = 1.0f - 0.000001f;
    public static bool IsAlphaZero(this Texture2D texture)
    {
        Texture2D readTexture = GetCanReadTexture(texture);
        Color[] colors = readTexture.GetPixels();
        foreach (var color in colors)
        {
            if (Mathf.Abs(color.a) > PRECISION)
                return false;
        }
        return true;
    }

    public static bool IsAlphaZero(this Texture2D texture, int x, int y, int blockWidth, int blockHeight)
    {
        Texture2D readTexture = GetCanReadTexture(texture);
        Color[] colors = readTexture.GetPixels(x, y, blockWidth, blockHeight);
        foreach (var color in colors)
        {
            if (Mathf.Abs(color.a) > PRECISION)
                return false;
        }
        return true;
    }

    public static bool IsAlphaOne(this Texture2D texture)
    {
        Texture2D readTexture = GetCanReadTexture(texture);
        Color[] colors = readTexture.GetPixels();
        foreach (var color in colors)
        {
            if (color.a < ONEPRECISION)
                return false;
        }
        return true;
    }
    public static bool IsSolidColor(this Texture2D texture)
    {
        Texture2D readTexture = GetCanReadTexture(texture);
        Color[] colors = readTexture.GetPixels();
        if (colors.Length == 0)
            return true;
        Color initColor = colors[0];
        foreach (var col in colors)
        {
            if (col != initColor)
                return false;
        }
        return true;
    }

    static Texture2D GetCanReadTexture(Texture2D texture)
    {
        Texture2D readTexture;
        if (texture.isReadable)
        {
            readTexture = texture;
        }
        else
        {
            RenderTexture tmp = RenderTexture.GetTemporary(texture.width, texture.height, 0, RenderTextureFormat.Default, RenderTextureReadWrite.Linear);
            Graphics.Blit(texture, tmp);
            RenderTexture previous = RenderTexture.active;
            RenderTexture.active = tmp;
            readTexture = new Texture2D(texture.width, texture.height);
            readTexture.ReadPixels(new Rect(0, 0, tmp.width, tmp.height), 0, 0);
            readTexture.Apply();
            RenderTexture.active = previous;
            RenderTexture.ReleaseTemporary(tmp);
        }
        return readTexture;
    }
}
