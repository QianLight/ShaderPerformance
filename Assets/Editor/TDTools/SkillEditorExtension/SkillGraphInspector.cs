using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using static TDTools.SkillGraphUtils;

namespace TDTools
{
    public class SkillGraphInspector : EditorWindow
    {
		[NonSerialized] bool m_Initialized;
		[SerializeField] TreeViewState m_TreeViewState;
		[SerializeField] MultiColumnHeaderState m_MultiColumnHeaderState;
		SkillGraphShow m_TreeView;
		//SkillGraphTitleAsset m_SkillGraphTitleAsset;
		List<string> m_SelectScripts;
		string m_SelectPath = "";
		private string m_ExportPath = "";
		private string m_SaveName = "";
		private string m_SavePath = "";
		int m_CheckType = 0;
		string[] m_TypeArray;
		bool m_ShowTreeView = false;

		//these params are specially for filter display
		private SkillGraphShowDataTitle m_Title;
		private FilterData m_FilterData;

		Rect m_SkillGraphTreeViewRect
		{
			get { return new Rect(20, 30, position.width - 40, position.height - 100
			); }
		}
		
		//rect for bottom button
		Rect bottomBarRect
		{
			get { return new Rect(20f, position.height - 60f, position.width - 40f, 40f);  }
		}

		[MenuItem("Tools/TDTools/关卡相关工具/SkillGraphInspector")]
		public static SkillGraphInspector GetWindow()
		{
			var window = GetWindow<SkillGraphInspector>();
			window.titleContent = new GUIContent("SkillGraphInspector");
			window.Focus();
			window.Repaint();
			return window;
		}
		void OnGUI()
		{
			InitIfNeeded();
			if(m_ShowTreeView)
            {
				if (GUILayout.Button("返回选择"))
				{
					m_ShowTreeView = false;
				}
				DoTreeView(m_SkillGraphTreeViewRect);
				BottomToolBar(bottomBarRect);
            }
			else
            {
				EditorGUILayout.BeginHorizontal();
				m_SelectPath = EditorPrefs.GetString("SKILL_INSPECTOR_PATH", $"{Application.dataPath}/BundleRes/SkillPackage/Role_hancock");
				EditorGUILayout.LabelField("目录：", m_SelectPath);
				if (GUILayout.Button("...", new GUILayoutOption[] { GUILayout.Width(50) }))
				{
					m_SelectPath = EditorUtility.OpenFolderPanel("选择要输出的目录", "/BundleRes/SkillPackage", "");
					EditorPrefs.SetString("SKILL_INSPECTOR_PATH", m_SelectPath);
				}
				EditorGUILayout.EndHorizontal();
				EditorGUILayout.BeginHorizontal();
				m_CheckType = EditorGUITool.Popup("查看类型", m_CheckType, m_TypeArray);
				if (m_CheckType != EditorPrefs.GetInt("CHECK_TYPE"))
				{
					m_Title = SkillGraphDataManager.GetTitleDataByType(m_TypeArray[m_CheckType]);
					InitFilter(m_Title, ref m_FilterData);
					EditorPrefs.SetInt("CHECK_TYPE", m_CheckType);
				}
				EditorGUILayout.EndHorizontal();
				
				//start to draw filter requirements based on title
				DrawFilter(m_Title, ref m_FilterData);

				if(GUILayout.Button("确认检索"))
                {
					DoCheck();
				}
			}
		}
		
		void InitFilter(SkillGraphShowDataTitle title, ref FilterData data)
		{
			data = new FilterData();
			data.Logic = true;
			//I dont know why, but bool params need a false as start
			data.BoolFilter.Add(false);
			//Our selection starts from index 1, This false is meant to match with "Name"
			data.Selected.Add(false);
			
			for (int i = 1; i < title.TitleItems.Length; i++)
			{
				var item = title.TitleItems[i];
				data.Selected.Add(false);
				switch (item.ParamType)
				{
					case "int":
						data.IntMax.Add(Int32.MaxValue);
						data.IntMin.Add(Int32.MinValue);
						break;
					case "float":
						data.FloatMax.Add(Single.MaxValue);
						data.FloatMin.Add(Single.MinValue);
						break;
					case "bool":
						data.BoolFilter.Add(true);
						break;
				}
			}
		}
    
	    void DrawFilter(SkillGraphShowDataTitle title, ref FilterData data)
	    {
		    EditorGUILayout.BeginHorizontal();
		    data.Logic = EditorGUILayout.Toggle("满足下列所有条件", data.Logic);
		    data.Logic = !EditorGUILayout.Toggle("满足下列一种条件", !data.Logic);
		    EditorGUILayout.EndHorizontal();
		    
		    //select restrictions for data
		    EditorGUILayout.BeginHorizontal();
		    EditorGUILayout.LabelField("选择需要添加过滤条件的参数",GUILayout.Width(200));
		    if (GUILayout.Button("+", new GUILayoutOption[] {GUILayout.Width(50)}))
		    {
			    Event current = Event.current;
			    Rect rect_show = new Rect(current.mousePosition.x, current.mousePosition.y, 0, 0);
			    PopupWindow.Show(rect_show, new PopupSelect(data, title, this));
		    }
		    EditorGUILayout.EndHorizontal();

		    for (int i = 1; i < title.TitleItems.Length; i++)
			{
				if (!data.Selected[i])
					continue;
				var item = title.TitleItems[i];
				int index = title.TitleItems[i].ListIndex;
				
				if (item.ParamType == "string")
					continue;
				EditorGUILayout.BeginHorizontal();
				//data.Selected[i] = EditorGUILayout.Toggle("", data.Selected[i]);
				
				EditorGUILayout.LabelField($"{item.Title}:", GUILayout.Width(200));
				switch (item.ParamType)
				{
					case "int":
						data.IntMin[index] = EditorGUILayout.IntField(data.IntMin[index],GUILayout.Width(180));
						EditorGUILayout.LabelField($"<={item.Title}<=",GUILayout.Width(180));
						data.IntMax[index] = EditorGUILayout.IntField(data.IntMax[index],GUILayout.Width(180));
						break;
					case "float":
						data.FloatMin[index] = EditorGUILayout.FloatField(data.FloatMin[index],GUILayout.Width(180));
						EditorGUILayout.LabelField($"<={item.Title}<=",GUILayout.Width(180));
						data.FloatMax[index] = EditorGUILayout.FloatField(data.FloatMax[index],GUILayout.Width(180));                       break;
					case "bool": 
						EditorGUILayout.LabelField($"{item.Title}=",GUILayout.Width(180)); 
						data.BoolFilter[index] = EditorGUILayout.Toggle("True", data.BoolFilter[index],GUILayout.Width(180));
						data.BoolFilter[index] = !EditorGUILayout.Toggle("False", !data.BoolFilter[index],GUILayout.Width(180));
						break;
				}
				EditorGUILayout.EndHorizontal();
			}
	    }

		void DoCheck()
        {
			if(m_SelectPath == "")
            {
				Debug.Log("没有选择要检索的路径");
				return;
            }
			SetTreeView();
			m_ShowTreeView = true;
		}
		void InitIfNeeded()
		{
			if (!m_Initialized)
			{
				if (m_SelectScripts == null)
					m_SelectScripts = new List<string>();
				m_SelectScripts.Clear();
				if (m_TreeViewState == null)
					m_TreeViewState = new TreeViewState();
				SkillGraphDataManager.InitTitleFile();
				m_TypeArray = SkillGraphDataManager.GetTypeArray();
				m_Initialized = true;
				EditorPrefs.SetInt("CHECK_TYPE", -1);
			} 
		}

		void SetTreeView()
		{
			var treeModel = new TreeModel<SkillGraphShowData>(GetData(m_Title.DataFunc));
			m_TreeView = new SkillGraphShow(m_TreeViewState, m_SkillGraphTreeViewRect.width, m_Title, treeModel, m_FilterData, this);
		}

		public IList<SkillGraphShowData> GetData(string name)
		{
			var list = Directory.GetFiles(m_SelectPath, "*.bytes", SearchOption.AllDirectories).ToList();   
			return SkillGraphDataManager.GetDataByFuncName(list, name);
		}

		void DoTreeView(Rect rect)
		{
			m_TreeView.OnGUI(rect);
		}
		
		//bottom tool bar for export and other functions
		void BottomToolBar(Rect rect)
		{
			GUILayout.BeginArea(rect);
			
			//select export path
			EditorGUILayout.BeginHorizontal();
			m_ExportPath = EditorPrefs.GetString("SKILL_EXPORT_PATH", $"{Application.dataPath}/BundleRes/SkillPackage/Save");
			m_SaveName = EditorPrefs.GetString("EXPORT_FILE_NAME", "Save");
			m_SavePath = EditorPrefs.GetString("EXPORT_FILE_PATH", $"{m_ExportPath}/{m_SaveName}.txt");
			EditorGUILayout.LabelField("输出目录：", m_ExportPath);
			if (GUILayout.Button("...", new GUILayoutOption[] { GUILayout.Width(50) }))
			{
				m_SavePath = EditorUtility.SaveFilePanel("选择输出文件", m_ExportPath,m_SaveName, "txt");
				EditorPrefs.SetString("SKILL_EXPORT_PATH", m_ExportPath);
				EditorPrefs.SetString("EXPORT_FILE_NAME", m_SaveName);
				EditorPrefs.SetString("EXPORT_FILE_PATH", m_SavePath);
			}
			EditorGUILayout.EndHorizontal();
			
			EditorGUILayout.BeginHorizontal();
			//confirm to export
			if (GUILayout.Button("Export All"))
			{ 
				m_TreeView.SaveTreeView(m_SavePath);
			}
			EditorGUILayout.EndHorizontal();
			GUILayout.EndArea();
		}
    }
}