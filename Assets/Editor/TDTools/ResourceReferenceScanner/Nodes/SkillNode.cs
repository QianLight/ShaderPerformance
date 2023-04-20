using CFEngine;
using EcsData;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace TDTools.ResourceScanner {
    public partial class ResourceReferenceScanner {

        #region Utility
        public static T DeserializeEcsData<T>(string path) {
            string json = "";
            byte[] bytes;
            using (var fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read)) {
                bytes = new byte[fs.Length + 1];
                fs.Read(bytes, 0, bytes.Length);
                //bytes[fs.Length] = 0;
            }
            SimpleTools.Unlock(ref bytes, 0, bytes.Length - 1);
            json = System.Text.Encoding.UTF8.GetString(bytes);
            return JsonUtility.FromJson<T>(json);
        }
        #endregion

        ScannerResult ScanSkill(string skillName, string presentID) {
            ScannerResult results = new ScannerResult();
            if (skillName == "" || _skillListForEnemyTable.Set.Contains(skillName)) {
                return results;
            }

            if (skillName.Contains("=") || skillName.Contains("|")) {
                string[] s = skillName.Split('=', '|');
                for (int i = 0; i < s.Length; i++)
                    results.AddRange(ScanSkill(s[i], presentID));
                return results;
            }

            if (!_skillListForEnemyTable.Dic.ContainsKey(skillName)) {
                if (_skillListForPet.Dic.ContainsKey(skillName)) {
                    return ScanPetSkill(skillName, presentID);
                } else {
                    results.MissingFiles.Add($"技能 {skillName}  不存在");
                    //Debug.Log($"找不到技能 {skillName}");
                    return results;
                }
            }

            if (!_presentationTable.Dic.ContainsKey(presentID)) {
                results.MissingFiles.Add($"技能{skillName} PresentID:{presentID} 不存在");
                //Debug.Log($"技能{skillName}找不到   PresentID:{presentID}");
                return results;
            }
            Dictionary<string, string> pRow = _presentationTable.Table[_presentationTable.Dic[presentID]];
            _skillListForEnemyTable.Set.Add(skillName);

            var row = _skillListForEnemyTable.Table[_skillListForEnemyTable.Dic[skillName]];

            if (row["BeginBuff"] != "")
                results.AddRange(ScanBuff(row["BeginBuff"], new ReferenceRecord(NodeType.XEntitySkill, skillName)));

            if (row["EndBuff"] != "")
                results.AddRange(ScanBuff(row["EndBuff"], new ReferenceRecord(NodeType.XEntitySkill, skillName)));

            if (row["ResistBuff"] != "")
                results.AddRange(ScanBuff(row["ResistBuff"], new ReferenceRecord(NodeType.XEntitySkill, skillName)));

            if (row["BuffID"] != "") {
                string[] buffs = row["BuffID"].Split('|');
                for (int i = 0; i < buffs.Length; i++) {
                    string[] s = buffs[i].Split('=');
                    if (s.Length < 3)
                        continue;
                    results.AddRange(ScanBuff($"{s[1]}={s[2]}", new ReferenceRecord(NodeType.XEntitySkill, skillName)));
                }
            }

            string path = $@"{DATAPATH}\BundleRes\SkillPackage\{pRow["SkillLocation"]}{skillName}".Replace('/', '\\');
            if (!File.Exists($@"{path}.bytes")) {
                results.MissingFiles.Add($@"技能文件:{skillName}.bytes 不存在");
                //Debug.Log($@"技能文件:{path}.bytes 不存在");
            } else {
                results.skillFiles.Add($"{path}.bytes".ToLower());
                results.AddRange(ScanSkillGraph($@"{path}.bytes", presentID));
            }

            if (!File.Exists($@"{path}.ecfg")) {
                results.MissingFiles.Add($@"技能文件:{skillName}.ecfg 不存在");
                //Debug.Log($@"技能文件:{path}.ecfg 不存在");
            } else {
                results.skillFiles.Add($"{path}.ecfg".ToLower());
            }

            return results;
        }

        ScannerResult ScanSkillGraph(string skillPath, string presentID) {
            XSkillData configData;
            ScannerResult results = new ScannerResult();
            if (!File.Exists(skillPath)) {
                results.MissingFiles.Add($"技能图{skillPath}不存在");
                //Debug.Log($"技能图{skillPath}不存在");
                return results;
            }
            if (_skillGraphSet.Contains(skillPath))
                return results;
            _skillGraphSet.Add(skillPath);
            try {
                configData = DeserializeEcsData<XSkillData>(skillPath);
            } catch (Exception e) {
                results.MissingFiles.Add($"{skillPath} {e.Message}");
                //Debug.Log($"{skillPath} {e.Message}");
                return results;
            }

            for (int i = 0; i < configData?.AnimationData.Count; i++) {
                var anim = configData.AnimationData[i];
                //Debug.LogWarning($@"扫描动画 {DATAPATH}\BundleRes\{anim.ClipPath}.anim".Replace('/','\\'));
                results.animationFiles.Add($@"{DATAPATH}\BundleRes\{anim.ClipPath}.anim".Replace('/', '\\').ToLower());
            }

            for (int i = 0; i < configData.BuffData.Count; i++) {
                var buff = configData.BuffData[i];
                results.AddRange(ScanBuff($"{buff.BuffID}={buff.BuffLevel}", new ReferenceRecord(NodeType.SkillGraph, skillPath), presentID));
                //Debug.LogWarning($"{buff.BuffID}={buff.BuffLevel}");
            }

            for (int i = 0; i < configData.FxData.Count; i++) {
                var fx = configData.FxData[i];
                if (fx.FxPath == "")
                    continue;
                string fileName = $"{fx.FxPath}.prefab".ToLower();
                if (_effectDictionary.ContainsKey(fileName)) {
                    results.FxFiles.Add(_effectDictionary[fileName].ToLower());
                } else {
                    results.MissingFiles.Add($"特效资源: {fileName} 不存在");
                    //Debug.Log($"fx: {fileName} not found!");
                }
            }

            ///扫描Trans
            for (int i = 0; i < configData.ScriptTransData.Count; i++) {
                var trans = configData.ScriptTransData[i];
                //results.AddRange(ScanSkill(trans.Name, presentID));
                string path = skillPath.Substring(0, skillPath.LastIndexOf('\\'));
                results.AddRange(ScanSkillGraph($@"{path}\{trans.Name}.bytes", presentID));
                results.skillFiles.Add($@"{path}\{trans.Name}.bytes".ToLower());
                //Debug.LogWarning($@"{path}\{trans.Name}.ecfg");
            }

            //for (int i = 0; i < configData.AudioData.Count; i++) {
            //    var audio = configData.AudioData[i];
            //    Debug.LogWarning($"{audio.AudioName}");
            //}

            return results;
        }
    }
}