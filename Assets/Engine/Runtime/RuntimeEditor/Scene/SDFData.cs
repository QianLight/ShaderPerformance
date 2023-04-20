
#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEngine;

public class SDFData : ScriptableObject
{
    [SerializeField]
    public List<float> distances = new List<float>();
    // Start is called before the first frame update
}
#endif
