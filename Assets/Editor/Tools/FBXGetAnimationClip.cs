using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace FBXmessage_UKI
{
    public class FBXGetAnimationClip : StringClass
    {
        public static string suff = "FBX";
        public static List<string> SubPathList;
        public static string SumFbXaddAin;
        public static int a = 0;
        
        [MenuItem("Assets/统计工具/统计模型动画片段")]
        static void GetAss()
        {
            a += 1;
            SubPathList = new List<string>();
            SumFbXaddAin = "";
            string[] strs = Selection.assetGUIDs;
            string path = AssetDatabase.GUIDToAssetPath(strs[0]);
            //Debug.Log(path);
            GetFiles(path);
            StringAssets();
        }

        public static void GetFiles(string path)
        {
            if (Directory.Exists(path))
            {
                DirectoryInfo mDirectoryInfo = new DirectoryInfo(path);
                FileInfo[] mFileInfos = mDirectoryInfo.GetFiles();
                DirectoryInfo[] mDirectoryInfos = mDirectoryInfo.GetDirectories();
                MyFileInfo(mFileInfos, suff, ref SubPathList);
                //GetFiles();
                if (mDirectoryInfos != null)
                {
                    foreach (DirectoryInfo directoryInfo in mDirectoryInfos)
                    {
                        //SystemPath--->ProjectPath
                        string mSubPath = MyDirectoryInfo(directoryInfo);
                        if (mSubPath == null || mDirectoryInfos==null)
                        {
                            return;
                        }
                        else
                        {
                            GetFiles(mSubPath);
                        }
                    }
                }
            }

        }

        public static void StringAssets()
        {
            for (int i = 0; i < SubPathList.Count; i++)
            {
                Object[] sumassets = AssetDatabase.LoadAllAssetsAtPath(SubPathList[i]);
                GameObject gameObj = AssetDatabase.LoadAssetAtPath<GameObject>(SubPathList[i]);
                foreach (var sumasset in sumassets)
                {
                    if (sumasset is AnimationClip clip)
                    {
                        SumFbXaddAin += ($"{gameObj.name}:{clip.name}\n");
                    }
                }
                EditorUtility.UnloadUnusedAssetsImmediate();
                GC.Collect();
            }
            AddTxtTextByFileInfo(SumFbXaddAin);
        }
        public static void AddTxtTextByFileInfo(string txtText)
        {
            string path = Path.Combine(Application.persistentDataPath, "FiltersaveFile"+a);
            Debug.Log(path);
            StreamWriter sw;
            FileInfo fi = new FileInfo(path);

            if (!File.Exists(path))
            {
                sw = fi.CreateText();
            }
            else
            {
                sw = fi.AppendText(); //在原文件后面追加内容      
            }

            sw.WriteLine(txtText);
            sw.Close();
            sw.Dispose();
        }
    }

    public class StringClass
    {
        public static string MyDirectoryInfo(DirectoryInfo directoryInfo)
        {
            string mSystemPath = directoryInfo.ToString();
            int mPathIndex = mSystemPath.LastIndexOf(@"Assets\");
            string mSubPath = mSystemPath.Substring(mPathIndex, mSystemPath.Length - mSystemPath.LastIndexOf(@"Assets\"));
            mSubPath = mSubPath.Replace('\\','/');
            return mSubPath;
        }
        
        public static void MyFileInfo(FileInfo[] mFileInfos,string suff,ref List<string> SubPathList)
        {
            foreach (var f in mFileInfos)
            {
                string fileName = f.FullName;
                if (fileName.EndsWith(suff))
                {
                    int c = fileName.LastIndexOf(@"Assets\");
                    fileName = fileName.Substring(c, fileName.Length - fileName.LastIndexOf(@"Assets\"));
                    SubPathList.Add(fileName);
                }
            }
        }
    }
    
}
// string st = "/";
// st = st.Replace('/', '\\');
// Debug.Log(st);