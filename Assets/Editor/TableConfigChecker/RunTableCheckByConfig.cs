using System.IO;
using UnityEngine;
using System.Collections.Generic;

namespace TDTools
{
    public class RunTableCheckByConfig
    {
        static List<string> Configs = new List<string>();
        static BaseChecker BC = new BaseChecker();
        static void GetConfigFile()
        {
            try
            {
                string ConfigPath = string.Concat(Application.dataPath, "/Editor/TableConfigChecker/ConfigForCheck/") ;
                DirectoryInfo directoryInfo = new DirectoryInfo(ConfigPath);
                FileStream s;
                foreach (FileInfo file in directoryInfo.GetFiles())
                {
                    if(file.Extension == ".txt")
                    {
                        s = file.Open(FileMode.OpenOrCreate);
                        StreamReader sr = new StreamReader(s);
                        string line;
                        while ((line = sr.ReadLine()) != null)
                        {
                            Configs.Add(line);
                        }
                    }
                }
            }
            catch
            {
                Debug.LogError("获取信息失败，未能找到配置文件路径");
            }
        }

        public static void DoCheckByFile()
        {
            GetConfigFile();
            foreach(string s in Configs)
            {
                string[] ConfigInfo = s.Split('|');
                if(ConfigInfo[1] == "EmptyCheck")
                {
                    BaseChecker.EmptyCheck(ConfigInfo[0], ConfigInfo[2]);
                }
                else if (ConfigInfo[1] == "RepeatedCheck")
                {
                    BaseChecker.RepeatedCheck(ConfigInfo[0], ConfigInfo[2]);
                }
                else if (ConfigInfo[1] == "ForeignkeyCheck")
                {
                    BaseChecker.ForeignkeyCheck(ConfigInfo[0], ConfigInfo[2]);
                } 
            }
            Configs.Clear();
        }
    }
}
