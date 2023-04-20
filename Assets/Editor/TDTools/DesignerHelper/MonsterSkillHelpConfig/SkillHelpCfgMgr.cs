using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using UnityEngine;
using UnityEditor;
using CFUtilPoolLib;

namespace TDTools
{
    [Serializable]
    public class EnemySkillHelpCfg
    {
        [NonSerialized]
        public static string[] SkillCustomName = new string[]
        {
            "默认（未设置）",
            "Boss普攻",
            "Boss小技能",
            "Boss大技能",
            "Boss机制大招",
            "Boss机制大招",
            "精英技能",
            "小怪技能",
            "无打击点",
            "特殊情况",
        };
        [NonSerialized]
        public static int[] SkillDSALevel = new int[]
        {
            0,
            100,
            100,
            300,
            400,
            1000,
            100,
            0,
        };
        [NonSerialized]
        public static int[] SkillISALevel = new int[]
        {
            0,
            0,
            0,
            0,
            5000,
            5000,
            0,
            0,
        };

        public string SkillName;
        public int StatisticsID;

        public string DecreaseSuperArmor;
        public string IncreaseSuperArmor;
        public string MagicDeSuperArmor;
        public string MagicInSuperArmor;

        public int SkillCustomType;

        public EnemySkillHelpCfg()
        {

        }
        public EnemySkillHelpCfg(string name, int id, string dsa, string isa, string mdsa, string misa)
        {
            SkillName = name;
            StatisticsID = id;
            DecreaseSuperArmor = dsa;
            IncreaseSuperArmor = isa;
            MagicDeSuperArmor = mdsa;
            MagicInSuperArmor = misa;
            SkillCustomType = 0;
        }
    }
    [Serializable]
    public class EnemySkillHelpData
    {
        public string SkillName;
        public int StatisticsID;
        public int SkillCustomType;
        public EnemySkillHelpData()
        {

        }
    }

    [Serializable]
    public class EnemySkillHelpFile
    {
        public string Version;
        public List<EnemySkillHelpData> DataList;

        public EnemySkillHelpFile()
        {
            Version = EnemySkillHelpCfgMgr.Version;
            DataList = new List<EnemySkillHelpData>();
        }
    }

    [Serializable]
    public class EnemySkillHelpTempFile
    {
        public string Version;
        public List<EnemySkillHelpCfg> DataList;

        public EnemySkillHelpTempFile()
        {
            Version = EnemySkillHelpCfgMgr.Version;
            DataList = new List<EnemySkillHelpCfg>();
        }
    }

    public class EnemySkillHelpCfgMgr
    {
        public static string Version = "V0.1";
        public static string TablePath = "Table/SkillListForEnemy.txt";
        public static string ConfigPath = "Editor/TDTools/DesignerHelper/MonsterSkillHelpConfig/MonsterSkillHelpConfig.xml";
        public static string TempFilePath = "Editor/TDTools/DesignerHelper/MonsterSkillHelpConfig/MonsterSkillHelpConfigTemp.xml";
        public const string StrScript = "SkillScript";
        public const string StrStatisticsID = "XEntityStatisticsID";
        public const string StrDecreaseSuperArmor = "DecreaseSuperArmor";
        public const string StrIncreaseSuperArmor = "IncreaseSuperArmor";
        public const string StrMagicDeSuperArmor = "MagicDecreaseSuperArmor";
        public const string StrMagicInSuperArmor = "MagicIncreaseSuperArmor";

        public static int curDeSpArIdx;
        public static int curInSpArIdx;
        public static int curMaDeSpArIdx;
        public static int curMaInSpArIdx;
        public static List<string> titleList;

        public static Dictionary<uint, EnemySkillHelpData> skillDataDic = new Dictionary<uint, EnemySkillHelpData>();
        public static Dictionary<uint, EnemySkillHelpCfg> skillDataDicTemp = new Dictionary<uint, EnemySkillHelpCfg>();

        public static void Build()
        {
            skillDataDic.Clear();
            skillDataDicTemp.Clear();
            if (File.Exists($"{Application.dataPath}/{ConfigPath}"))
            {
                var config = DataIO.DeserializeData<EnemySkillHelpFile>($"{Application.dataPath}/{ConfigPath}");
                if(config.Version==Version)
                {
                    foreach(var item in config.DataList)
                    {
                        skillDataDic.Add(GetSkillHash(item.SkillName, item.StatisticsID), item);
                    }
                }
            }
            if (File.Exists($"{Application.dataPath}/{TempFilePath}"))
            {
                var config = DataIO.DeserializeData<EnemySkillHelpTempFile>($"{Application.dataPath}/{TempFilePath}");
                if (config.Version == Version)
                {
                    foreach (var item in config.DataList)
                    {
                        skillDataDicTemp.Add(GetSkillHash(item.SkillName, item.StatisticsID), item);
                    }
                }
            }
            string path = $"{Application.dataPath}/{TablePath}";
            titleList = TDTableReadHelper.GetTitleData(path);
            curDeSpArIdx = titleList.IndexOf(StrDecreaseSuperArmor);
            curInSpArIdx = titleList.IndexOf(StrIncreaseSuperArmor);
            curMaDeSpArIdx = titleList.IndexOf(StrMagicDeSuperArmor);
            curMaInSpArIdx = titleList.IndexOf(StrMagicInSuperArmor);
        }

        public static bool GetCfg(string SkillName, int StatisticsID, out EnemySkillHelpCfg cfg)
        {
            if (skillDataDicTemp.TryGetValue(GetSkillHash(SkillName, StatisticsID), out cfg))
                return true;

            string path = $"{Application.dataPath}/{TablePath}";
            string[] values = new string[] { SkillName, StatisticsID.ToString() };
            var result = TDTableReadHelper.GetTableData(path, new string[] { StrScript, StrStatisticsID }, values,
                new bool[] { false, true }, true);

            if (result != null)
            {
                if(TryGetDiskData(SkillName, StatisticsID, out EnemySkillHelpData data))
                {
                    cfg = new EnemySkillHelpCfg(SkillName, StatisticsID, result.valueList[curDeSpArIdx], result.valueList[curInSpArIdx],
                        result.valueList[curMaDeSpArIdx], result.valueList[curMaInSpArIdx]);
                    cfg.SkillCustomType = data.SkillCustomType;
                }
                else
                {
                    cfg = new EnemySkillHelpCfg(SkillName, StatisticsID, result.valueList[curDeSpArIdx], result.valueList[curInSpArIdx],
                        result.valueList[curMaDeSpArIdx], result.valueList[curMaInSpArIdx]);
                }
                skillDataDicTemp.Add(GetSkillHash(SkillName, StatisticsID), cfg);
                return true;
            }
            cfg = null;
            return false;
        }

        static uint GetSkillHash(string name, int id)
        {
            string skillName = name;
            if (id > 0)
            {
                skillName = $"{skillName}_{id}";
            }
            return XCommon.singleton.XHash(skillName);
        }

        public static bool TryGetDiskData(string name, int id, out EnemySkillHelpData data)
        {
            var hash = GetSkillHash(name, id);

            if(skillDataDic.TryGetValue(hash, out data))
            {
                return true;
            }
            data = null;
            return false;
        }

        public static void SaveTempFile()
        {
            var cfg = new EnemySkillHelpTempFile();
            cfg.Version = EnemySkillHelpCfgMgr.Version;
            cfg.DataList = skillDataDicTemp.Values.ToList();
            DataIO.SerializeData($"{Application.dataPath}/{TempFilePath}", cfg);
            skillDataDicTemp.Clear();
        }

        public static void ClearTempFile()
        {
            skillDataDicTemp.Clear();
            File.Delete($"{Application.dataPath}/{TempFilePath}");
        }

        public static void SaveToDisk()
        {
            foreach(var item in skillDataDicTemp)
            {
                var data = new EnemySkillHelpData();
                data.SkillName = item.Value.SkillName;
                data.StatisticsID = item.Value.StatisticsID;
                data.SkillCustomType = item.Value.SkillCustomType;
                skillDataDic[item.Key] = data;
            }
            var cfg = new EnemySkillHelpFile();
            cfg.Version = EnemySkillHelpCfgMgr.Version;
            cfg.DataList = skillDataDic.Values.ToList();
            DataIO.SerializeData($"{Application.dataPath}/{ConfigPath}", cfg);
            string path = $"{Application.dataPath}/{TablePath}";
            var fullData = TDTableReadHelper.GetTableFullData(path, true);
            foreach (var data in skillDataDicTemp)
            {
                var dataList = fullData.dataList;
                int indexScript = fullData.nameList.IndexOf(StrScript);
                int indexSID = fullData.nameList.IndexOf(StrStatisticsID);
                var nameList = dataList.Where(item => item.valueList[indexScript] == data.Value.SkillName).ToList();
                var resultList = nameList.Where(item => item.valueList[indexSID] == data.Value.StatisticsID.ToString()).ToList();
                if(resultList.Count == 0 )
                {
                    resultList = nameList.Where(item => item.valueList[indexSID] == string.Empty).ToList();
                }
                resultList[0].valueList[curDeSpArIdx] = data.Value.DecreaseSuperArmor;
                resultList[0].valueList[curInSpArIdx] = data.Value.IncreaseSuperArmor;
                resultList[0].valueList[curMaDeSpArIdx] = data.Value.MagicDeSuperArmor;
                resultList[0].valueList[curMaInSpArIdx] = data.Value.MagicInSuperArmor;
            }
            SaveTempFile();
            if (TDTableReadHelper.WriteTable(path, fullData))
            {
                EditorUtility.DisplayDialog("成功", "保存完成", "确定");
                ClearTempFile();
            }
        }

        public static bool Check(EnemySkillHelpCfg data)
        {
            if (data.SkillCustomType == 9)
                return true;

            if (data.SkillCustomType == 8)
                return true;

            if(!int.TryParse(data.IncreaseSuperArmor, out int isa))
            {
                isa = 0;
            }
            if (isa != EnemySkillHelpCfg.SkillISALevel[data.SkillCustomType])
                return false;
            if (!int.TryParse(data.MagicInSuperArmor, out int misa))
            {
                misa = 0;
            }
            if (misa != EnemySkillHelpCfg.SkillISALevel[data.SkillCustomType])
                return false;

            bool flagCommon = true;
            bool flagMagic = true;
            bool flagSpecial = true;

            if (data.DecreaseSuperArmor == string.Empty)
            {
                flagCommon = EnemySkillHelpCfg.SkillDSALevel[data.SkillCustomType] == 0;
            }
            else
            {
                flagSpecial = false;
                var dsas = data.DecreaseSuperArmor.Split('|');
                foreach(var dsa in dsas)
                {
                    int value = int.Parse(dsa);
                    if (value != EnemySkillHelpCfg.SkillDSALevel[data.SkillCustomType] && value != 0)
                        flagCommon = false;
                }
            }
            if (data.MagicDeSuperArmor == string.Empty)
            {
                flagMagic = EnemySkillHelpCfg.SkillDSALevel[data.SkillCustomType] == 0;
            }
            else
            {
                flagSpecial = false;
                var dsas = data.MagicDeSuperArmor.Split('|');
                foreach (var dsa in dsas)
                {
                    int value = int.Parse(dsa);
                    if (value != EnemySkillHelpCfg.SkillDSALevel[data.SkillCustomType] && value != 0)
                        flagMagic = false;
                }
            }
            if (flagCommon || flagMagic || flagSpecial)
                return true;
            else
                return false;
        }
    }
}
