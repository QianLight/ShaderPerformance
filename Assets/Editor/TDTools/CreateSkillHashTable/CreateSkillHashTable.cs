using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
using UnityEditor;
using CFUtilPoolLib;

namespace TDTools
{
    public static class CreateSkillHashTable
    {
        static readonly string outPath = "Editor/TDTools/CreateSkillHashTable";
        [MenuItem("Tools/TDTools/关卡相关工具/输出技能hash对应表")]
        public static void OutputSkillHashTableFile()
        {
            SkillListForRole.RowData[] RoleList = XSkillReader.RoleSkill.Table;
            SkillListForEnemy.RowData[] EnemyList = XSkillReader.EnemySkill.Table;
            SkillListForPet.RowData[] PetList = XSkillReader.PetSkill.Table;

            StringBuilder output = new StringBuilder();
            foreach(var item in RoleList)
            {
                output.AppendLine($"{item.SkillScript}\t{XCommon.singleton.XHash(item.SkillScript)}");
            }
            foreach (var item in EnemyList)
            {
                output.AppendLine($"{item.SkillScript}\t{XCommon.singleton.XHash(item.SkillScript)}");
            }
            foreach (var item in PetList)
            {
                output.AppendLine($"{item.SkillScript}\t{XCommon.singleton.XHash(item.SkillScript)}");
            }

            using (var file = File.Open($"{Application.dataPath}/{outPath}/技能hash对应表.txt", FileMode.Create))
            {
                var info = Encoding.UTF8.GetBytes(output.ToString());
                file.Write(info, 0, info.Length);
            }
            EditorUtility.DisplayDialog("输出完成", "输出目录Asset/Editor/TDTools/CreateSkillHashTable/技能hash对应表.txt", "确定");
        }
    }
}
