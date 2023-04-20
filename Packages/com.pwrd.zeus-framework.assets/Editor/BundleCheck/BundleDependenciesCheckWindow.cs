/*******************************************************************
* Copyright © 2017—2021 Perfect World.Co.Ltd. All rights reserved.
* author: 移动项目支持部 ZEUS FRAMEWORK TEAM
********************************************************************/
using System;
using UnityEditor;
using UnityEngine;
using System.Linq;
using System.Collections.Generic;
using System.Text;

namespace Zeus.Framework.Asset
{
    public class BundleDependenciesCheckWindow : EditorWindow
    {
        [MenuItem("Zeus/Asset/Bundle Dependencies Check", false, 9)]
        static public void Open()
        {
            window = GetWindow<BundleDependenciesCheckWindow>(false, "Bundle Dependencies Check");
            AssetBundleUtils.ReInit();
        }

        static public BundleDependenciesCheckWindow window;

        private BundleDependenciesCheck_All allCheckUI = new BundleDependenciesCheck_All();
        private BundleDependenciesCheck_Filter filterCheckUI = new BundleDependenciesCheck_Filter();
        private BundleDependenciesCheck_Specify specifyCheckUI = new BundleDependenciesCheck_Specify();

        int tab = 0;
        int lastTab = -1;
        string[] tabs = new string[] {"检查全部Bundle","筛选Bundle","查看具体Bundle"}; 

        private void OnGUI()
        {
            GUILayout.BeginHorizontal();
            tab = GUILayout.Toolbar(tab,tabs);
            GUILayout.EndHorizontal();

            switch (tab)
            {
                case 0:
                    if (lastTab != tab)
                        allCheckUI.PreProcess();
                    allCheckUI.Display();
                    break;
                case 1:
                    filterCheckUI.Display();
                    break;
                case 2:
                    specifyCheckUI.Display();
                    break;
                default:
                    break;
            }

            lastTab = tab;
        }
    }

    public class BundleDependenciesCheckUI
    {
        public List<BundleDependencyInfo> bundleDependencyInfoList = new List<BundleDependencyInfo>();
        Vector2 scrollPos = new Vector2(0, 100);

        public void Display()
        {
            DisplayOptions();
            DisplayResult();
        }

        public virtual void DisplayOptions()
        {
            bundleDependencyInfoList.Clear();
        }

        int currentPage;
        int wantedPage;
        int ItemCountPerPage = 100;

        public virtual void DisplayResult()
        {
            int lastPage = Mathf.CeilToInt(bundleDependencyInfoList.Count / (ItemCountPerPage * 1.0f)) - 1;

            currentPage = Mathf.Clamp(currentPage, 0, lastPage);

            //scroll view 高度应该减去头部ui高度
            scrollPos = EditorGUILayout.BeginScrollView(scrollPos, GUILayout.Width(BundleDependenciesCheckWindow.window.position.width - 40), GUILayout.Height(BundleDependenciesCheckWindow.window.position.height - 120));

            if(bundleDependencyInfoList == null || bundleDependencyInfoList.Count <= 0)
                EditorGUILayout.LabelField("无结果");
            else
            {
                GUILayout.BeginVertical();

                int startIndex = ItemCountPerPage * currentPage;

                for (int i = 0; i < ItemCountPerPage; i++)
                {
                    var itemIndex = startIndex + i;
                    if(itemIndex < bundleDependencyInfoList.Count)
                    {
                        var ui = new BundleDependencyDisplayItem();
                        ui.Display(itemIndex, bundleDependencyInfoList[itemIndex]);
                    }
                }
                
                GUILayout.EndVertical();
            }

            EditorGUILayout.EndScrollView();

            if(bundleDependencyInfoList != null && bundleDependencyInfoList.Count > 0)
                DrawPageInfo(lastPage);
        }

        public void DrawPageInfo(int lastPage)
        {
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("首页", GUILayout.Width(60)))
                currentPage = 0;
            if(GUILayout.Button("上一页",GUILayout.Width(60)))
                currentPage -= 1;
            var pageInfo = currentPage + "/" + lastPage;
            EditorGUILayout.LabelField(pageInfo,GUILayout.Width(8*pageInfo.Length));
            if(GUILayout.Button("下一页",GUILayout.Width(60)))
                currentPage += 1;
            if (GUILayout.Button("最后一页", GUILayout.Width(60)))
                currentPage = lastPage;

            wantedPage = EditorGUILayout.IntField(wantedPage,GUILayout.Width(60));
            if (GUILayout.Button("Go", GUILayout.Width(30)))
                currentPage = wantedPage;
            EditorGUILayout.EndHorizontal();
        }

        public void OutPutJson(string filePath)
        {
            AllBundleDependencyInfos allInfos = new AllBundleDependencyInfos();
            foreach (var item in bundleDependencyInfoList)
                allInfos.allBundleDependencyInfos.Add(new BundleDependencyInfo_forJson(item));

            var jsonContens = UnityEngine.JsonUtility.ToJson(allInfos, true);

            System.IO.File.WriteAllText(filePath,jsonContens);
            Debug.Log("输出成功，请查看 " + filePath);
        }

        //todo
        public void OutPutMarkdown(string filePath)
        {

        }
        
        public void OutPutCycles(string filePath)
        {
            HashSet<string> cycles = new HashSet<string>();
            StringBuilder result = new StringBuilder();
            foreach (var item in bundleDependencyInfoList)
            {
                foreach (var cycle in item.cycles)
                {
                    string str = null;
                    cycle.Sort();
                    foreach (var node in cycle)
                    {
                        str += node;
                    }
                    if (cycles.Add(str))
                    {
                        foreach (var c in cycle)
                        {
                            result.Append(c + " ");
                        }
                        result.AppendLine();
                    }
                }
            }
            System.IO.File.WriteAllText(filePath, result.ToString());
        }
    }
    
    

    public class BundleDependenciesCheck_All : BundleDependenciesCheckUI
    {
        public override void DisplayOptions()
        {
            GUILayout.Space(10);
            GUILayout.BeginHorizontal();

            if (GUILayout.Button("输出Json", GUILayout.Width(200)))
            {
                OutPutJson(System.IO.Path.GetFullPath("BundleDependenciesCheck" + DateTime.Now.ToFileTime() +".json"));
                OutPutCycles(System.IO.Path.GetFullPath("BundleDependenciesCheckCycles" + DateTime.Now.ToFileTime() + ".txt"));
            }
                

            GUILayout.EndHorizontal();
        }

        public void PreProcess()
        {
            bundleDependencyInfoList.Clear();

            var allBundles = AssetBundleUtils.GetAllAssetBundles();

            foreach (var abName in allBundles)
            {
                int maxDepth;
                List<List<string>> cycles = null;
                List<string> allDependencies = null;

                BundleDependenciesCheck.GetBundleDependenciesInfoByDFS(abName, out allDependencies, out maxDepth, out cycles);
                var bundleDInfo = new BundleDependencyInfo();
                bundleDInfo.abName = abName;
                bundleDInfo.maxDepth = maxDepth;
                bundleDInfo.cycles = cycles;
                bundleDInfo.dependencies = allDependencies;
                bundleDependencyInfoList.Add(bundleDInfo);
            }
        }
    }

    public class BundleDependenciesCheck_Filter : BundleDependenciesCheckUI
    {
        int minMaxDepth = 0;
        bool isOnlyContainsRefCycles = false;

        public override void DisplayOptions()
        {
            GUILayout.Space(10);
            GUILayout.BeginHorizontal();

            EditorGUILayout.LabelField("依赖深度 >=", GUILayout.Width(70));
            minMaxDepth = EditorGUILayout.IntField(minMaxDepth,GUILayout.Width(100));

            GUILayout.Space(10);
            
            isOnlyContainsRefCycles = EditorGUILayout.Toggle("仅含含有循环依赖的Bundle", isOnlyContainsRefCycles);

            GUILayout.Space(10);
           
            if (GUILayout.Button("筛选出目标Bundle，并展示依赖信息"))
            {
                bundleDependencyInfoList.Clear();

                var allBundles = AssetBundleUtils.GetAllAssetBundles();

                foreach (var abName in allBundles)
                {
                    int maxDepth;
                    List<List<string>> cycles = null;
                    List<string> allDependencies = null;

                    BundleDependenciesCheck.GetBundleDependenciesInfoByDFS(abName, out allDependencies, out maxDepth, out cycles);
                    if(maxDepth >= minMaxDepth)
                    {
                        if(isOnlyContainsRefCycles)
                        {
                            if(cycles != null && cycles.Count >0)
                            {
                                var bundleDInfo = new BundleDependencyInfo();
                                bundleDInfo.abName = abName;
                                bundleDInfo.maxDepth = maxDepth;
                                bundleDInfo.cycles = cycles;
                                bundleDInfo.dependencies = allDependencies;
                                bundleDependencyInfoList.Add(bundleDInfo);
                            }
                        }
                        else
                        {
                            var bundleDInfo = new BundleDependencyInfo();
                            bundleDInfo.abName = abName;
                            bundleDInfo.maxDepth = maxDepth;
                            bundleDInfo.cycles = cycles;
                            bundleDInfo.dependencies = allDependencies;
                            bundleDependencyInfoList.Add(bundleDInfo);
                        }
                    }
                }
            }

            if (GUILayout.Button("输出Json", GUILayout.Width(200)))
            {
                OutPutJson(System.IO.Path.GetFullPath("BundleDependenciesCheck" + DateTime.Now.ToFileTime() +".json"));
                OutPutCycles(System.IO.Path.GetFullPath("BundleDependenciesCheckCycles" + DateTime.Now.ToFileTime() + ".txt"));
            }

            GUILayout.EndHorizontal();
        }
    }

    public class BundleDependenciesCheck_Specify : BundleDependenciesCheckUI
    {
        string abName = string.Empty;
        public override void DisplayOptions()
        {
            GUILayout.Space(10);
            GUILayout.BeginHorizontal();
            var allBundles = AssetBundleUtils.GetAllAssetBundles();
            GUILayout.Label("BundleName:",GUILayout.Width(100));
            abName = GUILayout.TextField(abName,GUILayout.Width(200));

            GUILayout.Space(10);
            if (GUILayout.Button("展示指定Bundle的依赖信息", GUILayout.Width(200)))
            {
                bundleDependencyInfoList.Clear();

                if(!allBundles.Contains(abName))
                {
                    GUILayout.EndHorizontal();
                    EditorUtility.DisplayDialog("Error",$"{abName} 不存在","OK");
                    return;
                }

                int maxDepth;
                List<List<string>> cycles = null;
                List<string> allDependencies = null;

                BundleDependenciesCheck.GetBundleDependenciesInfoByDFS(abName, out allDependencies, out maxDepth, out cycles);
                var bundleDInfo = new BundleDependencyInfo();
                bundleDInfo.abName = abName;
                bundleDInfo.maxDepth = maxDepth;
                bundleDInfo.cycles = cycles;
                bundleDInfo.dependencies = allDependencies;
                bundleDependencyInfoList.Add(bundleDInfo);

            }

            if (GUILayout.Button("输出Json", GUILayout.Width(200)))
            {
                OutPutJson(System.IO.Path.GetFullPath("BundleDependenciesCheck" + DateTime.Now.ToFileTime() +".json"));
                OutPutCycles(System.IO.Path.GetFullPath("BundleDependenciesCheckCycles" + DateTime.Now.ToFileTime() + ".txt"));
            }

            GUILayout.EndHorizontal();
        }
    }

    public class BundleDependencyDisplayItem
    {
        public void Display(int index, BundleDependencyInfo content)
        {
            EditorGUILayout.BeginHorizontal();

            GUILayout.Label(index + ".BundleName:",GUILayout.Width(100));
            GUILayout.TextField(content.abName,GUILayout.Width(200));
            GUILayout.Space(10);

            if(content.maxDepth == 0)
                GUILayout.Label("不依赖其它bundle",GUILayout.Width(200));
            else
            {
                GUILayout.Label("依赖最大深度为:",GUILayout.Width(100));
                GUILayout.TextField(content.maxDepth.ToString(),GUILayout.Width(200));
                GUILayout.Space(10);

                GUILayout.Label("依赖的bundle为：", GUILayout.Width(100));
                string allDependencies = string.Empty;
                foreach (var item in content.dependencies)
                    allDependencies += (item + " ");
                GUILayout.TextField(allDependencies,GUILayout.Width(6 * allDependencies.Length));

                GUILayout.Space(10);

                if (content.cycles != null && content.cycles.Count > 0)
                {
                    GUILayout.Label("存在循环引用：",GUILayout.Width(100));

                    foreach (var cycle in content.cycles)
                    {
                        var cycle_str = "┌->"; 
                        foreach (var ab in cycle)
                            cycle_str += (ab + " -> ");
                        cycle_str +=  "┐";

                        GUILayout.TextField(cycle_str, GUILayout.Width(cycle_str.Length * 7));
                    }
                }
            }

            EditorGUILayout.EndHorizontal();
        }
    }

    [Serializable]
    public class AllBundleDependencyInfos
    {
        [SerializeField]
        public List<BundleDependencyInfo_forJson> allBundleDependencyInfos = new List<BundleDependencyInfo_forJson>(); 
    }

    [Serializable]
    public class BundleDependencyInfo_forJson
    {
        public string abName;
        public int maxDepth;
        public List<string> dependencies;
        public List<DependencyCycle> dependencyCycles;

        public BundleDependencyInfo_forJson(BundleDependencyInfo bundleInfo)
        {
            abName = bundleInfo.abName;
            maxDepth = bundleInfo.maxDepth;
            dependencies = bundleInfo.dependencies;
            dependencyCycles = new List<DependencyCycle>();

            if (bundleInfo.cycles == null || bundleInfo.cycles.Count < 1)
                return;

            foreach (var item in bundleInfo.cycles)
            {
                var cycle = new DependencyCycle();
                cycle.nodes = item;
                dependencyCycles.Add(cycle);
            }
        }

        [Serializable]
        public class DependencyCycle
        {
            public List<string> nodes = new List<string>();
        }
    }


    [Serializable]
    public class BundleDependencyInfo
    {
        public string abName;
        public int maxDepth;
        public List<string> dependencies;
        public List<List<string>> cycles;
    }

    public static class BundleDependenciesCheck
    {
        /// <summary>
        ///基于深度搜素，获取依赖的结点数和最大深度，以及极小循环引用环 
        /// </summary>
        /// <param name="abName">bundle 名称</param>
        /// <param name="allDependencies"> 所有依赖bundle的名称</param>
        /// <param name="maxDepth">最大引用深度，不计算循环引用造成的深度</param>
        /// <param name="cycles">（如果有）循环引用的引用环（使用list表示，表尾指向表首）</param>
        static public void GetBundleDependenciesInfoByDFS(string abName, out List<string> allDependencies, out int maxDepth, out List<List<string>> cycles)
        {
            var dirRefs = AssetBundleUtils.GetDirectDependencies(abName);
            maxDepth = 0;
            cycles = new List<List<string>>();
            allDependencies = new List<string>();

            Dictionary<string, string> nodesRecord = new Dictionary<string, string>();

            //没有引用其它bundle
            if (dirRefs == null || dirRefs.Length == 0)
                return;

            //引用了其它bundle
            foreach (var item in dirRefs)
            {
                var list = new List<string>();
                list.Add(abName);

                DepthFirstSearchHelper(item, list, nodesRecord, ref maxDepth, cycles);
            }

            allDependencies = nodesRecord.Keys.ToList();
        }

        //只检测构成的极小环
        static public void DepthFirstSearchHelper(string child_abName, List<string> depthList, Dictionary<string, string> nodeRecordTable, ref int maxDepth, List<List<string>> cycles)
        {
            //如果该节点已经在引用环里了，跳出
            //可避免重复记录引用环
            //减少子节点考查消耗
            foreach (var item in cycles)
            {
                if (item.Contains(child_abName))
                    return;
            }

            //之前的搜索中存在该节点，说明有环存在，且该节点不用记录
            if (depthList.Contains(child_abName))
            {
                var index = depthList.IndexOf(child_abName);
                //记录引用环
                var cycle = depthList.GetRange(index, depthList.Count - index);
                cycles.Add(cycle);

                maxDepth = Mathf.Max(maxDepth, depthList.Count - 1);
                return;
            }

            //记录遍历过程中的所有节点
            if (!nodeRecordTable.ContainsKey(child_abName))
                nodeRecordTable.Add(child_abName, child_abName);

            var list = new List<string>(depthList);
            list.Add(child_abName);

            var dirRefsOfChild = AssetBundleUtils.GetDirectDependencies(child_abName);

            //叶子节点
            if (dirRefsOfChild == null || dirRefsOfChild.Length == 0)
            {
                maxDepth = Mathf.Max(maxDepth, list.Count - 1);
                return;
            }

            //遍历子节点
            foreach (var item in dirRefsOfChild)
                DepthFirstSearchHelper(item, list, nodeRecordTable, ref maxDepth, cycles);
        }
    }
}
