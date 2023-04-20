using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEditor;
using EcsData;
using CFUtilPoolLib;
using TDTools;
using FBXmessage_UKI;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace TDTools
{
    public class FBXCompairAnim : StringClass
    {
        private static List<string> motionName = new List<string>();
        private static string path = $"{Application.dataPath}/Creatures/Monster_Alvida";
        public static string suff = "FBX";
        public static List<string> SubPathList;
        public static string SumFbXaddAin;
        public static List<string> SumFbXaddAinList = new List<string>();
        public static int a = 0;
        public static List<fileType> FBXAnimNameList = new List<fileType>();
        public static List<fileType> SkillAnimNameList = new List<fileType>();

        [MenuItem("Tools/TDTools/通用工具/动画文件对比工具")]
        static void Start()
        {
            DoExporter_ani();
            resetList(motionName, ref SkillAnimNameList);
            a += 1;
            SubPathList = new List<string>();
            SumFbXaddAin = "";
            string[] strs = Selection.assetGUIDs;
            GetFiles(path);
            StringAssets();
            StringtoList(SumFbXaddAin,ref SumFbXaddAinList);
            resetList(SumFbXaddAinList, ref FBXAnimNameList);
            CompairList(SkillAnimNameList, FBXAnimNameList);
        }

        public static void CompairList(List<fileType> SkillAnimNameList,List<fileType> FBXAnimNameList)
        {
            var exp1 = SkillAnimNameList.Where(a => FBXAnimNameList.Exists(t => a.AnimName.Contains(t.AnimName)))
                .ToList();
            var exp2 = FBXAnimNameList.Where(a => !SkillAnimNameList.Exists(t => a.AnimName.Contains(t.AnimName)))
                .ToList();
            AddTxtTextByFileInfo(exp2,"FBX文件里有技能脚本里没有的Anim文件");
        }
        public static void StringtoList(string fileName, ref List<string> fileNameList)
        {
            fileNameList = fileName.Split('\n').ToList();
        }

        public static void resetList(List<string> targetList, ref List<fileType> newTargetList)
        {
            foreach (string name in targetList)
            {
                newTargetList.Add(new fileType() {FBXorSkillName = name.Split(':').First(), AnimName = name.Split(':').Last()});
            }
        }
        public static void DoExporter_ani()
        {
            string skillPath = $"{Application.dataPath}/BundleRes/SkillPackage/Monster_Alvida";
            var files = Directory.GetFiles(skillPath, "*.bytes", SearchOption.AllDirectories);
            var skillEditor = EditorWindow.GetWindow<SkillEditor>();
            var skillGraph = skillEditor.CurrentGraph as SkillGraph;
            foreach (var file in files)
            {
                try
                {
                    string filename = file.Split('/').Last();
                    filename = filename.Split('\\').Last();
                    filename = filename.Split('.')[0];
                    skillGraph.OpenData(file);
                    SkillGraphChangeTemp.DoChange_ani(skillGraph, filename, ref motionName);
                }
                catch
                {
                    Debug.Log(skillGraph.configData.Name);
                }
            }
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
                        if (mSubPath == null || mDirectoryInfos == null)
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
            //AddTxtTextByFileInfo(SumFbXaddAin);
        }
        public static void AddTxtTextByFileInfo(List<fileType> fileNameList,string txtName)
        {
            string savePath = $"{Application.dataPath}/BundleRes/SkillPackage/Save";
            if (!System.IO.Directory.Exists(savePath))
            {
                Directory.CreateDirectory(savePath);
            }

            TextWriter writer = new StreamWriter(@savePath + '/' + txtName + ".txt");
            foreach (var name in fileNameList)
            {
                writer.WriteLine(name.FBXorSkillName + " " + name.AnimName + "\n");
            }
            writer.Close();
            //string path = Path.Combine(Application.persistentDataPath, "FiltersaveFile" + a);
            //Debug.Log(path);
            //StreamWriter sw;
            //FileInfo fi = new FileInfo(path);

            //if (!File.Exists(path))
            //{
            //    sw = fi.CreateText();
            //}
            //else
            //{
            //    sw = fi.AppendText(); //在原文件后面追加内容      
            //}

            //sw.WriteLine(txtText);
            //sw.Close();
            //sw.Dispose();
        }
    }

    public class StringClass
    {
        public static string MyDirectoryInfo(DirectoryInfo directoryInfo)
        {
            string mSystemPath = directoryInfo.ToString();
            int mPathIndex = mSystemPath.LastIndexOf(@"Assets\");
            string mSubPath = mSystemPath.Substring(mPathIndex, mSystemPath.Length - mSystemPath.LastIndexOf(@"Assets\"));
            mSubPath = mSubPath.Replace('\\', '/');
            return mSubPath;
        }

        public static void MyFileInfo(FileInfo[] mFileInfos, string suff, ref List<string> SubPathList)
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

    public class fileType
    {
        public string FBXorSkillName;

        public string AnimName;

    }
}