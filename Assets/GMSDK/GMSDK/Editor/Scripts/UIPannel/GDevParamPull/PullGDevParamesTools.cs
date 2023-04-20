//拉取GDev平台参数工具类

using System;
using UNBridgeLib.LitJson;
using UnityEngine;

public class PullGDevParamesTools
{
    private const string GDevServerUrl = "https://gdev.nvsgames.cn/hub/invoke/generateHubParamConfig";

    public static void PullParams(string param, Action<PullResult> responseHandler)
    {
        NetworkUtils.Post(GDevServerUrl, param, (string resp) =>
        {
            if (string.IsNullOrEmpty(resp))
            {
                responseHandler(new PullResult(PullResult.PullError, "result is null", null));
                return;
            }

            try
            {
                JsonData respJson = JsonMapper.ToObject(resp);
                if (respJson == null)
                {
                    responseHandler(new PullResult(PullResult.PullError, "respJson is null, resp:" + resp, null));
                    Debug.Log("Parameter pull failed:" + "respJson is null, resp:" + resp);
                    return;
                }

                if (!respJson.ContainsKey("code"))
                {
                    responseHandler(new PullResult(PullResult.PullError, "respJson not ContainsKey code, resp:" + resp, null));
                    Debug.Log("Parameter pull failed:" + "respJson not ContainsKey code, resp:" + resp);
                    return;
                }

                if (int.Parse(respJson["code"].ToString()) == 0)
                {
                    //去做参数解析
                    if (respJson.ContainsKey("data"))
                    {
                        Debug.Log("Parameters pulled successfully");
                        responseHandler(new PullResult(0, "success", respJson["data"].ToJson()));
                    }
                    else
                    {
                        Debug.Log("Parameter pull failed:" + "respJson not ContainsKey data, resp:" + resp);
                        responseHandler(new PullResult(PullResult.PullError, "respJson not ContainsKey data, resp:" + resp, null));
                    }
                }
                else
                {
                    string errorMsg = "";
                    if (respJson.ContainsKey("message"))
                    {
                        errorMsg = respJson["message"].ToString();
                    }

                    Debug.Log("Parameter pull failed:" + errorMsg);
                    responseHandler(new PullResult(PullResult.PullError, errorMsg, null));
                }
            }
            catch (Exception exception)
            {
                Debug.Log("Parameter pull failed:" + exception.Message);
                responseHandler(new PullResult(PullResult.PullError, exception.Message, null));
            }
        });
    }
}