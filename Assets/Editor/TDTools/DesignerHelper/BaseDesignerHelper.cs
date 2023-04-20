using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEditor.SceneManagement;
using CFClient;

namespace TDTools
{
    public class BaseDesignerHelper : EditorWindow
    {
        private const string BaseDesignerHelperUxmlPath = "Assets/Editor/TDTools/DesignerHelper/BaseDesignerHelper.uxml";
        private const string configPath = "Editor/TDTools/DesignerHelper/BaseDesignerConfig.xml";

        protected CustomizationDataConfig Config;
        protected Dictionary<string, CustomizationTableData> DefaultTableConfig = new Dictionary<string, CustomizationTableData>();
        protected bool isRuntime = false;

        protected VisualElement CommonBtnRoot;
        protected IMGUIContainer SceneContainer;
        protected VisualElement TableAreaRoot;
        protected PopupField<string> TablePathField;
        protected VisualElement MonsterAreaRoot;

        protected List<VisualElement> rtElement = new List<VisualElement>();
        protected List<VisualElement> cmElement = new List<VisualElement>();
        private void OnEnable()
        {
            GetConfigFromDisk();

            var vta = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(BaseDesignerHelperUxmlPath);
            vta.CloneTree(rootVisualElement);
            CommonBtnRoot = rootVisualElement.Q<VisualElement>("CommonBtnRoot");
            SceneContainer = rootVisualElement.Q<IMGUIContainer>("CustomSceneContainer");

            TableAreaRoot = rootVisualElement.Q<VisualElement>("TableAreaRoot");
            MonsterAreaRoot = rootVisualElement.Q<VisualElement>("MonsterAreaRoot");
            BindCommonArea();
            BindSceneArea();
            BindTableArea();
            BindSpecialArea();
        }

        private void OnFocus()
        {
            RefreshWhenFocus();
        }

        protected virtual void RefreshWhenFocus()
        {
            isRuntime = Application.isPlaying && XClientNetwork.singleton.XLoginStep == XLoginStep.Playing;
            rtElement.ForEach(it => it.style.display = isRuntime ? DisplayStyle.Flex : DisplayStyle.None);
            cmElement.ForEach(it => it.style.display = isRuntime ? DisplayStyle.None : DisplayStyle.Flex);
        }
        protected virtual void BindCommonArea()
        {
            #if USE_GM
            var entrance = rootVisualElement.Q<Button>("EntranceBtn");
            cmElement.Add(entrance);
            entrance.RegisterCallback<MouseUpEvent>(
                obj => EditorSceneManager.OpenScene("Assets/Scenes/entrance.unity"));
            var serverpage = rootVisualElement.Q<Button>("ServerPageBtn");
            serverpage.RegisterCallback<MouseUpEvent>(
                obj => Application.OpenURL("http://10.253.17.49/"));
            var hotfix = rootVisualElement.Q<Button>("HotfixBtn");
            hotfix.RegisterCallback<MouseUpEvent>(
                obj => HotUpdateWindow.ShowWindow());
            var testscene = rootVisualElement.Q<Button>("RtTestSceneBtn");
            rtElement.Add(testscene);
            testscene.RegisterCallback<MouseUpEvent>(
                obj => CFCommand.singleton.ProcessClientCommand("mm 50001"));
            var see = rootVisualElement.Q<Button>("RtSeeBtn");
            rtElement.Add(see);
            see.RegisterCallback<MouseUpEvent>(
                obj => CFCommand.singleton.ProcessClientCommand("see"));
            #endif
            
        }

        protected virtual void BindSceneArea()
        {

        }

        protected virtual void BindTableArea()
        {
            TablePathField = new PopupField<string>(Config.TablePathDesc, 0);
            TableAreaRoot.Q<VisualElement>("TableAreaTitle").Insert(1, TablePathField);
            rootVisualElement.Q<IMGUIContainer>("TableContainer").onGUIHandler = DrawTableList;
        }

        protected virtual void BindSpecialArea()
        {
            MonsterAreaRoot.style.display = DisplayStyle.None;
        }

        public void GetConfigFromDisk()
        {
            if(File.Exists($"{Application.dataPath}/{configPath}"))
            {
                Config = DataIO.DeserializeData<CustomizationDataConfig>($"{Application.dataPath}/{configPath}");
                SortSceneList();
            }
            else
            {
                Config = new CustomizationDataConfig();
            }
            if (Config.Table.Count == 0)
                InitTableList();
            InitDefaultTableConfig();
        }

        public void WriteConfigToDisk()
        {
            DataIO.SerializeData($"{Application.dataPath}/{configPath}", Config);
        }

        protected void SortSceneList()
        {
            Config.Scene.Sort((a, b) => a.index - b.index);
        }

        protected void SortTableList()
        {
            Config.Table.Sort((a, b) => a.index - b.index);
        }
        protected void InitTableList()
        {
            Config.TablePath = OpenWpsHelper.Config.PathDic.Values.ToList();
            Config.TablePathDesc = OpenWpsHelper.Config.PathDic.Keys.ToList();

            OpenWpsHelper.Config.TableConfigList.ForEach(it =>
            {
                var param = it.Split('=');
                if (param.Length >= 2)
                    Config.Table.Add(new CustomizationTableData()
                    {
                        index = Config.Table.Count,
                        TableName = param[0],
                        TableFreezeRange = param[1]
                    });
            });
            WriteConfigToDisk();
        }

        void InitDefaultTableConfig()
        {
            OpenWpsHelper.Config.TableConfigList.ForEach(it =>
            {
                var param = it.Split('=');
                if (param.Length >= 2)
                {
                    DefaultTableConfig[param[0]] = new CustomizationTableData()
                    {
                        index = Config.Table.Count,
                        TableName = param[0],
                        TableFreezeRange = param[1]
                    };
                }
            });
        }

        void DrawTableList()
        {
            for (int i = 0; i < Config.Table.Count; ++i)
            {
                var item = Config.Table[i];
                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button(item.TableName, GUILayout.Width(320f)))
                {
                    OpenWpsHelper.OpenWps($"{Config.TablePath[TablePathField.index]}", item.TableName, item.TableFreezeRange);
                }
                GUILayout.Space(20f);
                if (GUILayout.Button("x", GUILayout.Width(30f)))
                {
                    Config.Table.RemoveAt(i);
                    WriteConfigToDisk();
                }
                GUILayout.Space(10f);
                if (i > 0)
                {
                    if (GUILayout.Button("↑", GUILayout.Width(30f)))
                    {
                        item.index = i - 1;
                        Config.Table[i - 1].index = i;
                        SortTableList();
                        WriteConfigToDisk();
                    }
                }
                else
                {
                    GUILayout.Space(33f);
                }
                GUILayout.Space(10f);
                if (i < Config.Table.Count - 1)
                {
                    if (GUILayout.Button("↓", GUILayout.Width(30f)))
                    {
                        item.index = i + 1;
                        Config.Table[i + 1].index = i;
                        SortTableList();
                        WriteConfigToDisk();
                    }
                }
                EditorGUILayout.EndHorizontal();
            }
        }
    }
}
