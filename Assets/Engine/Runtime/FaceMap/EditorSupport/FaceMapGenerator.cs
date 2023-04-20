#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class FaceMapFrame
{
    public int angle;
    public Texture2D texture;
}

public class FaceMapGenerator : MonoBehaviour
{
    [Header("图片目录")]
    public string folder = "Assets/luffy";
    [Header("数据列表")]
    public List<FaceMapFrame> frames = new List<FaceMapFrame>();
    [Header("生成结果")]
    public Texture2D facemap;
}

#endif