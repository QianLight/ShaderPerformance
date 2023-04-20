
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using UnityEngine.Playables;  
using UnityEngine.Timeline;
using UnityEditor.Timeline;
using System.Reflection;
using System;
using System.IO;
using CFEngine.Editor;


// public class CurveData 
// {
//     public String path="";
//     public String name="";
//     public Transform transform=null;
//     public AnimationCurve curve=new AnimationCurve();
// }

class AnimTexParam
{
    public string name;
    public TextureImporterShape textureImporterShape;
    public bool isReadable;

    public TextureImporterPlatformSettings AndroidSettings;

    public TextureImporterPlatformSettings IOSSettings;

    public TextureImporterPlatformSettings PCSettings;


}
 
public class Art_AnimationTools : ArtToolsTemplate
{
   // private PlayableDirector qPlayableDirector;


//[MenuItem("tools/test")]
    // static void Test()
    // {
       
    // }

    // public class CurveData 
    // {
    //     public String path="";
    //     public String name="";
    //     public Transform transform=null;
    //     public AnimationCurve curve=new AnimationCurve();
    // }

    PlayableDirector dir ;
    Transform T;
    // int current=0;
    GameObject GO = null;
    TimelineGizmos tlGizmos;
    bool onEdit=false;
    AnimationTrack ATrack =null;
    //List<float> times=new List<float>();
    float startFrame=0;
    float endFrame=100;
    float pointDensity=100;
    public Color FrameColor=new Color(1,1,1,1);
    public Color KeyColor=new Color(1,0,0,1);
    public Color LineColor=new Color(1,0,0,1);
    private GameObject bandposeGO=null;
    //private Transform TPoseTrans=null;
    //private Transform RefTrans=null;
    //private Vector3Int trs =new Vector3Int();
    //private Vector3 offsetRot =new Vector3();

    private AnimationClip brokeAnim=null;
   // private int vertID=0;
    //private int BoneID=0;
    //private Color VC =Color.black;
    SkinnedMeshRenderer SMR =null;
    private int bonesCount=0;
    private int framesCount=0;
    private Mesh M =null;
   // private bool IsExr=false;
    //private List<Vector3> listBonePos=new List<Vector3>();
    //private Transform pivotT=null;
    private String BakeBrokenDataPath = "";
   // private Matrix4x4 matr =new Matrix4x4();
    //private Matrix4x4 matr1 =new Matrix4x4();
    private static AnimTexParam animTexParam=new AnimTexParam();



    enum FrameRateType
    {
        Seconds=1,
        Frame_30=30,
        Frame_60=60
    }
    FrameRateType frameRateType=FrameRateType.Frame_30;





    public override void OnGUI() 
    {
        EditorGUILayout.Space();

        // GUILayout.BeginHorizontal();
        // GUILayout.Label("播放动画");

        // if (GUILayout.Button("播放动画", GUILayout.MaxWidth(200)))
        // {
        //     playSelectionAnim();
        // }
        // GUILayout.EndHorizontal();


        // GUILayout.Label ("-------------------------------Timeline旋转角度修复------------------------------------------");


        // if (GUILayout.Button("修复", GUILayout.MaxWidth(100)))
        // {
        //     curveData.Clear();
        //     AnimationTrack aaa=(AnimationTrack)Selection.activeObject;
        //     dir = TimelineEditor.inspectedDirector;
        //     IEnumerable<TimelineClip> IEnumerableAC =aaa.GetClips();
        //     TimelineAsset TA = aaa.timelineAsset;
        //     Animator bind = (Animator)dir.GetGenericBinding(aaa);
        //     // dir.time=25;
        //     T =bind.transform;
        //     AnimationClip AC =aaa.infiniteClip;
        //     // dir.Evaluate();
        //     //AC.SampleAnimation(T.gameObject,20f);
        //     //dir.Evaluate();
        //     AnimationClipData ACData =new AnimationClipData();
        //     EditorCurveBinding[] curvesBinding= AnimationUtility.GetCurveBindings(AC);
        //     foreach(EditorCurveBinding ecb in curvesBinding) 
        //     {
        //         String CurveName=ecb.propertyName;
                
        //         if(CurveName.Contains("localEulerAnglesRaw"))
        //         {
        //             Transform trans = T.Find(ecb.path);
        //             String n =ecb.path+"/Rota."+CurveName.Split('.')[1];
        //             CurveData cd =new CurveData();
        //             cd.path=ecb.path;
        //             cd.name=n;
        //             cd.transform=trans;
        //             cd.curve=AnimationUtility.GetEditorCurve(AC,ecb);
        //             curveData.Add(cd);
        //         }
        //     }
        // }

        // for(int i=0 ; i<curveData.Count ; i++) 
        // {
        //     EditorGUILayout.BeginHorizontal();
        //     if (GUILayout.Button(curveData[i].name, GUILayout.MaxWidth(200)))
        //     {
        //         Keyframe[] keys = curveData[i].curve.keys;
        //         for(int j=0 ; j<keys.Length; j++)
        //         {
        //             dir.time= keys[j].time;
        //             dir.Evaluate();
        //             switch(Path.GetFileName(curveData[i].name))
        //             {
        //                 case "Rota.x":
        //                     SetRotationLikeInspector(curveData[i].transform,new Vector3(keys[j].value,0,0) );
        //                     UnityEngine.Debug.Log(keys[j].value);
        //                     break;
        //                 case "Rota.y":
        //                     UnityEngine.Debug.Log(curveData[i].name);
        //                     break;
        //                 case "Rota.z":
        //                     UnityEngine.Debug.Log(curveData[i].name);
        //                     break;
        //             }
        //             dir.Evaluate();
        //         }
        //     }


        //     Keyframe[] keys1 = curveData[i].curve.keys;

        //     if (GUILayout.Button(">|", GUILayout.MaxWidth(50)))
        //     {
                

        //             dir.time= keys1[current].time;
        //             dir.Evaluate();
        //             switch(Path.GetFileName(curveData[i].name))
        //             {
        //                 case "Rota.x":
        //                     SetRotationLikeInspector(curveData[i].transform,new Vector3(keys1[current].value,0,0) );
        //                     UnityEngine.Debug.Log(keys1[current].value);
        //                     break;
        //                 case "Rota.y":
        //                     UnityEngine.Debug.Log(curveData[i].name);
        //                     break;
        //                 case "Rota.z":
        //                     UnityEngine.Debug.Log(curveData[i].name);
        //                     break;
        //             }
        //             dir.Evaluate();
        //         current++;
        //         if(current>=keys1.Length) current=0;
        //     }
        //     EditorGUILayout.EndHorizontal();
        // }
        
        GUILayout.Label ("-------------------------------Trajectories------------------------------------------");

// bool isGUIChange=false;

        frameRateType = (FrameRateType) EditorGUILayout.EnumPopup (frameRateType, GUILayout.Width (200));

        startFrame*=(float)frameRateType;
        endFrame*=(float)frameRateType;
        pointDensity/=(float)frameRateType;

        //EditorGUI.BeginChangeCheck();
        startFrame=EditorGUILayout.FloatField("startTime",startFrame);
        endFrame=EditorGUILayout.FloatField("endTime",endFrame);
        pointDensity=EditorGUILayout.FloatField("pointDensity",pointDensity);
        FrameColor=EditorGUILayout.ColorField("FrameColor",FrameColor);
        LineColor=EditorGUILayout.ColorField("LineColor",LineColor);
        KeyColor=EditorGUILayout.ColorField("KeyColor",KeyColor);

        startFrame/=(float)frameRateType;
        endFrame/=(float)frameRateType;
        pointDensity*=(float)frameRateType;
        // tlGizmos.FrameColor=FrameColor;
        // tlGizmos.LineColor=LineColor;
        // tlGizmos.KeyColor=KeyColor;

        //if(EditorGUI.EndChangeCheck()) isGUIChange=true;

        if(!onEdit)
        {
            if (GUILayout.Button("创建轨迹", GUILayout.MaxWidth(100)))
            {
                if(Selection.transforms[0]!=null)
                {
                    dir = TimelineEditor.inspectedDirector;

                    if(dir!=null)
                    {
                            //tlGizmos.IsON=true;
                        GameObject.DestroyImmediate(GO);
                        GO=new GameObject(Selection.transforms[0].name+"trajectory") ;
                        tlGizmos=GO.AddComponent<TimelineGizmos>();
                        
                        tlGizmos.trans=Selection.transforms[0];
                        tlGizmos.Dir=dir;
                        

                        tlGizmos.FrameColor=FrameColor;
                        tlGizmos.LineColor=LineColor;
                        tlGizmos.KeyColor=KeyColor;
                        tlGizmos.CurrentTime=dir.time;
                        tlGizmos.startTime=startFrame;
                        tlGizmos.endTime=endFrame;
                        tlGizmos.pointDensity=pointDensity;
                        tlGizmos.CollectObjTrajectory(tlGizmos.Dir);
                        onEdit=true;
                    }
                    else
                    {
                        UnityEditor.EditorUtility.DisplayDialog("提示","需要激活Timeline","确定");
                    }
                    
                }
                //UnityEngine.Debug.Log(Selection.activeObject.GetType());
                 
            }

            
           
        }
        else
        {
            if (GUILayout.Button("刷新轨迹", GUILayout.MaxWidth(100)))
            {
                if(dir!=null)
                {
                    tlGizmos.FrameColor=FrameColor;
                    tlGizmos.LineColor=LineColor;
                    tlGizmos.KeyColor=KeyColor;
                    tlGizmos.CurrentTime=dir.time;
                    tlGizmos.startTime=startFrame;
                    tlGizmos.endTime=endFrame;
                    tlGizmos.pointDensity=pointDensity;
                    tlGizmos.CollectObjTrajectory(tlGizmos.Dir);
                }
                else
                {
                    UnityEditor.EditorUtility.DisplayDialog("提示","需要激活Timeline","确定");
                }
            
            }





            try
            {
                ATrack=(AnimationTrack)Selection.activeObject;
                if(ATrack!=null)
                {
                    Animator bind = (Animator)dir.GetGenericBinding(ATrack);
                    T =bind.transform;
                    //UnityEngine.Debug.Log(T.Find("L_Arm_3"));
                    //Transform CurrentTrans=T.Find("root/Sphere");
                    Transform CurrentTrans=tlGizmos.trans;
                    tlGizmos.AC =ATrack.infiniteClip;
                    tlGizmos.curvesBinding= AnimationUtility.GetCurveBindings(tlGizmos.AC);
                    if(tlGizmos.AC!=null)
                    {
                        tlGizmos.CollectKeysPos(T);
                        // times.Clear();
                        // tlGizmos.KeyPosList.Clear();
                        // foreach(EditorCurveBinding ecb in tlGizmos.curvesBinding) 
                        // {

                        //     String path =  ecb.path;
                        //     Transform t =T.Find(ecb.path);
                        //     // AnimationCurve ACurve = AnimationUtility.GetEditorCurve(AC,ecb);
                        //     // tlGizmos.CollectKeysPos(ACurve,T);
                        //     if(t==tlGizmos.trans)
                        //     {
                        //         UnityEngine.Debug.Log(true);
                        //         if(ecb.propertyName.Contains("m_LocalPosition"))
                        //         {
                        //             AnimationCurve ACurve = AnimationUtility.GetEditorCurve(AC,ecb);
                        //             Keyframe[] keys =ACurve.keys;
                        //             //tlGizmos.CollectKeysPos(ACurve,T);
                        //             for(int i=0;i<keys.Length;i++)
                        //             {

                        //                 if(keys[i].time>=startTime && keys[i].time<=endTime)
                        //                 {
                        //                     bool IsSameTime=false;
                        //                     float keyTime= keys[i].time;
                        //                     for(int j=0;j<times.Count;j++)
                        //                     {
                        //                         if(times[j]==keyTime) IsSameTime=true;
                        //                     }
                        //                     if(IsSameTime)
                        //                     {
                        //                         break;
                        //                     }
                        //                     else
                        //                     {
                        //                         times.Add(keyTime);
                        //                         dir.time=keyTime;
                        //                         dir.Evaluate();
                        //                         tlGizmos.KeyPosList.Add(tlGizmos.trans.position);
                        //                     }
                        //                 }
                                        

                                        
                        //             }
                        //         }
                        //     }
                        //     UnityEngine.Debug.Log(ecb.path+"-----"+ecb.propertyName);
                        // }
                        



                        //tlGizmos.CollectKeysPos(curvesBinding,T);
                        // EditorCurveBinding[] curvesBinding= AnimationUtility.GetCurveBindings(AC);
                        // foreach(EditorCurveBinding ecb in curvesBinding) 
                        // {
                        //     String path =  ecb.path;
                        //     Transform t =T.Find(ecb.path);
                        //     if(t==tlGizmos.trans)
                        //     {
                        //         UnityEngine.Debug.Log(true);
                        //         if(ecb.propertyName.Contains("m_LocalPosition"))
                        //         {

                        //         }
                        //     }
                        //     UnityEngine.Debug.Log(ecb.path+"-----"+ecb.propertyName);
                        // }
                    }
                }
            }
            catch
            {
                UnityEngine.Debug.Log(11111);
            }

            if (GUILayout.Button("清除轨迹", GUILayout.MaxWidth(100)))
            {

                if(tlGizmos!=null)
                {
                    tlGizmos.IsON=false;
                }
                


                // for(int i=0 ; i<10;i++)
                // {
                //     dir.time=i;
                //     dir.Evaluate();
                //     UnityEngine.Debug.Log(ttt.position);
                // }
                GameObject.DestroyImmediate(GO);
                onEdit=false;
            }
        }



        // EditorGUILayout.Space();
        // GUILayout.Label ("-------------------------------BakeBrokenData------------------------------------------");

        // bandposeGO= (GameObject) EditorGUILayout.ObjectField ("bandposeGO",bandposeGO, typeof (GameObject), true, GUILayout.MaxWidth (300));
        // brokeAnim= (AnimationClip) EditorGUILayout.ObjectField ("brokeAnim",brokeAnim, typeof (AnimationClip), true, GUILayout.MaxWidth (300));
        // vertID = (int) EditorGUILayout.IntField ("vertID",vertID, GUILayout.MaxWidth (300));
        // BoneID = (int) EditorGUILayout.IntField ("BoneID",BoneID, GUILayout.MaxWidth (300));
        // VC=(Color) EditorGUILayout.ColorField ("VC",VC, GUILayout.MaxWidth (300));
        // IsExr=EditorGUILayout.Toggle("isExr",IsExr);
        // pivotT= (Transform) EditorGUILayout.ObjectField ("pivotT",pivotT, typeof (Transform), true, GUILayout.MaxWidth (300));

        // if (GUILayout.Button("Test", GUILayout.MaxWidth(100)))
        // {
        //     SMR = bandposeGO.GetComponentInChildren<SkinnedMeshRenderer>();
        //     Animator animator = bandposeGO.GetComponent<Animator>();
        //     Avatar avatar = animator.avatar;

        //     if(SMR!=null)
        //     {
        //         if(M==null)
        //         {
        //             M = Mesh.Instantiate(SMR.sharedMesh) ;
        //         }
        //         Transform pivotTrans=pivotT;
        //         SMR.sharedMesh=M;
        //         List<BoneWeight> listBW = new List<BoneWeight>();
        //         Transform[] bones =  SMR.bones;
        //         Vector3 offsetDir=Vector3.Normalize(bones[BoneID].position-bandposeGO.transform.position);
        //         bones[BoneID].position += new Vector3(offsetDir.x,0,offsetDir.y) *0.2f;
        //         BoneWeight[] boneWeights = M.boneWeights;
        //         Color[] colors=M.colors;
        //         List<Color> ListVC=new List<Color>();


        //         List<int> ListVertsID=new List<int>();
        //         List<List<int>> LListVertsID =new List<List<int>>();
                
        //         for( int i=0 ; i<bones.Length ; i++ )
        //         {
        //             List<int> LVertsID=new List<int>();
        //             LListVertsID.Add(LVertsID);
                    
        //         }

        //         for(int i=0;i<boneWeights.Length;i++)
        //         {
        //             int boneID0=M.boneWeights[i].boneIndex0;
        //             LListVertsID[boneID0].Add(i);
        //             ListVC.Add(Color.black);
        //         }

        //         for( int i=0 ; i<LListVertsID.Count ; i++ )
        //         {
        //             for(int j=0;j<LListVertsID[i].Count;j++)
        //             {
        //                 Color col=new Color();
        //                 col.a =(float)i/32;
        //                 ListVC[LListVertsID[i][j]]=col;
        //             }
        //         }
                    
        //         M.SetColors(ListVC);
                    
        //         Mesh bakedMesh= new Mesh();
        //         SMR.BakeMesh(bakedMesh);
        //         AssetDatabase.CreateAsset(bakedMesh,BakeBrokenDataPath+"mmm.asset");

        //         int texWidth=512;
        //         int texHeight=32;

        //         Texture2D AnimTex =null;
        //         if(IsExr)
        //         {
        //                 AnimTex = new Texture2D(texWidth, texHeight, TextureFormat.RGBAFloat, false);
        //         }
        //         else
        //         {
        //                 AnimTex = new Texture2D(texWidth, texHeight, TextureFormat.RGBA32, false);
        //         }
                
            
        //         EditorCurveBinding[] editorCurveBindings= AnimationUtility.GetCurveBindings(brokeAnim);
        //         EditorCurveBinding[] editorCurveBindings1= AnimationUtility.GetObjectReferenceCurveBindings(brokeAnim);
        //         AnimationCurve animationCurve= AnimationUtility.GetEditorCurve(brokeAnim,editorCurveBindings[0]);
        //         Keyframe[] keys = animationCurve.keys;
        //         float timeLength= brokeAnim.length;
        //         int frameCount =Mathf.FloorToInt(timeLength*30);
        //         Debug.Log(brokeAnim.length);
        //         List<Matrix4x4> ListMarix =new List<Matrix4x4>();


        //         Color[] texPix = new Color[512*32];
                
        //         List<Vector3> startPosList   = new List<Vector3>();
        //         List<Vector3> startEulerList = new List<Vector3>();
        //         List<Vector3> startScaleList = new List<Vector3>();
        //         List<Vector3> pivotPosList   = new List<Vector3>();

        //         for(int j=0 ; j<(int)( texWidth*0.25f) ; j++)
        //         {
        //             brokeAnim.SampleAnimation(bandposeGO,((float)j)/30);
                    
        //             listBonePos.Add(bones[5].localPosition);
                    
        //             for(int i=0 ; i< texHeight ; i++)
        //             {
        //                 if(i<bones.Length)
        //                 {
        //                     if(j==0)
        //                     {
        //                         startPosList.Add(bones[i].localPosition);
        //                         startEulerList.Add(bones[i].localEulerAngles);
        //                         // startScale=bones[i].localScale;
        //                         pivotPosList.Add(bones[i].localPosition-pivotTrans.localPosition);
        //                     }

        //                     Vector3 pos=bones[i].localPosition-startPosList[i];

        //                     // if(j==4)
        //                     // {
        //                     //     Debug.Log(bones[i].name+"       "+startPos+"      "+pos+"       "+bones[i].localPosition);
        //                     // }
                            
                
        //                     // Vector3 rotEuler =Quaternion.Euler (bones[i].localEulerAngles-startEuler);
        //                     Quaternion rot = Quaternion.Euler (bones[i].localEulerAngles-startEulerList[i]);
        //                     Vector3 scale= bones[i].localScale;
        //                     Matrix4x4 marix = Matrix4x4.TRS(pos,rot,scale);


        //                     pos= new Vector3(marix.m03,marix.m13,marix.m23)*0.1f*0.5f+new Vector3(0.5f,0.5f,0.5f);
        //                     Vector3 pivot =pivotPosList[i]*0.1f*0.5f+new Vector3(0.5f,0.5f,0.5f);
                            
        //                     Vector2 encodePosX= EncodeFloatRG(pos.x);
        //                     Vector2 encodePosY= EncodeFloatRG(pos.y);
        //                     Vector2 encodePosZ= EncodeFloatRG(pos.z);

        //                     texPix[i*texWidth+j*4]  = new Color(marix.m00*0.5f+0.5f,marix.m01*0.5f+0.5f,marix.m02*0.5f+0.5f,encodePosX.x);
        //                     texPix[i*texWidth+j*4+1]= new Color(marix.m10*0.5f+0.5f,marix.m11*0.5f+0.5f,marix.m12*0.5f+0.5f,encodePosY.x);
        //                     texPix[i*texWidth+j*4+2]= new Color(marix.m20*0.5f+0.5f,marix.m21*0.5f+0.5f,marix.m22*0.5f+0.5f,encodePosZ.x);
        //                     texPix[i*texWidth+j*4+3]= new Color(encodePosX.y ,encodePosY.y ,encodePosZ.y ,1);

        //                     Vector2 encodePivotX= EncodeFloatRG(pivot.x);
        //                     Vector2 encodePivotY= EncodeFloatRG(pivot.y);
        //                     Vector2 encodePivotZ= EncodeFloatRG(pivot.z);

        //                     if(j==(int)( texWidth*0.25f)-1)
        //                     {
        //                         texPix[i*texWidth+j*4]  = new Color(encodePivotX.x,encodePivotY.x,encodePivotZ.x,1);
        //                         texPix[i*texWidth+j*4+1]= new Color(encodePivotX.y,encodePivotY.y,encodePivotZ.y,1);
        //                         texPix[i*texWidth+j*4+2]= new Color(0,0,0,0);
        //                         texPix[i*texWidth+j*4+3]= new Color(0,0,0,0);
        //                     }
        //                 }
        //                 else
        //                 { 
        //                     texPix[i*texWidth+j*4]  = Color.black;
        //                     texPix[i*texWidth+j*4+1]= Color.black;
        //                     texPix[i*texWidth+j*4+2]= Color.black;
        //                     texPix[i*texWidth+j*4+3]= Color.black;
        //                 }
        //             }
        //         }
        //         brokeAnim.SampleAnimation(bandposeGO,0);

        //         AnimTex.SetPixels(texPix);
        //         AnimTex.Apply();
        //         String path="";
        //         byte[] bytes=null;

        //         if(IsExr)
        //         {
        //             path=BakeBrokenDataPath+"aa.exr";
        //             bytes = AnimTex.EncodeToEXR();
        //         }
        //         else
        //         {
        //             path=BakeBrokenDataPath+"aa.png";
        //             bytes = AnimTex.EncodeToPNG();
        //         }
        //         File.WriteAllBytes(path, bytes);
        //         AssetDatabase.ImportAsset(path);
        //         AssetDatabase.Refresh();
        //     }

            
        // }

        // if (GUILayout.Button("bakeMesh", GUILayout.MaxWidth(100)))
        // {
        //     List<Vector3> newPos=new List<Vector3>();
        //     if(SMR!=null)
        //     {
        //         Mesh bakedMesh= new Mesh();
        //         SMR.BakeMesh(bakedMesh);
        //             Vector3[] verts = bakedMesh.vertices;
        //         for(int i=0;i<verts.Length;i++)
        //         {
        //                 Vector3 p = verts[i];
        //             //p=RotateRound(p,pivotT.position,new Vector3(1,0,0),-90);
        //             Matrix4x4 mart =Matrix4x4.Rotate(Quaternion.Euler(-90,0,0));
        //             Vector4 rotaVert =mart * (p);
        //             Vector3 newP=new Vector3(rotaVert.x,rotaVert.y,rotaVert.z);
        //             //newP=p+new Vector3(1,1,1);
        //             newPos.Add(p);
        //         }
        //         bakedMesh.SetVertices(newPos);
        //         // // Bounds bounds=new Bounds();
        //         // // bounds.center=pivotT.position;
        //         // // bakedMesh.bounds=bounds;
        //         AssetDatabase.CreateAsset(bakedMesh,BakeBrokenDataPath+"mmm.asset");
        //     }
        // }








        
        EditorGUILayout.Space();
        GUILayout.Label ("-------------------------------BakeGPUAnimData------------------------------------------");

        bandposeGO= (GameObject) EditorGUILayout.ObjectField ("bandposeGO",bandposeGO, typeof (GameObject), true, GUILayout.MaxWidth (300));
        brokeAnim= (AnimationClip) EditorGUILayout.ObjectField ("brokeAnim",brokeAnim, typeof (AnimationClip), true, GUILayout.MaxWidth (300));
        //vertID = (int) EditorGUILayout.IntField ("vertID",vertID, GUILayout.MaxWidth (300));
        //BoneID = (int) EditorGUILayout.IntField ("BoneID",BoneID, GUILayout.MaxWidth (300));
        //VC=(Color) EditorGUILayout.ColorField ("VC",VC, GUILayout.MaxWidth (300));
       // IsExr=EditorGUILayout.Toggle("isExr",IsExr);
        //pivotT= (Transform) EditorGUILayout.ObjectField ("pivotT",pivotT, typeof (Transform), true, GUILayout.MaxWidth (300));
       // BakeBrokenDataPath=EditorGUILayout.TextField("path",BakeBrokenDataPath);
        if (GUILayout.Button("BakeData", GUILayout.MaxWidth(100)))
        {
            GameObject TempGO = GameObject.Instantiate(bandposeGO);
            TempGO.transform.position=Vector3.zero;
            SMR = TempGO.GetComponentInChildren<SkinnedMeshRenderer>();
            if(SMR!=null)
            {
                M = SMR.sharedMesh;
                if(M!=null)
                {
                    float maxBoneCount=256;
                    //Transform pivotTrans=pivotT;
                    Transform[] bones =  SMR.bones;
                // Vector3 offsetDir = Vector3.Normalize(bones[BoneID].position-TempGO.transform.position);
                    float timeLength= brokeAnim.length;
                    framesCount =Mathf.FloorToInt(timeLength*30);
                    bonesCount = bones.Length;
                    Matrix4x4 W2O_matr = SMR.transform.worldToLocalMatrix;
                    Vector3 offsetPos = W2O_matr*(SMR.transform.position);
                    Matrix4x4 R90Matr =Matrix4x4.Rotate (Quaternion.Euler(new Vector3(-90,0,0)) );

                    BoneWeight[] boneWeights = M.boneWeights;

                    Color[] colors = M.colors;

                    #region Calculate Outline Vectors and copy into color

                    Mesh skinnedMesh = SMR.sharedMesh;
                    Vector3[] outlineVectors = BandposeData.CalculateOutlineVectors(
                        skinnedMesh.vertexCount, skinnedMesh.vertices, skinnedMesh.triangles, 
                        skinnedMesh.tangents, skinnedMesh.normals, skinnedMesh.uv3, skinnedMesh.colors);

                    if (colors != null)
                    {
                        var tangents = M.tangents;
                        var nors = M.normals;
                        
                        List<Vector4> uv2_v4 = new List<Vector4>();

                        Vector2[] uv2 = M.uv2.Length == colors.Length?M.uv2 : null;
                        Vector2[] uv3 = M.uv3.Length == colors.Length?M.uv3 : null;
                        Vector2[] uv4 = M.uv4.Length == colors.Length?M.uv4 : null;
                        for (int j = 0; j < colors.Length; ++j)
                        {
                            ref var t = ref tangents[j]; 
                            ref var n = ref nors[j];
                            ref var color = ref colors[j];

                            Vector3 tangent = Vector3.Normalize (new Vector3 (t.x, t.y, t.z));
                            Vector3 normal = Vector3.Normalize (n);
                            Vector3 binormal = Vector3.Cross (normal, tangent) * t.w;
                            Matrix4x4 matr = new Matrix4x4 ();
                            matr.m00 = tangent.x;
                            matr.m01 = tangent.y;
                            matr.m02 = tangent.z;
                            matr.m03 = 0;
                            matr.m10 = binormal.x;
                            matr.m11 = binormal.y;
                            matr.m12 = binormal.z;
                            matr.m13 = 0;
                            matr.m20 = normal.x;
                            matr.m21 = normal.y;
                            matr.m22 = normal.z;
                            matr.m23 = 0;
                            matr.m30 = 0;
                            matr.m31 = 0;
                            matr.m32 = 0;
                            matr.m33 = 1;
                            Vector4 c = outlineVectors[j];
                            c = matr * c;
                            c.w = 0; // Height Gradient

                            if (uv4 != null)
                            {
                                Vector4 v4 = new Vector4(uv2[j].x,uv2[j].y,uv4[j].x,1);
                                uv2_v4.Add(v4);
                            }

                            colors[j]= c;
                        }
                    }

                    #endregion    

                    #region Make BakeMesh
                    List<Color> ListVC=new List<Color>();
                    List<int> ListVertsID=new List<int>();
                    List<List<int>> LListVertsID =new List<List<int>>();
                    List<Vector4> ListUV1=new List<Vector4>();
                    List<Vector4> ListWeight=new List<Vector4>();
                    
                    for( int i=0 ; i<bones.Length ; i++ )
                    {
                        List<int> LVertsID=new List<int>();
                        LListVertsID.Add(LVertsID);
                    }

                    for(int i=0;i<boneWeights.Length;i++)
                    {
                        // int boneID0=boneWeights[i].boneIndex0;
                        // LListVertsID[boneID0].Add(i);
                        // ListVC.Add(Color.black);
                        float boneID_0=(float)boneWeights[i].boneIndex0/maxBoneCount;
                        float boneID_1=(float)boneWeights[i].boneIndex1/maxBoneCount;
                        float boneWeight=(float)boneWeights[i].weight0;
                        float none=0;
                        Vector4 data=new Vector4 (boneID_0,boneID_1,boneWeight,none);
                        ListUV1.Add(data);

                        // float weight_0=boneWeights[i].weight0;
                        // float weight_1=boneWeights[i].weight1;
                        // float weight_2=boneWeights[i].weight2;
                        // float weight_3=boneWeights[i].weight3;
                        // Vector4 weight=new Vector4 (weight_0,weight_1,weight_2,weight_3);
                        // ListWeight.Add(weight);
                    }

                    // for( int i=0 ; i<LListVertsID.Count ; i++ )
                    // {
                    //     for(int j=0;j<LListVertsID[i].Count;j++)
                    //     {
                    //         Color col=new Color();
                    //         col.a =(float)i/bones.Length; 
                    //         ListVC[LListVertsID[i][j]]=col;
                    //     }
                    // }

                    //brokeAnim.SampleAnimation(TempGO,0);
                    //Mesh MInst= Mesh.Instantiate(M) ;
                    //SMR.sharedMesh=MInst;
                    Mesh bakeM = new Mesh();

                    SMR.BakeMesh(bakeM);

                    float x_min=0;
                    float x_max=0;
                    float y_min=0;
                    float y_max=0;
                    float z_min=0;
                    float z_max=0;

                    Vector3[] verts=bakeM.vertices;
                    Vector3[] normals=bakeM.normals;
                    for(int i=0 ; i<verts.Length ; i++)
                    {
                        verts[i]=verts[i]+ offsetPos;
                        verts[i]=R90Matr*verts[i];
                        normals[i]=R90Matr*normals[i];
                    }
                                        
                    //bakeM.SetColors(ListVC);  
                    bakeM.colors=colors;
                    bakeM.SetUVs(1,ListUV1);
                    //bakeM.SetUVs(2,ListWeight);
                    bakeM.vertices=verts;
                    bakeM.normals=normals;
                    #endregion

                    #region Make Texture
                    int texWidth=(int)Mathf.Pow(2,Mathf.CeilToInt(Mathf.Log(framesCount*4,2)));
                    int texHeight=(int)Mathf.Pow(2,Mathf.CeilToInt(Mathf.Log(bones.Length,2)));

                    Texture2D AnimTex =null;
                    // if(IsExr)
                    // {
                    //         AnimTex = new Texture2D(texWidth, texHeight, TextureFormat.RGBAFloat, false);
                    // }
                    // else
                    // {
                            AnimTex = new Texture2D(texWidth, texHeight, TextureFormat.RGBA32, false);
                    // }
                    
                
                    EditorCurveBinding[] editorCurveBindings= AnimationUtility.GetCurveBindings(brokeAnim);
                    EditorCurveBinding[] editorCurveBindings1= AnimationUtility.GetObjectReferenceCurveBindings(brokeAnim);
                    AnimationCurve animationCurve= AnimationUtility.GetEditorCurve(brokeAnim,editorCurveBindings[0]);
                    Keyframe[] keys = animationCurve.keys;
                    
                    //Debug.Log(brokeAnim.length);
                    List<Matrix4x4> ListMarix =new List<Matrix4x4>();


                    Color[] texPix = new Color[texWidth*texHeight];
                    
                    List<Vector3> startPosList   = new List<Vector3>();
                    List<Vector3> startEulerList = new List<Vector3>();
                    List<Vector3> startScaleList = new List<Vector3>();
                    List<Quaternion> startRotList = new List<Quaternion>();
                    //List<Vector3> pivotPosList   = new List<Vector3>();

                    // List<Vector3> TPosePosList   = new List<Vector3>();
                    // List<Vector3> TPoseEulerList = new List<Vector3>();
                    // List<Vector3> TPoseScaleList = new List<Vector3>();


                    //List<Vector3> pivotPosList   = new List<Vector3>();
                    

                    
                    for(int i=0 ;i<bones.Length ;i++)
                    {
                        startPosList.Add (bones[i].position);
                        startEulerList.Add (bones[i].rotation.eulerAngles);
                        //.Add (bones[i].position-pivotTrans.position);
                        startRotList.Add (bones[i].rotation);
                        startScaleList.Add (bones[i].lossyScale);
                        //Debug.Log(i+"~~~~~~~"+bones[i].name+"~~~~~~~~"+(bones[i].position-pivotTrans.position));
                    }
                                    
                    

                    for(int j=0 ; j<(int)( texWidth*0.25f) ; j++)
                    {
                        brokeAnim.SampleAnimation(TempGO,((float)j)/30);

                        Mesh m =new Mesh();
                        SMR.BakeMesh(m);

                        Vector3[] tempVerts= m.vertices;
                        for(int k=0;k<tempVerts.Length;k++)
                        {

                            tempVerts[k]=tempVerts[k]+ offsetPos;
                            tempVerts[k]=R90Matr*tempVerts[k];
                            x_min= Mathf.Min (tempVerts[k].x ,x_min);
                            x_max= Mathf.Max (tempVerts[k].x ,x_max);
                            y_min= Mathf.Min (tempVerts[k].y ,y_min);
                            y_max= Mathf.Max (tempVerts[k].y ,y_max);
                            z_min= Mathf.Min (tempVerts[k].z ,z_min);
                            z_max= Mathf.Max (tempVerts[k].z ,z_max);
                        }

                    
                        Mesh.DestroyImmediate(m);

                        
                        for(int i=0 ; i< texHeight ; i++)
                        { 
                            if(i<bones.Length)
                            {
                                // if(j==0)
                                // {
                                //     startPosList.Add (bones[i].position);
                                //     startEulerList.Add (bones[i].rotation.eulerAngles);
                                //     pivotPosList.Add (bones[i].position-pivotTrans.position);
                                // }

                                Vector3 pos0=startPosList[i];
                                Quaternion rot0 = startRotList[i];
                                Vector3 scale0=startScaleList[i];
 
                                Vector3 pos1=bones[i].position;
                                Quaternion rot1 =bones[i].rotation;
                                Vector3 scale1=bones[i].lossyScale;


                                Vector3 scale= bones[i].lossyScale;
                                Matrix4x4 marix0 = Matrix4x4.TRS(pos0, rot0,scale0);
                                Matrix4x4 marix1 = Matrix4x4.TRS(pos1, rot1,scale1);

                                Matrix4x4 marix = marix1*marix0.inverse;

                                Vector3 pos= new Vector3(marix.m03,marix.m13,marix.m23)*0.1f*0.5f+new Vector3(0.5f,0.5f,0.5f); 
                                //Vector3 pivot =pivotPosList[i]*0.1f*0.5f+new Vector3(0.5f,0.5f,0.5f);
                                
                                Vector2 encodePosX= EncodeFloatRG(pos.x);
                                Vector2 encodePosY= EncodeFloatRG(pos.y);
                                Vector2 encodePosZ= EncodeFloatRG(pos.z);

                                texPix[i*texWidth+j*4]  = new Color(marix.m00*0.5f+0.5f,marix.m01*0.5f+0.5f,marix.m02*0.5f+0.5f,encodePosX.x);
                                texPix[i*texWidth+j*4+1]= new Color(marix.m10*0.5f+0.5f,marix.m11*0.5f+0.5f,marix.m12*0.5f+0.5f,encodePosY.x);
                                texPix[i*texWidth+j*4+2]= new Color(marix.m20*0.5f+0.5f,marix.m21*0.5f+0.5f,marix.m22*0.5f+0.5f,encodePosZ.x);
                                texPix[i*texWidth+j*4+3]= new Color(encodePosX.y ,encodePosY.y ,encodePosZ.y ,1);

                                // Vector2 encodePivotX= EncodeFloatRG(pivot.x);
                                // Vector2 encodePivotY= EncodeFloatRG(pivot.y);
                                // Vector2 encodePivotZ= EncodeFloatRG(pivot.z);

                                // if(j==(int)( texWidth*0.25f)-1)
                                // {
                                //     texPix[i*texWidth+j*4]  = new Color(encodePivotX.x,encodePivotY.x,encodePivotZ.x,1);
                                //     texPix[i*texWidth+j*4+1]= new Color(encodePivotX.y,encodePivotY.y,encodePivotZ.y,1);
                                //     texPix[i*texWidth+j*4+2]= new Color(0,0,0,0);
                                //     texPix[i*texWidth+j*4+3]= new Color(0,0,0,0);
                                // }
                            }
                            else
                            { 
                                texPix[i*texWidth+j*4]  = Color.black;
                                texPix[i*texWidth+j*4+1]= Color.black;
                                texPix[i*texWidth+j*4+2]= Color.black;
                                texPix[i*texWidth+j*4+3]= Color.black;
                            }
                        }
                    }
                    
                    Bounds bound =new Bounds();
                    bound.center=new Vector3((x_max+x_min)*0.5f,(y_max+y_min)*0.5f,(z_max+z_min)*0.5f);
                    //bound.size=new Vector3((x_max-x_min),(y_max-y_min),(z_max-z_min));
                    bound.size=new Vector3(10,10,10);
                    bakeM.bounds=bound;

                    String meshName =M.name;
                    String meshPath = Path.GetDirectoryName(AssetDatabase.GetAssetPath(M));

                    BakeBrokenDataPath=meshPath;
                    meshPath = BakeBrokenDataPath +"\\"+ meshName+"_"+"anim" +".asset";
                    AssetDatabase.CreateAsset(bakeM,meshPath);

                    AnimTex.SetPixels(texPix);
                    AnimTex.Apply();
                    #endregion
                    
                    String path="";
                    byte[] bytes=null;
                    String texName = brokeAnim.name;

                    path=BakeBrokenDataPath+"\\"+texName+"_"+framesCount+"_"+bonesCount+"_"+"anim"+".png";
                    bytes = AnimTex.EncodeToPNG();

                    File.WriteAllBytes(path, bytes);
                    AssetDatabase.ImportAsset(path);
                    AssetDatabase.Refresh();




                TextureImporter importer = TextureImporter.GetAtPath(path) as TextureImporter;
                collectAnimTexData(importer);
                TextureImporterPlatformSettings TIS_PC= importer.GetPlatformTextureSettings("PC");
                TextureImporterPlatformSettings TIS_IOS= importer.GetPlatformTextureSettings("iPhone");
                TextureImporterPlatformSettings TIS_Android= importer.GetPlatformTextureSettings("Android");



                importer.textureShape=TextureImporterShape.Texture2D;
                importer.sRGBTexture=false;
                importer.alphaSource=TextureImporterAlphaSource.FromInput;
                importer.alphaIsTransparency=false;
                importer.mipmapEnabled=false;
                importer.wrapMode=TextureWrapMode.Clamp;
                importer.filterMode=FilterMode.Point;

                // int PCTexSize=TIS_PC.maxTextureSize;
                // int IOSTexSize=TIS_IOS.maxTextureSize;
                // int AndroidTexSize=TIS_Android.maxTextureSize;
                TIS_PC.overridden=true;
                TIS_IOS.overridden=true;
                TIS_Android.overridden=true;

                TIS_PC.maxTextureSize=2048;
                TIS_IOS.maxTextureSize=2048;
                TIS_Android.maxTextureSize=2048;

                TIS_PC.resizeAlgorithm=TextureResizeAlgorithm.Mitchell;
                TIS_IOS.resizeAlgorithm=TextureResizeAlgorithm.Mitchell;
                TIS_Android.resizeAlgorithm=TextureResizeAlgorithm.Mitchell;

                TIS_PC.format=TextureImporterFormat.RGBA32;
                TIS_IOS.format=TextureImporterFormat.RGBA32;
                TIS_Android.format=TextureImporterFormat.RGBA32;
                

                //TIS.format=TextureImporterFormat.BC6H;
                //TIS.crunchedCompression=false;
                importer.SetPlatformTextureSettings(TIS_PC);
                importer.SetPlatformTextureSettings(TIS_IOS);
                importer.SetPlatformTextureSettings(TIS_Android);
                //importer.crunchedCompression=true;

                importer.SaveAndReimport();
                AssetDatabase.Refresh ();






                EditorGUIUtility.PingObject(AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(meshPath));
                    
                }
            }
            GameObject.DestroyImmediate(TempGO);
        }




    }


    // public Vector3 ShowRotationLikeInspector(Transform t)
    // {
    //     var type = t.GetType();
    //     var mi = type.GetMethod("GetLocalEulerAngles", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
    //     var rotationOrderPro = type.GetProperty("rotationOrder", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
    //     var rotationOrder = rotationOrderPro.GetValue(t, null);
    //     var EulerAnglesInspector = mi.Invoke(t, new[] {rotationOrder});
    //     return (Vector3) EulerAnglesInspector;
    // }
    // public void SetRotationLikeInspector(Transform t, Vector3 v)
    // {
    //     var type = t.GetType();
    //     var mi = type.GetMethod("SetLocalEulerAngles", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
    //     var rotationOrderPro = type.GetProperty("rotationOrder", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
    //     var rotationOrder = rotationOrderPro.GetValue(t, null);
    //     mi.Invoke(t, new[] {v, rotationOrder});
    // }

    public static void collectAnimTexData(TextureImporter myImporter)
    {
        animTexParam.textureImporterShape=myImporter.textureShape;
        animTexParam.isReadable=myImporter.isReadable;
        animTexParam.name = myImporter.name;

        animTexParam.AndroidSettings= myImporter.GetPlatformTextureSettings("Android");
        animTexParam.IOSSettings= myImporter.GetPlatformTextureSettings("iPhone");
        animTexParam.PCSettings= myImporter.GetPlatformTextureSettings("PC");
    }
    private Vector2 EncodeFloatRG(float v)
    {
        Vector2 kEncodeMul=new Vector2(1,255);
        float kEncodeBit=1.0f/255.0f;
        Vector2 enc = kEncodeMul*v;
        enc.x = Mathf.Repeat(enc.x,1);
        enc.y = Mathf.Repeat(enc.y,1);
        enc.x-=enc.y*kEncodeBit;
        return enc;
    }

    // private void playSelectionAnim()
    // {
    //     GameObject MyGO =  Selection.activeGameObject;
    //     if(MyGO)
    //     {
    //         Animation MyAnim = MyGO.GetComponent<Animation>();
    //         if(MyAnim)
    //         { 
    //             MyAnim.Play("jump");
                
    //         }
    //     }

    // }

}
