using CFEngine.Editor;
using CFUtilPoolLib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;
using UnityEditorInternal;

public class XGuideTools
{
    [MenuItem("Tools/Guide/Skip Beginner")]
    public static void SkipBeginner(){
        string filePath = Application.dataPath + @"/beginner.txt";
        // Debug.Log("file:"+filePath);
        try
        {
            File.WriteAllBytes(filePath, XCryptography(string.Format("{0}", 0)));
            UnityEditor.EditorUtility.DisplayDialog("新手关卡","添加跳过新手关卡成功","确定");
        }
        catch (Exception)
        {
            UnityEditor.EditorUtility.DisplayDialog("新手关卡","添加跳过新手关卡文件失败!!!","确定");
        }
    }
    [MenuItem("Tools/Guide/Reset Beginner")]
    public static void ResetBeginner(){
        string filePath = Application.dataPath + @"/beginner.txt";
        if (!File.Exists(filePath)){
            UnityEditor.EditorUtility.DisplayDialog("新手关卡","新手关卡文件不存在","确定");
            return;
        }
        File.Delete(filePath);
        UnityEditor.EditorUtility.DisplayDialog("新手关卡","删除新手关卡文件成功","确定");
    }

    private static byte[] XCryptography(string content)
    {
        byte[] bs = Encoding.UTF8.GetBytes(content);
        for (int i = 0; i < bs.Length; i++)
        {
            bs[i] = (byte)(bs[i] ^ _seed);
        }
        return bs;
    }
    private static readonly byte _seed = 0x94;
}