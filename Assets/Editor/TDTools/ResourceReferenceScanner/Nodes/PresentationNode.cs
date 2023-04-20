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
                    results.MissingFiles.Add($"PresentID: {id} 不存在");
                    //Debug.Log($"找不到PresentID: {id}");
                }
                return results;
            }
            _presentationTable.Set.Add(id);

            Dictionary<string, string> row = _presentationTable.Table[_presentationTable.Dic[id]];
            //Debug.Log($"扫描PresentID: {id}{row["Name"]}");

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
                            results.MissingFiles.Add($"动画文件: {sp[j]} 不存在");
                            //Debug.Log($"动画文件: {path} 不存在");
                        }
                    }
                }
            }

            if (row["AvatarPos"] != "") {
                string path = $@"{DATAPATH}\BundleRes\Animation\{row["AnimLocation"]}{row["AvatarPos"]}.anim".Replace('/', '\\');
                if (File.Exists(path))
                    results.animationFiles.Add(path.ToLower());
                else {
                    results.MissingFiles.Add($"动画文件: {row["AvatarPos"]} 不存在");
                    //Debug.Log($"动画文件: {path} 不存在");
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
                    results.MissingFiles.Add($@"受击文件:{s[1]}.bytes 不存在");
                    //Debug.Log($@"受击文件:{path}.bytes 不存在");
                } else {
                    results.BehitFiles.Add($"{path}.bytes".ToLower());
                    results.AddRange(ScanBehitGraph($@"{path}.bytes", id));
                }

                if (!File.Exists($@"{path}.ecfg")) {
                    results.MissingFiles.Add($@"受击文件:{s[1]}.ecfg 不存在");
                    //Debug.Log($@"受击文件:{path}.ecfg 不存在");
                } else {
                    results.BehitFiles.Add($"{path}.ecfg".ToLower());
                }
            }

            //string[] skillCol = new string[] {};
            //for (int i = 0; i < animeCol.Lengt i++) {
            //    if (row[animeCol[i]] != "") {
            //        results.AddRange(ScanSkillow[animeCol[i]], id));
            //        Debug.LogWarning($"扫描技能 {r[animeCol[i]]}");
            //    }
            //}

            return results;
        }
    }
}
