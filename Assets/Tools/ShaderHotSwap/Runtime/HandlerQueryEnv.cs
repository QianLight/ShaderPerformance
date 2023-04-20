using System;
using UnityEngine;

namespace UsingTheirs.ShaderHotSwap
{

    public static class HandlerQueryEnv
    {
        public static string HandlerMain(string jsonRequest)
        {
            try
            {
                var res = new QueryEnvRes();
                res.platform = Application.platform.ToString();
                var resString = JsonUtility.ToJson(res);
                
                MemoryLogger.Log("[HandlerQueryEnv] plat. res:{0}", resString);

                return resString;
            }
            catch( System.Exception e )
            {
                return ErrorString(e.ToString());
            }
        }

        static string ErrorString(string reason)
        {
            var res = new QueryEnvRes();
            res.error = reason;
            return JsonUtility.ToJson(res);
        }
    }


}