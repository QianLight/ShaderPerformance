using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;

namespace TDTools.ResourceScanner {
    public partial class ResourceReferenceScanner {

        ScannerResult ScanPartnerShow(string id) {
            var results = new ScannerResult();
            var row = _partnerShowTable.Table[_partnerShowTable.Dic[id]];
            return results;
        }

        async Task<ScannerResult> ScanPartnerInfo(int index) {
            ScannerResult results = new ScannerResult();
            Dictionary<string, string> row = _roleTable.Table[index];
            results.AddRange(ScanPresentID(row["PresentId"]));
            if (row["InBornBuff"] != "")
                results.AddRange(ScanBuff(row["InBornBuff"], new ReferenceRecord(NodeType.Partner, row["ID"]), row["PresentId"]));

            int id = int.Parse(row["ID"]);

            if (_partnerShowTable.Dic.ContainsKey(row["ID"])) {
                results.AddRange(ScanPartnerShow(row["ID"]));
            }

            Dictionary<string, string> pRow = _presentationTable.Table[_presentationTable.Dic[row["PresentId"]]];

            void ScanSkill(string skillName) {
                if (_skillListForRoleSet.Contains(skillName)) {
                    return;
                }

                _skillListForRoleSet.Add(skillName);

                if (!_skillListForRoleDic.ContainsKey(skillName))
                    return;

                for (int i = 0; i < _skillListForRoleDic[skillName].Count; i++) {
                    var skillRow = _skillListForRole[_skillListForRoleDic[skillName][i]];

                    results.AddRange(ScanBuff(skillRow["ResistBuff"], new ReferenceRecord(NodeType.PartnerSkill, skillName), row["PresentId"]));
                    results.AddRange(ScanBuff(skillRow["BeginBuff"], new ReferenceRecord(NodeType.PartnerSkill, skillName), row["PresentId"]));
                    results.AddRange(ScanBuff(skillRow["EndBuff"], new ReferenceRecord(NodeType.PartnerSkill, skillName), row["PresentId"]));
                    results.AddRange(ScanBuff(skillRow["AllBuff"], new ReferenceRecord(NodeType.PartnerSkill, skillName), row["PresentId"]));
                    results.AddRange(ScanBuff(skillRow["OnBuff"], new ReferenceRecord(NodeType.PartnerSkill, skillName), row["PresentId"]));
                    results.AddRange(ScanBuff(skillRow["OffBuff"], new ReferenceRecord(NodeType.PartnerSkill, skillName), row["PresentId"]));
                    results.AddRange(ScanBuff(skillRow["OnBuffRemove"], new ReferenceRecord(NodeType.PartnerSkill, skillName), row["PresentId"]));
                    results.AddRange(ScanBuff(skillRow["OffBuffRemove"], new ReferenceRecord(NodeType.PartnerSkill, skillName), row["PresentId"]));

                    if (skillRow["BuffID"] != "") {
                        string[] buffs = skillRow["BuffID"].Split('|');
                        for (int j = 0; j < buffs.Length; j++) {
                            string[] s = buffs[j].Split('=');
                            if (s.Length < 3)
                                continue;
                            results.AddRange(ScanBuff($"{s[1]}={s[2]}", new ReferenceRecord(NodeType.PartnerSkill, skillName), row["PresentId"]));
                        }
                    }
                }

                //Debug.Log($@"扫描技能:{pRow["SkillLocation"]}{skillName}");
                string path = $@"{DATAPATH}\BundleRes\SkillPackage\{pRow["SkillLocation"]}{skillName}".Replace('/', '\\');

                if (!File.Exists($@"{path}.bytes")) {
                    results.MissingFiles.Add($@"技能文件:{skillName}.bytes 不存在");
                    //Debug.Log($@"技能文件:{path}.bytes 不存在");
                } else {
                    results.skillFiles.Add($"{path}.bytes".ToLower());
                    results.AddRange(ScanSkillGraph($@"{path}.bytes", row["PresentId"]));
                }

                if (!File.Exists($@"{path}.ecfg")) {
                    results.MissingFiles.Add($@"技能文件:{skillName}.ecfg 不存在");
                    //Debug.Log($@"技能文件:{path}.ecfg 不存在");
                } else {
                    results.skillFiles.Add($"{path}.ecfg".ToLower());
                }
            }


            ScanSkill(row["WinSkill"]);

            string[] switchs = row["Switch"].Split('|');
            for (int i = 0; i < switchs.Length; i++) {
                string[] s = switchs[i].Split('=');
                if (s.Length >= 2)
                    ScanSkill(s[1]);
            }

            ScanSkill(row["ExtremeDodgeSkills"]);

            if (!_skillListForRoleIndexer.ContainsKey(id))
                return results;

            for (int i = 0; i < _skillListForRoleIndexer[id].Count; i++) {
                string skill = _skillListForRole[_skillListForRoleIndexer[id][i]]["SkillScript"];
                ScanSkill(skill);
            }

            await Task.Yield();
            return results;
        }
    }
}
