/*******************************************************************
* Copyright © 2017—2021 Perfect World.Co.Ltd. All rights reserved.
* author: 移动项目支持部 ZEUS FRAMEWORK TEAM
********************************************************************/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;

public class CustomMeshTool : MonoBehaviour
{
	// Start is called before the first frame update
	[MenuItem("Zeus/Asset/SetMeshReadable")]
	public static void SetMeshReadable()
	{
		Object[] SelectedAsset = Selection.GetFiltered(typeof(Object), SelectionMode.DeepAssets);
		foreach (Object obj in SelectedAsset)
		{
			if(obj is Mesh)
            {
				string filePath = AssetDatabase.GetAssetPath(obj);
				filePath = filePath.Replace("/", "\\");
				string fileText = File.ReadAllText(filePath);
                if (fileText.Contains("m_IsReadable:"))
                {
					fileText = fileText.Replace("m_IsReadable: 0", "m_IsReadable: 1");
					File.WriteAllText(filePath, fileText);
				}
			}
		}
		AssetDatabase.Refresh();
	}
}
