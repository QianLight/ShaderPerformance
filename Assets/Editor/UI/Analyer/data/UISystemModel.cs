using System;
using System.Diagnostics;
using UnityEditor;
using UnityEngine;
using System.Text;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace UIAnalyer
{
    public class UISystemModel
    {
        public static string BYTES_PATH = "SystemDefineTable.bytes";

        public static string FILE_PATH =  Application.dataPath + "/Table/SystemDefine.txt";

        public static string FILE_SAVE_PATH = Application.dataPath + "/Table/SystemDefineDemo.txt";
        public static string field_name_str;
        public static string field_desc_str;

 

        private UISystemFolderInfo m_folder = null;

        public static int CurrentLastIndex = 0;

        public UISystemFolderInfo folder{get{return m_folder;}}
        public void InitUISourceInfo(){ 
            if(m_folder == null){
                m_folder = XDataPool<UISystemFolderInfo>.GetData();
                m_folder.Setup(null, null);     
            }  
            Clear();
            SystemDefineTable tableData=  ConvertSystemDefine();
            for(int i = 0 ;i < tableData.tables.Count;i++){
                m_folder.AddFolder<UISystemFolderInfo>(tableData.tables[i]);
            }
        }

    

        public void SaveSourceInfo(){
            SaveFileStream();
            // if(m_folder == null) return;
            // List<SystemDefineData> list = new List<SystemDefineData>();
            // ReadSystemDefineData(m_folder , ref list);
            // string[] contents = new string[list.Count +2];
            // contents[0] = field_name_str;
            // contents[1] = field_desc_str;

            // for(int i = 0; i < list.Count; i++){
            //     contents[i+2] = list[i].ToString();
            //     UnityEngine.Debug.Log(contents[i+2]);
            // }
            // File.WriteAllLines(@FILE_SAVE_PATH, contents , Encoding.UTF8);
        }

        public void SaveFileStream(){
            if(m_folder == null) return;
            StreamWriter file = new StreamWriter( FILE_SAVE_PATH, false, Encoding.UTF8);  
                List<SystemDefineData> list = new List<SystemDefineData>();
            ReadSystemDefineData(m_folder , ref list);
            file.WriteLine(field_name_str);
            file.WriteLine(field_desc_str);
            for(int i = 0; i < list.Count; i++){
                file.WriteLine(list[i].ToString());
                // UnityEngine.Debug.Log(list[i]);
            }    
            file.Flush();
            file.Close();
        }

        public void ReadSystemDefineData(UISystemFolderInfo folder, ref List<SystemDefineData> list ){
            if(folder.data != null){
                list.Add(folder.data);
            }

            if(folder.m_children.Count > 0){
                foreach(var folderInfo in folder.m_children){
                    ReadSystemDefineData(folderInfo.Value as UISystemFolderInfo ,ref list);
                }
            }
        }


        public void Clear()
        {
            if(m_folder != null){
                m_folder.Clear();
            }
        }

#region  Editor

        public static SystemDefineTable ConvertSystemDefine(){
            string path = FILE_SAVE_PATH;
            if (!File.Exists(path))
            {
                UnityEngine.Debug.LogError("not found file = " + path);
                path = FILE_PATH;            
            }
            
            IEnumerable<string> data = File.ReadAllLines(path, Encoding.UTF8);
            IEnumerator<string> table = data.GetEnumerator();

            string[] reader = null;
            string line = string.Empty;
            CurrentLastIndex = 0;
            SystemDefineData tableObj = null;
            int index = 0;
            SystemDefineTable systemTable = new SystemDefineTable();
            while (table.MoveNext())
            {
                line = table.Current;
                reader = line.Split('\t');
                if (index == 0)
                {
                    SystemDefineData.tabStrs = reader;
                    field_name_str = @line;
                }else if(index == 1)
                {
                    SystemDefineData.filedTypes = reader;
                    field_desc_str = @line;
                }
                else
                if (index >= 2)
                {
                    tableObj = new SystemDefineData( SystemDefineData.tabStrs.Length);
                    CurrentLastIndex = index;
                    tableObj.id = index;
                    for (int i = 0; i <  SystemDefineData.tabStrs.Length; i++)
                    {
                        try
                        {
                            tableObj.SetPublicField(tableObj,i, reader[i]);
                            if(!string.IsNullOrEmpty(reader[i])&& string.IsNullOrEmpty(tableObj.displayName)){
                                tableObj.displayName = reader[i];
                            }
                        }
                        catch (Exception)
                        {
                            UnityEngine.Debug.Log("Convert Error!" +  SystemDefineData.tabStrs[i] + " : " + index + " : " + reader[i]);
                        }
                    }
            
                    systemTable.tables.Add(tableObj);
                }
                index++;
            }
            return systemTable;
        }


        [MenuItem("Tools/UI/Convert SystemDefine To Assets")]
        public static void ConvertTxtToAssets()
        {
            SystemDefineTable systemTable = ConvertSystemDefine();
            if(systemTable == null) return;           
            DataIO.SerializeData<SystemDefineTable>(BYTES_PATH, systemTable);
            UnityEngine.Debug.Log("Convert Success!" + systemTable.tables.Count);
        } 
    }
    #endregion
}