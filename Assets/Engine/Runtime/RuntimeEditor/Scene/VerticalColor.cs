using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//[ExecuteInEditMode]
public class VerticalColor : MonoBehaviour
{
    public Vector2 BoundsHighScale = new Vector2(0f,1f);
    // public float BoundsHigh = 0f;
    // public float BoundsScale = 1f;
    [Range(0.01f, 10f)]public float GradientSacale = 1f;
    public bool ColorBlend = true;
    public enum BlendMode{ Multiply, AddMultiply, Lerp}
    public BlendMode blendMode;
    [Range(0f, 1f)]public float BlendIntensity = 1f;
    public Color GradientBegin =  Color.white;
    public Color GradientEnd = Color.black;
    Renderer rend;
    Bounds maxBounds;
    Vector3 center;
    Vector3 size;

    void Awake()
    {
        CalculateBounds();
    }

    void Update()
    {
        #if UNITY_EDITOR
            CalculateBounds();
        #endif
    }
    void CalculateBounds()
    {
         Renderer[] childRend = GetComponentsInChildren<Renderer>();
            maxBounds = childRend[0].bounds;
            for (int i = 0; i < childRend.Length ; i++)
            {
                rend = childRend[i];
                maxBounds.Encapsulate(rend.bounds);
                SetMaterial();  
            }
        // if (transform.childCount != 0)
        // {
            // maxBounds = transform.GetChild(0).GetComponent<Renderer>().bounds;
            // for (int i = 0; i < transform.childCount ; i++)
            // {
            //     Transform child = transform.GetChild(i);
            //     rend = child.GetComponent<Renderer>();
            //     maxBounds.Encapsulate(rend.bounds);
            //     SetMaterial();  
            // }
        // }
        // else
        // {
        //     rend = GetComponent<Renderer>();
        //     maxBounds = rend.bounds;
        //     SetMaterial();
        // }
        // center = new Vector3(maxBounds.center.x  ,maxBounds.center.y + BoundsHigh , maxBounds.center.z);
        // size = new Vector3(maxBounds.size.x , maxBounds.size.y * BoundsScale , maxBounds.size.z);
        center = new Vector3(maxBounds.center.x  ,maxBounds.center.y + BoundsHighScale.x , maxBounds.center.z);
        size = new Vector3(maxBounds.size.x , maxBounds.size.y * BoundsHighScale.y , maxBounds.size.z);
    }
    void SetMaterial()
    {
        rend.material.EnableKeyword("_GRADIENT_ON");
        rend.material.SetVector("_WorldPositionScale",size);
        rend.material.SetVector("_WorldPositionOffset",center);
        rend.material.SetInt("_GradientSwitch",Convert.ToInt32(ColorBlend));
        rend.material.SetFloat("_GradientScale",GradientSacale);
        rend.material.SetColor("_GradientBegin",GradientBegin);
        rend.material.SetColor("_GradientEnd",GradientEnd);
        rend.material.SetInt("_BlendMode",Convert.ToInt32(blendMode));
        rend.material.SetFloat("_BlendIntensity",BlendIntensity);
    }
    void OnDrawGizmos()
    { 
                
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(center, size);
    }
}
