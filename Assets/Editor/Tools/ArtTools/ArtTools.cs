using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;



public class ArtToolsTemplate  {
	public virtual void OnEnable() {
		
	}
	public virtual void OnGUI() {
		
	}
}


public class ArtTools : EditorWindow 
{
	private  GUIContent[] GC =null;
	private int barInt=2;
	List<ArtToolsTemplate> tools= new List<ArtToolsTemplate>();
	ArtToolsTemplate tool=null;

	[MenuItem("ArtTools/ArtTools")]
	private static void ShowWindow() {
		var window = GetWindow<ArtTools>();
		window.titleContent = new GUIContent("ArtTools");
		window.Show();
	}
	private void OnEnable() {
		GC=new GUIContent[]{
            new GUIContent("Common"),
            new GUIContent("Character"),
			new GUIContent("Scene"),
            new GUIContent("Animation"),
			new GUIContent("SFX"),
            new GUIContent("UI")
		};
        tools.Add(new Art_CommonTools());
        tools.Add(new Art_CharacterTools());
        tools.Add(new Art_SceneTools());
		tools.Add(new Art_AnimationTools());
		tools.Add(new Art_SFXTools());
        tools.Add(null);
		tool=tools[barInt];
		

	}

	private void OnGUI() 
	{
		EditorGUI.BeginChangeCheck();
		barInt=GUILayout.Toolbar(barInt,GC);
		if(EditorGUI.EndChangeCheck())
		{
			tool=tools[barInt];
		}
		if(tool!=null)
		{
			tool.OnEnable();
			tool.OnGUI();
		}
	}
}

