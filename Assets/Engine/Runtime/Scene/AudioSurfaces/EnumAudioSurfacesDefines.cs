using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CFEngine
{
    public enum EnumAudioSurfacesDefines
    {
        Default = 0,
        grass = 1, //草地
        gravel = 2, //砂石
        metal = 3, //金属
        wood = 4, //木头
        water = 5, //水面
        soil = 6, //土壤
        sandbeach = 7, //沙滩
        rock = 8, //石头
        carpet=9,
        cloud=10,
        ice=11,
        lava=12,
        snow=13
    }

    public class AudioSurfacesDefines
    {
        public static byte GetSurfacesFlag(AudioSurfaces type)
        {
            byte flag = GetByteByType(type.sufaceType);

            if (!flagColorDic.ContainsKey(flag))
                flagColorDic.Add(flag, type.debugColor);

            return flag;
        }

        public static byte GetByteByType(EnumAudioSurfacesDefines sufaceType)
        {
            return (byte) sufaceType;
        }

        public static void ClearFlagColorDic()
        {
            flagColorDic.Clear();
        }

        private static Dictionary<byte, Color> flagColorDic = new Dictionary<byte, Color>();

        public static void CheckFlagDebugColor(byte flag, ref Color color)
        {
            if (flagColorDic.ContainsKey(flag))
            {
                color = flagColorDic[flag];
                color.a = 1;
            }

        }


        public static Color GetColorByBarycentric(int nTriangleIndex, Vector3 hitLocalPos, Mesh oriMesh, Mesh colorMesh)
        {
            int[] OriTriangles = oriMesh.GetTriangles(0);
            int nIndex = OriTriangles[nTriangleIndex * 3];
            int nIndex1 = OriTriangles[nTriangleIndex * 3 + 1];
            int nIndex2 = OriTriangles[nTriangleIndex * 3 + 2];

            Vector3[] vertices = oriMesh.vertices;
            Vector3 vertice0 = vertices[nIndex];
            Vector3 vertice1 = vertices[nIndex1];
            Vector3 vertice2 = vertices[nIndex2];


            Color[] colors = colorMesh.colors;
            Color color0 = colors[nIndex];
            Color color1 = colors[nIndex1];
            Color color2 = colors[nIndex2];


            Vector3 CurBary = Barycentric(hitLocalPos, vertice0, vertice1, vertice2);

            // float h, v, s;
            // Color.RGBToHSV(color0, out h, out v, out s);
            // Color c0 = Color.HSVToRGB(h, CurBary.x / (1f - CurBary.y), 1f - CurBary.y);
            //
            // Color.RGBToHSV(color1, out h, out v, out s);
            // Color c1 = Color.HSVToRGB(h, CurBary.x / (1f - CurBary.y), 1f - CurBary.y);
            //
            // Color.RGBToHSV(color2, out h, out v, out s);
            // Color c2 = Color.HSVToRGB(h, CurBary.x / (1f - CurBary.y), 1f - CurBary.y);

            //Color final = (c0 + c1 + c2) / 3;
            Color final = color0 * CurBary.x + color1 * CurBary.y + color2 * CurBary.z;
            
            return final;
        }

        private static Vector3  Barycentric(Vector3 p, Vector3 a, Vector3 b, Vector3 c)
        {
            Vector3 bary = Vector3.zero;
            Vector3 v0 = b - a;
            Vector3 v1 = c - a;
            Vector3 v2 = p - a;
            float d00 = Vector3.Dot(v0, v0);
            float d01 = Vector3.Dot(v0, v1);
            float d11 = Vector3.Dot(v1, v1);
            float d20 = Vector3.Dot(v2, v0);
            float d21 = Vector3.Dot(v2, v1);
            float denom = d00 * d11 - d01 * d01;
            bary.y = (d11 * d20 - d01 * d21) / denom;
            bary.z = (d00 * d21 - d01 * d20) / denom;
            bary.x = 1.0f - bary.y - bary.z;
            return bary;
        }

        
        
    }


}
