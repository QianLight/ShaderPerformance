using System.Collections.Generic;
using TDTools;
using UnityEngine;
using UnityEngine.CFUI;
using Random = UnityEngine.Random;
using System.IO;
using System.Text;

namespace UnityEditor.TreeViewExamples
{

	static class MyTreeElementGenerator
	{
		static int IDCounter;
		static int minNumChildren = 5;
		static int maxNumChildren = 10;
		static float probabilityOfBeingLeaf = 0.5f;

		public static List<MyTreeElement> GenerateLabelTree(string url)
		{
            var treeElements = new List<MyTreeElement>();
			// string path = Application.dataPath + "/BundleRes/UI/OPsystemprefab" ;
			string path = Application.dataPath+ "/"+ url;
            string[] files = Directory.GetFiles(path, "*.prefab", SearchOption.AllDirectories);
			IDCounter = 0;
			var root = new MyTreeElement("Root", -1, IDCounter);
            treeElements.Add(root);
			string filepath = string.Empty;
			for(int i = 0;i < files.Length; i++)
            {
                filepath = files[i].Substring(files[i].IndexOf("Assets/"));
                //GameObject go = PrefabUtility.LoadPrefabContents(filepath);

				GameObject go = AssetDatabase.LoadAssetAtPath<GameObject>(filepath);
				if (go == null) continue;
				CFText[] cts = go.GetComponentsInChildren<CFText>(true);
				if (cts.Length > 0)
				{

					for (int j = 0; j < cts.Length; j++)
					{
						IDCounter++;
						var node = new MyTreeElement(cts[j].name, 0, IDCounter);
						node.Setup(cts[j] , filepath);			
						treeElements.Add(node);
					}
				}

				//PrefabUtility.UnloadPrefabContents(go);


            }
            return treeElements;
		}

	}
}
