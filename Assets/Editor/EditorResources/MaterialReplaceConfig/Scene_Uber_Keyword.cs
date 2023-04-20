using System.Collections;
using System.Collections.Generic;
using CFEngine.Editor;
using UnityEngine;

public class Scene_Uber_Keyword : MaterialReplaceProcessor
{
    private bool emission;
    public override string DisplayName
    {
        get
        {
            return "Scene_Uber";
        } 
    }

    public override void PreProcessMaterial(Material material)
    {
        emission = material.GetVector("_Param2").w > 0.5f;
    }

    public override void PostProcessMaterial(Material material)
    {

        if (material.IsKeywordEnabled("_ALPHA_TEST"))
        {
            material.DisableKeyword("_ALPHA_TEST");
            material.EnableKeyword("_ALPHATEST_ON");
        }

        if (emission)
        {
            material.EnableKeyword("_EMISSION_ON");
        }
        material.EnableKeyword("_METALLIC_ON");
        material.EnableKeyword("_OCCLUSION_ON");
        
    }
}
