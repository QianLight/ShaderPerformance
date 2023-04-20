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

namespace  TDTools
{
    public class MonsterSkillExporter : EditorWindow
    {
        private static readonly string MonsterSkillExporterWinUxmlPath = "Assets/Editor/TDTools/MonsterSkillExport/MonsterSkillExporter.uxml";
        private static readonly string TitleLine1 = "StaticsID\tSkillScript\tScriptName\tHitComment\tTotalRatio\tPhysicalRatio\tPhysicalFixed\t" +
                "DecreaseSuperArmor\tIncreaseSuperArmor\tMagicRatio\tMagicFixed\tMagicDecreaseSuperArmor\tMagicIncreaseSuperArmor\t" +
            "InitCD\tCDRatio\tBuffID";
        private static readonly string TitleLine2 = "怪物ID\t技能名称\t技能名称\t打击点注释\t总系数注释\t伤害系数(多打击点)\t伤害固定值(多打击点)\t技能破霸体值\t" +
                "技能霸体值\t法术伤害系数(多打击点)\t法术伤害固定值(多打击点)\t法术技能破霸体值\t法术技能霸体值\t初始CD系数\tCD系数\t打击点=BuffID=等级|...";
        private int progressId;
        private bool isOutput = false;

        private string EnemyID;
        private List<string> noUseID;

        [MenuItem("Tools/TDTools/关卡相关工具/怪物技能导出")]
        static void Init()
        {
            MonsterSkillExporter win = GetWindow<MonsterSkillExporter>();
            win.titleContent=new GUIContent("怪物技能数据导出");
            win.Show();
        }

        private void OnEnable()
        {
            EnemySkillNameReader.Reload();
            XSkillReader.Reload();
            EnemyID = null;
            var vta = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(MonsterSkillExporterWinUxmlPath);//读取uxml文件
            var root = this.rootVisualElement;
            vta.CloneTree(root);
            rootVisualElement.Q<Button>("SaveBtn1").RegisterCallback<MouseUpEvent>(obj => { SaveFile(); }); //使用了UQuery查询系统，通过名称"savebtn"去查找刚刚创建的元素。并给其注册回调事件
            rootVisualElement.Q<TextField>("the-uxml-field").RegisterCallback<ChangeEvent<string>>((evt) =>{ EnemyID = evt.newValue;}); //使用了UQuery查询系统，通过名称"savebtn"去查找刚刚创建的元素。并给其注册回调事件
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

        void OnInspectorUpdate() //更新  
        {
            Repaint();  //重新绘制  
        }


        private void SaveFile()
        {
            if (string.IsNullOrEmpty(EnemyID))
            {
                ShowNotification(new GUIContent("请先输入怪物ID"), 3);
                return;
            }

            string dir = EditorPrefs.GetString("MONSTERSKILL_EXPORTER_DIR", $"{Application.dataPath}/Editor/TDTools/MonsterSkillExport");//类似PlayerPrefs的数据保存方式EditorPrefs,适用于编辑器模式
            string name = EditorPrefs.GetString("MONSTERSKILL_EXPORTER_NAME", "怪物技能数据导出");
            string path = EditorUtility.SaveFilePanel("选择保存地址", dir, name, "txt");//EditorUtility编辑器类，内含弹窗、进度条、对话框等工具。
            if (string.IsNullOrEmpty(path))
                return;
            EditorPrefs.SetString("MONSTER_EXPORTER_DIR", Path.GetDirectoryName(path));
            EditorPrefs.SetString("MONSTERSKILL_EXPORTER_NAME", Path.GetFileNameWithoutExtension(path));

            EnemySkillNameReader.Reload();
            EnemySkillReader.Reload();

            StringBuilder result = DoExporter(EnemyID);

            if (result!=null)
            {
                File.WriteAllText(path, result.ToString(), Encoding.GetEncoding("gb2312"));  //使用汉字编码字符集
                if (noUseID.Count > 0)
                {
                    string temp = string.Join(",", noUseID.ToArray());
                    ShowNotification(new GUIContent($"{temp}没有对应的技能"), 3);
                }
                else ShowNotification(new GUIContent("导出成功！"), 3);
            }
            else
            {
                string temp = string.Join(",", noUseID.ToArray());
                ShowNotification(new GUIContent($"{temp}没有对应的技能"), 3);
            }
                
            return;
        }
        private StringBuilder DoExporter(string eid)
        {
            String[] id = eid.Split(',');
            if(id.Length<1)
            {
                ShowNotification(new GUIContent("该ID没有对应的技能"), 3);
                return null;
            }
            noUseID = new List<string>();
            StringBuilder result = new StringBuilder();
            bool flag = false; //判断是否为有效数据
            result.AppendLine(TitleLine1);
            result.AppendLine(TitleLine2);

            for (int i=0;i<id.Length;i++)
            {
                List<string> str = EnemySkillNameReader.GetEnemySkillNameByID(int.Parse(id[i]));
                if (str==null||str.Count < 1)
                {
                    noUseID.Add(id[i]);
                }
                else
                {
                    string tempResult = GetData(str, int.Parse(id[i]));
                    if(!string.IsNullOrEmpty(tempResult))
                    {
                        result.Append(tempResult);
                        flag = true;
                        EnemySkillNameReader.Reload();
                    }
                }    
            }

            if (!flag)
                return null;
            return result; 
        }

        //根据UnitAITable的ID去找skilllist中的数据
        private string GetData(List<string> name, int id)
        {
            StringBuilder output = new StringBuilder();
            
            for (int i = 0; i < name.Count; ++i)
            {
                var SkillScript = name[i];

                SkillListForEnemy.RowData row = EnemySkillReader.GetEnemySkillRowData(SkillScript, id);

                if (row != null)
                {
                    ExporterMonsterSkillData data = new ExporterMonsterSkillData(row, id);
                    output.AppendLine(data.ToString());
                }
                else
                {
                    Debug.LogError($"{SkillScript} 没有配置在SkillListForEnemy中");
                }
            }
            return output.ToString();
        }
    }
    public class ExporterMonsterSkillData
    {
        //tID\tPhysicalFixed\tDecreaseSuperArmor\tIncreaseSuperArmor\t" +
        //"MagicRatio\tMagicFixed\tInitCD\tCDRatio\tBuffID
        public uint ID;
        public string SkillScript;
        public string ScriptName;
        public string HitCommentTotelRation;
        public SeqListRef<float> PhysicalRatio;
        public SeqListRef<float> PhysicalFixed;
        public short[] DecreaseSuperArmor;
        public int IncreaseSuperArmor;
        public SeqListRef<float> MagicRatio;
        public SeqListRef<float> MagicFixed;
        //public short[] MagicDecreaseSuperArmor;
        //public int MagicIncreaseSuperArmor;
        public float InitCD;
        public float CDRatio;
        public SeqListRef<int> BuffID;

        public ExporterMonsterSkillData(SkillListForEnemy.RowData row,int id)
        {
            ID = (uint)id; 
            SkillScript = row.SkillScript;
            ScriptName = row.ScriptName;
            HitCommentTotelRation = EnemySkillReader.GetHitCommentTotelRation(SkillScript, id);
            PhysicalRatio = row.PhysicalRatio;
            PhysicalFixed = row.PhysicalFixed;
            DecreaseSuperArmor = row.DecreaseSuperArmor;
            IncreaseSuperArmor = row.IncreaseSuperArmor;
            MagicRatio = row.MagicRatio;
            MagicFixed = row.MagicFixed;
            //MagicDecreaseSuperArmor = row.MagicDecreaseSuperArmor;
            //MagicIncreaseSuperArmor = row.MagicIncreaseSuperArmor;
            InitCD = row.InitCD;
            CDRatio = row.CDRatio;
            BuffID = row.BuffID;
        }

        public override string ToString()
        {
            return $"{ID}\t{SkillScript}\t{ScriptName}\t{HitCommentTotelRation}\t{PhysicalRatio}\t{PhysicalFixed}\t" +
                $"{ArrayToString(DecreaseSuperArmor)}\t{IncreaseSuperArmor}\t{MagicRatio}\t{MagicFixed}\t" +
                $"{InitCD}\t{CDRatio}\t{BuffID}";
        }
        public string ArrayToString(short[] arry)
        {
            if (arry == null)
                return null;
            else if (arry.Length >= 2)
                return String.Join("|", arry);
            else return arry[0].ToString();
        }
    }
}