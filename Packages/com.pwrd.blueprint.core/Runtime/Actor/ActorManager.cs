using LitJsonForSaveGame;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Blueprint.Actor
{
    public static class ActorManager
    {
        public static List<ActorBase> GetAllActorsWithTag(string tag = "")
        {
            List<ActorBase> ret = new List<ActorBase>();
            try
            {
                GameObject[] objectList = GameObject.FindGameObjectsWithTag(tag);
                for (int i = 0; i < objectList.Length; i++)
                {
                    var comp = objectList[i].GetComponent<BlueprintActor>();
                    if (comp != null)
                        ret.Add(comp.BpActor);
                }
                return ret;
            }
            catch (Exception e)
            {
                return ret;
            }
        }

        public static List<T> GetAllActorsOfClassWithTag<T>(string name, string tag = "") where T : ActorBase
        {
            List<T> ret = new List<T>();
            try
            {
                GameObject[] objectList = GameObject.FindGameObjectsWithTag(tag);
                for (int i = 0; i < objectList.Length; i++)
                {
                    var comp = objectList[i].GetComponent<BlueprintActor>();
                    if (comp != null && comp.bpClassName == name)
                        ret.Add(comp.BpActor as T);
                }
                return ret;
            }
            catch (Exception e)
            {
                return ret;
            }
        }

        public static List<T> GetAllActorsOfClass<T>(string name) where T : ActorBase
        {
            List<T> ret = new List<T>();
            BlueprintActor[] objectList = GameObject.FindObjectsOfType<BlueprintActor>();
            for (int i = objectList.Length - 1; i >= 0; i--)
            {
                if (objectList[i].bpClassName == name)
                    ret.Add(objectList[i].BpActor as T);
            }
            return ret;
        }

        public static T GetActorOfClass<T>(string name) where T : ActorBase
        {
            BlueprintActor[] objectList = GameObject.FindObjectsOfType<BlueprintActor>();
            for (int i = objectList.Length - 1; i >= 0; i--)
            {
                if (objectList[i].bpClassName == name)
                    return objectList[i].BpActor as T;
            }
            return null;
        }

        public static T JsonToStruct<T>(string jsonStr) where T : class
        {
            try
            {
                return JsonMapper.ToObject(jsonStr, typeof(T)) as T;
            }
            catch(Exception e)
            {
                return null;
            }
        }
    }
}
