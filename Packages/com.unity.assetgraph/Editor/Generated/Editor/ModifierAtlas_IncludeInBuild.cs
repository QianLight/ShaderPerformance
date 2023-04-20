using UnityEngine;
using UnityEditor;
using System.Linq;
using System;
using System.Collections.Generic;
using UnityEngine.U2D;
using UnityEditor.U2D;
using UnityEngine.AssetGraph;

[CustomModifier("IncludeInBuild", typeof(SpriteAtlas))]
public class ModifierAtlas_IncludeInBuild : IModifier {

	[SerializeField] private bool includeInBuild;
    [SerializeField] private bool packAtlas;

    public void OnValidate () {
    }
    
	// Test if asset is different from intended configuration 
	public bool IsModified (UnityEngine.Object[] assets, List<AssetReference> group) {

        return true;
	}

	// Actually change asset configurations. 
	public void Modify (UnityEngine.Object[] assets, List<AssetReference> group) {
        var spriteAssets = group.Where(a => a.assetType == typeof(SpriteAtlas)).OrderBy(a => a.fileName);
        List<SpriteAtlas> atlasList = new List<SpriteAtlas>();
        foreach (var a in assets)
        {
            SpriteAtlas atlas = a as SpriteAtlas;
            if (atlas == null)
            {
                continue;
            }
            SpriteAtlasExtensions.SetIncludeInBuild(atlas, includeInBuild);
            atlasList.Add(atlas);
        }
        if (packAtlas)
        {
            SpriteAtlasUtility.PackAtlases(atlasList.ToArray(), EditorUserBuildSettings.activeBuildTarget, false);
        }

    }

	// Draw inspector gui 
	public void OnInspectorGUI (Action onValueChanged) {

		EditorGUILayout.HelpBox("This is the inspector of your custom Modifier. You can customize by implementing OnInspectorGUI().", MessageType.Info);

		var newValue = GUILayout.Toggle(includeInBuild, "Include In Build");
		if(newValue != includeInBuild) {
            includeInBuild = newValue;
			onValueChanged();
		}

        var newPackAtlas = GUILayout.Toggle(packAtlas, "Pack Atlas");
		if(newPackAtlas != packAtlas) {
            packAtlas = newPackAtlas;
			onValueChanged();
		}
	}
}
