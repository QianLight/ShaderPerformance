/*******************************************************************
* Copyright © 2017—2021 Perfect World.Co.Ltd. All rights reserved.
* author: 移动项目支持部 ZEUS FRAMEWORK TEAM
********************************************************************/
using UnityEditor;
using UnityEngine;
using System.IO;
using System.Collections.Generic;
using System;

namespace Zeus.Core.FileSystem
{
    public class SplitingToolWindow : EditorWindow
    {
        [MenuItem("Zeus/FileSystem/VfileContent Spliting Tool", false, 21)]
        public static void Open()
        {
            SplitingToolWindow window = GetWindow<SplitingToolWindow>("Spliting Tool", true);
            window.autoRepaintOnSceneChange = true;
            window.Show();
        }

        string _vfileContentFolder = string.Empty;
        string _vfileIndexPath = string.Empty;
        string _outputPath = string.Empty;

        private void OnGUI()
        {
            GUILayout.BeginHorizontal();
            _vfileContentFolder = EditorGUILayout.TextField("Content Folder", _vfileContentFolder);
            if (GUILayout.Button("Browse", EditorStyles.miniButton, GUILayout.Width(70)))
            {
                string temp = EditorUtility.OpenFolderPanel("Content Folder", _vfileContentFolder, string.Empty);
                if (!string.IsNullOrEmpty(temp))
                {
                    _vfileContentFolder = temp;
                }
            }
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            _vfileIndexPath = EditorGUILayout.TextField("Index Path", _vfileIndexPath);
            if (GUILayout.Button("Browse", EditorStyles.miniButton, GUILayout.Width(70)))
            {
                string temp = EditorUtility.OpenFilePanel("Index Path", _vfileIndexPath, string.Empty);
                if (!string.IsNullOrEmpty(temp))
                {
                    _vfileIndexPath = temp;
                }
            }
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            _outputPath = EditorGUILayout.TextField("Output Folder", _outputPath);
            if (GUILayout.Button("Browse", EditorStyles.miniButton, GUILayout.Width(70)))
            {
                string temp = EditorUtility.OpenFolderPanel("Output Folder", _outputPath, string.Empty);
                if (!string.IsNullOrEmpty(temp))
                {
                    _outputPath = temp;
                }
            }
            GUILayout.EndHorizontal();
            if (GUILayout.Button("Split"))
            {
                SplitVfileContent();
            }
        }

        private void SplitVfileContent()
        {
            if (string.IsNullOrEmpty(_vfileIndexPath) || string.IsNullOrEmpty(_vfileContentFolder) || string.IsNullOrEmpty(_outputPath))
            {
                return;
            }
            if (!Directory.Exists(_vfileContentFolder) || !File.Exists(_vfileIndexPath))
            {
                return;
            }
            byte[] bytes = File.ReadAllBytes(_vfileIndexPath);
            ZeusFlatBuffers.ByteBuffer buffer = new ZeusFlatBuffers.ByteBuffer(bytes);
            VFileContentFB contentFB = VFileContentFB.GetRootAsVFileContentFB(buffer);
            Dictionary<string, VFileEntry> _vFileEntrys = new Dictionary<string, VFileEntry>(contentFB.FileEntryCount);
            int vfileContentCount = contentFB.EntrysLength;
            for (int i = 0; i < vfileContentCount; ++i)
            {
                VFileContentEntryFB indexEntry = contentFB.Entrys(i).Value;
                for (int j = 0; j < indexEntry.EntrysLength; ++j)
                {
                    VFileEntryFB entryFB = indexEntry.Entrys(j).Value;
                    VFileEntry entry = new VFileEntry(i, entryFB.Offset, entryFB.Length);
                    _vFileEntrys.Add(entryFB.Path, entry);
                }
            }

            FileStream[] streams = new FileStream[contentFB.EntrysLength];
            long targetSize = 0;
            for (int i = 0; i < contentFB.EntrysLength; ++i)
            {
                string filePath = Path.Combine(_vfileContentFolder, InnerPackage._VFileContent + i.ToString());
                FileInfo info = new FileInfo(filePath);
                if (info.Exists)
                {
                    streams[i] = File.OpenRead(filePath);
                    targetSize += info.Length;
                }
                else
                {
                    Debug.LogError($"Can't find file \"{filePath}\"");
                    for(int j = i - 1; j > 0; --j)
                    {
                        streams[j].Close();
                    }
                    return;
                }
            }

            long readByteCount = 0;
            foreach (var pair in _vFileEntrys)
            {
                VFileEntry entry = pair.Value;
                string desFilePath = Path.Combine(_outputPath, pair.Key);
                FileUtil.EnsureFolder(desFilePath);
                using (var fw = File.OpenWrite(desFilePath))
                {
                    byte[] buff = new byte[entry.Length];
                    try
                    {
                        readByteCount += StreamUtils.ReadBytes(streams[entry.VfileContentIndex], buff, entry.Offset, (int)entry.Length, SeekOrigin.Begin);
                    }
                    catch (ArgumentException e)
                    {
                        Debug.LogError("Wrong index file. [ArgumentException]" + e.Message);
                        return;
                    }
                    fw.Write(buff, 0, buff.Length);
                }
            }

            if (readByteCount != targetSize)
            {
                Debug.LogError("Wrong index file. Read count doesn't match file length." + readByteCount + " " + targetSize);
            }
            else
            {
                Debug.Log("Split successfully!");
            }
        }
    }
}


