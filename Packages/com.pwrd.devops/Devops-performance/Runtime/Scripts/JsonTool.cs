using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Json;
using System.Text;

namespace Devops.Performance
{
    class JsonTool
    {
        public static string Serialize<T>(T obj)
        {
            try
            {
                return UnityEngine.JsonUtility.ToJson(obj);
            }
            catch (Exception e)
            {
                UnityEngine.Debug.LogError("Serialize Error:" + e.Message);
            }
            return "";
        }

        public static T Deserialize<T>(string str)
        {
            return UnityEngine.JsonUtility.FromJson<T>(str);
        }
    }
}