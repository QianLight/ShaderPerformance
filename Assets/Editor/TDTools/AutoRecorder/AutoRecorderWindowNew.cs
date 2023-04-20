using System;
using System.Text;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;
using UnityEditor.Recorder;
using Cinemachine;
using VirtualSkill;
using CFEngine;
using CFUtilPoolLib;

namespace TDTools
{
    public class AutoRecorderWindowNew : EditorWindow
    {
        private static readonly string[] SpecialMask = new string[]
        {
            "NeedHit",
            "AIMode",
            //"AllDirection"
        };

        public static AutoRecorderWindowNew Instance;
        private static AutoRecorderMgr mgr;
        private GUIStyle style;
        private bool needSelect;

        private Vector2 skillListScrollViewPos;
        [MenuItem("Tools/TDTools/监修相关工具/AutoRecorderNew &%Y")]
        public static void ShowWindow()
        {
            if (!AutoRecorderMgr.GetMgr.PreOpen())
                return;
            mgr = AutoRecorderMgr.GetMgr;
            if (!Instance) Instance = GetWindow<AutoRecorderWindowNew>("技能半自动录制工具新");
            Instance.Focus();
        }

        private void OnEnable()
        {
            style = new GUIStyle() { fontSize = 12, normal = new GUIStyleState() { textColor = Color.white } };

            mgr.InitEnv();
        }

        private void OnGUI()
        {
            if (!mgr.IsRecording)
            {
                if(mgr.IsEditor)
                {
                    mgr.IsBehit = EditorGUILayout.Toggle("受击脚本录制", mgr.IsBehit);
                    EditorGUILayout.Space(20);
                }
                if (!mgr.IsBehit)
                {
                    DrawCommonArea();
                    mgr.ARCD.CameraType = EditorGUITool.Popup("相机规格", mgr.ARCD.CameraType, AutoRecorderCameraData.CameraTypeStr);

                    if (mgr.ARCD.CameraType == AutoRecorderCameraData.CameraTypeStr.Length - 1)
                    {
                        EditorGUILayout.LabelField("相机距离");
                        mgr.ARCD.CustomHeight = EditorGUILayout.Slider("高度", mgr.ARCD.CustomHeight, 1f, 12f);
                        mgr.ARCD.CustomRadius = EditorGUILayout.Slider("半径", mgr.ARCD.CustomRadius, 2f, 40f);
                        mgr.ARCD.CustomOffset = EditorGUILayout.Slider("视角", mgr.ARCD.CustomOffset, -5f, 5f);
                        mgr.ARCD.SetCustomParam();
                        EditorGUILayout.Space(10);
                    }
                    //cameraDirection = EditorGUITool.Popup("相机位置", cameraDirection, CameraDirectionStr);
                    EditorGUILayout.BeginHorizontal();
                    if (GUILayout.Button("预览相机配置（第一次需要点两下）"))
                    {
                        mgr.SetCameraParam();
                    }
                    if (GUILayout.Button("重置相机配置（如果预览相机配置没反应，可以先点这个）"))
                    {
                        mgr.ResetCameraParam();
                    }
                    EditorGUILayout.EndHorizontal();
                    //needResetCamera = EditorGUILayout.ToggleLeft("需要在每个脚本录制前，重置相机参数为上方配置", needResetCamera);
                    if (mgr.IsEditor)
                    {
                        mgr.ERD.PupetPos = EditorGUILayout.Vector3Field("受击目标相对位置", mgr.ERD.PupetPos);
                    }
                    needSelect = EditorGUILayout.ToggleLeft("选择文件模式", needSelect);
                    if (needSelect)
                    {
                        EditorGUILayout.BeginHorizontal();
                        if (GUILayout.Button("全选"))
                        {
                            mgr.SetSelectAll(true);
                        }
                        if (GUILayout.Button("全不选"))
                        {
                            mgr.SetSelectAll(false);
                        }
                        if (GUILayout.Button("粘贴"))
                        {
                            mgr.PasteData(out List<string> error);
                            if(error.Count > 0)
                            {
                                string temp = string.Join("\n", error);
                                ShowNotification(new GUIContent(temp + "不存在!"), 5);
                                Debug.Log(temp + "不存在!");
                            }
                        }
                        EditorGUILayout.EndHorizontal();
                        skillListScrollViewPos = EditorGUILayout.BeginScrollView(skillListScrollViewPos, false, true);

                        for (int i = 0; i < mgr.FileList.Count; ++i)
                        {
                            EditorGUILayout.BeginHorizontal();
                            var item = mgr.FileList[i] as FileData;
                            item.Select = EditorGUILayout.ToggleLeft(new GUIContent(item.Name), item.Select, new GUILayoutOption[] { GUILayout.Width(400f) });
                            item.SpecialInt = EditorGUILayout.MaskField(item.SpecialInt, SpecialMask, new GUILayoutOption[] { GUILayout.Width(150f) });
                            item.PupetSkillName = EditorGUILayout.TextField("召唤ID|技能名(多个用,隔开)", item.PupetSkillName);
                            EditorGUILayout.EndHorizontal();
                        }
                        EditorGUILayout.EndScrollView();
                    }
                }
                else
                {
                    
                }
            }
            else
            {
                
            }
        }

        private void DrawCommonArea()
        {
            EditorGUILayout.BeginHorizontal(style);
            EditorGUILayout.LabelField("录制目录：", mgr.SkillLocation, style);
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.LabelField("录制角色名：", mgr.CurPreName, style);
            EditorGUILayout.Space(20);
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("输出目录：", mgr.OutputPath, style);
            if (GUILayout.Button("...", new GUILayoutOption[] { GUILayout.Width(50) }))
            {
                mgr.OutputPath = EditorUtility.OpenFolderPanel("选择要输出的目录", mgr.OutputPath, "");
            }
            EditorGUILayout.EndHorizontal();
            if (GUILayout.Button("配置检查"))
            {

            }
            if (GUILayout.Button("开始录制"))
            {
                mgr.StartRecord();
            }
            EditorGUILayout.Space(20);
        }
    }
}
