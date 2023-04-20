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
    public class LevelWaveDataExporter : EditorWindow
    {
        private static readonly string WaveDataExporterWinUxmlPath = "Assets/Editor/TDTools/LevelWaveDataExport/LevelWaveDataExporter.uxml";
        private static readonly string LevelFilePath = "/BundleRes/Table/";
        private static readonly string TitleLine1 = "LevelFile\tSceneID\tSceneType\tGraphID\tGraphName\tAttackBaseRate\t" +
                "MagicAttackBaseRate\tMaxHpRate}\tDefenceRate\tMagicDefenceRate\tSpwanID\tSpwanCount\tSpwanName\tSpwanType\t" +
                "Fightgroup\tAttrCopy\tAttackBase\tMagicAttackBase\tMaxHP\tDefence\tMagicDefence\tApplyScale\tResistValue\tResistMagnification\t" +
                "BKMaxValue\tBKTime\tCondition\tBloodScection\tBuffList";
        private static readonly string TitleLine2 = "关卡脚本路径\t场景ID\t场景类型\t子图ID\t子图名（可能空）\t" +
                "物攻系数\t法攻系数\t最大血量系数\t物防系数\t法防系数\t怪物ID\t种怪数量\t怪物名\t怪物类型\t" +
                "战斗关系\t属性引用ID\t物理攻击力\t魔法攻击\t最大血量\t物理防御\t魔法防御\t场景系数\t\t异常状态总值\t异常状态倍率\t" +
                "进入BK的最大值\tBK状态持续时间秒\t转阶段条件（buff层数从小往大填，血量从大往小填）\t2|3|1---->第一阶段2条血，第二阶段3条血，第三阶段1条血\t" +
                "关卡中加的buff（格式|分隔多个buff，其中每个依次为：BuffID,Buff等级,Buff名,Buff描述）";
        private int progressId;
        private bool isOutput = false;

        [MenuItem("Tools/TDTools/关卡相关工具/关卡脚本导出【数值】")]
        public static void ShowWindow()
        {
            var win = GetWindowWithRect<LevelWaveDataExporter>(new Rect(0, 0, 400, 100));
            win.Show();
        }

        private void OnEnable()
        {
            MapListReader.Reload();
            BuffListReader.Reload();
            SceneListReader.Reload();
            XEntityStatisticsReader.Reload();
            //LevelHelper.ReadLevelCustomConfig();
            var vta = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(WaveDataExporterWinUxmlPath);//读取uxml文件
            vta.CloneTree(rootVisualElement);//rootVisualElement 标签下的根窗口 在rootVisualElement下实例化或克隆VisualTreeAsset
            rootVisualElement.Q<Button>("SaveBtn").RegisterCallback<MouseUpEvent>(obj => { SaveFile(); });
            rootVisualElement.Q<Button>("StopBtn").RegisterCallback<MouseUpEvent>(obj => { StopSaveFile(); }); //使用了UQuery查询系统，通过名称"StopBtn"去查找刚刚创建的元素。并给其注册回调事件
        }

        //使用async/await创建异步方法，防止耗时操作阻塞当前线程。
        private async Task<List<WaveExporterLevelData>> GetLevelDataAsync(string path, int sceneID)//一次读取一个关卡蓝图
        {
            return await Task.Run(() =>
            {
                List<WaveExporterLevelData> levelData = new List<WaveExporterLevelData>();
                var grphData = DataIO.DeserializeData<LevelEditorData>(path);
                foreach (var graph in grphData.GraphDataList)
                {
                    WaveExporterLevelData data = new WaveExporterLevelData(path, sceneID, graph.graphID, graph.name);
                    foreach (var node in graph.WaveData) //怪物信息存在wavadata节点下
                    {
                        data.AddMonster(node.SpawnID, (short)node.SpawnsInfo.Count);//一个节点里可能有多个怪物
                    }

                    foreach (var node in graph.ScriptData) //buff信息存在scriptdata节点下
                    {
                        if (node.Cmd == LevelScriptCmd.Level_Cmd_Addbuff && ((int)node.valueParam[0] != 0))
                        {
                            data.AddMonsterBuff((int)node.valueParam[0], (int)node.valueParam[1], (int)node.valueParam[2]);
                        }
                    }
                    levelData.Add(data);
                }
                Thread.Sleep(1000);
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
            if (!Progress.Exists(progressId) || Progress.GetStatus(progressId) != Progress.Status.Running)  //保证该线程不重复运行
            {
                isOutput = false;
            }
            if (isOutput)
            {
                Debug.Log("正在导出中...");
                return;
            }
            string dir = EditorPrefs.GetString("LEVEL_DATA_EXPORTER_DIR", $"{Application.dataPath}/Editor/TDTools/LevelWaveDataExport"); //类似PlayerPrefs的数据保存方式EditorPrefs,适用于编辑器模式
            string name = EditorPrefs.GetString("LEVEL_DATA_EXPORTER_NAME", "关卡怪物数据导出");
            string path = EditorUtility.SaveFilePanel("选择保存地址", dir, name, "txt");  //EditorUtility编辑器类，内含弹窗、进度条、对话框等工具。
            if (string.IsNullOrEmpty(path))
                return;
            EditorPrefs.SetString("LEVEL_DATA_EXPORTER_DIR", Path.GetDirectoryName(path));
            EditorPrefs.SetString("LEVEL_DATA_EXPORTER_NAME", Path.GetFileNameWithoutExtension(path));
            progressId = Progress.Start("关卡数据导出");
            var rows = SceneListReader.SceneTable.Table;
            isOutput = true;
            string result = await DoExporter(rows);
            File.WriteAllText(path, result, Encoding.GetEncoding("gb2312"));   //使用汉字编码字符集
            Progress.Finish(progressId);
            isOutput = false;
        }

        //根据maplist表去找关卡蓝图的xml文件
        private async Task<string> DoExporter(SceneTable.RowData[] array)
        {
            StringBuilder output = new StringBuilder();
            output.AppendLine(TitleLine1);
            output.AppendLine(TitleLine2);
            byte[] monsterFilter = new byte[] { 1, 2, 6 };
            for(int i = 0; i < array.Length; ++i)
            {
                try
                {
                    var item = array[i];
                    uint mapId = item.MapID;
                    var mapItem = MapListReader.GetData(mapId);
                    if(!string.IsNullOrEmpty(mapItem.LevelConfigFile) && isOutput)
                    {
                        string path = $"{Application.dataPath}{LevelFilePath}/{mapItem.LevelConfigFile}.cfg";//关卡蓝图的xml文件
                        var list = await GetLevelDataAsync(path, item.id); //每次读取一行，传入maplist表中该关卡的场景ID和对应的xml文件地址
                        foreach (var result in list) 
                        {
                            output.Append(result.GetResultByFilter(data => monsterFilter.Contains(data.MonsterType)));//判断data.MonsterType是否是1，2，6中的一个
                            if (!isOutput)  //Progress和这里的Task不是一个东西，即便在上面remove掉了peogress，这里的task仍在后台继续运行，所以这里需要用isOutput控制其结束
                                return null;
                        }
                    }
                    Progress.Report(progressId, (i * 1.0f) / array.Length);
                }
                catch
                {
                    Debug.Log($"导出错误，MapID = { array[i]?.MapID}, SceneList Index = {i}");
                }
            }
            return output.ToString();
            
        }
    }

    public class WaveExporterLevelData
    {
        public string LevelFile;
        public int SceneID;
        public int SceneType;
        public float AttackBaseRate;
        public float MagicAttackBaseRate;
        public float MaxHpRate;
        public float DefenceRate;
        public float MagicDefenceRate;
        public int GraphID;
        public string GraphName;
        public Dictionary<int, WaveExporterMonsterData> Data;

        public WaveExporterLevelData(string file, int id, int graphId, string name)
        {
            LevelFile = file;
            SceneID = id;
            var row = SceneListReader.GetData(SceneID);
            SceneType = row.type;
            AttackBaseRate = row.AttackBaseRate;
            MagicAttackBaseRate = row.MagicAttackBaseRate;
            MaxHpRate = row.MaxHpRate;
            DefenceRate = row.DefenceRate;
            MagicDefenceRate = row.MagicDefenceRate;
            GraphID = graphId;
            GraphName = name;
            Data = new Dictionary<int, WaveExporterMonsterData>();
        }

        public void AddMonster(int id, int count)
        {
            if (id <= 0)
                return;
            if(Data.ContainsKey(id))
            {
                Data[id].AddNum(count);
                return;
            }

            Data.Add(id, new WaveExporterMonsterData(id, count));
        }

        public void AddMonsterBuff(int monsterId, int id, int level)
        {
            if (Data.ContainsKey(monsterId))
            {
                Data[monsterId].AddBuff(id, level);
            }
        }

        public override string ToString()
        {
            StringBuilder result = new StringBuilder();
            foreach(var item in Data.Values)
            {
                string line = $"{LevelFile}\t{SceneID}\t{SceneType}\t{GraphID}\t{GraphName}\t{AttackBaseRate}\t" +
                    $"{MagicAttackBaseRate}\t{MaxHpRate}\t{DefenceRate}\t{MagicDefenceRate}\t{item}";
                result.AppendLine(line);
            }
            return result.ToString();
        }

        public string GetResultByFilter(Predicate<WaveExporterMonsterData> func)
        {
            StringBuilder result = new StringBuilder();
            foreach (var item in Data.Values)
            {
                if (!func(item))
                    continue;
                string line = $"{LevelFile}\t{SceneID}\t{SceneType}\t{GraphID}\t{GraphName}\t{AttackBaseRate}\t" +
                   $"{MagicAttackBaseRate}\t{MaxHpRate}\t{DefenceRate}\t{MagicDefenceRate}\t{item}";
                result.AppendLine(line);
            }
            return result.ToString();
        }
    }

    public class WaveExporterMonsterData
    {
        public int SpwanID;
        public int SpwanCount;
        public string MonsterName;
        public byte MonsterType;
        public uint MonsterAttrCopy;
        public uint MonsterApplyScale;
        public short MonsterFightgroup;
        public float AttackBase;
        public float MagicAttackBase;
        public float MaxHP;
        public float Defence;
        public float MagicDefence;
        public uint ResistValue;
        public SeqListRef<double> ResistMagnification;

        public WaveExporterModeStateData ModeStateData;
        public WaveExporterStageData StageData;
        public List<WaveExporterBuffData> BuffList;

        public WaveExporterMonsterData(int id, int count)
        {
            SpwanID = id;
            SpwanCount = count;
            var row = XEntityStatisticsReader.GetDataBySid((uint)id);
            if(row != null)
            {
                MonsterName = row.Name;
                MonsterType = row.Type;
                MonsterAttrCopy = row.AttrCopy;
                MonsterFightgroup = row.Fightgroup;
                MonsterApplyScale = row.ApplyScale;
                AttackBase = row.AttackBase;
                MagicAttackBase = row.MagicAttackBase;
                MaxHP = row.MaxHP;
                Defence = row.Defence;
                MagicDefence = row.MagicDefence;
                ResistValue = row.ResistValue;
                ResistMagnification = row.ResistMagnification;
                ModeStateData = new WaveExporterModeStateData(id);
                StageData = new WaveExporterStageData(id);
            }
            BuffList = new List<WaveExporterBuffData>();
        }

        public void AddNum(int count)
        {
            SpwanCount += count;
        }

        public void AddBuff(int buffID, int buffLevel)
        {
            if(BuffList.Any(item => item.BuffId == buffID && item.BuffLevel == buffLevel))
            {
                return;
            }
            BuffList.Add(new WaveExporterBuffData(buffID, buffLevel));
        }

        public string AddZero(SeqListRef<double> data)
        {
            string s = "123";
            s = s.Insert(0, "00");
            string value = data.ToString();

            if (value.Contains("|"))
            {
                int index = value.IndexOf("|");
                if (index < 3)
                    value = value.Insert(1, "=0");
                if (value.Length < 7)
                    value += "=0";
            }
            else
            {
                if (value.Length > 0 && value.Length < 3)
                {
                    value += "=0";
                }
            }
            return value;
        }
        public override string ToString()
        {
            //Debug.Log(ResistMagnification);
            return $"{SpwanID}\t{SpwanCount}\t{MonsterName}\t{MonsterType}\t{MonsterFightgroup}" +
                $"\t{MonsterAttrCopy}\t{AttackBase}\t{MagicAttackBase}\t{MaxHP}\t{Defence}\t{MagicDefence}\t{MonsterApplyScale}\t{ResistValue}\t{AddZero(ResistMagnification)}" +
                $"\t{ModeStateData.ToString()}\t{StageData.ArrayToString(StageData.Condition)}\t{StageData.ArrayToString(StageData.BloodScection)}\t{string.Join<WaveExporterBuffData>("|", BuffList.ToArray())}";
            //使用|分隔符把bufflist中的值连接在一起,如果用其他方式连接，很容易最后多一个连接符，用join就不会出现这种问题
        }
    }

    public class WaveExporterBuffData
    {
        public int BuffId;
        public int BuffLevel;
        public string BuffName;
        public string BuffDesc;

        public WaveExporterBuffData(int buffID, int buffLevel)
        {
            BuffId = buffID;
            BuffLevel = buffLevel;
            var row = BuffListReader.GetData((uint)buffID, (byte)buffLevel);
            if(row != null)
            {
                BuffName = row.BuffName;
                BuffDesc = row.Description;
            }
        }

        public override string ToString()
        {
            return $"{BuffId},{BuffLevel},{BuffName},{BuffDesc}";
        }
    }
    public class WaveExporterModeStateData
    {
        public int EnemyID;
        public uint BKMaxValue;
        public float BKTime;

        public WaveExporterModeStateData(int id)
        {
            EnemyID = id;
            var row = EnemyModeStateReader.GetDataBySid((uint)id);
            var row2= XEntityStatisticsReader.GetDataBySid((uint)id);
            if (row != null&&row2!=null)
            {
                BKMaxValue = row2.ModeBKMaxValue;
                BKTime = row.BKTime;
            }
        }
        public override string ToString()
        {
            return $"{BKMaxValue}\t{BKTime}";
        }
    }
    public class WaveExporterStageData
    {
        public int EnemyID;
        public uint[] Condition;
        public uint[] BloodScection;


        public WaveExporterStageData(int id)
        {
            EnemyID = id;
            var row = XEntityStatisticsReader.GetDataBySid((uint)id);
            if (row != null)
            {
                Condition = row.StageCondition;
                BloodScection = row.StageBloodScection;
            }
        }

        public string ArrayToString(uint[] arry)
        {
            if (arry == null)
                return null;
            else if (arry.Length >= 2)
                return String.Join("|", arry);
            else return arry[0].ToString(); 

            //如果数组只有一个值，用string.join就会出现value cannot be null的错误
        }
    }
}
