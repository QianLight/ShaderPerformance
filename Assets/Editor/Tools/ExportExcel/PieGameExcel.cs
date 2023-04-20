using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEditor;
using OfficeOpenXml;
using System.IO;
using Zeus.Framework.Asset;
using System;
using System.Linq;
using System.Text;
using OfficeOpenXml.Style;
using ICSharpCode.ZeusSharpZipLib.Zip;
using Zeus.Framework;

namespace CFEngine.Quantify
{
    public partial class PieGameExcel : EditorWindow
    {
        [MenuItem("Tools/游戏量化分析/PieGameSize")]
        public static void ShowPieGameSize()
        {
            PieGameExcel pieGameSize = GetWindowWithRect<PieGameExcel>(new Rect(150, 35, 550, 700), false, "量化分析");
        }


        private static string Excele_PathShowJson_Tag = "Excele_PathShowJson";

        public static string Excele_PathShowJson
        {
            get { return PlayerPrefs.GetString(Excele_PathShowJson_Tag); }
            set { PlayerPrefs.SetString(Excele_PathShowJson_Tag, value); }
        }

        private static string Excele_PathShowJsonCompare1_Tag = "Excele_PathShowJsonCompare1";

        public static string Excele_PathShowJsonCompare1
        {
            get { return PlayerPrefs.GetString(Excele_PathShowJsonCompare1_Tag); }
            set { PlayerPrefs.SetString(Excele_PathShowJsonCompare1_Tag, value); }
        }

        private static string Excele_PathShowJsonCompare2_Tag = "Excele_PathShowJsonCompare2";

        public static string Excele_PathShowJsonCompare2
        {
            get { return PlayerPrefs.GetString(Excele_PathShowJsonCompare2_Tag); }
            set { PlayerPrefs.SetString(Excele_PathShowJsonCompare2_Tag, value); }
        }

        private void OnEnable()
        {
            InitConfigData_Snapdragon();
            InitConfigData_Functions();
            InitConfigData();
        }

        private void OnGUI()
        {
            OnGUI_GameSize();
            OnGUI_Functions();
            OnGUI_Snapdragon();
        }

        private void OnGUI_GameSize()
        {
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("分析项目大小", EditorStyles.miniButton, GUILayout.Width(200)))
            {
                PieGameExcelShows();
            }

            GUILayout.EndHorizontal();


            GUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("预览路径:" + Excele_PathShowJson, new GUILayoutOption[] {GUILayout.Width(450f)});
            if (GUILayout.Button("预览", EditorStyles.miniButton, GUILayout.Width(60)))
            {
                string path = EditorUtility.OpenFilePanel("选择需要显示的数据", Excele_PathShowJson, "json");
                if (!string.IsNullOrEmpty(path))
                {
                    ShowJsonData(path);
                    Excele_PathShowJson = path;
                }
            }

            GUILayout.EndHorizontal();


            GUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("数据1:" + Excele_PathShowJsonCompare1,
                new GUILayoutOption[] {GUILayout.Width(450f)});
            if (GUILayout.Button("选择", EditorStyles.miniButton, GUILayout.Width(60)))
            {
                string path = EditorUtility.OpenFilePanel("选择需要显示的数据", Excele_PathShowJsonCompare1, "json");
                if (!string.IsNullOrEmpty(path))
                {
                    Excele_PathShowJsonCompare1 = path;
                }
            }

            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("数据2:" + Excele_PathShowJsonCompare2,
                new GUILayoutOption[] {GUILayout.Width(450f)});
            if (GUILayout.Button("选择", EditorStyles.miniButton, GUILayout.Width(60)))
            {
                string path = EditorUtility.OpenFilePanel("选择需要显示的数据", Excele_PathShowJsonCompare2, "json");
                if (!string.IsNullOrEmpty(path))
                {
                    Excele_PathShowJsonCompare2 = path;
                }
            }

            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("对比数据", EditorStyles.miniButton, GUILayout.Width(60)))
            {
                ShowCompareJsonData();
            }

            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("对比数据", EditorStyles.miniButton, GUILayout.Width(60)))
            {
                ShowCompareJsonData();
            }

            GUILayout.EndHorizontal();
        }



        public static void PieGameExcelShows(string targetXlsxPath = "")
        {

            Debug.Log("PieGameExcelShows Begin!");

            string filePath = targetXlsxPath;

            if (string.IsNullOrEmpty(filePath))
            {
                filePath = Application.dataPath.Replace("/Assets", "/Library") + '/' + PlayerSettings.productName +
                           ".xlsx"; //这里是文件路径
            }

            SubpackageInfo.allGameMapDatas.Clear();

            PieGameAbAnalysis.CaculateAllAb(false);



            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }

            Debug.Log("PieGameExcelShows xlsx:" + filePath);


            InitConfigData();
            InitDataAssetBundle();

            FileInfo fileInfo = new FileInfo(filePath);
            using (ExcelPackage excelPackage = new ExcelPackage(fileInfo))
            {
                Analysis_Project();
                Show_AllDatas(excelPackage);
                excelPackage.Save(); //写入后保存表格
            }

            SaveDatas(filePath.Replace(".xlsx", ".json"));

            Debug.Log("PieGameExcelShows End!");
        }


        private static void SaveDatas(string filePath)
        {
            Debug.Log("PieGameExcelShows SaveDatas, file path: " + filePath);
            string content = JsonUtility.ToJson(new PieExcelListClass(_AllSubPackList), true);
            File.WriteAllText(filePath, content);
            Debug.Log("PieGameExcelShows Save successfully, file path: " + filePath);
        }


        private static void ShowJsonData(string jsonFilePath)
        {
            PieExcelListClass allDatas = LoadDatas(jsonFilePath);
            _AllSubPackList = allDatas.list;

            string excelPath = jsonFilePath.Replace(".json", ".xlsx");
            if (File.Exists(excelPath))
            {
                File.Delete(excelPath);
            }

            FileInfo fileInfo = new FileInfo(excelPath);
            using (ExcelPackage excelPackage = new ExcelPackage(fileInfo))
            {
                Show_AllDatas(excelPackage);
                excelPackage.Save(); //写入后保存表格
            }

            Debug.Log("ShowJsonData xlsx:" + excelPath);
        }


        private static void ShowCompareJsonData()
        {
            if (string.IsNullOrEmpty(Excele_PathShowJsonCompare1) ||
                string.IsNullOrEmpty(Excele_PathShowJsonCompare2)) return;

            if (!File.Exists(Excele_PathShowJsonCompare1))
            {
                return;
            }

            if (!File.Exists(Excele_PathShowJsonCompare2))
            {
                return;
            }

            //FileInfo info1 = new FileInfo(Excele_PathShowJsonCompare1);
            FileInfo info2 = new FileInfo(Excele_PathShowJsonCompare2);

            PieExcelListClass allDatas1 = LoadDatas(Excele_PathShowJsonCompare1);
            PieExcelListClass allDatas2 = LoadDatas(Excele_PathShowJsonCompare2);
            ShowCompare_DataDif(allDatas1, allDatas2);

            string excelPath =
                Excele_PathShowJsonCompare1.Replace(".json", "_不同于_" + info2.Name.Replace(".json", "") + ".xlsx");
            if (File.Exists(excelPath))
            {
                File.Delete(excelPath);
            }

            FileInfo fileInfo = new FileInfo(excelPath);
            using (ExcelPackage excelPackage = new ExcelPackage(fileInfo))
            {
                Show_AllDatas(excelPackage);
                excelPackage.Save(); //写入后保存表格
            }

            Debug.Log("ShowCompareJsonData xlsx:" + excelPath);
        }

        private static void ShowCompare_DataDif(PieExcelListClass allDatas1, PieExcelListClass allDatas2)
        {
            _AllSubPackList = new List<SubpackageInfo>();
            for (int i = 0; i < allDatas1.list.Count; i++)
            {
                _AllSubPackList.Add(ShowCompare_DataDif_Item(allDatas1.list[i], allDatas2.list[i]));
            }
        }

        private static SubpackageInfo ShowCompare_DataDif_Item(SubpackageInfo sub1, SubpackageInfo sub2)
        {
            SubpackageInfo compare = new SubpackageInfo(sub1.tagName);
            compare.allDatas = GetCompareList(sub1.allDatas, sub1.allDatas);
            compare.otherDatas = GetCompareList(sub1.otherDatas, sub1.otherDatas);
            for (int i = 0; i < sub1.allClassifyList.Count; i++)
            {
                ClassifyDataInfo itm1 = sub1.allClassifyList[i];
                ClassifyDataInfo itm2 = sub2.allClassifyList[i];

                ClassifyDataInfo newCDI = new ClassifyDataInfo();
                newCDI.des = itm1.des;
                newCDI.tag = itm1.tag;

                newCDI.allDatas = GetCompareList(itm1.allDatas, itm2.allDatas);
                compare.allClassifyList.Add(newCDI);
            }

            return compare;
        }

        public static List<AssetsBundleDataMap> GetCompareList(List<AssetsBundleDataMap> list1,
            List<AssetsBundleDataMap> list2)
        {
            Dictionary<string, AssetsBundleDataMap> diffDic = new Dictionary<string, AssetsBundleDataMap>();

            for (int i = 0; i < list1.Count; i++)
            {
                AssetsBundleDataMap itm = list1[i];
                diffDic.Add(itm.assetName, itm);
            }

            for (int i = 0; i < list2.Count; i++)
            {
                AssetsBundleDataMap itm = list2[i];
                AssetsBundleDataMap itmCompare = null;
                if (diffDic.TryGetValue(itm.assetName, out itmCompare))
                {
                    itmCompare.bundleSize -= itm.bundleSize;
                    if (itmCompare.bundleSize == 0)
                    {
                        diffDic.Remove(itm.assetName);
                    }
                }
                else
                {
                    diffDic.Add(itm.assetName, itm);
                }
            }

            List<AssetsBundleDataMap> compareList = new List<AssetsBundleDataMap>();
            foreach (KeyValuePair<string, AssetsBundleDataMap> itm in diffDic)
            {
                compareList.Add(itm.Value);
            }

            compareList.Sort((x, y) => { return y.bundleSize.CompareTo(x.bundleSize); });
            return compareList;
        }


        private static PieExcelListClass LoadDatas(string filePath)
        {
            Debug.Log("PieGameExcelShows LoadDatas, file path: " + filePath);
            PieExcelListClass pelc = JsonUtility.FromJson<PieExcelListClass>(File.ReadAllText(filePath));
            Debug.Log("PieGameExcelShows LoadDatas successfully, file path: " + filePath);
            return pelc;
        }



        private static Dictionary<string, string> _ClassifyTagList = new Dictionary<string, string>();
        private static List<SubpackageInfo> _AllSubPackList = new List<SubpackageInfo>();

        private static void InitConfigData()
        {
            _ClassifyTagList = new Dictionary<string, string>();
            _ClassifyTagList.Add("/scene", "场景");
            _ClassifyTagList.Add("shared_", "共享资源");
            _ClassifyTagList.Add("uibackground/", "UI背景");
            _ClassifyTagList.Add("timeline/", "Timeline");
            _ClassifyTagList.Add("animation/", "动画");
            _ClassifyTagList.Add("runtime/sfx/", "特效");
            _ClassifyTagList.Add("assets/effects/", "特效资源");
            _ClassifyTagList.Add("creatures/", "角色资源");
            _ClassifyTagList.Add("runtime/prefab/", "预制体(角色 lod 场景动态物体)");
            _ClassifyTagList.Add("ui/", "UI");
            _ClassifyTagList.Add("spine2d/", "Spine2d");
            _ClassifyTagList.Add("prefabs/cinemachine/", "Cinemachine");
            _ClassifyTagList.Add("specialaction/radialblurv2/", "Specialaction");
            _ClassifyTagList.Add(".bank", "音频");
            _ClassifyTagList.Add(@"bundleres/video", "视频");
            _ClassifyTagList.Add(@"bundleres/skillpackage", "技能");
            _ClassifyTagList.Add(@"bundleres/hitpackage", "被击");
            _ClassifyTagList.Add(@"bundleres/reactpackage", "反应");
            _ClassifyTagList.Add(@"bundleres/config", "配置");
            _ClassifyTagList.Add(@"bundleres/table", "策划表");
            _ClassifyTagList.Add(@"bundleres/guide", "引导");
            _ClassifyTagList.Add(@"lua/", "lua");
            _ClassifyTagList.Add(@".so", "库脚本");
            _ClassifyTagList.Add(@".dex", "java文件");
            _ClassifyTagList.Add(@".xml", "XML");
            _ClassifyTagList.Add("META-INF/", "Android资源文件");
            //_ClassifyTagList.Add("*other*", "未分类");

            _AllSubPackList = new List<SubpackageInfo>();
            _AllSubPackList.Add(new SubpackageInfo("Apk的大小(M)"));
            _AllSubPackList.Add(new SubpackageInfo("放到Apk的AB大小(M)"));
            _AllSubPackList.Add(new SubpackageInfo("硬分包大小(M)"));
            _AllSubPackList.Add(new SubpackageInfo("软分包大小(M)"));
        }

        private static void InitDataAssetBundle()
        {
            SubpackageWindow.Setting = SubpackageSetting.LoadSetting();
            SubpackageWindow.TagAsset = TagAsset.LoadTagAsset();

            AssetBundleUtils.Init();
        }


        private static void Analysis_Project()
        {
            List<string> tagList = SubpackageWindow.TagAsset.TagListForRecord;
            Analysis_ApkSizeExcel(_AllSubPackList[0]);
            for (int i = 1; i < _AllSubPackList.Count; i++)
            {
                Analysis_AbSizeExcel(AddBundleDataByTag(_AllSubPackList[i], tagList[i - 1]));
            }

            for (int i = 0; i < _AllSubPackList.Count; i++)
            {
                _AllSubPackList[i].CaculateClassAllData();
            }
        }

        private static void Show_AllDatas(ExcelPackage excelPackage)
        {
            Sheet_TotalSizeExcel(excelPackage);
            for (int i = 0; i < _AllSubPackList.Count; i++)
            {
                _AllSubPackList[i].ShowDataToSelfExcel(excelPackage);
            }
        }

        private static void Sheet_TotalSizeExcel(ExcelPackage excelPackage)
        {
            ExcelWorksheet worksheet = excelPackage.Workbook.Worksheets.Add("大小总览(M)");
            worksheet.DefaultColWidth = 20;

            int nStartCell = 2;
            worksheet.Cells[nStartCell, 1].Value = "总大小(M)";
            nStartCell++;

            foreach (KeyValuePair<string, string> itmClass in _ClassifyTagList)
            {
                worksheet.Cells[nStartCell, 1].Value = itmClass.Value + "（路径包含:" + itmClass.Key + ")";
                worksheet.Cells[nStartCell, 1].AutoFitColumns(40);
                nStartCell++;
            }

            worksheet.Cells[nStartCell, 1].Value = "未分类";

            nStartCell = 1;
            int nStartCol = 2;

            ShowSubSummaryData(worksheet, nStartCell, nStartCol);

            nStartCol += 2;
            nStartCell = 1;

            for (int i = 0; i < _AllSubPackList.Count; i++)
            {
                SubpackageInfo subData = _AllSubPackList[i];

                int nCol = nStartCol + (i) * 2;
                ExcelRange range = worksheet.Cells[nStartCell, nCol, nStartCell, nCol + 1];
                range.Merge = true;
                range.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                range.Value = subData.tagName;
                Sheet_TotalSizeExcel_Class(subData, worksheet, nStartCell, nCol);
            }
        }

        private static void ShowSubSummaryData(ExcelWorksheet worksheet, int nStartCell, int nStartCol)
        {
            ExcelRange range = worksheet.Cells[nStartCell, nStartCol, nStartCell, nStartCol + 1];
            range.Merge = true;
            range.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
            range.Value = "分类总大小(M)";

            nStartCell++;

            int nTotalCount = 0;
            double bundlesTotalSize = 0;

            nTotalCount = 0;
            bundlesTotalSize = 0;

            for (int i = 0; i < _AllSubPackList.Count; i++)
            {
                SubpackageInfo subData = _AllSubPackList[i];
                nTotalCount += subData.allDatas.Count;
                bundlesTotalSize += subData.GetAllDatasSize();
            }

            worksheet.Cells[nStartCell, nStartCol].Value = GetSizeString(bundlesTotalSize);
            worksheet.Cells[nStartCell, nStartCol + 1].Value = "数量:" + (nTotalCount);


            int nIndex = 0;
            nStartCell++;
            foreach (KeyValuePair<string, string> itmClass in _ClassifyTagList)
            {

                nTotalCount = 0;
                bundlesTotalSize = 0;

                for (int i = 0; i < _AllSubPackList.Count; i++)
                {
                    SubpackageInfo subData = _AllSubPackList[i];
                    ClassifyDataInfo cdi = subData.allClassifyList[nIndex];
                    nTotalCount += cdi.allDatas.Count;
                    bundlesTotalSize += cdi.GetAllDatasSize();
                }

                worksheet.Cells[nStartCell, nStartCol].Value = GetSizeString(bundlesTotalSize);

                worksheet.Cells[nStartCell, nStartCol + 1].Value = "数量:" + (nTotalCount);

                nIndex++;
                nStartCell++;
            }


            nTotalCount = 0;
            bundlesTotalSize = 0;

            for (int i = 0; i < _AllSubPackList.Count; i++)
            {
                SubpackageInfo subData = _AllSubPackList[i];
                nTotalCount += subData.otherDatas.Count;
                bundlesTotalSize += subData.GetOtherDatasSize();
            }

            worksheet.Cells[nStartCell, nStartCol].Value = GetSizeString(bundlesTotalSize);
            worksheet.Cells[nStartCell, nStartCol + 1].Value = "数量:" + (nTotalCount);

        }

        private static void Sheet_TotalSizeExcel_Class(SubpackageInfo subData, ExcelWorksheet worksheet, int nStartCell,
            int nOffset)
        {
            double bundlesTotalSize = subData.GetAllDatasSize();
            nStartCell++;
            worksheet.Cells[nStartCell, nOffset].Value = GetSizeString(bundlesTotalSize);
            worksheet.Cells[nStartCell, nOffset + 1].Value = "数量:" + subData.allDatas.Count;
            worksheet.Cells[nStartCell, nOffset + 1].AutoFitColumns(15);

            nStartCell++;

            double bundlesTotalSizeHas = 0;
            int nTotalCountLeft = 0;

            for (int i = 0; i < subData.allClassifyList.Count; i++)
            {
                ClassifyDataInfo classItem = subData.allClassifyList[i];

                double dTotalSize = classItem.GetAllDatasSize();
                HyperlinkItem(worksheet.Cells[nStartCell, nOffset], GetSizeString(dTotalSize), subData.tagName,
                    "A" + (nTotalCountLeft + 3 + i + subData.otherDatas.Count));

                worksheet.Cells[nStartCell, nOffset + 1].Value = "数量:" + classItem.allDatas.Count;
                worksheet.Cells[nStartCell, nOffset + 1].AutoFitColumns(15);
                bundlesTotalSizeHas += dTotalSize;
                nTotalCountLeft += classItem.allDatas.Count;

                nStartCell++;
            }

            HyperlinkItem(worksheet.Cells[nStartCell, nOffset], GetSizeString(bundlesTotalSize - bundlesTotalSizeHas),
                subData.tagName, "A2");

            worksheet.Cells[nStartCell, nOffset + 1].Value = "数量:" + (subData.otherDatas.Count);
            worksheet.Cells[nStartCell, nOffset + 1].AutoFitColumns(15);
            //nStartCell++;

            //nStartCell++;
            //worksheet.Cells[nStartCell, 1].Value = "资源路径";
            //worksheet.Cells[nStartCell, 2].Value = "资源大小";
            //worksheet.Cells[nStartCell, 3].Value = "AB名字";
            //nStartCell++;
        }

        private static void HyperlinkItem(ExcelRange excelRange, string strValue, string sheet, string pos)
        {
            excelRange.Value = strValue;

            string _UriName = $"#'{sheet}'!{pos}";
            //Debug.Log("HyperlinkItem:" + _UriName);
            excelRange.Hyperlink = new Uri(_UriName, UriKind.Relative);

            excelRange.Style.Font.Color.SetColor(255, 0, 0, 255);
            excelRange.Style.Font.UnderLine = true;
        }


        private static void Analysis_ApkSizeExcel(SubpackageInfo subData)
        {
            string apkPath = Application.dataPath.Replace("Assets", "") + "Android/cfgame.apk";
            FileStream fsFile_ = new FileStream(apkPath, FileMode.Open);
            ICSharpCode.ZeusSharpZipLib.Zip.ZipFile zipFile_ = new ICSharpCode.ZeusSharpZipLib.Zip.ZipFile(fsFile_);
            foreach (ZipEntry z in zipFile_)
            {

                if (z.Name.Contains("assets/Bundles")) continue;

                AssetsBundleDataMap newData = new AssetsBundleDataMap();
                newData.assetName = z.Name;
                newData.bundleSize = (double) z.CompressedSize;
                subData.allDatas.Add(newData);
            }

            zipFile_.Close();
            fsFile_.Close();
        }

        private static void Analysis_AbSizeExcel(SubpackageInfo subData)
        {

        }

        public static string GetSizeString(double size)
        {
            return String.Format("{0:N6}", size / Zeus.Core.ZeusConstant.MB);
        }


        private static SubpackageInfo AddBundleDataByTag(SubpackageInfo subData, string tag)
        {
            SetAssetList(tag);

            var bundleList = AssetListHelper.GetBundleListFromAssetList(_assetList);

            double bundlesTotalSize = 0;
            string bundleDir = EditorAssetBundleUtils.GetPathRoot();
            var bundleMapAssets = AssetBundleUtils.GetBundleMapAssets();


            for (int i = 0; i < bundleList.Count; i++)
            {
                string bundle = bundleList[i];

                FileInfo info = new FileInfo(Path.Combine(bundleDir, bundle));
                if (info.Exists)
                {
                    bundlesTotalSize += info.Length;
                }
                else
                {
                    Debug.LogWarning($"Can't get info of bundle \"{bundle}\".");
                    continue;
                }


                List<PieGameExcel.AssetsBundleDataMap> abDatas = PieGameAbAnalysis.AnalysisAllAb(bundle, info.FullName);
                if (abDatas == null)
                {
                    Debug.LogWarning($"abDatas null \"{bundle}\".");
                    continue;
                }

                subData.AddByRange(abDatas);

                // AssetsBundleDataMap newData = new AssetsBundleDataMap();
                //
                // if (!bundleMapAssets.ContainsKey(bundle))
                // {
                //     Debug.LogWarning($"Can't get info of bundleMapAssets \"{bundle}\".");
                //     newData.assetName = bundle;
                // }
                // else
                // {
                //     newData.assetName = string.Join(",", bundleMapAssets[bundle]);
                // }
                //
                // newData.bundleName = bundle;
                // newData.bundleSize = (double)info.Length;
                // subData.allDatas.Add(newData);
            }

            return subData;
        }


        static List<string> _assetList = new List<string>();

        private static void SetAssetList(string tag)
        {
            try
            {
                _assetList.Clear();
                _assetList = AssetListHelper.LoadAssetListFromFiles(new string[1] {tag}, false);
                SimplifyAssetList(ref _assetList);
            }
            catch (Exception)
            {
                Debug.Log("Tag : " + tag + " 不存在,请确认AssetList路径");
            }
        }


        private static void SimplifyAssetList(ref List<string> assetList)
        {
            List<string> tempAssetList = new List<string>();
            HashSet<string> assetSet = new HashSet<string>();
            foreach (string asset in assetList)
            {
                if (assetSet.Add(asset))
                {
                    tempAssetList.Add(asset);
                }
            }

            assetList = tempAssetList;
        }


        [Serializable]
        public class AssetsBundleSerializeData
        {
            public string typeName;
            public double size;
        }


        [Serializable]
        public class AssetsBundleDataMap
        {
            public string assetName;
            public string bundleName;
            public double bundleSize;
            public List<AssetsBundleSerializeData> _BundleSerializeData = new List<AssetsBundleSerializeData>();
        }

        [Serializable]
        public class ClassifyDataInfo
        {
            public string des;
            public string tag;
            public List<AssetsBundleDataMap> allDatas = new List<AssetsBundleDataMap>();

            public double GetAllDatasSize()
            {
                double bundlesTotalSizeHas = 0;
                for (int i = 0; i < allDatas.Count; i++)
                {
                    bundlesTotalSizeHas += allDatas[i].bundleSize;
                }

                return bundlesTotalSizeHas;
            }


        }

        [Serializable]
        public class SubpackageInfo
        {
            public string tagName;

            [NonSerialized] public ExcelWorksheet worksheet;
            public List<ClassifyDataInfo> allClassifyList = new List<ClassifyDataInfo>();

            public static List<string> allGameMapDatas = new List<string>();

            public List<AssetsBundleDataMap> allDatas = new List<AssetsBundleDataMap>();

            public void AddByRange(List<AssetsBundleDataMap> rangeData)
            {
                for (int i = 0; i < rangeData.Count; i++)
                {
                    AssetsBundleDataMap itmData = rangeData[i];
                    if (allGameMapDatas.Contains(itmData.assetName)) continue;
                    allDatas.Add(itmData);
                    allGameMapDatas.Add(itmData.assetName);
                }
            }

            public List<AssetsBundleDataMap> otherDatas = new List<AssetsBundleDataMap>();

            public void SortAllData()
            {
                allDatas.Sort((x, y) => { return y.bundleSize.CompareTo(x.bundleSize); });
            }

            public void CaculateClassAllData()
            {
                SortAllData();
                otherDatas.AddRange(allDatas);

                foreach (KeyValuePair<string, string> itmClass in _ClassifyTagList)
                {
                    string tagType = itmClass.Key;
                    ClassifyDataInfo newClassifyDataInfo = new ClassifyDataInfo();
                    newClassifyDataInfo.tag = itmClass.Key;
                    newClassifyDataInfo.des = itmClass.Value;

                    for (int j = otherDatas.Count - 1; j >= 0; j--)
                    {
                        AssetsBundleDataMap bundle = otherDatas[j];
                        if (!bundle.assetName.Contains(tagType)) continue;

                        newClassifyDataInfo.allDatas.Add(bundle);
                        otherDatas.RemoveAt(j);
                    }

                    newClassifyDataInfo.allDatas.Reverse();

                    allClassifyList.Add(newClassifyDataInfo);
                }
            }

            public double GetAllDatasSize()
            {
                double bundlesTotalSizeHas = 0;
                for (int i = 0; i < allDatas.Count; i++)
                {
                    bundlesTotalSizeHas += allDatas[i].bundleSize;
                }

                return bundlesTotalSizeHas;
            }

            public double GetOtherDatasSize()
            {
                double bundlesTotalSizeHas = 0;
                for (int i = 0; i < otherDatas.Count; i++)
                {
                    bundlesTotalSizeHas += otherDatas[i].bundleSize;
                }

                return bundlesTotalSizeHas;
            }

            private int nStartCell = 1;

            private int nMaxCol = 4;

            public void ShowDataToSelfExcel(ExcelPackage excelPackage)
            {
                worksheet = excelPackage.Workbook.Worksheets.Add(tagName);
                worksheet.DefaultColWidth = 20;

                nStartCell = 1;
                worksheet.Cells[nStartCell, 2].Value = "资源路径";
                worksheet.Cells[nStartCell, 2].AutoFitColumns(60);

                worksheet.Cells[nStartCell, 3].Value = "资源大小(M)";

                worksheet.Cells[nStartCell, 4].Value = "AB名字";
                nStartCell++;

                ShowCells("Others", otherDatas);

                nMaxCol = 4;

                for (int i = 0; i < allClassifyList.Count; i++)
                {
                    ClassifyDataInfo classItem = allClassifyList[i];
                    ShowCells(classItem.des, classItem.allDatas);
                }

                for (int i = 5; i < nMaxCol; i++)
                {
                    worksheet.Cells[1, i].Value = "类型-大小(M)";
                }
            }

            private void ShowCells(string tag, List<AssetsBundleDataMap> dataList)
            {
                worksheet.Cells[nStartCell, 1].Value = tag;
                worksheet.Cells[nStartCell, 1, nStartCell, 100].Merge = true;
                nStartCell++;

                for (int i = 0; i < dataList.Count; i++)
                {
                    AssetsBundleDataMap bundle = dataList[i];
                    int nRow = i + nStartCell;

                    int nStartCol = 2;
                    worksheet.Cells[nRow, nStartCol].Value = bundle.assetName;
                    worksheet.Cells[nRow, nStartCol].AutoFitColumns(60);
                    worksheet.Cells[nRow, nStartCol].Style.WrapText = true;

                    nStartCol++;
                    worksheet.Cells[nRow, nStartCol].Value = GetSizeString(bundle.bundleSize);
                    worksheet.Cells[nRow, nStartCol].Style.Numberformat.Format = "00";
                    nStartCol++;
                    worksheet.Cells[nRow, nStartCol].Value = bundle.bundleName;
                    nStartCol++;

                    for (int j = 0; j < bundle._BundleSerializeData.Count; j++)
                    {
                        AssetsBundleSerializeData seriData = bundle._BundleSerializeData[j];

                        worksheet.Cells[nRow, nStartCol].Value = seriData.typeName + "-" + GetSizeString(seriData.size);
                        worksheet.Cells[nRow, nStartCol].Style.WrapText = true;

                        nStartCol++;
                    }

                    if (nStartCol > nMaxCol)
                    {
                        nMaxCol = nStartCol;
                    }
                }

                nStartCell += dataList.Count;
            }

            public SubpackageInfo(string name)
            {
                tagName = name;
            }

        }




        [Serializable]
        private class PieExcelListClass
        {
            public List<SubpackageInfo> list;

            public PieExcelListClass(List<SubpackageInfo> list)
            {
                this.list = list;
            }
        }
    }
}
