using UnityEngine;
using UnityEditor;

using System;
using System.Collections.Generic;

using UnityEngine.AssetGraph;

[CustomModifier("Detach Reference From Scene", typeof(LightingDataAsset))]
public class LightingDataZeusModifier : IModifier {

	public void OnValidate () {
    }
    
	// Test if asset is different from intended configuration 
	public bool IsModified (UnityEngine.Object[] assets, List<AssetReference> group) {
		return true;
	}

	// Actually change asset configurations. 
	public void Modify (UnityEngine.Object[] assets, List<AssetReference> group) {
		foreach (var assetReference in group)
		{
			var assetsData = assetReference.allData;
			foreach (var tempObj in assetsData)
			{
				try
				{
					SerializedObject tempLightingData = new SerializedObject(tempObj);
					SerializedProperty tempSceneProperty = tempLightingData.FindProperty("m_Scene");
					Debug.Log("LightingData.asset : " + tempObj.name + " | " + tempSceneProperty?.name);
					if (null == tempSceneProperty)
						continue;
					tempSceneProperty.objectReferenceInstanceIDValue = 0;
					tempSceneProperty.objectReferenceValue = null;
					tempLightingData.ApplyModifiedProperties();
					EditorUtility.SetDirty(tempObj);
					Debug.Log("LightingData.asset clear : " + tempSceneProperty.objectReferenceValue);
				}
				catch (Exception e)
				{
					Debug.LogError("LightingData.asset E : "+e);
					throw;
				}
			}
		}
		AssetDatabase.SaveAssets();
		AssetDatabase.Refresh();
	}

	// Draw inspector gui 
	public void OnInspectorGUI (Action onValueChanged) {

	}
}
