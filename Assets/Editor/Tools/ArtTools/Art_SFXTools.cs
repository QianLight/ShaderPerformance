using System;
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using CFEngine.Editor;

public class Art_SFXTools : ArtToolsTemplate
{
    public override void OnGUI() 
    {
        EditorGUILayout.Space();

        GUILayout.BeginHorizontal();
        GUILayout.Label("检查材质");

        if (GUILayout.Button("CheckMaterial", GUILayout.MaxWidth(200)))
        {
           EditorWindow.GetWindow(typeof(checkMat));
        }
        GUILayout.EndHorizontal();
        EditorGUILayout.Space();


        GUILayout.BeginHorizontal();
        GUILayout.Label("复制水的参数");

        if (GUILayout.Button("CopyWaterParameter", GUILayout.MaxWidth(200)))
        {
           CopyWaterParam();
        }
        GUILayout.EndHorizontal();

    }

    private void CopyWaterParam()
        {
            GameObject[] gameObjects= Selection.gameObjects;
            Material waterMaterial=null;
            List<Material> otherMaterial=new List<Material>();


            foreach(GameObject g in gameObjects)
            {
                Renderer r = g.GetComponent<Renderer>();
                if(r)
                {
                    if(r.sharedMaterial)
                    {
                        bool isWater=false;
                        isWater= r.sharedMaterial.shader.name.Equals("Custom/Scene/Water");
                        if(isWater==true && waterMaterial==null)
                        {
                            waterMaterial=r.sharedMaterial;
                            
                        }
                        else
                        {
                            otherMaterial.Add(r.sharedMaterial);
                        }
                    }
                    
                }
                
                    
            }
            

            if(waterMaterial)
            {
                foreach(Material myMat in otherMaterial)
                {
                    myMat.SetVector("_ParamA",waterMaterial.GetVector("_ParamA"));
                    myMat.SetVector("_ParamB",waterMaterial.GetVector("_ParamB"));
                    myMat.SetVector("_ParamC",waterMaterial.GetVector("_ParamC"));
                    myMat.SetVector("_Wave1",waterMaterial.GetVector("_Wave1"));
                    myMat.SetVector("_Wave2",waterMaterial.GetVector("_Wave2"));
                    myMat.SetVector("_Wave3",waterMaterial.GetVector("_Wave3"));
                    myMat.SetVector("_Wave4",waterMaterial.GetVector("_Wave4"));
                    myMat.SetVector("_Wave5",waterMaterial.GetVector("_Wave5"));
                    myMat.SetVector("_Wave6",waterMaterial.GetVector("_Wave6"));
                    myMat.SetVector("_Wave7",waterMaterial.GetVector("_Wave7"));
                    myMat.SetVector("_Wave8",waterMaterial.GetVector("_Wave8"));
                    myMat.SetVector("_Wave9",waterMaterial.GetVector("_Wave9"));
                    myMat.SetVector("_SteepnessFadeout",waterMaterial.GetVector("_SteepnessFadeout"));
                }
            }
            
           



        }
}
