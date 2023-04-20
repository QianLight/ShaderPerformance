using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TDTools
{
    internal class UnitAITableChecker: BaseChecker
    {
        string[] AllSkillColumn = { "MainSkillName", "LeftSkillName", "RightSkillName", "BackSkillName", "CheckingSkillName", "DashSkillName", "TurnSkillName", "MoveSkillName" };
        public void SkillNameExistCheck(string TableName, string ColumnNames)
        {
            //List<string> EnemySkill = DSC.GetTableSpecificColumnContent("SkillListForEnemy", "SkillScript");
            //List<string> RoleSkill = DSC.GetTableSpecificColumnContent("SkillListForRole", "SkillScript");
            //List<string> PetSkill = DSC.GetTableSpecificColumnContent("SkillListForPet", "SkillScript");
            List<string> AllSkill = new List<string>();
            DataStructCreator DSC = new DataStructCreator();
            AllSkill.AddRange(DSC.GetTableSpecificColumnContent("SkillListForEnemy", "SkillScript"));
            AllSkill.AddRange(DSC.GetTableSpecificColumnContent("SkillListForRole", "SkillScript"));
            AllSkill.AddRange(DSC.GetTableSpecificColumnContent("SkillListForPet", "SkillScript"));
            List<int> Result = new List<int>();
            string[] ColumnNameArraty;

            if (ColumnNames == "All")
                ColumnNameArraty = AllSkillColumn;
            else
                ColumnNameArraty = ColumnNames.Split('|');  //MainSkillName|LeftSkillName|RightSkillName|BackSkillName|CheckingSkillName|DashSkillName|TurnSkillName|MoveSkillName
            for (int i =0; i< ColumnNameArraty.Length;i++)
            {
                List<string> CheckedColumn = DSC.GetTableSpecificColumnContent(TableName, ColumnNameArraty[i]);
                for (int j = 0; j < CheckedColumn.Count; j++)
                {
                    if (CheckedColumn[j] == "")
                        continue;
                    List<string> SingleSkillList = new List<string>();
                    List<string> SingleSkillList_F = new List<string>();
                    SingleSkillList.AddRange(CheckedColumn[j].Split('|').ToList());
                    SingleSkillList.AddRange(CheckedColumn[j].Split('=').ToList());
                    foreach(string s in SingleSkillList)
                    {
                        if (!(s.Contains('|') || s.Contains('=')))
                            SingleSkillList_F.Add(s);
                    }                
            
                    foreach (string SingleSkill in SingleSkillList_F)
                    {
                        if (SingleSkill == "") //配表习惯真的不行,如果能规范一下配表习惯能少考虑几种情况
                            continue;
                        if (!AllSkill.Contains(SingleSkill))
                        {
                            Result.Add(j);
                            TableConfigCheckerUI.ResultOutput($"表{TableName}的{ColumnNameArraty[i]}列的第{j + 3}行内容{SingleSkill}无法在Skill相关的三张表中找到");
                        }
                    }  
                }
            }
            if (Result.Count == 0)
            {
                TableConfigCheckerUI.ResultOutput($"表{TableName}的{ColumnNames}的所有内容都能在Skill相关的三张表中找到");
            }

        }

        public void SkillScriptExistCheck(string TableName, string ColumnNames)
        {
            DataStructCreator DSC = new DataStructCreator();
            string[] ColumnNameArraty;
            List<int> Result = new List<int>();

            if (ColumnNames == "All")
                ColumnNameArraty = AllSkillColumn;
            else
                ColumnNameArraty = ColumnNames.Split('|');  //MainSkillName|LeftSkillName|RightSkillName|BackSkillName|CheckingSkillName|DashSkillName|TurnSkillName|MoveSkillName
            for (int i = 0; i < ColumnNameArraty.Length; i++)
            {
                List<string> CheckedColumn = DSC.GetTableSpecificColumnContent(TableName, ColumnNameArraty[i]);
                for (int j = 0; j < CheckedColumn.Count; j++)
                {
                    if (CheckedColumn[j] == "")
                        continue;
                    List<string> SingleSkillList = new List<string>();
                    List<string> SingleSkillList_F = new List<string>();
                    SingleSkillList.AddRange(CheckedColumn[j].Split('|').ToList());
                    SingleSkillList.AddRange(CheckedColumn[j].Split('=').ToList());
                    foreach (string s in SingleSkillList)
                    {
                        if (!(s.Contains('|') || s.Contains('=')))
                            SingleSkillList_F.Add(s);
                    }

                    foreach (string SingleSkill in SingleSkillList_F)
                    {
                        if (SingleSkill == "") //配表习惯真的不行,如果能规范一下配表习惯能少考虑几种情况
                            continue;
                        if (!DataStructCreator.skill_files.Contains(SingleSkill + ".bytes"))
                        {
                            Result.Add(j);
                            TableConfigCheckerUI.ResultOutput($"表{TableName}的{ColumnNameArraty[i]}列的第{j + 3}行内容{SingleSkill}无法找到对应的脚本文件");
                        }
                    }
                }
            }
            if (Result.Count == 0)
            {
                TableConfigCheckerUI.ResultOutput($"表{TableName}的{ColumnNames}的所有内容都能找到对应的技能脚本");
            }
        }

        public void PassMethords()     //希望一来就存在的没有没有必要调函数
        {
            bool exist_1 = false;
            bool exist_2 = false;
            bool exist_3 = false;
            bool exist_4 = false;
            bool exist_5 = false;
            MethordInfo AimMethord_1 = new MethordInfo("UnitAITable", "EmptyCheck", "筛选列内空行", 1, EmptyCheck);
            MethordInfo AimMethord_2 = new MethordInfo("UnitAITable", "RepeatedCheck", "筛选列内重复行", 1, RepeatedCheck);
            MethordInfo AimMethord_3 = new MethordInfo("UnitAITable", "ForeignkeyCheck", "筛选没有在目标列中出现的内容", 1, ForeignkeyCheck);
            MethordInfo AimMethord_4 = new MethordInfo("UnitAITable", "SkillNameExistCheck", "检查本表中出现的技能是否也在Skill相关的三张表中出现", 1, SkillNameExistCheck);
            MethordInfo AimMethord_5 = new MethordInfo("UnitAITable", "SkillScriptExistCheck", "检查本表中出现的技能是否有对应的技能脚本", 1, SkillScriptExistCheck);
            foreach (MethordInfo m in TableConfigCheckerUI.AllMethords)
            {
                if ((m.ObjectName + m.MethordName) == (AimMethord_1.ObjectName + AimMethord_1.MethordName))
                    exist_1 = true;
                if ((m.ObjectName + m.MethordName) == (AimMethord_2.ObjectName + AimMethord_2.MethordName))
                    exist_2 = true;
                if ((m.ObjectName + m.MethordName) == (AimMethord_3.ObjectName + AimMethord_3.MethordName))
                    exist_3 = true;
                if ((m.ObjectName + m.MethordName) == (AimMethord_4.ObjectName + AimMethord_4.MethordName))
                    exist_4 = true;
                if ((m.ObjectName + m.MethordName) == (AimMethord_5.ObjectName + AimMethord_5.MethordName))
                    exist_5 = true;
            }
            if (!exist_1)
                TableConfigCheckerUI.AllMethords.Add(AimMethord_1);
            if (!exist_2)
                TableConfigCheckerUI.AllMethords.Add(AimMethord_2);
            if (!exist_3)
                TableConfigCheckerUI.AllMethords.Add(AimMethord_3);
            if (!exist_4)
                TableConfigCheckerUI.AllMethords.Add(AimMethord_4);
            if (!exist_5)
                TableConfigCheckerUI.AllMethords.Add(AimMethord_5);
        }
    }
}
