using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEngine.Rendering;
using UnityEditor;
[ExecuteInEditMode]
#endif
public class DestoryMe : MonoBehaviour
{
    public float Destory = -1; 
    private float duration = 0;
    // Update is called once per frame
    void Update()
    {
        if(Destory > 0)
        {
            duration += Time.deltaTime;
            if(duration > 0)
            {
                Destory = -1;
                DestroyImmediate(gameObject);
            }
        }
    }
}
