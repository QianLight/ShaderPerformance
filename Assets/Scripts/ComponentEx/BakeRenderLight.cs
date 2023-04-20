using UnityEngine;

[RequireComponent(typeof(Light))]
public class BakeRenderLight : MonoBehaviour
{
    private Light BakeLight;
    void Awake()
    {
        BakeLight = GetComponent<Light>();
    }


}