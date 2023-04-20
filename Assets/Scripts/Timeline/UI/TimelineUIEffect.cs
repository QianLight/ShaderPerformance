using UnityEngine;

#if UNITY_EDITOR
[ExecuteInEditMode]
#endif

public class TimelineUIEffect : MonoBehaviour
{
    public Material material;

#if UNITY_EDITOR
    [Range(0, 1)]
#endif
    public float softness;

    public Color color;

    public Texture2D tex;

    private void Start()
    {
        if (material && tex)
        {
            material.SetTexture("_MaskTex", tex);
        }
    }


    public void SyncEffect()
    {
        if (material != null)
        {
            material.SetColor("_Color", color);
            material.SetFloat("_Softness", softness);
            if (tex)
            {
                material.SetTexture("_MaskTex", tex);
            }
        }
    }

    private void OnValidate()
    {
        SyncEffect();
    }

    // Call from unity if animation properties have changed
    private void OnDidApplyAnimationProperties()
    {
        SyncEffect();
    }
}