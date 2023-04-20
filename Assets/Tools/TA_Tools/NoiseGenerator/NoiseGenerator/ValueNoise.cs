using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ValueNoise : NoiseBase
{
    private Texture texture;

    public override Texture Generate(Int32 size, Vector4 scale, NoiseGenerator.NoiseMapType type, TextureFormat textureFormat, bool mipChain)
    {
        if(type == NoiseGenerator.NoiseMapType.Texture2D)
        {
            texture = new Texture2D(size, size, textureFormat, mipChain);
            Single nodeWidthR = (Single)size / scale.x;
            Single nodeWidthG = (Single)size / scale.y;
            Single nodeWidthB = (Single)size / scale.z;
            Single nodeWidthA = (Single)size / scale.w;

            Int32 nodeCountR = Mathf.FloorToInt(scale.x) + 1;
            Int32 nodeCountG = Mathf.FloorToInt(scale.y) + 1;
            Int32 nodeCountB = Mathf.FloorToInt(scale.z) + 1;
            Int32 nodeCountA = Mathf.FloorToInt(scale.w) + 1;
            Single[][] nodeValueR = new Single[nodeCountR][];
            Single[][] nodeValueG = new Single[nodeCountG][];
            Single[][] nodeValueB = new Single[nodeCountB][];
            Single[][] nodeValueA = new Single[nodeCountA][];

            for(Int32 i = 0; i < nodeCountR; i++)
            {
                nodeValueR[i] = new Single[nodeCountR];
                for(Int32 j = 0; j < nodeCountR; j++)
                {
                    nodeValueR[i][j] = UnityEngine.Random.Range(0.0f, 1.0f);
                }
            }
            for(Int32 i = 0; i < nodeCountG; i++)
            {
                nodeValueG[i] = new Single[nodeCountG];
                for(Int32 j = 0; j < nodeCountG; j++)
                {
                    nodeValueG[i][j] = UnityEngine.Random.Range(0.0f, 1.0f);
                }
            }
            for(Int32 i = 0; i < nodeCountB; i++)
            {
                nodeValueB[i] = new Single[nodeCountB];
                for(Int32 j = 0; j < nodeCountB; j++)
                {
                    nodeValueB[i][j] = UnityEngine.Random.Range(0.0f, 1.0f);
                }
            }
            for(Int32 i = 0; i < nodeCountA; i++)
            {
                nodeValueA[i] = new Single[nodeCountA];
                for(Int32 j = 0; j < nodeCountA; j++)
                {
                    nodeValueA[i][j] = UnityEngine.Random.Range(0.0f, 1.0f);
                }
            }
            for(Int32 u = 0; u < size; u++)
            {
                Single nodeUR = (Single)u / nodeWidthR;
                Single nodeUG = (Single)u / nodeWidthG;
                Single nodeUB = (Single)u / nodeWidthB;
                Single nodeUA = (Single)u / nodeWidthA;

                Int32 urmin = Mathf.FloorToInt(nodeUR);
                Int32 urmax = urmin + 1;
                Int32 ugmin = Mathf.FloorToInt(nodeUG);
                Int32 ugmax = ugmin + 1;
                Int32 ubmin = Mathf.FloorToInt(nodeUB);
                Int32 ubmax = ubmin + 1;
                Int32 uamin = Mathf.FloorToInt(nodeUA);
                Int32 uamax = uamin + 1;
                for (Int32 v = 0; v < size; v++)
                {
                    Single nodeVR = (Single)v / nodeWidthR;
                    Single nodeVG = (Single)v / nodeWidthG;
                    Single nodeVB = (Single)v / nodeWidthB;
                    Single nodeVA = (Single)v / nodeWidthA;

                    Int32 vrmin = Mathf.FloorToInt(nodeVR);
                    Int32 vrmax = vrmin + 1;
                    Int32 vgmin = Mathf.FloorToInt(nodeVG);
                    Int32 vgmax = vgmin + 1;
                    Int32 vbmin = Mathf.FloorToInt(nodeVB);
                    Int32 vbmax = vbmin + 1;
                    Int32 vamin = Mathf.FloorToInt(nodeVA);
                    Int32 vamax = vamin + 1;

                    Single r1 = Mathf.Lerp(nodeValueR[urmin][vrmin], nodeValueR[urmax][vrmin], MagicNumber(nodeUR - urmin));
                    Single r2 = Mathf.Lerp(nodeValueR[urmin][vrmax], nodeValueR[urmax][vrmax], MagicNumber(nodeUR - urmin));
                    Single r = Mathf.Lerp(r1, r2, MagicNumber(nodeVR - vrmin));

                    Single g1 = Mathf.Lerp(nodeValueG[ugmin][vgmin], nodeValueG[ugmax][vgmin], MagicNumber(nodeUG - ugmin));
                    Single g2 = Mathf.Lerp(nodeValueG[ugmin][vgmax], nodeValueG[ugmax][vgmax], MagicNumber(nodeUG - ugmin));
                    Single g = Mathf.Lerp(g1, g2, MagicNumber(nodeVG - vgmin));

                    Single b1 = Mathf.Lerp(nodeValueB[ubmin][vbmin], nodeValueB[ubmax][vbmin], MagicNumber(nodeUB - ubmin));
                    Single b2 = Mathf.Lerp(nodeValueB[ubmin][vbmax], nodeValueB[ubmax][vbmax], MagicNumber(nodeUB - ubmin));
                    Single b = Mathf.Lerp(b1, b2, MagicNumber(nodeVB - vbmin));
                    
                    Single a1 = Mathf.Lerp(nodeValueA[uamin][vamin], nodeValueA[uamax][vamin], MagicNumber(nodeUA - uamin));
                    Single a2 = Mathf.Lerp(nodeValueA[uamin][vamax], nodeValueA[uamax][vamax], MagicNumber(nodeUA - uamin));
                    Single a = Mathf.Lerp(a1, a2, MagicNumber(nodeVA - vamin));


                    ((Texture2D)texture).SetPixel(u, v, new Color(r, g, b, a));
                }
            }
        }
        else if(type == NoiseGenerator.NoiseMapType.Texture3D)
        {
            texture = new Texture3D(size, size, size, textureFormat, mipChain);
            Single nodeWidthR = (Single)size / scale.x;
            Single nodeWidthG = (Single)size / scale.y;
            Single nodeWidthB = (Single)size / scale.z;
            Single nodeWidthA = (Single)size / scale.w;

            Int32 nodeCountR = Mathf.FloorToInt(scale.x) + 1;
            Int32 nodeCountG = Mathf.FloorToInt(scale.y) + 1;
            Int32 nodeCountB = Mathf.FloorToInt(scale.z) + 1;
            Int32 nodeCountA = Mathf.FloorToInt(scale.w) + 1;
            Single[][][] nodeValueR = new Single[nodeCountR][][];
            Single[][][] nodeValueG = new Single[nodeCountG][][];
            Single[][][] nodeValueB = new Single[nodeCountB][][];
            Single[][][] nodeValueA = new Single[nodeCountA][][];
            for (Int32 i = 0; i < nodeCountR; i++)
            {
                nodeValueR[i] = new Single[nodeCountR][];
                for (Int32 j = 0; j < nodeCountR; j++)
                {
                    nodeValueR[i][j] = new Single[nodeCountR];
                    for (Int32 k = 0; k < nodeCountR; k++)
                    {
                        nodeValueR[i][j][k] = UnityEngine.Random.Range(0.0f, 1.0f);
                    }
                }
            }
            for (Int32 i = 0; i < nodeCountG; i++)
            {
                nodeValueG[i] = new Single[nodeCountG][];
                for (Int32 j = 0; j < nodeCountG; j++)
                {
                    nodeValueG[i][j] = new Single[nodeCountG];
                    for (Int32 k = 0; k < nodeCountG; k++)
                    {
                        nodeValueG[i][j][k] = UnityEngine.Random.Range(0.0f, 1.0f);
                    }
                }
            }
            for (Int32 i = 0; i < nodeCountB; i++)
            {
                nodeValueB[i] = new Single[nodeCountB][];
                for (Int32 j = 0; j < nodeCountB; j++)
                {
                    nodeValueB[i][j] = new Single[nodeCountB];
                    for (Int32 k = 0; k < nodeCountB; k++)
                    {
                        nodeValueB[i][j][k] = UnityEngine.Random.Range(0.0f, 1.0f);
                    }
                }
            }
            for (Int32 i = 0; i < nodeCountA; i++)
            {
                nodeValueA[i] = new Single[nodeCountA][];
                for (Int32 j = 0; j < nodeCountA; j++)
                {
                    nodeValueA[i][j] = new Single[nodeCountA];
                    for (Int32 k = 0; k < nodeCountA; k++)
                    {
                        nodeValueA[i][j][k] = UnityEngine.Random.Range(0.0f, 1.0f);
                    }
                }
            }
            for (Int32 u = 0; u < size; u++)
            {
                Single nodeUR = (Single)u / nodeWidthR;
                Single nodeUG = (Single)u / nodeWidthG;
                Single nodeUB = (Single)u / nodeWidthB;
                Single nodeUA = (Single)u / nodeWidthA;

                Int32 urmin = Mathf.FloorToInt(nodeUR);
                Int32 urmax = urmin + 1;
                Int32 ugmin = Mathf.FloorToInt(nodeUG);
                Int32 ugmax = ugmin + 1;
                Int32 ubmin = Mathf.FloorToInt(nodeUB);
                Int32 ubmax = ubmin + 1;
                Int32 uamin = Mathf.FloorToInt(nodeUA);
                Int32 uamax = uamin + 1;
                for (Int32 v = 0; v < size; v++)
                {
                    Single nodeVR = (Single)v / nodeWidthR;
                    Single nodeVG = (Single)v / nodeWidthG;
                    Single nodeVB = (Single)v / nodeWidthB;
                    Single nodeVA = (Single)v / nodeWidthA;

                    Int32 vrmin = Mathf.FloorToInt(nodeVR);
                    Int32 vrmax = vrmin + 1;
                    Int32 vgmin = Mathf.FloorToInt(nodeVG);
                    Int32 vgmax = vgmin + 1;
                    Int32 vbmin = Mathf.FloorToInt(nodeVB);
                    Int32 vbmax = vbmin + 1;
                    Int32 vamin = Mathf.FloorToInt(nodeVA);
                    Int32 vamax = vamin + 1;

                    for (Int32 w = 0; w < size; w++)
                    {
                        Single nodeWR = (Single)w / nodeWidthR;
                        Single nodeWG = (Single)w / nodeWidthG;
                        Single nodeWB = (Single)w / nodeWidthB;
                        Single nodeWA = (Single)w / nodeWidthA;

                        Int32 wrmin = Mathf.FloorToInt(nodeWR);
                        Int32 wrmax = wrmin + 1;
                        Int32 wgmin = Mathf.FloorToInt(nodeWG);
                        Int32 wgmax = wgmin + 1;
                        Int32 wbmin = Mathf.FloorToInt(nodeWB);
                        Int32 wbmax = wbmin + 1;
                        Int32 wamin = Mathf.FloorToInt(nodeWA);
                        Int32 wamax = wamin + 1;

                        Single r1 = Mathf.Lerp(nodeValueR[urmin][vrmin][wrmin], nodeValueR[urmax][vrmin][wrmin], MagicNumber(nodeUR - urmin));
                        Single r2 = Mathf.Lerp(nodeValueR[urmin][vrmax][wrmin], nodeValueR[urmax][vrmax][wrmin], MagicNumber(nodeUR - urmin));
                        Single r3 = Mathf.Lerp(r1, r2, MagicNumber(nodeVR - vrmin));
                        Single r4 = Mathf.Lerp(nodeValueR[urmin][vrmin][wrmax], nodeValueR[urmax][vrmin][wrmax], MagicNumber(nodeUR - urmin));
                        Single r5 = Mathf.Lerp(nodeValueR[urmin][vrmax][wrmax], nodeValueR[urmax][vrmax][wrmax], MagicNumber(nodeUR - urmin));
                        Single r6 = Mathf.Lerp(r4, r5, MagicNumber(nodeVR - vrmin));
                        Single r = Mathf.Lerp(r3, r6, MagicNumber(nodeWR - wrmin));

                        Single g1 = Mathf.Lerp(nodeValueG[ugmin][vgmin][wgmin], nodeValueG[ugmax][vgmin][wgmin], MagicNumber(nodeUG - ugmin));
                        Single g2 = Mathf.Lerp(nodeValueG[ugmin][vgmax][wgmin], nodeValueG[ugmax][vgmax][wgmin], MagicNumber(nodeUG - ugmin));
                        Single g3 = Mathf.Lerp(g1, g2, MagicNumber(nodeVG - vgmin));
                        Single g4 = Mathf.Lerp(nodeValueG[ugmin][vgmin][wgmax], nodeValueG[ugmax][vgmin][wgmax], MagicNumber(nodeUG - ugmin));
                        Single g5 = Mathf.Lerp(nodeValueG[ugmin][vgmax][wgmax], nodeValueG[ugmax][vgmax][wgmax], MagicNumber(nodeUG - ugmin));
                        Single g6 = Mathf.Lerp(g4, g5, MagicNumber(nodeVG - vgmin));
                        Single g = Mathf.Lerp(g3, g6, MagicNumber(nodeWG - wgmin));

                        Single b1 = Mathf.Lerp(nodeValueB[ubmin][vbmin][wbmin], nodeValueB[ubmax][vbmin][wbmin], MagicNumber(nodeUB - ubmin));
                        Single b2 = Mathf.Lerp(nodeValueB[ubmin][vbmax][wbmin], nodeValueB[ubmax][vbmax][wbmin], MagicNumber(nodeUB - ubmin));
                        Single b3 = Mathf.Lerp(b1, b2, MagicNumber(nodeVB - vbmin));
                        Single b4 = Mathf.Lerp(nodeValueB[ubmin][vbmin][wbmax], nodeValueB[ubmax][vbmin][wbmax], MagicNumber(nodeUB - ubmin));
                        Single b5 = Mathf.Lerp(nodeValueB[ubmin][vbmax][wbmax], nodeValueB[ubmax][vbmax][wbmax], MagicNumber(nodeUB - ubmin));
                        Single b6 = Mathf.Lerp(b4, b5, MagicNumber(nodeVB - vbmin));
                        Single b = Mathf.Lerp(b3, b6, MagicNumber(nodeWB - wbmin));

                        Single a1 = Mathf.Lerp(nodeValueA[uamin][vamin][wamin], nodeValueA[uamax][vamin][wamin], MagicNumber(nodeUA - uamin));
                        Single a2 = Mathf.Lerp(nodeValueA[uamin][vamax][wamin], nodeValueA[uamax][vamax][wamin], MagicNumber(nodeUA - uamin));
                        Single a3 = Mathf.Lerp(a1, a2, MagicNumber(nodeVA - vamin));
                        Single a4 = Mathf.Lerp(nodeValueA[uamin][vamin][wamax], nodeValueA[uamax][vamin][wamax], MagicNumber(nodeUA - uamin));
                        Single a5 = Mathf.Lerp(nodeValueA[uamin][vamax][wamax], nodeValueA[uamax][vamax][wamax], MagicNumber(nodeUA - uamin));
                        Single a6 = Mathf.Lerp(a4, a5, MagicNumber(nodeVA - vamin));
                        Single a = Mathf.Lerp(a3, a6, MagicNumber(nodeWA - wamin));

                        ((Texture3D)texture).SetPixel(u, v, w, new Color(r, g, b, a));
                    }
                }
            }
        }

        return texture;
    }

    private Single MagicNumber(Single x)
    {
        return ((6 * x - 15) * x + 10) * x * x * x;
    }
}
