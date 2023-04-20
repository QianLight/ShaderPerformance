using UnityEngine;
using UnityEditor;

using System;
using System.Collections.Generic;

using UnityEngine.AssetGraph;

[CustomModifier("ClearDefaultMaps", typeof(Shader))]
public class ModifierShader_ClearDefaultMaps : IModifier {

	[SerializeField] private bool doSomething;

    public void OnValidate () {
    }
    
	// Test if asset is different from intended configuration 
	public bool IsModified (UnityEngine.Object[] assets, List<AssetReference> group) {
		return true;
	}

	// Actually change asset configurations. 
	public void Modify (UnityEngine.Object[] assets, List<AssetReference> group) {
		string[] defaultNames = new string[0];
		Texture[] defaultTextures = new Texture[0];
		foreach (var a in assets)
        {
            var assetPath = AssetDatabase.GetAssetPath(a);
			var importer =  AssetImporter.GetAtPath(assetPath);
            if (importer == null)
            {
                continue;
            }
            var shaderImporter = importer as ShaderImporter;
			
			shaderImporter.SetDefaultTextures(defaultNames, defaultTextures);
			shaderImporter.SaveAndReimport();
        }

	}

	// Draw inspector gui 
	public void OnInspectorGUI (Action onValueChanged) {

		EditorGUILayout.HelpBox("This is the inspector of your custom Modifier. You can customize by implementing OnInspectorGUI().", MessageType.Info);

		var newValue = GUILayout.Toggle(doSomething, "Example toggle");
		if(newValue != doSomething) {
			doSomething = newValue;
			onValueChanged();
		}
	}
}
