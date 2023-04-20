using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
//using UnityEngineEditor = UnityEditor.Editor;
[CanEditMultipleObjects]

[CustomEditor (typeof (BindingTranform))]
public class BindingTranformEditor : Editor
{
#if UNITY_EDITOR
  public override void OnInspectorGUI()
  {
      //base.OnHeaderGUI();

    BindingTranform T = target as BindingTranform;
    Undo.RecordObject(T, "RecordTest");
    base.OnInspectorGUI();

    EditorGUILayout.BeginVertical();
    // test.TransCount=EditorGUILayout.IntField("DummyCount",test.TransCount);


    // if(!test.Trans.Count.Equals(test.TransCount))
    // {
    //     test.Trans.Clear();
    //     for(int i=0 ; i<test.TransCount;i++)
    //     {
    //         test.Trans.Add(new transformSync());
    //     }
    // }
    


    // for(int i=0 ; i<test.TransCount;i++)
    // {
    //     test.Trans[i].Target=(Transform)EditorGUILayout.ObjectField("Target",test.Trans[i].Target,typeof(Transform));
    //     test.Trans[i].Dummy=(Transform)EditorGUILayout.ObjectField("Dummy",test.Trans[i].Dummy,typeof(Transform));
    //     EditorGUILayout.Space();
    // }

    T.preTime=EditorGUILayout.Slider(T.preTime,0,T.MaxTime);
    EditorGUILayout.BeginHorizontal();
    string PlayStr ;
    if(T._play)
    {
      PlayStr="停止";
    }
    else
    {
      PlayStr="播放";
    }

    if(GUILayout.Button(PlayStr))
    {
        T.Play();
    }
    string str ;
    if(!T._play)
    {
      str="继续";
    }
    else
    {
      str="暂停";
    }
      if(GUILayout.Button(str))
        {
            T.Pause();
        }
    EditorGUILayout.EndHorizontal();
    EditorGUILayout.EndVertical();
  }
#endif
}
