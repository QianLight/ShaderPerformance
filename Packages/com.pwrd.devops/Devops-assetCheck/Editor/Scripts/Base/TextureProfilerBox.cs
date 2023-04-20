using AssetCheck;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;

public class TextureProfilerBox : MonoBehaviour
{
    Dictionary<string, TextureData> dicCacheProfilerDatas = new Dictionary<string, TextureData>();

    static TextureProfilerBox instance;

    public static TextureProfilerBox Instance()
    {
        if (instance == null)
        {
            instance = new TextureProfilerBox();
            instance.InitContext();
        }
        return instance;
    }

    void InitContext()
    {
      
    }

    public void AddCheckTask(string path, Action<bool,string> callback)
    {
        TextureData texture = new TextureData();

        string md5 = md5file(path);
        texture.md5 = md5;
        texture.path = path;
        if (dicCacheProfilerDatas.ContainsKey(md5))
        {
            callback?.Invoke(false, path);
        }
        else {
            dicCacheProfilerDatas[md5] = texture;
            callback?.Invoke(true, path);
        }
    }

    public string md5file(string file)
    {
        try
        {
            FileStream fs = new FileStream(file, FileMode.Open);
            MD5 md5 = new MD5CryptoServiceProvider();
            byte[] retVal = md5.ComputeHash(fs);
            fs.Close();

            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < retVal.Length; i++)
            {
                sb.Append(retVal[i].ToString("x2"));
            }
            return sb.ToString();
        }
        catch (Exception ex)
        {
            throw new Exception("md5file() fail, error:" + ex.Message);
        }
    }
}
