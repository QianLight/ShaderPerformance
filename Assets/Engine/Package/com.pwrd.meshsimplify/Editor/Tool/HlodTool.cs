using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace Athena.MeshSimplify
{
    public class CombineSetting
    {
        public string                  inPath;//输入fbx路径   
        public string                  outFbxPath;//输出fbx路径
        public string                  outTexPath;//输出图片路径，最终会加上图片类型作为后缀（比如_albedo）
        public float                   percent;//减面比例
        public int                     screenSize;//屏幕大小（最大1200）
        public int                     uvQuality;//uv质量    0：默认（超过25k快速，低于25k质量） 1：快速 2：质量
        public int                     excuteOrder;//执行顺序   0：先合并再减面  1：先减面再合并    两种方式在时间上和最终效果上都有所不同
        public int                     texWidth;//图片输出宽度
        public int                     texHeight;//图片输出高度
        public int                     padding;//uv之间的缝隙
        public int                     lockBound;//0 不锁定边界 1 锁定边界
        public int                     minAngle;//三角形最小角度
        public int                     textureChannel;//需要处理哪些图片
        public float                   weldingThreshold;//融合阈值
        public bool                    useVoxel;//使用体素
        public bool                    useOcclusion;//使用可见性检测
        public List<Vector3>           cameraPos = new List<Vector3>();//自定义可见性检测观测点
    };
    
    public static class HlodTool
    {
        private static string scriptPath = Path.GetDirectoryName(new System.Diagnostics.StackTrace(true).GetFrame(0).GetFileName());

        public static GameObject Combine(CombineSetting setting)
        {
            try
            {
                Process process = new Process();
                
                process.StartInfo.FileName = scriptPath + "\\Plugins\\Hlod.exe";
                process.StartInfo.UseShellExecute = false;
                    
                process.StartInfo.CreateNoWindow = true;
                    
                process.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;

                StringBuilder sb = new StringBuilder();
                sb.Append("\"");
                if (setting.useVoxel)
                {
                    sb.Append("method|2;");
                }
                else
                {
                    sb.Append("method|1;");
                }

                sb.Append("inPath|" + setting.inPath + ";");
                sb.Append("outFbxPath|" + setting.outFbxPath + ";");
                sb.Append("outTexPath|" + setting.outTexPath + ";");
                sb.Append("percent|" + setting.percent + ";");
                sb.Append("screenSize|" + setting.screenSize + ";");
                sb.Append("texChannel|" + setting.textureChannel + ";");
                sb.Append("texWidth|" + setting.texWidth + ";");
                sb.Append("texHeight|" + setting.texHeight + ";");
                sb.Append("weldingThreshold|" + setting.weldingThreshold + ";");
                sb.Append("useOcclusion|" + (setting.useOcclusion ? 1 : 0) + ";");
                sb.Append("occlusionPoses|");
                for (int i = 0; i < setting.cameraPos.Count; i++)
                {
                    Vector3 p = setting.cameraPos[i];
                    sb.Append(p.x);
                    sb.Append(",");
                    sb.Append(p.y);
                    sb.Append(",");
                    sb.Append(p.z);
                    if(i < setting.cameraPos.Count - 1)
                        sb.Append(",");
                }

                sb.Append(";");
                sb.Append("debug|1;");
                sb.Append("\"");

                process.StartInfo.Arguments = sb.ToString();

                process.EnableRaisingEvents = true;

                process.StartInfo.RedirectStandardOutput = true;

                process.OutputDataReceived += ChangeOutput;
                
                process.Start();

                process.BeginOutputReadLine();

                process.WaitForExit();

                EditorUtility.ClearProgressBar();

                int ExitCode = process.ExitCode;

                Debug.Log("执行结束:" + ExitCode);

                process.Close();
                
                string fbxfileName = setting.outFbxPath.Substring(Application.dataPath.Length - 6);
                string texfileName = setting.outTexPath.Substring(Application.dataPath.Length - 6);
                AssetDatabase.ImportAsset(fbxfileName);
                //AssetDatabase.ImportAsset(texfileName);
                // AssetDatabase.Refresh();
                // return AssetDatabase.LoadAssetAtPath<GameObject>(fileName); //实际是mesh的Obj
            }
            catch (Exception e)
            {
                EditorUtility.ClearProgressBar();
                Debug.LogError(e.Message);
            }

            return null;
        }
        
        //输出重定向函数
        private static void ChangeOutput(object sendingProcess, DataReceivedEventArgs outLine)
        {
            if (!String.IsNullOrEmpty(outLine.Data)) //字符串不为空时
            {
                UnityEngine.Debug.Log(outLine.Data);//将进程的输出信息转移
            }
        }
    }
}