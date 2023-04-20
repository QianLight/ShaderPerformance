/*************************************************************
 *Parameter:
 * 1.Texture world center position
 * 2.Collision Data
 *     a. collision position
 *     b. collision Radius
 *     c. stable rate
 *
 * Result
 * 1. Generate final movement RT
 *     a. Assign collision RT to related shader
 ************************************************************/

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

//todo: class should be singleton and reset is required
public class GrassCollisionManager : MonoBehaviour
{
    public struct GrassCollisionData
    {
        //not supported yet
        public int CharactorID;
        public Vector3 PosWs;
        public float RadiusWS;

        //not supported yet
        public float CollisionDepth;
        public float StableRate;
    }

    RenderTexture RT_CollisionDepth;
    RenderTexture RT_LastCollisionDepth;
    [SerializeField] RenderTexture RT_Collision;

    [Header ("Target Actor")] public GameObject Actor;
    [Header ("Second Actor")] public GameObject SecondActor;
    [Header ("Collision Center")] public GameObject WorldCenterRef;

    [Header ("Fade Out Time in Seconds")][Range (0, 5)]
    public float FadeOutTime;

    [Range (0, 2)] public float StableRate;
    [Space (10)] public float World2RTScale = 32;
    public float ballSize = 1;
    public float springCircle = 4;

    public static Queue<GrassCollisionData> Quene_CollisionNotPlayer = new Queue<GrassCollisionData> ();

    //public static GrassCollisionData ST_CollisionNotPlayer;
    public Material MT_AddSphereGrassCollision;
    public Material MT_UpdateFade;
    public Material MT_OutputDir;

#if UNITY_EDITOR
    public bool EnableDebug = false;

    [FormerlySerializedAs ("GO_DebugQuad")][Header ("Debug GameObjects")]
    public GameObject GO_DebugQuad_Fade;

    public GameObject GO_DebugQuad2_Collision;
    public GameObject GO_DebugQuad3_Movement;
#endif

    public static Vector3 WorldCenterPosition
    {
        set => CurrentCenterPos = value;
        get => CurrentCenterPos;
    }

    public static Vector3 WorldCenterOffset
    {
        set => WorldOffset = value;
        //hack: only access when using ref world center position
        get => CurrentCenterPos - LastCenterPos;
    }

    private static Vector3 WorldOffset = Vector3.zero;
    private static Vector3 CurrentCenterPos = Vector3.zero;
    private static Vector3 LastCenterPos = Vector3.zero;

    //static Queue<GrassCollisionData> m_grassCollisionDatas = new Queue<GrassCollisionData>();
    private Vector3 UVOffset;
    private bool needUpdatePosition;
    private static readonly int CollisionRt = Shader.PropertyToID ("_CollisionRT");
    private static readonly int DeltaHeight = Shader.PropertyToID ("deltaHeight");
    private static readonly int Rate = Shader.PropertyToID ("stableRate");
    private static readonly int SpringCircle = Shader.PropertyToID ("SpringCircle");
    private static readonly int Radius = Shader.PropertyToID ("radius");
    private static readonly int Stable = Shader.PropertyToID ("stable");
    private static readonly int CollisionTexCenterWSID = Shader.PropertyToID ("CollisionTexCenterWS");

    private static readonly int posUV = Shader.PropertyToID ("posUV");

    //private static readonly int offset = Shader.PropertyToID("Offset");
    void Start ()
    {
        //set RT center wolrd postion
        if (WorldCenterRef != null)
            WorldCenterPosition = WorldCenterRef.transform.position;
        //R Depth, G Held, B Stable
        RT_CollisionDepth = new RenderTexture (256, 256, 0, RenderTextureFormat.ARGBHalf);
        RT_LastCollisionDepth = new RenderTexture (256, 256, 0, RenderTextureFormat.ARGBHalf);
        RT_Collision.Create ();
        RT_CollisionDepth.Create ();
        //RT_Collision
    }

    public static void AddCollisionWorldSpace (Vector3 worldPosition, float radius)
    {
        Quene_CollisionNotPlayer.Enqueue (new GrassCollisionData () { PosWs = worldPosition, CollisionDepth = 1, StableRate = 1, RadiusWS = radius });
    }

    private void CalculateOffset ()
    {
        var totalOffset = Vector3.zero;
        if (Actor != null)
        {
            CurrentCenterPos = Actor.transform.position;
            totalOffset += CurrentCenterPos - LastCenterPos;
        }
        else if (WorldCenterRef != null)
        {
            CurrentCenterPos = WorldCenterRef.transform.position;
            totalOffset += WorldOffset;

        }
        else
        {
            totalOffset += CurrentCenterPos - LastCenterPos;
        }

        UVOffset = totalOffset / World2RTScale;
        LastCenterPos = CurrentCenterPos;
    }

    void Update ()
    {
        CalculateOffset ();
        Shader.SetGlobalVector (CollisionTexCenterWSID,
            new Vector4 (CurrentCenterPos.x, CurrentCenterPos.y, CurrentCenterPos.z, World2RTScale));
        UpdateFade ();

        while (Quene_CollisionNotPlayer.Count > 0)
        {
            var data = Quene_CollisionNotPlayer.Dequeue ();
            AddSphereCollisionWS (data.PosWs, data.RadiusWS, data.StableRate);
        }
#if UNITY_EDITOR
        if (EnableDebug)
        {
            AddCenterSphereCollision ();
        }
#endif
        OutputDir ();
    }

    private void AddSphereCollisionWS (Vector3 posWS, float radius, float stable)
    {
        if (MT_AddSphereGrassCollision != null)
        {
            MT_AddSphereGrassCollision.SetTexture (CollisionRt, RT_LastCollisionDepth);
            MT_AddSphereGrassCollision.SetFloat (Radius, radius / World2RTScale);
            MT_AddSphereGrassCollision.SetFloat (Stable, stable);
            var localOffset = (posWS - CurrentCenterPos) / World2RTScale;
            MT_AddSphereGrassCollision.SetVector (posUV, localOffset);
            Graphics.Blit (null, RT_CollisionDepth, MT_AddSphereGrassCollision);
            Graphics.CopyTexture (RT_CollisionDepth, RT_LastCollisionDepth);
#if UNITY_EDITOR
            if (GO_DebugQuad2_Collision != null)
            {
                GO_DebugQuad2_Collision.GetComponent<Renderer> ().material.mainTexture = RT_CollisionDepth;
            }
#endif
        }

    }

    private void UpdateFade ()
    {
        if (MT_UpdateFade != null)
        {
            MT_UpdateFade.SetTexture (CollisionRt, RT_LastCollisionDepth);
            MT_UpdateFade.SetFloat (DeltaHeight, (1 / FadeOutTime) * Time.deltaTime);
            MT_UpdateFade.SetFloat (Rate, StableRate * Time.deltaTime);
            MT_UpdateFade.SetVector (posUV, new Vector2 (UVOffset.x, UVOffset.z));

            Graphics.Blit (null, RT_CollisionDepth, MT_UpdateFade);
            Graphics.CopyTexture (RT_CollisionDepth, RT_LastCollisionDepth);
#if UNITY_EDITOR
            if (GO_DebugQuad_Fade != null)
            {
                GO_DebugQuad_Fade.GetComponent<Renderer> ().material.mainTexture = RT_CollisionDepth;
            }
#endif
        }

    }

    private void OutputDir ()
    {
        if (MT_OutputDir != null)
        {
            MT_OutputDir.SetTexture (CollisionRt, RT_CollisionDepth);
            MT_OutputDir.SetFloat (SpringCircle, springCircle);
            Graphics.Blit (null, RT_Collision, MT_OutputDir);
#if UNITY_EDITOR
            if (GO_DebugQuad3_Movement != null)
            {
                //Graphics.CopyTexture(RT_Collision, RT_Collision);
                GO_DebugQuad3_Movement.GetComponent<Renderer> ().material.mainTexture = RT_Collision;
            }
#endif
        }

    }

#if UNITY_EDITOR
    //**********Test Code**************//
    //do not use following code when 
    private void AddCenterSphereCollision ()
    {
        if (MT_AddSphereGrassCollision != null)
        {
            MT_AddSphereGrassCollision.SetTexture (CollisionRt, RT_LastCollisionDepth);
            MT_AddSphereGrassCollision.SetFloat (Radius, ballSize / World2RTScale);
            MT_AddSphereGrassCollision.SetFloat (Stable, 1f);
            //MT_AddSphereGrassCollision.SetVector("posWS", Offset);
            Graphics.Blit (null, RT_CollisionDepth, MT_AddSphereGrassCollision);
            //debug
            if (GO_DebugQuad2_Collision != null)
            {
                GO_DebugQuad2_Collision.GetComponent<Renderer> ().material.mainTexture = RT_CollisionDepth;
            }

            Graphics.CopyTexture (RT_CollisionDepth, RT_LastCollisionDepth);
        }
    }
    //**********Test Code**************//
#endif
}