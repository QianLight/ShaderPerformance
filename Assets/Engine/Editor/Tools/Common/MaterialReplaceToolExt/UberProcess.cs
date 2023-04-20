using CFEngine.Editor;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class UberProcess : MaterialReplaceProcessor
{
    public override string DisplayName
    {
        get {  return "UberProcess"; }
    }

    private void ClearData(Material mat)
    {
        SerializedObject psSource = new SerializedObject(mat);
        SerializedProperty emissionProperty = psSource.FindProperty("m_SavedProperties");
        SerializedProperty texEnvs = emissionProperty.FindPropertyRelative("m_TexEnvs");
        SerializedProperty floats = emissionProperty.FindPropertyRelative("m_Floats");
        SerializedProperty colors = emissionProperty.FindPropertyRelative("m_Colors");
        texEnvs.ClearArray();
        floats.ClearArray();
        colors.ClearArray();
        psSource.ApplyModifiedProperties();
    }
    
    public override void PostProcessMaterial(Material material)
    {
        if (material.IsKeywordEnabled("_ALPHA_TEST"))
        {
            material.EnableKeyword("_ALPHATEST_ON");
            material.SetFloat("_Cutoff", 0.5f);
            material.SetFloat("_AlphaTest", 1);
            material.renderQueue = 2450;
        }
        else
        {
            material.DisableKeyword("_ALPHATEST_ON");
            material.SetFloat("_Cutoff", 0.5f);
            material.SetFloat("_AlphaTest", 0);
        }
        material.DisableKeyword("_ALPHA_TEST");
        
        if (material.GetFloat("_Occlusion") > 0.5f)
        {
            material.EnableKeyword("_OCCLUSION_ON");
        }
        else
        {
            material.DisableKeyword("_OCCLUSION_ON");
        }
        
        if (material.GetFloat("_Metallic") > 0.5f)
        {
            material.EnableKeyword("_METALLIC_ON");
        }
        else
        {
            material.DisableKeyword("_METALLIC_ON");
        }
        
        if (material.GetFloat("_Emission") > 0.5f)
        {
            material.EnableKeyword("_EMISSION_ON");
        }
        else
        {
            material.DisableKeyword("_EMISSION_ON");
        }
    }
}
