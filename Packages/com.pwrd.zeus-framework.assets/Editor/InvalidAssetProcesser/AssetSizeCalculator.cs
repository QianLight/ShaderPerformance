/*******************************************************************
* Copyright © 2017—2022 Perfect World.Co.Ltd. All rights reserved.
* author: 移动项目支持部 ZEUS FRAMEWORK TEAM
********************************************************************/
using System.Collections.Generic;
using System.IO;
using System;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;
using Zeus.Build;
using Object = UnityEngine.Object;


namespace Zeus.Framework.Asset
{
    public class AssetSizeCalculator : EditorWindow
    {
        private string _assetPath;
        private long _size;
        private long _toalSize;
        private string _tips;
        private Queue<string> _tipList = new Queue<string>();

        [MenuItem("Zeus/Asset/统计资源bundle大小工具", false, 6)]
        static void Open()
        {
            AssetSizeCalculator window = GetWindow<AssetSizeCalculator>("统计资源bundle大小工具");
        }
        
        private void OnEnable()
        {
            
        }

        
        private void OnGUI()
        {
            EditorGUILayout.BeginVertical();
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("资源路径:", GUILayout.Width(70));
            _assetPath = EditorGUILayout.TextField(_assetPath);
            
            if (GUILayout.Button("统计"))
            {
                string info = CalculateSize(_assetPath, out _size, out _toalSize);
                if(info == null)
                {
                    _tips = string.Format("{0} 总大小：{1}KB  依赖大小： {2}KB", _assetPath, _toalSize / 1024, (_toalSize - _size)/1024);
                    _tipList.Enqueue(_tips);
                    if(_tipList.Count > 20)
                    {
                        _tipList.Dequeue();
                    }
                    _tips = null;
                }
                else
                {
                    _tips = info;
                }
                
            }
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space();
            if(_tips != null)
                EditorGUILayout.LabelField(_tips);

            foreach (var tips in _tipList)
            {
                EditorGUILayout.LabelField(tips);
            }
            EditorGUILayout.EndVertical();
        }
        
        private string CalculateSize(string path, out long size, out long totalSize)
        {
            AssetBundleUtils.Init();
            string abName;
            string assetName;
            size = 0;
            totalSize = 0;
            if (!AssetBundleUtils.TryGetAssetBundleName(path, out abName, out assetName))
            {
                return @"未找到资源，请检查资源路径";
            }

            string abPath = AssetBundleUtils.GetAssetBundlePath(abName); //获取到AssetBundlePath
            if (!File.Exists(abPath))
            {
                return @"未找到资源bundle，请确认是否打bundle";
            }

            size = new FileInfo(abPath).Length;

            string[] depNames = AssetBundleUtils.GetAllDependencies(abName);
            foreach(var dep in depNames)
            {
                abPath = AssetBundleUtils.GetAssetBundlePath(dep); //获取到AssetBundlePath
                if (!File.Exists(abPath))
                {
                    return @"未找到资源bundle，请确认是否打bundle";
                }
                totalSize += new FileInfo(abPath).Length;
            }

            totalSize += size;
            return null;
        }
        
    }
}