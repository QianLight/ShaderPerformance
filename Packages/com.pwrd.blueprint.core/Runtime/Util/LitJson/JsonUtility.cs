using System;
using LitJsonForSaveGame;

namespace Blueprint.Logic
{
    public class JsonUtility
    {
        public static string ToJson(object obj)
        {
            return JsonMapper.ToJson(obj);
        }

        public static object ToObject(string jsonStr, Type type)
        {
            return JsonMapper.ToObject(jsonStr, type);
        }
    }
}