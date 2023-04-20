using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class GameObjectPrimitive
{ 
[MenuItem("GameObject/3D Object/Shaderball")]
    public static void Shaderball()
    { 
        GameObject MyGO= AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Editor/Tools/ArtTools/Shaderball.fbx");
        GameObject GO= GameObject.Instantiate(MyGO); 
        GO.name="Shaderball";
        Vector3 CameraDir=  SceneView.lastActiveSceneView.camera.transform.forward; 
        Vector3 offsetPoint=new Vector3(CameraDir.x*10f,CameraDir.y*10f,CameraDir.z*10f);
        GO.transform.position=SceneView.lastActiveSceneView.camera.transform.position+offsetPoint; 
 
    } 
[MenuItem("GameObject/3D Object/RoundedCube")]
    public static void RoundedCube()
    { 
        GameObject MyGO= AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Editor/Tools/ArtTools/RoundedCube.fbx");
        GameObject GO= GameObject.Instantiate(MyGO); 
        GO.name="RoundedCube";
        Vector3 CameraDir=  SceneView.lastActiveSceneView.camera.transform.forward;
        Vector3 offsetPoint=new Vector3(CameraDir.x*10f,CameraDir.y*10f,CameraDir.z*10f);
        GO.transform.position=SceneView.lastActiveSceneView.camera.transform.position+offsetPoint; 
    } 
   
}
 