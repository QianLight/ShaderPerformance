using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using TableEditor;

namespace TDTools
{
    public class DataStructCreator
    {
        public static string asset_path;
        public static string table_path;
        public static string skill_path;
        public static string hit_path;
        public static string react_path;
        public static string level_path;
        public static string effects_path;
        public static List<string> skill_files;
        public static List<string> level_files;

        static DataStructCreator() //不允许从可编辑脚本中调用get_dataPath。不声明成静态的太卡了
        {
            asset_path = Application.dataPath;
            table_path = string.Concat(asset_path, "/Table/");
            skill_path = string.Concat(asset_path, "/BundleRes/SkillPackage/");
            hit_path = string.Concat(asset_path, "/BundleRes/HitPackage/");
            react_path = string.Concat(asset_path, "/BundleRes/ReactPackage/");
            level_path = string.Concat(asset_path, "/BundleRes/Table/");
            effects_path = string.Concat(asset_path, "/BundleRes/Effects/Prefabs/");
            skill_files = new List<string>();
            GetTargetSource(skill_path, ".bytes", ref skill_files);
            level_files = new List<string>();
            GetTargetSource(level_path, ".cfg", ref level_files);
        }

        static List<string> GetTargetSource(string FilePath, string FileType,ref List<string> Container)   //写了个递归，应该是好代码
        {
            DirectoryInfo directoryInfo = new DirectoryInfo(FilePath);
            DirectoryInfo[] subdirectories = directoryInfo.GetDirectories();
            if (subdirectories.Length == 0)
            {
                foreach (FileInfo file in directoryInfo.GetFiles())
                {
                    if (file.Extension == FileType)
                    {
                        Container.Add(file.Name);
                    }
                }               
            }
            else
            {
                for (int i = 0; i< subdirectories.Length;i++ )
                {
                    GetTargetSource(subdirectories[i].FullName, FileType, ref Container);
                }            
            }
            return Container;
        }

        public List<string> GetTableSpecificColumnContent(string TableName, string RowName)
        {
            TableFullData tableData = new TableFullData();
            TableEditor.TableReader.ReadTableByFileStream(table_path + TableName + ".txt", ref tableData);
            int RowIndex = tableData.nameList.FindIndex(n => n == RowName);
            if (RowIndex != -1)
            {
                List<string> SpRowContent = new List<string>();
                for (int i = 0; i < tableData.dataList.Count; i++)
                {
                    SpRowContent.Add(tableData.dataList.Find(n => n.index == i).valueList[RowIndex]);
                }
                return SpRowContent;
            }
            else
            {
                Debug.Log($"{TableName}的检查中，输入了不存在的列{RowName}");
                return null;
            }
        }
    }
}