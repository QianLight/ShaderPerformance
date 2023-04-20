using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
[ExecuteInEditMode]
#endif
public class setTerrainShadowTex : MonoBehaviour
{
#if UNITY_EDITOR
    // Start is called before the first frame update
    Shader sha0;

    Material mat0;

    RenderTexture RT;
    Material myMat;
    private Material TerrainMat =null;
    public RenderTexture myRT;

    private void Awake() {
        TerrainMat=GetComponent<Terrain>().materialTemplate;
        TerrainMat.shader=Shader.Find("Hidden/Custom/Editor/TerrainEdit_Custom");

        if(RT!=null)
        {
            RT.Release();
        }
        
        sha0=Shader.Find("Hidden/Custom/Editor/ShadowMap_Extra");

        mat0=new Material(sha0);

      //  myMat=GetComponent<Renderer>().sharedMaterial;
        RT= new RenderTexture(2048,2048,24);
    }

    // Update is called once per frame
    void Update()
    {
        if(RT!=null)
        {
            if(mat0!=null)
            {
                Graphics.Blit(null,RT,mat0);
                Shader.SetGlobalTexture("_ShadowMapTerrainTexTest",RT);
            }
            else
            {
                sha0=Shader.Find("Hidden/Custom/Editor/ShadowMap_Extra");
                mat0=new Material(sha0);
            }
        }
        myRT=RT;
        
    }
#endif
}
