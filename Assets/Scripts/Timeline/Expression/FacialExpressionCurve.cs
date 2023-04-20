using System;
using CFEngine;
using System.Collections.Generic;
using System.Runtime.Serialization;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;

[ExecuteInEditMode]
[Serializable]
public class FacialExpressionCurve : MonoBehaviour
{
    public float idle = 1;

    public float A = 0;
    public float E = 0;
    public float I = 0;
    public float O = 0;
    public float U = 0;

    public float eyebrow_sad = 0;
    public float eyebrow_happy = 0;
    public float eyebrow_angry = 0;

    public float eye_Squint = 0;
    public float eye_Earnest = 0;
    public float eye_Stare = 0;

    public float mouth_smile = 0;
    public float mouth_laugh = 0;
    public float mouth_sad = 0;
    public float mouth_angry = 0;

    public float blink = 0;

    //public float angry = 0;
    //public float furious = 0;
    //public float giggle = 0;
    //public float happy = 0;
    //public float low = 0;
    //public float laugh = 0;
    //public float smile = 0;
    //public float pain = 0;
    //public float serious = 0;
    //public float surprise = 0;
    //public float getangry = 0;
    //public float normal = 0;
    //public float teether = 0;
    //public float afraid = 0;
    //public float nervous = 0;
    //public float sad = 0;

    [HideInInspector]
    public List<float> m_weights;
    public bool m_clearWeight = false;

    private void OnEnable()
    {
        if (m_weights == null) m_weights = new List<float>();
        m_weights.Clear();
        m_weights.Add(idle);
        m_weights.Add(A);
        m_weights.Add(E);
        m_weights.Add(I);
        m_weights.Add(O);
        m_weights.Add(U);

        m_weights.Add(eyebrow_sad);
        m_weights.Add(eyebrow_happy);
        m_weights.Add(eyebrow_angry);
        m_weights.Add(eye_Squint);
        m_weights.Add(eye_Earnest);
        m_weights.Add(eye_Stare);
        m_weights.Add(mouth_smile);
        m_weights.Add(mouth_laugh);
        m_weights.Add(mouth_angry);
        m_weights.Add(mouth_sad);

        m_weights.Add(blink);

        //m_weights.Add(surprise);
        //m_weights.Add(getangry);
        //m_weights.Add(normal);
        //m_weights.Add(teether);
        //m_weights.Add(afraid);
        //m_weights.Add(nervous);
        //m_weights.Add(sad);
    }

    public void Update()
    {
        if (m_clearWeight)
        {
            SetAllWeightToZero();
        }
        else
        {
            m_weights[0] = idle;
            m_weights[1] = A;
            m_weights[2] = E;
            m_weights[3] = I;
            m_weights[4] = O;
            m_weights[5] = U;

            m_weights[6] = eyebrow_sad;
            m_weights[7] = eyebrow_happy;
            m_weights[8] = eyebrow_angry;

            m_weights[9]  = eye_Squint;
            m_weights[10] = eye_Earnest;
            m_weights[11] = eye_Stare;

            m_weights[12] = mouth_smile;
            m_weights[13] = mouth_laugh;
            m_weights[14] = mouth_sad;
            m_weights[15] = mouth_angry;

            m_weights[16] = blink;

            //m_weights[15] = surprise;
            //m_weights[16] = getangry;
            //m_weights[17] = normal;
            //m_weights[18] = teether;
            //m_weights[19] = afraid;
            //m_weights[20] = nervous;
            //m_weights[21] = sad;
        }
    }

    public void SetToZero(bool clearWeight)
    {
        m_clearWeight = clearWeight;
    }

    public void SetAllWeightToZero()
    {
        //A = 0;
        //E = 0;
        //I = 0;
        //O = 0;
        //U = 0;
        eyebrow_sad = 0;
        eyebrow_happy = 0;
        eyebrow_angry = 0;

        eye_Squint = 0;
        eye_Earnest = 0;
        eye_Stare = 0;

        mouth_smile = 0;
        mouth_laugh = 0;
        mouth_angry = 0;
        mouth_sad = 0;

        blink = 0;
        //angry = 0;
        //furious = 0;
        //giggle = 0;
        //happy = 0;
        //low = 0;
        //laugh = 0;
        //smile = 0;
        //pain = 0;
        //serious = 0;
        //surprise = 0;
        //getangry = 0;
        //normal = 0;
        //teether = 0;
        //afraid = 0;
        //nervous = 0;
        //sad = 0;
    }
}
