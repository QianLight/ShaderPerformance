using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using Ender.LitJson;

namespace Ender
{
    public static class GMEnderMsgPacker
    {


        //public static object ParseResult(string json)
        //{
        //    JsonData packet = JsonMapper.ToObject(json);
        //    string type = DataUtils.GetString(packet, "type");
        //    int code = DataUtils.GetInt(packet, "code");
        //    string failMsg = DataUtils.GetString(packet, "failMsg");

        //    if (code != 0)
        //    {
        //        LogUtils.E("{code:" + code + "],failMsg:" + failMsg + "}");
        //    }
        //    object value = DataUtils.GetObject(packet, "value");
        //    return value;
        //}
    }
}
#endif
