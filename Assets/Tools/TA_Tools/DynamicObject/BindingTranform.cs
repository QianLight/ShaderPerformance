using System;
using UnityEngine;
//using UnityEditor;
using System.Collections.Generic;
using System.Diagnostics;

#if UNITY_EDITOR
using UnityEditor;

[Serializable]
public class transformSync
{
    public Transform Target;
    public Transform Dummy; 
}
[ExecuteInEditMode]
#endif

public class BindingTranform : MonoBehaviour
{
#if UNITY_EDITOR
    public List<transformSync> BindingTrans=new List<transformSync>() ;
    private Animation myAnim;
      [HideInInspector]

    public float preTime = 0;
      [HideInInspector]
    public float MaxTime = 0;
    AnimationState animationState;
    [HideInInspector]
    public bool _play=false;
    public void Start() 
    { 
       myAnim=GetComponent<Animation>();
       
    }

 
    void Update()
    {
        if(!myAnim)
        {
            myAnim=GetComponent<Animation>();
        }

        if(_play)
        {
            preTime+=Time.deltaTime;
            if(preTime>myAnim.clip.length)
                _play=false;
        }

        
        if(AnimationMode.InAnimationMode())
        {
            AnimationMode.BeginSampling();
            AnimationMode.SampleAnimationClip(this.gameObject, myAnim.clip, preTime);
            AnimationMode.EndSampling();
            SceneView.RepaintAll();
        } 
         
       
         
        if(BindingTrans!=null)
        {
            foreach(transformSync tS in BindingTrans)
            {
                if(tS.Target!=null && tS.Dummy!=null)
                {
                    tS.Target.position=tS.Dummy.position;
                    tS.Target.rotation=tS.Dummy.rotation;
                    tS.Target.localScale=tS.Dummy.lossyScale;
                }
            }
        }

        
    }

    public void Play()
    {
        if(myAnim!=null)
        {
            
            preTime=0.0f;
            if(_play)
            {
                _play=false;
                AnimationMode.StopAnimationMode();
                
            }
            else
            {
                AnimationMode.StartAnimationMode();
                _play=true;
                
                
            }
        }
    }

    public void Pause()
    {
        if(myAnim!=null)
        {
            if(_play)
            {
                _play=false;
                //AnimationMode.StopAnimationMode();
            }
            else
            {
                AnimationMode.StartAnimationMode();
                _play=true;
            }
             
        }
    }
    
#endif
}