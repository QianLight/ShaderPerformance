using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using CFClient;

namespace TDTools
{
    public class MonsterDesignerHelper : LevelDesignerHelper
    {
        private MonsterDHBindData curBindData;
        private SkillEditor skillEditor;
        private BehitEditor behitEditor;

        private bool editSkillValue = false;

        [MenuItem("Tools/TDTools/通用工具/TestLevelWin &X")]
        public static void ShowWindow()
        {
            var win = GetWindowWithRect<MonsterDesignerHelper>(new Rect(0, 0, 550, 830), true, "策划工具整合-boss");
            win.Show();
            win.Focus();
        }
        protected override void BindSpecialArea()
        {
            MonsterAreaRoot.style.display = DisplayStyle.Flex;
            curBindData = CreateInstance<MonsterDHBindData>();
            var so = new SerializedObject(curBindData);
            MonsterAreaRoot.Bind(so);
            RegisterGenBtn();
            RegisterPresent();
            RegisterStatis();
            RegisterAI();
            RegisterSkillList();
        }

        protected override void RefreshWhenFocus()
        {
            base.RefreshWhenFocus();
            if (curBindData == null)
                return;
            if (MonsterDHBindDataHelper.HasValue(curBindData.StatisticsID))
            {
                MonsterDHBindDataHelper.GenAllData(ref curBindData);
            }
            else if(Config.IDHistory != null && Config.IDHistory.Count > 0)
            {
                curBindData.InputID = Config.IDHistory.Last();
                MonsterDHBindDataHelper.GenAllData(ref curBindData);
            }
            RefreshLayout();
        }

        void RefreshLayout()
        {
            RefreshPresent();
            RefreshStatis();
            RefreshAI();
            RefreshSkillList();
        }
        void RegisterGenBtn()
        {
            MonsterAreaRoot.Q<Button>("MonsterGenDataBtn").RegisterCallback<MouseUpEvent>(obj =>
            {
                MonsterDHBindDataHelper.GenAllData(ref curBindData);
                AddConfigHistory(curBindData.InputID);
                RefreshLayout();
            });
            MonsterAreaRoot.Q<Button>("PresentRefreshBtn").RegisterCallback<MouseUpEvent>(obj =>
            {
                MonsterDHBindDataHelper.GenerateCurPresentData(true, ref curBindData);
                RefreshPresent();
            });
            MonsterAreaRoot.Q<Button>("StatisticsRefreshBtn").RegisterCallback<MouseUpEvent>(obj =>
            {
                MonsterDHBindDataHelper.GenerateCurStatisticsData(true, ref curBindData);
                RefreshStatis();
            });
            MonsterAreaRoot.Q<Button>("UnitAIRefreshBtn").RegisterCallback<MouseUpEvent>(obj =>
            {
                MonsterDHBindDataHelper.GenerateCurUnitAIData(true, ref curBindData);
                RefreshAI();
            });
        }

        void RegisterPresent()
        {
            RegisterOpenTable("PresentOpenBtn", "XEntityPresentation.txt");
            RegisterPingObject("PresentPingBtn", () => $"Assets/BundleRes/Prefabs/{curBindData.PrefabName}.prefab");
            RegisterPingAndOpen("AnimLocBtn", () => $"Assets/BundleRes/Animation/{curBindData.AnimLoc.TrimEnd('/')}");
            RegisterOpenFolder("AnimFolderBtn", () => $"BundleRes/Animation/{curBindData.AnimLoc.TrimEnd('/')}");
            RegisterPingAndOpen("CurveLocBtn", () => $"Assets/Editor/EditorResources/Curve/{curBindData.CurveLoc.TrimEnd('/')}");
            RegisterPingAndOpen("CurveFolderBtn", () => $"Editor/EditorResources/Curve/{curBindData.CurveLoc.TrimEnd('/')}");
            RegisterPingAndOpen("SkillLocBtn", () => $"Assets/BundleRes/SkillPackage/{curBindData.SkillLoc.TrimEnd('/')}");
            RegisterPingAndOpen("SkillFolderBtn", () => $"BundleRes/SkillPackage/{curBindData.SkillLoc.TrimEnd('/')}");
            RegisterPingAndOpen("BehitLocBtn", () => $"Assets/BundleRes/HitPackage/{curBindData.BehitLoc.TrimEnd('/')}");
            RegisterPingAndOpen("BehitFolderBtn", () => $"BundleRes/HitPackage/{curBindData.BehitLoc.TrimEnd('/')}");

            MonsterAreaRoot.Q<Button>("CurveHelperBtn").RegisterCallback<MouseUpEvent>(obj =>
            {
                CurveHelperWindow.ShowWindow();
            });
            MonsterAreaRoot.Q<Button>("CurveGenBtn").RegisterCallback<MouseUpEvent>(obj =>
            {
                XEditor.XCurveExtractor.CurveOpener();
            });
            MonsterAreaRoot.Q<Button>("HugeToolBtn").RegisterCallback<MouseUpEvent>(obj =>
            {
                XEditor.HugeEditor.Open((uint)curBindData.PresentID);
            });

            RegisterPingObject("IdleAnimBtn", 
                () => $"Assets/BundleRes/Animation/{curBindData.AnimLoc.TrimEnd('/')}/{curBindData.IdleAnim}.anim");
            RegisterPingObject("AttackIdleAnimBtn", 
                () => $"Assets/BundleRes/Animation/{curBindData.AnimLoc.TrimEnd('/')}/{curBindData.AttackIdleAnim}.anim");
            RegisterPingObject("WalkAnimBtn",
                () => $"Assets/BundleRes/Animation/{curBindData.AnimLoc.TrimEnd('/')}/{curBindData.WalkAnim}.anim");
            RegisterPingObject("AttackWalkAnimBtn",
                () => $"Assets/BundleRes/Animation/{curBindData.AnimLoc.TrimEnd('/')}/{curBindData.AttackWalkAnim}.anim");
            RegisterPingObject("RunAnimBtn",
                () => $"Assets/BundleRes/Animation/{curBindData.AnimLoc.TrimEnd('/')}/{curBindData.RunAnim}.anim");
            RegisterPingObject("AttackRunAnimBtn",
                () => $"Assets/BundleRes/Animation/{curBindData.AnimLoc.TrimEnd('/')}/{curBindData.AttackRunAnim}.anim");
            RegisterPingObject("DeadAnimBtn",
                () => $"Assets/BundleRes/Animation/{curBindData.AnimLoc.TrimEnd('/')}/{curBindData.DeadAnim}.anim");
            RegisterOpenSkillInEditor("AppearSkillBtn",
                () => $"{Application.dataPath}/BundleRes/SkillPackage/{curBindData.SkillLoc.TrimEnd('/')}/{curBindData.AppearSkill}.bytes");

            MonsterAreaRoot.Q<IMGUIContainer>("BehitSkillContain").onGUIHandler = DrawBehitSkill;
        }

        void RefreshPresent()
        {
            MonsterAreaRoot.Q<Foldout>("PresentRoot").style.display = MonsterDHBindDataHelper.HasValue(curBindData.PresentID)? DisplayStyle.Flex : DisplayStyle.None;
            MonsterAreaRoot.Q<Button>("HugeToolBtn").style.display = curBindData.Huge? DisplayStyle.Flex : DisplayStyle.None;
            MonsterAreaRoot.Q<Button>("AppearSkillBtn").visible = MonsterDHBindDataHelper.HasValue(curBindData.AppearSkill);
        }

        void RegisterStatis()
        {
            RegisterOpenTable("StatisticsOpenBtn", "XEntityStatistics.txt");
            RegisterOpenTable("FightGroupOpenBtn", "FightGroup.txt");

            MonsterAreaRoot.Q<IMGUIContainer>("CallerAttrListContain").onGUIHandler = DrawCallerAttrList;
            MonsterAreaRoot.Q<IMGUIContainer>("InBornBuffContain").onGUIHandler = DrawInBornBuff;
            MonsterAreaRoot.Q<IMGUIContainer>("ResistMagnification").onGUIHandler = DrawResistMagnification;
        }

        void RefreshStatis()
        {
            MonsterAreaRoot.Q<Foldout>("StatisticsRoot").style.display = MonsterDHBindDataHelper.HasValue(curBindData.StatisticsID) ? DisplayStyle.Flex : DisplayStyle.None;
            MonsterAreaRoot.Q<Label>("AttrCopyLabel").visible = MonsterDHBindDataHelper.HasValue(curBindData.AttrCopy);
            MonsterAreaRoot.Q<Foldout>("CallerAttrList").style.display = MonsterDHBindDataHelper.HasValue(curBindData.CallerAttrList) ? DisplayStyle.Flex : DisplayStyle.None;
            MonsterAreaRoot.Q<Foldout>("InBornBuff").style.display = MonsterDHBindDataHelper.HasValue(curBindData.InBornBuff) ? DisplayStyle.Flex : DisplayStyle.None;
            MonsterAreaRoot.Q<Foldout>("Resist").style.display = MonsterDHBindDataHelper.HasValue(curBindData.ResistValue) ? DisplayStyle.Flex : DisplayStyle.None;
            MonsterAreaRoot.Q<Foldout>("Stage").style.display = MonsterDHBindDataHelper.HasValue(curBindData.StageCondition) ? DisplayStyle.Flex : DisplayStyle.None;
            MonsterAreaRoot.Q<Foldout>("Mode").style.display = MonsterDHBindDataHelper.HasValue(curBindData.ModeBKMaxValue)? DisplayStyle.Flex : DisplayStyle.None;
            MonsterAreaRoot.Q<Foldout>("Patrol").style.display = MonsterDHBindDataHelper.HasValue(curBindData.PatrolID)? DisplayStyle.Flex : DisplayStyle.None;
        }

        void RegisterAI()
        {
            RegisterOpenTable("UnitAIOpenBtn", "UnitAITable.txt");
            MonsterAreaRoot.Q<IMGUIContainer>("SubTreeListContain").onGUIHandler = DrawSubTree;
            MonsterAreaRoot.Q<IMGUIContainer>("PreCombatSubTreeListContain").onGUIHandler = DrawPreSubTree;
            MonsterAreaRoot.Q<IMGUIContainer>("EventsListContain").onGUIHandler = DrawEventList;
        }

        void RefreshAI()
        {
            MonsterAreaRoot.Q<Foldout>("UnitAIRoot").style.display = MonsterDHBindDataHelper.HasValue(curBindData.UnitAIID) ? DisplayStyle.Flex : DisplayStyle.None;
            MonsterAreaRoot.Q<Foldout>("SubTree").style.display = MonsterDHBindDataHelper.HasValue(curBindData.SubTree) ? DisplayStyle.Flex : DisplayStyle.None;
            MonsterAreaRoot.Q<Foldout>("PreCombatSubTree").style.display = MonsterDHBindDataHelper.HasValue(curBindData.PreSubTree) ? DisplayStyle.Flex : DisplayStyle.None;
            MonsterAreaRoot.Q<Foldout>("EventsList").style.display = MonsterDHBindDataHelper.HasValue(curBindData.AIEvent) ? DisplayStyle.Flex : DisplayStyle.None;
        }

        void RegisterSkillList()
        {
            MonsterAreaRoot.Q<IMGUIContainer>("SkillListContainer").onGUIHandler = DrawAllSkillList;
        }

        void RefreshSkillList()
        {
            MonsterAreaRoot.Q<Foldout>("SkillListRoot").style.display = MonsterDHBindDataHelper.HasValue(curBindData.SkillList) ? DisplayStyle.Flex : DisplayStyle.None;
            MonsterAreaRoot.Q<Button>("SkillInspecterBtn").style.display = DisplayStyle.None;
        }

        void DrawBehitSkill()
        {
            if (!MonsterDHBindDataHelper.HasValue(curBindData.BehitSkill))
                return;
            try
            {
                var behitSkills = curBindData.BehitSkill.Split('|');
                for(int i = 0; i < behitSkills.Length; ++i)
                {
                    if (behitSkills[i] == string.Empty)
                        continue;
                    var item = behitSkills[i].Split('=');
                    EditorGUILayout.BeginHorizontal();
                    GUILayout.Label($"{item[0]}=", GUILayout.Width(30f));
                    if(GUILayout.Button(item[1], GUILayout.Width(400f)))
                    {
                        if (behitEditor == null)
                        {
                            behitEditor = GetWindow<BehitEditor>();
                        }
                        (behitEditor.CurrentGraph as BehitGraph).OpenData($"{Application.dataPath}/BundleRes/HitPackage/{curBindData.BehitLoc.TrimEnd('/')}/{item[1]}.bytes");
                    }
                    EditorGUILayout.EndHorizontal();
                }
            }
            catch
            {
                Debug.Log($"PresentID = {curBindData.PresentID}, 受击技能配置格式错误");
            }
        }

        void DrawCallerAttrList()
        {
            if (!MonsterDHBindDataHelper.HasValue(curBindData.CallerAttrList))
                return;
            try
            {
                var callerAttrs = curBindData.CallerAttrList.Split('|');
                for (int i = 0; i < callerAttrs.Length; ++i)
                {
                    var item = callerAttrs[i].Split('=');
                    uint.TryParse(item[0], out uint id);
                    var attrDesc = AttrDefineReader.AttrDefine.GetByAttrID(id).Comment;
                    EditorGUILayout.BeginHorizontal();
                    GUILayout.Label($"{id}={attrDesc}", GUILayout.Width(200f));
                    GUILayout.Space(30f);
                    var value = $"value={item[1]}";
                    if(item.Length>2)
                    {
                        value += " + parent";
                    }
                    else
                    {
                        value += "%";
                    }
                    GUILayout.Label(value, GUILayout.Width(200f));
                    EditorGUILayout.EndHorizontal();
                }
            }
            catch
            {
                Debug.Log($"StatisticsID = {curBindData.StatisticsID}, 继承属性配置格式错误");
            }
        }

        void DrawInBornBuff()
        {
            if (!MonsterDHBindDataHelper.HasValue(curBindData.InBornBuff))
                return;
            try
            {
                var inBornBuffs = curBindData.InBornBuff.Split('|');
                for (int i = 0; i < inBornBuffs.Length; ++i)
                {
                    var item = inBornBuffs[i].Split('=');
                    uint.TryParse(item[0], out uint id);
                    byte.TryParse(item[1], out byte level);
                    var buffName = BuffListReader.GetData(id, level).BuffName;
                    EditorGUILayout.BeginHorizontal();
                    GUILayout.Label($"{id}={buffName}", GUILayout.Width(300f));
                    GUILayout.Space(30f);
                    GUILayout.Label($"lv={level}", GUILayout.Width(100f));
                    EditorGUILayout.EndHorizontal();
                }
            }
            catch
            {
                Debug.Log($"StatisticsID = {curBindData.StatisticsID}, 天生buff配置格式错误");
            }
        }

        void DrawResistMagnification()
        {
            if (!MonsterDHBindDataHelper.HasValue(curBindData.ResistMagnification))
                return;
            try
            {
                var inBornBuffs = curBindData.ResistMagnification.Split('|');
                for (int i = 0; i < inBornBuffs.Length; ++i)
                {
                    var item = inBornBuffs[i].Split('=');
                    uint.TryParse(item[0], out uint id);
                    byte.TryParse(item[1], out byte level);
                    var stateName = EnemyResistReader.EnemyResist.GetByStateID(id).StateName;
                    EditorGUILayout.BeginHorizontal();
                    GUILayout.Label($"{id}={stateName}", GUILayout.Width(200f));
                    GUILayout.Space(30f);
                    GUILayout.Label($"倍率={level}", GUILayout.Width(200f));
                    EditorGUILayout.EndHorizontal();
                }
            }
            catch
            {
                Debug.Log($"StatisticsID = {curBindData.StatisticsID}, 异常状态配置格式错误");
            }
        }

        void DrawAllSkillList()
        {
            if (!MonsterDHBindDataHelper.HasValue(curBindData.SkillList))
                return;
            if (!isRuntime && editSkillValue)
            {
                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("关闭模式", GUILayout.Width(130f)))
                {
                    EnemySkillHelpCfgMgr.SaveTempFile();
                    editSkillValue = false;
                }
                EditorGUILayout.Space(40f);
                if (GUILayout.Button("全部保存", GUILayout.Width(130f)))
                {
                    EnemySkillHelpCfgMgr.SaveToDisk();
                    editSkillValue = false;
                }
                if (GUILayout.Button("清除缓存", GUILayout.Width(130f)))
                {
                    EnemySkillHelpCfgMgr.ClearTempFile();
                    EnemySkillHelpCfgMgr.Build();
                }
                EditorGUILayout.EndHorizontal();
            }
            else if(!isRuntime)
            {
                if (GUILayout.Button("开启编辑模式", GUILayout.Width(300f)))
                {
                    EnemySkillHelpCfgMgr.Build();
                    editSkillValue = true;
                }
            }
            foreach (var item in curBindData.SkillList)
            {
                try
                {
                    EditorGUILayout.BeginHorizontal();
                    if(GUILayout.Button(item, GUILayout.Width(350f)))
                    {
                        if (skillEditor == null)
                        {
                            skillEditor = GetWindow<SkillEditor>();
                        }
                        (skillEditor.CurrentGraph as SkillGraph).OpenData(
                            $"{Application.dataPath}/BundleRes/SkillPackage/{curBindData.SkillLoc.TrimEnd('/')}/{item}.bytes");
                    }
                    if(isRuntime)
                    {
                        if (GUILayout.Button("Play", GUILayout.Width(50f)))
                        {
                            #if USE_GM
                            CFCommand.singleton.ProcessServerCommand($"monstercastskill {curBindData.StatisticsID} {item}");
                            #endif
                            Debug.Log($"monstercastskill {curBindData.StatisticsID} {item}");
                        }
                        EditorGUILayout.EndHorizontal();
                    }
                    else
                    {
                        EditorGUILayout.EndHorizontal();
                        if(editSkillValue)
                        {
                            if (EnemySkillHelpCfgMgr.GetCfg(item, curBindData.StatisticsID, out EnemySkillHelpCfg data))
                            {
                                data.SkillCustomType = EditorGUILayout.Popup(data.SkillCustomType, EnemySkillHelpCfg.SkillCustomName);
                                data.DecreaseSuperArmor = EditorGUILayout.TextField("破霸体值", data.DecreaseSuperArmor);
                                data.IncreaseSuperArmor = EditorGUILayout.TextField("技能霸体值", data.IncreaseSuperArmor);
                                data.MagicDeSuperArmor = EditorGUILayout.TextField("法术破霸体值", data.MagicDeSuperArmor);
                                data.MagicInSuperArmor = EditorGUILayout.TextField("法术技能霸体值", data.MagicInSuperArmor);
                                if(!EnemySkillHelpCfgMgr.Check(data))
                                {
                                    EditorGUILayout.LabelField("校验未通过，请确认");
                                }
                                EditorGUILayout.Space();
                            }
                        }
                    }
                    
                }
                catch(Exception e)
                {
                    Debug.Log($"{item} 技能脚本加载错误 =={e}");
                }
            }
            if (!isRuntime && editSkillValue)
            {
                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("关闭模式", GUILayout.Width(130f)))
                {
                    EnemySkillHelpCfgMgr.SaveTempFile();
                    editSkillValue = false;
                }
                EditorGUILayout.Space(40f);
                if (GUILayout.Button("全部保存", GUILayout.Width(130f)))
                {
                    EnemySkillHelpCfgMgr.SaveToDisk();
                    editSkillValue = false;
                }
                EditorGUILayout.EndHorizontal();
            }
        }

        void DrawSubTree()
        {
            if (!MonsterDHBindDataHelper.HasValue(curBindData.SubTree))
                return;
            try
            {
                var subtrees = curBindData.SubTree.Split('|');
                for (int i = 0; i < subtrees.Length; ++i)
                {
                    var item = subtrees[i];
                    EditorGUILayout.BeginHorizontal();
                    GUILayout.Label($"{item}", GUILayout.Width(400f));
                    EditorGUILayout.EndHorizontal();
                }
            }
            catch
            {
                Debug.Log($"UnitAIID = {curBindData.UnitAIID}, 通用子树配置格式错误");
            }
        }
        void DrawPreSubTree()
        {
            if (!MonsterDHBindDataHelper.HasValue(curBindData.PreSubTree))
                return;
            try
            {
                var subtrees = curBindData.PreSubTree.Split('|');
                for (int i = 0; i < subtrees.Length; ++i)
                {
                    var item = subtrees[i];
                    EditorGUILayout.BeginHorizontal();
                    GUILayout.Label($"{item}", GUILayout.Width(400f));
                    EditorGUILayout.EndHorizontal();
                }
            }
            catch
            {
                Debug.Log($"UnitAIID = {curBindData.UnitAIID}, 通用前置子树配置格式错误");
            }
        }

        void DrawEventList()
        {
            if (!MonsterDHBindDataHelper.HasValue(curBindData.AIEvent))
                return;
            try
            {
                var subtrees = curBindData.AIEvent.Split('|');
                for (int i = 0; i < subtrees.Length; ++i)
                {
                    var item = subtrees[i];
                    EditorGUILayout.BeginHorizontal();
                    GUILayout.Label($"{item}", GUILayout.Width(400f));
                    EditorGUILayout.EndHorizontal();
                }
            }
            catch
            {
                Debug.Log($"UnitAIID = {curBindData.UnitAIID}, 监听事件配置格式错误");
            }
        }
        void RegisterOpenTable(string btnName, string tableName, string selectRow = null, string selectValue = null)
        {
            CustomizationTableData table;
            try
            {
                table = Config.Table.First(item => item.TableName == tableName);
            }
            catch
            {
                if(!DefaultTableConfig.TryGetValue(tableName, out table))
                {
                    Debug.Log($"{tableName} 表配置不存在");
                    return;
                }
            }
            MonsterAreaRoot.Q<Button>(btnName).RegisterCallback<MouseUpEvent>(obj =>
            {
                OpenWpsHelper.OpenWps($"{Config.TablePath[TablePathField.index]}", table.TableName, table.TableFreezeRange, selectRow, selectValue);
            });
        }

        void RegisterOpenFolder(string btnName, Func<string> folder)
        {
            MonsterAreaRoot.Q<Button>(btnName).RegisterCallback<MouseUpEvent>(obj =>
            {
                EditorUtility.RevealInFinder($"{Application.dataPath}/{folder.Invoke()}");
            });
        }

        void RegisterPingObject(string btnName, Func<string> getPath)
        {
            MonsterAreaRoot.Q<Button>(btnName).RegisterCallback<MouseUpEvent>(obj =>
            {
                UnityEngine.Object go = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(getPath.Invoke());
                EditorGUIUtility.PingObject(go);
            });
        }
        void RegisterPingAndOpen(string btnName, Func<string> getPath)
        {
            MonsterAreaRoot.Q<Button>(btnName).RegisterCallback<MouseUpEvent>(obj =>
            {
                UnityEngine.Object go = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(getPath.Invoke());
                //EditorGUIUtility.PingObject(go);
                AssetDatabase.OpenAsset(go);
            });
        }
        void RegisterOpenSkillInEditor(string btnName, Func<string> getPath)
        {
            MonsterAreaRoot.Q<Button>(btnName).RegisterCallback<MouseUpEvent>(obj =>
            {
                if(skillEditor == null)
                {
                    skillEditor = GetWindow<SkillEditor>();
                }
                (skillEditor.CurrentGraph as SkillGraph).OpenData(getPath.Invoke());
            });
        }

        void AddConfigHistory(int id)
        {
            if (!MonsterDHBindDataHelper.HasValue(id))
                return;
            if (Config.IDHistory.Contains(id))
            {
                Config.IDHistory.Remove(id);
            }
            Config.IDHistory.Add(id);
            if (Config.IDHistory.Count > Config.MaxHistory)
                Config.IDHistory.RemoveAt(0);
            WriteConfigToDisk();
        }
    }
}
