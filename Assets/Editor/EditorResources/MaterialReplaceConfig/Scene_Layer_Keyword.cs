using CFEngine.Editor;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Scene_Layer_Keyword : MaterialReplaceProcessor
{

    public override string DisplayName
    {
        get
        {
            return "Scene_Layer";
        }
    }

    string[] OLD_KW =
    {
        "_SPLAT1",
        "_SPLAT2",
        "_SPLAT3",
        "_VERTEX_COLOR"
};

    string[] NEW_KW =
    {
        "_SPLAT_1X",
        "_SPLAT_2X",
        "_SPLAT_3X",
        "_VCMODE_ON"
    };

    public override void PostProcessMaterial(Material material)
    {
        for (int i = 0; i < 3; i++)
        {                
            material.DisableKeyword(NEW_KW[i]);

            if (material.IsKeywordEnabled(OLD_KW[i]))
            {
                material.DisableKeyword(OLD_KW[i]);
                material.EnableKeyword(NEW_KW[i]);
                material.SetInt("_SPLAT", i);
            }
        }

        for (int i = 3; i < 4; i++)
        {
            material.DisableKeyword(NEW_KW[i]);
            if (material.IsKeywordEnabled(OLD_KW[i]))
            {
                material.DisableKeyword(OLD_KW[i]);
                material.EnableKeyword(NEW_KW[i]);
                material.SetInt("_VCMode", 1);
            }
            else
            {
                material.SetInt("_VCMode", 0);
            }
        }
    }
}
