using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;
using UnityEngine.Profiling;
namespace CFEngine.Editor
{
    public class SFXInfo
    {
        public GameObject prefab;
        public long size;
        public string info;
        public SFXContext context;
    }
    

    public class SFXConfigTool : BaseConfigTool<EditorSFXData>
    {
        enum OpSFXConfigType
        {
            OpNone,
            OpScan,
            OpClear,
            OpSort,
            OpSaveResult
        }
        enum OpSFXSortType
        {
            RenderCount,
            ParticleCount,
            Size
        }
        private OpSFXConfigType opSFXType = OpSFXConfigType.OpNone;
        private Vector2 scanResultScroll = Vector2.zero;
        private Vector2 scanShaderScroll = Vector2.zero;
        private List<SFXInfo> sfxInfos = new List<SFXInfo> ();
        private int scanIndex = -1;
        private bool savePrefab = false;
        private bool saveConfig = false;
        private bool saveOverride = false;
        private OpSFXSortType sortType = OpSFXSortType.RenderCount;

        private int sfxTypeGridId;
        private int sfxProfileLevelGridId;

        private bool isAddingNewFolder;
        private bool isAddingNewDoc;
        private string newName;
        private int newType;
        private Vector2 _skillTypeByFolderListPos;
        private Vector2 _skillTypeByDocListPos;
        
        private GUIStyle bold;
        public override void OnInit ()
        {
            base.OnInit ();
            config = EditorSFXData.instance;
            
            

            bold = new GUIStyle() {fontStyle = FontStyle.Bold, normal = new GUIStyleState(){textColor = Color.white}, fontSize = 12, alignment = TextAnchor.MiddleLeft};
            isAddingNewFolder = false;
            isAddingNewDoc = false;
            newName = String.Empty;
            newType = 0;
            
        }

        protected override void OnConfigGui (ref Rect rect)
        {
            if (config.folder.FolderGroup ("SFXConfig", "SFXConfig", rect.width))
            {
                EditorGUILayout.BeginHorizontal ();
                if (GUILayout.Button ("Add", GUILayout.MaxWidth (80)))
                {
                    config.dirs.Add ("");

                }
                if (GUILayout.Button ("Scan", GUILayout.MaxWidth (80)))
                {
                    scanIndex = -1;
                    opSFXType = OpSFXConfigType.OpScan;
                }
                if (GUILayout.Button ("Save", GUILayout.MaxWidth (80)))
                {
                    scanIndex = -1;
                    opSFXType = OpSFXConfigType.OpScan;
                }
                if (sfxInfos.Count > 0)
                {
                    if (GUILayout.Button ("SortByRender", GUILayout.MaxWidth (120)))
                    {
                        opSFXType = OpSFXConfigType.OpSort;
                        sortType = OpSFXSortType.RenderCount;
                    }
                    if (GUILayout.Button ("SortByParticle", GUILayout.MaxWidth (120)))
                    {
                        opSFXType = OpSFXConfigType.OpSort;
                        sortType = OpSFXSortType.ParticleCount;
                    }
                    if (GUILayout.Button ("SortBySize", GUILayout.MaxWidth (120)))
                    {
                        opSFXType = OpSFXConfigType.OpSort;
                        sortType = OpSFXSortType.Size;
                    }
                }
                EditorGUILayout.EndHorizontal ();
                EditorGUILayout.BeginHorizontal ();
                savePrefab = EditorGUILayout.Toggle ("SavePrefab", savePrefab, GUILayout.MaxWidth (300));
                saveConfig = EditorGUILayout.Toggle ("SaveConfig", saveConfig, GUILayout.MaxWidth (300));
                saveOverride = EditorGUILayout.Toggle ("SaveOverride", saveOverride, GUILayout.MaxWidth (300));
                EditorGUILayout.EndHorizontal ();
                EditorGUILayout.BeginHorizontal ();
                SFXWrapper.context.filterType = (SFXFilterType) EditorGUILayout.EnumPopup ("FilterType",
                    SFXWrapper.context.filterType, GUILayout.MaxWidth (300));
                SFXWrapper.context.filterStr = EditorGUILayout.TextField (SFXWrapper.context.filterStr, GUILayout.MaxWidth (200));
                EditorGUILayout.EndHorizontal ();
                DeleteInfo dirDelete = new DeleteInfo ();
                dirDelete.BeginDelete ();
                for (int i = 0; i < config.dirs.Count; ++i)
                {
                    string dir = config.dirs[i];
                    EditorGUILayout.BeginHorizontal ();
                    ToolsUtility.FolderSelect (ref dir);
                    config.dirs[i] = dir;
                    if (GUILayout.Button ("Scan", GUILayout.MaxWidth (80)))
                    {
                        scanIndex = i;
                        opSFXType = OpSFXConfigType.OpScan;
                        saveConfig = false;
                    }
                    if (GUILayout.Button ("Save", GUILayout.MaxWidth (80)))
                    {
                        scanIndex = i;
                        opSFXType = OpSFXConfigType.OpScan;
                        saveConfig = true;
                    }
                    if (GUILayout.Button ("Clear", GUILayout.MaxWidth (80)))
                    {
                        opSFXType = OpSFXConfigType.OpClear;
                    }
                    dirDelete.RemveButton (i);
                    EditorGUILayout.EndHorizontal ();
                }
                dirDelete.EndDelete (config.dirs);

                if (sfxInfos.Count > 0)
                {
                    EditorGUILayout.BeginHorizontal ();
                    EditorGUILayout.LabelField (string.Format ("TotalCount:{0}", sfxInfos.Count), GUILayout.MaxWidth (100));
                    if (GUILayout.Button ("Save", GUILayout.MaxWidth (80)))
                    {
                        opSFXType = OpSFXConfigType.OpSaveResult;
                    }
                    EditorGUILayout.EndHorizontal ();
                    // totalLine = 0;
                    // EditorCommon.BeginScroll (ref scanResultScroll, totalLine, 10, 600, rect.width - 20);
                    // for (int i = 0; i < sfxInfos.Count; ++i)
                    // {
                    //     var sfxInfo = sfxInfos[i];
                    //     ref var context = ref sfxInfo.context;
                    //     EditorGUILayout.BeginHorizontal ();
                    //     EditorGUILayout.ObjectField ("", sfxInfo.prefab, typeof (GameObject), false, GUILayout.MaxWidth (300));
                    //     EditorGUILayout.LabelField (string.Format ("render {0} particle {1} size {2}",
                    //             context.renderCount.ToString (),
                    //             context.psCount.ToString (),
                    //             EditorUtility.FormatBytes (sfxInfo.size)),
                    //         GUILayout.MaxWidth (300));
                    //     EditorGUILayout.EndHorizontal ();
                    //     EditorGUILayout.BeginHorizontal ();
                    //     EditorGUI.indentLevel++;
                    //     EditorGUILayout.TextArea (sfxInfo.info, GUILayout.MaxWidth (rect.width - 200));
                    //     EditorGUI.indentLevel--;
                    //     EditorGUILayout.EndHorizontal ();
                    //     totalLine += 1 + context.lineCount;
                    // }
                    // EditorCommon.EndScroll ();
                }
                //int count = SFXWrapper.sfxShader.Count + SFXWrapper.monos.Count;
                //if (count > 0)
                //{
                //    EditorCommon.BeginScroll (ref scanShaderScroll, count, 10, 600, rect.width - 20);
                //    var shaderIt = SFXWrapper.sfxShader.GetEnumerator ();
                //    while (shaderIt.MoveNext ())
                //    {
                //        var shader = shaderIt.Current;
                //        EditorGUILayout.BeginHorizontal ();
                //        EditorGUILayout.ObjectField ("", shader, typeof (Shader), false, GUILayout.MaxWidth (300));
                //        EditorGUILayout.EndHorizontal ();
                //    }
                //    var monoIt = SFXWrapper.monos.GetEnumerator ();
                //    while (monoIt.MoveNext ())
                //    {
                //        var mono = monoIt.Current;
                //        EditorGUILayout.BeginHorizontal ();
                //        EditorGUILayout.LabelField (mono, GUILayout.MaxWidth (200));
                //        EditorGUILayout.EndHorizontal ();

                //    }
                //    EditorCommon.EndScroll ();
                //}
                EditorCommon.EndFolderGroup ();
            }

            if (config.folder.FolderGroup("SFXExecutiveStandard", "SFXExecutiveStandard", rect.width))
            {
                config.profileLevels = EditorGUILayout.IntField("技能标准分级", config.profileLevels);
                CheckSettingLength();
                string[] typeName = new string[config.profileLevels];/*{"缺省", "主要技能", "次要技能", "受击"};*/
                for (int i = 0; i < typeName.Length; i++)
                {
                    typeName[i] = config.settingType[i].exampleInfo;
                }

                sfxTypeGridId = GUILayout.SelectionGrid(sfxTypeGridId, typeName, config.profileLevels);
                sfxTypeGridId = sfxTypeGridId > (config.profileLevels-1) ? (config.profileLevels-1) : sfxTypeGridId;
            
                SFXProfileSettings currentType = config.settingType[sfxTypeGridId];
                currentType.countInfo = EditorGUILayout.TextField("最大数量限制", currentType.countInfo);
                currentType.exampleInfo = EditorGUILayout.TextField("使用环境", currentType.exampleInfo);
                using (new EditorGUILayout.HorizontalScope())
                {
                    using (new EditorGUILayout.VerticalScope())
                    {
                        GUILayout.Label("高配", bold);
                        EditorGUILayout.Space(10);
                        DrawProfileProferties(currentType.profileLevels[0]);
                    }
                    using (new EditorGUILayout.VerticalScope())
                    {
                        GUILayout.Label("中配", bold);
                        EditorGUILayout.Space(10);
                        DrawProfileProferties(currentType.profileLevels[1]);
                    }
                    using (new EditorGUILayout.VerticalScope())
                    {
                        GUILayout.Label("低配", bold);
                        EditorGUILayout.Space(10);
                        DrawProfileProferties(currentType.profileLevels[2]);
                    }    
                }
                EditorCommon.EndFolderGroup();
            }
            
            if (config.folder.FolderGroup("SFXType",  "SFXType", rect.width))
            {
                string[] option = new string[EditorSFXData.instance.profileLevels];
                for (int i = 0; i < EditorSFXData.instance.profileLevels; i++)
                {
                    option[i] = EditorSFXData.instance.settingType[i].exampleInfo;
                }

                using (new EditorGUILayout.HorizontalScope(GUILayout.Width(820)))
                {
                    using (new EditorGUILayout.VerticalScope(GUILayout.Width(400)))
                    {
                        using (var scrollView = new EditorGUILayout.ScrollViewScope(_skillTypeByFolderListPos,
                            GUILayout.Width(400), GUILayout.Height(450)))
                        {
                            _skillTypeByFolderListPos = scrollView.scrollPosition;
                            
                            for (var index = 0; index < config.skillTypeByFolder.Count; index++)
                            {
                                var type = config.skillTypeByFolder[index];
                                using (new EditorGUILayout.HorizontalScope(GUILayout.Width(400)))
                                {
                                    EditorGUILayout.LabelField(type.skillName, GUILayout.Width(250));
                                    type.skillType = EditorGUILayout.Popup(type.skillType, option, GUILayout.Width(100));
                                    if (GUILayout.Button("Del", GUILayout.Width(30)))
                                    {
                                        config.skillTypeByFolder.RemoveAt(index);
                                        continue;
                                    }
                                    config.skillTypeByFolder[index] = type;
                                }
                            }
                            
                        }
                        if (!isAddingNewFolder)
                        {
                            if (GUILayout.Button("Manual Add Folder"))
                            {
                                isAddingNewFolder = true;
                            }
                        }
                        else
                        {
                            using (new EditorGUILayout.HorizontalScope(GUILayout.Width(400)))
                            {
                                newName = EditorGUILayout.TextField("Name", newName);
                                newType = EditorGUILayout.Popup("Type", newType, option);
                                if (GUILayout.Button("Add"))
                                {
                                    config.skillTypeByFolder.Add(new SkillProfileType()
                                        {skillName = newName, skillType = newType});
                                    isAddingNewFolder = false;
                                }
                            }
                        }
                    }
                    using (new EditorGUILayout.VerticalScope(GUILayout.Width(400)))
                    {
                        using (var scrollview = new EditorGUILayout.ScrollViewScope(_skillTypeByDocListPos,
                            GUILayout.Width(400), GUILayout.Height(450)))
                        {
                            _skillTypeByDocListPos = scrollview.scrollPosition;
                            for (var index = 0; index < config.skillTypeByDoc.Count; index++)
                            {
                                var type = config.skillTypeByDoc[index];
                                using (new EditorGUILayout.HorizontalScope(GUILayout.Width(400)))
                                {
                                    EditorGUILayout.LabelField(type.skillName, GUILayout.Width(250));
                                    type.skillType = EditorGUILayout.Popup(type.skillType, option, GUILayout.Width(100));
                                    if (GUILayout.Button("Del", GUILayout.Width(30)))
                                    {
                                        config.skillTypeByDoc.RemoveAt(index);
                                        continue;
                                    }

                                    config.skillTypeByDoc[index] = type;
                                }
                            }
                        }
                        if (!isAddingNewDoc)
                        {
                            if (GUILayout.Button("Manual Add Doc"))
                            {
                                isAddingNewDoc = true;
                            }
                        }
                        else
                        {
                            using (new EditorGUILayout.HorizontalScope(GUILayout.Width(400)))
                            {
                                newName = EditorGUILayout.TextField("Name", newName);
                                newType = EditorGUILayout.Popup("Type", newType, option);
                                if (GUILayout.Button("Add"))
                                {
                                    config.skillTypeByDoc.Add(new SkillProfileType()
                                        {skillName = newName, skillType = newType});
                                    isAddingNewDoc = false;
                                }
                            }
                        }

                    }

                    // using (new EditorGUILayout.VerticalScope(GUILayout.Width(400)))
                    // {
                    //     
                    // }
                }




                EditorCommon.EndFolderGroup();
            }
        }

        private void CheckSettingLength()
        {
            RemoveExtraSettings();
            AddLackSettings();
        }

        private void RemoveExtraSettings()
        {
            bool stillOverflow = config.settingType.Count > config.profileLevels;
            if (stillOverflow)
            {
                config.settingType.RemoveAt(config.settingType.Count-1);
                RemoveExtraSettings();
            }
        }

        private void AddLackSettings()
        {
            bool stillLack = config.settingType.Count < config.profileLevels;
            if (stillLack)
            {
                config.settingType.Add(new SFXProfileSettings());
                AddLackSettings();
            }
        }
        protected override void OnConfigUpdate ()
        {

            switch (opSFXType)
            {
                case OpSFXConfigType.OpScan:
                    ScanSfx ();
                    break;
                case OpSFXConfigType.OpClear:
                    ClearSfx ();
                    break;
                case OpSFXConfigType.OpSort:
                    SortSfx ();
                    break;
                case OpSFXConfigType.OpSaveResult:
                    SaveResult ();
                    break;

            }
            opSFXType = OpSFXConfigType.OpNone;
        }
        private List<ObjectInfo> GetPrefabs ()
        {
            List<string> dirs = new List<string> ();
            if (scanIndex >= 0)
            {
                dirs.Add (config.dirs[scanIndex]);
            }
            else
            {
                dirs.AddRange (config.dirs);
            }
            List<ObjectInfo> prefabs = new List<ObjectInfo> ();
            for (int i = 0; i < dirs.Count; ++i)
            {
                CommonAssets.GetObjectsInfolder<GameObject> (dirs[i], prefabs, true, "*.prefab");
            }
            return prefabs;
        }

        private void ClearSfx ()
        {
            var prefabs = GetPrefabs ();
            var runtimePrefabs = new List<ObjectInfo> ();
            CommonAssets.GetObjectsInfolder<GameObject> (AssetsConfig.instance.sfxDir, runtimePrefabs, true, "*.prefab");
            HashSet<string> prefabNames = new HashSet<string> ();
            for (int i = 0; i < prefabs.Count; ++i)
            {
                var prefab = prefabs[i].obj as GameObject;
                prefabNames.Add (prefab.name.ToLower ());
            }

            for (int i = 0; i < runtimePrefabs.Count; ++i)
            {
                var prefab = runtimePrefabs[i].obj as GameObject;
                EditorUtility.DisplayProgressBar (string.Format ("CleaSFX-{0}/{1}", i, runtimePrefabs.Count),
                    runtimePrefabs[i].path, (float) i / runtimePrefabs.Count);
                if (!prefabNames.Contains (prefab.name.ToLower ()))
                {
                    string prefabPath = runtimePrefabs[i].path;
                    AssetDatabase.DeleteAsset (prefabPath);
                    prefabPath = prefabPath.Replace (".prefab", ".asset");
                    AssetDatabase.DeleteAsset (prefabPath);
                    prefabPath = prefabPath.Replace(".asset", ".bytes");
                    AssetDatabase.DeleteAsset(prefabPath);
                }
            }
            EditorUtility.DisplayDialog ("Finish", "All assets processed finish", "OK");
            EditorUtility.ClearProgressBar ();
            AssetDatabase.SaveAssets ();
        }

        private void ScanSfx ()
        {
            // var prefabs = GetPrefabs ();

            // totalLine = 0;
            // SFXWrapper.ClearContext ();
            // sfxInfos.Clear ();
            // Dictionary<string, List<string>> duplicateSFX = new Dictionary<string, List<string>> ();
            // for (int i = 0; i < prefabs.Count; ++i)
            // {
            //     SFXWrapper.InitContext ();
            //     var prefab = prefabs[i].obj as GameObject;
            //     string name = prefab.name.ToLower ();
            //     List<string> sfx;
            //     if (!duplicateSFX.TryGetValue (name, out sfx))
            //     {
            //         sfx = new List<string> ();
            //         duplicateSFX.Add (name, sfx);
            //         EditorUtility.DisplayProgressBar (string.Format ("ScanSFX-{0}/{1}", i, prefabs.Count),
            //             prefabs[i].path, (float) i / prefabs.Count);

            //         SFXWrapper sfxWrapper;
            //         if (!prefab.TryGetComponent (out sfxWrapper))
            //         {
            //             sfxWrapper = prefab.AddComponent<SFXWrapper> ();
            //         }
            //         if (saveConfig || savePrefab)
            //         {
            //             sfxWrapper.Save (prefab, savePrefab, saveConfig,saveOverride);
            //         }
            //         else if (SFXWrapper.context.filterType == SFXFilterType.None)
            //         {
            //             SFXWrapper.PreFilterSfx (prefab.transform, prefab.transform);
            //             SFXWrapper.AnalyzeSfx (prefab.transform, null);
            //             SFXWrapper.context.hasFilterResult = true;
            //             if (SFXWrapper.context.fatelError)
            //             {
            //                 DebugLog.AddErrorLog2 ("fatal error:{0}", prefab.name);
            //             }
            //         }
            //         else
            //         {
            //             SFXWrapper.FilterSfx (prefab.transform);
            //         }

            //         if (SFXWrapper.context.hasFilterResult)
            //         {
            //             var fi = new FileInfo (AssetDatabase.GetAssetPath (prefab));
            //             var si = new SFXInfo ()
            //             {
            //                 prefab = prefab,
            //                 size = fi.Length,
            //                 info = SFXWrapper.sb.ToString (),
            //             };
            //             si.context.Copy (ref SFXWrapper.context);
            //             sfxInfos.Add (si);
            //         }
            //     }
            //     sfx.Add (prefabs[i].path);

            // }
            // saveConfig = false;
            // AssetDatabase.SaveAssets ();
            // EditorUtility.ClearProgressBar ();
            // EditorUtility.DisplayDialog ("Finish", "All assets processed finish", "OK");

            // var it = duplicateSFX.GetEnumerator ();
            // while (it.MoveNext ())
            // {
            //     if (it.Current.Value.Count > 1)
            //     {
            //         SFXWrapper.sb.Clear ();
            //         SFXWrapper.sb.AppendLine (string.Format ("same name sfx:{0}", it.Current.Key));
            //         var sfx = it.Current.Value;
            //         for (int i = 0; i < sfx.Count; ++i)
            //         {
            //             SFXWrapper.sb.AppendLine (sfx[i]);
            //         }
            //         DebugLog.AddErrorLog (SFXWrapper.sb.ToString ());
            //     }
            // }

            // sortType = OpSFXSortType.RenderCount;
            // SortSfx ();

        }
        private void SortSfx ()
        {
            switch (sortType)
            {
                case OpSFXSortType.RenderCount:
                    sfxInfos.Sort ((x, y) => y.context.renderCount.CompareTo (x.context.renderCount));
                    break;
                case OpSFXSortType.ParticleCount:
                    sfxInfos.Sort ((x, y) => y.context.psCount.CompareTo (x.context.psCount));
                    break;
                case OpSFXSortType.Size:
                    sfxInfos.Sort ((x, y) => y.size.CompareTo (x.size));
                    break;
            }
        }

        private void SaveResult ()
        {
            // for (int i = 0; i < sfxInfos.Count; ++i)
            // {
            //     var sfxInfo = sfxInfos[i];
            //     if (sfxInfo.prefab != null)
            //     {
            //         if (sfxInfo.prefab.TryGetComponent (out SFXWrapper sfx))
            //         {
            //             EditorUtility.DisplayProgressBar (string.Format ("SaveSFX-{0}/{1}", i, sfxInfos.Count),
            //                 sfxInfo.prefab.name, (float) i / sfxInfos.Count);
            //             sfx.Save (sfxInfo.prefab);
            //         }
            //     }
            // }
            EditorUtility.ClearProgressBar ();
            EditorUtility.DisplayDialog ("Finish", "All assets processed finish", "OK");
        }

        private void DrawProfileProferties(SFXProfileProperties current)
        {
            current.pCount = EditorGUILayout.IntField("最大粒子数", current.pCount);
            // current.delay = EditorGUILayout.FloatField("最长结束延迟", current.delay);
            current.psCount = EditorGUILayout.IntField("最大粒子系统数", current.psCount);
            current.fillrate = EditorGUILayout.FloatField("最大填充率", current.fillrate);
            current.batches = EditorGUILayout.IntField("最大Batch", current.batches);
            current.fillrateArea = EditorGUILayout.FloatField("最大填充总量", current.fillrateArea);
        }

    }
}