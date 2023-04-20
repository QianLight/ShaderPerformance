using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
namespace AssetCheck
{
    [CheckRuleDescription("Texture", "纯色纹理", "t:Texture2D", "")]
    public class TextureSolidCheck : RuleBase
    {
        [PublicMethod]
        public bool Check(string path, out string output)
        {
            output = string.Empty;
            Texture tex = AssetDatabase.LoadAssetAtPath<Texture>(path);
            if (tex == null)
                return true;
            bool isSolidColor = _CheckSolidColor(tex);
            if(isSolidColor)
            {
                output = "纯色纹理";
            }
            return !isSolidColor;
        }

        bool _CheckSolidColor(Texture texture)
        {
            //方法2：mipmap+泊松   blit到一张小图上
            Texture currentTexture = texture;
            int currentTextureWidth = currentTexture.width;
            int currentTextureHeight = currentTexture.height;
            int rtWidth = currentTextureWidth;
            int rtHeight = currentTextureHeight;
            RenderTexture _rt = null;

            if (currentTextureWidth >= 128)
            {
                rtWidth = currentTextureWidth / 8;
            }
            if (currentTextureHeight >= 128)
            {
                rtHeight = currentTextureHeight / 8;
            }

            if (_rt != null)
            {
                RenderTexture.ReleaseTemporary(_rt = null);
            }

            if (_rt == null)
            {
                _rt = RenderTexture.GetTemporary(rtWidth, rtHeight, 0, RenderTextureFormat.Default, RenderTextureReadWrite.Default);
                Graphics.Blit(currentTexture, _rt);
            }
            //进行泊松分布采样判断
            return IsSolidColor(_rt, rtWidth, rtHeight);
        }

        bool IsSolidColor(RenderTexture rt, int rtWidth, int rtHeight)
        {
            int samplePointCount = rtWidth * rtHeight;
            int sampleCountLimit = samplePointCount;   //最大可采样数
            int iterLimit = 100; // 迭代上限, 100次内决定有效位置
            Vector2 sample;
            int count = 0;
            //float extend = 1;
            //bool avaliable;

            // RT像素画到一张小图上可以后面读取像素
            RenderTexture.active = rt;
            Texture2D texture = new Texture2D(rt.width, rt.height, TextureFormat.RGBA32, false);

            texture.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0);
            texture.Apply();

            //把像素点读取存储到数组中
            List<Vector2> samplePoints = new List<Vector2>();
            for (int i = 0; i < rtWidth; i++)
            {
                for (int j = 0; j < rtHeight; j++)
                {
                    samplePoints.Add(new Vector2(i, j));
                }
            }


            Color initColor = new Color(0, 0, 0);

            while (samplePoints.Count > 0 && sampleCountLimit > 0 && iterLimit-- > 0)
            {
                int next = (int)Mathf.Lerp(0, samplePoints.Count - 1, UnityEngine.Random.value);
                sample = samplePoints[next]; // 在这些点中随便选一个采样点进行范围随机

                bool found = false;
                int kloop = 30; // 迭代30次, 找到泊松分布点
                float radius = 1;   //采样半径为1像素
                float radius2 = radius * radius;

                for (int j = 0; j < kloop; j++)
                {
                    var angle = 2 * Mathf.PI * UnityEngine.Random.value;
                    float r = Mathf.Sqrt(UnityEngine.Random.value * 3 * radius2 + radius2);
                    //得到临近分布点
                    Vector2 candidate = sample + r * new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));

                    if (candidate.x <= rtWidth && candidate.y <= rtHeight && candidate.x >= 0 && candidate.y >= 0)
                    {
                        found = true;

                        samplePoints.Add(candidate);
                        radius2 = radius * radius;
                        count++;
                        //找到采样点 进行颜色采样
                        Color candidateColor = texture.GetPixelBilinear(candidate.x, candidate.y);
                        if (sampleCountLimit == samplePointCount)
                        {
                            initColor = candidateColor;
                        }
                        else
                        {
                            initColor += candidateColor;
                            if (candidateColor != initColor / count)
                            {
                                //如果颜色有差异，说明不是纯色纹理，直接返回false
                                return false;
                            }
                        }
                        samplePointCount--;
                        break;
                    }
                }

                if (!found)
                {
                    // 如果这个点找不到周围可用点则移出采样点列表
                    samplePoints[next] = samplePoints[samplePoints.Count - 1];
                    samplePoints.RemoveAt(samplePoints.Count - 1);
                }
            }
            return true;

        }
    }
}

