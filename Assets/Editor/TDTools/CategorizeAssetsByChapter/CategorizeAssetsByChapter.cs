using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System;
using System.IO;
using System.Xml;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

using UnityEngine;
using UnityEditor;
using UnityEngine.Playables;
using UnityEngine.Timeline;

using LevelEditor;
using CFUtilPoolLib;

namespace TDTools
{
    public class CategorizeAssetsByChapter : EditorWindow
    {
        // Load level infomation from json
        private string k_jsonFullPath;
        private ChapterLevels chapterLevelInstance;

        // The index of the selected chapter
        private int chapterIndex;

        // The selected levels
        private string[] chapterLevels;
        private bool[] selectedLevels;

        // Useful paths
        private string k_dataPath;
        private string k_levelPath;

        // Output file is by default set at desktop
        private string outputPath = Environment.GetFolderPath (Environment.SpecialFolder.DesktopDirectory);
        private string outputFile = "default.txt";
        private string outputFullPath;

        // Scroll position
        private Vector2 scrollPosition;

        [MenuItem("Tools/TDTools/关卡相关工具/按章节拆分资源")]
        static void Init ()
        {
            CategorizeAssetsByChapter window = GetWindow<CategorizeAssetsByChapter>();
            window.titleContent = new GUIContent ("按章节拆分资源");
            window.minSize = new Vector2(400, 400);
            window.Show ();
        }

        
        private void OnEnable ()
        {
            // Reload tables
            XEntityStatisticsReader.Reload();
            XEntityPresentationReader.Reload();

            // Note: Application.dataPath cannot be called before OnEnable ()
            k_dataPath = Application.dataPath;

            var t_splitted = k_dataPath.Split('/').ToList();
            t_splitted.RemoveAt(t_splitted.Count() - 1);

            k_jsonFullPath = Application.dataPath  + "/Editor/TDTools/CategorizeAssetsByChapter/ChapterInfo.json";
            k_levelPath = Application.dataPath + "/BundleRes/Table/Level/";
            outputFullPath = Path.Combine(outputPath, outputFile);

            chapterIndex = 0;
            EditorPrefs.SetInt("CATEGORIZE_CHAPTER", -1);

            chapterLevelInstance = LoadJson.LoadJsonFromFile<ChapterLevels>(k_jsonFullPath);
        }

        private void OnGUI ()
        {
            // Select the chapter
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("选择主线章节: ");
            chapterIndex = EditorGUILayout.Popup(chapterIndex, chapterLevelInstance.chapters, new GUILayoutOption[]{ GUILayout.MinWidth(100)});
            chapterLevels = chapterLevelInstance.GetLevels(chapterLevelInstance.chapters[chapterIndex]);
            if (EditorPrefs.GetInt("CATEGORIZE_CHAPTER", -1) != chapterIndex)
            {
                ReloadField(chapterIndex);
            }
            GUILayout.FlexibleSpace(); 
            EditorGUILayout.EndHorizontal();

            // Select all or none
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("全选"))
            {
                SetSelect(true);
            }
            if (GUILayout.Button("全不选"))
            {
                SetSelect(false);
            }
            EditorGUILayout.EndHorizontal();

            // Select the levels that should be processed 
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, false, false);
            for (int i = 0; i < chapterLevels.Length; ++i)
            {
                selectedLevels[i] = EditorGUILayout.ToggleLeft(chapterLevels[i], selectedLevels[i]);
            }
            EditorGUILayout.EndScrollView();

            // Count the number of selected levels
            int selectCount = selectedLevels.Sum<bool>(x => Convert.ToInt32(x));
            GUILayout.Label($"已选择{selectCount}个关卡脚本");

            // Set output path and execute
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("导出目录：");
            GUILayout.Label(outputFullPath);
            if (GUILayout.Button("选择目录"))
            {
                outputFullPath = EditorUtility.SaveFilePanel("选择导出目录", outputPath, outputFile, "txt");
            }
            EditorGUILayout.EndHorizontal();

            if (GUILayout.Button("开始导出"))
            {
                DoExport();
            }
        }

        private void ReloadField (int chapterIndex)
        {
            EditorPrefs.SetInt("CATEGORIZE_CHAPTER", chapterIndex);
            selectedLevels = new bool[chapterLevels.Length];
        }

        private void SetSelect (bool select)
        {
            for (int i = 0; i < selectedLevels.Length; ++i)
                selectedLevels[i] = select;
        }

        private void DoExport ()
        {
            List<string> allPaths = new List<string>();

            var skillEditor = EditorWindow.GetWindow<SkillEditor>();
            var skillGraph = skillEditor.CurrentGraph as SkillGraph;

            for (int i = 0; i < chapterLevels.Length; ++i)
            {
                if (selectedLevels[i])
                {
                    List<string> levelPaths = DoExportByLevel(chapterLevels[i], skillGraph);
                    allPaths.AddRange(levelPaths);   
                }
            }

            skillEditor.Close();

            List<string> distinctAllPaths = HelperFunctions.RemoveDuplicateRowsFromFile(allPaths);

            if (HelperFunctions.CheckValidation(k_dataPath, allPaths))
            {
                HelperFunctions.ExportToFile(distinctAllPaths, outputFullPath);
                ShowNotification(new GUIContent("导出成功！"), 3);
            }
        }

        private List<string> DoExportByLevel (string levelFile, SkillGraph skillGraph)
        {
            List<string> paths = new List<string>();

            string levelFullPath = k_levelPath + levelFile + ".cfg";
            var graphData = DataIO.DeserializeData<LevelEditorData>(levelFullPath);

            // Scene bound to the graph
            string sceneName = FindScene(levelFile, ref paths);

            // Dynamic objects -> prefab
            // FindDynamicPrefab(graphData, sceneName, ref paths);
                
            foreach (var graph in graphData.GraphDataList)
            {
                // Cutscene node -> playable & mp4 & ui_cutscene
                FindCutscene(graph, ref paths);

                // Wave node -> XEntityStatistics -> PresenID -> XEntityPresentation -> Prefab & Curve & SkillLocation (-> Animation)
                FindSkillAnimation(graph, skillGraph, ref paths);
            }

            return paths;
 
        }

        private string FindScene (string levelFile, ref List<string> paths)
        {
            XmlDocument xmlDoc = new XmlDocument();

            string xmlFullPath = k_levelPath + levelFile + ".ecfg";
            xmlDoc.Load(xmlFullPath);

            XmlElement root = xmlDoc.DocumentElement;
            XmlNode sceneNameNode = root.SelectSingleNode("SceneName");

            string sceneName = null;
            if (sceneNameNode != null)
            {
                sceneName = sceneNameNode.InnerText;
                sceneName = sceneName.Substring(7);
                sceneName = ModifySceneNamePrefix(sceneName);
                paths.Add(sceneName);
            }

            return sceneName;

            string ModifySceneNamePrefix (string sceneName)
            {
                List<string> t_splitted = sceneName.Split('/').ToList();
                t_splitted = t_splitted.GetRange(3, t_splitted.Count - 3);
                t_splitted.Insert(0, "Scene");
                t_splitted.Insert(0, "BundleRes");
                string t_sceneName = String.Join("/", t_splitted);
                return t_sceneName;
            }
        }

        private void FindDynamicPrefab (LevelEditorData graphData, string sceneName, ref List<string> paths)
        {
            if (sceneName != null)
            {
                string dynamicRoot = graphData.DynamicRoot + ".prefab";
                //
                //string dynamicRootPath = Path.GetDirectoryName(sceneName);  ** Do not use Path.GetDirectoryName() '
                //                                                            ** as it will change the delimiter from '/' to '\',
                //                                                            ** which is not we want.
                List<string> t_splitted = sceneName.Split('/').ToList();
                t_splitted.RemoveAt(t_splitted.Count() - 1);
                string dynamicRootPath = String.Join("/", t_splitted);
                string dynamicRootFullPath = k_dataPath + "/" + dynamicRootPath;

                DirectoryInfo dirInfo = new DirectoryInfo(dynamicRootFullPath);
                FileInfo[] fileInfos = dirInfo.GetFiles(dynamicRoot, SearchOption.TopDirectoryOnly);

                for (int i = 0; i < fileInfos.Length; i++)
                {
                    string name = fileInfos[i].Name;

                    t_splitted = sceneName.Split('/').ToList();
                    t_splitted.RemoveAt(t_splitted.Count() - 1);

                    string fullName = String.Join("/", t_splitted) + "/" + name;
                    paths.Add(fullName);
                }
            }
        }

        private void FindCutscene(LevelGraphData graph, ref List<string> paths)
        {
            foreach (var node in graph.ScriptData)
            {
                if (node.Cmd == LevelScriptCmd.Level_Cmd_Cutscene)
                {
                    if (!node.enabled)
                        continue;

                    bool bMp4 = node.valueParam.Count > 1 ? (node.valueParam[1] > 0 ? true : false) : false;
                    if (bMp4)
                        paths.Add($"BundleRes/Video/{node.stringParam[0]}" + ".mp4");
                    else
                    {
                        paths.Add($"BundleRes/Timeline/{node.stringParam[0]}" + ".playable");

                        string playablePath = $"Assets/BundleRes/Timeline/{node.stringParam[0]}" + ".playable";
                        if (!File.Exists(k_dataPath + $"/BundleRes/Timeline/{node.stringParam[0]}" + ".playable"))
                            continue;

                        //
                        // The "true" method is to load the corresponding playable asset
                        //      and read the asset info.
                        //
                        // var playableAsset = AssetDatabase.LoadAssetAtPath<PlayableAsset>(playablePath);
                        //

                        TextReader tr = new StreamReader(playablePath, Encoding.UTF8);

                        string playableContent = tr.ReadToEnd();

                        int index = playableContent.IndexOf("UIBackground", 0, StringComparison.OrdinalIgnoreCase);
                        while (index != -1)
                        {
                            int newLineIndex = playableContent.IndexOf("\r\n", index);
                            string picPath = $"BundleRes/" + playableContent.Substring(index, newLineIndex - index) + ".png";
                            if (File.Exists(k_dataPath + "/" + picPath))
                                paths.Add (picPath);
                            index = playableContent.IndexOf("UIBackground", newLineIndex, StringComparison.OrdinalIgnoreCase);
                        }

                        tr.Close();
                    }   
                }
            }
        }

        private void FindSkillAnimation(LevelGraphData graph, SkillGraph skillGraph, ref List<string> paths)
        {
            foreach (var node in graph.WaveData)
            {
                if (!node.enabled)
                    continue;

                int sid = node.SpawnID;
                uint pid = XEntityStatisticsReader.GetPresentid((uint)sid);
                var row = XEntityPresentationReader.GetData(pid);

                string prefabName = row.Prefab;
                string curveLocation = row.CurveLocation;
                string skillLocation = row.SkillLocation;

                if (!prefabName.StartsWith("monster", StringComparison.OrdinalIgnoreCase))
                    continue;
                if (curveLocation == "" || !curveLocation.StartsWith("monster", StringComparison.OrdinalIgnoreCase))
                    continue;
                if (skillLocation == "" || !skillLocation.StartsWith("monster", StringComparison.OrdinalIgnoreCase))
                    continue;

                string prefabPath = k_dataPath + "/BundleRes/Runtime/Prefab/Monster";
                string curvePath = k_dataPath + "/BundleRes/Curve/" + curveLocation;
                string skillPath = k_dataPath + "/BundleRes/SkillPackage/" + skillLocation;
                string sfxPath = k_dataPath + "/BundleRes/Runtime/SFX";

                curvePath = curvePath.TrimEnd('/');
                skillPath = skillPath.TrimEnd('/');

                // Get prefab data
                // The if-expression is redundant
                if (Directory.Exists(prefabPath))
                {
                    DirectoryInfo pathInfo = new DirectoryInfo(prefabPath);
                    DirectoryInfo[] dirInfos = pathInfo.GetDirectories();

                    foreach (DirectoryInfo dirInfo in dirInfos)
                    {
                        FileInfo[] fileInfos = dirInfo.GetFiles(prefabName + ".prefab", SearchOption.TopDirectoryOnly);
                        if (fileInfos.Length != 0)
                        {
                            string t_dirName = dirInfo.Name;
                            paths.Add("BundleRes/Runtime/Prefab/Monster/" + t_dirName + "/" + prefabName + ".prefab");
                            break;
                        }
                    }
                }

                // Get curve data
                /*
                if (Directory.Exists(curvePath))
                {
                    DirectoryInfo dirInfo = new DirectoryInfo(curvePath);
                    FileInfo[] fileInfos = dirInfo.GetFiles("*.prefab", SearchOption.TopDirectoryOnly);

                    foreach (FileInfo fileInfo in fileInfos)
                        paths.Add("BundleRes/Curve/" + curveLocation + fileInfo.Name);
                }
                */

                // Get skill animation data and sfx data
                if (Directory.Exists(skillPath))
                {
                    DirectoryInfo dirInfo = new DirectoryInfo(skillPath);
                    FileInfo[] fileInfos = dirInfo.GetFiles("*.bytes", SearchOption.TopDirectoryOnly);

                    foreach (FileInfo fileInfo in fileInfos)
                    {
                        string skillFullPath = skillPath + "/" + fileInfo.Name;
                        skillGraph.OpenData (skillFullPath);

                        foreach (var anim in skillGraph.configData.AnimationData)
                        {
                            paths.Add("BundleRes/" + anim.ClipPath + ".anim");
                        }

                        foreach (var fx in skillGraph.configData.FxData)
                        {
                            if (File.Exists(sfxPath + "/" + fx.FxPath + ".prefab"))
                                paths.Add("BundleRes/Runtime/SFX/" + fx.FxPath + ".prefab");
                        }
                    }
                }
            }
        }
    }

    [Serializable]
    public class ChapterLevels
    {
        public string[] chapters;

        [SerializeField]
        private string[] Headquarter;
        [SerializeField]
        private string[] Alvida;
        [SerializeField]
        private string[] Newvillage;
        [SerializeField]
        private string[] OrangeTown;
        [SerializeField]
        private string[] Syrupvillage;
        [SerializeField]
        private string[] Baratie;
        [SerializeField]
        private string[] Skypiea;
        [SerializeField]
        private string[] Enieslobby;

        public ChapterLevels ()
        {
            chapters = new string[] {
                "Headquarter",
                "Alvida",
                "Newvillage",
                "OrangeTown",
                "Syrupvillage",
                "Baratie",
                "Skypiea",
                "Enieslobby"
            };
        }

        public ChapterLevels (ChapterLevels chapterLevelsInstance) : this()
        {
            foreach (string chapter in chapters)
            {
                string[] temp = (string[]) chapterLevelsInstance.GetType().GetField(chapter, BindingFlags.NonPublic | BindingFlags.Instance).GetValue(chapterLevelsInstance);
                temp = (string[]) temp.Clone();
                this.GetType().GetField(chapter, BindingFlags.NonPublic | BindingFlags.Instance).SetValue(this, temp);
            }
        }

        public void AppendPathPrefix (string prefix)
        {
            foreach (FieldInfo field in this.GetType().GetFields(BindingFlags.NonPublic | BindingFlags.Instance))
                if (field.FieldType.Equals(typeof(string[])))
                    AppendPathPrefixByField(field.Name, prefix);
        }

        private void AppendPathPrefixByField (string fieldName, string prefix)
        {
            string[] fieldValue = (string[]) this.GetType().GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance).GetValue(this);
            for (int i = 0; i < fieldValue.Length; ++i)
                fieldValue[i] = prefix + fieldValue[i];
            this.GetType().GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance).SetValue(this, fieldValue);
        }

        public string[] GetLevels (string chapter)
        {
            return (string[]) this.GetType().GetField(chapter, BindingFlags.NonPublic | BindingFlags.Instance).GetValue(this);
        }
    }

    public class LoadJson
    {
        public static T LoadJsonFromFile <T> (string jsonFile) where T : class
        {
            if (jsonFile == null)
                throw new ArgumentNullException("jsonFile", "jsonFile is null");
            if (!File.Exists(jsonFile))
                throw new ArgumentException($"{jsonFile} does not exist");

            StreamReader sr = new StreamReader(jsonFile);

            if (sr == null)
                throw new NullReferenceException($"Cannot construct a stream reader with {jsonFile}");

            string jsonData = sr.ReadToEnd ();

            if (jsonData.Length > 0)
                return JsonUtility.FromJson<T>(jsonData);

            return null;
        }
    }

    public class HelperFunctions
    {
        public static List<string> RemoveDuplicateRowsFromFile (List<string> allPaths)
        {
            List<string> distinct = allPaths.Distinct().ToList();
            return distinct;
        }
        public static void ExportToFile (List<string> allPaths, string outputFullPath)
        {
            if (outputFullPath == null)
                throw new ArgumentNullException("outputFullPath", "The output path cannot be null");

            TextWriter tw = new StreamWriter(outputFullPath);  

            for (int i = 0; i < allPaths.Count; i++)
            {
                tw.WriteLine(allPaths[i]);
            }

            tw.Close();
        }
        public static bool CheckValidation (string k_dataPath, List<string> paths)
        {
            string fullPath;
            bool validated = true;

            for (int i = 0; i < paths.Count; i++)
            {
                if (paths[i].EndsWith("playable") || paths[i].EndsWith("mp4"))
                    continue;

                fullPath = k_dataPath + "/" + paths[i];

                if (!File.Exists(fullPath))
                {
                    Debug.Log($"Line {i + 1}, file \"{paths[i]}\" does not exist!");
                    validated = false;
                }     
            }

            return validated;
        }
    }

}
