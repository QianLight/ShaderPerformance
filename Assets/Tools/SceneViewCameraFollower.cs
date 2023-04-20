//Allows multiple SceneView cameras in the editor to be setup to follow gameobjects.
//October 2012 - Joshua Berberick

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[ExecuteInEditMode]
public class SceneViewCameraFollower : MonoBehaviour
{
	#if UNITY_EDITOR
	
	public bool on = true;
	private ArrayList sceneViews;
	
	void LateUpdate()
	{
		sceneViews = UnityEditor.SceneView.sceneViews;
		if(!on || sceneViews.Count == 0) return;

		UnityEditor.SceneView sceneView = (UnityEditor.SceneView) sceneViews[0];

		this.transform.position = sceneView.camera.transform.position;
		this.transform.rotation = sceneView.camera.transform.rotation;
	}
	
	public void OnDrawGizmos()
	{
		sceneViews = UnityEditor.SceneView.sceneViews;
		if(!on || sceneViews.Count == 0) return;
		
		UnityEditor.SceneView sceneView = (UnityEditor.SceneView) sceneViews[0];
		
		this.transform.position = sceneView.camera.transform.position;
		this.transform.rotation = sceneView.camera.transform.rotation;
	}
	
	#endif
}