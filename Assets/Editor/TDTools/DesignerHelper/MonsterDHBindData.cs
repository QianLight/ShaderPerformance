using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using TableEditor;

namespace TDTools
{
    public class MonsterDHBindDataHelper
    {
        public static readonly string PresentationTablePath = "Table/XEntityPresentation.txt";
        public static List<string> CurPresentationTitleList;
        public static int CurPresentId;
        public static TableEditor.TableData CurPresentationData;

        public static readonly string StatisticsTablePath = "Table/XEntityStatistics.txt";
        public static List<string> CurStatisticsTitleList;
        public static int CurStatisticsId;
        public static TableEditor.TableData CurStatisticsData;

        public static readonly string UnitAITablePath = "Table/UnitAITable.txt";
        public static List<string> CurUnitAITitleList;
        public static int CurUnitAIId;
        public static TableEditor.TableData CurUnitAIData;

        public static bool HasValue(int value)
        {
            return value != -1;
        }

        public static bool HasValue(float value)
        {
            return value != float.NaN;
        }
        public static bool HasValue(string value)
        {
            return value != string.Empty;
        }

        public static bool HasValue(List<string> value)
        {
            return value != null && value.Count > 0;
        }

        public static void GenAllData(ref MonsterDHBindData bindData)
        {
            if (!HasValue(bindData.InputID))
                return;
            GenerateStatisticsData(bindData.InputID, true, ref bindData);
            GeneratePresentData(bindData.PresentID, true, ref bindData);
            if (HasValue(bindData.UnitAIID))
            {
                GenerateUnitAIData(bindData.UnitAIID, true, ref bindData);
            }
            GetAllSkill(true, ref bindData);
        }

        public static void GetAllSkill(bool reload, ref MonsterDHBindData bindData)
        {
            if (reload)
            {
                EnemySkillHelpCfgMgr.Build();
                EnemySkillNameReader.Reload();
            }
            bindData.SkillList = EnemySkillNameReader.GetEnemySkillNameByID(bindData.StatisticsID);

        }

        public static string GetTableValue(TableEditor.TableData data, List<string> titleList, string title)
        {
            var index = titleList.IndexOf(title);
            return data.valueList[index];
        }
        public static void GetPresentData(int id, bool needReload)
        {
            string path = $"{Application.dataPath}/{PresentationTablePath}";
            CurPresentId = id;
            CurPresentationTitleList = TDTableReadHelper.GetTitleData(path);
            CurPresentationData = TDTableReadHelper.GetTableData(path, PresentTitleConfig.PresentID, id.ToString(), needReload);
        }

        public static string GetCurPresentValue(string title)
        {
            return GetTableValue(CurPresentationData, CurPresentationTitleList, title);
        }

        public static void GenerateCurPresentData(bool needReload, ref MonsterDHBindData bindData)
        {
            GeneratePresentData(CurPresentId, needReload, ref bindData);
        }
        public static void GeneratePresentData(int id, bool needReload, ref MonsterDHBindData bindData)
        {
            GetPresentData(id, needReload);
            bindData.PresentID = id;
            bindData.PrefabName = GetCurPresentValue(PresentTitleConfig.PrefabName);
            bindData.AnimLoc = GetCurPresentValue(PresentTitleConfig.AnimLoc);
            bindData.CurveLoc = GetCurPresentValue(PresentTitleConfig.CurveLoc);
            bindData.SkillLoc = GetCurPresentValue(PresentTitleConfig.SkillLoc);
            bindData.BehitLoc = GetCurPresentValue(PresentTitleConfig.BehitLoc);

            TryParseFloat(GetCurPresentValue(PresentTitleConfig.BoundRadius), ref bindData.BoundRadius);
            TryParseFloat(GetCurPresentValue(PresentTitleConfig.BoundHeight), ref bindData.BoundHeight);
            TryParseFloat(GetCurPresentValue(PresentTitleConfig.Scale), ref bindData.Scale);
            bindData.HitFxScale = GetCurPresentValue(PresentTitleConfig.HitFxScale);
            bindData.BuffFxScale = GetCurPresentValue(PresentTitleConfig.BuffFxScale);

            bindData.Huge = GetCurPresentValue(PresentTitleConfig.Huge) == "TRUE";

            bindData.IdleAnim = GetCurPresentValue(PresentTitleConfig.IdleAnim);
            bindData.AttackIdleAnim = GetCurPresentValue(PresentTitleConfig.AttackIdleAnim);
            bindData.WalkAnim = GetCurPresentValue(PresentTitleConfig.WalkAnim);
            bindData.AttackWalkAnim = GetCurPresentValue(PresentTitleConfig.AttackWalkAnim);
            bindData.RunAnim = GetCurPresentValue(PresentTitleConfig.RunAnim);
            bindData.AttackRunAnim = GetCurPresentValue(PresentTitleConfig.AttackRunAnim);
            bindData.DeadAnim = GetCurPresentValue(PresentTitleConfig.DeadAnim);
            bindData.AppearSkill = GetCurPresentValue(PresentTitleConfig.AppearSkill);
            bindData.BehitSkill = GetCurPresentValue(PresentTitleConfig.BehitSkill);
        }

        public static void GetStatisticsData(int id, bool needReload)
        {
            string path = $"{Application.dataPath}/{StatisticsTablePath}";
            CurStatisticsId = id;
            CurStatisticsTitleList = TDTableReadHelper.GetTitleData(path);
            CurStatisticsData = TDTableReadHelper.GetTableData(path, StatisticsTitleConfig.StatisticsID, id.ToString(), needReload);
        }

        public static string GetCurStatisticsValue(string title)
        {
            return GetTableValue(CurStatisticsData, CurStatisticsTitleList, title);
        }

        public static void GenerateCurStatisticsData(bool needReload, ref MonsterDHBindData bindData)
        {
            GenerateStatisticsData(CurStatisticsId, needReload, ref bindData);
        }
        public static void GenerateStatisticsData(int id, bool needReload, ref MonsterDHBindData bindData)
        {
            GetStatisticsData(id, needReload);
            bindData.StatisticsID = id;
            bindData.PresentID = int.Parse(GetCurStatisticsValue(StatisticsTitleConfig.SPresentID));
            TryParseInt(GetCurStatisticsValue(StatisticsTitleConfig.SAIID), ref bindData.UnitAIID);
            if (TryParseInt(GetCurStatisticsValue(StatisticsTitleConfig.StatisticsType), ref bindData.StatisticsType))
            {
                bindData.StatisticsTypeString = STypeDesc[bindData.StatisticsType];
            }
            bindData.FightGroup = GetCurStatisticsValue(StatisticsTitleConfig.Fightgroup);

            TryParseFloat(GetCurStatisticsValue(StatisticsTitleConfig.WalkSpeed), ref bindData.WalkSpeed);
            TryParseFloat(GetCurStatisticsValue(StatisticsTitleConfig.RunSpeed), ref bindData.RunSpeed);
            TryParseInt(GetCurStatisticsValue(StatisticsTitleConfig.RotateSpeed), ref bindData.RotateSpeed);

            if(!TryParseInt(GetCurStatisticsValue(StatisticsTitleConfig.AttrCopy), ref bindData.AttrCopy))
            {
                TryParseInt(GetCurStatisticsValue(StatisticsTitleConfig.AttackBase), ref bindData.AttackBase);
                TryParseInt(GetCurStatisticsValue(StatisticsTitleConfig.MagicAttackBase), ref bindData.MagicAttackBase);
                TryParseInt(GetCurStatisticsValue(StatisticsTitleConfig.MaxHP), ref bindData.MaxHP);
                TryParseInt(GetCurStatisticsValue(StatisticsTitleConfig.Defence), ref bindData.Defence);
                TryParseInt(GetCurStatisticsValue(StatisticsTitleConfig.MagicDefence), ref bindData.MagicDefence);
            }
            bindData.ApplyScale = GetCurStatisticsValue(StatisticsTitleConfig.ApplyScale) == "1";
            TryParseInt(GetCurStatisticsValue(StatisticsTitleConfig.MaxSuperArmor), ref bindData.MaxSuperArmor);
            bindData.CallerAttrList = GetCurStatisticsValue(StatisticsTitleConfig.CallerAttrList);
            bindData.InBornBuff = GetCurStatisticsValue(StatisticsTitleConfig.InBornBuff);
            if(TryParseInt(GetCurStatisticsValue(StatisticsTitleConfig.ResistValue), ref bindData.ResistValue))
            {
                bindData.ResistMagnification = GetCurStatisticsValue(StatisticsTitleConfig.ResistMagnification);
            }
            bindData.StageCondition = GetCurStatisticsValue(StatisticsTitleConfig.StageCondition);
            bindData.StageBloodScection = GetCurStatisticsValue(StatisticsTitleConfig.StageBloodScection);
            bindData.ModeBKMaxValue = GetCurStatisticsValue(StatisticsTitleConfig.ModeBKMaxValue);

            if(TryParseInt(GetCurStatisticsValue(StatisticsTitleConfig.PatrolID), ref bindData.PatrolID))
            {
                bindData.IsNavPingpong = GetCurStatisticsValue(StatisticsTitleConfig.IsNavPingpong);
            }
        }

        public static void GetUnitAIData(int id, bool needReload)
        {
            string path = $"{Application.dataPath}/{UnitAITablePath}";
            CurUnitAIId = id;
            CurUnitAITitleList = TDTableReadHelper.GetTitleData(path);
            CurUnitAIData = TDTableReadHelper.GetTableData(path, UnitAITitleConfig.UnitAIID, id.ToString(), needReload);
        }

        public static string GetCurUnitAIValue(string title)
        {
            return GetTableValue(CurUnitAIData, CurUnitAITitleList, title);
        }

        public static void GenerateCurUnitAIData(bool needReload, ref MonsterDHBindData bindData)
        {
            GenerateUnitAIData(CurUnitAIId, needReload, ref bindData);
        }
        public static void GenerateUnitAIData(int id, bool needReload, ref MonsterDHBindData bindData)
        {
            GetUnitAIData(id, needReload);
            bindData.UnitAIID = id;
            bindData.Tree = GetCurUnitAIValue(UnitAITitleConfig.Tree);
            bindData.SubTree = GetCurUnitAIValue(UnitAITitleConfig.SubTree);
            bindData.PreSubTree = GetCurUnitAIValue(UnitAITitleConfig.PreSubTree);

            TryParseFloat(GetCurUnitAIValue(UnitAITitleConfig.HeartRate), ref bindData.HeartRate);
            TryParseInt(GetCurUnitAIValue(UnitAITitleConfig.Sight), ref bindData.Sight);
            TryParseInt(GetCurUnitAIValue(UnitAITitleConfig.FightTogetherDis), ref bindData.FightTogetherDis);
            
            bindData.CountEX = GetCurUnitAIValue(UnitAITitleConfig.CountEX);
            bindData.AngleLimit = GetCurUnitAIValue(UnitAITitleConfig.AngleLimit);
            bindData.Distance = GetCurUnitAIValue(UnitAITitleConfig.Distance);
            bindData.FarMove = GetCurUnitAIValue(UnitAITitleConfig.FarMove);
            bindData.Move = GetCurUnitAIValue(UnitAITitleConfig.Move);
            bindData.NearMove = GetCurUnitAIValue(UnitAITitleConfig.NearMove);
            bindData.ActionChooseEx = GetCurUnitAIValue(UnitAITitleConfig.ActionChooseEx);
            bindData.AIEvent = GetCurUnitAIValue(UnitAITitleConfig.AIEvent);
        }

        static bool TryParseInt(string str, ref int data)
        {
            if(int.TryParse(str, out int result))
            {
                data = result;
                return true;
            }
            return false;
        }
        static bool TryParseFloat(string str, ref float data)
        {
            if (float.TryParse(str, out float result))
            {
                data = result;
                return true;
            }
            return false;
        }
        public class PresentTitleConfig
        {
            public const string PresentID = "PresentID";
            public const string PrefabName = "Prefab";
            public const string AnimLoc = "AnimLocation";
            public const string CurveLoc = "CurveLocation";
            public const string SkillLoc = "SkillLocation";
            public const string BehitLoc = "BehitLocation";
            public const string BoundRadius = "BoundRadius";
            public const string BoundHeight = "BoundHeight";
            public const string Scale = "Scale";
            public const string HitFxScale = "HitFxScale";
            public const string BuffFxScale = "BuffFxScale";
            public const string Huge = "Huge";
            public const string IdleAnim = "Idle";
            public const string AttackIdleAnim = "AttackIdle";
            public const string WalkAnim = "Walk";
            public const string AttackWalkAnim = "AttackWalk";
            public const string RunAnim = "Run";
            public const string AttackRunAnim = "AttackRun";
            public const string DeadAnim = "Death";
            public const string AppearSkill = "Appear";
            public const string BehitSkill = "BeHit";
        }

        public class StatisticsTitleConfig
        {
            public const string StatisticsID = "ID";
            public const string SPresentID = "PresentID";
            public const string SAIID = "AIID";
            public const string StatisticsType = "Type";
            public const string Fightgroup = "Fightgroup";
            public const string WalkSpeed = "WalkSpeed";
            public const string RunSpeed = "RunSpeed";
            public const string RotateSpeed = "RotateSpeed";
            public const string AttrCopy = "AttrCopy";
            public const string ApplyScale = "ApplyScale";
            public const string MaxSuperArmor = "MaxSuperArmor";
            public const string AttackBase = "AttackBase";
            public const string MagicAttackBase = "MagicAttackBase";
            public const string MaxHP = "MaxHP";
            public const string Defence = "Defence";
            public const string MagicDefence = "MagicDefence";
            public const string CallerAttrList = "CallerAttrList";
            public const string InBornBuff = "InBornBuff";
            public const string ResistValue = "ResistValue";
            public const string ResistMagnification = "ResistMagnification";
            public const string StageCondition = "StageCondition";
            public const string StageBloodScection = "StageBloodScection";
            public const string ModeBKMaxValue = "ModeBKMaxValue";
            public const string PatrolID = "PatrolID";
            public const string IsNavPingpong = "IsNavPingpong";
        }

        public class UnitAITitleConfig
        {
            public const string UnitAIID = "ID";
            public const string Tree = "Tree";
            public const string SubTree = "Combat_SubTree";
            public const string PreSubTree = "Combat_PreCombatSubTree";
            public const string HeartRate = "HeartRateScale";
            public const string Sight = "Sight";
            public const string FightTogetherDis = "FightTogetherDis";
            public const string CountEX = "CountEX";
            public const string AngleLimit = "AngleLimit";
            public const string Distance = "Distance";
            public const string FarMove = "FarMove";
            public const string Move = "Move";
            public const string NearMove = "NearMove";
            public const string ActionChooseEx = "ActionChooseEx";
            public const string AIEvent = "Events";
        }

        public static string[] STypeDesc = new string[]
        {
            "x",
            "Boss",
            "Opposer",
            "Puppet",
            "Switch",
            "Substance",
            "Elite",
            "交互怪",//7
            "x",
            "x",
            "x",
            "x",
            "x",
            "x",
            "x",
            "x",
            "x",
            "x",
            "表现召唤物",//18
        };
    }


    public class MonsterDHBindData : ScriptableObject
    {
        public int InputID = -1;
        public int PresentID = -1;
        public string PrefabName;
        public string PresentIDAndPrefab = "ID";
        public string AnimLoc;
        public string CurveLoc;
        public string SkillLoc;
        public string BehitLoc;
        public string PresentParamString = "Param";
        public float BoundRadius = float.NaN;
        public float BoundHeight = float.NaN;
        public float Scale = float.NaN;
        public string HitFxScale;
        public string BuffFxScale;
        public bool Huge;
        public string IdleAnim;
        public string AttackIdleAnim;
        public string WalkAnim;
        public string AttackWalkAnim;
        public string RunAnim;
        public string AttackRunAnim;
        public string DeadAnim;
        public string AppearSkill;
        public string BehitSkill;
        public int StatisticsID = -1;
        public int StatisticsType = -1;
        public string FightGroup;
        public string StatisticsTypeString;
        public string StatisticsIDAndType = "ID";
        public float WalkSpeed = float.NaN;
        public float RunSpeed = float.NaN;
        public int RotateSpeed = -1;
        public int AttrCopy = -1;
        public bool ApplyScale;
        public int MaxSuperArmor = 0;
        public int AttackBase = 0;
        public int MagicAttackBase = 0;
        public int MaxHP = 0;
        public int Defence = 0;
        public int MagicDefence = 0;
        public string CallerAttrList;
        public string InBornBuff;
        public int ResistValue = -1;
        public string ResistMagnification;
        public string StageCondition;
        public string StageBloodScection;
        public string ModeBKMaxValue;
        public int PatrolID = -1;
        public string IsNavPingpong;
        public int UnitAIID = -1;
        public string Tree;
        public string SubTree;
        public string PreSubTree;
        public string UnitAIIDString = "ID";
        public float HeartRate = float.NaN;
        public int Sight = -1;
        public int FightTogetherDis = -1;
        public string CountEX;
        public string AngleLimit;
        public string Distance;
        public string FarMove;
        public string Move;
        public string NearMove;
        public string ActionChooseEx;
        public string AIEvent;
        public List<string> SkillList = new List<string>();
    }
}
