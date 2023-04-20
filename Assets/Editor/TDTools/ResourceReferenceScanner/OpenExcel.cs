using System.Diagnostics;
using UnityEngine;

namespace TDTools {
    public class OpenExcel {
        public static void OpenWps(string path, string name, string freezeRange, string selectRow = null, string selectValue = null) {
            string exePath = $@"{Application.dataPath}\Editor\TDTools\OpenWpsHelper\OpenWpsNew.exe".Replace("/", "\\");
            ProcessStartInfo startInfo = new ProcessStartInfo($"{exePath}", $"wps {path} {name} {freezeRange} A{selectRow}");
            startInfo.CreateNoWindow = true;
            startInfo.UseShellExecute = false;
            startInfo.RedirectStandardOutput = true;
            Process.Start(startInfo);
        }

        public static void OpenOffice(string path, string name, string freezeRange, string selectRow = null, string selectValue = null) {
            string exePath = $@"{Application.dataPath}\Editor\TDTools\OpenWpsHelper\OpenWpsNew.exe".Replace("/", "\\");
            //string exePath = @"C:\Project\tools\OpenWps\OpenWpsNew\bin\Release\net6.0\OpenWpsNew.exe";
            ProcessStartInfo startInfo = new ProcessStartInfo($"{exePath}", $"office {path} {name} {freezeRange} A{selectRow}");
            startInfo.CreateNoWindow = true;
            startInfo.UseShellExecute = false;
            startInfo.RedirectStandardOutput = true;
            Process.Start(startInfo);
        }
    }
}