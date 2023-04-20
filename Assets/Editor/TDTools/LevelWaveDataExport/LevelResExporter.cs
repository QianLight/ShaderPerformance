using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using LevelEditor;
using BluePrint;
using CFUtilPoolLib;
using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using UnityEditor.UIElements;

namespace TDTools
{
    public class LevelResExporter : EditorWindow
    {
        private static readonly string WaveDataExporterWinUxmlPath = "Assets/Editor/TDTools/LevelWaveDataExport/LevelWaveDataExporter.uxml";
        private static readonly string LevelFilePath = "/BundleRes/Table/";
        private static readonly string[] TitleLine1 = new string[] {
            "PrefabName\tPresentationID\tStaticticsID\tMapID\tSceneName",
            "PrefabName\tPresentationID\tStaticticsID\tSceneID\tSceneName",
        };
        private int progressId;
        private bool isOutput = false;

        private LevelResExporterData interfaceData;
        private string[] filterArray;
        private Dictionary<string, ResExporterPrefabData> prefabDic = new Dictionary<string, ResExporterPrefabData>();

        [MenuItem("Tools/TDTools/关卡相关工具/关卡脚本导出【资源】")]
        public static void ShowWindow()
        {
            var win = GetWindowWithRect<LevelResExporter>(new Rect(0, 0, 400, 200));
            win.Show();
        }

        private void OnEnable()
        {
            Reload();
            var vta = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(WaveDataExporterWinUxmlPath);
            vta.CloneTree(rootVisualElement);
            interfaceData = CreateInstance<LevelResExporterData>();
            var so = new SerializedObject(interfaceData);
            rootVisualElement.Q<Button>("SaveBtn").RegisterCallback<MouseUpEvent>(obj => { SaveFile(); });
            rootVisualElement.Q<Button>("StopBtn").RegisterCallback<MouseUpEvent>(obj => { StopSaveFile(); });
            rootVisualElement.Add(new Toggle("是否合并id输出：")
            {
                bindingPath = "MergeId",
            });
            rootVisualElement.Add(new Toggle("输出SceneID：")
            {
                bindingPath = "UseSceneID",
            });
            rootVisualElement.Add(new IntegerField("MapId最小限制：")
            {
                bindingPath = "MapIdMin",
            });
            rootVisualElement.Add(new IntegerField("MapId最大限制：")
            {
                bindingPath = "MapIdMax",
            });
            rootVisualElement.Add(new TextField("Prefab过滤")
            {
                bindingPath = "PrefabFilter",
            });
            rootVisualElement.Bind(so);
        }

        private void Reload()
        {
            MapListReader.Reload();
            SceneListReader.Reload();
            XEntityStatisticsReader.Reload();
            XEntityPresentationReader.Reload();
            prefabDic.Clear();
        }

        private async Task<List<ResExporterWaveData>> GetLevelDataAsync(string path, int mapID, int sceneID)
        {
            return await Task.Run(() =>
            {
                List<ResExporterWaveData> levelData = new List<ResExporterWaveData>();
                var grphData = DataIO.DeserializeData<LevelEditorData>(path);
                foreach (var graph in grphData.GraphDataList)
                {
                    foreach (var node in graph.WaveData)
                    {
                        if (!node.enabled)
                            continue;
                        var level = interfaceData.UseSceneID ? sceneID : mapID;
                        var comment = interfaceData.UseSceneID ? SceneListReader.GetData(sceneID).SceneTitle :
                            MapListReader.GetData((uint)mapID).Comment;
                        ResExporterWaveData data = new ResExporterWaveData(node.SpawnID, level, comment);
                        levelData.Add(data);
                    }
                }
                Thread.Sleep(500);
                return levelData;
            });
        }
        private void StopSaveFile()
        {
            int isRemoved = 0;
            if (isOutput)
            {
                isRemoved = Progress.Remove(progressId);
                if (isRemoved == -1) Debug.Log("RemoveSuccess!");
                else Debug.Log($"Progress:{isRemoved}RemoveFailed");
            }
            isOutput = false;
        }
        private async void SaveFile()
        {
            if (!Progress.Exists(progressId) || Progress.GetStatus(progressId) != Progress.Status.Running)
            {
                isOutput = false;
            }
            if (isOutput)
            {
                Debug.Log("正在导出中...");
                return;
            }
            string dir = EditorPrefs.GetString("LEVEL_DATA_EXPORTER_DIR", $"{Application.dataPath}/Editor/TDTools/LevelWaveDataExport"); 
            string name = EditorPrefs.GetString("LEVEL_DATA_EXPORTER_NAME", "关卡资源数据导出");
            string path = EditorUtility.SaveFilePanel("选择保存地址", dir, name, "txt");
            if (string.IsNullOrEmpty(path))
                return;
            EditorPrefs.SetString("LEVEL_DATA_EXPORTER_DIR", Path.GetDirectoryName(path));
            EditorPrefs.SetString("LEVEL_DATA_EXPORTER_NAME", Path.GetFileNameWithoutExtension(path));
            Reload();
            filterArray = interfaceData.PrefabFilter.ToLower().Split(';');
            progressId = Progress.Start("关卡数据导出");
            var rows = SceneListReader.SceneTable.Table;
            isOutput = true;
            string result = await DoExporter(rows);
            File.WriteAllText(path, result, Encoding.GetEncoding("gb2312"));
            Progress.Finish(progressId);
            isOutput = false;
        }

        private async Task<string> DoExporter(SceneTable.RowData[] array)
        {
            StringBuilder output = new StringBuilder();
            var index = interfaceData.UseSceneID ? 1 : 0;
            output.AppendLine(TitleLine1[index]);
            for(int i = 0; i < array.Length; ++i)
            {
                try
                {
                    var item = array[i];
                    uint mapId = item.MapID;
                    if (item.id == 88 || (item.id <= 971 && item.id >= 965) ||
                        mapId < interfaceData.MapIdMin || mapId > interfaceData.MapIdMax)
                    {
                        Progress.Report(progressId, (i * 1.0f) / array.Length);
                        continue;
                    }
                    var mapItem = MapListReader.GetData(mapId);
                    if(!string.IsNullOrEmpty(mapItem.LevelConfigFile) && isOutput)
                    {
                        string path = $"{Application.dataPath}{LevelFilePath}/{mapItem.LevelConfigFile}.cfg";
                        var list = await GetLevelDataAsync(path, (int)mapId, item.id);
                        foreach (var result in list) 
                        {
                            AddMonster(result);
                            if (!isOutput)
                                return null;
                        }
                    }
                    Progress.Report(progressId, (i * 1.0f) / array.Length);
                }
                catch(Exception e)
                {
                    Debug.Log($"导出错误，MapID = { array[i]?.MapID}, SceneList Index = {i}, {e}");
                }
            }
            foreach(var data in prefabDic.Values)
            {
                if(!interfaceData.MergeId)
                {
                    output.Append(data.ToString());
                }
                else
                {
                    output.Append(data.GetResultWithIDMerge());
                }
            }
            return output.ToString();
            
        }

        private void AddMonster(ResExporterWaveData data)
        {
            uint sid = (uint)data.StatisticsID;
            uint pid = XEntityStatisticsReader.GetPresentid(sid);
            string prefab = XEntityPresentationReader.GetData(pid).Prefab;
            if (filterArray.Contains(prefab))
                return;
            if(prefabDic.ContainsKey(prefab))
            {
                prefabDic[prefab].AddPresent((int)pid, (int)sid, data);
            }
            else
            {
                var prefabData = new ResExporterPrefabData(prefab, (int)pid, (int)sid, data);
                prefabDic.Add(prefab, prefabData);
            }
        }
    }

    public class ResExporterWaveData : IEqualityComparer<ResExporterWaveData>
    {
        public int StatisticsID;
        public int LevelId;
        public string MapComment;

        public ResExporterWaveData()
        {

        }

        public ResExporterWaveData(int id, int level, string comment)
        {
            StatisticsID = id;
            LevelId = level;
            MapComment = comment;
        }

        public override string ToString()
        {
            return $"PREFAB_NAME\tPRESENT_ID\tSTATICTIS_ID\t{LevelId}\t{MapComment}\n";
        }

        public bool Equals(ResExporterWaveData a, ResExporterWaveData b)
        {
            return (a.LevelId == b.LevelId);
        }

        public int GetHashCode(ResExporterWaveData obj)
        {
            return obj.LevelId.GetHashCode();
        }
    }

    public class ResExporterStatisticsData
    {
        public int StatisticsID;
        public Dictionary<int, ResExporterWaveData> Data;

        public ResExporterStatisticsData(int id, ResExporterWaveData map)
        {
            Data = new Dictionary<int, ResExporterWaveData>();
            StatisticsID = id;
            AddMapRecord(map);
        }

        public void AddMapRecord(ResExporterWaveData map)
        {
            Data[map.LevelId] = map;
        }

        public override string ToString()
        {
            StringBuilder output = new StringBuilder();
            foreach (var wdata in Data.Values)
            {
                var str = wdata.ToString();
                output.Append(str.Replace("STATICTIS_ID", StatisticsID.ToString()));
            }
            return output.ToString();
        }
    }

    public class ResExporterPresentData
    {
        public int PresentID;
        public Dictionary<int, ResExporterStatisticsData> Data;

        public ResExporterPresentData(int id, int sid, ResExporterWaveData map)
        {
            Data = new Dictionary<int, ResExporterStatisticsData>();
            PresentID = id;
            AddStatistics(sid, map);
        }

        public void AddStatistics(int sid, ResExporterWaveData map)
        {
            if (Data.ContainsKey(sid))
            {
                Data[sid].AddMapRecord(map);
            }
            else
            {
                ResExporterStatisticsData data = new ResExporterStatisticsData(sid, map);
                Data[sid] = data;
            }
        }

        public override string ToString()
        {
            StringBuilder output = new StringBuilder();
            foreach (var sdata in Data.Values)
            {
                output.Append(sdata.ToString().Replace("PRESENT_ID", PresentID.ToString()));
            }
            return output.ToString();
        }

        public List<ResExporterWaveData> GetAllLevelID()
        {
            var result = new List<ResExporterWaveData>();
            foreach(var item in Data.Values)
            {
                result.AddRange(item.Data.Values);
            }
            return result.Distinct(new ResExporterWaveData()).ToList();
        }
    }

    public class ResExporterPrefabData
    {
        public string PrefabName;
        public Dictionary<int, ResExporterPresentData> PresentDict;

        public ResExporterPrefabData(string name, int id, int sid, ResExporterWaveData map)
        {
            PrefabName = name;
            PresentDict = new Dictionary<int, ResExporterPresentData>();
            AddPresent(id, sid, map);
        }
        public void AddPresent(int presentID, int sid, ResExporterWaveData map)
        {
            if(PresentDict.ContainsKey(presentID))
            {
                PresentDict[presentID].AddStatistics(sid, map);
            }
            else
            {
                ResExporterPresentData data = new ResExporterPresentData(presentID, sid, map);
                PresentDict[presentID] = data;
            }
        }

        public override string ToString()
        {
            StringBuilder output = new StringBuilder();
            foreach(var pdata in PresentDict.Values)
            {
                output.Append(pdata.ToString().Replace("PREFAB_NAME", PrefabName));
            }
            return output.ToString();
        }

        public string GetResultWithIDMerge()
        {
            var result = new List<ResExporterWaveData>();
            foreach (var item in PresentDict.Values)
            {
                result.AddRange(item.GetAllLevelID());
            }
            result = result.Distinct(new ResExporterWaveData()).ToList();
            StringBuilder output = new StringBuilder();
            foreach (var pdata in result)
            {
                output.Append(pdata.ToString()
                    .Replace("PREFAB_NAME", PrefabName)
                    .Replace("PRESENT_ID", "")
                    .Replace("STATICTIS_ID", ""));
            }
            return output.ToString();
        }
    }

    public class LevelResExporterData : ScriptableObject
    {
        public string PrefabFilter = "Monster_empty";
        public int MapIdMin = 100;
        public int MapIdMax = 50000;
        public bool MergeId = false;
        public bool UseSceneID = false;
    }
}
