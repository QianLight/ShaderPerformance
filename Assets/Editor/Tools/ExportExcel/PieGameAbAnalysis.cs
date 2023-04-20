using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using UnityEngine;
using  AssetStudio;
using B;
using TreeEditor;
using Unity.Collections;
using UnityEditor;
using Zeus.Framework;
using  AssetBundle=AssetStudio.AssetBundle;
using Debug = UnityEngine.Debug;
using Object = AssetStudio.Object;


namespace CFEngine.Quantify
{
    public class PieGameAbAnalysis
    {

        [MenuItem("Tools/游戏量化分析/TestAb")]
        public static void TestAb()
        {
            string path = Application.dataPath.Replace("/Assets", "/Bundles/Android/d2f3992.ab");
            AssetBundle_Analysis(path);
            AssetBundle_Analysis_Manifest(path, "d2f3992");
        }


        [MenuItem("Tools/游戏量化分析/AllTest")]
        public static void AllTest()
        {
            CaculateAllAb();
        }

        public static void CaculateAllAb(bool isShowProgress = true)
        {

            Debug.Log("PieGameAbAnalysis CaculateAllAb Begin!");

            allAbMapDic.Clear();

            string bundlesFolder = EditorAssetBundleUtils.GetPathRoot();
            ;
            string[] allFiles = Directory.GetFiles(bundlesFolder);

            string AbAnalysisFolder = Application.dataPath.Replace("/Assets", "/Library/AbAnalysis/");
            if (!Directory.Exists(AbAnalysisFolder))
            {
                Directory.CreateDirectory(AbAnalysisFolder);
            }

            for (int i = 0; i < allFiles.Length; i++)
            {
                string filePath = allFiles[i];
                if (filePath.Contains(".manifest")) continue;

                filePath = filePath.Replace(@"\", "/");
                string abName = filePath.Substring(filePath.LastIndexOf("/") + 1);
                if (isShowProgress)
                {
                    if (EditorUtility.DisplayCancelableProgressBar("解析数据:" + abName, filePath,
                            (float) i / allFiles.Length))
                    {
                        break;
                    }
                }

                string saveConfig = AbAnalysisFolder + GetStringHash(filePath) + "_" + abName;
                if (File.Exists(saveConfig))
                {
                    AbAnalysisData abad = LoadDatas(saveConfig);
                    allAbMapDic.Add(abName, abad.lists);
                }
                else
                {
                    AssetBundle_Analysis(filePath);
                    List<PieGameExcel.AssetsBundleDataMap> newData = AssetBundle_Analysis_Manifest(filePath, abName);
                    allAbMapDic.Add(abName, newData);
                    SaveDatas(saveConfig, newData);
                }
            }

            if (isShowProgress)
                EditorUtility.ClearProgressBar();

            Debug.Log("PieGameAbAnalysis CaculateAllAb End!");
        }

        private static string GetStringHash(string fileName)
        {
            return Zeus.Build.Crc32.ComputeFromFile(fileName);
            // using (HashAlgorithm hash = HashAlgorithm.Create())
            // using (FileStream file1 = new FileStream(fileName, FileMode.Open))
            // {
            //     byte[] hashByte1 = hash.ComputeHash(file1);
            //     return Convert.ToString(hashByte1);
            // }
        }

        private static void SaveDatas(string filePath, List<PieGameExcel.AssetsBundleDataMap> lists)
        {
            //Debug.Log("PieGameAbAnalysis SaveDatas, file path: " + filePath);
            string content = JsonUtility.ToJson(new AbAnalysisData(lists), true);
            File.WriteAllText(filePath, content);
            // Debug.Log("PieGameAbAnalysis Save successfully, file path: " + filePath);
        }

        private static AbAnalysisData LoadDatas(string filePath)
        {
            // Debug.Log("PieGameAbAnalysis LoadDatas, file path: " + filePath);
            AbAnalysisData pelc = JsonUtility.FromJson<AbAnalysisData>(File.ReadAllText(filePath));
            // Debug.Log("PieGameAbAnalysis LoadDatas successfully, file path: " + filePath);
            return pelc;
        }


        [Serializable]
        public class AbAnalysisData
        {
            public List<PieGameExcel.AssetsBundleDataMap> lists = new List<PieGameExcel.AssetsBundleDataMap>();

            public AbAnalysisData(List<PieGameExcel.AssetsBundleDataMap> lists)
            {
                this.lists = lists;
            }
        }

        public static Dictionary<string, List<PieGameExcel.AssetsBundleDataMap>> allAbMapDic =
            new Dictionary<string, List<PieGameExcel.AssetsBundleDataMap>>();

        public static List<PieGameExcel.AssetsBundleDataMap> AnalysisAllAb(string abName, string abPath)
        {
            if (allAbMapDic.ContainsKey(abName)) return allAbMapDic[abName];

            AssetBundle_Analysis(abPath);
            List<PieGameExcel.AssetsBundleDataMap> newData = AssetBundle_Analysis_Manifest(abPath, abName);
            allAbMapDic.Add(abName, newData);

            return newData;
        }


        private static long abTotalLongSize = 0;

        private static double totalInAbBytes = 0;

        private static Dictionary<string, Dictionary<ClassIDType, long>> allInAbDatas =
            new Dictionary<string, Dictionary<ClassIDType, long>>();

        public static void AssetBundle_Analysis(string fileName)
        {
            if (!File.Exists(fileName)) return;

            AssetsManager assetsManager = new AssetsManager();
            assetsManager.LoadFiles(new string[1] {fileName});

            FileInfo info = new FileInfo(fileName);
            abTotalLongSize = info.Length;

            string productName = null;
            var objectCount = assetsManager.assetsFileList.Sum(x => x.Objects.Count);
            var objectAssetItemDic = new Dictionary<Object, AssetItem>(objectCount);
            var containers = new List<(PPtr<Object>, string)>();
            int i = 0;
            foreach (var assetsFile in assetsManager.assetsFileList)
            {
                foreach (var asset in assetsFile.Objects)
                {
                    var assetItem = new AssetItem(asset);
                    objectAssetItemDic.Add(asset, assetItem);
                    assetItem.UniqueID = " #" + i;
                    var exportable = false;
                    switch (asset)
                    {
                        case AssetStudio.GameObject m_GameObject:
                            assetItem.Text = m_GameObject.m_Name;
                            break;
                        case AssetStudio.Texture2D m_Texture2D:
                            if (!string.IsNullOrEmpty(m_Texture2D.m_StreamData?.path))
                                assetItem.FullSize = asset.byteSize + m_Texture2D.m_StreamData.size;
                            assetItem.Text = m_Texture2D.m_Name;
                            exportable = true;
                            break;
                        case AssetStudio.AudioClip m_AudioClip:
                            if (!string.IsNullOrEmpty(m_AudioClip.m_Source))
                                assetItem.FullSize = asset.byteSize + m_AudioClip.m_Size;
                            assetItem.Text = m_AudioClip.m_Name;
                            exportable = true;
                            break;
                        case AssetStudio.VideoClip m_VideoClip:
                            if (!string.IsNullOrEmpty(m_VideoClip.m_OriginalPath))
                                assetItem.FullSize = asset.byteSize + (long) m_VideoClip.m_ExternalResources.m_Size;
                            assetItem.Text = m_VideoClip.m_Name;
                            exportable = true;
                            break;
                        case AssetStudio.Shader m_Shader:
                            assetItem.Text = m_Shader.m_ParsedForm?.m_Name ?? m_Shader.m_Name;
                            exportable = true;
                            break;
                        case AssetStudio.Mesh _:
                        case AssetStudio.TextAsset _:
                        case AssetStudio.AnimationClip _:
                        case AssetStudio.Font _:
                        case AssetStudio.MovieTexture _:
                        case AssetStudio.Sprite _:
                            assetItem.Text = ((NamedObject) asset).m_Name;
                            exportable = true;
                            break;
                        case AssetStudio.Animator m_Animator:
                            if (m_Animator.m_GameObject.TryGet(out var gameObject))
                            {
                                assetItem.Text = gameObject.m_Name;
                            }

                            exportable = true;
                            break;
                        case AssetStudio.MonoBehaviour m_MonoBehaviour:
                            if (m_MonoBehaviour.m_Name == "" && m_MonoBehaviour.m_Script.TryGet(out var m_Script))
                            {
                                assetItem.Text = m_Script.m_ClassName;
                            }
                            else
                            {
                                assetItem.Text = m_MonoBehaviour.m_Name;
                            }

                            exportable = true;
                            break;
                        case AssetStudio.PlayerSettings m_PlayerSettings:
                            productName = m_PlayerSettings.productName;
                            break;
                        case AssetStudio.AssetBundle m_AssetBundle:
                            foreach (var m_Container in m_AssetBundle.m_Container)
                            {
                                var preloadIndex = m_Container.Value.preloadIndex;
                                var preloadSize = m_Container.Value.preloadSize;
                                var preloadEnd = preloadIndex + preloadSize;
                                for (int k = preloadIndex; k < preloadEnd; k++)
                                {
                                    containers.Add((m_AssetBundle.m_PreloadTable[k], m_Container.Key));
                                }
                            }

                            assetItem.Text = m_AssetBundle.m_Name;
                            break;
                        case AssetStudio.ResourceManager m_ResourceManager:
                            foreach (var m_Container in m_ResourceManager.m_Container)
                            {
                                containers.Add((m_Container.Value, m_Container.Key));
                            }

                            break;
                        case AssetStudio.NamedObject m_NamedObject:
                            assetItem.Text = m_NamedObject.m_Name;
                            break;
                    }

                    if (assetItem.Text == "")
                    {
                        assetItem.Text = assetItem.TypeString + assetItem.UniqueID;
                    }
                }
            }


            foreach ((var pptr, var container) in containers)
            {
                if (pptr.TryGet(out var obj))
                {
                    objectAssetItemDic[obj].Container = container;
                }
            }

            containers.Clear();


            totalInAbBytes = 0;
            allInAbDatas.Clear();

            foreach (KeyValuePair<Object, AssetItem> itm in objectAssetItemDic)
            {
                totalInAbBytes += itm.Value.FullSize;

                Dictionary<ClassIDType, long> tmp = null;

                string strContainer = itm.Value.Container;
                // if (string.IsNullOrEmpty(strContainer))
                // {
                //     strContainer = "nocontainer";
                // }

                if (!allInAbDatas.TryGetValue(strContainer, out tmp))
                {
                    tmp = new Dictionary<ClassIDType, long>();
                    allInAbDatas.Add(strContainer, tmp);
                }

                ClassIDType type = itm.Value.Asset.type;
                if (tmp.ContainsKey(type))
                {
                    tmp[type] += itm.Value.FullSize;
                }
                else
                {
                    tmp.Add(type, itm.Value.FullSize);
                }


                // Debug.Log("abInfo：" + itm.Value.Text + "  " + itm.Value.Container + "  " + itm.Value.FullSize + "  " +
                //           itm.Value.SourceFile.originalPath + "  " + itm.Value.SourceFile.fileName + "  " +
                //           itm.Value.SourceFile.fullName + "  " + itm.Value.Asset.byteSize);
            }

            //Debug.Log("totalBytes:" + totalInAbBytes);
        }


        private static List<PieGameExcel.AssetsBundleDataMap> AssetBundle_Analysis_Manifest(string path, string abName)
        {
            string manifestPath = path + ".manifest";
            if (!File.Exists(manifestPath)) return null;

            string[] allLines = GetAllAssetsLines(manifestPath);
            if (allLines.Length == 0) return null;
            bool bIsScene = allLines[0].Contains(".unity");

            float percent = (float) ((float) abTotalLongSize / totalInAbBytes);

            List<PieGameExcel.AssetsBundleDataMap> allAssetsPath = new List<PieGameExcel.AssetsBundleDataMap>();

            PieGameExcel.AssetsBundleDataMap lastAssetsBundleDataMap = null;
            if (!bIsScene)
            {
                for (int i = 0; i < allLines.Length; i++)
                {
                    string assetPath = allLines[i];

                    PieGameExcel.AssetsBundleDataMap itmData = new PieGameExcel.AssetsBundleDataMap();

                    itmData.assetName = assetPath;
                    itmData.bundleName = abName;

                    foreach (KeyValuePair<string, Dictionary<ClassIDType, long>> itmInAb in allInAbDatas)
                    {
                        string strContainer = itmInAb.Key;
                        if (string.IsNullOrEmpty(strContainer)) continue;
                        strContainer = strContainer.Substring(strContainer.LastIndexOf("/"));
                        if (!assetPath.Contains(strContainer)) continue;
                        long totalType = AddBundleSerializeData(itmInAb, itmData, percent);
                        itmData.bundleSize = totalType * percent;
                        allInAbDatas.Remove(itmInAb.Key);
                        break;
                    }

                    if (itmData.bundleSize == 0) //为0的已经有了归属 不必重复计算
                    {
                        continue;
                    }

                    itmData._BundleSerializeData.Sort((x, y) => { return y.size.CompareTo(x.size); });
                    lastAssetsBundleDataMap = itmData;
                    allAssetsPath.Add(itmData);
                }
            }



            PieGameExcel.AssetsBundleDataMap finalAbData = lastAssetsBundleDataMap; //没有归属有的资源 例如assetbundle
            if (finalAbData == null)
            {
                finalAbData = new PieGameExcel.AssetsBundleDataMap();
                if (bIsScene)
                {
                    finalAbData.assetName = allLines[0];
                }

                finalAbData.bundleName = abName;
            }


            foreach (KeyValuePair<string, Dictionary<ClassIDType, long>> itmInAb in allInAbDatas) //还有剩余的数据也要统计
            {
                long totalType = AddBundleSerializeData(itmInAb, finalAbData, percent);
                finalAbData.bundleSize += totalType * percent;
            }

            finalAbData._BundleSerializeData.Sort((x, y) => { return y.size.CompareTo(x.size); });

            if (!allAssetsPath.Contains(finalAbData))
                allAssetsPath.Add(finalAbData);

            return allAssetsPath;
        }

        private static string[] GetAllAssetsLines(string manifestPath)
        {
            string[] allLines = File.ReadAllLines(manifestPath);
            List<string> listLines = new List<string>();
            for (int i = 0; i < allLines.Length; i++)
            {
                string lineItem = allLines[i];
                if (!lineItem.Contains("Assets/")) continue;
                string assetPath = lineItem.Replace("- ", string.Empty).ToLower();
                listLines.Add(assetPath);
            }

            return listLines.ToArray();
        }

        private static long AddBundleSerializeData(KeyValuePair<string, Dictionary<ClassIDType, long>> itmInAb,
            PieGameExcel.AssetsBundleDataMap itmData, float percent)
        {
            long totalType = 0;
            foreach (KeyValuePair<ClassIDType, long> typeItem in itmInAb.Value)
            {
                totalType += typeItem.Value;
                PieGameExcel.AssetsBundleSerializeData newData = new PieGameExcel.AssetsBundleSerializeData();
                newData.size = typeItem.Value * percent;
                newData.typeName = typeItem.Key.ToString();
                itmData._BundleSerializeData.Add(newData);
            }

            return totalType;
        }


        internal class AssetItem
        {
            public Object Asset;
            public SerializedFile SourceFile;
            public string Container = string.Empty;
            public string TypeString;
            public long m_PathID;
            public long FullSize;
            public ClassIDType Type;
            public string InfoText;
            public string UniqueID;
            public string Text;

            public AssetItem(Object asset)
            {
                Asset = asset;
                SourceFile = asset.assetsFile;
                Type = asset.type;
                TypeString = Type.ToString();
                m_PathID = asset.m_PathID;
                FullSize = asset.byteSize;
            }

        }
    }
}
