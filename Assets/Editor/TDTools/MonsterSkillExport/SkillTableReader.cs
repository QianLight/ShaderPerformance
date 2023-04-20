#if UNITY_EDITOR
using CFUtilPoolLib;
using UnityEditor;
using UnityEngine;
using System.Linq;
using System.Collections.Generic;
using System;
using System.IO;
using UnityEngine.UIElements;
using TableEditor;


namespace TDTools
{
    public class XEntityPresentationReader
    {
        private static XEntityPresentation _presentations = new XEntityPresentation();
        public static XEntityPresentation Presentations { get { return _presentations; } }
        public static string TablePath = "Table/XEntityPresentation.txt";
        public const string ID = "PresentID";
        public const string OfficialName = "OfficialName";
        public static int curOfficialNameIndex;
        public static List<string> TitleList= new List<string>();

        static XEntityPresentationReader()
        {
            XTableReader.ReadFile(@"Table/XEntityPresentation", _presentations);
            string path = $"{Application.dataPath}/{TablePath}";
            TitleList = TDTableReadHelper.GetTitleData(path);
            curOfficialNameIndex = TitleList.IndexOf(OfficialName);
        }

        public static void Reload()
        {
            _presentations = new XEntityPresentation();

            XTableReader.ReadFile(@"Table/XEntityPresentation", _presentations);
            string path = $"{Application.dataPath}/{TablePath}";
            TitleList = TDTableReadHelper.GetTitleData(path);
            curOfficialNameIndex = TitleList.IndexOf(OfficialName);
        }

        public static XEntityPresentation.RowData GetData(uint presentid)
        {
            return _presentations.GetByPresentID(presentid);
        }

        public static string GetSkillLocByPresentId(uint id)
        {
            for (int i = 0; i < _presentations.Table.Length; ++i)
            {
                if (_presentations.Table[i].PresentID == id)
                    return _presentations.Table[i].SkillLocation;
            }

            return string.Empty;
        }
        public static string GetAnimLocationByPresentId(uint id)
        {
            for (int i = 0; i < _presentations.Table.Length; ++i)
            {
                if (_presentations.Table[i].PresentID == id)
                    return _presentations.Table[i].AnimLocation;
            }

            return string.Empty;
        }


        public static XEntityPresentation.RowData FindByColliderID(int colliderid)
        {
            int cnt = _presentations.Table.Length;
            foreach (var item in _presentations.Table)
            {
                if (item.ColliderID != null)
                {
                    foreach (var id in item.ColliderID)
                    {
                        if (id == colliderid) return item;
                    }
                }
            }
            return null;
        }

        public static void GetAllEntities(out uint[] ids, out string[] prefabs)
        {
            var table = _presentations.Table;
            ids = table.Select(x => x.PresentID).ToArray();
            prefabs = table.Select(x => x.Prefab).ToArray();
        }


        public static GameObject GetDummy(uint presentid)
        {
            XEntityPresentation.RowData raw_data = GetData(presentid);
            if (raw_data == null) return null;

            return GetDummy(raw_data.Prefab);
        }

        public static GameObject GetDummy(string path)
        {

            int n = path.LastIndexOf("_SkinnedMesh");
            int m = path.LastIndexOf("Loading");
            return n < 0 || m > 0 ?
                AssetDatabase.LoadAssetAtPath("Assets/BundleRes/Prefabs/" + path + ".prefab", typeof(GameObject)) as GameObject :
                AssetDatabase.LoadAssetAtPath("Assets/Editor/EditorResources/Prefabs/" + path.Substring(0, n) + ".prefab", typeof(GameObject)) as GameObject;
        }
        //一个prefabName可能对应多个PresentID,这里默认返回第一个读取到的PresentID
        public static uint GetPresentIDByPrefab(string prefabName)
        {
            foreach(var row in _presentations.Table)
            {
                if (row.Prefab == prefabName)
                {
                    return row.PresentID;
                }
            }
            return 0;
        }

        public static string GetOfficialNameByID( uint presentID)
        {
            string path = $"{Application.dataPath}/{TablePath}";
            var result = TDTableReadHelper.GetTableData(path, ID, presentID.ToString(), true);

            if (result != null)
                return result.valueList[curOfficialNameIndex];
            return null;
        }
    }

    public class XEntityStatisticsReader
    {
        private static XEntityStatistics _statistics = new XEntityStatistics();
        public static XEntityStatistics Statistics { get { return _statistics; } }

        static XEntityStatisticsReader()
        {
            XTableReader.ReadFile(@"Table/XEntityStatistics", _statistics);
        }

        public static void Reload()
        {
            _statistics = new XEntityStatistics();

            XTableReader.ReadFile(@"Table/XEntityStatistics", _statistics);
        }

        public static XEntityStatistics.RowData GetData(uint presentid)
        {
            for (int i = 0; i < _statistics.Table.Length; ++i)
            {
                if (_statistics.Table[i].PresentID == presentid && _statistics.Table[i].RunSpeed != 0)
                    return _statistics.Table[i];
            }

            return null;
        }

        public static XEntityStatistics.RowData GetDataBySid(uint statisticsid)
        {
            return _statistics.GetByID(statisticsid);
        }
        public static uint GetPresentid(uint statisticsid)
        {
            for (int i = 0; i < _statistics.Table.Length; ++i)
            {
                if (_statistics.Table[i].ID == statisticsid)
                    return _statistics.Table[i].PresentID;
            }

            return 0;
        }
    }
    public class EnemySkillReader
    {
        private static SkillListForEnemy _enemy_skill = new SkillListForEnemy();
        public static SkillListForEnemy EnemySkill { get { return _enemy_skill; } }
        private static Dictionary<uint, SkillListForEnemy.RowData> _enemy_skill_dic = new Dictionary<uint, SkillListForEnemy.RowData>();

        public static string TablePath = "Table/SkillListForEnemy.txt";
        public static List<string> TitleList = new List<string>();
        public const string SkillScript = "SkillScript";
        public const string StatisticsID = "XEntityStatisticsID";
        public const string HitComment = "HitComment";

        public static int curHitCommentIndex;

        public static void Reload()
        {
            _enemy_skill = new SkillListForEnemy();

            XTableReader.ReadFile(@"Table/SkillListForEnemy", _enemy_skill);

            Build();
        }

        EnemySkillReader()
        {
            XTableReader.ReadFile(@"Table/SkillListForEnemy", _enemy_skill); ;
            Build();
        }

        static uint GetSkillHash(SkillListForEnemy.RowData data)
        {
            string skillName = data.SkillScript;
            if(data.XEntityStatisticsID > 0)
            {
                skillName = $"{skillName}_{data.XEntityStatisticsID}";
            }
            return XCommon.singleton.XHash(skillName);
        }

        static void Build()
        {
            foreach (SkillListForEnemy.RowData data in _enemy_skill.Table)
            {
                _enemy_skill_dic[GetSkillHash(data)] = data;
            }
            string path = $"{Application.dataPath}/{TablePath}";
            TitleList = TDTableReadHelper.GetTitleData(path);
            curHitCommentIndex = TitleList.IndexOf(HitComment);
        }
        
        public static SkillListForEnemy.RowData GetEnemySkillRowData(string skillScript, int id)
        {
            uint hash1 = XCommon.singleton.XHash($"{skillScript}_{id}");
            uint hash2 = XCommon.singleton.XHash($"{skillScript}");
            if (_enemy_skill_dic.ContainsKey(hash1) && _enemy_skill_dic[hash1] != null)
                return _enemy_skill_dic[hash1];
            else if (_enemy_skill_dic.ContainsKey(hash2) && _enemy_skill_dic[hash2] != null)
                return _enemy_skill_dic[hash2];
            return null;
        }
        public static string GetHitCommentTotelRation(string skillScript, int id)
        {
            string path = $"{Application.dataPath}/{TablePath}";
            string[] values = new string[] { skillScript, id.ToString() };
            var result = TDTableReadHelper.GetTableData(path, new string[] { SkillScript, StatisticsID }, values,
                new bool[] { false, true });

            if (result != null)
                    return result.valueList[curHitCommentIndex] + '\t' + result.valueList[curHitCommentIndex + 1];
            return null;
        }
    }

    public class EnemyResistReader
    {
        static EnemyResist _enemyResist = new EnemyResist();

        public static EnemyResist EnemyResist { get { return _enemyResist; } }
        public static void Reload()
        {
            _enemyResist = new EnemyResist();
            XTableReader.ReadFile(@"Table/EnemyResist", _enemyResist);
        }
        static EnemyResistReader()
        {
            XTableReader.ReadFile(@"Table/EnemyResist", _enemyResist);
        }
    }
    public class UnitAIReader
    {
        private static UnitAITable _unitAIInfo = new UnitAITable();

        public static UnitAITable UnitAIInfoReader { get { return _unitAIInfo; } }

        public static void Reload()
        {
            _unitAIInfo = new UnitAITable();

            XTableReader.ReadFile(@"Table/UnitAITable", _unitAIInfo);
        }
        static UnitAIReader()
        {
            XTableReader.ReadFile(@"Table/UnitAITable", _unitAIInfo);
        }

        public static List<string> GetSkillNameByID(int id)
        {
            List<string> name = new List<string>();
            UnitAITable.RowData[] row = _unitAIInfo.Table;

            for (int i = 0; i < _unitAIInfo.Table.Length; ++i)
            {
                if (id == _unitAIInfo.Table[i].ID)
                {
                    string strs = row[i].TurnSkillName.ToString();
                    string[] tempName1 = new string[] { };
                    if (!string.IsNullOrEmpty(strs))
                        tempName1 = strs.Split('=');

                    strs = row[i].MoveSkillName.ToString();
                    string[] tempName2 = new string[] { }; ;
                    if (!string.IsNullOrEmpty(strs))
                        tempName2 = strs.Split('=');

                    string[][] tempName = new string[][] { row[i].LeftSkillName, row[i].RightSkillName, row[i].BackSkillName, row[i].CheckingSkillName, row[i].DashSkillName, row[i].MainSkillName,
            row[i].FarSkillName,row[i].SelectSkillName,row[i].UnusedSkillName,row[i].CheckingSkillAndStopName,tempName1,tempName2};

                    if (tempName != null)
                    {
                        for (int j = 0; j < tempName.Length; j++)
                        {
                            if (tempName[j] == null)
                                continue;
                            for (int k = 0; k < tempName[j].Length; k++)
                            {
                                if (!string.IsNullOrEmpty(tempName[j][k]))
                                {
                                    name.Add(tempName[j][k]);
                                    //Debug.Log(tempName[j][k]);
                                }
                            }
                        }
                    }
                    name = name.Distinct().ToList();
                    return name;
                }
            }
            return null;
        }
        public static List<string> GetAllSkillName()
        {
            List<string> name = new List<string>();
            UnitAITable.RowData[] row = _unitAIInfo.Table;

            for (int i = 0; i < _unitAIInfo.Table.Length; ++i)
            {
                string strs = row[i].TurnSkillName.ToString();
                string[] tempName1 = new string[] { };
                if (strs != null)
                    tempName1 = strs.Split('|');

                strs = row[i].MoveSkillName.ToString();
                string[] tempName2 = new string[] { }; ;
                if (strs != null)
                    tempName2 = strs.Split('|');

                string[][] tempName = new string[][] { row[i].LeftSkillName, row[i].RightSkillName, row[i].BackSkillName, row[i].CheckingSkillName, row[i].DashSkillName, row[i].MainSkillName,
            row[i].FarSkillName,row[i].SelectSkillName,row[i].UnusedSkillName,row[i].CheckingSkillAndStopName,tempName1,tempName2};

                if (tempName != null)
                {
                    for (int j = 0; j < tempName.Length && tempName[j] != null; j++)
                    {
                        for (int k = 0; k < tempName[j].Length; k++)
                        {
                            if (!string.IsNullOrEmpty(tempName[j][k]))
                            {
                                name.Add(tempName[j][k]);
                            }
                        }
                    }
                }
            }
            name = name.Distinct().ToList();
            return name;
        }
    }

    public class EnemyModeStateReader
    {
        private static EnemyModeState _statistics = new EnemyModeState();
        public static EnemyModeState Statistics { get { return _statistics; } }

        static EnemyModeStateReader()
        {
            XTableReader.ReadFile(@"Table/EnemyModeState", _statistics);
        }

        public static void Reload()
        {
            _statistics = new EnemyModeState();

            XTableReader.ReadFile(@"Table/EnemyModeState", _statistics);
        }

        public static EnemyModeState.RowData GetData(uint presentid)
        {
            for (int i = 0; i < _statistics.Table.Length; ++i)
            {
                if (_statistics.Table[i].EnemyID == presentid)
                    return _statistics.Table[i];
            }

            return null;
        }

        public static EnemyModeState.RowData GetDataBySid(uint statisticsid)
        {
            return _statistics.GetByEnemyID(statisticsid);
        }
        public static List<string> GetSkillNameByID(int id)
        {
            List<string> name = new List<string>();
            for (int i = 0; i < _statistics.Table.Length; ++i)
            {
                if (id == _statistics.Table[i].EnemyID)
                {
                    string[] tempName = new string[] { _statistics.Table[i].BKEnterSkillID, _statistics.Table[i].NormalEnterSkillID };
                    string[] tempName1 = _statistics.Table[i].BindSkillList;
                    if (tempName1 != null)
                        Array.Copy(tempName, tempName1, tempName1.Length);

                    if (tempName != null)
                    {
                        foreach (string j in tempName)
                        {
                            if (!string.IsNullOrEmpty(j))
                            {
                                name.Add(j);
                            }
                        }
                    }
                    name = name.Distinct().ToList();
                    return name;
                }
            }
            return null;
        }
        public static List<string> GetAllSkillName()
        {
            List<string> name = new List<string>();
            for (int i = 0; i < _statistics.Table.Length; ++i)
            {
                string[] tempName = new string[] { _statistics.Table[i].BKEnterSkillID, _statistics.Table[i].NormalEnterSkillID };
                string[] tempName1 = _statistics.Table[i].BindSkillList;
                if (tempName1 != null)
                    Array.Copy(tempName, tempName1, tempName1.Length);

                if (tempName != null)
                {
                    foreach (string j in tempName)
                    {
                        if (!string.IsNullOrEmpty(j))
                        {
                            name.Add(j);
                        }
                    }
                }
            }
            name = name.Distinct().ToList();
            return name;
        }
    }
    public class EnemyJZReader
    {
        private static EnemyJZ _statistics = new EnemyJZ();
        public static EnemyJZ Statistics { get { return _statistics; } }

        static EnemyJZReader()
        {
            XTableReader.ReadFile(@"Table/EnemyJZ", _statistics);
        }

        public static void Reload()
        {
            _statistics = new EnemyJZ();

            XTableReader.ReadFile(@"Table/EnemyJZ", _statistics);
        }

        public static EnemyJZ.RowData GetData(uint presentid)
        {
            for (int i = 0; i < _statistics.Table.Length; ++i)
            {
                if (_statistics.Table[i].StatictiscID == presentid)
                    return _statistics.Table[i];
            }

            return null;
        }

        public static List<string> GetSkillNameByID(int id)
        {
            List<string> name = new List<string>();
            for (int i = 0; i < _statistics.Table.Length; ++i)
            {
                if (id == _statistics.Table[i].StatictiscID)
                {
                    string[] tempName = new string[] { _statistics.Table[i].LeaveSkillID, _statistics.Table[i].XLeaveSkillID };
                    if (tempName != null)
                    {
                        foreach (string j in tempName)
                        {
                            if (!string.IsNullOrEmpty(j))
                            {
                                name.Add(j);
                            }
                        }
                    }
                    name = name.Distinct().ToList();
                    return name;
                }
            }
            return null;
        }
        public static List<string> GetAllSkillName()
        {
            List<string> name = new List<string>();
            for (int i = 0; i < _statistics.Table.Length; ++i)
            {
                string[] tempName = new string[] { _statistics.Table[i].LeaveSkillID, _statistics.Table[i].XLeaveSkillID };
                if (tempName != null)
                {
                    foreach (string j in tempName)
                    {
                        if (!string.IsNullOrEmpty(j))
                        {
                            name.Add(j);
                        }
                    }
                }
            }
            name = name.Distinct().ToList();
            return name;
        }
    }
    public class EnemyStageReader
    {
        private static EnemyStage _statistics = new EnemyStage();
        public static EnemyStage Statistics { get { return _statistics; } }

        static EnemyStageReader()
        {
            XTableReader.ReadFile(@"Table/EnemyStage", _statistics);
        }

        public static void Reload()
        {
            _statistics = new EnemyStage();

            XTableReader.ReadFile(@"Table/EnemyStage", _statistics);
        }

        public static EnemyStage.RowData GetData(uint presentid)
        {
            for (int i = 0; i < _statistics.Table.Length; ++i)
            {
                if (_statistics.Table[i].StaticsID == presentid)
                    return _statistics.Table[i];
            }

            return null;
        }

        public static EnemyStage.RowData GetDataBySid(uint statisticsid)
        {
            return _statistics.GetByStaticsID(statisticsid);
        }
        public static uint GetPresentid(uint statisticsid)
        {
            for (int i = 0; i < _statistics.Table.Length; ++i)
            {
                if (_statistics.Table[i].StaticsID == statisticsid)
                    return _statistics.Table[i].StaticsID;
            }

            return 0;
        }
        public static List<string> GetSkillNameByID(int id)
        {
            List<string> name = new List<string>();
            for (int i = 0; i < _statistics.Table.Length; ++i)
            {
                if (id == _statistics.Table[i].StaticsID)
                {
                    string[] tempName = _statistics.Table[i].SkillID;
                    if (tempName != null)
                    {
                        foreach (string j in tempName)
                        {
                            if (!string.IsNullOrEmpty(j))
                                name.Add(j);
                        }
                    }
                    name = name.Distinct().ToList();
                    return name;
                }
            }
            return null;
        }
        public static List<string> GetAllSkillName()
        {
            List<string> name = new List<string>();
            for (int i = 0; i < _statistics.Table.Length; ++i)
            {
                string[] tempName = _statistics.Table[i].SkillID;
                if (tempName != null)
                {
                    foreach (string j in tempName)
                    {
                        if (!string.IsNullOrEmpty(j))
                            name.Add(j);
                    }
                }
            }
            name = name.Distinct().ToList();
            return name;
        }
    }

    //该类用于从EnemyModeState,EnemyJZ,EnemyStage,UnitAITable中读取怪物技能名scriptname，并且删去重复
    public class EnemySkillNameReader
    {
        private static List<string> _enemySkillName = new List<string>();
        public static string TablePath = "Table/XEntityStatistics.txt";
        public static List<string> TitleList = new List<string>();
        public const string StatisticsID = "ID";
        public const string AIID = "AIID";

        public static int curAIIDIndex;
        public static void Reload()
        {
            _enemySkillName = new List<string>();

            EnemyModeStateReader.Reload();
            EnemyJZReader.Reload();
            EnemyStageReader.Reload();
            UnitAIInfoReader.Reload();
            string path = $"{Application.dataPath}/{TablePath}";
            TitleList = TDTableReadHelper.GetTitleData(path);
            curAIIDIndex = TitleList.IndexOf(AIID);
        }

        static int GetAIIDBySid(int sid)
        {
            string path = $"{Application.dataPath}/{TablePath}";
            var data = TDTableReadHelper.GetTableData(path, StatisticsID, sid.ToString(), true);
            var value = data.valueList[curAIIDIndex];
            if(int.TryParse(value, out int result))
            {
                return result;
            }
            return -1;
        }
        public static List<string> GetEnemySkillNameByID(int id)
        {
            List<string> temp = null;
            temp = EnemyModeStateReader.GetSkillNameByID(id);
            if (temp != null)
                _enemySkillName.AddRange(temp);
            temp = EnemyJZReader.GetSkillNameByID(id);
            if (temp != null)
                _enemySkillName.AddRange(temp);
            temp = EnemyStageReader.GetSkillNameByID(id);
            if (temp != null)
                _enemySkillName.AddRange(temp);
            int ai = GetAIIDBySid(id);
            temp = UnitAIReader.GetSkillNameByID(ai);
            if (temp != null)
                _enemySkillName.AddRange(temp);
            return _enemySkillName.Distinct().ToList();
        }

        public static List<string> GetAllEnemySkillName()
        {
            _enemySkillName.AddRange(EnemyModeStateReader.GetAllSkillName());
            _enemySkillName.AddRange(EnemyJZReader.GetAllSkillName());
            _enemySkillName.AddRange(EnemyStageReader.GetAllSkillName());
            _enemySkillName.AddRange(UnitAIReader.GetAllSkillName());
            return _enemySkillName;
        }
    }

    public class MapListReader
    {
        private static MapList _MapList = new MapList();

        public static MapList MapList { get { return _MapList; } }

        public static void Reload()
        {
            _MapList = new MapList();

            XTableReader.ReadFile(@"Table/MapList", _MapList);
        }
        static MapListReader()
        {
            XTableReader.ReadFile(@"Table/MapList", _MapList);
        }

        public static MapList.RowData GetData(uint mapid)
        {
            return _MapList.GetByMapID(mapid);
        }
        public static MapList.RowData GetDataByLevelConfigFile(string path)
        {
            foreach (var map in _MapList.Table)
            {
                if (map.LevelConfigFile == path)
                    return map;
            }
            return null;
        }
        public static MapList.RowData GetZhuxianDataByLevelConfigFile(string path)
        {
            foreach (var map in _MapList.Table)
            {
                if (map.LevelConfigFile == path&&map.MapID>=10000&&map.MapID<=20000)
                    return map;
            }
            return null;
        }
    }
}
#endif