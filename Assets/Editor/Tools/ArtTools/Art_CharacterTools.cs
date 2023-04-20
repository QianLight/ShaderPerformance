using System;
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using CFEngine.Editor;

public class Art_CharacterTools : ArtToolsTemplate
{

    private bool IsButtonDown = false;
    private List<Renderer> listRenderer = new List<Renderer> ();
    private List<Material> listMat = new List<Material> ();
    private List<Material> listVaMat = new List<Material> ();
    private Shader myShader;
    private Shader OrShader;
    private GameObject[] myGOs;
    private String LabelStr = "";


    public override void OnGUI() 
    {
        EditorGUILayout.Space();

        GUILayout.BeginHorizontal();
        GUILayout.Label("检查顶点信息");

       if (!IsButtonDown)
        {
            if (GUILayout.Button ("替换验证材质", GUILayout.Width (100)))
            {
                listRenderer = new List<Renderer> ();
                listMat = new List<Material> ();
                listVaMat = new List<Material> ();
                myGOs = null;
                myGOs = Selection.gameObjects;
                LabelStr = "";
                //myRenderers=null;
                foreach (GameObject go in myGOs)
                {
                    Renderer[] myRenderers;
                    if (go)
                    {
                        myRenderers = go.GetComponentsInChildren<Renderer> ();
                        foreach (Renderer R in myRenderers)
                        {
                            if (R.sharedMaterial)
                            {
                                String SName = R.sharedMaterial.shader.name;
                                listMat.Add (R.sharedMaterial);
                                listRenderer.Add (R);
                            } 
                            else
                            {
                                listMat.Add (null);
                                listRenderer.Add (R);
                            }
                        }
                    }

                }
                myShader= Shader.Find ("Art_Tools/VertexsCheck");
                foreach (Renderer r in listRenderer)
                {
                    Material tempMat = new Material (myShader);
                    //tempMat.shader = Shader.Find ("Art_Tools/VertexsCheck");
                    tempMat.SetFloat ("_channel", 6);
                    r.sharedMaterial = tempMat;
                    listVaMat.Add (tempMat);
                }

                if (listRenderer.Count < 1)
                {
                    LabelStr = "未选中有效物件";
                }
                else
                {
                    IsButtonDown = true;
                }
            }
        }
        else
        {
            if (GUILayout.Button ("恢复原始材质", GUILayout.Width (100)))
            {
                for (int i = 0; i < listRenderer.Count; i++)
                {
                    if (listRenderer[i])
                    {
                        listRenderer[i].sharedMaterial = listMat[i];
                    }
                }
                IsButtonDown = false;
            }

        }
        



            GUILayout.EndHorizontal();
            GUILayout.Label (LabelStr);
            EditorGUILayout.Space();
        }
    


}
