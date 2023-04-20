using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaterEffencyTest : MonoBehaviour
{
    public Material m_matOriginal;
    public Material m_mat1;
    public Material m_mat2;
    public Material m_mat3;
    public Material m_mat4;

    private void Start()
    {
        DontDestroyOnLoad(this.gameObject);
    }

    private void OnGUI()
    {
        GUI.Label(new Rect(200, 200, 400, 100), "水效率测试按钮，临时使用，之后删除");
        if(GUI.Button(new Rect(200,300,400,100), "Original")){
            GameObject waterGo = GameObject.Find("water2_fbx_0");
            waterGo.GetComponent<MeshRenderer>().sharedMaterial = m_matOriginal;
        }
        if (GUI.Button(new Rect(200, 400, 400, 100), "1"))
        {
            GameObject waterGo = GameObject.Find("water2_fbx_0");
            waterGo.GetComponent<MeshRenderer>().sharedMaterial = m_mat1;
        }
        if (GUI.Button(new Rect(200, 500, 400, 100), "2"))
        {
            GameObject waterGo = GameObject.Find("water2_fbx_0");
            waterGo.GetComponent<MeshRenderer>().sharedMaterial = m_mat2;
        }
        if (GUI.Button(new Rect(200, 600, 400, 100), "3"))
        {
            GameObject waterGo = GameObject.Find("water2_fbx_0");
            waterGo.GetComponent<MeshRenderer>().sharedMaterial = m_mat3;
        }
        if (GUI.Button(new Rect(200, 700, 400, 100), "4"))
        {
            GameObject waterGo = GameObject.Find("water2_fbx_0");
            waterGo.GetComponent<MeshRenderer>().sharedMaterial = m_mat4;
        }
        if(GUI.Button(new Rect(200, 800, 400, 100),"Dont Lock FPS")){
            Application.targetFrameRate = -1;
        }
    }
}
