using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace TDTools.ResourceScanner {
    public partial class ResourceReferenceScanner {
        ScannerResult ScanPresentID(string id) {
            ScannerResult results = new ScannerResult();
            if (_presentationTable.Set.Contains(id) || !_presentationTable.Dic.ContainsKey(id)) {
                if (!_presentationTable.Dic.ContainsKey(id)) {
                    results.MissingFiles.Add($"PresentID: {id} ������");
                    //Debug.Log($"�Ҳ���PresentID: {id}");
                }
                return results;
            }
            _presentationTable.Set.Add(id);

            Dictionary<string, string> row = _presentationTable.Table[_presentationTable.Dic[id]];
            //Debug.Log($"ɨ��PresentID: {id}{row["Name"]}");

            if (row["MoveFx"] != "") {
                results.FxFiles.Add($@"{DATAPATH}\BundleRes\{row["MoveFx"]}.prefab".Replace('/', '\\').ToLower());
                //Debug.LogWarning($@"{DATAPATH}\BundleRes\{row["MoveFx"]}.prefab");
            }
            string[] animeCol = new string[] { "Idle",
                "AttackIdle",
                "FishingIdle",
                "Walk",
                "AttackWalk",
                "Run",
                "AttackRun",
                "Brake",
                "RunLeft",
                "AttackRunLeft",
                "RunRight",
                "AttackRunRight",
                "Sheath",
                "Death",
                "OtherSkills",
                "Appear",
                "Disappear"
            };
            for (int i = 0; i < animeCol.Length; i++) {
                if (row[animeCol[i]] != "") {
                    string[] sp = row[animeCol[i]].Split('|');
                    for (int j = 0; j < sp.Length; j++) {
                        string path = $@"{DATAPATH}\BundleRes\Animation\{row["AnimLocation"]}{sp[j]}.anim".Replace('/', '\\');
                        if (File.Exists(path))
                            results.animationFiles.Add(path.ToLower());
                        else {
                            results.MissingFiles.Add($"�����ļ�: {sp[j]} ������");
                            //Debug.Log($"�����ļ�: {path} ������");
                        }
                    }
                }
            }

            if (row["AvatarPos"] != "") {
                string path = $@"{DATAPATH}\BundleRes\Animation\{row["AnimLocation"]}{row["AvatarPos"]}.anim".Replace('/', '\\');
                if (File.Exists(path))
                    results.animationFiles.Add(path.ToLower());
                else {
                    results.MissingFiles.Add($"�����ļ�: {row["AvatarPos"]} ������");
                    //Debug.Log($"�����ļ�: {path} ������");
                }
            }

            if (Directory.Exists($@"{DATAPATH}\BundleRes\HitPackage\{row["BehitLocation"]}")) {
                var files = GetAllFiles(new DirectoryInfo($@"{DATAPATH}\BundleRes\HitPackage\{row["BehitLocation"]}"), new string[] { ".ecfg", ".bytes" });
                for (int i = 0; i < files.Count; i++) {
                    if (files[i].Name.ToLower().Contains("_hit_header"))
                        results.BehitFiles.Add(files[i].FullName.ToLower());
                }
            }

            string[] behits = row["BeHit"].Split('|');
            for (int j = 0; j < behits.Length; j++) {
                string[] s = behits[j].Split('=');
                if (s.Length < 2) continue;
                string path = $@"{DATAPATH}\BundleRes\HitPackage\{row["BehitLocation"]}{s[1]}".Replace('/', '\\');



                if (!File.Exists($@"{path}.bytes")) {
                    results.MissingFiles.Add($@"�ܻ��ļ�:{s[1]}.bytes ������");
                    //Debug.Log($@"�ܻ��ļ�:{path}.bytes ������");
                } else {
                    results.BehitFiles.Add($"{path}.bytes".ToLower());
                    results.AddRange(ScanBehitGraph($@"{path}.bytes", id));
                }

                if (!File.Exists($@"{path}.ecfg")) {
                    results.MissingFiles.Add($@"�ܻ��ļ�:{s[1]}.ecfg ������");
                    //Debug.Log($@"�ܻ��ļ�:{path}.ecfg ������");
                } else {
                    results.BehitFiles.Add($"{path}.ecfg".ToLower());
                }
            }

            //string[] skillCol = new string[] {};
            //for (int i = 0; i < animeCol.Lengt i++) {
            //    if (row[animeCol[i]] != "") {
            //        results.AddRange(ScanSkillow[animeCol[i]], id));
            //        Debug.LogWarning($"ɨ�輼�� {r[animeCol[i]]}");
            //    }
            //}

            return results;
        }
    }
}
