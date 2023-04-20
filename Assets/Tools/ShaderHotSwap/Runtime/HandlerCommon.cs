using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UsingTheirs.ShaderHotSwap
{
    public static  class HandlerCommon
    {
        public  static string ErrorString(string reason)
        {
            var res = new QueryEnvRes();
            res.error = reason;
            return JsonUtility.ToJson(res);
        }
    }
}
