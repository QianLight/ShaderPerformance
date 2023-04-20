using System.Collections;
using System.Collections.Generic;
using System.IO;
using B;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using UnityEditor;
using UnityEngine;

namespace CFEngine.Quantify
{
    public partial class PieGameExcel
    {


        private static string Excel_Functions_Tag = "Excel_Functions_Tag";

        public static string Excel_Functions
        {
            get { return PlayerPrefs.GetString(Excel_Functions_Tag); }
            set { PlayerPrefs.SetString(Excel_Functions_Tag, value); }
        }

        private  List<string> m_FunctionKeys = new List<string>();
        private List<ClassGameModuleShow> _GameModuleList = new List<ClassGameModuleShow>();
        private ClassGameModuleShow _GameModuleOther;
        
        private  List<string> m_FunctionFilterKeys = new List<string>();

        private void InitConfigData_Functions()
        {
            m_FunctionKeys.Clear();

            m_FunctionKeys.Add("Name");
            m_FunctionKeys.Add("Total Time");
            m_FunctionKeys.Add("Count Total");

            m_FunctionFilterKeys = new List<string>();
            m_FunctionFilterKeys.Add("Update");

            _GameModuleList = new List<ClassGameModuleShow>();
            _GameModuleList.Add(new ClassGameModuleShow(new string[] {"universal","volume","renderpipelines"}, "URP"));
            _GameModuleList.Add(new ClassGameModuleShow(new string[] {"impostor","avoidblock"}, "Impostor"));
            _GameModuleList.Add(new ClassGameModuleShow(new string[] {"probe","meshrender","grass","water"}, "场景"));
            _GameModuleList.Add(new ClassGameModuleShow(new string[] {"cinemachine","controller","camera"}, "相机"));
            _GameModuleList.Add(new ClassGameModuleShow(new string[] {"scroll", "rect","canvas","ui"}, "UI"));
            _GameModuleList.Add(new ClassGameModuleShow(new string[] {"ecs"}, "Ecs"));
            _GameModuleList.Add(new ClassGameModuleShow(new string[] {"sfx","particle","effect"}, "特效"));
            _GameModuleList.Add(new ClassGameModuleShow(new string[] {"network","connection"}, "网络"));
            _GameModuleList.Add(new ClassGameModuleShow(new string[] {"fmod","audio"}, "音效"));
            _GameModuleList.Add(new ClassGameModuleShow(new string[] {"lua"}, "Lua"));
            _GameModuleList.Add(new ClassGameModuleShow(new string[] {"anim","skin"}, "动画 skin"));
            _GameModuleList.Add(new ClassGameModuleShow(new string[] {"physics"}, "物理"));
            _GameModuleList.Add(new ClassGameModuleShow(new string[] {"input","touch","event"}, "输入"));
            _GameModuleList.Add(new ClassGameModuleShow(new string[] {"zeus"}, "Ab加载"));
            _GameModuleList.Add(new ClassGameModuleShow(new string[] {"sdk"}, "sdk"));
            _GameModuleList.Add(new ClassGameModuleShow(new string[] {"timeline"}, "timeline"));
            _GameModuleList.Add(new ClassGameModuleShow(new string[] {"entity"}, "Entity"));
            _GameModuleList.Add(new ClassGameModuleShow(new string[] {"cfclient"}, "CFClient"));
            _GameModuleList.Add(new ClassGameModuleShow(new string[] {"cfengine"}, "CFEngine"));
            _GameModuleList.Add(new ClassGameModuleShow(new string[] {"cfutilpoollib"}, "CFUtilPoolLib"));
            _GameModuleList.Add(new ClassGameModuleShow(new string[] {"assembly-csharp"}, "Assembly-CSharp"));
            _GameModuleList.Add(new ClassGameModuleShow(new string[] {"earlyupdate"}, "EarlyUpdate"));
            _GameModuleList.Add(new ClassGameModuleShow(new string[] {"prelateupdate"}, "PreLateUpdate"));
            _GameModuleList.Add(new ClassGameModuleShow(new string[] {"lateupdate"}, "LateUpdate"));
            _GameModuleList.Add(new ClassGameModuleShow(new string[] {"postlateupdate"}, "PostLateUpdate"));
            _GameModuleList.Add(new ClassGameModuleShow(new string[] {"fixedupdate"}, "FixedUpdate"));
            _GameModuleList.Add(new ClassGameModuleShow(new string[] {"preupdate"}, "PreUpdate"));
            _GameModuleList.Add(new ClassGameModuleShow(new string[] {"update"}, "Update"));
            
            _GameModuleOther = new ClassGameModuleShow(new string[] {"other"}, "未分类");
            
        }

        private Dictionary<ClassGameModuleShow, List<string[]>> GetShowDic()
        {
            Dictionary<ClassGameModuleShow, List<string[]>> dic = new Dictionary<ClassGameModuleShow, List<string[]>>();

            for (int i = 0; i < _GameModuleList.Count; i++)
            {
                dic.Add(_GameModuleList[i], new List<string[]>());
            }

            dic.Add(_GameModuleOther, new List<string[]>());
            return dic;
        }

        public class ClassGameModuleShow
        {
            public List<string> allTags = new List<string>();
            public string showDes;

            public ClassGameModuleShow(string[] values, string showTag)
            {
                allTags.AddRange(values);
                showDes = showTag;
            }

            public bool CheckIn(string tag)
            {
                string lowTag = tag.ToLower();

                for (int i = 0; i < allTags.Count; i++)
                {
                    if (lowTag.Contains(allTags[i]))
                        return true;
                }
                return false;
            }
            
        }

        private ClassGameModuleShow Function_GetGameModule(string strData)
        {
            for (int i = 0; i < _GameModuleList.Count; i++)
            {
                ClassGameModuleShow module = _GameModuleList[i];

                if (module.CheckIn(strData))
                    return module;
            }
            
            return _GameModuleOther;
        }

        private bool Function_Filter(string strData)
        {
            for (int i = 0; i < m_FunctionFilterKeys.Count; i++)
            {
                if (strData.Contains(m_FunctionFilterKeys[i]))
                    return true;
            }
            return false;
        }

        void OnGUI_Functions()
        {
            GUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("函数_预览路径:" + Excel_Functions, new GUILayoutOption[] {GUILayout.Width(450f)});
            if (GUILayout.Button("函数_预览", EditorStyles.miniButton, GUILayout.Width(100)))
            {
                string path = EditorUtility.OpenFilePanel("选择需要显示的数据", Excel_Functions, "csv");
                if (!string.IsNullOrEmpty(path))
                {
                    Function_Xlsx(path);
                    Excel_Functions = path;
                }
            }

            GUILayout.EndHorizontal();
        }

        private void Function_Xlsx(string path)
        {
            List<string[]> _tables = CsvTool.LoadFile(path);

            string[] allKeys = _tables[0];
            List<int> dataIndex = new List<int>();

            foreach (var keyShow in m_FunctionKeys)
            {
                for (int i = 0; i < allKeys.Length; i++)
                {
                    string strItm = allKeys[i];
                    if (strItm.Contains(keyShow))
                    {
                        dataIndex.Add(i);
                        break;
                    }
                }
            }

            Dictionary<ClassGameModuleShow, List<string[]>> showDic = GetShowDic();
            
            for (int i = 1; i < _tables.Count; i++)
            {
                string[] datas = _tables[i];

                string functionLine = datas[dataIndex[0]];
                if (!Function_Filter(functionLine)) continue;

                ClassGameModuleShow module = Function_GetGameModule(functionLine);

                List<string[]> tmpList = new List<string[]>();
                if (!showDic.TryGetValue(module, out tmpList))
                {
                    tmpList = new List<string[]>();
                    showDic.Add(module, tmpList);
                }
                tmpList.Add(datas);
            }
            

            string fileName=path.Substring(path.LastIndexOf("/"));

            string targetFileName = "60帧_Cpu优化";
            
            string filePath = path.Replace(fileName,"/"+targetFileName+".xlsx");
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }

            Debug.Log("Function_Xlsx xlsx:" + filePath);
            
            FileInfo fileInfo = new FileInfo(filePath);
            using (ExcelPackage excelPackage = new ExcelPackage(fileInfo))
            {

                ExcelWorksheet worksheet = excelPackage.Workbook.Worksheets.Add(targetFileName);
                worksheet.DefaultColWidth = 20;

                int nStartRow = 1;
                ExcelRange rangeEmpty0 = worksheet.Cells[nStartRow, 1, nStartRow, 100];
                rangeEmpty0.Style.Fill.PatternType = ExcelFillStyle.Solid;
                rangeEmpty0.Style.Fill.BackgroundColor.SetColor(178,255,0,0);
                
                worksheet.Cells[nStartRow, 1].Value = "模块";
                worksheet.Cells[nStartRow, 2].Value = "函数路径";
                worksheet.Cells[nStartRow, 3].Value = "总时间";
                worksheet.Cells[nStartRow, 4].Value = "总次数";
                worksheet.Cells[nStartRow, 5].Value = "是否能降帧优化";
                worksheet.Cells[nStartRow, 6].Value = "负责人";

                nStartRow++;
                foreach (KeyValuePair<ClassGameModuleShow, List<string[]>> itmData in showDic)
                {
                    List<string[]> allDatas = itmData.Value;
                    for (int i = 1; i < allDatas.Count; i++)
                    {
                        string[] datas = allDatas[i];
                        worksheet.Cells[nStartRow, 1].Value = itmData.Key.showDes;

                        for (int j = 0; j < dataIndex.Count; j++)
                        {
                            ExcelRange range = worksheet.Cells[nStartRow, 2 + j];
                            range.Value = datas[dataIndex[j]];
                            if (j == 0)
                            {
                                range.AutoFitColumns(120);
                                range.Style.WrapText = true;
                            }
                        }

                        nStartRow++;
                    }

                    ExcelRange rangeEmpty = worksheet.Cells[nStartRow, 1, nStartRow, 100];
                    rangeEmpty.Style.Fill.PatternType = ExcelFillStyle.Solid;
                    rangeEmpty.Style.Fill.BackgroundColor.SetColor(178,255,255,0);
                    rangeEmpty.Merge = true;

                    nStartRow++;
                }

                excelPackage.Save(); //写入后保存表格
            }
        }
        
    }
}

