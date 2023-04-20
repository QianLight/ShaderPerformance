using System.Diagnostics;
using System.Collections.Generic;
using System.Text;
using System.IO;
using UnityEngine;

namespace TDTools
{
    public class OpenWpsHelper
    {
        public string configPath = "";
        public static readonly string exePath = "Editor/TDTools/OpenWpsHelper/OpenWps.exe";
        public static WpsHelperConfig Config = new WpsHelperConfig();
        public static void OpenWps(string dir, string name, string freezeRange, string selectRow = null, string selectValue = null)
        {
            ProcessStartInfo startInfo = new ProcessStartInfo($"{Application.dataPath}/{exePath}", $"{dir} {name} {freezeRange}");
            startInfo.CreateNoWindow = true;
            startInfo.UseShellExecute = false;
            startInfo.RedirectStandardOutput = true;
            Process.Start(startInfo);
        }
    }
    public class WpsHelperConfig
    {
        private static readonly string defaultConfig = "Editor/TDTools/DesignerHelper/DefaultTableConfig.txt";
        public Dictionary<string, string> PathDic;
        public List<string> TableConfigList;

        public WpsHelperConfig()
        {
            Init($"{Application.dataPath}/{defaultConfig}");
        }
        public void Init(string configPath)
        {
            PathDic = new Dictionary<string, string>();
            TableConfigList = new List<string>();
            PathDic.Add("项目内", $"{Application.dataPath}/Table");
            using (StreamReader reader = new StreamReader(configPath, Encoding.GetEncoding("UTF-8")))
            {
                string line = string.Empty;
                while ((line = reader.ReadLine()) != null)
                {
                    try
                    {
                        var items = line.Split('\t');
                        string op = items[0];
                        string param = items[1];
                        switch (op)
                        {
                            case "--":
                                break;
                            case "D":
                                var ps = param.Split('=');
                                PathDic.Add(ps[0], ps[1]);
                                break;
                            case "T":
                                TableConfigList.Add(param);
                                break;
                        }
                    }
                    catch
                    {
                        UnityEngine.Debug.Log($"OpenWpsHelper.Config {line} 解析失败");
                    }
                }
            }
        }
    }
}
