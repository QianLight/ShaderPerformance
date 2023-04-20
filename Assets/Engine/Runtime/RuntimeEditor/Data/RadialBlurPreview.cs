#if UNITY_EDITOR

using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering.Universal;

[ExecuteInEditMode]
public class RadialBlurPreview : MonoBehaviour
{
    public RadialBlurParam param;
    public int id;
    
    private void OnEnable()
    {
        EditorApplication.update += EditorUpdate;
        id = URPRadialBlur.instance.AddParam(param, URPRadialBlurSource.EditorPreview, 0);
    }

    void EditorUpdate()
    {
        URPRadialBlur.instance.ModifyParam(id, param);
    }

    private void OnDisable()
    {
        EditorApplication.update -= EditorUpdate;
        URPRadialBlur.instance.RemoveParam(id);
        id = -1;
    }
}

#endif