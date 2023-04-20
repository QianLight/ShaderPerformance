#if UNITY_EDITOR

using UnityEditor;
using UnityEngine;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using System.IO;
using System.Xml.Serialization;
using CFUtilPoolLib;
using System;
using UnityEngine.CFUI;
using CFEngine;
namespace XEditor
{
    public enum CreatType
    {
        Player,
        CommonPlayer,
        CommonBoss,
        CommonMonster,
        Stone,
        OtherCommon,
        
    }
    public class XReactCreatWindow
    {
        public GUIContent CreateContent = new GUIContent("创建", "创建Rect配置");
        public GUIContent LoadContent = new GUIContent("加载", "加载Rect配置");

        Rect FuncBtnRect0 = new Rect(10f, 10f, 100f, 30f);

        /* Creat */
        private string create_name = "default_react";
        private int dummy_id = 0;
        private string create_directory = "/";
        private string directory_suffix = "/";

        private AnimationClip create_clip = null;



        public XReactEntranceWindow Window;

        CreatType _CreatType = CreatType.CommonBoss;
        public void Init(XReactEntranceWindow parent)
        {
            Window = parent;
        }
   

        Vector2 posLeft;
        Vector2 posRight;
        public void OnGUI()
        {
            GUILayout.BeginHorizontal();
            {
                // Left view
                posLeft = GUILayout.BeginScrollView(posLeft,
                    GUILayout.Width(Window.position.width * 0.5f),
                    GUILayout.ExpandWidth(false));

                OnLeftGUI();

                GUILayout.EndScrollView();

                GUILayout.Box("", GUILayout.Width(2),
                GUILayout.ExpandHeight(true));

                // Right view
                posRight = GUILayout.BeginScrollView(posRight,
                    GUILayout.Width(Window.position.width * 0.5f),
                    GUILayout.ExpandWidth(true));

                OnRightGUI();

                GUILayout.EndScrollView();
            }
            GUILayout.EndHorizontal();
        }

        public void OnLeftGUI()
        {

            GUILayout.BeginVertical();
            _CreatType = (CreatType)EditorGUILayout.EnumPopup("创建模板类型", _CreatType, GUILayout.Width(300));

            RefreshLeftInfo();
            GUILayout.Space(30);
            if (GUILayout.Button( CreateContent, GUILayout.Height(50), GUILayout.Width(100)))
            {
                CreateReact();
            }

            GUILayout.EndVertical();
        }

        float secondConteolX = 170f;
        float indentWidth = 20;
        float verticalSpacing = 30;
        float RefreshLeftInfo()
        {
            /////////// 创建配置 /////////////
            float y = FuncBtnRect0.y + FuncBtnRect0.height + 10f;
            EditorGUILayout.Space(20);
            if(_CreatType == CreatType.CommonBoss || _CreatType == CreatType.CommonMonster ||
                _CreatType == CreatType.Stone || _CreatType == CreatType.CommonPlayer)
            {
                EditorGUILayout.BeginHorizontal();
                GUILayout.Space(indentWidth);
                GUILayout.Label("*React Name:", GUILayout.Width(100));
                GUILayout.Box(_CreatType.ToString(), GUILayout.Width(100));
                create_name = _CreatType.ToString();
                EditorGUILayout.EndHorizontal();

                GUILayout.Space(verticalSpacing);
                EditorGUILayout.BeginHorizontal();
                GUILayout.Space(indentWidth);
                GUILayout.Label("*Directory:", GUILayout.Width(100));
                GUILayout.Box("Common", GUILayout.Width(100));
                create_directory = "Common";
                EditorGUILayout.EndHorizontal();

                GUILayout.Space(verticalSpacing);
                EditorGUILayout.BeginHorizontal();
                GUILayout.Space(indentWidth);
                GUILayout.Label("*Animation:", GUILayout.Width(100));
                create_clip = EditorGUILayout.ObjectField(create_clip, typeof(AnimationClip), true) as AnimationClip;
                EditorGUILayout.EndHorizontal();
            }
            else if(_CreatType == CreatType.Player)
            {
                XEntityPresentation.RowData presentData = dummy_id != 0 ? XAnimationLibrary.AssociatedAnimations((uint)dummy_id) : null;
                bool isLegal = presentData != null;
                create_name = EditorGUI.TextField(new Rect(20f, y, 370f, 20f), "*React Name:", create_name);
                GUILayout.Space(40f);

                y += 40f;
                GUI.Label(new Rect(indentWidth, y, 200f, 30f), "*Directory:", ReactCommon.BoldLabelstyle);
                var rr2 = new Rect(secondConteolX, y, 220f, 20f);
                ReactUtils.DrawBox(rr2, isLegal ? ReactUtils.GetColor("#BBFFBB") : ReactCommon.BoxColor, null, "目录以 / 结尾, OP 项目保持与SkillLocation一致");

                create_directory = isLegal ? presentData.BehitLocation : directory_suffix;
                GUI.Label(rr2, create_directory, ReactCommon.BoldLabelStyle_Black);
                //create_directory = GUI.TextField(rr2, create_directory, BoldLabelStyle_Black);

                GUILayout.Space(40f);
                y += 40f;
                GUI.Label(new Rect(indentWidth, y, 200f, 30f), "*Dummy ID:", ReactCommon.BoldLabelstyle);
                var rr3 = new Rect(secondConteolX, y, 220f, 20f);


                ReactUtils.DrawBox(rr3, isLegal ? Color.green : ReactCommon.BoxColor, null, " Present ID ");
                dummy_id = EditorGUI.IntField(rr3, dummy_id);

                y += 40f;
                GUILayout.Space(40f);
                GUI.Label(new Rect(indentWidth, y, 200f, 30f), "*Animation:", ReactCommon.BoldLabelstyle);
                var rr4 = new Rect(secondConteolX, y, 220f, 20f);
                create_clip = EditorGUI.ObjectField(rr4, create_clip, typeof(AnimationClip), true) as AnimationClip;

                GUILayout.Space(40f);
            }
            else if(_CreatType == CreatType.OtherCommon)
            {

                create_name = EditorGUI.TextField(new Rect(20f, y, 370f, 20f), "*React Name:", create_name);
                GUILayout.Space(40f);

                y += 40f;


                create_directory = EditorGUI.TextField(new Rect(20f, y, 370f, 20f), "*Directory:", create_directory);

                GUILayout.Space(40f);
                y += 40f;
                GUILayout.Space(40f);
                GUI.Label(new Rect(indentWidth, y, 200f, 30f), "*Animation:", ReactCommon.BoldLabelstyle);
                var rr4 = new Rect(secondConteolX, y, 220f, 20f);
                create_clip = EditorGUI.ObjectField(rr4, create_clip, typeof(AnimationClip), true) as AnimationClip;

                GUILayout.Space(40f);
            }

            return y;
        }

        void CreateReact()
        {
            switch (_CreatType)
            {
                case CreatType.CommonBoss:
                    dummy_id = 0;
                    break;
                case CreatType.CommonMonster:
                    dummy_id = 0;
                    break;
                case CreatType.Stone:
                    dummy_id = 0;
                    break;
                case CreatType.CommonPlayer:
                    dummy_id = 0;
                    break;
                case CreatType.Player:
                    //dummy_id = 0;
                    break;
                case CreatType.OtherCommon:
                    dummy_id = 0;
                    break;
            }

            string pathWithName = CreateReact(dummy_id, create_name, create_directory, create_clip);
            if (!string.IsNullOrEmpty(pathWithName))
            {
                if (LoadReact(pathWithName, out Window.ReactDataSet))
                {
                    Window.CurState = XReactEntranceWindow.ProcessState.Editor;
                    Window.minSize = new Vector2(1200f, 600f);
                    Window.position = new Rect(Window.position.x, Window.position.y, 1200f, 600f);
                }
            }
        }


        string searchStr = "";
        public void OnRightGUI()
        {
            GUILayout.Space(50);

            GUILayout.BeginHorizontal();
            {
                searchStr = EditorGUILayout.TextField("Search:", searchStr, "SearchTextField");
                if (GUILayout.Button("Find", GUILayout.Width(40)))
                {
                    XReactBatch.SearchText(searchStr);
                }
                GUILayout.Space(10);
            }
            GUILayout.EndHorizontal();
        }


        public static string CreateReact(int id, string name, string directory, AnimationClip clip)
        {
            string skp = XEditorPath.GetPath("ReactPackage" + "/" + directory) + name + ".bytes";
            string config = XEditorPath.GetEditorBasedPath("ReactPackage" + "/" + directory) + name + ".config";

            XReactConfigData conf = new XReactConfigData();

            if (clip != null)
            {
                conf.ReactClip = AssetDatabase.GetAssetPath(clip);
                conf.ReactClipName = clip.name;
            }

            conf.Player = id;
            conf.Directory = directory;
            conf.ReactName = name;

            XReactData data = new XReactData();
            data.Name = name;
            data.PresentID = id;

            if (clip != null)
            {
                data.ClipName = conf.ReactClip.Remove(conf.ReactClip.LastIndexOf('.'));
                data.ClipName = data.ClipName.Remove(0, 17);
            }

            XDataIO<XReactData>.singleton.SerializeData(skp, data);

            using (FileStream writer = new FileStream(config, FileMode.Create))
            {
                XmlSerializer formatter = new XmlSerializer(typeof(XReactConfigData));
                formatter.Serialize(writer, conf);
            }

            AssetDatabase.Refresh();
            return skp;
        }


        public static bool LoadReact(string pathwithname, out XReactDataSet DataSet)
        {
            return XReactDataHostBuilder.singleton.Load(pathwithname, out DataSet);
        }

    }
}

#endif
