using System;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
using System.Collections.Generic;
using UnityEngine.Playables;  
using UnityEngine.Timeline;



[ExecuteInEditMode]
public class TimelineGizmos : MonoBehaviour
{
#if UNITY_EDITOR

    public PlayableDirector Dir = new PlayableDirector();
    public double CurrentTime=0;
    public bool IsON=false;
    public List<Vector3> positionList=new List<Vector3>();
    public List<Vector3> KeyPosList=new List<Vector3>();
    public Transform trans =null;
    public AnimationTrack ATrack =null;
    //public Transform T;
    //public PlayableDirector dir ;
    public float pointDensity=100;
    public float startTime=0;
    public float endTime=10;
    public List<float> times=new List<float>();
    public EditorCurveBinding[] curvesBinding;
    public AnimationClip AC=null;
    public Color FrameColor=new Color(1,1,1,1);
    public Color KeyColor=new Color(1,0,0,1);
    public Color LineColor=new Color(1,0,0,1);

    void Start()
    {
        
    }
    void OnDrawGizmos()
    {
        // if(IsON)
        // {
        //     CurrentTime=Dir.time;
        //     DrawMyTrajectories(Dir);
        // }
        //
        Gizmos.color = FrameColor;

        for(int i=0;i<positionList.Count;i++)
        {
            Vector3 cameraPos=Camera.current.transform.position;
            Vector3 GizmoCubePos=positionList[i];
            float Cube2CamDistance=Vector3.Distance(cameraPos,GizmoCubePos);

            Gizmos.DrawWireCube (positionList[i], new Vector3(.005f,.005f,.005f) *(Cube2CamDistance));

        }
        Gizmos.color = LineColor;
        for(int i=0;i<positionList.Count-1;i++)
        {
            

            Gizmos.DrawLine(positionList[i],positionList[Math.Min(i+1,positionList.Count-1)]);
        }
        Gizmos.color = KeyColor;
        for(int i=0;i<KeyPosList.Count;i++)
        {
            Vector3 cameraPos=Camera.current.transform.position;
            Vector3 GizmoCubePos=positionList[i];
            float Cube2CamDistance=Vector3.Distance(cameraPos,GizmoCubePos);

            Gizmos.DrawWireCube (KeyPosList[i], new Vector3(.02f,.02f,.02f) *(Cube2CamDistance));

        }
    }
    public void CollectObjTrajectory(PlayableDirector D )
    {
            
            positionList.Clear();
            
            for(int i=0;i<(float)D.duration*pointDensity;i++ )
            {
                if(i>=startTime*pointDensity && i<=endTime*pointDensity)
                {
                    D.time=i*(1/pointDensity);
                    D.Evaluate();
                    positionList.Add(trans.position);
                    //Gizmos.DrawCube(Selection.transforms[0].position,new Vector3(0.1f,0.1f,0.1f));
                }
                
            }
            D.time=CurrentTime;
            D.Evaluate();
    }

    public void  CollectKeysPos(Transform T )
    {
        AnimationCurve ACurve;
        times.Clear();
        KeyPosList.Clear();
        foreach(EditorCurveBinding ecb in curvesBinding) 
        {

            String path =  ecb.path;
            Transform t =T.Find(ecb.path);
            // AnimationCurve ACurve = AnimationUtility.GetEditorCurve(AC,ecb);
            // tlGizmos.CollectKeysPos(ACurve,T);
            if(t==trans)
            {
                //UnityEngine.Debug.Log(true);
                if(ecb.propertyName.Contains("m_LocalPosition"))
                {
                    ACurve = AnimationUtility.GetEditorCurve(AC,ecb);
                    Keyframe[] keys =ACurve.keys;
                    //tlGizmos.CollectKeysPos(ACurve,T);
                    for(int i=0;i<keys.Length;i++)
                    {

                        if(keys[i].time>=startTime && keys[i].time<=endTime)
                        {
                            bool IsSameTime=false;
                            float keyTime= keys[i].time;
                            for(int j=0;j<times.Count;j++)
                            {
                                if(times[j]==keyTime) IsSameTime=true;
                            }
                            if(IsSameTime)
                            {
                                break;
                            }
                            else
                            {
                                times.Add(keyTime);
                                Dir.time=keyTime;
                                Dir.Evaluate();
                                KeyPosList.Add(trans.position);
                            }
                        }

                    }
                }
            }
        }
        
    }



#endif
}
