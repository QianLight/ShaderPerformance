#if UNITY_EDITOR
using CFUtilPoolLib;
using UnityEditor;
using UnityEngine;
using System.Linq;
using System.Collections.Generic;
using System;
using System.IO;
using UnityEngine.UIElements;


public class HeadDialogReader
{
    public static HeadDialogTable HeadDialog { get; private set; } = new HeadDialogTable();

    static HeadDialogReader()
    {
        XTableReader.ReadFile(@"Table/HeadDialog", HeadDialog);
    }
    public static void Reload()
    {
        HeadDialog = new HeadDialogTable();
        XTableReader.ReadFile(@"Table/HeadDialog", HeadDialog);
    }
    public static string[] GetTalkContent(uint talkID)
    {
        var talk = HeadDialog.GetByID(talkID);
        return talk == null ? null : talk.TalkStr;
    }

    public static HeadDialogTable.RowData GetData(uint talkID)
    {
        return HeadDialog.GetByID(talkID);
    }
}

public class XEntityPresentationReader
{
    private static XEntityPresentation _presentations = new XEntityPresentation();
    public static XEntityPresentation Presentations { get { return _presentations; } }

    static XEntityPresentationReader()
    {
        XTableReader.ReadFile(@"Table/XEntityPresentation", _presentations);
    }

    public static void Reload()
    {
        _presentations = new XEntityPresentation();

        XTableReader.ReadFile(@"Table/XEntityPresentation", _presentations);
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

public class XSkillReader
    {
        private static SkillListForRole _role_skill = new SkillListForRole();
        public static SkillListForRole RoleSkill { get { return _role_skill; } }
        private static SkillListForEnemy _enemy_skill = new SkillListForEnemy();
        public static SkillListForEnemy EnemySkill { get { return _enemy_skill; } }

        private static SkillListForPet _pet_skill = new SkillListForPet();
        public static SkillListForPet PetSkill { get { return _pet_skill; } }
        private static Dictionary<uint, SkillListForRole.RowData> _role_skill_dic = new Dictionary<uint, SkillListForRole.RowData>();
        private static Dictionary<uint, SkillListForEnemy.RowData> _enemy_skill_dic = new Dictionary<uint, SkillListForEnemy.RowData>();
        private static Dictionary<uint, SkillListForPet.RowData> _pet_skill_dic = new Dictionary<uint, SkillListForPet.RowData>();
        private static Dictionary<int, List<uint>> _qte2skill = new Dictionary<int, List<uint>>();

        public static void Reload()
        {
            _role_skill = new SkillListForRole();
            _enemy_skill = new SkillListForEnemy();
            _pet_skill = new SkillListForPet();
            _qte2skill.Clear();

            XTableReader.ReadFile(@"Table/SkillListForRole", _role_skill);
            XTableReader.ReadFile(@"Table/SkillListForEnemy", _enemy_skill);
            XTableReader.ReadFile(@"Table/SkillListForPet", _pet_skill);

            XEntityPresentationReader.Reload();
            XEntityStatisticsReader.Reload();
            Build();
        }

        static XSkillReader()
        {
            XTableReader.ReadFile(@"Table/SkillListForRole", _role_skill);
            XTableReader.ReadFile(@"Table/SkillListForEnemy", _enemy_skill);
            XTableReader.ReadFile(@"Table/SkillListForPet", _pet_skill);
            Build();
        }

        static void Build()
        {
            foreach (SkillListForRole.RowData data in _role_skill.Table)
            {
                _role_skill_dic[XCommon.singleton.XHash(data.SkillScript)] = data;

                for (int i = 0; i < data.QTE.Count; ++i)
                {
                    int qte = data.QTE[i, 0];

                    if (!_qte2skill.ContainsKey(qte)) _qte2skill[qte] = new List<uint>();
                    _qte2skill[qte].Add(XCommon.singleton.XHash(data.SkillScript));
                }
            }

            foreach (SkillListForEnemy.RowData data in _enemy_skill.Table)
            {
                _enemy_skill_dic[XCommon.singleton.XHash(data.SkillScript)] = data;

                for (int i = 0; i < data.QTE.Count; ++i)
                {
                    int qte = data.QTE[i, 0];

                    if (!_qte2skill.ContainsKey(qte)) _qte2skill[qte] = new List<uint>();
                    _qte2skill[qte].Add(XCommon.singleton.XHash(data.SkillScript));
                }
            }

            foreach (SkillListForPet.RowData data in _pet_skill.Table)
            {
                _pet_skill_dic[XCommon.singleton.XHash(data.SkillScript)] = data;
            }
        }



        public static List<uint> GetQteSkills(int qte)
        {
            if (_qte2skill.ContainsKey(qte))
                return _qte2skill[qte];
            else return null;
        }

        public static SeqListRef<int> GetSkillQTE(uint hash)
        {
            if (_role_skill_dic.ContainsKey(hash))
                return _role_skill_dic[hash].QTE;
            if (_enemy_skill_dic.ContainsKey(hash))
                return _enemy_skill_dic[hash].QTE;
            SeqListRef<int> tmp = new SeqListRef<int>();
            tmp.count = 0;
            return tmp;
        }

        public static string GetSkillSkillScript(uint hash)
        {
            if (_role_skill_dic.ContainsKey(hash))
                return _role_skill_dic[hash].SkillScript;
            if (_enemy_skill_dic.ContainsKey(hash))
                return _enemy_skill_dic[hash].SkillScript;
            if (_pet_skill_dic.ContainsKey(hash))
                return _pet_skill_dic[hash].SkillScript;
            return "";
        }

        public static uint GetSkillPartnerID(uint hash)
        {
            if (_role_skill_dic.ContainsKey(hash))
                return _role_skill_dic[hash].SkillPartnerID;
            return 0;
        }

    }

public class PartnerInfoReader
    {
        private static PartnerInfoTable _partnerInfo = new PartnerInfoTable();

        public static PartnerInfoTable PartnerInfo { get { return _partnerInfo; } }

        static PartnerInfoReader()
        {
            XTableReader.ReadFile(@"Table/PartnerInfo", _partnerInfo);
        }

        public static string GetNameByID(uint id)
        {
            for (int i = 0; i < _partnerInfo.Table.Length; ++i)
            {
                if (_partnerInfo.Table[i].ID == id)
                    return _partnerInfo.Table[i].Name;
            }

            return string.Empty;
        }

        public static uint GetPresentIDByID(uint id)
        {
            for (int i = 0; i < _partnerInfo.Table.Length; ++i)
            {
                if (_partnerInfo.Table[i].ID == id)
                    return _partnerInfo.Table[i].PresentId;
            }

            return uint.MinValue;
        }
    }

public class BuffListReader
    {
        private static BuffTable _BuffList = new BuffTable();

        public static BuffTable BuffTable { get { return _BuffList; } }

        public static void Reload()
        {
            _BuffList = new BuffTable();

            XTableReader.ReadFile(@"Table/BuffList", _BuffList);
        }
        static BuffListReader()
        {
            XTableReader.ReadFile(@"Table/BuffList", _BuffList);
        }

        public static BuffTable.RowData GetData(uint buffid, byte bufflevel)
        {
            return _BuffList.Table.First(row => row.BuffID == buffid && row.BuffLevel == bufflevel);
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
    }
public class SceneListReader
    {
        private static SceneTable _SceneList = new SceneTable();

        public static SceneTable SceneTable { get { return _SceneList; } }

        public static void Reload()
        {
            _SceneList = new SceneTable();

            XTableReader.ReadFile(@"Table/SceneList", _SceneList);
        }
        static SceneListReader()
        {
            XTableReader.ReadFile(@"Table/SceneList", _SceneList);
        }

        public static SceneTable.RowData GetData(int sceneid)
        {
            return _SceneList.GetBySceneID(sceneid);
        }
    }

public class AttrDefineReader
{
    static AttrDefine _attrDefine = new AttrDefine();

    public static AttrDefine AttrDefine { get { return _attrDefine; } }
    public static void Reload()
    {
        _attrDefine = new AttrDefine();
        XTableReader.ReadFile(@"Table/AttrDefine", _attrDefine);
    }
    static AttrDefineReader()
    {
        XTableReader.ReadFile(@"Table/AttrDefine", _attrDefine);
    }
}

public class UnitAIInfoReader
    {
        private static UnitAITable _unitAIInfo = new UnitAITable();

        public static UnitAITable UnitAIReader { get { return _unitAIInfo; } }

        public static void Reload()
        {
            _unitAIInfo = new UnitAITable();

            XTableReader.ReadFile(@"Table/UnitAITable", _unitAIInfo);
        }
        static UnitAIInfoReader()
        {
            XTableReader.ReadFile(@"Table/UnitAITable", _unitAIInfo);
        }

        public static string[] GetMainSkillNameByID(uint id)
        {
            for (int i = 0; i < _unitAIInfo.Table.Length; ++i)
            {
                if (_unitAIInfo.Table[i].ID == id)
                    return _unitAIInfo.Table[i].MainSkillName;
            }
            string[] s = new string[] { "" };
            return s;
        }
        public static string[] GetLeftSkillNameByID(uint id)
        {
            for (int i = 0; i < _unitAIInfo.Table.Length; ++i)
            {
                if (_unitAIInfo.Table[i].ID == id)
                    return _unitAIInfo.Table[i].LeftSkillName;
            }
            string[] s = new string[] { "" };
            return s;
        }
        public static string[] GetRightSkillNameByID(uint id)
        {
            for (int i = 0; i < _unitAIInfo.Table.Length; ++i)
            {
                if (_unitAIInfo.Table[i].ID == id)
                    return _unitAIInfo.Table[i].RightSkillName;
            }
            string[] s = new string[] { "" };
            return s;
        }
        public static string[] GetBackSkillNameByID(uint id)
        {
            for (int i = 0; i < _unitAIInfo.Table.Length; ++i)
            {
                if (_unitAIInfo.Table[i].ID == id)
                    return _unitAIInfo.Table[i].BackSkillName;
            }
            string[] s = new string[] { "" };
            return s;
        }
        public static string[] GetCheckingSkillNameByID(uint id)
        {
            for (int i = 0; i < _unitAIInfo.Table.Length; ++i)
            {
                if (_unitAIInfo.Table[i].ID == id)
                    return _unitAIInfo.Table[i].CheckingSkillName;
            }
            string[] s = new string[] { "" };
            return s;
        }
        public static string[] GetDashSkillNameByID(uint id)
        {
            for (int i = 0; i < _unitAIInfo.Table.Length; ++i)
            {
                if (_unitAIInfo.Table[i].ID == id)
                    return _unitAIInfo.Table[i].DashSkillName;
            }
            string[] s = new string[] { "" };
            return s;
        }
        public static string[] GetUnusedSkillNameByID(uint id)
        {
            for (int i = 0; i < _unitAIInfo.Table.Length; ++i)
            {
                if (_unitAIInfo.Table[i].ID == id)
                    return _unitAIInfo.Table[i].UnusedSkillName;
            }
            string[] s = new string[] { "" };
            return s;
        }

        public static string TurnSkillName(uint id)
        {
            for (int i = 0; i < _unitAIInfo.Table.Length; ++i)
            {
                if (_unitAIInfo.Table[i].ID == id)
                    return _unitAIInfo.Table[i].TurnSkillName.ToString();
            }
            string s = "";
            return s;
        }
        public static string MoveSkillName(uint id)
        {
            for (int i = 0; i < _unitAIInfo.Table.Length; ++i)
            {
                if (_unitAIInfo.Table[i].ID == id)
                    return _unitAIInfo.Table[i].MoveSkillName.ToString();

            }
            string s = "";
            return s;
        }
        public static string[] UnusedSkillName(uint id)
        {
            for (int i = 0; i < _unitAIInfo.Table.Length; ++i)
            {
                if (_unitAIInfo.Table[i].ID == id)
                    return _unitAIInfo.Table[i].UnusedSkillName;
            }
            string[] s = new string[] { "" };
            return s;
        }
        public static string[] CheckingSkillAndStopName(uint id)
        {
            for (int i = 0; i < _unitAIInfo.Table.Length; ++i)
            {
                if (_unitAIInfo.Table[i].ID == id)
                    return _unitAIInfo.Table[i].CheckingSkillAndStopName;
            }
            string[] s = new string[] { "" };
            return s;
        }
        public static string[] CheckingSkillName(uint id)
        {
            for (int i = 0; i < _unitAIInfo.Table.Length; ++i)
            {
                if (_unitAIInfo.Table[i].ID == id)
                    return _unitAIInfo.Table[i].CheckingSkillName;
            }
            string[] s = new string[] { "" };
            return s;
        }
        //把unitai左右转的技能名转换成gm用的string
        public static string ConvertToString(uint id)
        {
            string s = "";
            string s1 = "";
            string s2 = "";
            string s3 = "";
            string s4 = "";
            string s5 = "";
            string s6 = "";
            string s7 = "";
            string s8 = "";
            string s9 = "";
            string s10 = "";
            string s11 = "";
            string s12 = "";
            var r = TurnSkillName(id).Replace('|', '=');
            r = r.Replace("=", ";reloadskill ");
            if (TurnSkillName(id) != "")
            {
                s1 = r.Insert(0, "reloadskill ") + ";";
            }
            else
            {
                s1 = "";
            }
            var r0 = MoveSkillName(id).Replace('|', '=');
            r0 = r0.Replace("=", ";reloadskill ");
            if (MoveSkillName(id) != "")
            {
                s2 += r0.Insert(0, "reloadskill ") + ";";
                Debug.Log(s2);
            }
            else
            {
                s2 = "";
            }
            //var r1 = GetMainSkillNameByID(id).Concat(GetLeftSkillNameByID(id)).Concat(GetRightSkillNameByID(id)).Concat(GetBackSkillNameByID(id)).Concat(GetCheckingSkillNameByID(id)).Concat(GetDashSkillNameByID(id)).ToArray();
            var r1 = GetMainSkillNameByID(id);
            if (r1 != null)
            {
                for (int i = 0; i < r1.Length; i++)
                {
                    s3 += r1[i].Insert(0, "reloadskill ") + ";";
                }
            }
            else
            {
                s3 = "";
            }
            var r2 = GetLeftSkillNameByID(id);
            if (r2 != null)
            {
                for (int i = 0; i < r2.Length; i++)
                {
                    s4 += r2[i].Insert(0, "reloadskill ") + ";";
                }
            }
            else
            {
                s4 = "";
            }
            var r3 = GetRightSkillNameByID(id);
            if (r3 != null)
            {
                for (int i = 0; i < r3.Length; i++)
                {
                    s5 += r3[i].Insert(0, "reloadskill ") + ";";
                }
            }
            else
            {
                s5 = "";
            }
            var r4 = GetBackSkillNameByID(id);
            if (r4 != null)
            {
                for (int i = 0; i < r4.Length; i++)
                {
                    s6 += r4[i].Insert(0, "reloadskill ") + ";";
                }
            }
            else
            {
                s6 = "";
            }
            var r5 = GetCheckingSkillNameByID(id);
            if (r5 != null)
            {
                for (int i = 0; i < r5.Length; i++)
                {
                    s7 += r5[i].Insert(0, "reloadskill ") + ";";
                }
            }
            else
            {
                s7 = "";
            }
            var r6 = GetDashSkillNameByID(id);
            if (r6 != null)
            {
                for (int i = 0; i < r6.Length; i++)
                {
                    s8 += r6[i].Insert(0, "reloadskill ") + ";";
                }
            }
            else
            {
                s8 = "";
            }
            if (GetUnusedSkillNameByID(id) != null)
            {
                for (int i = 0; i < GetUnusedSkillNameByID(id).Length; i++)
                {
                    s9 += GetUnusedSkillNameByID(id)[i].Insert(0, "reloadskill ") + ";";
                }
            }
            else
            {
                s9 = "";
            }
            var r7 = UnusedSkillName(id);
            if (r7 != null)
            {
                for (int i = 0; i < r7.Length; i++)
                {
                    s10 += r7[i].Insert(0, "reloadskill ") + ";";
                }
            }
            else
            {
                s10 = "";
            }
            var r8 = CheckingSkillAndStopName(id);
            if (r8 != null)
            {
                for (int i = 0; i < r8.Length; i++)
                {
                    s11 += r8[i].Insert(0, "reloadskill ") + ";";
                }
            }
            else
            {
                s11 = "";
            }
            var r9 = CheckingSkillName(id);
            if (r9 != null)
            {
                for (int i = 0; i < r9.Length; i++)
                {
                    s12 += r9[i].Insert(0, "reloadskill ") + ";";
                }
            }
            else
            {
                s12 = "";
            }
            s = s1 + s2 + s3 + s4 + s5 + s6 + s7 + s8 + s9 + s10 + s11 + s12;
            if (s != "")
            {
                return s;
            }
            else
            {
                return s = "";
            }
        }
    }
#endif