/*******************************************************************
* Copyright © 2017—2022 Perfect World.Co.Ltd. All rights reserved.
* author: 移动项目支持部 ZEUS FRAMEWORK TEAM
********************************************************************/
using System;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

public class ExtractMemoryEditor: EditorWindow
{
    private static ExtractMemoryEditor Window;
    private static string _elements = string.Empty;

	[MenuItem("Zeus/Asset/Profiler信息导出", false, 10)]
    public static void ShowWindow()
    {
        EditorApplication.ExecuteMenuItem("Window/Analysis/Profiler");
        if (Window == null)
        {
            Window = CreateInstance<ExtractMemoryEditor>();
        }
        Window.Show();
    }

    private void OnGUI()
    {
        EditorGUILayout.BeginVertical();

        EditorGUILayout.LabelField("Current Target: " + ProfilerDriver.GetConnectionIdentifier(ProfilerDriver.connectedProfiler));
        if (GUILayout.Button("Take Sample"))
        {
            TakeSample();
        }
        _elements = EditorGUILayout.TextField("Elements", _elements);
        if (GUILayout.Button("Export"))
        {
            Export(_elements);
        }
        if (GUILayout.Button("Export All"))
        {
            ExportAll();
        }
        if (GUILayout.Button("DecodeSerializedFile"))
        {
            DecodeSerializedFile();
        }
        
        EditorGUILayout.BeginVertical();
    }

    private static void TakeSample()
    {
        ProfilerWindow.RefreshMemoryData();
    }

    private void Export(string elementsStr)
    {
        var elements = elementsStr.Split(';');
        var rootElement = ProfilerWindow.GetMemoryDetailRoot();
        foreach (var element in elements)
        {
            var childElement = rootElement.GetChildByName(element);
            if (childElement != null)
            {
                rootElement = childElement;
            }
        }
        
        if (rootElement != null)
        {
            var outputPath = $"MemoryDetailed{DateTime.Now:yyyy_MM_dd_HH_mm_ss}.txt";
            File.Create(outputPath).Dispose();
            var sw = new StreamWriter(outputPath);
            WriteMemoryDetail(sw, rootElement);
            sw.Flush();
            sw.Close();
        }
    }
    
    private void ExportAll()
    {
        var rootElement = ProfilerWindow.GetMemoryDetailRoot();

        if (null != rootElement)
        {
            var outputPath = $"MemoryDetailed{DateTime.Now:yyyy_MM_dd_HH_mm_ss}.txt";
            var sw = new StreamWriter(File.Create(outputPath));
            WriteMemoryDetail(sw, rootElement);
            sw.Flush();
            sw.Close();
        }
    }
    private static void WriteMemoryDetail(StreamWriter sw, MemoryElement root)
    {
        if (null == root) return;
        sw.WriteLine(root.ToString());
        foreach (var memoryElement in root.children)
        {
            if (null != memoryElement)
            {
                WriteMemoryDetail(sw, memoryElement);
            }
        }
    }

    private static void DecodeSerializedFile()
    {
        var decodeDic = DecodeHelper.GetDictionary(DecodeHelper.FindBundleNames());
        var rootElement = ProfilerWindow.GetMemoryDetailRoot();
        rootElement = rootElement.GetChildByName("Other").GetChildByName("SerializedFile");

        string regexStr = @"[a-f\d]{32}|[A-F\d]{32}";
        
        var outputPath = $"MemoryDetailed{DateTime.Now:yyyy_MM_dd_HH_mm_ss}.txt";
        var sw = new StreamWriter(File.Create(outputPath));
        foreach (var child in rootElement.children)
        {
            var ciphertext = Regex.Match(child.name, regexStr).Value;
            if (decodeDic.TryGetValue(ciphertext, out var bundleName))
            {
                child.name = bundleName;
            }
            else if(child.name.StartsWith("archive:"))
            {
                child.name = child.name.Split('/')[1];
            }
            sw.WriteLine(child.ToString());
        }
        sw.Flush();
        sw.Close();
    }
}
