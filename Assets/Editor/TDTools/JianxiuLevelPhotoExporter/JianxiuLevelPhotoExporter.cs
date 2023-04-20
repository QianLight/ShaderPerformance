using UnityEditor;
using UnityEngine;
using System.IO;
using System.Text;
using LevelEditor;
using System.Collections.Generic;
using System.Diagnostics;
using CFUtilPoolLib;
using System.Linq;

namespace TDTools
{
	public class JianxiuLevelPhotoExporter : EditorWindow
	{
		private GUIStyle style;
		private static string levelPath = "";
		private static string cutScenePath = "";
		private const string exportPath = "/Editor/TDTools/JianxiuLevelPhotoExporter";
		private static List<LevelCharacterData> LevelCharacterDataList;
		private static readonly string TitleLine = "ScriptName\tOfficialName\tShowName";

		[MenuItem("Tools/TDTools/监修相关工具/关卡人物图片导出")]
		public static void ShowWindow()
		{
			//Thread thread = new Thread(new ThreadStart(CmdCtr));
			//thread.Start();
			var win = GetWindowWithRect<JianxiuLevelPhotoExporter>(new Rect(0, 0, 800, 400));
			win.titleContent = new GUIContent("关卡人物图片导出");
			win.Show();
		}
		private void OnEnable()
		{
			style = new GUIStyle() { fontSize = 12, normal = new GUIStyleState() { textColor = Color.white } };
			levelPath = $"{Application.dataPath}/BundleRes/Table/Level";
			cutScenePath = $"{Application.dataPath}/BundleRes/Timeline";
			
		}

		private void OnGUI()
		{
			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.LabelField("选择关卡目录：", levelPath, style);
			if (GUILayout.Button("...", new GUILayoutOption[] { GUILayout.Width(50) }))
			{
				levelPath = EditorUtility.OpenFolderPanel("选择关卡目录", $"{Application.dataPath}/BundleRes/Table/Level", "");
			}
			EditorGUILayout.EndHorizontal();
			if (GUILayout.Button("导出"))
			{
				Initial();
				DoExporter();
			}
			if (GUILayout.Button("打开导出文件夹"))
			{
				EditorUtility.RevealInFinder(Application.dataPath+exportPath+ "/JianxiuLevelPhotoExporter.cs");
			}
		}
		void Initial()
        {
			LevelCharacterDataList = new List<LevelCharacterData>();
		}

		static void DoExporter()
		{
			GetCFGMessage();
			GetName();
			string arg=SaveFile();
            UnityEngine.Debug.Log(arg);
			string basePath = $"{Application.dataPath}{exportPath}/JianxiuPhoto.py";
			RunPythonScript(basePath, arg);
		}

		//读关卡脚本得到wave信息和cutscene信息
		static void GetCFGMessage()
        {
			DirectoryInfo root = new DirectoryInfo(levelPath);
			MapListReader.Reload();
			XEntityStatisticsReader.Reload();

			foreach (FileInfo f in root.GetFiles("*.cfg"))
			{
				string name = f.Name;
				string path = levelPath + "\\" + name;
				string configFile = path.Replace(Application.dataPath + "/BundleRes/Table/", "").Replace(".cfg", "").Replace('\\','/');
				//UnityEngine.Debug.Log(configFile);
				string levelName;
				if (MapListReader.GetZhuxianDataByLevelConfigFile(configFile) != null&& MapListReader.GetZhuxianDataByLevelConfigFile(configFile).Comment!="")
                {
					levelName = MapListReader.GetZhuxianDataByLevelConfigFile(configFile).Comment;
					UnityEngine.Debug.Log(levelName);

					var grphData = DataIO.DeserializeData<LevelEditorData>(path);

					LevelCharacterDataList.Add(new LevelCharacterData(levelName));
					int num = LevelCharacterDataList.Count - 1;

					foreach (var graph in grphData.GraphDataList)
					{
						foreach (var node in graph.WaveData)
						{
							if(node.NodeID==34)
                            {
								int i = 0;
                            }
							if (LevelEditorTools.IsNodeEnable(graph, node, node.NodeID, name))
								LevelCharacterDataList[num].PresentID.Add(XEntityStatisticsReader.GetDataBySid((uint)node.SpawnID).PresentID);
						}
						foreach (var node in graph.ScriptData)
						{
							switch (node.Cmd)
							{
								case LevelScriptCmd.Level_Cmd_Cutscene:
									if (LevelEditorTools.IsNodeEnable(graph, node, node.NodeID, name))
									{
										string perfabPath = cutScenePath + "/" + node.stringParam[0] + ".prefab";

										GameObject cutscenePrefab;
										try
										{
											cutscenePrefab = PrefabUtility.LoadPrefabContents(perfabPath);
											var comp = cutscenePrefab.GetComponentInChildren<OrignalTimelineData>();
											var chars = comp?.chars;
											if (chars != null)
											{
												foreach (var c in chars)
												{
													//UnityEngine.Debug.Log(c.prefab);
													uint presentID = XEntityPresentationReader.GetPresentIDByPrefab(c.prefab.ToLower());
													if (presentID != 0)
														LevelCharacterDataList[num].PresentID.Add(presentID);
												}
											}
											//UnityEngine.Debug.Log(node.stringParam[0]);
										}
										catch
										{
											UnityEngine.Debug.Log($"{node.stringParam[0]} not found");
											break;
										}
									}
									break;
								default:
									break;
							}
						}
					}
					LevelCharacterDataList[num].PresentID = LevelCharacterDataList[num].PresentID.Distinct().ToList();
				}
			}
			//foreach(var i in LevelCharacterDataList)
			//{
			//	foreach(var j in i.PresentID)
			//		UnityEngine.Debug.Log(i.LevelName+j);
			//}
		}
		static void GetName()
        {
			XEntityPresentationReader.Reload();
			foreach (var i in LevelCharacterDataList)
            {
				i.PresentID= i.PresentID.Distinct().ToList();
				foreach(var j in i.PresentID)
                {
					string oName = XEntityPresentationReader.GetOfficialNameByID(j);
					if (!string.IsNullOrEmpty(oName))
                    {
						if(i.OfficialName.Contains(oName)==false)
                        {
							i.OfficialName.Add(oName);
							if (XEntityStatisticsReader.GetData(j) != null)
								i.ShowName.Add(XEntityStatisticsReader.GetData(j).Name);
							else i.ShowName.Add("");
							//UnityEngine.Debug.Log(name);
						}
					}
				}
			}
        }

		static string SaveFile()
		{
			string name = levelPath.Replace($"{Application.dataPath}/BundleRes/Table/Level","")+".txt";
			StringBuilder file = new StringBuilder();
			file.AppendLine(TitleLine);
			foreach (var i in LevelCharacterDataList)
			{
				for (int j=0;j<i.OfficialName.Count;j++)
                {
                    file.AppendLine($"{i.LevelName}\t{i.OfficialName[j]}\t{i.ShowName[j]}");
				}

			}
			try
			{
				File.WriteAllText($"{Application.dataPath}{exportPath}/LevelNameFile{name}", file.ToString(), Encoding.GetEncoding("gb2312"));
			}
			catch
			{
			}
			return name;
		}
		public static void RunPythonScript(string pyScriptPath, params string[] argvs)
		{
			Process process = new Process();

			//在python中用sys.argv[ ]使用该参数
			process.StartInfo.FileName = $"{Application.dataPath}{exportPath}/dist/JianxiuPhoto.exe";

			if (argvs != null)
			{
				// 添加参数 （组合成：python xxx/xxx/xxx/test.python param1 param2）
				foreach (string item in argvs)
				{
					pyScriptPath += " " + item;
				}
			}
			//UnityEngine.Debug.Log(pyScriptPath);
			//UnityEngine.Debug.Log($"{Application.dataPath}{exportPath}/dist/CheckName.exe");

			process.StartInfo.UseShellExecute = false;
			process.StartInfo.Arguments = pyScriptPath;     // 路径+参数
			process.StartInfo.WorkingDirectory = $"{Application.dataPath}{exportPath}";  //如果没有这个，运行python.exe会无法写入文档
			process.StartInfo.RedirectStandardError = true;
			process.StartInfo.RedirectStandardInput = true;
			process.StartInfo.RedirectStandardOutput = true;
			process.StartInfo.CreateNoWindow = true;        // 不显示执行窗口

			// 开始执行，获取执行输出，添加结果输出委托
			process.Start();
			process.BeginOutputReadLine();
			process.BeginErrorReadLine();//开始读取错误数据
			process.OutputDataReceived += new DataReceivedEventHandler(GetData);
			process.WaitForExit();
			process.Close();
		}

		static void GetData(object sender, DataReceivedEventArgs e)
		{

			// 结果不为空才打印（后期可以自己添加类型不同的处理委托）
			if (string.IsNullOrEmpty(e.Data) == false)
			{
				UnityEngine.Debug.Log(e.Data);
			}
		}

	}
	public class LevelCharacterData
	{
		public string LevelName;
		public List<uint> PresentID;
		public List<string> OfficialName;
		public List<string> ShowName;

		public LevelCharacterData(string levelName)
		{
			LevelName = levelName;
			PresentID = new List<uint>();
			OfficialName = new List<string>();
			ShowName = new List<string>();
		}
	}
}