using System.IO;
using UnityEditor;
using UnityEngine;
using System;
using System.Reflection;


namespace Blueprint
{
    using Blueprint.Actor;
    using Blueprint.ActorEditor;
    
    public class BpAssetPostProcessor: AssetPostprocessor
    {

        const string c_Prefab = ".prefab";
        const string c_field = "_Field.cs";

        static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
        {
            foreach (string str in importedAssets)
            {
                if (str.EndsWith(c_Prefab))
                {
                    GameObject obj = AssetDatabase.LoadAssetAtPath<GameObject>(str);
                    
                    if (obj != null && obj.GetComponent<BlueprintActor>())
                    {
                        obj.GetOrAddComponent<BPInit>();
                        BlueprintActor actor = obj.GetComponent<BlueprintActor>();
                        actor.RefreshActorParam();
                        Blueprint.ActorEditor.ActorEditor.SendAcotrRefresh(str, actor);
                        CheckCopyActor(actor,str);
                        //删除无效field文件
                        var expiredFieldList = obj.GetComponents<ActorFieldBase>();
                        foreach (var item in expiredFieldList)
                        {
                            if(obj.name!= (item.GetType().GetCustomAttribute(typeof(Blueprint.Actor.OrignBPClass)) as Blueprint.Actor.OrignBPClass).GetName())
                                (item as ActorFieldBase).DestroyField();
                        }
                        //绑定field文件
                        Assembly assembly = Assembly.Load("Assembly-CSharp");
                        Type[] comps = assembly.GetTypes();
                        foreach (var item in comps)
                        {
                            var baseType = item.BaseType;//获取元素类型的基类
                            if (baseType != null)//如果有基类
                            {
                                if (baseType.Name == typeof(ActorFieldBase).Name)//如果基类就是给定的父类
                                {
                                    System.Attribute orignNameAtt = item.GetCustomAttribute(typeof(Blueprint.Actor.OrignBPClass));
                                    if ((orignNameAtt as Blueprint.Actor.OrignBPClass).GetName() == obj.name)
                                        obj.GetOrAddComponent(item);
                                }
                            }
                        } 
                    }
                }
                //field编译完以后绑定
                if (str.EndsWith(c_field))
                {
                    string pName = System.IO.Path.GetFileName(str);
                    string fieldTypeName = pName.Substring(0,pName.Length-3);
                    Assembly assembly = Assembly.Load("Assembly-CSharp");
                    Type fieldComp = assembly.GetType(fieldTypeName);
                    var orignNameAtt = fieldComp.GetCustomAttribute(typeof(Blueprint.Actor.OrignBPClass));
                    string objStr = string.Empty;
                    if (orignNameAtt!=null)
                        objStr = (orignNameAtt as Blueprint.Actor.OrignBPClass).GetName();
                    
                    string[] objStrs = AssetDatabase.FindAssets($"{objStr} t:Prefab",null);
                    foreach (var item in objStrs)
                    {
                        var path =  AssetDatabase.GUIDToAssetPath(item);
                        GameObject obj = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                        if (obj != null && obj.GetComponent<BlueprintActor>() && objStr.Equals(obj.name))
                        {   
                            obj.GetOrAddComponent(fieldComp);
                        }
                    }
                }
            }
        }

        static void CheckCopyActor(BlueprintActor actor,string actorPath)
        {
            if(actor.bpClassName != actor.gameObject.name)
            {
                if(PENet.BpClient.IsConnected)
                {
                    string[] objStrs = AssetDatabase.FindAssets($"{actor.bpClassName} t:Prefab",null);
                    string path = string.Empty;
                   foreach (var item in objStrs)
                    {
                        var _path =  AssetDatabase.GUIDToAssetPath(item);
                        GameObject obj = AssetDatabase.LoadAssetAtPath<GameObject>(_path);
                        if (obj != null && obj.GetComponent<BlueprintActor>())
                        {
                            if(actor.bpClassName.Equals(obj.name))
                            {
                                path = _path;
                                ActorMessageData data = new ActorMessageData()
                                {
                                    path =  path,
                                    exportClassName = actor.gameObject.name,
                                };
                                PENet.BpClient.SendMessage(PENet.MessageType.CopyActor, JsonUtility.ToJson(data));
                                actor.bpClassName = actor.gameObject.name;
                                return;
                            }
                        }
                    }
                }
            }
        }
    }

}