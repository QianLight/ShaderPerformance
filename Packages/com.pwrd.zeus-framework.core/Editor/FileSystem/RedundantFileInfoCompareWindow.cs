/*******************************************************************
* Copyright © 2017—2021 Perfect World.Co.Ltd. All rights reserved.
* author: 移动项目支持部 ZEUS FRAMEWORK TEAM
********************************************************************/
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;

namespace Zeus.Core.FileSystem
{
    public class RedundantFileInfoCompareWindow : EditorWindow
    {

        string path1 = string.Empty;
        RedundantFileCheckSumInfo checkSumInfo1;

        string path2 = string.Empty;
        RedundantFileCheckSumInfo checkSumInfo2;

        Vector2 vec = Vector2.zero;
        bool showCompare = false;
        int compareType = 0;
        string[] displayedOptions = new string[] { "校验值不同", "文件1独有", "文件2独有", "校验值相同" };
        Dictionary<int, List<List<string>>> compareResult;
        List<string> emptyList = new List<string>() { "", "", "", "" };
        int curPage = 1;
        int maxPage = 1;
        int jumpPage = 1;
        int onePageCount = 100;

        private float Width
        {
            get
            {
                return this.position.width - 30;
            }
        }

        [MenuItem("Zeus/FileSystem/RedundantFileInfoCompareTool", false, 21)]
        public static void Open()
        {
            RedundantFileInfoCompareWindow window = GetWindow<RedundantFileInfoCompareWindow>();
            window.titleContent = new GUIContent("RedundantFileInfoCompareTool", "RedundantFileInfoCompareTool");
            window.Show();
        }

        private void OnGUI()
        {
            GUILayout.Space(10);
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("文件1路径：", GUILayout.Width(70));
            string newPath = EditorGUILayout.TextField(path1);
            if (!newPath.Equals(path1))
            {
                path1 = newPath;
                showCompare = false;
            }
            if (GUILayout.Button("选择", GUILayout.Width(75)))
            {
                newPath = EditorUtility.OpenFilePanel("SelectRedundantFileInfo", path1, "");
                if (!newPath.Equals(path1))
                {
                    path1 = newPath;
                    showCompare = false;
                }
            }
            EditorGUILayout.EndHorizontal();


            GUILayout.Space(10);
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("文件2路径：", GUILayout.Width(70));
            newPath = EditorGUILayout.TextField(path2);
            if (!newPath.Equals(path2))
            {
                path2 = newPath;
                showCompare = false;
            }
            if (GUILayout.Button("选择", GUILayout.Width(75)))
            {
                newPath = EditorUtility.OpenFilePanel("SelectRedundantFileInfo", path2, "");
                if (!newPath.Equals(path2))
                {
                    path2 = newPath;
                    showCompare = false;
                }
            }
            EditorGUILayout.EndHorizontal();

            if (File.Exists(path1) && File.Exists(path2))
            {
                if (GUILayout.Button("对比差异"))
                {
                    checkSumInfo1 = RedundantFileCheckSumInfo.EditorLoadFromFile(path1);
                    checkSumInfo2 = RedundantFileCheckSumInfo.EditorLoadFromFile(path2);
                    if (checkSumInfo1 != null && checkSumInfo2 != null)
                    {
                        curPage = 1;
                        showCompare = true;
                        compareResult = new Dictionary<int, List<List<string>>>();
                        compareResult.Add(compareType, GenerateCompareResult(compareType, checkSumInfo1, checkSumInfo2));
                        maxPage = compareResult[compareType].Count / onePageCount;
                        if (compareResult[compareType].Count % onePageCount > 0)
                        {
                            maxPage++;
                        }
                    }
                }
                if (showCompare)
                {
                    GUILayout.Space(10);
                    int newType = EditorGUILayout.Popup(compareType, displayedOptions, GUILayout.Width(100));
                    if (newType != compareType)
                    {
                        compareType = newType;
                        if (!compareResult.ContainsKey(compareType))
                        {
                            compareResult.Add(compareType, GenerateCompareResult(compareType, checkSumInfo1, checkSumInfo2));
                        }
                        curPage = 1;
                        maxPage = compareResult[compareType].Count / onePageCount;
                        if (compareResult[compareType].Count % onePageCount > 0)
                        {
                            maxPage++;
                        }
                    }

                    if (compareResult == null || !compareResult.ContainsKey(compareType) || compareResult[compareType].Count == 0)
                    {
                        EditorGUILayout.TextField("无");
                    }
                    else
                    {
                        if (compareType == 0)
                        {
                            EditorGUILayout.BeginHorizontal();
                            GUILayout.Space(Width * 0.025f);
                            GUILayout.Label("资源", GUILayout.Width(Width * 0.3f));
                            GUILayout.Space(Width * 0.025f);
                            GUILayout.Label("文件1对应校验值", GUILayout.Width(Width * 0.3f));
                            GUILayout.Space(Width * 0.025f);
                            GUILayout.Label("文件2对应校验值", GUILayout.Width(Width * 0.3f));
                            GUILayout.Space(Width * 0.025f);
                            EditorGUILayout.EndHorizontal();
                        }
                        using (var scr = new EditorGUILayout.ScrollViewScope(vec))
                        {
                            vec = scr.scrollPosition;
                            for (int i = (curPage - 1) * onePageCount; i < curPage * onePageCount; i++)
                            {
                                List<List<string>> list = compareResult[compareType];
                                if (i < list.Count)
                                {
                                    ShowResult(compareType, list[i]);
                                }
                            }
                        }
                        GUILayout.Space(10);

                        EditorGUILayout.BeginHorizontal();
                        EditorGUILayout.LabelField("");
                        if (GUILayout.Button("上一页") && curPage > 1)
                        {
                            curPage--;
                            vec = Vector2.zero;
                        }
                        EditorGUILayout.LabelField(curPage + " / " + maxPage, GUILayout.Width(100));
                        if (GUILayout.Button("下一页") && curPage < maxPage)
                        {
                            curPage++;
                            vec = Vector2.zero;
                        }

                        jumpPage = EditorGUILayout.IntField(jumpPage, GUILayout.Width(100));
                        if (GUILayout.Button("Go"))
                        {
                            if (jumpPage > 0 && jumpPage <= maxPage)
                            {
                                curPage = jumpPage;
                                vec = Vector2.zero;
                            }
                        }
                        EditorGUILayout.EndHorizontal();
                        GUILayout.Space(10);
                    }
                }
            }
        }

        private void ShowProcessBar(string info, float f)
        {
            EditorUtility.DisplayProgressBar("正在对比中...", info, f);
        }

        private List<List<string>> GenerateCompareResult(int compareType, RedundantFileCheckSumInfo info1, RedundantFileCheckSumInfo info2)
        {
            var result = new List<List<string>>();
            //校验值不同
            if (compareType == 0)
            {
                float all = info1.InfoDic.Count;
                int cur = 0;
                foreach (var item in info1.InfoDic)
                {
                    cur++;
                    ShowProcessBar(item.Key, cur / all);
                    string checksum2;
                    if (info2.InfoDic.TryGetValue(item.Key, out checksum2))
                    {
                        if (!checksum2.Equals(item.Value))
                        {
                            List<string> list = new List<string>();
                            result.Add(list);
                            list.Add(item.Key);
                            list.Add(item.Value);
                            list.Add(checksum2);
                        }
                    }
                }
            }
            //文件1独有
            else if (compareType == 1)
            {
                float all = info1.InfoDic.Count;
                int cur = 0;
                foreach (var item in info1.InfoDic)
                {
                    cur++;
                    ShowProcessBar(item.Key, cur / all);
                    if (!info2.InfoDic.ContainsKey(item.Key))
                    {
                        List<string> list = new List<string>();
                        result.Add(list);
                        list.Add(item.Key);
                        list.Add(item.Value);
                    }
                }
            }
            //文件2独有
            else if (compareType == 2)
            {
                float all = info2.InfoDic.Count;
                int cur = 0;
                foreach (var item in info2.InfoDic)
                {
                    cur++;
                    ShowProcessBar(item.Key, cur / all);
                    if (!info1.InfoDic.ContainsKey(item.Key))
                    {
                        List<string> list = new List<string>();
                        result.Add(list);
                        list.Add(item.Key);
                        list.Add(item.Value);
                    }
                }
            }
            //校验值相同
            else if (compareType == 3)
            {
                float all = info1.InfoDic.Count;
                int cur = 0;
                foreach (var item in info1.InfoDic)
                {
                    cur++;
                    ShowProcessBar(item.Key, cur / all);
                    string checksum2;
                    if (info2.InfoDic.TryGetValue(item.Key, out checksum2))
                    {
                        if (checksum2.Equals(item.Value))
                        {
                            List<string> list = new List<string>();
                            result.Add(list);
                            list.Add(item.Key);
                            list.Add(item.Value);
                        }
                    }
                }
            }
            result.Sort((a, b) => { return a[0].CompareTo(b[0]); });
            EditorUtility.ClearProgressBar();
            return result;
        }


        private void ShowResult(int compareType, List<string> result)
        {
            //校验值不同
            if (compareType == 0)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Space(Width * 0.025f);
                GUILayout.TextField(result[0], GUILayout.Width(Width * 0.3f));
                GUILayout.Space(Width * 0.025f);
                GUILayout.TextField(result[1], GUILayout.Width(Width * 0.3f));
                GUILayout.Space(Width * 0.025f);
                GUILayout.TextField(result[2], GUILayout.Width(Width * 0.3f));
                GUILayout.Space(Width * 0.025f);
                GUILayout.EndHorizontal();
            }
            //         1:文件1独有        2:文件2独有       3:校验值相同
            else if (compareType == 1 || compareType == 2 || compareType == 3)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Space(Width * 0.033f);
                GUILayout.TextField(result[0], GUILayout.Width(Width * 0.45f));
                GUILayout.Space(Width * 0.033f);
                GUILayout.TextField(result[1], GUILayout.Width(Width * 0.45f));
                GUILayout.Space(Width * 0.033f);
                GUILayout.EndHorizontal();
            }
        }
    }
}
