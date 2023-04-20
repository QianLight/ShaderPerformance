#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using CFUtilPoolLib;

[ExecuteInEditMode]
public class ShipPointBehaviour : MonoBehaviour
{
    public uint roomPos;
    public float displayProbability;
    public bool HoldHere;
    public float patroIdleTime;
    [HideInInspector]public float patroIdleTime2;
    [HideInInspector] public string anim;
}

#endif
