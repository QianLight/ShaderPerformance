using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Cinemachine.Utility;
using UnityEngine;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using static TDTools.SkillGraphUtils;

namespace TDTools
{
	public enum SkillGraphShowDataType
	{
		
	}

	public class SkillGraphShowData : TreeElement
    {
		public int MaxLine = 1;
		public int ListCol = 1;
        public List<string> StringParam = new List<string>();
        public List<int> IntParam = new List<int>(); 
        public List<float> FloatParam = new List<float>(); // 0-SkillTime
		public List<bool> BoolParam = new List<bool>();
		public List<object> ObjectParam = new List<object>();
		
		//Params for display
		public List<string> DisplayParam = new List<string>();

		public SkillGraphShowData(string name, int depth, int id) : base(name, depth, id)
		{
			StringParam.Clear();
			IntParam.Clear();
			FloatParam.Clear();
			BoolParam.Clear();
			ObjectParam.Clear();
			DisplayParam.Clear();
			StringParam.Add(name);
			BoolParam.Add(false);
		}

		public SkillGraphShowData() : base()
		{
			StringParam.Clear();
			IntParam.Clear();
			FloatParam.Clear();
			BoolParam.Clear();
			ObjectParam.Clear();
			DisplayParam.Clear();
			StringParam.Add(name);
			BoolParam.Add(false);
		}

		public void SetMaxLine(int i)
        {
			MaxLine = Mathf.Max(MaxLine, i);
        }
    }

	//public class SkillGraphTitleAsset : ScriptableObject
 //   {
	//	public SkillGraphShowDataTitle[] Titles;
	//	[NonSerialized]
	//	public Dictionary<string, SkillGraphShowDataTitle> TitleDic;
	//	public void ReBuild()
 //       {
	//		if (TitleDic == null)
	//			TitleDic = new Dictionary<string, SkillGraphShowDataTitle>();
	//		TitleDic.Clear();
	//		foreach(var item in Titles)
 //           {
	//			TitleDic[item.TypeName] = item;
 //           }
 //       }
	//}
	[System.Serializable]
	public class SkillGraphShowDataTitle
    {
		public string TypeName;
		public string DataFunc;
		public string isScript;
        public SkillGraphShowDataItem[] TitleItems;
		[NonSerialized]
		public SkillGraphShowDataItem ListItem;
		public SkillGraphShowDataItem[] TitleItemsWithoutList;
	}

	public enum SkillGraphShowTitleType
    {
		Name,
		Label,
		Area,
		List,
    }
	[System.Serializable]
	public class SkillGraphShowDataItem
    {
        public string Title;
        public string Desc;
        public string ParamType;
        public int ListIndex;
		public bool AllowHide;
		public float DefaultWidth;
		public float MinWidth;
		public string ShowType;

		public SkillGraphShowDataItem()
        {

        }
    }

	public class SkillGraphShow
	{
		private SkillGraphTreeView skillGraphTreeView;
		private SkillGraphShowDataTitle dataTitle;
		private FilterData filterData;
		public int extraCol = 0;
		
		//model backup for data change
		private SkillGraphInspector inspector;
		private TreeModel<SkillGraphShowData> model;
		private TreeViewState state;
		private float width;
		
		public SkillGraphShow(TreeViewState state, float width,
			SkillGraphShowDataTitle title, TreeModel<SkillGraphShowData> model, 
			FilterData filter, SkillGraphInspector inspector)
		{
			//save filterdata for further 
			this.inspector = inspector;
			this.state = state;
			this.width = width;
			dataTitle = title;
			filterData = filter;
			InitExtraCol(model);
			dataTitle.TitleItemsWithoutList = dataTitle.TitleItems.Where(item => item.ShowType != "List").ToArray();

			//initial display params
			InitDisplayParams(model, filterData.Logic);
			skillGraphTreeView = new SkillGraphTreeView(state, width, title, model, filter);
		}

		public void Repaint()
		{
			model = new TreeModel<SkillGraphShowData>(inspector.GetData(dataTitle.DataFunc));
			InitDisplayParams(model, filterData.Logic);
			skillGraphTreeView = new SkillGraphTreeView(state, width, dataTitle, model, filterData);
		}

		public void OnGUI(Rect rect)
		{
			skillGraphTreeView.OnGUI(rect);
			Rect rect1 = new Rect(rect.x, rect.y, rect.width, EditorGUIUtility.singleLineHeight);
			Rect rect2 = new Rect(rect.x, rect1.yMax, rect.width, rect.height - rect1.height);

			Event current = Event.current;
			Rect rect_show = new Rect(current.mousePosition.x, current.mousePosition.y, 0, 0);
			if (current.type == UnityEngine.EventType.MouseDown && current.button == 1 && rect2.Contains(current.mousePosition))
			{
				PopupWindow.Show(rect_show, new PopupFilter(filterData, dataTitle, this));
			}
		}
		
		public void InitExtraCol(TreeModel<SkillGraphShowData> model)
		{
			int listNum = 0;
			foreach (var item in dataTitle.TitleItems)
			{
				if (item.ShowType == "List")
				{
					listNum++;
					if (listNum >= 2)
					{
						Debug.Log("Has 2 or more List Title Item");
						return;
					}

					dataTitle.ListItem = item;
					try
					{
						var list = model.DataList;
						foreach (var data in list)
						{
							if (data.ObjectParam.Count > 0)
							{
								extraCol = Mathf.Max(extraCol,
									(data.ObjectParam[item.ListIndex] as List<string>).Count);
							}
						}
					}
					catch
					{
						extraCol = 0;
					}
				}
			}
		}

		
		//Initial Display Params
		public void InitDisplayParams(TreeModel<SkillGraphShowData> model, bool logic)
		{
			//count for selected restrictions
			int resNumber = 0;
			foreach (var selected in filterData.Selected)
			{
				if (selected)
				{
					resNumber += 1;
				}
			}

			foreach(var data in model.DataList)
			{
				data.DisplayParam.Clear();
				//judge will be used to judge whether the data satisfy our request
				int judge = 0;
				for (int i = 0; i < dataTitle.TitleItems.Length; i++)
				{
					var item = dataTitle.TitleItems[i];
					string value = "";

					switch (item.ShowType)
					{
						case "Name":
						{
							value = data.name;
						}
							break;

						case "Label":
						{
							int index = item.ListIndex;

							switch (item.ParamType)
							{
								case "string":
									ReadFromList(data.StringParam, index, ref value);
									break;
								case "int":
									if (filterData.Selected[i])
										FihlData(data.IntParam, filterData.IntMax, filterData.IntMin, index, ref judge);
									ReadFromList(data.IntParam, index, ref value);
									break;
								case "bool":
									if (filterData.Selected[i])
										FihlData(data.BoolParam, filterData.BoolFilter, filterData.BoolFilter, index,
											ref judge);
									ReadFromList(data.BoolParam, index, ref value);
									break;
								case "float":
									if (filterData.Selected[i])
										FihlData_float(data.FloatParam, filterData.FloatMax, filterData.FloatMin, index,
											ref judge);
									ReadFromList(data.FloatParam, index, ref value);
									break;
							}
						}
							break;
						case "Area":
						{
							int index = item.ListIndex;
							switch (item.ParamType)
							{
								case "string":
									value = data.StringParam[index];
									break;
								case "int":
									value = data.IntParam[index].ToString();
									break;
								case "bool":
									value = data.BoolParam[index].ToString();
									break;
								case "float":
									value = data.FloatParam[index].ToString();
									break;
							}
						}
							break;
					}

					data.DisplayParam.Add(value);
				}

				if (!KeepData(judge, resNumber, logic))
				{
					data.DisplayParam.Clear();
					model.RemoveElements(new List<SkillGraphShowData> {data});
				}
			}
		}
		public void SaveTreeView(string savePath)
		{
			skillGraphTreeView.SaveTreeView(savePath);
		}
	}

	internal class SkillGraphTreeView : TreeViewWithTreeModel<SkillGraphShowData>
	{
		public int extraCol = 0;
		const float kRowHeights = 20f;
		const float kToggleWidth = 18f;
		public bool showControls = true;
		private SkillGraphShowDataTitle dataTitle;
		
		public SkillGraphTreeView(TreeViewState state, float width,
			SkillGraphShowDataTitle title, TreeModel<SkillGraphShowData> model, 
			FilterData filter) : base(state, model)
		{
			dataTitle = title;
			var headerState = CreateDefaultMultiColumnHeaderState(width);
			multiColumnHeader = new MultiColumnHeader(headerState);
			//multiColumnHeader.ResizeToFit();
			multiColumnHeader.sortingChanged += OnSortingChanged;

			rowHeight = kRowHeights;
			columnIndexForTreeFoldouts = 0;
			showAlternatingRowBackgrounds = true;
			showBorder = true;
			customFoldoutYOffset = (kRowHeights - EditorGUIUtility.singleLineHeight) * 0.5f;
			extraSpaceBeforeIconAndLabel = kToggleWidth;
			Reload();
		}

		public void SetDataTitle(SkillGraphShowDataTitle title)
		{
			dataTitle = title;
		}
		
		
		protected override float GetCustomRowHeight(int row, TreeViewItem item)
        {
			var data = (item as TreeViewItem<SkillGraphShowData>).data;
			if(data.MaxLine > 1)
            {
				return (kRowHeights + (data.MaxLine - 1) * EditorGUIUtility.singleLineHeight);
            }
			return kRowHeights;
        }

		public static void TreeToList(TreeViewItem root, IList<TreeViewItem> result)
		{
			if (root == null)
				throw new NullReferenceException("root");
			if (result == null)
				throw new NullReferenceException("result");

			result.Clear();

			if (root.children == null)
				return;

			Stack<TreeViewItem> stack = new Stack<TreeViewItem>();
			for (int i = root.children.Count - 1; i >= 0; i--)
				stack.Push(root.children[i]);

			while (stack.Count > 0)
			{
				TreeViewItem current = stack.Pop();
				result.Add(current);

				if (current.hasChildren && current.children[0] != null)
				{
					for (int i = current.children.Count - 1; i >= 0; i--)
					{
						stack.Push(current.children[i]);
					}
				}
			}
		}

		protected override IList<TreeViewItem> BuildRows(TreeViewItem root)
		{
			var rows = base.BuildRows(root);
			SortIfNeeded(root, rows);
			return rows;
		}

		void OnSortingChanged(MultiColumnHeader multiColumnHeader)
		{
			SortIfNeeded(rootItem, GetRows());
		}

		void SortIfNeeded(TreeViewItem root, IList<TreeViewItem> rows)
		{
			if (rows.Count <= 1)
				return;
			
			if (multiColumnHeader.sortedColumnIndex == -1)
			{
				return;
			}

			SortByMultipleColumns();
			TreeToList(root, rows);
			Repaint();
		}

		void SortByMultipleColumns()
		{
			var sortedColumns = multiColumnHeader.state.sortedColumns;

			if (sortedColumns.Length == 0)
				return;

			var myTypes = rootItem.children.Cast<TreeViewItem<SkillGraphShowData>>();
			var orderedQuery =  InitialOrder(myTypes, sortedColumns);

			for (int i = 1; i < sortedColumns.Length; i++)
			{
				int index = sortedColumns[i];
				var item = index >= dataTitle.TitleItemsWithoutList.Length ? dataTitle.ListItem : dataTitle.TitleItemsWithoutList[index];
				//SortOption sortOption = m_SortOptions[sortedColumns[i]];
				bool ascending = multiColumnHeader.IsSortedAscending(sortedColumns[i]);

				switch(item.ParamType)
                {
					case "string":
						orderedQuery = orderedQuery.ThenBy(l => l.data.StringParam[item.ListIndex], ascending);
						break;
					case "int":
						orderedQuery = orderedQuery.ThenBy(l => l.data.IntParam[item.ListIndex], ascending);
						break;
					case "bool":
						orderedQuery = orderedQuery.ThenBy(l => l.data.BoolParam[item.ListIndex], ascending);
						break;
					case "float":
						orderedQuery = orderedQuery.ThenBy(l => l.data.FloatParam[item.ListIndex], ascending);
						break;
				}
			}

			rootItem.children = orderedQuery.Cast<TreeViewItem>().ToList();
		}

		IOrderedEnumerable<TreeViewItem<SkillGraphShowData>> InitialOrder(IEnumerable<TreeViewItem<SkillGraphShowData>> myTypes, int[] history)
		{
			int index = history[0];
			var item = index >= dataTitle.TitleItemsWithoutList.Length ? dataTitle.ListItem : dataTitle.TitleItemsWithoutList[index];
			bool ascending = multiColumnHeader.IsSortedAscending(history[0]);
			switch (item.ParamType)
			{
				case "string":
					return myTypes.Order(l => l.data.StringParam[item.ListIndex], ascending);
				case "int":
					return myTypes.Order(l => l.data.IntParam[item.ListIndex], ascending);
				case "bool":
					return myTypes.Order(l => l.data.BoolParam[item.ListIndex], ascending);
				case "float":
					return myTypes.Order(l => l.data.FloatParam[item.ListIndex], ascending);
			}
			return myTypes.Order(l => l.data.StringParam[0], ascending);
		}
		
		public override void OnGUI(Rect rect)
		{
			base.OnGUI(rect);
		}
		
		protected override void RowGUI(RowGUIArgs args)
		{
			var item = (TreeViewItem<SkillGraphShowData>)args.item;

			for (int i = 0; i < args.GetNumVisibleColumns(); ++i)
			{
				CellGUI(args.GetCellRect(i), item, args.GetColumn(i), ref args);
			}
		}

		void CellGUI(Rect cellRect, TreeViewItem<SkillGraphShowData> item, int column, ref RowGUIArgs args)
		{
			//if we dont want this item, its display params would be empty
			if (item.data.DisplayParam.Count == 0)
			{
				return;
			}
			if(dataTitle.ListItem != null && column >= dataTitle.TitleItemsWithoutList.Length)
            {
				int index = column - dataTitle.TitleItemsWithoutList.Length;
				var ListItem = dataTitle.ListItem;
				var list = item.data.ObjectParam[ListItem.ListIndex] as List<string>;
				if (index < list.Count)
                {
					int lines = list[index].Split('\n').Length;
					CenterRectUsingComplexHeight(ref cellRect, lines);
					DefaultGUI.Label(cellRect, list[index], args.selected, args.focused);
                }
				return;
            }
			var showTitleItem = dataTitle.TitleItemsWithoutList[column];
			if (showTitleItem.ShowType == "Name")
			{
				CenterRectUsingSingleLineHeight(ref cellRect);
				args.rowRect = cellRect;
				base.RowGUI(args);
			}
			else
			{
				int lines = 1;
				string value = item.data.DisplayParam[column];
				lines = value.Split('\n').Length;
				
				CenterRectUsingComplexHeight(ref cellRect, lines);
				DefaultGUI.Label(cellRect, value, args.selected, args.focused);
			}
		}

		public MultiColumnHeaderState CreateDefaultMultiColumnHeaderState(float treeViewWidth)
		{
			List<MultiColumnHeaderState.Column> columns = new List<MultiColumnHeaderState.Column>();
			foreach(var item in dataTitle.TitleItemsWithoutList)
            {
				columns.Add(new MultiColumnHeaderState.Column
				{
					headerContent = new GUIContent(item.Title, item.Desc),
					headerTextAlignment = TextAlignment.Center,
					sortedAscending = true,
					sortingArrowAlignment = TextAlignment.Center,
					width = item.DefaultWidth,
					minWidth = item.MinWidth,
					autoResize = false,
					allowToggleVisibility = item.AllowHide
				});
            }
			for(int i = 0; i < extraCol;++i)
            {
				columns.Add(new MultiColumnHeaderState.Column
				{
					headerContent = new GUIContent($"{dataTitle.ListItem.Title}_{i + 1}", dataTitle.ListItem.Desc),
					headerTextAlignment = TextAlignment.Center,
					sortedAscending = true,
					sortingArrowAlignment = TextAlignment.Center,
					width = dataTitle.ListItem.DefaultWidth,
					minWidth = dataTitle.ListItem.MinWidth,
					autoResize = false,
					allowToggleVisibility = dataTitle.ListItem.AllowHide
				});
			}

			var state = new MultiColumnHeaderState(columns.ToArray());
			return state;
		}
		//save this TreeView as tsv file
		public void SaveTreeView(string savePath)
		{
			TextWriter writer = new StreamWriter(@savePath);

			List<TreeViewItem> itemList = new List<TreeViewItem>();
			TreeToList(rootItem, itemList);
			string line = "";
			//write lines
			if (dataTitle.isScript == "true")
			{
				line = "";
				//write titles
				foreach (var item in dataTitle.TitleItems)
				{
					line += $"{item.Title}\t";
				}
				writer.WriteLine(line);
				
				//write content
				for (int i = 0; i < itemList.Count; i++) 
				{
					line = "";
					var data = (itemList[i] as TreeViewItem<SkillGraphShowData>).data;
					if (data.DisplayParam.Count == 0)
						continue;
					foreach (string param in data.DisplayParam)
					{
						line += $"{param}\t";
					}

					writer.WriteLine(line);
				}
			}
			else
			{
				line = "";
				foreach (var item in dataTitle.TitleItems)
				{
					line += $"{item.Title}\t";
					if (item.Title == "Name")
						line += "ScriptName\t";
				}
				writer.WriteLine(line);

				for (int i = 0; i < itemList.Count; i++) 
				{
					line = "";
					if (itemList[i].depth < 1)
						continue;

					var data = (itemList[i] as TreeViewItem<SkillGraphShowData>).data;
					if (data.DisplayParam.Count == 0)
						continue;
					
					for (int j = 0; j < data.DisplayParam.Count; j++)
					{
						line += $"{data.DisplayParam[j]}\t";
						if (j == 0)
						{
							line += $"{itemList[i].parent.displayName}\t";
						}
					}
					writer.WriteLine(line);
				}
			}

			//writer.WriteLine("");
			writer.Close();
		}
	}
}
