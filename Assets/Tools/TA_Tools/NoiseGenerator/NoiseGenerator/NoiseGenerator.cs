using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

public class NoiseGenerator : EditorWindow
{
    public static class Styles
    {
        public static readonly GUIContent Size = new GUIContent("噪声图大小", "生成的噪声图的长宽高");
        public static readonly GUIContent R = new GUIContent("R通道噪声密度", "生成的噪声图的噪声密度");
        public static readonly GUIContent G = new GUIContent("G通道噪声密度", "生成的噪声图的噪声密度");
        public static readonly GUIContent B = new GUIContent("B通道噪声密度", "生成的噪声图的噪声密度");
        public static readonly GUIContent A = new GUIContent("A通道噪声密度", "生成的噪声图的噪声密度");
        public static readonly GUIContent MapType = new GUIContent("噪声图类型", "2D噪声还是3D噪声");
        public static readonly GUIContent NoiseType = new GUIContent("噪声类型", "生成的噪声图所使用的噪声类型");
        public static readonly GUIContent RW = new GUIContent("R通道权重", "R通道的Perlin和Worley噪声权重");
        public static readonly GUIContent GW = new GUIContent("G通道权重", "G通道的Perlin和Worley噪声权重");
        public static readonly GUIContent BW = new GUIContent("B通道权重", "B通道的Perlin和Worley噪声权重");
        public static readonly GUIContent AW = new GUIContent("A通道权重", "A通道的Perlin和Worley噪声权重");
        public static readonly GUIContent RWValue = new GUIContent("R通道权重值", "R通道的Perlin和Worley噪声权重值");
        public static readonly GUIContent GWValue = new GUIContent("G通道权重值", "G通道的Perlin和Worley噪声权重值");
        public static readonly GUIContent BWValue = new GUIContent("B通道权重值", "B通道的Perlin和Worley噪声权重值");
        public static readonly GUIContent AWValue = new GUIContent("A通道权重值", "A通道的Perlin和Worley噪声权重值");
    }
    [MenuItem("ArtTools/Noise Generator")]
    public static void ShowWindow()
    {
        EditorWindow editorWindow = EditorWindow.GetWindow(typeof(NoiseGenerator));
        editorWindow.autoRepaintOnSceneChange = false;
    }
    
    public enum NoiseMapType
    {
        Texture2D = 1,
        Texture3D = 2,
    }

    public enum NoiseMethodType
    {
        Value = 1,
        Perlin_Worley = 2,
        //SimplexValue = 3,
    }

    public enum PWWeight
    {
        PerlinOnly = 1,
        WorleyOnly = 2,
        PerlinMain = 3,
        WorleyMain = 4,
    }

    public struct Perlin_Worley_Vec4
    {
        public PWWeight R;
        public PWWeight G;
        public PWWeight B;
        public PWWeight A;

        public Single RValue;
        public Single GValue;
        public Single BValue;
        public Single AValue;
    }

    private ComputeShader cs;
    private ComputeShader cs2;
    private NoiseBase noise;
    private Int32 size = 64;
    private Vector4 scale = new Vector4(10, 20, 30, 40);
    private NoiseMapType mapType = NoiseMapType.Texture2D;
    private NoiseMethodType noiseType = NoiseMethodType.Perlin_Worley;
    private Perlin_Worley_Vec4 vec = new Perlin_Worley_Vec4();

    private void OnGUI()
    {
        size = EditorGUILayout.IntField(Styles.Size, size);
        //scale = EditorGUILayout.Vector4Field("RGB noise Scale", scale);
        scale.x = EditorGUILayout.Slider(Styles.R, scale.x, 1, size, new GUILayoutOption[0]);
        scale.y = EditorGUILayout.Slider(Styles.G, scale.y, 1, size, new GUILayoutOption[0]);
        scale.z = EditorGUILayout.Slider(Styles.B, scale.z, 1, size, new GUILayoutOption[0]);
        scale.w = EditorGUILayout.Slider(Styles.A, scale.w, 1, size, new GUILayoutOption[0]);
        mapType = (NoiseMapType)EditorGUILayout.EnumPopup(Styles.MapType, mapType);
        noiseType = (NoiseMethodType)EditorGUILayout.EnumPopup(Styles.NoiseType, noiseType);

        if (noiseType == NoiseMethodType.Perlin_Worley)
        {
            EditorGUI.indentLevel++;
            vec.R = (PWWeight)EditorGUILayout.EnumPopup(Styles.RW, vec.R);
            if(vec.R == PWWeight.PerlinMain || vec.R == PWWeight.WorleyMain)
            {
                EditorGUI.indentLevel++;
                vec.RValue = EditorGUILayout.Slider(Styles.RWValue, vec.RValue, 0, 1, new GUILayoutOption[0]);
                EditorGUI.indentLevel--;
            }

            vec.G = (PWWeight)EditorGUILayout.EnumPopup(Styles.GW, vec.G);
            if (vec.G == PWWeight.PerlinMain || vec.G == PWWeight.WorleyMain)
            {
                EditorGUI.indentLevel++;
                vec.GValue = EditorGUILayout.Slider(Styles.GWValue, vec.GValue, 0, 1, new GUILayoutOption[0]);
                EditorGUI.indentLevel--;
            }

            vec.B = (PWWeight)EditorGUILayout.EnumPopup(Styles.BW, vec.B);
            if (vec.B == PWWeight.PerlinMain || vec.B == PWWeight.WorleyMain)
            {
                EditorGUI.indentLevel++;
                vec.BValue = EditorGUILayout.Slider(Styles.BWValue, vec.BValue, 0, 1, new GUILayoutOption[0]);
                EditorGUI.indentLevel--;
            }

            vec.A = (PWWeight)EditorGUILayout.EnumPopup(Styles.AW, vec.A);
            if (vec.A == PWWeight.PerlinMain || vec.A == PWWeight.WorleyMain)
            {
                EditorGUI.indentLevel++;
                vec.AValue = EditorGUILayout.Slider(Styles.AWValue, vec.AValue, 0, 1, new GUILayoutOption[0]);
                EditorGUI.indentLevel--;
            }
            EditorGUI.indentLevel--;
        }

        if (GUILayout.Button("generate"))
        {
            GenerateAndSaveNoise();

            AssetDatabase.Refresh();
        }
    }

    private void GenerateAndSaveNoise()
    {
        if (noiseType == NoiseMethodType.Value)
        {
            noise = new ValueNoise();
            Texture t = noise.Generate(size, scale, mapType, TextureFormat.RGBA32, false);
            if (mapType == NoiseMapType.Texture2D)
            {
                Texture2D t2d = t as Texture2D;
                Byte[] bytes = t2d.EncodeToTGA();
                if (!Directory.Exists("Assets/Noise"))
                {
                    Directory.CreateDirectory("Assets/Noise");
                }
                File.WriteAllBytes("Assets/Noise/ValueNoise2D.tga", bytes);
            }
            else if (mapType == NoiseMapType.Texture3D)
            {
                Texture3D t3d = t as Texture3D;
                if (!Directory.Exists("Assets/Noise"))
                {
                    Directory.CreateDirectory("Assets/Noise");
                }
                AssetDatabase.CreateAsset(t3d, "Assets/Noise/ValueNoise3D.asset");
            }
        }
        else if (noiseType == NoiseMethodType.Perlin_Worley)
        {
            noise = new PerlinNoise();
            cs = AssetDatabase.LoadAssetAtPath<ComputeShader>("Assets/Tools/TA_Tools/NoiseGenerator/ResourcesCF/PerlinNoise.compute"); //Resources.Load("PerlinNoise") as ComputeShader;
            cs2 = AssetDatabase.LoadAssetAtPath<ComputeShader>("Assets/Tools/TA_Tools/NoiseGenerator/ResourcesCF/Slice3DRT.compute"); //Resources.Load("Slice3DRT") as ComputeShader;

            noise.SetComputeShader(cs, cs2);
            ((PerlinNoise)noise).SetChannelMode(vec);
            Texture t = noise.Generate(size, scale, mapType, TextureFormat.RGBA32, false);
            if (mapType == NoiseMapType.Texture2D)
            {
                Texture2D t2d = t as Texture2D;
                Byte[] bytes = t2d.EncodeToTGA();
                if (!Directory.Exists("Assets/Noise"))
                {
                    Directory.CreateDirectory("Assets/Noise");
                }
                File.WriteAllBytes("Assets/Noise/PerlinNoise2D.tga", bytes);
            }
            else if (mapType == NoiseMapType.Texture3D)
            {
                Texture3D t3d = t as Texture3D;
                if (!Directory.Exists("Assets/Noise"))
                {
                    Directory.CreateDirectory("Assets/Noise");
                }
                AssetDatabase.CreateAsset(t3d, "Assets/Noise/PerlinNoise3D.asset");
            }
        }
    }
}
