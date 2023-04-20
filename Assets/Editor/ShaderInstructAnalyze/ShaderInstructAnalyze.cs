using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.IO;
using System;
using System.Runtime.Serialization.Formatters.Binary;

namespace ShaderInstructAnalyze
{ 
    
    public class ShaderInstructAnalyze : MonoBehaviour
    {
        ShaderAndAnalyeResult shaderAndAnalyeResult;
        void Start()
        {
            string shaderFolderPath = Application.dataPath + "/ShaderInstructAnalyze/Shaders/" + "ShaderLab/";
            
            /*
            // 1. Ԥ���������ļ�����ȫ��Դ�ļ���ÿ��ShaderLab��Ӧn��pass��ÿ��pass�ֶ�Ӧn�ֹؼ�����ϡ�
            shaderAndAnalyeResult = new ShaderAndAnalyeResult();
            shaderAndAnalyeResult.SetShaderlabFolder(Application.dataPath + "/ShaderInstructAnalyze/Shaders/");
            DirectoryInfo folder = new DirectoryInfo(shaderFolderPath);
            var files = folder.GetFiles("*.shader");
            for (int i = 0; i < files.Length; i++)
            {
                Debug.Log("Analye Start !!!!! file is : " + files[i].Name);
                shaderAndAnalyeResult.AddShaderlab(files[i].Name);
            }
            shaderAndAnalyeResult.SortVariants(CyclePathType.total);

            // 2. ���л�Ԥ������
            FileStream stream = new FileStream(shaderFolderPath + "shaderAndAnalyeResult.bin", FileMode.Create, FileAccess.Write, FileShare.None);
            BinaryFormatter formatter = new BinaryFormatter();
            formatter.Serialize(stream, shaderAndAnalyeResult);
            stream.Close();
            */

            // 3. �����л��Ͳ�ѯ
            FileStream fs = new FileStream(shaderFolderPath + "shaderAndAnalyeResult.bin", FileMode.Open);
            BinaryFormatter bf = new BinaryFormatter();
            shaderAndAnalyeResult = bf.Deserialize(fs) as ShaderAndAnalyeResult;
            fs.Close();

            string[] keywords = new string[] { "_ALPHATEST_ON", "_EMISSION" };
            AnalyzeResult[] analyzeResult = shaderAndAnalyeResult.GetAnalyzeResult("SimpleLit", "ForwardLit", keywords);
            string shortestPathBound = analyzeResult[1].GetShortestPathBound();
            analyzeResult[1].Show();

            //shaderAndAnalyeResult.ShowTopShaders(20);
            Debug.Log("shortest path bound is :" + shortestPathBound);
            Debug.Log("Total Cycle is :" + analyzeResult[1].GetTotalCycle(CyclePathType.total));
        }
    }
}

