using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(FaceMapGenerator))]
public class FaceMapGeneratorEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        FaceMapGenerator generator = target as FaceMapGenerator;
        if (!generator)
            return;
        
        GUILayout.Space(10f);
        if (GUILayout.Button("自动填入"))
        {
            FillFrames(generator);
        }

        GUILayout.Space(10f);
        if (!FaceMapCore.Processing)
        {
            if (GUILayout.Button("生成"))
            {
                Generate(generator);
            }
        }
        else
        {
            EditorGUI.BeginDisabledGroup(true);
            GUILayout.Button("生成中");
            EditorGUI.EndDisabledGroup();
        }
    }

    private static void Generate(FaceMapGenerator generator)
    {
        if (!FaceMapCore.IsValid(generator.frames, out List<string> errors))
        {
            StringBuilder sb = new StringBuilder();
            foreach (string error in errors)
                sb.AppendLine(error);
            EditorUtility.DisplayDialog("贴图不规范", sb.ToString(), "OK");
        }
        else
        {
            string path = Path.Combine(generator.folder, "facemap");
            FaceMapCore.Generate(generator.frames, path, facemap => generator.facemap = facemap);
        }
    }

    private static void FillFrames(FaceMapGenerator generator)
    {
        if (!Directory.Exists(generator.folder))
        {
            EditorUtility.DisplayDialog("目录不存在", "填入的目录不存在。\n请在Project视图中右键目录CopyPath并填入。", "OK");
        }
        else
        {
            // 填入图片
            generator.frames.Clear();
            int index = 0;
            while (true)
            {
                string path = Path.Combine(generator.folder, (char) ('a' + index++) + ".png");
                if (!File.Exists(path))
                    break;
                Texture2D frameTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
                if (!frameTexture)
                    break;
                var frame = new FaceMapFrame()
                {
                    texture = frameTexture,
                };
                generator.frames.Add(frame);
            }

            // 计算角度
            for (int i = 0; i < generator.frames.Count; i++)
            {
                generator.frames[i].angle = (int) (180f / (generator.frames.Count - 1) * i);
            }
        }
    }
}